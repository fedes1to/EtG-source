using System;
using UnityEngine;

namespace DaikonForge.Tween
{
	public class TweenEasingFunctions
	{
		public static float Linear(float t)
		{
			return t;
		}

		public static float Spring(float t)
		{
			float num = Mathf.Clamp01(t);
			return (Mathf.Sin(num * (float)Math.PI * (0.2f + 2.5f * num * num * num)) * Mathf.Pow(1f - num, 2.2f) + num) * (1f + 1.2f * (1f - num));
		}

		public static float EaseInQuad(float t)
		{
			return t * t;
		}

		public static float EaseOutQuad(float t)
		{
			return -1f * t * (t - 2f);
		}

		public static float EaseInOutQuad(float t)
		{
			t /= 0.5f;
			if (t < 1f)
			{
				return 0.5f * t * t;
			}
			t -= 1f;
			return -0.5f * (t * (t - 2f) - 1f);
		}

		public static float EaseInCubic(float t)
		{
			return t * t * t;
		}

		public static float EaseOutCubic(float t)
		{
			t -= 1f;
			return t * t * t + 1f;
		}

		public static float EaseInOutCubic(float t)
		{
			t /= 0.5f;
			if (t < 1f)
			{
				return 0.5f * t * t * t;
			}
			t -= 2f;
			return 0.5f * (t * t * t + 2f);
		}

		public static float EaseInQuart(float t)
		{
			return t * t * t * t;
		}

		public static float EaseOutQuart(float t)
		{
			t -= 1f;
			return -1f * (t * t * t * t - 1f);
		}

		public static float EaseInOutQuart(float t)
		{
			t /= 0.5f;
			if (t < 1f)
			{
				return 0.5f * t * t * t * t;
			}
			t -= 2f;
			return -0.5f * (t * t * t * t - 2f);
		}

		public static float EaseInQuint(float t)
		{
			return t * t * t * t * t;
		}

		public static float EaseOutQuint(float t)
		{
			t -= 1f;
			return t * t * t * t * t + 1f;
		}

		public static float EaseInOutQuint(float t)
		{
			t /= 0.5f;
			if (t < 1f)
			{
				return 0.5f * t * t * t * t * t;
			}
			t -= 2f;
			return 0.5f * (t * t * t * t * t + 2f);
		}

		public static float EaseInSine(float t)
		{
			return -1f * Mathf.Cos(t / 1f * ((float)Math.PI / 2f)) + 1f;
		}

		public static float EaseOutSine(float t)
		{
			return Mathf.Sin(t / 1f * ((float)Math.PI / 2f));
		}

		public static float EaseInOutSine(float t)
		{
			return -0.5f * (Mathf.Cos((float)Math.PI * t / 1f) - 1f);
		}

		public static float EaseInExpo(float t)
		{
			return Mathf.Pow(2f, 10f * (t / 1f - 1f));
		}

		public static float EaseOutExpo(float t)
		{
			return 0f - Mathf.Pow(2f, -10f * t / 1f) + 1f;
		}

		public static float EaseInOutExpo(float t)
		{
			t /= 0.5f;
			if (t < 1f)
			{
				return 0.5f * Mathf.Pow(2f, 10f * (t - 1f));
			}
			t -= 1f;
			return 0.5f * (0f - Mathf.Pow(2f, -10f * t) + 2f);
		}

		public static float EaseInCirc(float t)
		{
			return -1f * (Mathf.Sqrt(1f - t * t) - 1f);
		}

		public static float EaseOutCirc(float t)
		{
			t -= 1f;
			return Mathf.Sqrt(1f - t * t);
		}

		public static float EaseInOutCirc(float t)
		{
			t /= 0.5f;
			if (t < 1f)
			{
				return -0.5f * (Mathf.Sqrt(1f - t * t) - 1f);
			}
			t -= 2f;
			return 0.5f * (Mathf.Sqrt(1f - t * t) + 1f);
		}

		public static float EaseInBack(float t)
		{
			t /= 1f;
			float num = 1.70158f;
			return t * t * ((num + 1f) * t - num);
		}

		public static float EaseOutBack(float t)
		{
			float num = 1.70158f;
			t = t / 1f - 1f;
			return t * t * ((num + 1f) * t + num) + 1f;
		}

		public static float EaseInOutBack(float t)
		{
			float num = 1.70158f;
			t /= 0.5f;
			if (t < 1f)
			{
				num *= 1.525f;
				return 0.5f * (t * t * ((num + 1f) * t - num));
			}
			t -= 2f;
			num *= 1.525f;
			return 0.5f * (t * t * ((num + 1f) * t + num) + 2f);
		}

		public static float Bounce(float t)
		{
			t /= 1f;
			if (t < 0.363636374f)
			{
				return 7.5625f * t * t;
			}
			if (t < 0.727272749f)
			{
				t -= 0.545454562f;
				return 7.5625f * t * t + 0.75f;
			}
			if ((double)t < 0.90909090909090906)
			{
				t -= 0.8181818f;
				return 7.5625f * t * t + 0.9375f;
			}
			t -= 21f / 22f;
			return 7.5625f * t * t + 63f / 64f;
		}

		public static float Punch(float t)
		{
			float num = 9f;
			if (t == 0f)
			{
				return 0f;
			}
			if (t == 1f)
			{
				return 0f;
			}
			float num2 = 0.3f;
			num = num2 / ((float)Math.PI * 2f) * Mathf.Asin(0f);
			return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 1f - num) * ((float)Math.PI * 2f) / num2);
		}

		public static TweenEasingCallback GetFunction(EasingType easeType)
		{
			switch (easeType)
			{
			case EasingType.BackEaseIn:
				return EaseInBack;
			case EasingType.BackEaseInOut:
				return EaseInOutBack;
			case EasingType.BackEaseOut:
				return EaseOutBack;
			case EasingType.Bounce:
				return Bounce;
			case EasingType.CircEaseIn:
				return EaseInCirc;
			case EasingType.CircEaseInOut:
				return EaseInOutCirc;
			case EasingType.CircEaseOut:
				return EaseOutCirc;
			case EasingType.CubicEaseIn:
				return EaseInCubic;
			case EasingType.CubicEaseInOut:
				return EaseInOutCubic;
			case EasingType.CubicEaseOut:
				return EaseOutCubic;
			case EasingType.ExpoEaseIn:
				return EaseInExpo;
			case EasingType.ExpoEaseInOut:
				return EaseInOutExpo;
			case EasingType.ExpoEaseOut:
				return EaseOutExpo;
			case EasingType.Linear:
				return Linear;
			case EasingType.QuadEaseIn:
				return EaseInQuad;
			case EasingType.QuadEaseInOut:
				return EaseInOutQuad;
			case EasingType.QuadEaseOut:
				return EaseOutQuad;
			case EasingType.QuartEaseIn:
				return EaseInQuart;
			case EasingType.QuartEaseInOut:
				return EaseInOutQuart;
			case EasingType.QuartEaseOut:
				return EaseOutQuart;
			case EasingType.QuintEaseIn:
				return EaseInQuint;
			case EasingType.QuintEaseInOut:
				return EaseInOutQuint;
			case EasingType.QuintEaseOut:
				return EaseOutQuint;
			case EasingType.SineEaseIn:
				return EaseInSine;
			case EasingType.SineEaseInOut:
				return EaseInOutSine;
			case EasingType.SineEaseOut:
				return EaseOutSine;
			case EasingType.Spring:
				return Spring;
			default:
				throw new NotImplementedException();
			}
		}
	}
}
