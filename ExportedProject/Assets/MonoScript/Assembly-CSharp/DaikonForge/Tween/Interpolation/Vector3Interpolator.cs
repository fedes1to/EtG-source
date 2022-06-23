using UnityEngine;

namespace DaikonForge.Tween.Interpolation
{
	public class Vector3Interpolator : Interpolator<Vector3>
	{
		protected static Vector3Interpolator singleton;

		public static Interpolator<Vector3> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new Vector3Interpolator();
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
			return startValue + (endValue - startValue) * time;
		}
	}
}
