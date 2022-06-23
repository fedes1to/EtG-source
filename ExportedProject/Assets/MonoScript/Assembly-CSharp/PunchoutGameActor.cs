using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PunchoutGameActor : BraveBehaviour
{
	public abstract class State
	{
		public bool IsLeft;

		private int m_lastReportedFrame = -1;

		public bool IsDone { get; set; }

		public PunchoutGameActor Actor { get; set; }

		public PunchoutPlayerController ActorPlayer
		{
			get
			{
				return (PunchoutPlayerController)Actor;
			}
		}

		public PunchoutAIActor ActorEnemy
		{
			get
			{
				return (PunchoutAIActor)Actor;
			}
		}

		public virtual string AnimName
		{
			get
			{
				return null;
			}
		}

		public virtual float PunishTime
		{
			get
			{
				return 0f;
			}
		}

		public bool WasBlocked { get; set; }

		public State()
		{
		}

		public State(bool isLeft)
		{
			IsLeft = isLeft;
		}

		public virtual void Start()
		{
			if (AnimName != null)
			{
				Actor.Play(AnimName, IsLeft);
			}
			tk2dSpriteAnimator spriteAnimator = Actor.spriteAnimator;
			spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
		}

		public virtual void Update()
		{
			if (AnimName != null && Actor.aiAnimator.IsIdle())
			{
				IsDone = true;
			}
			int currentFrame = Actor.spriteAnimator.CurrentFrame;
			if (currentFrame < m_lastReportedFrame)
			{
				if (Actor.spriteAnimator.CurrentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection)
				{
					m_lastReportedFrame = currentFrame - 1;
				}
				else
				{
					m_lastReportedFrame = -1;
				}
			}
			while (currentFrame > m_lastReportedFrame)
			{
				m_lastReportedFrame++;
				OnFrame(m_lastReportedFrame);
			}
		}

		public virtual void Stop()
		{
			tk2dSpriteAnimator spriteAnimator = Actor.spriteAnimator;
			spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(AnimationCompleted));
		}

		private void AnimationCompleted(tk2dSpriteAnimator tk2DSpriteAnimator, tk2dSpriteAnimationClip tk2DSpriteAnimationClip)
		{
			OnAnimationCompleted();
		}

		public virtual void OnFrame(int currentFrame)
		{
		}

		public virtual void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
		}

		public virtual void OnAnimationCompleted()
		{
		}

		public virtual bool CanBeHit(bool isLeft)
		{
			return true;
		}

		public virtual bool IsFarAway()
		{
			return false;
		}

		public virtual bool ShouldInstantKO(int starsUsed)
		{
			return false;
		}
	}

	public class DuckState : State
	{
		public override string AnimName
		{
			get
			{
				return "duck";
			}
		}

		public override void Start()
		{
			base.Start();
			base.Actor.MoveCamera(new Vector2(0f, -0.4f), 0.2f);
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.MoveCamera(new Vector2(0f, 0f), 0.2f);
		}
	}

	public class DodgeState : State
	{
		public override string AnimName
		{
			get
			{
				return "dodge";
			}
		}

		public DodgeState(bool isLeft)
			: base(isLeft)
		{
		}

		public override void Start()
		{
			base.Start();
			base.Actor.MoveCamera(new Vector2(0.7f * (float)((!IsLeft) ? 1 : (-1)), 0f), 0.15f);
		}

		public override void Stop()
		{
			base.Stop();
			base.Actor.MoveCamera(new Vector2(0f, 0f), 0.25f);
		}
	}

	public class HitState : State
	{
		public override string AnimName
		{
			get
			{
				return "hit";
			}
		}

		public HitState(bool isLeft)
			: base(isLeft)
		{
		}
	}

	public class BlockState : State
	{
		public override string AnimName
		{
			get
			{
				return "block";
			}
		}

		public virtual void Bonk()
		{
		}
	}

	public abstract class BasicAttackState : State
	{
		public abstract int DamageFrame { get; }

		public abstract float Damage { get; }

		public BasicAttackState()
		{
		}

		public BasicAttackState(bool isLeft)
			: base(isLeft)
		{
		}

		public override void OnFrame(int currentFrame)
		{
			base.OnFrame(currentFrame);
			if (currentFrame == DamageFrame && CanHitOpponent(base.Actor.Opponent.state))
			{
				base.Actor.Opponent.Hit(IsLeft, Damage);
			}
		}

		public abstract bool CanHitOpponent(State state);
	}

	public abstract class BasicComboState : State
	{
		public State[] States;

		private int m_index;

		public State CurrentState
		{
			get
			{
				return States[m_index];
			}
		}

		public BasicComboState()
		{
			States = new State[0];
		}

		public BasicComboState(State[] states)
		{
			States = states;
		}

		public override void Start()
		{
			CurrentState.Actor = base.Actor;
			CurrentState.Start();
		}

		public override void Update()
		{
			CurrentState.Update();
			CurrentState.WasBlocked = base.WasBlocked;
			if (CurrentState.IsDone)
			{
				CurrentState.Stop();
				m_index++;
				if (m_index >= States.Length || base.Actor.Opponent.IsDead)
				{
					base.IsDone = true;
					return;
				}
				CurrentState.Actor = base.Actor;
				CurrentState.Start();
				base.WasBlocked = false;
			}
		}

		public override void OnHit(ref bool preventDamage, bool isLeft, int starsUsed)
		{
			if (m_index < States.Length)
			{
				CurrentState.OnHit(ref preventDamage, isLeft, starsUsed);
			}
		}

		public override bool CanBeHit(bool isLeft)
		{
			if (m_index < States.Length)
			{
				return !base.WasBlocked && CurrentState.CanBeHit(isLeft);
			}
			return true;
		}
	}

	public dfSprite HealthBarUI;

	public dfSprite[] StarsUI;

	public PunchoutGameActor Opponent;

	[NonSerialized]
	public float Health = 100f;

	private IEnumerator m_flashCoroutine;

	private State m_state;

	private bool m_isFlashing;

	protected List<Material> materialsToFlash = new List<Material>();

	protected List<Material> outlineMaterialsToFlash = new List<Material>();

	protected List<Material> materialsToEnableBrightnessClampOn = new List<Material>();

	protected List<Color> sourceColors = new List<Color>();

	private Vector2 m_cameraVelocity;

	private Vector2 m_cameraTarget;

	private float m_cameraTime;

	public int Stars { get; set; }

	public State LastHitBy { get; set; }

	public int CurrentFrame
	{
		get
		{
			return base.spriteAnimator.CurrentFrame;
		}
	}

	public float CurrentFrameFloat
	{
		get
		{
			return (float)base.spriteAnimator.CurrentFrame + base.spriteAnimator.clipTime % 1f;
		}
	}

	public Vector2 CameraOffset { get; set; }

	public bool IsYellow { get; set; }

	public bool IsFarAway
	{
		get
		{
			return state != null && state.IsFarAway();
		}
	}

	public abstract bool IsDead { get; }

	public State state
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != null)
			{
				m_state.Stop();
			}
			m_state = value;
			if (m_state != null)
			{
				m_state.Actor = this;
				m_state.Start();
			}
		}
	}

	public virtual void Start()
	{
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
	}

	public virtual void ManualUpdate()
	{
		if (this is PunchoutPlayerController && Health <= 0f && !(state is PunchoutPlayerController.DeathState))
		{
			Health = 100f;
			(this as PunchoutPlayerController).UpdateUI();
		}
		if ((bool)HealthBarUI)
		{
			HealthBarUI.FillAmount = Mathf.Max(0f, Health / 100f);
		}
		for (int i = 0; i < StarsUI.Length; i++)
		{
			StarsUI[i].IsVisible = i < Stars;
		}
		CameraOffset = Vector2.SmoothDamp(CameraOffset, m_cameraTarget, ref m_cameraVelocity, m_cameraTime, 100f, BraveTime.DeltaTime);
	}

	public virtual void Hit(bool isLeft, float damage, int starsUsed = 0, bool skipProcessing = false)
	{
	}

	public void Play(string name)
	{
		base.aiAnimator.FacingDirection = 90f;
		base.aiAnimator.PlayUntilFinished(name);
	}

	public void Play(string name, bool isLeft)
	{
		base.aiAnimator.FacingDirection = (isLeft ? 180 : 0);
		base.aiAnimator.PlayUntilFinished(name);
	}

	public void FlashDamage(float flashTime = 0.04f)
	{
		StopFlash();
		m_flashCoroutine = FlashColor(Color.white, flashTime);
		StartCoroutine(m_flashCoroutine);
	}

	public void FlashWarn(float flashFrames)
	{
		float flashTime = flashFrames / base.spriteAnimator.ClipFps;
		StopFlash();
		IsYellow = true;
		m_flashCoroutine = FlashColor(Color.yellow, flashTime);
		StartCoroutine(m_flashCoroutine);
		AkSoundEngine.PostEvent("Play_BOSS_RatPunchout_Flash_01", base.gameObject);
	}

	public void PulseColor(Color overrideColor, float flashFrames)
	{
		float flashTime = flashFrames / base.spriteAnimator.ClipFps;
		StopFlash();
		m_flashCoroutine = FlashColor(overrideColor, flashTime, true);
		StartCoroutine(m_flashCoroutine);
	}

	protected IEnumerator FlashColor(Color overrideColor, float flashTime, bool roundtrip = false)
	{
		if (this is PunchoutPlayerController && (this as PunchoutPlayerController).IsEevee)
		{
			yield break;
		}
		m_isFlashing = true;
		overrideColor.a = 1f;
		if ((bool)base.sprite)
		{
			base.sprite.usesOverrideMaterial = true;
		}
		materialsToEnableBrightnessClampOn.Clear();
		materialsToFlash.Clear();
		outlineMaterialsToFlash.Clear();
		Material bodyMaterial = base.sprite.renderer.material;
		materialsToFlash.Add(bodyMaterial);
		for (int i = 0; i < bodyMaterial.shaderKeywords.Length; i++)
		{
			if (bodyMaterial.shaderKeywords[i] == "BRIGHTNESS_CLAMP_ON")
			{
				bodyMaterial.DisableKeyword("BRIGHTNESS_CLAMP_ON");
				bodyMaterial.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
				materialsToEnableBrightnessClampOn.Add(bodyMaterial);
				break;
			}
		}
		tk2dSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(base.sprite);
		for (int j = 0; j < outlineSprites.Length; j++)
		{
			if ((bool)outlineSprites[j] && (bool)outlineSprites[j].renderer && (bool)outlineSprites[j].renderer.material)
			{
				outlineMaterialsToFlash.Add(outlineSprites[j].renderer.material);
			}
		}
		sourceColors.Clear();
		for (int k = 0; k < materialsToFlash.Count; k++)
		{
			materialsToFlash[k].SetColor("_OverrideColor", overrideColor);
		}
		for (int l = 0; l < outlineMaterialsToFlash.Count; l++)
		{
			sourceColors.Add(outlineMaterialsToFlash[l].GetColor("_OverrideColor"));
			outlineMaterialsToFlash[l].SetColor("_OverrideColor", overrideColor);
		}
		for (float elapsed = 0f; elapsed < flashTime; elapsed += BraveTime.DeltaTime)
		{
			float t;
			Color baseColor;
			if (roundtrip)
			{
				t = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(elapsed * 2f / flashTime, 1f));
				baseColor = new Color(0f, 0f, 0f, 0f);
			}
			else
			{
				t = 1f - elapsed / flashTime;
				baseColor = new Color(1f, 1f, 1f, 0f);
			}
			for (int m = 0; m < materialsToFlash.Count; m++)
			{
				materialsToFlash[m].SetColor("_OverrideColor", Color.Lerp(baseColor, overrideColor, t));
			}
			for (int n = 0; n < outlineMaterialsToFlash.Count; n++)
			{
				outlineMaterialsToFlash[n].SetColor("_OverrideColor", Color.Lerp(sourceColors[n], overrideColor, t));
			}
			yield return null;
		}
		StopFlash();
	}

	private void StopFlash()
	{
		if (!(this is PunchoutPlayerController) || !(this as PunchoutPlayerController).IsEevee)
		{
			for (int i = 0; i < materialsToFlash.Count; i++)
			{
				materialsToFlash[i].SetColor("_OverrideColor", new Color(1f, 1f, 1f, 0f));
			}
			for (int j = 0; j < outlineMaterialsToFlash.Count; j++)
			{
				outlineMaterialsToFlash[j].SetColor("_OverrideColor", sourceColors[j]);
			}
			for (int k = 0; k < materialsToEnableBrightnessClampOn.Count; k++)
			{
				materialsToEnableBrightnessClampOn[k].DisableKeyword("BRIGHTNESS_CLAMP_OFF");
				materialsToEnableBrightnessClampOn[k].EnableKeyword("BRIGHTNESS_CLAMP_ON");
			}
			m_isFlashing = false;
			IsYellow = false;
			if (m_flashCoroutine != null)
			{
				StopCoroutine(m_flashCoroutine);
			}
			m_flashCoroutine = null;
		}
	}

	public void MoveCamera(Vector2 offset, float time)
	{
		m_cameraTarget = offset;
		m_cameraTime = time;
	}
}
