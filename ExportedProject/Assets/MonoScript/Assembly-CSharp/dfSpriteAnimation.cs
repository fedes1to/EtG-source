using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Tweens/Sprite Animator")]
public class dfSpriteAnimation : dfTweenPlayableBase
{
	[SerializeField]
	private string animationName = "ANIMATION";

	[SerializeField]
	private dfAnimationClip clip;

	[SerializeField]
	public dfAnimationClip alternativeLoopClip;

	[SerializeField]
	public float percentChanceToPlayAlternative = 0.05f;

	private dfAnimationClip m_cachedBaseClip;

	[SerializeField]
	private dfComponentMemberInfo memberInfo = new dfComponentMemberInfo();

	[SerializeField]
	private dfTweenLoopType loopType = dfTweenLoopType.Loop;

	[SerializeField]
	public int LoopSectionFrameTarget = 7;

	[SerializeField]
	public float LoopSectionFirstLength = -1f;

	[SerializeField]
	public float LoopDelayMin;

	[SerializeField]
	public float LoopDelayMax;

	[SerializeField]
	public bool maxOneFrameDelta;

	[SerializeField]
	private float length = 1f;

	[SerializeField]
	private bool autoStart;

	[SerializeField]
	private bool skipToEndOnStop;

	[SerializeField]
	private dfPlayDirection playDirection;

	private bool autoRunStarted;

	private bool isRunning;

	private bool isPaused;

	private dfControl m_selfControl;

	public bool UseDefaultSpriteNameProperty = true;

	private System.Random myRandom;

	private float m_elapsedSinceLoop;

	private float m_lastRealtime;

	public dfAnimationClip Clip
	{
		get
		{
			return clip;
		}
		set
		{
			clip = value;
		}
	}

	public dfComponentMemberInfo Target
	{
		get
		{
			return memberInfo;
		}
		set
		{
			memberInfo = value;
		}
	}

	public bool AutoRun
	{
		get
		{
			return autoStart;
		}
		set
		{
			autoStart = value;
		}
	}

	public float Length
	{
		get
		{
			return length;
		}
		set
		{
			length = Mathf.Max(value, 0.03f);
		}
	}

	public dfTweenLoopType LoopType
	{
		get
		{
			return loopType;
		}
		set
		{
			loopType = value;
		}
	}

	public dfPlayDirection Direction
	{
		get
		{
			return playDirection;
		}
		set
		{
			playDirection = value;
			if (IsPlaying)
			{
				Play();
			}
		}
	}

	public bool IsPaused
	{
		get
		{
			return isRunning && isPaused;
		}
		set
		{
			if (value != IsPaused)
			{
				if (value)
				{
					Pause();
				}
				else
				{
					Resume();
				}
			}
		}
	}

	public override bool IsPlaying
	{
		get
		{
			return isRunning;
		}
	}

	public override string TweenName
	{
		get
		{
			return animationName;
		}
		set
		{
			animationName = value;
		}
	}

	public event TweenNotification AnimationStarted;

	public event TweenNotification AnimationStopped;

	public event TweenNotification AnimationPaused;

	public event TweenNotification AnimationResumed;

	public event TweenNotification AnimationReset;

	public event TweenNotification AnimationCompleted;

	public void Awake()
	{
	}

	public void Start()
	{
		m_lastRealtime = Time.realtimeSinceStartup;
		m_selfControl = GetComponent<dfControl>();
		m_cachedBaseClip = clip;
	}

	public void LateUpdate()
	{
		if (AutoRun && !IsPlaying && !autoRunStarted)
		{
			autoRunStarted = true;
			Play();
		}
	}

	public void PlayForward()
	{
		playDirection = dfPlayDirection.Forward;
		Play();
	}

	public void PlayReverse()
	{
		playDirection = dfPlayDirection.Reverse;
		Play();
	}

	public void Pause()
	{
		if (isRunning)
		{
			isPaused = true;
			onPaused();
		}
	}

	public void Resume()
	{
		if (isRunning && isPaused)
		{
			isPaused = false;
			onResumed();
		}
	}

