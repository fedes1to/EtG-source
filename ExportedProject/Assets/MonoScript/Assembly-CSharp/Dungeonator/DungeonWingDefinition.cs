using System;

namespace Dungeonator
{
	[Serializable]
	public class DungeonWingDefinition
	{
		public WeightedIntCollection includedMaterialIndices;

		public float weight = 1f;

		public bool canBeCriticalPath;

		public bool canBeNoncriticalPath;
	}
}
