using UnityEngine;

namespace DaikonForge.Tween.Interpolation
{
	public class Vector2Interpolator : Interpolator<Vector2>
	{
		protected static Vector2Interpolator singleton;

		public static Interpolator<Vector2> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new Vector2Interpolator();
				}
				return singleton;
			}
		}

		public override Vector2 Add(Vector2 lhs, Vector2 rhs)
		{
			return lhs + rhs;
		}

		public override Vector2 Interpolate(Vector2 startValue, Vector2 endValue, float time)
		{
			return startValue + (endValue - startValue) * time;
		}
	}
}
