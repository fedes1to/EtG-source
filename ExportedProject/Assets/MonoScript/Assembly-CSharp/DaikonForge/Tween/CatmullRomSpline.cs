using UnityEngine;

namespace DaikonForge.Tween
{
	public class CatmullRomSpline : ISplineInterpolator
	{
		public Vector3 Evaluate(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			return 0.5f * (2f * b + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t * t + (-a + 3f * b - 3f * c + d) * t * t * t);
		}
	}
}
