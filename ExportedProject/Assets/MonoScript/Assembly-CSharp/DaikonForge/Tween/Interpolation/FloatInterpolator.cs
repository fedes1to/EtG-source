namespace DaikonForge.Tween.Interpolation
{
	public class FloatInterpolator : Interpolator<float>
	{
		protected static FloatInterpolator singleton;

		public static Interpolator<float> Default
		{
			get
			{
				if (singleton == null)
				{
					singleton = new FloatInterpolator();
				}
				return singleton;
			}
		}

		public override float Add(float lhs, float rhs)
		{
			return lhs + rhs;
		}

		public override float Interpolate(float startValue, float endValue, float time)
		{
			return startValue + (endValue - startValue) * time;
		}
	}
}
