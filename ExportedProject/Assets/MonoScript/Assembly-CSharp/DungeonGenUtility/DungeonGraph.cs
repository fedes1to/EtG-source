using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenUtility
{
	public class DungeonGraph
	{
		public List<Vector2> vertices;

		public List<Edge> edges;

		public DungeonGraph()
		{
			vertices = new List<Vector2>();
			edges = new List<Edge>();
		}
	}
}
