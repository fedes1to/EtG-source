using UnityEngine;

namespace DaikonForge.Tween
{
	public interface IPathIterator
	{
		Vector3 GetPosition(float time);

		Vector3 GetTangent(float time);
	}
}
