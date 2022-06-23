using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class SenseOfDirectionItem : PlayerItem
{
	public GameObject arrowVFX;

	protected override void DoEffect(PlayerController user)
	{
		RoomHandler roomHandler = null;
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			RoomHandler roomHandler2 = GameManager.Instance.Dungeon.data.rooms[i];
			if (roomHandler2.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.EXIT)
			{
				roomHandler = roomHandler2;
				break;
			}
		}
		if (roomHandler == null)
		{
			List<AIActor> allEnemies = StaticReferenceManager.AllEnemies;
			for (int j = 0; j < allEnemies.Count; j++)
			{
				if ((bool)allEnemies[j] && (bool)allEnemies[j].GetComponent<LichDeathController>())
				{
					roomHandler = allEnemies[j].ParentRoom;
					break;
				}
			}
		}
		if (roomHandler == null)
		{
			Debug.LogError("Using SenseOfDirection in Dungeon with no EXIT?");
			return;
		}
		RoomHandler currentRoom = user.CurrentRoom;
		if (roomHandler != currentRoom)
		{
			List<RoomHandler> list = FindPathBetweenNodes(currentRoom, roomHandler, GameManager.Instance.Dungeon.data.rooms);
			IntVector2 intVector = user.CenterPosition.ToIntVector2(VectorConversions.Floor);
			IntVector2 intVector2 = intVector;
			if (list != null && list.Count > 0 && (list[0] != currentRoom || list.Count >= 2))
			{
				RoomHandler otherRoom = ((list[0] != currentRoom) ? list[0] : list[1]);
				RuntimeExitDefinition exitDefinitionForConnectedRoom = currentRoom.GetExitDefinitionForConnectedRoom(otherRoom);
				intVector2 = exitDefinitionForConnectedRoom.GetUpstreamBasePosition();
			}
			Vector2 vector = intVector2.ToCenterVector2() - user.CenterPosition;
			SpawnManager.SpawnVFX(arrowVFX, user.SpriteBottomCenter + vector.ToVector3ZUp().normalized, Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(vector)), false);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public static List<RoomHandler> FindPathBetweenNodes(RoomHandler origin, RoomHandler target, List<RoomHandler> allRooms)
	{
		Dictionary<RoomHandler, int> dictionary = new Dictionary<RoomHandler, int>();
		Dictionary<RoomHandler, RoomHandler> dictionary2 = new Dictionary<RoomHandler, RoomHandler>();
		for (int i = 0; i < allRooms.Count; i++)
		{
			int value = int.MaxValue;
			if (allRooms[i] == origin)
			{
				value = 0;
			}
			if (!dictionary.ContainsKey(allRooms[i]))
			{
				dictionary.Add(allRooms[i], value);
			}
		}
		RoomHandler roomHandler = origin;
		int num = 1;
		while (true)
		{
			List<RoomHandler> connectedRooms = roomHandler.connectedRooms;
			int num2 = 0;
			while (true)
			{
				if (num2 < connectedRooms.Count)
				{
					if (connectedRooms[num2] != target)
					{
						if (dictionary.ContainsKey(connectedRooms[num2]) && dictionary[connectedRooms[num2]] > num)
						{
							dictionary[connectedRooms[num2]] = num;
							if (dictionary2.ContainsKey(connectedRooms[num2]))
							{
								dictionary2[connectedRooms[num2]] = roomHandler;
							}
							else
							{
								dictionary2.Add(connectedRooms[num2], roomHandler);
							}
						}
						num2++;
						continue;
					}
					if (dictionary2.ContainsKey(connectedRooms[num2]))
					{
						dictionary2[connectedRooms[num2]] = roomHandler;
					}
					else
					{
						dictionary2.Add(connectedRooms[num2], roomHandler);
					}
				}
				else
				{
					dictionary.Remove(roomHandler);
					if (dictionary.Count == 0)
					{
						return null;
					}
					roomHandler = null;
					num = int.MaxValue;
					foreach (RoomHandler key in dictionary.Keys)
					{
						if (dictionary[key] < num)
						{
							roomHandler = key;
							num = dictionary[key];
						}
					}
					if (roomHandler != null)
					{
						break;
					}
				}
				if (!dictionary2.ContainsKey(target))
				{
					return null;
				}
				List<RoomHandler> list = new List<RoomHandler>();
				for (RoomHandler roomHandler2 = target; roomHandler2 != null; roomHandler2 = ((!dictionary2.ContainsKey(roomHandler2)) ? null : dictionary2[roomHandler2]))
				{
					list.Insert(0, roomHandler2);
				}
				return list;
			}
		}
	}
}
