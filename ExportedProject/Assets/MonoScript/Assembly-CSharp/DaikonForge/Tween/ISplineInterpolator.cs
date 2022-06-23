using UnityEngine;

namespace DaikonForge.Tween
{
	public interface ISplineInterpolator
	{
		Vector3 Evaluate(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t);
	}
}
