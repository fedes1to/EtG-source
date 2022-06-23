using System.Collections.Generic;
using UnityEngine;

public class BossManager : ScriptableObject
{
	public static bool HasOverriddenCoreBoss;

	public static PrototypeDungeonRoom PriorFloorSelectedBossRoom;

	[SerializeField]
	public List<BossFloorEntry> BossFloorData;

	[SerializeField]
	public List<OverrideBossFloorEntry> OverrideBosses;

	private BossFloorEntry GetBossDataForFloor(GlobalDungeonData.ValidTilesets targetTileset)
	{
		BossFloorEntry bossFloorEntry = null;
		for (int i = 0; i < BossFloorData.Count; i++)
		{
			if ((BossFloorData[i].AssociatedTilesets | targetTileset) == BossFloorData[i].AssociatedTilesets)
			{
				bossFloorEntry = BossFloorData[i];
			}
		}
		if (bossFloorEntry == null)
		{
			bossFloorEntry = BossFloorData[0];
		}
		return bossFloorEntry;
	}

	public PrototypeDungeonRoom SelectBossRoom()
	{
		if (PriorFloorSelectedBossRoom != null)
		{
			return PriorFloorSelectedBossRoom;
		}
		GenericRoomTable genericRoomTable = SelectBossTable();
		if (genericRoomTable == null)
		{
			genericRoomTable = GetBossDataForFloor(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId).Bosses[0].TargetRoomTable;
		}
		if (!HasOverriddenCoreBoss)
		{
			for (int i = 0; i < OverrideBosses.Count; i++)
			{
				if (OverrideBosses[i].GlobalPrereqsValid(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId) && BraveRandom.GenerationRandomValue() < OverrideBosses[i].ChanceToOverride)
				{
					HasOverriddenCoreBoss = true;
					Debug.Log("Boss overridden: " + OverrideBosses[i].Annotation);
					genericRoomTable = OverrideBosses[i].TargetRoomTable;
					break;
				}
			}
		}
		if (GameStatsManager.Instance.LastBossEncounteredMap.ContainsKey(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId))
		{
			GameStatsManager.Instance.LastBossEncounteredMap[GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId] = genericRoomTable.name;
		}
		else
		{
			GameStatsManager.Instance.LastBossEncounteredMap.Add(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId, genericRoomTable.name);
		}
		WeightedRoom weightedRoom = genericRoomTable.SelectByWeight();
		if (weightedRoom == null && genericRoomTable != null && genericRoomTable.includedRooms.elements.Count > 0)
		{
			weightedRoom = genericRoomTable.includedRooms.elements[0];
		}
		if (weightedRoom == null)
		{
			Debug.LogError("BOSS FAILED TO SELECT");
			return null;
		}
		PriorFloorSelectedBossRoom = weightedRoom.room;
		return weightedRoom.room;
	}

	public GenericRoomTable SelectBossTable()
	{
		BossFloorEntry bossDataForFloor = GetBossDataForFloor(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId);
		IndividualBossFloorEntry individualBossFloorEntry = bossDataForFloor.SelectBoss();
		return individualBossFloorEntry.TargetRoomTable;
	}
}
