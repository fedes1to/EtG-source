using System;

namespace Dungeonator
{
	[Serializable]
	public class Subrule
	{
		public string ruleName = "Generic";

		public RoomCreationRule.PlacementStrategy placementRule;

		public int minToSpawn = 1;

		public int maxToSpawn = 1;

		public DungeonPlaceable placeableObject;
	}
}
