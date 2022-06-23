using System;

namespace Dungeonator
{
	[Serializable]
	public class MetaInjectionUnlockedChanceEntry
	{
		public DungeonPrerequisite[] prerequisites;

		public float ChanceToTrigger = 1f;
	}
}