	public override void Play()
	{
		if (IsPlaying)
		{
			Stop();
		}
		if (base.enabled && base.gameObject.activeSelf && base.gameObject.activeInHierarchy)
		{
			if (memberInfo == null)
			{
				throw new NullReferenceException("Animation target is NULL");
			}
			StartCoroutine(Execute());
		}
	}

	public override void Reset()
	{
		List<string> list = ((!(clip != null)) ? null : clip.Sprites);
		if (memberInfo.IsValid && list != null && list.Count > 0)
		{
			SetProperty(memberInfo.Component, memberInfo.MemberName, list[0]);
		}
		if (isRunning)
		{
			StopAllCoroutines();
			isRunning = false;
			isPaused = false;
			onReset();
		}
	}

	public override void Stop()
	{
		if (isRunning)
		{
			List<string> list = ((!(clip != null)) ? null : clip.Sprites);
			if (skipToEndOnStop && list != null)
			{
				setFrame(Mathf.Max(list.Count - 1, 0));
			}
			StopAllCoroutines();
			isRunning = false;
			isPaused = false;
			onStopped();
		}
	}

	protected void onPaused()
	{
		SendMessage("AnimationPaused", this, SendMessageOptions.DontRequireReceiver);
		if (this.AnimationPaused != null)
		{
			this.AnimationPaused(this);
		}
	}

	protected void onResumed()
	{
		SendMessage("AnimationResumed", this, SendMessageOptions.DontRequireReceiver);
		if (this.AnimationResumed != null)
		{
			this.AnimationResumed(this);
		}
	}

	protected void onStarted()
	{
		SendMessage("AnimationStarted", this, SendMessageOptions.DontRequireReceiver);
		if (this.AnimationStarted != null)
		{
			this.AnimationStarted(this);
		}
	}

	protected void onStopped()
	{
		SendMessage("AnimationStopped", this, SendMessageOptions.DontRequireReceiver);
		if (this.AnimationStopped != null)
		{
			this.AnimationStopped(this);
		}
	}

	protected void onReset()
	{
		SendMessage("AnimationReset", this, SendMessageOptions.DontRequireReceiver);
		if (this.AnimationReset != null)
		{
			this.AnimationReset(this);
		}
	}

	protected void onCompleted()
	{
		SendMessage("AnimationCompleted", this, SendMessageOptions.DontRequireReceiver);
		if (this.AnimationCompleted != null)
		{
			this.AnimationCompleted(this);
		}
	}

