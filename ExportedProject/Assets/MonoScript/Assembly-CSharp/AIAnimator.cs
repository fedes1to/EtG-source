using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AIAnimator : BraveBehaviour
{
	public enum FacingType
	{
		Default,
		Target,
		Movement,
		SlaveDirection
	}

	public enum DirectionalType
	{
		Sprite = 10,
		Rotation = 20,
		SpriteAndRotation = 30
	}

	public delegate void PlayUntilFinishedDelegate(string name, bool suppressHitStates = false, string overrideHitState = null, float warpClipDuration = -1f, bool skipChildAnimators = false);

	public delegate void EndAnimationIfDelegate(string name);

	public delegate void PlayVfxDelegate(string name, Vector2? sourceNormal, Vector2? sourceVelocity, Vector2? position);

	public delegate void StopVfxDelegate(string name);

	private class AnimatorState
	{
		public enum StateEndType
		{
			UntilCancelled,
			UntilFinished,
			Duration,
			DurationOrFinished
		}

		public string Name;

		public DirectionalAnimation DirectionalAnimation;

		public tk2dSpriteAnimationClip AnimationClip;

		public StateEndType EndType;

		public float Timer;

		public float WarpClipDuration;

		public float ArtAngle;

		public bool SuppressHitStates;

		public string OverrideHitStateName;

		public bool HasStarted;

		public AnimatorState()
		{
		}

		public AnimatorState(AnimatorState other)
		{
			Name = other.Name;
			DirectionalAnimation = other.DirectionalAnimation;
			AnimationClip = other.AnimationClip;
			EndType = other.EndType;
			Timer = other.Timer;
			WarpClipDuration = other.WarpClipDuration;
			ArtAngle = other.ArtAngle;
			SuppressHitStates = other.SuppressHitStates;
			OverrideHitStateName = other.OverrideHitStateName;
			HasStarted = other.HasStarted;
		}

		public void Update(tk2dSpriteAnimator spriteAnimator, float facingDirection)
		{
			if (DirectionalAnimation != null)
			{
				DirectionalAnimation.Info info = DirectionalAnimation.GetInfo(facingDirection, true);
				if (info != null)
				{
					AnimationClip = spriteAnimator.GetClipByName(info.name);
					ArtAngle = info.artAngle;
				}
			}
		}
	}

	public enum HitStateType
	{
		Basic,
		FacingDirection
	}

	[Serializable]
	public class NamedDirectionalAnimation
	{
		public string name;

		public DirectionalAnimation anim;
	}

	[Serializable]
	public class NamedVFXPool
	{
		public string name;

		public Transform anchorTransform;

		public VFXPool vfxPool;
	}

	[Serializable]
	public class NamedScreenShake
	{
		public string name;

		public ScreenShakeSettings screenShake;
	}

	public AIAnimator ChildAnimator;

	public AIActor SpecifyAiActor;

	public FacingType facingType;

	[ShowInInspectorIf("ShowDirectionParent", true)]
	public AIAnimator DirectionParent;

	[ShowInInspectorIf("ShowFaceSouthWhenStopped", true)]
	public bool faceSouthWhenStopped;

	[ShowInInspectorIf("ShowFaceSouthWhenStopped", true)]
	public bool faceTargetWhenStopped;

	[HideInInspector]
	public float AnimatedFacingDirection = -90f;

	public DirectionalType directionalType = DirectionalType.Sprite;

	[ShowInInspectorIf("ShowRotationOptions", true)]
	public float RotationQuantizeTo;

	[ShowInInspectorIf("ShowRotationOptions", true)]
	public float RotationOffset;

	public bool ForceKillVfxOnPreDeath;

	public bool SuppressAnimatorFallback;

	public bool IsBodySprite = true;

	[Header("Animations")]
	public DirectionalAnimation IdleAnimation;

	[FormerlySerializedAs("BaseAnimation")]
	public DirectionalAnimation MoveAnimation;

	public DirectionalAnimation FlightAnimation;

	public DirectionalAnimation HitAnimation;

	[ShowInInspectorIf("ShowHitAnimationOptions", true)]
	public float HitReactChance = 1f;

	[ShowInInspectorIf("ShowHitAnimationOptions", true)]
	public float MinTimeBetweenHitReacts;

	[ShowInInspectorIf("ShowHitAnimationOptions", true)]
	public HitStateType HitType;

	public DirectionalAnimation TalkAnimation;

	public List<NamedDirectionalAnimation> OtherAnimations;

	public List<NamedVFXPool> OtherVFX;

	public List<NamedScreenShake> OtherScreenShake;

	public List<DirectionalAnimation> IdleFidgetAnimations;

	private float m_facingDirection;

	private float m_cachedTurbo = -1f;

	public PlayUntilFinishedDelegate OnPlayUntilFinished;

	private const float c_FIDGET_COOLDOWN = 2f;

	public EndAnimationIfDelegate OnEndAnimationIf;

	public PlayVfxDelegate OnPlayVfx;

	public StopVfxDelegate OnStopVfx;

	private bool m_playingHitEffect;

	private bool m_hasPlayedAwaken;

	private tk2dSpriteAnimationClip m_currentBaseClip;

	private float m_currentBaseArtAngle;

	private DirectionalAnimation m_baseDirectionalAnimationOverride;

	private tk2dSpriteAnimationClip m_currentOverrideBaseClip;

	private AnimatorState m_currentActionState;

	private float m_fidgetTimer;

	private float m_fidgetCooldown;

	private float m_suppressHitReactTimer;

	private float m_fpsScale = 1f;

	public bool UseAnimatedFacingDirection { get; set; }

	protected float LocalDeltaTime
	{
		get
		{
			if ((bool)base.aiActor)
			{
				return base.aiActor.LocalDeltaTime;
			}
			if ((bool)base.behaviorSpeculator)
			{
				return base.behaviorSpeculator.LocalDeltaTime;
			}
			return BraveTime.DeltaTime;
		}
	}

	public bool SpriteFlipped
	{
		get
		{
			if (m_currentBaseClip == null)
			{
				return false;
			}
			return m_currentBaseClip.name == "move_back" || m_currentBaseClip.name == "move_left" || m_currentBaseClip.name == "move_forward_left" || m_currentBaseClip.name == "move_back_left";
		}
	}

	public bool SuppressHitStates { get; set; }

	public bool LockFacingDirection { get; set; }

	public float FacingDirection
	{
		get
		{
			return m_facingDirection;
		}
		set
		{
			if (!float.IsNaN(value))
			{
				m_facingDirection = value;
			}
		}
	}

	public float FpsScale
	{
		get
		{
			return m_fpsScale;
		}
		set
		{
			if (m_fpsScale != value)
			{
				m_fpsScale = value;
				bool flag = m_currentActionState != null && m_currentActionState.WarpClipDuration > 0f;
				if (base.spriteAnimator.Playing && !flag)
				{
					float num = base.spriteAnimator.CurrentClip.fps * m_fpsScale;
					if (num == 0f)
					{
						num = 0.001f;
					}
					base.spriteAnimator.ClipFps = num;
				}
			}
			if ((bool)ChildAnimator)
			{
				ChildAnimator.FpsScale = value;
			}
		}
	}

	public float CurrentClipLength
	{
		get
		{
			return (float)base.spriteAnimator.CurrentClip.frames.Length / base.spriteAnimator.CurrentClip.fps;
		}
	}

	public float CurrentClipProgress
	{
		get
		{
			return Mathf.Clamp01(base.spriteAnimator.ClipTimeSeconds / CurrentClipLength);
		}
	}

	public string OverrideIdleAnimation { get; set; }

	public string OverrideMoveAnimation { get; set; }

	public float CurrentArtAngle
	{
		get
		{
			if (m_currentActionState != null)
			{
				return m_currentActionState.ArtAngle;
			}
			return FacingDirection;
		}
	}

	public bool HasDefaultAnimation
	{
		get
		{
			return MoveAnimation.Type != 0 || IdleAnimation.Type != 0 || FlightAnimation.Type != DirectionalAnimation.DirectionType.None;
		}
	}

	public event Action OnSpawnCompleted;

	private bool ShowDirectionParent()
	{
		return facingType == FacingType.SlaveDirection;
	}

	private bool ShowFaceSouthWhenStopped()
	{
		return facingType == FacingType.Movement;
	}

	private bool ShowRotationOptions()
	{
		return directionalType != DirectionalType.Sprite;
	}

	private bool ShowHitAnimationOptions()
	{
		return HitAnimation.Type != DirectionalAnimation.DirectionType.None;
	}

	public void Awake()
	{
		base.spriteAnimator.playAutomatically = false;
		if (GameManager.Instance.InTutorial && base.name.Contains("turret", true))
		{
			FacingDirection = 180f;
			LockFacingDirection = true;
			base.specRigidbody.enabled = false;
		}
		if ((bool)SpecifyAiActor)
		{
			base.aiActor = SpecifyAiActor;
			base.specRigidbody = base.aiActor.specRigidbody;
		}
		if (ForceKillVfxOnPreDeath && (bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath += OnPreDeath;
		}
	}

	public void Start()
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
		if ((bool)base.healthHaver && (bool)ChildAnimator && (bool)ChildAnimator.sprite && ChildAnimator.IsBodySprite)
		{
			base.healthHaver.RegisterBodySprite(ChildAnimator.sprite);
		}
	}

	private void UpdateTurboSettings()
	{
		if (m_cachedTurbo != TurboModeController.sEnemyAnimSpeed && GameManager.IsTurboMode)
		{
			if (m_cachedTurbo > 0f)
			{
				FpsScale /= m_cachedTurbo;
				m_cachedTurbo = -1f;
			}
			FpsScale *= TurboModeController.sEnemyAnimSpeed;
			m_cachedTurbo = TurboModeController.sEnemyAnimSpeed;
		}
		else if (m_cachedTurbo > 0f && !GameManager.IsTurboMode)
		{
			FpsScale /= m_cachedTurbo;
			m_cachedTurbo = -1f;
		}
	}

	public void Update()
	{
		m_suppressHitReactTimer = Mathf.Max(0f, m_suppressHitReactTimer - BraveTime.DeltaTime);
		UpdateTurboSettings();
		UpdateFacingDirection();
		UpdateCurrentBaseAnimation();
		if (m_currentActionState != null && m_currentActionState.DirectionalAnimation != null && m_currentActionState.AnimationClip == m_currentBaseClip)
		{
			m_currentActionState = null;
		}
		if (m_currentActionState != null)
		{
			m_currentActionState.Update(base.spriteAnimator, FacingDirection);
			if (!m_currentActionState.HasStarted)
			{
				base.spriteAnimator.Stop();
				PlayClip(m_currentActionState.AnimationClip, m_currentActionState.WarpClipDuration);
				m_currentActionState.HasStarted = true;
				m_playingHitEffect = false;
			}
			else if (!m_playingHitEffect && base.spriteAnimator.CurrentClip != m_currentActionState.AnimationClip)
			{
				base.spriteAnimator.Play(m_currentActionState.AnimationClip, base.spriteAnimator.ClipTimeSeconds, GetFps(m_currentActionState.AnimationClip, m_currentActionState.WarpClipDuration), true);
			}
			if (m_currentActionState.EndType == AnimatorState.StateEndType.Duration || m_currentActionState.EndType == AnimatorState.StateEndType.DurationOrFinished)
			{
				m_currentActionState.Timer -= LocalDeltaTime;
				if (m_currentActionState.Timer <= 0f)
				{
					m_currentActionState = null;
					m_playingHitEffect = false;
				}
			}
		}
		else if (!m_playingHitEffect && m_baseDirectionalAnimationOverride != null && !base.spriteAnimator.IsPlaying(m_currentOverrideBaseClip))
		{
			PlayClip(m_currentOverrideBaseClip, -1f);
		}
		else if (!m_playingHitEffect && m_baseDirectionalAnimationOverride == null && m_currentBaseClip != null && !base.spriteAnimator.IsPlaying(m_currentBaseClip))
		{
			PlayClip(m_currentBaseClip, -1f);
		}
		UpdateFacingRotation();
		for (int i = 0; i < OtherVFX.Count; i++)
		{
			OtherVFX[i].vfxPool.RemoveDespawnedVfx();
		}
	}

	private string GetDebugString()
	{
		string text = string.Format("{0}: {1} ({2}) - {3} ({4})", base.name, (m_currentActionState != null) ? m_currentActionState.Name : "null", (m_currentActionState != null) ? m_currentActionState.Timer.ToString() : "null", base.spriteAnimator.CurrentClip.name, base.spriteAnimator.ClipTimeSeconds);
		if ((bool)ChildAnimator && ChildAnimator.IsBodySprite)
		{
			text = text + " | " + ChildAnimator.GetDebugString();
		}
		return text;
	}

	protected override void OnDestroy()
	{
		if ((bool)base.spriteAnimator)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
		}
		base.OnDestroy();
		for (int i = 0; i < OtherVFX.Count; i++)
		{
			OtherVFX[i].vfxPool.DestroyAll();
		}
	}

	private void AnimationCompleted(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		if (!base.enabled)
		{
			return;
		}
		if (m_playingHitEffect)
		{
			m_playingHitEffect = false;
			if (m_currentActionState != null && m_currentActionState.HasStarted)
			{
				PlayClip(m_currentActionState.AnimationClip, m_currentActionState.WarpClipDuration);
			}
			else
			{
				PlayClip(m_currentBaseClip, -1f);
			}
		}
		else if (m_currentActionState != null && m_currentActionState.AnimationClip == clip && m_currentActionState.EndType != 0)
		{
			m_currentActionState = null;
		}
	}

	public bool PlayUntilCancelled(string name, bool suppressHitStates = false, string overrideHitState = null, float warpClipDuration = -1f, bool skipChildAnimators = false)
	{
		bool flag = false;
		if (!skipChildAnimators && (bool)ChildAnimator)
		{
			flag = ChildAnimator.PlayUntilCancelled(name, suppressHitStates, overrideHitState);
		}
		if (HasDirectionalAnimation(name))
		{
			return Play(name, GetDirectionalAnimation(name), AnimatorState.StateEndType.UntilCancelled, 0f, warpClipDuration, suppressHitStates, overrideHitState) || flag;
		}
		return Play(name, AnimatorState.StateEndType.UntilCancelled, 0f, warpClipDuration, suppressHitStates, overrideHitState) || flag;
	}

	public bool PlayUntilFinished(string name, bool suppressHitStates = false, string overrideHitState = null, float warpClipDuration = -1f, bool skipChildAnimators = false)
	{
		if (OnPlayUntilFinished != null)
		{
			OnPlayUntilFinished(name, suppressHitStates, overrideHitState, warpClipDuration, skipChildAnimators);
		}
		bool flag = false;
		if (!skipChildAnimators && (bool)ChildAnimator)
		{
			flag = ChildAnimator.PlayUntilFinished(name, suppressHitStates, overrideHitState, warpClipDuration);
		}
		if (HasDirectionalAnimation(name))
		{
			return Play(name, GetDirectionalAnimation(name), AnimatorState.StateEndType.UntilFinished, 0f, warpClipDuration, suppressHitStates, overrideHitState) || flag;
		}
		return Play(name, AnimatorState.StateEndType.UntilFinished, 0f, warpClipDuration, suppressHitStates, overrideHitState) || flag;
	}

	public bool PlayForDuration(string name, float duration, bool suppressHitStates = false, string overrideHitState = null, float warpClipDuration = -1f, bool skipChildAnimators = false)
	{
		bool flag = false;
		if (!skipChildAnimators && (bool)ChildAnimator)
		{
			flag = ChildAnimator.PlayForDuration(name, duration, suppressHitStates, overrideHitState);
		}
		if (HasDirectionalAnimation(name))
		{
			return Play(name, GetDirectionalAnimation(name), AnimatorState.StateEndType.Duration, duration, warpClipDuration, suppressHitStates, overrideHitState) || flag;
		}
		return Play(name, AnimatorState.StateEndType.Duration, duration, warpClipDuration, suppressHitStates, overrideHitState) || flag;
	}

	public bool PlayForDurationOrUntilFinished(string name, float duration, bool suppressHitStates = false, string overrideHitState = null, float warpClipDuration = -1f, bool skipChildAnimators = false)
	{
		bool flag = false;
		if (!skipChildAnimators && (bool)ChildAnimator)
		{
			flag = ChildAnimator.PlayForDurationOrUntilFinished(name, duration, suppressHitStates, overrideHitState);
		}
		if (HasDirectionalAnimation(name))
		{
			return Play(name, GetDirectionalAnimation(name), AnimatorState.StateEndType.DurationOrFinished, duration, warpClipDuration, suppressHitStates, overrideHitState) || flag;
		}
		return Play(name, AnimatorState.StateEndType.DurationOrFinished, duration, warpClipDuration, suppressHitStates, overrideHitState) || flag;
	}

	private bool Play(string name, AnimatorState.StateEndType endType, float duration, float warpClipDuration, bool suppressHitStates, string overrideHitState)
	{
		if (SuppressAnimatorFallback)
		{
			return false;
		}
		AnimatorState animatorState = new AnimatorState();
		animatorState.Name = name;
		animatorState.AnimationClip = base.spriteAnimator.GetClipByName(name);
		animatorState.EndType = endType;
		animatorState.Timer = duration;
		animatorState.WarpClipDuration = warpClipDuration;
		animatorState.SuppressHitStates = suppressHitStates;
		animatorState.OverrideHitStateName = overrideHitState;
		return Play(animatorState);
	}

	private bool Play(string name, DirectionalAnimation directionalAnimation, AnimatorState.StateEndType endType, float duration, float warpClipDuration, bool suppressHitStates, string overrideHitState)
	{
		if (directionalAnimation.Type == DirectionalAnimation.DirectionType.None)
		{
			return false;
		}
		AnimatorState animatorState = new AnimatorState();
		animatorState.Name = name;
		animatorState.DirectionalAnimation = directionalAnimation;
		animatorState.AnimationClip = base.spriteAnimator.GetClipByName(directionalAnimation.GetInfo(FacingDirection, true).name);
		animatorState.EndType = endType;
		animatorState.Timer = duration;
		animatorState.WarpClipDuration = warpClipDuration;
		animatorState.SuppressHitStates = suppressHitStates;
		animatorState.OverrideHitStateName = overrideHitState;
		return Play(animatorState);
	}

	private bool Play(AnimatorState state)
	{
		if (state.DirectionalAnimation != null && state.DirectionalAnimation.Type == DirectionalAnimation.DirectionType.None)
		{
			return false;
		}
		if (state.AnimationClip != null)
		{
			m_currentActionState = state;
			base.spriteAnimator.Stop();
			PlayClip(m_currentActionState.AnimationClip, state.WarpClipDuration);
			m_currentActionState.HasStarted = true;
			m_playingHitEffect = false;
			return true;
		}
		return false;
	}

	public void SetBaseAnim(string name, bool useFidgetTimer = false)
	{
		if ((bool)ChildAnimator)
		{
			ChildAnimator.SetBaseAnim(name);
		}
		if (HasDirectionalAnimation(name))
		{
			m_baseDirectionalAnimationOverride = GetDirectionalAnimation(name);
			if (useFidgetTimer)
			{
				tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = (m_currentOverrideBaseClip = base.spriteAnimator.GetClipByName(m_baseDirectionalAnimationOverride.GetInfo(FacingDirection).name));
				m_fidgetCooldown = 2f;
				m_fidgetTimer = tk2dSpriteAnimationClip2.BaseClipLength;
			}
		}
	}

	public void ClearBaseAnim()
	{
		if ((bool)ChildAnimator)
		{
			ChildAnimator.ClearBaseAnim();
		}
		m_baseDirectionalAnimationOverride = null;
	}

	public bool IsIdle()
	{
		return m_currentActionState == null;
	}

	public bool IsPlaying(string animName)
	{
		if (m_currentActionState != null && m_currentActionState.Name == animName && base.spriteAnimator.Playing)
		{
			return true;
		}
		if (base.spriteAnimator.IsPlaying(animName))
		{
			return true;
		}
		if ((bool)ChildAnimator)
		{
			return ChildAnimator.IsPlaying(animName);
		}
		return false;
	}

	public bool GetWrapType(string animName, out tk2dSpriteAnimationClip.WrapMode wrapMode)
	{
		DirectionalAnimation directionalAnimation = GetDirectionalAnimation(animName);
		if (directionalAnimation != null)
		{
			tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(directionalAnimation.GetInfo(0).name);
			if (clipByName != null)
			{
				wrapMode = clipByName.wrapMode;
				return true;
			}
		}
		if ((bool)ChildAnimator)
		{
			return ChildAnimator.GetWrapType(animName, out wrapMode);
		}
		wrapMode = tk2dSpriteAnimationClip.WrapMode.Single;
		return false;
	}

	public bool EndAnimation()
	{
		bool result = false;
		if ((bool)ChildAnimator)
		{
			result = ChildAnimator.EndAnimation();
		}
		if (m_currentActionState != null)
		{
			m_currentActionState = null;
			return true;
		}
		return result;
	}

	public bool EndAnimationIf(string name)
	{
		if (OnEndAnimationIf != null)
		{
			OnEndAnimationIf(name);
		}
		bool result = false;
		if ((bool)ChildAnimator)
		{
			result = ChildAnimator.EndAnimationIf(name);
		}
		if (m_currentActionState != null && m_currentActionState.Name == name)
		{
			m_currentActionState = null;
			return true;
		}
		return result;
	}

	public string PlayDefaultSpawnState()
	{
		bool isPlayingAwaken;
		return PlayDefaultSpawnState(out isPlayingAwaken);
	}

	public string PlayDefaultSpawnState(out bool isPlayingAwaken)
	{
		isPlayingAwaken = false;
		if ((bool)ChildAnimator)
		{
			ChildAnimator.PlayDefaultSpawnState();
		}
		if (!base.enabled || m_hasPlayedAwaken)
		{
			return null;
		}
		tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName("spawn");
		string result;
		if (clipByName != null)
		{
			if (clipByName.wrapMode == tk2dSpriteAnimationClip.WrapMode.Loop || clipByName.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection || clipByName.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopFidget)
			{
				PlayForDuration(clipByName.name, 2f, true);
			}
			else
			{
				PlayUntilFinished(clipByName.name, true);
			}
			result = clipByName.name;
		}
		else
		{
			result = PlayDefaultAwakenedState();
			isPlayingAwaken = true;
		}
		m_hasPlayedAwaken = true;
		return result;
	}

	public string PlayDefaultAwakenedState()
	{
		if ((bool)ChildAnimator)
		{
			ChildAnimator.PlayDefaultAwakenedState();
		}
		if (!base.enabled || m_hasPlayedAwaken)
		{
			return null;
		}
		m_hasPlayedAwaken = true;
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = null;
		if (HasDirectionalAnimation("awaken"))
		{
			DirectionalAnimation directionalAnimation = GetDirectionalAnimation("awaken");
			if (directionalAnimation.Type == DirectionalAnimation.DirectionType.None)
			{
				return null;
			}
			tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName(directionalAnimation.GetInfo(FacingDirection).name);
			if (tk2dSpriteAnimationClip2.wrapMode == tk2dSpriteAnimationClip.WrapMode.Loop || tk2dSpriteAnimationClip2.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection || tk2dSpriteAnimationClip2.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopFidget)
			{
				PlayForDuration("awaken", 2f, true);
			}
			else
			{
				PlayUntilFinished("awaken", true);
			}
			return "awaken";
		}
		tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName((!(FacingDirection < 90f) && !(FacingDirection > 1270f)) ? "awaken_left" : "awaken_right");
		if (tk2dSpriteAnimationClip2 == null)
		{
			tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName("awaken");
		}
		if (tk2dSpriteAnimationClip2 != null)
		{
			if (tk2dSpriteAnimationClip2.wrapMode == tk2dSpriteAnimationClip.WrapMode.Loop || tk2dSpriteAnimationClip2.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection || tk2dSpriteAnimationClip2.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopFidget)
			{
				PlayForDuration(tk2dSpriteAnimationClip2.name, 2f, true);
			}
			else
			{
				PlayUntilFinished(tk2dSpriteAnimationClip2.name, true);
			}
			return tk2dSpriteAnimationClip2.name;
		}
		return null;
	}

	public void PlayHitState(Vector2 damageVector)
	{
		if ((bool)ChildAnimator)
		{
			ChildAnimator.PlayHitState(damageVector);
		}
		if (!base.enabled || SuppressHitStates || (HitReactChance < 1f && UnityEngine.Random.value > HitReactChance) || m_suppressHitReactTimer > 0f || FpsScale == 0f)
		{
			return;
		}
		DirectionalAnimation directionalAnimation = HitAnimation;
		if (m_currentActionState != null)
		{
			if (m_currentActionState.SuppressHitStates)
			{
				return;
			}
			string overrideHitStateName = m_currentActionState.OverrideHitStateName;
			if (!string.IsNullOrEmpty(overrideHitStateName))
			{
				if (HasDirectionalAnimation(overrideHitStateName))
				{
					directionalAnimation = GetDirectionalAnimation(overrideHitStateName);
				}
				else
				{
					Debug.LogWarning("No override animation found with name " + overrideHitStateName);
				}
			}
		}
		if (directionalAnimation.Type != 0)
		{
			if (HitType == HitStateType.Basic)
			{
				Vector2 dir = ((!(damageVector == Vector2.zero)) ? damageVector : base.specRigidbody.Velocity);
				PlayClip(directionalAnimation.GetInfo(dir).name, -1f);
				m_playingHitEffect = true;
			}
			else
			{
				PlayClip(directionalAnimation.GetInfo(FacingDirection).name, -1f);
				m_playingHitEffect = true;
			}
		}
		if (MinTimeBetweenHitReacts > 0f)
		{
			m_suppressHitReactTimer = MinTimeBetweenHitReacts;
		}
	}

	public bool HasDirectionalAnimation(string animName)
	{
		if (string.IsNullOrEmpty(animName))
		{
			return false;
		}
		if (animName.Equals("idle", StringComparison.OrdinalIgnoreCase))
		{
			return !string.IsNullOrEmpty(OverrideIdleAnimation) || IdleAnimation.Type != DirectionalAnimation.DirectionType.None;
		}
		if (animName.Equals("move", StringComparison.OrdinalIgnoreCase))
		{
			return !string.IsNullOrEmpty(OverrideMoveAnimation) || MoveAnimation.Type != DirectionalAnimation.DirectionType.None;
		}
		if (animName.Equals("talk", StringComparison.OrdinalIgnoreCase))
		{
			return TalkAnimation.Type != DirectionalAnimation.DirectionType.None;
		}
		if (animName.Equals("hit", StringComparison.OrdinalIgnoreCase))
		{
			return HitAnimation.Type != DirectionalAnimation.DirectionType.None;
		}
		if (animName.Equals("flight", StringComparison.OrdinalIgnoreCase))
		{
			return FlightAnimation.Type != DirectionalAnimation.DirectionType.None;
		}
		for (int i = 0; i < OtherAnimations.Count; i++)
		{
			if (animName.Equals(OtherAnimations[i].name, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	public float GetDirectionalAnimationLength(string animName)
	{
		DirectionalAnimation directionalAnimation = GetDirectionalAnimation(animName);
		if (directionalAnimation == null)
		{
			return 0f;
		}
		return base.spriteAnimator.GetClipByName(directionalAnimation.GetInfo(-Vector2.up).name).BaseClipLength;
	}

	public void CopyStateFrom(AIAnimator other)
	{
		base.sprite.SetSprite(other.sprite.spriteId);
		base.spriteAnimator.PlayFrom(other.spriteAnimator.CurrentClip, other.spriteAnimator.clipTime);
		m_currentActionState = ((other.m_currentActionState != null) ? new AnimatorState(other.m_currentActionState) : null);
		m_playingHitEffect = other.m_playingHitEffect;
		m_hasPlayedAwaken = other.m_hasPlayedAwaken;
		m_currentBaseClip = other.m_currentBaseClip;
		m_currentBaseArtAngle = other.m_currentBaseArtAngle;
		m_baseDirectionalAnimationOverride = other.m_baseDirectionalAnimationOverride;
		m_currentOverrideBaseClip = other.m_currentOverrideBaseClip;
		m_currentActionState = other.m_currentActionState;
		m_fidgetTimer = other.m_fidgetTimer;
		m_fidgetCooldown = other.m_fidgetCooldown;
		m_suppressHitReactTimer = other.m_suppressHitReactTimer;
		m_fpsScale = other.m_fpsScale;
	}

	public void UpdateAnimation(float deltaTime)
	{
		AIAnimator aIAnimator = this;
		while ((bool)aIAnimator)
		{
			if ((bool)aIAnimator.spriteAnimator)
			{
				aIAnimator.spriteAnimator.UpdateAnimation(deltaTime);
			}
			aIAnimator = aIAnimator.ChildAnimator;
		}
	}

	public void PlayVfx(string name, Vector2? sourceNormal = null, Vector2? sourceVelocity = null, Vector2? position = null)
	{
		if (OnPlayVfx != null)
		{
			OnPlayVfx(name, sourceNormal, sourceVelocity, position);
		}
		if ((bool)ChildAnimator)
		{
			ChildAnimator.PlayVfx(name);
		}
		for (int i = 0; i < OtherVFX.Count; i++)
		{
			NamedVFXPool namedVFXPool = OtherVFX[i];
			if (!(namedVFXPool.name == name))
			{
				continue;
			}
			if (position.HasValue)
			{
				namedVFXPool.vfxPool.SpawnAtPosition(position.Value, 0f, base.transform, sourceNormal, sourceVelocity, null, true);
			}
			else if ((bool)namedVFXPool.anchorTransform)
			{
				if (!sourceVelocity.HasValue)
				{
					sourceVelocity = new Vector2(1f, 0f).Rotate(namedVFXPool.anchorTransform.eulerAngles.z + 180f);
				}
				namedVFXPool.vfxPool.SpawnAtLocalPosition(Vector3.zero, 0f, namedVFXPool.anchorTransform, sourceNormal, sourceVelocity, true);
			}
			else
			{
				namedVFXPool.vfxPool.SpawnAtPosition(base.specRigidbody.UnitCenter, 0f, base.transform, sourceNormal, sourceVelocity, null, true);
			}
		}
		for (int j = 0; j < OtherScreenShake.Count; j++)
		{
			NamedScreenShake namedScreenShake = OtherScreenShake[j];
			if (namedScreenShake.name == name)
			{
				Vector2 value = ((!base.specRigidbody) ? base.sprite.WorldCenter : base.specRigidbody.UnitCenter);
				GameManager.Instance.MainCameraController.DoScreenShake(namedScreenShake.screenShake, value);
			}
		}
	}

	public void StopVfx(string name)
	{
		if (OnStopVfx != null)
		{
			OnStopVfx(name);
		}
		if ((bool)ChildAnimator)
		{
			ChildAnimator.StopVfx(name);
		}
		for (int i = 0; i < OtherVFX.Count; i++)
		{
			NamedVFXPool namedVFXPool = OtherVFX[i];
			if (namedVFXPool.name == name)
			{
				namedVFXPool.vfxPool.DestroyAll();
			}
		}
	}

	private void OnPreDeath(Vector2 deathDirection)
	{
		if (ForceKillVfxOnPreDeath)
		{
			for (int i = 0; i < OtherVFX.Count; i++)
			{
				OtherVFX[i].vfxPool.DestroyAll();
			}
		}
	}

	private void UpdateFacingDirection()
	{
		if (LockFacingDirection)
		{
			return;
		}
		if (UseAnimatedFacingDirection)
		{
			FacingDirection = AnimatedFacingDirection;
		}
		else if (facingType == FacingType.SlaveDirection)
		{
			FacingDirection = DirectionParent.FacingDirection;
		}
		else if (facingType == FacingType.Movement)
		{
			if ((bool)base.aiActor)
			{
				if (base.aiActor.VoluntaryMovementVelocity != Vector2.zero)
				{
					FacingDirection = base.aiActor.VoluntaryMovementVelocity.ToAngle();
				}
				else if (faceSouthWhenStopped)
				{
					FacingDirection = -90f;
				}
				else if (faceTargetWhenStopped && (bool)base.aiActor && (bool)base.aiActor.TargetRigidbody)
				{
					FacingDirection = BraveMathCollege.Atan2Degrees(base.aiActor.TargetRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
				}
			}
			else if (base.specRigidbody.Velocity != Vector2.zero)
			{
				FacingDirection = base.specRigidbody.Velocity.ToAngle();
			}
		}
		else if (facingType == FacingType.Target)
		{
			if ((bool)base.aiActor && (bool)base.aiActor.TargetRigidbody)
			{
				FacingDirection = BraveMathCollege.Atan2Degrees(base.aiActor.TargetRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
			}
		}
		else if ((bool)base.talkDoer)
		{
			if (Mathf.Abs(base.specRigidbody.Velocity.x) > 0.0001f || Mathf.Abs(base.specRigidbody.Velocity.y) > 0.0001f)
			{
				FacingDirection = BraveMathCollege.Atan2Degrees(base.specRigidbody.Velocity);
				return;
			}
			PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(base.specRigidbody.UnitCenter);
			if (activePlayerClosestToPoint != null)
			{
				FacingDirection = BraveMathCollege.Atan2Degrees(activePlayerClosestToPoint.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
			}
			else if (GameManager.Instance.PrimaryPlayer != null)
			{
				FacingDirection = BraveMathCollege.Atan2Degrees(GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
			}
		}
		else if ((bool)base.aiShooter && (base.aiShooter.CurrentGun != null || base.aiShooter.ManualGunAngle))
		{
			FacingDirection = base.aiShooter.GunAngle;
		}
		else if ((bool)base.aiActor && (bool)base.aiActor.TargetRigidbody)
		{
			FacingDirection = BraveMathCollege.Atan2Degrees(base.aiActor.TargetRigidbody.UnitCenter - base.specRigidbody.UnitCenter);
		}
		else if ((bool)base.specRigidbody && base.specRigidbody.Velocity != Vector2.zero)
		{
			FacingDirection = BraveMathCollege.Atan2Degrees(base.specRigidbody.Velocity);
		}
	}

	private void UpdateCurrentBaseAnimation()
	{
		bool flag = m_fidgetTimer > 0f;
		m_fidgetTimer -= LocalDeltaTime;
		if (flag && m_fidgetTimer <= 0f)
		{
			ClearBaseAnim();
		}
		if (m_fidgetTimer <= 0f && m_fidgetCooldown > 0f)
		{
			m_fidgetCooldown -= LocalDeltaTime;
		}
		bool flag2 = FlightAnimation.Type != DirectionalAnimation.DirectionType.None;
		bool flag3 = !string.IsNullOrEmpty(OverrideMoveAnimation) || MoveAnimation.Type != DirectionalAnimation.DirectionType.None;
		bool flag4 = !string.IsNullOrEmpty(OverrideIdleAnimation) || IdleAnimation.Type != DirectionalAnimation.DirectionType.None;
		DirectionalAnimation directionalAnimation = null;
		if (m_baseDirectionalAnimationOverride != null)
		{
			DirectionalAnimation.Info info = m_baseDirectionalAnimationOverride.GetInfo(FacingDirection, true);
			m_currentOverrideBaseClip = base.spriteAnimator.GetClipByName(info.name);
		}
		if ((bool)base.aiActor && flag2 && base.aiActor.IsFlying && base.aiActor.IsOverPit)
		{
			directionalAnimation = FlightAnimation;
		}
		else if (flag3 && (bool)base.specRigidbody && base.specRigidbody.Velocity != Vector2.zero)
		{
			directionalAnimation = (string.IsNullOrEmpty(OverrideMoveAnimation) ? MoveAnimation : GetDirectionalAnimation(OverrideMoveAnimation));
		}
		else if (flag4)
		{
			directionalAnimation = (string.IsNullOrEmpty(OverrideIdleAnimation) ? IdleAnimation : GetDirectionalAnimation(OverrideIdleAnimation));
			if (IdleFidgetAnimations.Count > 0 && m_fidgetTimer <= 0f && m_fidgetCooldown <= 0f)
			{
				float value = UnityEngine.Random.value;
				float num = BraveMathCollege.SliceProbability(0.2f, LocalDeltaTime);
				if (value < num)
				{
					SetBaseAnim(IdleFidgetAnimations[0].GetInfo(FacingDirection, true).name, true);
				}
			}
		}
		if (directionalAnimation == null && flag3)
		{
			directionalAnimation = MoveAnimation;
		}
		if (directionalAnimation != null && (bool)base.spriteAnimator)
		{
			DirectionalAnimation.Info info2 = directionalAnimation.GetInfo(FacingDirection, true);
			if (info2 != null)
			{
				m_currentBaseClip = base.spriteAnimator.GetClipByName(info2.name);
				m_currentBaseArtAngle = info2.artAngle;
			}
		}
	}

	private DirectionalAnimation GetDirectionalAnimation(string animName)
	{
		if (string.IsNullOrEmpty(animName))
		{
			return null;
		}
		if (animName.Equals("idle", StringComparison.OrdinalIgnoreCase))
		{
			return IdleAnimation;
		}
		if (animName.Equals("move", StringComparison.OrdinalIgnoreCase))
		{
			return MoveAnimation;
		}
		if (animName.Equals("talk", StringComparison.OrdinalIgnoreCase))
		{
			return TalkAnimation;
		}
		if (animName.Equals("hit", StringComparison.OrdinalIgnoreCase))
		{
			return HitAnimation;
		}
		if (animName.Equals("flight", StringComparison.OrdinalIgnoreCase))
		{
			return FlightAnimation;
		}
		DirectionalAnimation result = null;
		int num = 0;
		for (int i = 0; i < OtherAnimations.Count; i++)
		{
			if (animName.Equals(OtherAnimations[i].name, StringComparison.OrdinalIgnoreCase))
			{
				num++;
				result = OtherAnimations[i].anim;
			}
		}
		switch (num)
		{
		case 0:
			return null;
		case 1:
			return result;
		default:
		{
			int num2 = UnityEngine.Random.Range(0, num);
			num = 0;
			for (int j = 0; j < OtherAnimations.Count; j++)
			{
				if (animName.Equals(OtherAnimations[j].name, StringComparison.OrdinalIgnoreCase))
				{
					if (num == num2)
					{
						return OtherAnimations[j].anim;
					}
					num++;
				}
			}
			Debug.LogError("GetDiretionalAnimation: THIS SHOULDN'T HAPPEN");
			return null;
		}
		}
	}

	private void PlayClip(string clipName, float warpClipDuration)
	{
		PlayClip(base.spriteAnimator.GetClipByName(clipName), warpClipDuration);
	}

	private void PlayClip(tk2dSpriteAnimationClip clip, float warpClipDuration)
	{
		base.spriteAnimator.Play(clip, 0f, GetFps(clip, warpClipDuration));
		UpdateFacingRotation();
	}

	private float GetFps(tk2dSpriteAnimationClip clip, float warpClipDuration = -1f)
	{
		if (warpClipDuration > 0f)
		{
			return (float)clip.frames.Length / warpClipDuration;
		}
		if (m_fpsScale != 1f)
		{
			return (!(m_fpsScale > 0f)) ? 1E-05f : (clip.fps * m_fpsScale);
		}
		return clip.fps;
	}

	private void UpdateFacingRotation()
	{
		if (directionalType != DirectionalType.Sprite)
		{
			float num = FacingDirection + RotationOffset;
			if (directionalType == DirectionalType.SpriteAndRotation)
			{
				num -= ((m_currentActionState == null) ? m_currentBaseArtAngle : m_currentActionState.ArtAngle);
			}
			if (RotationQuantizeTo != 0f)
			{
				num = BraveMathCollege.QuantizeFloat(num, RotationQuantizeTo);
			}
			base.transform.rotation = Quaternion.Euler(0f, 0f, num);
			base.sprite.UpdateZDepth();
			base.sprite.ForceBuild();
		}
	}
}
