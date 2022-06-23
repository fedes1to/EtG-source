using System;
using System.Collections.Generic;

namespace Dungeonator
{
	[Serializable]
	public class RoomCreationStrategy
	{
		public enum RoomType
		{
			SMOOTH_RECTILINEAR_ROOM,
			JAGGED_RECTILINEAR_ROOM,
			CIRCULAR_ROOM,
			SMOOTH_ANNEX,
			JAGGED_ANNEX,
			CAVE_ROOM,
			PREDEFINED_ROOM
		}

		public int minAreaSize;

		public int maxAreaSize = 2;

		public RoomType roomType;

		public List<RoomCreationRule> rules;

		public RoomCreationStrategy()
		{
			rules = new List<RoomCreationRule>();
		}
	}
}
