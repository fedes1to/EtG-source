using UnityEngine;

namespace DungeonGenUtility
{
	public class Edge
	{
		private Vector2 v0;

		private Vector2 v1;

		private float Length
		{
			get
			{
				return Vector2.Distance(v0, v1);
			}
		}

		public Edge(Vector2 vert0, Vector2 vert1)
		{
			v0 = vert0;
			v1 = vert1;
		}
	}
}
