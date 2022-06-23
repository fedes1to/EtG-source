using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class AGDEnemyReplacementTier
{
	public string Name;

	public DungeonPrerequisite[] Prereqs;

	[EnumFlags]
	public GlobalDungeonData.ValidTilesets TargetTileset;

	public float ChanceToReplace = 0.2f;

	public int MaxPerFloor = -1;

	public int MaxPerRun = -1;

	public bool TargetAllSignatureEnemies;

	public bool TargetAllNonSignatureEnemies;

	[EnemyIdentifier]
	public List<string> TargetGuids;

	[EnemyIdentifier]
	public List<string> ReplacementGuids;

	[Header("Exclusion Rules")]
	public bool RoomMustHaveColumns;

	public int RoomMinEnemyCount = -1;

	public int RoomMaxEnemyCount = -1;

	public int RoomMinSize = -1;

	[EnemyIdentifier]
	public List<string> RoomCantContain;

	[Header("Extras")]
	public bool RemoveAllOtherEnemies;

	public bool ExcludeForPrereqs()
	{
		return !DungeonPrerequisite.CheckConditionsFulfilled(Prereqs);
	}

	public bool ExcludeRoomForColumns(DungeonData data, RoomHandler room)
	{
		if (!RoomMustHaveColumns)
		{
			return false;
		}
		for (int i = 0; i < room.area.dimensions.x; i++)
		{
			for (int j = 0; j < room.area.dimensions.y; j++)
			{
				CellData cellData = data[room.area.basePosition.x + i, room.area.basePosition.y + j];
				if (cellData != null && cellData.type == CellType.WALL && cellData.isRoomInternal)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ExcludeRoomForEnemies(RoomHandler room, List<AIActor> activeEnemies)
	{
		if (RoomCantContain.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aIActor = activeEnemies[i];
			if ((bool)aIActor && RoomCantContain.Contains(aIActor.EnemyGuid))
			{
				return true;
			}
		}
		return false;
	}

	public bool ExcludeRoom(RoomHandler room)
	{
		if (RoomMinSize > 0 && (room.area.dimensions.x < RoomMinSize || room.area.dimensions.y < RoomMinSize))
		{
			return true;
		}
		if (RoomMinEnemyCount > 0 && room.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) < RoomMinEnemyCount)
		{
			return true;
		}
		if (RoomMaxEnemyCount > 0 && room.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) > RoomMaxEnemyCount)
		{
			return true;
		}
		return false;
	}
}
