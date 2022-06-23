using System;

namespace Dungeonator
{
	[Serializable]
	public struct CellDamageDefinition
	{
		public CoreDamageTypes damageTypes;

		public float damageToPlayersPerTick;

		public float damageToEnemiesPerTick;

		public float tickFrequency;

		public bool respectsFlying;

		public bool isPoison;

		public bool HasChanges()
		{
			return damageTypes != 0 || damageToPlayersPerTick != 0f || damageToEnemiesPerTick != 0f || tickFrequency != 0f || respectsFlying || isPoison;
		}
	}
}
