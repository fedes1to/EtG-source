using System;
using System.Collections;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteAnimator")]
public class tk2dSpriteAnimator : BraveBehaviour
{
	private enum State
	{
		Init,
		Playing,
		Paused
	}

	[SerializeField]
	private tk2dSpriteAnimation library;

	[SerializeField]
	private int defaultClipId;

	public float AdditionalCameraVisibilityRadius;

	private float m_fidgetDuration;

	private float m_fidgetElapsed;

	public bool AnimateDuringBossIntros;

	public bool AlwaysIgnoreTimeScale;

	public bool ForceSetEveryFrame;

	public bool playAutomatically;

	[NonSerialized]
	public bool alwaysUpdateOffscreen;

	[NonSerialized]
	public bool maximumDeltaOneFrame;

	[SerializeField]
	public bool IsFrameBlendedAnimation;

	private static State globalState;

	private tk2dSpriteAnimationClip currentClip;

	public float clipTime;

	private float clipFps = -1f;

	private int previousFrame = -1;

	public Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip> OnPlayAnimationCalled;

	public Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip> AnimationCompleted;

	private Action m_onDestroyAction;

	public Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int> AnimationEventTriggered;

	private State state;

	public bool deferNextStartClip;

	private bool m_hasAttachPoints;

	protected tk2dBaseSprite _sprite;

	protected tk2dSpriteCollectionData _startingSpriteCollection;

	protected int _startingSpriteId;

	private string m_queuedAnimationName;

	private GameObject m_overrideTargetDisableObject;

	private GameObject m_cachedAudioBaseObject;

	private bool m_isCurrentlyVisible;

	public static Vector2 CameraPositionThisFrame;

	public static bool InDungeonScene;

	[NonSerialized]
	public bool ignoreTimeScale;

	[NonSerialized]
	public float OverrideTimeScale = -1f;

	private bool m_forceNextSpriteUpdate;

	public static bool g_Paused
	{
		get
		{
			return (globalState & State.Paused) != 0;
		}
		set
		{
			globalState = (value ? State.Paused : State.Init);
		}
	}

	public bool MuteAudio { get; set; }

	public bool Paused
	{
		get
		{
			return (state & State.Paused) != 0;
		}
		set
		{
			if (value)
			{
				state |= State.Paused;
			}
			else
			{
				state &= (State)(-3);
			}
		}
	}

	public tk2dSpriteAnimation Library
	{
		get
		{
			return library;
		}
		set
		{
			library = value;
		}
	}

	public int DefaultClipId
	{
		get
		{
			return defaultClipId;
		}
		set
		{
			defaultClipId = value;
		}
	}

	public tk2dSpriteAnimationClip DefaultClip
	{
		get
		{
			return GetClipById(defaultClipId);
		}
	}

	public virtual tk2dBaseSprite Sprite
	{
		get
		{
			if (_sprite == null)
			{
				_sprite = GetComponent<tk2dBaseSprite>();
				if (_sprite == null)
				{
					Debug.LogError("Sprite not found attached to tk2dSpriteAnimator.");
				}
			}
			return _sprite;
		}
	}

	public bool Playing
	{
		get
		{
			return (state & State.Playing) != 0;
		}
	}

	public tk2dSpriteAnimationClip CurrentClip
	{
		get
		{
			return currentClip;
		}
	}

	public float ClipTimeSeconds
	{
		get
		{
			return (!(clipFps > 0f)) ? (clipTime / currentClip.fps) : (clipTime / clipFps);
		}
	}

	public float ClipFps
	{
		get
		{
			return clipFps;
		}
		set
		{
			if (currentClip != null)
			{
				clipFps = ((!(value > 0f)) ? currentClip.fps : value);
			}
		}
	}

	public static float DefaultFps
	{
		get
		{
			return 0f;
		}
	}

