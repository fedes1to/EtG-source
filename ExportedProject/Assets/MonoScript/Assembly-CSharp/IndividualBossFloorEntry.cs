using System;

[Serializable]
public class IndividualBossFloorEntry
{
	public DungeonPrerequisite[] GlobalBossPrerequisites;

	public float BossWeight = 1f;

	public GenericRoomTable TargetRoomTable;

	public float GetWeightModifier()
	{
		int num = 0;
		for (int i = 0; i < TargetRoomTable.includedRooms.elements.Count; i++)
		{
			if (!(TargetRoomTable.includedRooms.elements[i].room == null))
			{
				int num2 = GameStatsManager.Instance.QueryRoomDifferentiator(TargetRoomTable.includedRooms.elements[i].room);
				num += num2;
			}
		}
		if (num <= 0)
		{
			if (GameStatsManager.Instance.LastBossEncounteredMap.ContainsKey(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId) && !BraveRandom.IgnoreGenerationDifferentiator && GameStatsManager.Instance.LastBossEncounteredMap[GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId] == TargetRoomTable.name)
			{
				return 0.5f;
			}
			return 1f;
		}
		if (num == 1)
		{
			return 0.5f;
		}
		if (num >= 2)
		{
			return 0.01f;
		}
		return 0.01f;
	}

	public bool GlobalPrereqsValid()
	{
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
