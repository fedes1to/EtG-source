using System;

namespace HutongGames.PlayMaker.Actions
{
	[Serializable]
	public class MassiveSaveFlagEntry
	{
		public GungeonFlags RequiredFlag;

		public bool RequiredFlagState = true;

		public GungeonFlags CompletedFlag;

		public string mode;
	}
}
