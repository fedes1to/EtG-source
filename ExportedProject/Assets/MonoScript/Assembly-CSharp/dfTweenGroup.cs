using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Tweens/Group")]
public class dfTweenGroup : dfTweenPlayableBase
{
	public enum TweenGroupMode
	{
		Concurrent,
		Sequence
	}

	[SerializeField]
	protected string groupName = string.Empty;

	[SerializeField]
	protected bool autoStart;

	[SerializeField]
	protected float delayBeforeStarting;

	public List<dfTweenPlayableBase> Tweens = new List<dfTweenPlayableBase>();

	public TweenGroupMode Mode;

	public float StartDelay
	{
		get
		{
			return delayBeforeStarting;
		}
		set
		{
			delayBeforeStarting = value;
		}
	}

	public bool AutoStart
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

	public override string TweenName
	{
		get
		{
			return groupName;
		}
		set
		{
			groupName = value;
		}
	}

	public override bool IsPlaying
	{
		get
		{
			for (int i = 0; i < Tweens.Count; i++)
			{
				if (!(Tweens[i] == null) && Tweens[i].enabled && Tweens[i].IsPlaying)
				{
					return true;
				}
			}
			return false;
		}
	}

	public event TweenNotification TweenStarted;

	public event TweenNotification TweenStopped;

	public event TweenNotification TweenReset;

	public event TweenNotification TweenCompleted;

	public void Start()
	{
		if (AutoStart && !IsPlaying)
		{
			Play();
		}
	}

	public void EnableTween(string TweenName)
	{
		for (int i = 0; i < Tweens.Count; i++)
		{
			if (!(Tweens[i] == null) && Tweens[i].TweenName == TweenName)
			{
				Tweens[i].enabled = true;
				break;
			}
		}
	}

	public void DisableTween(string TweenName)
	{
		for (int i = 0; i < Tweens.Count; i++)
		{
			if (!(Tweens[i] == null) && Tweens[i].name == TweenName)
			{
				Tweens[i].enabled = false;
				break;
			}
		}
	}

	public override void Play()
	{
		if (IsPlaying)
		{
			Stop();
		}
		onStarted();
		if (Mode == TweenGroupMode.Concurrent)
		{
			StartCoroutine(runConcurrent());
		}
		else
		{
			StartCoroutine(runSequence());
		}
	}

	public override void Stop()
	{
		if (!IsPlaying)
		{
			return;
		}
		StopAllCoroutines();
		for (int i = 0; i < Tweens.Count; i++)
		{
			if (!(Tweens[i] == null))
			{
				Tweens[i].Stop();
			}
		}
		onStopped();
	}

	public override void Reset()
	{
		if (!IsPlaying)
		{
			return;
		}
		StopAllCoroutines();
		for (int i = 0; i < Tweens.Count; i++)
		{
			if (!(Tweens[i] == null))
			{
				Tweens[i].Reset();
			}
		}
		onReset();
	}

	[HideInInspector]
	private IEnumerator runSequence()
	{
		if (delayBeforeStarting > 0f)
		{
			float timeout = Time.realtimeSinceStartup + delayBeforeStarting;
			while (Time.realtimeSinceStartup < timeout)
			{
				yield return null;
			}
		}
		for (int i = 0; i < Tweens.Count; i++)
		{
			if (!(Tweens[i] == null) && Tweens[i].enabled)
			{
				dfTweenPlayableBase tween = Tweens[i];
				tween.Play();
				while (tween.IsPlaying)
				{
					yield return null;
				}
			}
		}
		onCompleted();
	}

	[HideInInspector]
	private IEnumerator runConcurrent()
	{
		if (delayBeforeStarting > 0f)
		{
			float timeout = Time.realtimeSinceStartup + delayBeforeStarting;
			while (Time.realtimeSinceStartup < timeout)
			{
				yield return null;
			}
		}
		for (int i = 0; i < Tweens.Count; i++)
		{
			if (!(Tweens[i] == null) && Tweens[i].enabled)
			{
				Tweens[i].Play();
			}
		}
		do
		{
			yield return null;
		}
		while (Tweens.Any((dfTweenPlayableBase tween) => tween != null && tween.IsPlaying));
		onCompleted();
	}

	protected internal void onStarted()
	{
		SendMessage("TweenStarted", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenStarted != null)
		{
			this.TweenStarted(this);
		}
	}

	protected internal void onStopped()
	{
		SendMessage("TweenStopped", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenStopped != null)
		{
			this.TweenStopped(this);
		}
	}

	protected internal void onReset()
	{
		SendMessage("TweenReset", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenReset != null)
		{
			this.TweenReset(this);
		}
	}

	protected internal void onCompleted()
	{
		SendMessage("TweenCompleted", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenCompleted != null)
		{
			this.TweenCompleted(this);
		}
	}
}
