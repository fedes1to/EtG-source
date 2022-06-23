using System;
using UnityEngine;

public class dfEasingFunctions
{
	public delegate float EasingFunction(float start, float end, float time);

	public static EasingFunction GetFunction(dfEasingType easeType)
	{
		switch (easeType)
		{
		case dfEasingType.BackEaseIn:
			return easeInBack;
		case dfEasingType.BackEaseInOut:
			return easeInOutBack;
		case dfEasingType.BackEaseOut:
			return easeOutBack;
		case dfEasingType.Bounce:
			return bounce;
		case dfEasingType.CircEaseIn:
			return easeInCirc;
		case dfEasingType.CircEaseInOut:
			return easeInOutCirc;
		case dfEasingType.CircEaseOut:
			return easeOutCirc;
		case dfEasingType.CubicEaseIn:
			return easeInCubic;
		case dfEasingType.CubicEaseInOut:
			return easeInOutCubic;
		case dfEasingType.CubicEaseOut:
			return easeOutCubic;
		case dfEasingType.ExpoEaseIn:
			return easeInExpo;
		case dfEasingType.ExpoEaseInOut:
			return easeInOutExpo;
		case dfEasingType.ExpoEaseOut:
			return easeOutExpo;
		case dfEasingType.Linear:
			return linear;
		case dfEasingType.QuadEaseIn:
			return easeInQuad;
		case dfEasingType.QuadEaseInOut:
			return easeInOutQuad;
		case dfEasingType.QuadEaseOut:
			return easeOutQuad;
		case dfEasingType.QuartEaseIn:
			return easeInQuart;
		case dfEasingType.QuartEaseInOut:
			return easeInOutQuart;
		case dfEasingType.QuartEaseOut:
			return easeOutQuart;
		case dfEasingType.QuintEaseIn:
			return easeInQuint;
		case dfEasingType.QuintEaseInOut:
			return easeInOutQuint;
		case dfEasingType.QuintEaseOut:
			return easeOutQuint;
		case dfEasingType.SineEaseIn:
			return easeInSine;
		case dfEasingType.SineEaseInOut:
			return easeInOutSine;
		case dfEasingType.SineEaseOut:
			return easeOutSine;
		case dfEasingType.Spring:
			return spring;
		default:
			throw new NotImplementedException();
		}
	}

	private static float linear(float start, float end, float time)
	{
		return Mathf.Lerp(start, end, time);
	}

	private static float clerp(float start, float end, float time)
	{
		float num = 0f;
		float num2 = 360f;
		float num3 = Mathf.Abs((num2 - num) / 2f);
		float num4 = 0f;
		float num5 = 0f;
		if (end - start < 0f - num3)
		{
			num5 = (num2 - start + end) * time;
			return start + num5;
		}
		if (end - start > num3)
		{
			num5 = (0f - (num2 - end + start)) * time;
			return start + num5;
		}
		return start + (end - start) * time;
	}

