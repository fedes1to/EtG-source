using System;

namespace Dungeonator
{
	[Serializable]
	public class DungeonPlaceableRoomMaterialRequirement
	{
		public GlobalDungeonData.ValidTilesets TargetTileset = GlobalDungeonData.ValidTilesets.CASTLEGEON;

		[PrettyDungeonMaterial("TargetTileset")]
		public int RoomMaterial;

		public bool RequireMaterial = true;
	}
}