	public int CurrentFrame
	{
		get
		{
			switch (currentClip.wrapMode)
			{
			case tk2dSpriteAnimationClip.WrapMode.Single:
				return 0;
			case tk2dSpriteAnimationClip.WrapMode.Once:
				return Mathf.Min((int)clipTime, currentClip.frames.Length);
			case tk2dSpriteAnimationClip.WrapMode.Loop:
			case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
			case tk2dSpriteAnimationClip.WrapMode.LoopFidget:
				return (int)clipTime % currentClip.frames.Length;
			case tk2dSpriteAnimationClip.WrapMode.LoopSection:
			{
				int num2 = (int)clipTime;
				int result = currentClip.loopStart + (num2 - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart);
				if (num2 >= currentClip.loopStart)
				{
					return result;
				}
				return num2;
			}
			case tk2dSpriteAnimationClip.WrapMode.PingPong:
			{
				int num = ((currentClip.frames.Length > 1) ? ((int)clipTime % (currentClip.frames.Length + currentClip.frames.Length - 2)) : 0);
				if (num >= currentClip.frames.Length)
				{
					num = 2 * currentClip.frames.Length - 2 - num;
				}
				return num;
			}
			default:
				Debug.LogError("Unhandled clip wrap mode");
				goto case tk2dSpriteAnimationClip.WrapMode.Loop;
			}
		}
	}

	public GameObject AudioBaseObject
	{
		get
		{
			if (m_cachedAudioBaseObject == null)
			{
				if ((bool)base.transform.parent && (bool)base.transform.parent.GetComponent<PlayerController>())
				{
					m_cachedAudioBaseObject = base.transform.parent.gameObject;
				}
				else
				{
					m_cachedAudioBaseObject = base.gameObject;
				}
			}
			return m_cachedAudioBaseObject;
		}
		set
		{
			m_cachedAudioBaseObject = value;
		}
	}

	public void ForceClearCurrentClip()
	{
		currentClip = null;
	}

	private void OnEnable()
	{
		if (Sprite == null)
		{
			base.enabled = false;
		}
	}

	private void Awake()
	{
		if ((bool)Sprite)
		{
			_startingSpriteCollection = Sprite.Collection;
			_startingSpriteId = Sprite.spriteId;
		}
		if (AlwaysIgnoreTimeScale)
		{
			ignoreTimeScale = true;
		}
	}

	private void Start()
	{
		if (!deferNextStartClip && playAutomatically && !IsPlaying(DefaultClip))
		{
			Play(DefaultClip);
		}
		deferNextStartClip = false;
		if ((bool)GetComponent<tk2dSpriteAttachPoint>())
		{
			m_hasAttachPoints = true;
		}
	}

	public void OnSpawned()
	{
		if (base.enabled)
		{
			OnEnable();
			Start();
		}
	}

	public void OnDespawned()
	{
		if (playAutomatically)
		{
			StopAndResetFrame();
		}
		else
		{
			Stop();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public static tk2dSpriteAnimator AddComponent(GameObject go, tk2dSpriteAnimation anim, int clipId)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = anim.clips[clipId];
		tk2dSpriteAnimator tk2dSpriteAnimator2 = go.AddComponent<tk2dSpriteAnimator>();
		tk2dSpriteAnimator2.Library = anim;
		if (tk2dSpriteAnimationClip2.frames[0].requiresOffscreenUpdate)
		{
			tk2dSpriteAnimator2.m_forceNextSpriteUpdate = true;
		}
		tk2dSpriteAnimator2.SetSprite(tk2dSpriteAnimationClip2.frames[0].spriteCollection, tk2dSpriteAnimationClip2.frames[0].spriteId);
		if (tk2dSpriteAnimationClip2.frames[0].requiresOffscreenUpdate)
		{
			tk2dSpriteAnimator2.m_forceNextSpriteUpdate = true;
		}
		return tk2dSpriteAnimator2;
	}

	private tk2dSpriteAnimationClip GetClipByNameVerbose(string name)
	{
		if (library == null)
		{
			Debug.LogError("Library not set");
			return null;
		}
		tk2dSpriteAnimationClip clipByName = library.GetClipByName(name);
		if (clipByName == null)
		{
			Debug.LogError("Unable to find clip '" + name + "' in library");
			return null;
		}
		return clipByName;
	}

	public void Play()
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		Play(currentClip);
	}

