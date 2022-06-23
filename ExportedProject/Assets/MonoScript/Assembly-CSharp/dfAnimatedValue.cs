using UnityEngine;

public abstract class dfAnimatedValue<T> where T : struct
{
	private T startValue;

	private T endValue;

	private float animLength = 1f;

	private float startTime;

	private bool isDone;

	private dfEasingType easingType;

	private dfEasingFunctions.EasingFunction easingFunction;

	public bool IsDone
	{
		get
		{
			return isDone;
		}
	}

	public float Length
	{
		get
		{
			return animLength;
		}
		set
		{
			animLength = value;
			startTime = Time.realtimeSinceStartup;
			isDone = false;
		}
	}

	public T StartValue
	{
		get
		{
			return startValue;
		}
		set
		{
			startValue = value;
			startTime = Time.realtimeSinceStartup;
			isDone = false;
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
			startTime = Time.realtimeSinceStartup;
			isDone = false;
		}
	}

	public T Value
	{
		get
		{
			float num = Time.realtimeSinceStartup - startTime;
			if (num >= animLength)
			{
				isDone = true;
				return endValue;
			}
			float time = Mathf.Clamp01(num / animLength);
			time = easingFunction(0f, 1f, time);
			return Lerp(startValue, endValue, time);
		}
	}

	public dfEasingType EasingType
	{
		get
		{
			return easingType;
		}
		set
		{
			easingType = value;
			easingFunction = dfEasingFunctions.GetFunction(easingType);
		}
	}

	protected internal dfAnimatedValue(T StartValue, T EndValue, float Time)
		: this()
	{
		startValue = StartValue;
		endValue = EndValue;
		animLength = Time;
	}

	protected internal dfAnimatedValue()
	{
		startTime = Time.realtimeSinceStartup;
		easingFunction = dfEasingFunctions.GetFunction(easingType);
	}

	protected abstract T Lerp(T start, T end, float time);

	public static implicit operator T(dfAnimatedValue<T> animated)
	{
		return animated.Value;
	}
}
