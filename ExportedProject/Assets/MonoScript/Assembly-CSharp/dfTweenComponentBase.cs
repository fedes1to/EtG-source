using System;
using UnityEngine;

[Serializable]
public abstract class dfTweenComponentBase : dfTweenPlayableBase
{
	[SerializeField]
	protected string tweenName = string.Empty;

	[SerializeField]
	protected dfComponentMemberInfo target;

	[SerializeField]
	protected dfEasingType easingType;

	[SerializeField]
	protected AnimationCurve animCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));

	[SerializeField]
	protected float length = 1f;

	[SerializeField]
	protected bool syncStartWhenRun;

	[SerializeField]
	protected bool startValueIsOffset;

	[SerializeField]
	protected bool syncEndWhenRun;

	[SerializeField]
	protected bool endValueIsOffset;

	[SerializeField]
	protected dfTweenLoopType loopType;

	[SerializeField]
	protected bool autoRun;

	[SerializeField]
	protected bool skipToEndOnStop;

	[SerializeField]
	protected float delayBeforeStarting;

	protected dfTweenState state;

	protected dfEasingFunctions.EasingFunction easingFunction;

	protected dfObservableProperty boundProperty;

	protected bool wasAutoStarted;

	public override string TweenName
	{
		get
		{
			if (tweenName == null)
			{
				tweenName = base.ToString();
			}
			return tweenName;
		}
		set
		{
			tweenName = value;
		}
	}

	public dfComponentMemberInfo Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
		}
	}

	public AnimationCurve AnimationCurve
	{
		get
		{
			return animCurve;
		}
		set
		{
			animCurve = value;
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
			length = Mathf.Max(0f, value);
		}
	}

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

	public dfEasingType Function
	{
		get
		{
			return easingType;
		}
		set
		{
			easingType = value;
			if (state != 0)
			{
				Stop();
				Play();
			}
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
			if (state != 0)
			{
				Stop();
				Play();
			}
		}
	}

	public bool SyncStartValueWhenRun
	{
		get
		{
			return syncStartWhenRun;
		}
		set
		{
			syncStartWhenRun = value;
		}
	}

	public bool StartValueIsOffset
	{
		get
		{
			return startValueIsOffset;
		}
		set
		{
			startValueIsOffset = value;
		}
	}

	public bool SyncEndValueWhenRun
	{
		get
		{
			return syncEndWhenRun;
		}
		set
		{
			syncEndWhenRun = value;
		}
	}

	public bool EndValueIsOffset
	{
		get
		{
			return endValueIsOffset;
		}
		set
		{
			endValueIsOffset = value;
		}
	}

	public bool AutoRun
	{
		get
		{
			return autoRun;
		}
		set
		{
			autoRun = value;
		}
	}

	public override bool IsPlaying
	{
		get
		{
			return base.enabled && state != dfTweenState.Stopped;
		}
	}

	public bool IsPaused
	{
		get
		{
			return state == dfTweenState.Paused;
		}
		set
		{
			bool flag = state == dfTweenState.Paused;
			if (value != flag && state != 0)
			{
				state = (value ? dfTweenState.Paused : dfTweenState.Playing);
				if (value)
				{
					onPaused();
				}
				else
				{
					onResumed();
				}
			}
		}
	}

	protected internal abstract void onPaused();

	protected internal abstract void onResumed();

	protected internal abstract void onStarted();

	protected internal abstract void onStopped();

	protected internal abstract void onReset();

	protected internal abstract void onCompleted();

	public void Start()
	{
		if (autoRun && !wasAutoStarted)
		{
			wasAutoStarted = true;
			Play();
		}
	}

	public void OnDisable()
	{
		Stop();
		wasAutoStarted = false;
	}

	public override string ToString()
	{
		if (Target != null && Target.IsValid)
		{
			string arg = target.Component.name;
			return string.Format("{0} ({1}.{2})", TweenName, arg, target.MemberName);
		}
		return TweenName;
	}
}
