using System;
using System.Collections.Generic;

namespace Dungeonator
{
	[Serializable]
	public class RoomCreationRule
	{
		public enum PlacementStrategy
		{
			CENTERPIECE,
			CORNERS,
			WALLS,
			BACK_WALL,
			RANDOM_CENTER,
			RANDOM
		}

		public float percentChance;

		public List<Subrule> subrules;

		public RoomCreationRule()
		{
			subrules = new List<Subrule>();
		}
	}
}
