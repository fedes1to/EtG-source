using UnityEngine;

namespace DaikonForge.Tween.Interpolation
{
	public class RectInterpolator : Interpolator<Rect>
	{
		protected static RectInterpolator singleton;

		public static Interpolator<Rect> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new RectInterpolator();
				}
				return singleton;
			}
		}

		public override Rect Add(Rect lhs, Rect rhs)
		{
			return new Rect(lhs.xMin + rhs.xMin, lhs.yMin + rhs.yMin, lhs.width + rhs.width, lhs.height + rhs.height);
		}

		public override Rect Interpolate(Rect startValue, Rect endValue, float time)
		{
			float x = startValue.xMin + (endValue.xMin - startValue.xMin) * time;
			float y = startValue.yMin + (endValue.yMin - startValue.yMin) * time;
			float width = startValue.width + (endValue.width - startValue.width) * time;
			float height = startValue.height + (endValue.height - startValue.height) * time;
			return new Rect(x, y, width, height);
		}
	}
}
