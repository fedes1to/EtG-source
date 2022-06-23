using UnityEngine;

namespace DaikonForge.Tween.Interpolation
{
	public class EulerInterpolator : Interpolator<Vector3>
	{
		protected static EulerInterpolator singleton;

		public static Interpolator<Vector3> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new EulerInterpolator();
				}
				return singleton;
			}
		}

		public override Vector3 Add(Vector3 lhs, Vector3 rhs)
		{
			return lhs + rhs;
		}

		public override Vector3 Interpolate(Vector3 startValue, Vector3 endValue, float time)
		{
			float x = clerp(startValue.x, endValue.x, time);
			float y = clerp(startValue.y, endValue.y, time);
			float z = clerp(startValue.z, endValue.z, time);
			return new Vector3(x, y, z);
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
	}
}