	private static float spring(float start, float end, float time)
	{
		time = Mathf.Clamp01(time);
		time = (Mathf.Sin(time * (float)Math.PI * (0.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + 1.2f * (1f - time));
		return start + (end - start) * time;
	}

	private static float easeInQuad(float start, float end, float time)
	{
		end -= start;
		return end * time * time + start;
	}

	private static float easeOutQuad(float start, float end, float time)
	{
		end -= start;
		return (0f - end) * time * (time - 2f) + start;
	}

	private static float easeInOutQuad(float start, float end, float time)
	{
		time /= 0.5f;
		end -= start;
		if (time < 1f)
		{
			return end / 2f * time * time + start;
		}
		time -= 1f;
		return (0f - end) / 2f * (time * (time - 2f) - 1f) + start;
	}

	private static float easeInCubic(float start, float end, float time)
	{
		end -= start;
		return end * time * time * time + start;
	}

	private static float easeOutCubic(float start, float end, float time)
	{
		time -= 1f;
		end -= start;
		return end * (time * time * time + 1f) + start;
	}

	private static float easeInOutCubic(float start, float end, float time)
	{
		time /= 0.5f;
		end -= start;
		if (time < 1f)
		{
			return end / 2f * time * time * time + start;
		}
		time -= 2f;
		return end / 2f * (time * time * time + 2f) + start;
	}

	private static float easeInQuart(float start, float end, float time)
	{
		end -= start;
		return end * time * time * time * time + start;
	}

	private static float easeOutQuart(float start, float end, float time)
	{
		time -= 1f;
		end -= start;
		return (0f - end) * (time * time * time * time - 1f) + start;
	}

	private static float easeInOutQuart(float start, float end, float time)
	{
		time /= 0.5f;
		end -= start;
		if (time < 1f)
		{
			return end / 2f * time * time * time * time + start;
		}
		time -= 2f;
		return (0f - end) / 2f * (time * time * time * time - 2f) + start;
	}

	private static float easeInQuint(float start, float end, float time)
	{
		end -= start;
		return end * time * time * time * time * time + start;
	}

	private static float easeOutQuint(float start, float end, float time)
	{
		time -= 1f;
		end -= start;
		return end * (time * time * time * time * time + 1f) + start;
	}

	private static float easeInOutQuint(float start, float end, float time)
	{
		time /= 0.5f;
		end -= start;
		if (time < 1f)
		{
			return end / 2f * time * time * time * time * time + start;
		}
		time -= 2f;
		return end / 2f * (time * time * time * time * time + 2f) + start;
	}

	private static float easeInSine(float start, float end, float time)
	{
		end -= start;
		return (0f - end) * Mathf.Cos(time / 1f * ((float)Math.PI / 2f)) + end + start;
	}

	private static float easeOutSine(float start, float end, float time)
	{
		end -= start;
		return end * Mathf.Sin(time / 1f * ((float)Math.PI / 2f)) + start;
	}

	private static float easeInOutSine(float start, float end, float time)
	{
		end -= start;
		return (0f - end) / 2f * (Mathf.Cos((float)Math.PI * time / 1f) - 1f) + start;
	}

	private static float easeInExpo(float start, float end, float time)
	{
		end -= start;
		return end * Mathf.Pow(2f, 10f * (time / 1f - 1f)) + start;
	}

	private static float easeOutExpo(float start, float end, float time)
	{
		end -= start;
		return end * (0f - Mathf.Pow(2f, -10f * time / 1f) + 1f) + start;
	}

	private static float easeInOutExpo(float start, float end, float time)
	{
		time /= 0.5f;
		end -= start;
		if (time < 1f)
		{
			return end / 2f * Mathf.Pow(2f, 10f * (time - 1f)) + start;
		}
		time -= 1f;
		return end / 2f * (0f - Mathf.Pow(2f, -10f * time) + 2f) + start;
	}

	private static float easeInCirc(float start, float end, float time)
	{
		end -= start;
		return (0f - end) * (Mathf.Sqrt(1f - time * time) - 1f) + start;
	}

	private static float easeOutCirc(float start, float end, float time)
	{
		time -= 1f;
		end -= start;
		return end * Mathf.Sqrt(1f - time * time) + start;
	}

	private static float easeInOutCirc(float start, float end, float time)
	{
		time /= 0.5f;
		end -= start;
		if (time < 1f)
		{
			return (0f - end) / 2f * (Mathf.Sqrt(1f - time * time) - 1f) + start;
		}
		time -= 2f;
		return end / 2f * (Mathf.Sqrt(1f - time * time) + 1f) + start;
	}

	private static float bounce(float start, float end, float time)
	{
		time /= 1f;
		end -= start;
		if (time < 0.363636374f)
		{
			return end * (7.5625f * time * time) + start;
		}
		if (time < 0.727272749f)
		{
			time -= 0.545454562f;
			return end * (7.5625f * time * time + 0.75f) + start;
		}
		if ((double)time < 0.90909090909090906)
		{
			time -= 0.8181818f;
			return end * (7.5625f * time * time + 0.9375f) + start;
		}
		time -= 21f / 22f;
		return end * (7.5625f * time * time + 63f / 64f) + start;
	}

	private static float easeInBack(float start, float end, float time)
	{
		end -= start;
		time /= 1f;
		float num = 1.70158f;
		return end * time * time * ((num + 1f) * time - num) + start;
	}

	private static float easeOutBack(float start, float end, float time)
	{
		float num = 1.70158f;
		end -= start;
		time = time / 1f - 1f;
		return end * (time * time * ((num + 1f) * time + num) + 1f) + start;
	}

	private static float easeInOutBack(float start, float end, float time)
	{
		float num = 1.70158f;
		end -= start;
		time /= 0.5f;
		if (time < 1f)
		{
			num *= 1.525f;
			return end / 2f * (time * time * ((num + 1f) * time - num)) + start;
		}
		time -= 2f;
		num *= 1.525f;
		return end / 2f * (time * time * ((num + 1f) * time + num) + 2f) + start;
	}

	private static float punch(float amplitude, float time)
	{
		float num = 9f;
		if (time == 0f)
		{
			return 0f;
		}
		if (time == 1f)
		{
			return 0f;
		}
		float num2 = 0.3f;
		num = num2 / ((float)Math.PI * 2f) * Mathf.Asin(0f);
		return amplitude * Mathf.Pow(2f, -10f * time) * Mathf.Sin((time * 1f - num) * ((float)Math.PI * 2f) / num2);
	}
}
