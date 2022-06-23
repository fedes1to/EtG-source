using Dungeonator;
using UnityEngine;

public static class TelevisionQuestController
{
	public static void RemoveMaintenanceRoomBackpack()
	{
		bool flag = false;
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_READY_FOR_UNLOCKS))
		{
			switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
			{
			case GlobalDungeonData.ValidTilesets.GUNGEON:
				if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_COMPLETE))
				{
					flag = true;
				}
				break;
			case GlobalDungeonData.ValidTilesets.MINEGEON:
				if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK2_COMPLETE) && GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_COMPLETE))
				{
					flag = true;
				}
				break;
			case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
				if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK3_COMPLETE) && GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK2_COMPLETE))
				{
					flag = true;
				}
				break;
			case GlobalDungeonData.ValidTilesets.FORGEGEON:
				if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK4_COMPLETE) && GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK3_COMPLETE))
				{
					flag = true;
				}
				break;
			}
		}
		if (flag)
		{
			return;
		}
		GameObject gameObject = GameObject.Find("MaintenanceRoom(Clone)");
		if (gameObject != null)
		{
			Transform transform = gameObject.transform.Find("Pack");
			if (transform != null)
			{
				transform.gameObject.SetActive(false);
			}
		}
	}

	public static void HandlePuzzleSetup()
	{
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.GUNGEON || GameManager.Instance.CurrentLevelOverrideState != 0 || !GameStatsManager.Instance.GetFlag(GungeonFlags.SHERPA_UNLOCK1_COMPLETE))
		{
			return;
		}
		RoomHandler roomHandler = null;
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			string roomName = GameManager.Instance.Dungeon.data.rooms[i].GetRoomName();
			if (roomName != null && roomName.Contains("Maintenance"))
			{
				roomHandler = GameManager.Instance.Dungeon.data.rooms[i];
			}
		}
		if (roomHandler != null)
		{
			bool success = false;
			IntVector2 centeredVisibleClearSpot = roomHandler.GetCenteredVisibleClearSpot(2, 2, out success, true);
			if (success)
			{
				DungeonPlaceableUtility.InstantiateDungeonPlaceable(BraveResources.Load("Global Prefabs/Global Items/BustedTelevisionPlaceable") as GameObject, roomHandler, centeredVisibleClearSpot - roomHandler.area.basePosition, false);
			}
		}
	}
}
