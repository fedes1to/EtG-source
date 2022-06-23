using Beebyte.Obfuscator;

namespace DaikonForge.Tween.Interpolation
{
	public abstract class Interpolator<T>
	{
		[Skip]
		public abstract T Add(T lhs, T rhs);

		[Skip]
		public abstract T Interpolate(T startValue, T endValue, float time);
	}
}
