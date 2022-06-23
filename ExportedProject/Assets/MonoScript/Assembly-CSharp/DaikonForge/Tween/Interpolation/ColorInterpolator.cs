using UnityEngine;

namespace DaikonForge.Tween.Interpolation
{
	public class ColorInterpolator : Interpolator<Color>
	{
		protected static ColorInterpolator singleton;

		public static Interpolator<Color> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new ColorInterpolator();
				}
				return singleton;
			}
		}

		public override Color Add(Color lhs, Color rhs)
		{
			return lhs + rhs;
		}

		public override Color Interpolate(Color startValue, Color endValue, float time)
		{
			return Color.Lerp(startValue, endValue, time);
		}
	}
}
