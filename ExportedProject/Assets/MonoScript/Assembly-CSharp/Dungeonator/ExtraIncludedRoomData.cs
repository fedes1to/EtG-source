using System;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class ExtraIncludedRoomData
	{
		[SerializeField]
		public PrototypeDungeonRoom room;

		[NonSerialized]
		public bool hasBeenProcessed;
	}
}
