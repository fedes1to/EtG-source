using UnityEngine;

namespace DaikonForge.Tween.Interpolation
{
	public class Vector4Interpolator : Interpolator<Vector4>
	{
		protected static Vector4Interpolator singleton;

		public static Interpolator<Vector4> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new Vector4Interpolator();
				}
				return singleton;
			}
		}

		public override Vector4 Add(Vector4 lhs, Vector4 rhs)
		{
			return lhs + rhs;
		}

		public override Vector4 Interpolate(Vector4 startValue, Vector4 endValue, float time)
		{
			return startValue + endValue * time;
		}
	}
}
