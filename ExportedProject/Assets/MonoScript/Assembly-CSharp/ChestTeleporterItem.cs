using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class ChestTeleporterItem : PlayerItem
{
	public GameObject TeleportVFX;

	public float ChanceToBossFoyerAndUpgrade = 0.5f;

	private List<CachedChestData> m_chestos = new List<CachedChestData>();

	private bool m_isSpawning;

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloorLoaded));
	}

	protected override void OnPreDrop(PlayerController user)
	{
		user.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(user.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloorLoaded));
		base.OnPreDrop(user);
	}

	protected override void OnDestroy()
	{
		if ((bool)LastOwner)
		{
			PlayerController lastOwner = LastOwner;
			lastOwner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(lastOwner.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloorLoaded));
		}
		base.OnDestroy();
	}

	private void HandleNewFloorLoaded(PlayerController obj)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.CHARACTER_PAST)
		{
			StartCoroutine(LaunchChestSpawns());
		}
	}

	public static RoomHandler FindBossFoyer()
	{
		RoomHandler roomHandler = null;
		foreach (RoomHandler room in GameManager.Instance.Dungeon.data.rooms)
		{
			if (room.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && room.area.PrototypeRoomBossSubcategory == PrototypeDungeonRoom.RoomBossSubCategory.FLOOR_BOSS)
			{
				roomHandler = room;
				break;
			}
		}
		for (int i = 0; i < roomHandler.connectedRooms.Count; i++)
		{
			if (roomHandler.connectedRooms[i].distanceFromEntrance <= roomHandler.distanceFromEntrance)
			{
				return roomHandler.connectedRooms[i];
			}
		}
		return null;
	}

	private IEnumerator LaunchChestSpawns()
	{
		if (m_isSpawning)
		{
			yield break;
		}
		m_isSpawning = true;
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		yield return null;
		List<CachedChestData> failedList = new List<CachedChestData>();
		for (int i = 0; i < m_chestos.Count; i++)
		{
			CachedChestData cachedChestData = m_chestos[i];
			RoomHandler entrance = GameManager.Instance.Dungeon.data.Entrance;
			RoomHandler roomHandler = entrance;
			float num = ChanceToBossFoyerAndUpgrade;
			if ((bool)LastOwner && LastOwner.HasActiveBonusSynergy(CustomSynergyType.DOUBLE_TELEPORTERS))
			{
				num = 1f;
			}
			if (UnityEngine.Random.value <= num)
			{
				roomHandler = FindBossFoyer() ?? roomHandler;
				cachedChestData.Upgrade();
			}
			CellValidator cellValidator = delegate(IntVector2 c)
			{
				for (int n = 0; n < 5; n++)
				{
					for (int num2 = 0; num2 < 5; num2++)
					{
						if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(c.x + n, c.y + num2) || GameManager.Instance.Dungeon.data[c.x + n, c.y + num2].type == CellType.PIT || GameManager.Instance.Dungeon.data[c.x + n, c.y + num2].isOccupied)
						{
							return false;
						}
					}
				}
				return true;
			};
			IntVector2? randomAvailableCell = roomHandler.GetRandomAvailableCell(IntVector2.One * 5, CellTypes.FLOOR, false, cellValidator);
			IntVector2? intVector = ((!randomAvailableCell.HasValue) ? null : new IntVector2?(randomAvailableCell.GetValueOrDefault() + IntVector2.One));
			if (intVector.HasValue)
			{
				cachedChestData.SpawnChest(intVector.Value);
				for (int j = 0; j < 3; j++)
				{
					for (int k = 0; k < 3; k++)
					{
						IntVector2 key = intVector.Value + IntVector2.One + new IntVector2(j, k);
						GameManager.Instance.Dungeon.data[key].isOccupied = true;
					}
				}
				continue;
			}
			roomHandler = ((roomHandler != entrance) ? entrance : FindBossFoyer());
			if (roomHandler == null)
			{
				roomHandler = entrance;
			}
			IntVector2? randomAvailableCell2 = roomHandler.GetRandomAvailableCell(IntVector2.One * 5, CellTypes.FLOOR, false, cellValidator);
			intVector = ((!randomAvailableCell2.HasValue) ? null : new IntVector2?(randomAvailableCell2.GetValueOrDefault() + IntVector2.One));
			if (intVector.HasValue)
			{
				cachedChestData.SpawnChest(intVector.Value);
				for (int l = 0; l < 3; l++)
				{
					for (int m = 0; m < 3; m++)
					{
						IntVector2 key2 = intVector.Value + IntVector2.One + new IntVector2(l, m);
						GameManager.Instance.Dungeon.data[key2].isOccupied = true;
					}
				}
			}
			else
			{
				failedList.Add(cachedChestData);
			}
		}
		m_chestos.Clear();
		m_chestos.AddRange(failedList);
		m_isSpawning = false;
	}

	public override bool CanBeUsed(PlayerController user)
	{
		if (!user || user.CurrentRoom == null)
		{
			return false;
		}
		IPlayerInteractable nearestInteractable = user.CurrentRoom.GetNearestInteractable(user.CenterPosition, 1f, user);
		if (nearestInteractable is Chest)
		{
			Chest chest = nearestInteractable as Chest;
			if (!chest)
			{
				return false;
			}
			if (chest.IsOpen)
			{
				return false;
			}
			if (chest.GetAbsoluteParentRoom() != user.CurrentRoom)
			{
				return false;
			}
			if (chest.ChestIdentifier == Chest.SpecialChestIdentifier.RAT)
			{
				return false;
			}
			return base.CanBeUsed(user);
		}
		return false;
	}

	protected override void DoEffect(PlayerController user)
	{
		if (!user || user.CurrentRoom == null)
		{
			return;
		}
		IPlayerInteractable nearestInteractable = user.CurrentRoom.GetNearestInteractable(user.CenterPosition, 1f, user);
		AkSoundEngine.PostEvent("Play_OBJ_chestwarp_use_01", base.gameObject);
		if (!(nearestInteractable is Chest))
		{
			return;
		}
		Chest chest = nearestInteractable as Chest;
		if ((bool)chest && !chest.IsOpen && chest.GetAbsoluteParentRoom() == user.CurrentRoom)
		{
			CachedChestData item = new CachedChestData(chest);
			SpawnManager.SpawnVFX(TeleportVFX, chest.sprite.WorldCenter, Quaternion.identity, true);
			user.CurrentRoom.DeregisterInteractable(chest);
			chest.DeregisterChestOnMinimap();
			if ((bool)chest.majorBreakable)
			{
				chest.majorBreakable.TemporarilyInvulnerable = true;
			}
			UnityEngine.Object.Destroy(chest.gameObject, 0.8f);
			m_chestos.Add(item);
		}
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add(m_chestos.Count);
		for (int i = 0; i < m_chestos.Count; i++)
		{
			data.Add(m_chestos[i].Serialize());
		}
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		int num = (int)data[0];
		m_chestos.Clear();
		for (int i = 1; i < num + 1; i++)
		{
			string data2 = (string)data[i];
			CachedChestData item = new CachedChestData(data2);
			m_chestos.Add(item);
		}
		if (m_chestos.Count > 0)
		{
			StartCoroutine(LaunchChestSpawns());
		}
	}
}
