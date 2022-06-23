using UnityEngine;

namespace BraveDynamicTree
{
	public struct b2RayCastInput
	{
		public Vector2 p1;

		public Vector2 p2;

		public float maxFraction;

		public b2RayCastInput(Vector2 p1, Vector2 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
			maxFraction = 1f;
		}
	}
}
