using UnityEngine;

namespace DaikonForge.Tween.Interpolation
{
	public class IntInterpolator : Interpolator<int>
	{
		protected static IntInterpolator singleton;

		public static Interpolator<int> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new IntInterpolator();
				}
				return singleton;
			}
		}

		public override int Add(int lhs, int rhs)
		{
			return lhs + rhs;
		}

		public override int Interpolate(int startValue, int endValue, float time)
		{
			return Mathf.RoundToInt((float)startValue + (float)(endValue - startValue) * time);
		}
	}
}