	public void Play(string name)
	{
		Play(GetClipByNameVerbose(name));
	}

	public void Play(tk2dSpriteAnimationClip clip)
	{
		Play(clip, 0f, DefaultFps);
	}

	public void PlayFromFrame(int frame)
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		PlayFromFrame(currentClip, frame);
	}

	public void PlayFromFrame(string name, int frame)
	{
		PlayFromFrame(GetClipByNameVerbose(name), frame);
	}

	public void PlayFromFrame(tk2dSpriteAnimationClip clip, int frame)
	{
		PlayFrom(clip, ((float)frame + 0.001f) / clip.fps);
	}

	public void PlayFrom(float clipStartTime)
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		PlayFrom(currentClip, clipStartTime);
	}

	public void PlayFrom(string name, float clipStartTime)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = ((!library) ? null : library.GetClipByName(name));
		if (tk2dSpriteAnimationClip2 == null)
		{
			ClipNameError(name);
		}
		else
		{
			PlayFrom(tk2dSpriteAnimationClip2, clipStartTime);
		}
	}

	public void PlayFrom(tk2dSpriteAnimationClip clip, float clipStartTime)
	{
		Play(clip, clipStartTime, DefaultFps);
	}

	public void QueueAnimation(string animationName)
	{
		m_queuedAnimationName = animationName;
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StartQueuedAnimationSimple));
	}

	private void StartQueuedAnimationSimple(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		Play(m_queuedAnimationName);
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StartQueuedAnimationSimple));
	}

	private void StopAndDisableGameObject(tk2dSpriteAnimator source, tk2dSpriteAnimationClip clip)
	{
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StopAndDisableGameObject));
		Stop();
		if (m_overrideTargetDisableObject != null)
		{
			m_overrideTargetDisableObject.SetActive(false);
			m_overrideTargetDisableObject = null;
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	private void StopAndDestroyGameObject(tk2dSpriteAnimator source, tk2dSpriteAnimationClip clip)
	{
		if (m_onDestroyAction != null)
		{
			m_onDestroyAction();
		}
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StopAndDestroyGameObject));
		Stop();
		SpawnManager.Despawn(base.gameObject);
	}

	public void PlayAndDestroyObject(string clipName = "", Action onDestroy = null)
	{
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StopAndDestroyGameObject));
		if (onDestroy != null)
		{
			m_onDestroyAction = onDestroy;
		}
		if (string.IsNullOrEmpty(clipName))
		{
			Play();
		}
		else
		{
			Play(clipName);
		}
	}

	public void PlayAndDisableObject(string clipName = "", GameObject overrideTargetObject = null)
	{
		m_overrideTargetDisableObject = overrideTargetObject;
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StopAndDisableGameObject));
		if (string.IsNullOrEmpty(clipName))
		{
			Play();
		}
		else
		{
			Play(clipName);
		}
	}

	public void PlayAndDisableRenderer(string clipName = "")
	{
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StopAndDisableRenderer));
		if (string.IsNullOrEmpty(clipName))
		{
			Play();
		}
		else
		{
			Play(clipName);
		}
	}

	private void StopAndDisableRenderer(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(StopAndDisableRenderer));
		Stop();
		GetComponent<Renderer>().enabled = false;
	}

	public void PlayForDurationForceLoop(tk2dSpriteAnimationClip clip, float duration)
	{
		if (clip == null)
		{
			ClipNameError(base.name);
			return;
		}
		Play(clip);
		if (duration < 0f)
		{
			duration = clip.BaseClipLength;
		}
		StartCoroutine(RevertToClipForceLoop(clip, duration));
	}

	private IEnumerator RevertToClipForceLoop(tk2dSpriteAnimationClip playingClip, float duration)
	{
		float timer = duration;
		yield return null;
		while (timer > 0f)
		{
			timer -= GetDeltaTime();
			if (timer <= 0f)
			{
				break;
			}
			if (!IsPlaying(playingClip))
			{
				Play(playingClip);
			}
			yield return null;
		}
	}

	public void PlayForDuration(string name, float duration, string revertAnimName, bool returnToLoopSection = false)
	{
		tk2dSpriteAnimationClip revertToClip = ((!library) ? null : library.GetClipByName(revertAnimName));
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = ((!library) ? null : library.GetClipByName(name));
		if (tk2dSpriteAnimationClip2 != null)
		{
			Play(tk2dSpriteAnimationClip2);
			if (duration < 0f)
			{
				duration = tk2dSpriteAnimationClip2.BaseClipLength;
			}
			StartCoroutine(RevertToClip(tk2dSpriteAnimationClip2, revertToClip, duration, returnToLoopSection));
		}
	}

	public void PlayForDuration(string name, float duration)
	{
		tk2dSpriteAnimationClip revertToClip = currentClip;
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = ((!library) ? null : library.GetClipByName(name));
		if (tk2dSpriteAnimationClip2 == null)
		{
			ClipNameError(name);
			return;
		}
		Play(tk2dSpriteAnimationClip2);
		if (duration < 0f)
		{
			duration = tk2dSpriteAnimationClip2.BaseClipLength;
		}
		StartCoroutine(RevertToClip(tk2dSpriteAnimationClip2, revertToClip, duration));
	}

	private IEnumerator RevertToClip(tk2dSpriteAnimationClip playingClip, tk2dSpriteAnimationClip revertToClip, float duration, bool returnToLoopSection = false)
	{
		float timer = duration;
		yield return null;
		while (currentClip == playingClip)
		{
			timer -= GetDeltaTime();
			if (timer <= 0f)
			{
				if (revertToClip != null)
				{
					if (revertToClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection && returnToLoopSection)
					{
						Play(revertToClip, (float)revertToClip.loopStart / revertToClip.fps, DefaultFps);
					}
					else
					{
						Play(revertToClip);
					}
				}
				break;
			}
			yield return null;
		}
	}

	public void PlayAndForceTime(string clipName, float forceTime)
	{
		PlayAndForceTime(GetClipByName(clipName), forceTime);
	}

	public void PlayAndForceTime(tk2dSpriteAnimationClip clip, float forceTime)
	{
		Play(clip, 0f, clip.fps * (clip.BaseClipLength / forceTime));
	}

	public void Play(tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps, bool skipEvents = false)
	{
		if (OnPlayAnimationCalled != null)
		{
			OnPlayAnimationCalled(this, clip);
		}
		if (clip != null)
		{
			float num = ((!(overrideFps > 0f)) ? clip.fps : overrideFps);
			if (clipStartTime == 0f && IsPlaying(clip))
			{
				clipFps = num;
				return;
			}
			state |= State.Playing;
			currentClip = clip;
			clipFps = num;
			if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single || currentClip.frames == null)
			{
				WarpClipToLocalTime(currentClip, 0f, skipEvents);
				state &= (State)(-2);
			}
			else if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame || currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomLoop)
			{
				int num2 = UnityEngine.Random.Range(0, currentClip.frames.Length);
				WarpClipToLocalTime(currentClip, num2, skipEvents);
				if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame)
				{
					previousFrame = -1;
					state &= (State)(-2);
				}
			}
			else
			{
				float num3 = clipStartTime * clipFps;
				if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Once && num3 >= clipFps * (float)currentClip.frames.Length)
				{
					WarpClipToLocalTime(currentClip, currentClip.frames.Length - 1, skipEvents);
					state &= (State)(-2);
				}
				else
				{
					WarpClipToLocalTime(currentClip, num3, skipEvents);
					clipTime = num3;
				}
			}
		}
		else
		{
			Debug.LogWarning("Calling clip.Play() with a null clip");
			OnAnimationCompleted();
			state &= (State)(-2);
		}
	}

	public bool QueryPreviousInvulnerabilityFrame(int framesBack)
	{
		if (CurrentClip != null && CurrentFrame >= framesBack && CurrentFrame < CurrentClip.frames.Length)
		{
			return CurrentClip.frames[CurrentFrame - framesBack].invulnerableFrame;
		}
		return false;
	}

	public bool QueryInvulnerabilityFrame()
	{
		if (CurrentClip != null && CurrentFrame >= 0 && CurrentFrame < CurrentClip.frames.Length)
		{
			return CurrentClip.frames[CurrentFrame].invulnerableFrame;
		}
		return false;
	}

	public bool QueryGroundedFrame()
	{
		if (CurrentClip != null && CurrentFrame >= 0 && CurrentFrame < CurrentClip.frames.Length)
		{
			return CurrentClip.frames[CurrentFrame].groundedFrame;
		}
		return true;
	}

	public void Stop()
	{
		state &= (State)(-2);
	}

	public void StopAndResetFrame()
	{
		if (currentClip != null)
		{
			if (currentClip.frames[0].requiresOffscreenUpdate)
			{
				m_forceNextSpriteUpdate = true;
			}
			SetSprite(currentClip.frames[0].spriteCollection, currentClip.frames[0].spriteId);
			if (currentClip.frames[0].requiresOffscreenUpdate)
			{
				m_forceNextSpriteUpdate = true;
			}
		}
		Stop();
	}

	public void StopAndResetFrameToDefault()
	{
		if (currentClip != null)
		{
			if (currentClip.frames[0].requiresOffscreenUpdate)
			{
				m_forceNextSpriteUpdate = true;
			}
			SetSprite(_startingSpriteCollection, _startingSpriteId);
			if (currentClip.frames[0].requiresOffscreenUpdate)
			{
				m_forceNextSpriteUpdate = true;
			}
		}
		Stop();
	}

	public bool IsPlaying(string name)
	{
		return Playing && CurrentClip != null && CurrentClip.name == name;
	}

	public bool IsPlaying(tk2dSpriteAnimationClip clip)
	{
		return Playing && CurrentClip != null && CurrentClip == clip;
	}

	public tk2dSpriteAnimationClip GetClipById(int id)
	{
		if (library == null)
		{
			return null;
		}
		return library.GetClipById(id);
	}

	public int GetClipIdByName(string name)
	{
		return (!library) ? (-1) : library.GetClipIdByName(name);
	}

	public tk2dSpriteAnimationClip GetClipByName(string name)
	{
		return (!library) ? null : library.GetClipByName(name);
	}

	public void Pause()
	{
		state |= State.Paused;
	}

	public void Resume()
	{
		state &= (State)(-3);
	}

	public void SetFrame(int currFrame)
	{
		SetFrame(currFrame, true);
	}

	public void SetFrame(int currFrame, bool triggerEvent)
	{
		if (currentClip == null)
		{
			currentClip = DefaultClip;
		}
		if (currentClip != null)
		{
			int num = currFrame % currentClip.frames.Length;
			SetFrameInternal(num);
			if (triggerEvent && currentClip.frames.Length > 0 && currFrame >= 0)
			{
				ProcessEvents(num - 1, num, 1);
			}
		}
	}

	public void UpdateAnimation(float deltaTime)
	{
		State state = this.state | globalState;
		if (state != State.Playing)
		{
			return;
		}
		if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopFidget && m_fidgetDuration > 0f)
		{
			m_fidgetElapsed += deltaTime;
			if (m_fidgetElapsed >= m_fidgetDuration)
			{
				m_fidgetElapsed = 0f;
				m_fidgetDuration = 0f;
				clipTime += deltaTime * clipFps;
			}
		}
		else
		{
			clipTime += deltaTime * clipFps;
		}
		int num = previousFrame;
		switch (currentClip.wrapMode)
		{
		case tk2dSpriteAnimationClip.WrapMode.Loop:
		case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
		{
			int num3 = (int)clipTime % currentClip.frames.Length;
			SetFrameInternal(num3);
			if (num3 < num)
			{
				ProcessEvents(num, currentClip.frames.Length - 1, 1);
				ProcessEvents(-1, num3, 1);
			}
			else
			{
				ProcessEvents(num, num3, 1);
			}
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.LoopFidget:
		{
			int num7 = (int)clipTime % currentClip.frames.Length;
			SetFrameInternal(num7);
			if (num7 < num)
			{
				ProcessEvents(num, currentClip.frames.Length - 1, 1);
				ProcessEvents(-1, num7, 1);
				m_fidgetElapsed = 0f;
				m_fidgetDuration = Mathf.Lerp(currentClip.minFidgetDuration, currentClip.maxFidgetDuration, UnityEngine.Random.value);
			}
			else
			{
				ProcessEvents(num, num7, 1);
			}
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.LoopSection:
		{
			int num4 = (int)clipTime;
			int num5 = currentClip.loopStart + (num4 - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart);
			if (num4 >= currentClip.loopStart)
			{
				SetFrameInternal(num5);
				num4 = num5;
				if (num < currentClip.loopStart)
				{
					ProcessEvents(num, currentClip.loopStart - 1, 1);
					ProcessEvents(currentClip.loopStart - 1, num4, 1);
				}
				else if (num4 < num)
				{
					ProcessEvents(num, currentClip.frames.Length - 1, 1);
					ProcessEvents(currentClip.loopStart - 1, num4, 1);
				}
				else
				{
					ProcessEvents(num, num4, 1);
				}
			}
			else
			{
				SetFrameInternal(num4);
				ProcessEvents(num, num4, 1);
			}
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.PingPong:
		{
			int num6 = ((currentClip.frames.Length > 1) ? (currentClip.frames.Length + currentClip.frames.Length - 2) : 0);
			int direction = 1;
			if (num6 >= currentClip.frames.Length)
			{
				num6 = 2 * currentClip.frames.Length - 2 - num6;
				direction = -1;
			}
			if (num6 < num)
			{
				direction = -1;
			}
			SetFrameInternal(num6);
			ProcessEvents(num, num6, direction);
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.Once:
		{
			int num2 = (int)clipTime;
			if (num2 >= currentClip.frames.Length)
			{
				SetFrameInternal(currentClip.frames.Length - 1);
				this.state &= (State)(-2);
				ProcessEvents(num, currentClip.frames.Length - 1, 1);
				OnAnimationCompleted();
			}
			else
			{
				SetFrameInternal(num2);
				ProcessEvents(num, num2, 1);
			}
			break;
		}
		case tk2dSpriteAnimationClip.WrapMode.RandomFrame:
		case tk2dSpriteAnimationClip.WrapMode.Single:
			break;
		}
	}

	private void ClipNameError(string name)
	{
		Debug.LogError("Unable to find clip named '" + name + "' in library");
	}

	private void ClipIdError(int id)
	{
		Debug.LogError("Play - Invalid clip id '" + id + "' in library");
	}

	private void WarpClipToLocalTime(tk2dSpriteAnimationClip clip, float time, bool skipEvents)
	{
		clipTime = time;
		int num = (int)clipTime % clip.frames.Length;
		tk2dSpriteAnimationFrame tk2dSpriteAnimationFrame2 = clip.frames[num];
		if (tk2dSpriteAnimationFrame2.requiresOffscreenUpdate)
		{
			m_forceNextSpriteUpdate = true;
		}
		SetSprite(tk2dSpriteAnimationFrame2.spriteCollection, tk2dSpriteAnimationFrame2.spriteId);
		if (tk2dSpriteAnimationFrame2.requiresOffscreenUpdate)
		{
			m_forceNextSpriteUpdate = true;
		}
		if (tk2dSpriteAnimationFrame2.triggerEvent && !skipEvents)
		{
			if (AnimationEventTriggered != null)
			{
				AnimationEventTriggered(this, clip, num);
			}
			if (!base.aiActor && tk2dSpriteAnimationFrame2.eventOutline != 0)
			{
				if (tk2dSpriteAnimationFrame2.eventOutline == tk2dSpriteAnimationFrame.OutlineModifier.TurnOn && !SpriteOutlineManager.HasOutline(base.sprite))
				{
					SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black, 0.1f);
				}
				if (tk2dSpriteAnimationFrame2.eventOutline == tk2dSpriteAnimationFrame.OutlineModifier.TurnOff && SpriteOutlineManager.HasOutline(base.sprite))
				{
					SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
				}
			}
			if (!string.IsNullOrEmpty(tk2dSpriteAnimationFrame2.eventAudio) && !MuteAudio)
			{
				AkSoundEngine.PostEvent(tk2dSpriteAnimationFrame2.eventAudio, AudioBaseObject);
			}
			if (!string.IsNullOrEmpty(tk2dSpriteAnimationFrame2.eventVfx) && (bool)base.aiAnimator)
			{
				base.aiAnimator.PlayVfx(tk2dSpriteAnimationFrame2.eventVfx);
			}
			if (!string.IsNullOrEmpty(tk2dSpriteAnimationFrame2.eventStopVfx) && (bool)base.aiAnimator)
			{
				base.aiAnimator.StopVfx(tk2dSpriteAnimationFrame2.eventStopVfx);
			}
			if (tk2dSpriteAnimationFrame2.eventLerpEmissive)
			{
				StartCoroutine(HandleEmissivePowerLerp(tk2dSpriteAnimationFrame2.eventLerpEmissiveTime, tk2dSpriteAnimationFrame2.eventLerpEmissivePower));
			}
			if (tk2dSpriteAnimationFrame2.forceMaterialUpdate && (!base.aiActor || !base.aiActor.IsBlackPhantom))
			{
				Sprite.ForceUpdateMaterial();
			}
		}
		previousFrame = num;
	}

	private void SetFrameInternal(int currFrame)
	{
		if (previousFrame != currFrame)
		{
			if (currentClip.frames[currFrame].requiresOffscreenUpdate)
			{
				m_forceNextSpriteUpdate = true;
			}
			SetSprite(currentClip.frames[currFrame].spriteCollection, currentClip.frames[currFrame].spriteId);
			if (currentClip.frames[currFrame].requiresOffscreenUpdate)
			{
				m_forceNextSpriteUpdate = true;
			}
			previousFrame = currFrame;
		}
		if (IsFrameBlendedAnimation)
		{
			float value = clipTime % 1f;
			base.sprite.renderer.material.SetFloat("_BlendFraction", value);
		}
	}

	private void ProcessEvents(int start, int last, int direction)
	{
		if (start == last || Mathf.Sign(last - start) != Mathf.Sign(direction))
		{
			return;
		}
		int num = last + direction;
		tk2dSpriteAnimationFrame[] frames = currentClip.frames;
		for (int i = start + direction; i != num; i += direction)
		{
			if (ForceSetEveryFrame)
			{
				SetFrameInternal(i);
			}
			if (frames[i].triggerEvent && AnimationEventTriggered != null)
			{
				AnimationEventTriggered(this, currentClip, i);
			}
			if (frames[i].triggerEvent && !string.IsNullOrEmpty(frames[i].eventAudio) && !MuteAudio)
			{
				AkSoundEngine.PostEvent(frames[i].eventAudio, AudioBaseObject);
			}
			if (!string.IsNullOrEmpty(frames[i].eventVfx) && (bool)base.aiAnimator)
			{
				base.aiAnimator.PlayVfx(frames[i].eventVfx);
			}
			if (!string.IsNullOrEmpty(frames[i].eventStopVfx) && (bool)base.aiAnimator)
			{
				base.aiAnimator.StopVfx(frames[i].eventStopVfx);
			}
			if (frames[i].eventLerpEmissive)
			{
				StartCoroutine(HandleEmissivePowerLerp(frames[i].eventLerpEmissiveTime, frames[i].eventLerpEmissivePower));
			}
		}
	}

	private IEnumerator HandleEmissivePowerLerp(float duration, float targetPower)
	{
		if (!Application.isPlaying)
		{
			yield break;
		}
		Material targetMaterial = base.sprite.renderer.material;
		if (!targetMaterial.HasProperty("_EmissivePower"))
		{
			yield break;
		}
		base.sprite.usesOverrideMaterial = true;
		if (duration <= 0f)
		{
			targetMaterial.SetFloat("_EmissivePower", targetPower);
			yield break;
		}
		float elapsed = 0f;
		float startPower = targetMaterial.GetFloat("_EmissivePower");
		while (elapsed < duration)
		{
			elapsed += ((!AnimateDuringBossIntros || !GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
			float t = elapsed / duration;
			targetMaterial.SetFloat("_EmissivePower", Mathf.Lerp(startPower, targetPower, t));
			yield return null;
		}
	}

	private void OnAnimationCompleted()
	{
		previousFrame = -1;
		if (AnimationCompleted != null)
		{
			AnimationCompleted(this, currentClip);
		}
	}

	private void HandleVisibilityCheck()
	{
		if (alwaysUpdateOffscreen)
		{
			m_isCurrentlyVisible = true;
			return;
		}
		if (!InDungeonScene)
		{
			m_isCurrentlyVisible = true;
			return;
		}
		Vector2 vector = base.transform.position.XY() - CameraPositionThisFrame;
		vector.y *= 1.7f;
		m_isCurrentlyVisible = vector.sqrMagnitude < 420f + AdditionalCameraVisibilityRadius * AdditionalCameraVisibilityRadius;
	}

	private float GetDeltaTime()
	{
		float num = ((!AnimateDuringBossIntros || !GameManager.IsBossIntro) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME);
		if ((bool)base.aiActor)
		{
			num = base.aiActor.LocalDeltaTime;
		}
		if (OverrideTimeScale > 0f)
		{
			num *= OverrideTimeScale;
		}
		if (ignoreTimeScale)
		{
			num = GameManager.INVARIANT_DELTA_TIME;
		}
		if (maximumDeltaOneFrame && CurrentClip != null)
		{
			num = Mathf.Min(num, 1f / CurrentClip.fps);
		}
		return num;
	}

	public virtual void LateUpdate()
	{
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH)
		{
			HandleVisibilityCheck();
		}
		deferNextStartClip = false;
		UpdateAnimation(GetDeltaTime());
	}

	public virtual void SetSprite(tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		bool flag = alwaysUpdateOffscreen;
		if (!alwaysUpdateOffscreen)
		{
			flag = base.renderer.isVisible;
			if (Application.isPlaying && GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH)
			{
				flag |= m_isCurrentlyVisible;
			}
		}
		if (alwaysUpdateOffscreen || !(Sprite is tk2dSprite) || flag || !Application.isPlaying || m_forceNextSpriteUpdate || m_hasAttachPoints)
		{
			Sprite.SetSprite(spriteCollection, spriteId);
			m_forceNextSpriteUpdate = false;
		}
		else
		{
			Sprite.hasOffScreenCachedUpdate = true;
			Sprite.offScreenCachedCollection = spriteCollection;
			Sprite.offScreenCachedID = spriteId;
		}
	}

	public void OnBecameVisible()
	{
		if ((bool)this && (bool)Sprite && Sprite.hasOffScreenCachedUpdate)
		{
			Sprite.hasOffScreenCachedUpdate = false;
			Sprite.SetSprite(Sprite.offScreenCachedCollection, Sprite.offScreenCachedID);
			Sprite.UpdateZDepth();
		}
	}

	public void ForceInvisibleSpriteUpdate()
	{
		if ((bool)this && (bool)Sprite && Sprite.hasOffScreenCachedUpdate)
		{
			Sprite.hasOffScreenCachedUpdate = false;
			Sprite.SetSprite(Sprite.offScreenCachedCollection, Sprite.offScreenCachedID);
			Sprite.UpdateZDepth();
		}
	}

	public Vector2[] GetNextFrameUVs()
	{
		if (state == State.Playing)
		{
			int num = (CurrentFrame + 1) % currentClip.frames.Length;
			if (CurrentFrame + 1 >= currentClip.frames.Length && currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.LoopSection)
			{
				num = currentClip.loopStart;
			}
			return currentClip.frames[num].spriteCollection.spriteDefinitions[currentClip.frames[num].spriteId].uvs;
		}
		return Sprite.GetCurrentSpriteDef().uvs;
	}
}
