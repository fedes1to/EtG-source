using System;

[Serializable]
public class OverrideBossFloorEntry
{
	public string Annotation;

	[EnumFlags]
	public GlobalDungeonData.ValidTilesets AssociatedTilesets;

	public DungeonPrerequisite[] GlobalBossPrerequisites;

	public float ChanceToOverride = 0.01f;

	public GenericRoomTable TargetRoomTable;

	public bool GlobalPrereqsValid(GlobalDungeonData.ValidTilesets targetTileset)
	{
		if ((AssociatedTilesets | targetTileset) != AssociatedTilesets)
		{
			return false;
		}
		for (int i = 0; i < GlobalBossPrerequisites.Length; i++)
		{
			if (!GlobalBossPrerequisites[i].CheckConditionsFulfilled())
			{
				return false;
			}
		}
		return true;
	}
}
