using System;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class DebugDungeonSettings
	{
		public bool RAPID_DEBUG_DUNGEON_ITERATION_SEEKER;

		public bool RAPID_DEBUG_DUNGEON_ITERATION;

		public int RAPID_DEBUG_DUNGEON_COUNT = 50;

		public bool GENERATION_VIEWER_MODE;

		public bool FULL_MINIMAP_VISIBILITY;

		public bool COOP_TEST;

		[Header("Generation Options")]
		public bool DISABLE_ENEMIES;

		public bool DISABLE_LOOPS;

		public bool DISABLE_SECRET_ROOM_COVERS;

		public bool DISABLE_OUTLINES;

		public bool WALLS_ARE_PITS;
	}
}
