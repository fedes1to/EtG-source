using System;

namespace HutongGames.PlayMaker.Actions
{
	[Serializable]
	public class DesiredItem
	{
		public enum DetectType
		{
			SPECIFIC_ITEM,
			CURRENCY,
			META_CURRENCY,
			KEYS
		}

		public GungeonFlags flagToSet;

		public DetectType type;

		public int specificItemId;

		public int amount;
	}
}
