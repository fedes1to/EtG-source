using System;
using System.Text;
using UnityEngine;

[Serializable]
public abstract class dfTweenComponent<T> : dfTweenComponentBase where T : struct
{
	[SerializeField]
	protected T startValue;

	[SerializeField]
	protected T endValue;

	[SerializeField]
	protected dfPlayDirection direction;

	private T actualStartValue;

	private T actualEndValue;

	private float startTime;

	private float pingPongDirection;

	public T StartValue
	{
		get
		{
			return startValue;
		}
		set
		{
			startValue = value;
			if (state != 0)
			{
				Stop();
				Play();
			}
		}
	}

	public T EndValue
	{
		get
		{
			return endValue;
		}
		set
		{
			endValue = value;
			if (state != 0)
			{
				Stop();
				Play();
			}
		}
	}

	public dfTweenState State
	{
		get
		{
			return state;
		}
	}

	public event TweenNotification TweenStarted;

	public event TweenNotification TweenStopped;

	public event TweenNotification TweenPaused;

	public event TweenNotification TweenResumed;

	public event TweenNotification TweenReset;

	public event TweenNotification TweenCompleted;

	public static dfTweenComponent<T> Create(Component target, string propertyName, T startValue, T endValue, float length)
	{
		return Create(target, propertyName, startValue, endValue, length, dfEasingType.Linear);
	}

	public static dfTweenComponent<T> Create(Component target, string propertyName, T startValue, T endValue, float length, dfEasingType func)
	{
		if (target == null || target.gameObject == null)
		{
			throw new ArgumentNullException("target");
		}
		if (string.IsNullOrEmpty(propertyName))
		{
			throw new ArgumentNullException("propertyName");
		}
		dfTweenComponent<T> dfTweenComponent2 = (dfTweenComponent<T>)target.gameObject.AddComponent(typeof(T));
		dfTweenComponent2.autoRun = false;
		dfTweenComponent2.target = new dfComponentMemberInfo
		{
			Component = target,
			MemberName = propertyName
		};
		dfTweenComponent2.startValue = startValue;
		dfTweenComponent2.endValue = endValue;
		dfTweenComponent2.length = length;
		dfTweenComponent2.easingType = func;
		return dfTweenComponent2;
	}

	public override void Play()
	{
		if (state != 0)
		{
			Stop();
		}
		if (base.enabled && base.gameObject.activeSelf && base.gameObject.activeInHierarchy)
		{
			if (target == null)
			{
				throw new NullReferenceException("Tween target is NULL");
			}
			if (!target.IsValid)
			{
				throw new InvalidOperationException("Invalid property binding configuration on " + getPath(base.gameObject.transform) + " - " + target);
			}
			boundProperty = target.GetProperty();
			easingFunction = dfEasingFunctions.GetFunction(easingType);
			onStarted();
			actualStartValue = startValue;
			actualEndValue = endValue;
			if (syncStartWhenRun)
			{
				actualStartValue = (T)boundProperty.Value;
			}
			else if (startValueIsOffset)
			{
				actualStartValue = offset(startValue, (T)boundProperty.Value);
			}
			if (syncEndWhenRun)
			{
				actualEndValue = (T)boundProperty.Value;
			}
			else if (endValueIsOffset)
			{
				actualEndValue = offset(endValue, (T)boundProperty.Value);
			}
			boundProperty.Value = actualStartValue;
			startTime = Time.realtimeSinceStartup;
			state = dfTweenState.Started;
		}
	}

	public override void Stop()
	{
		if (state != 0)
		{
			if (skipToEndOnStop)
			{
				boundProperty.Value = actualEndValue;
			}
			state = dfTweenState.Stopped;
			onStopped();
			easingFunction = null;
			boundProperty = null;
		}
	}

	public override void Reset()
	{
		if (boundProperty != null)
		{
			boundProperty.Value = actualStartValue;
		}
		state = dfTweenState.Stopped;
		onReset();
		easingFunction = null;
		boundProperty = null;
	}

	public void Pause()
	{
		base.IsPaused = true;
	}

	public void Resume()
	{
		base.IsPaused = false;
	}

	public void Update()
	{
		if (state == dfTweenState.Stopped || state == dfTweenState.Paused)
		{
			return;
		}
		if (state == dfTweenState.Started)
		{
			if (startTime + base.StartDelay >= Time.realtimeSinceStartup)
			{
				return;
			}
			state = dfTweenState.Playing;
			startTime = Time.realtimeSinceStartup;
			pingPongDirection = 0f;
		}
		float num = Mathf.Min(Time.realtimeSinceStartup - startTime, length);
		if (num >= length)
		{
			if (loopType == dfTweenLoopType.Once)
			{
				boundProperty.Value = actualEndValue;
				Stop();
				onCompleted();
				return;
			}
			if (loopType == dfTweenLoopType.Loop)
			{
				startTime = Time.realtimeSinceStartup;
				return;
			}
			if (loopType != dfTweenLoopType.PingPong)
			{
				throw new NotImplementedException();
			}
			startTime = Time.realtimeSinceStartup;
			if (pingPongDirection == 0f)
			{
				pingPongDirection = 1f;
			}
			else
			{
				pingPongDirection = 0f;
			}
		}
		else
		{
			float time = easingFunction(0f, 1f, Mathf.Abs(pingPongDirection - num / length));
			if (animCurve != null)
			{
				time = animCurve.Evaluate(time);
			}
			boundProperty.Value = evaluate(actualStartValue, actualEndValue, time);
		}
	}

	public abstract T evaluate(T startValue, T endValue, float time);

	public abstract T offset(T value, T offset);

	public override string ToString()
	{
		if (base.Target != null && base.Target.IsValid)
		{
			string arg = target.Component.name;
			return string.Format("{0} ({1}.{2})", TweenName, arg, target.MemberName);
		}
		return TweenName;
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

	protected internal static float Lerp(float startValue, float endValue, float time)
	{
		return startValue + (endValue - startValue) * time;
	}

	protected internal override void onPaused()
	{
		SendMessage("TweenPaused", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenPaused != null)
		{
			this.TweenPaused(this);
		}
	}

	protected internal override void onResumed()
	{
		SendMessage("TweenResumed", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenResumed != null)
		{
			this.TweenResumed(this);
		}
	}

	protected internal override void onStarted()
	{
		SendMessage("TweenStarted", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenStarted != null)
		{
			this.TweenStarted(this);
		}
	}

	protected internal override void onStopped()
	{
		SendMessage("TweenStopped", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenStopped != null)
		{
			this.TweenStopped(this);
		}
	}

	protected internal override void onReset()
	{
		SendMessage("TweenReset", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenReset != null)
		{
			this.TweenReset(this);
		}
	}

	protected internal override void onCompleted()
	{
		SendMessage("TweenCompleted", this, SendMessageOptions.DontRequireReceiver);
		if (this.TweenCompleted != null)
		{
			this.TweenCompleted(this);
		}
	}
}