	internal static void SetProperty(object target, string property, object value)
	{
		if (target == null)
		{
			throw new NullReferenceException("Target is null");
		}
		MemberInfo[] member = target.GetType().GetMember(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (member == null || member.Length == 0)
		{
			throw new IndexOutOfRangeException("Property not found: " + property);
		}
		MemberInfo memberInfo = member[0];
		if (memberInfo is FieldInfo)
		{
			((FieldInfo)memberInfo).SetValue(target, value);
			return;
		}
		if (memberInfo is PropertyInfo)
		{
			((PropertyInfo)memberInfo).SetValue(target, value, null);
			return;
		}
		throw new InvalidOperationException("Member type not supported: " + memberInfo.GetMemberType());
	}

	private IEnumerator Execute()
	{
		dfSprite localTargetSprite = memberInfo.Component as dfSprite;
		if (myRandom == null)
		{
			myRandom = new System.Random();
		}
		if (clip == null || clip.Sprites == null || clip.Sprites.Count == 0)
		{
			yield break;
		}
		isRunning = true;
		isPaused = false;
		onStarted();
		m_elapsedSinceLoop = 0f;
		m_lastRealtime = Time.realtimeSinceStartup;
		int direction = ((playDirection == dfPlayDirection.Forward) ? 1 : (-1));
		int lastFrameIndex = ((direction != 1) ? (clip.Sprites.Count - 1) : 0);
		setFrame(lastFrameIndex);
		while (true)
		{
			yield return null;
			if ((bool)localTargetSprite && !localTargetSprite.IsVisible)
			{
				continue;
			}
			float localDeltaTime = Time.realtimeSinceStartup - m_lastRealtime;
			if (maxOneFrameDelta)
			{
				localDeltaTime = Mathf.Min(localDeltaTime, 1f / ((float)clip.Sprites.Count / length));
			}
			if (IsPaused)
			{
				continue;
			}
			List<string> sprites = clip.Sprites;
			int maxFrameIndex = sprites.Count - 1;
			int testFrameIndex = Mathf.FloorToInt(Mathf.Clamp01(m_elapsedSinceLoop / length) * (float)sprites.Count);
			if (loopType == dfTweenLoopType.LoopSection && LoopSectionFirstLength > 0f && testFrameIndex < LoopSectionFrameTarget)
			{
				float num = length / LoopSectionFirstLength;
				m_elapsedSinceLoop += localDeltaTime * num;
			}
			else
			{
				m_elapsedSinceLoop += localDeltaTime;
			}
			m_lastRealtime = Time.realtimeSinceStartup;
			int frameIndex = Mathf.FloorToInt(Mathf.Clamp01(m_elapsedSinceLoop / length) * (float)sprites.Count);
			if (m_elapsedSinceLoop >= length)
			{
				switch (loopType)
				{
				case dfTweenLoopType.Once:
					isRunning = false;
					onCompleted();
					break;
				case dfTweenLoopType.Loop:
					m_elapsedSinceLoop = 0f;
					frameIndex = 0;
					if (alternativeLoopClip != null && clip != alternativeLoopClip)
					{
						if ((float)myRandom.NextDouble() < percentChanceToPlayAlternative)
						{
							clip = alternativeLoopClip;
						}
					}
					else if (clip != m_cachedBaseClip)
					{
						clip = m_cachedBaseClip;
					}
					m_elapsedSinceLoop = 0f;
					if (LoopDelayMax > 0f)
					{
						float delay = UnityEngine.Random.Range(LoopDelayMin, LoopDelayMax);
						float ela = 0f;
						while (ela < delay)
						{
							ela += GameManager.INVARIANT_DELTA_TIME;
							yield return null;
						}
						m_elapsedSinceLoop = 0f;
						m_lastRealtime = Time.realtimeSinceStartup;
					}
					break;
				case dfTweenLoopType.PingPong:
					m_elapsedSinceLoop = 0f;
					direction *= -1;
					frameIndex = 0;
					break;
				case dfTweenLoopType.LoopSection:
					frameIndex = LoopSectionFrameTarget;
					m_elapsedSinceLoop = (float)LoopSectionFrameTarget / (float)sprites.Count * length;
					if (LoopDelayMax > 0f)
					{
						float delay2 = UnityEngine.Random.Range(LoopDelayMin, LoopDelayMax);
						float ela2 = 0f;
						while (ela2 < delay2)
						{
							ela2 += GameManager.INVARIANT_DELTA_TIME;
							yield return null;
						}
						m_elapsedSinceLoop = (float)LoopSectionFrameTarget / (float)sprites.Count * length;
						m_lastRealtime = Time.realtimeSinceStartup;
					}
					break;
				}
			}
			if (direction == -1)
			{
				frameIndex = maxFrameIndex - frameIndex;
			}
			if (lastFrameIndex != frameIndex)
			{
				lastFrameIndex = frameIndex;
				setFrame(frameIndex);
			}
			if (isRunning)
			{
				continue;
			}
			break;
		}
	}

	private string getPath(Transform obj)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (obj != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Insert(0, "\\");
				stringBuilder.Insert(0, obj.name);
			}
			else
			{
				stringBuilder.Append(obj.name);
			}
			obj = obj.parent;
		}
		return stringBuilder.ToString();
	}

	public void SetFrameExternal(int index)
	{
		setFrame(index);
	}

	private void setFrame(int frameIndex)
	{
		List<string> sprites = clip.Sprites;
		if (sprites.Count == 0)
		{
			return;
		}
		frameIndex = Mathf.Max(0, Mathf.Min(frameIndex, sprites.Count - 1));
		if (memberInfo != null)
		{
			dfSprite dfSprite2 = memberInfo.Component as dfSprite;
			if ((bool)dfSprite2)
			{
				dfSprite2.SpriteName = sprites[frameIndex];
			}
			if (m_selfControl != null)
			{
				m_selfControl.Invalidate();
			}
		}
	}
}
