using System;

[Serializable]
public class DungeonFlowSubtypeRestriction
{
	public PrototypeDungeonRoom.RoomCategory baseCategoryRestriction = PrototypeDungeonRoom.RoomCategory.NORMAL;

	public PrototypeDungeonRoom.RoomNormalSubCategory normalSubcategoryRestriction;

	public PrototypeDungeonRoom.RoomBossSubCategory bossSubcategoryRestriction;

	public PrototypeDungeonRoom.RoomSpecialSubCategory specialSubcategoryRestriction;

	public PrototypeDungeonRoom.RoomSecretSubCategory secretSubcategoryRestriction;

	public int maximumRoomsOfSubtype = 1;
}
