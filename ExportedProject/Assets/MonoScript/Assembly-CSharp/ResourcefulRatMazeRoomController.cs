using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeonator;
using UnityEngine;

public class ResourcefulRatMazeRoomController : DungeonPlaceableBehaviour
{
	private List<ResourcefulRatMazeRoomController> m_mazes;

	private int minX = int.MaxValue;

	private int minY = int.MaxValue;

	private int maxX = int.MinValue;

	private int maxY = int.MinValue;

	private DungeonData.Direction m_correctDirection;

	private RoomHandler m_room;

	private int m_roomIndex;

	public IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		Initialize();
	}

	private int GetPositionInChain(RoomHandler room)
	{
		ResourcefulRatMazeRoomController[] collection = Object.FindObjectsOfType<ResourcefulRatMazeRoomController>();
		List<ResourcefulRatMazeRoomController> source = new List<ResourcefulRatMazeRoomController>(collection);
		return (m_mazes = source.OrderBy((ResourcefulRatMazeRoomController a) => a.transform.position.x).ToList()).IndexOf(this);
	}

	public void Initialize()
	{
		RoomHandler roomHandler = (m_room = base.transform.position.GetAbsoluteRoom());
		m_roomIndex = GetPositionInChain(roomHandler);
		DungeonData.Direction[] resourcefulRatSolution = GameManager.GetResourcefulRatSolution();
		m_correctDirection = resourcefulRatSolution[m_roomIndex];
		for (int i = 0; i < roomHandler.Cells.Count; i++)
		{
			IntVector2 intVector = roomHandler.Cells[i];
			minX = Mathf.Min(minX, intVector.x);
			minY = Mathf.Min(minY, intVector.y);
			maxX = Mathf.Max(maxX, intVector.x);
			maxY = Mathf.Max(maxY, intVector.y);
		}
	}

	private bool PlayerInTargetCell(Vector2 playerPos)
	{
		switch (m_correctDirection)
		{
		case DungeonData.Direction.NORTH:
			if (playerPos.y > (float)maxY)
			{
				return true;
			}
			break;
		case DungeonData.Direction.EAST:
			if (playerPos.x > (float)maxX)
			{
				return true;
			}
			break;
		case DungeonData.Direction.SOUTH:
			if (playerPos.y < (float)(minY + 1))
			{
				return true;
			}
			break;
		case DungeonData.Direction.WEST:
			if (playerPos.x < (float)(minX + 1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private bool PlayerInFailCell(Vector2 playerPos)
	{
		bool flag = m_roomIndex != 0;
		switch (m_correctDirection)
		{
		case DungeonData.Direction.NORTH:
			if (playerPos.x > (float)maxX)
			{
				return true;
			}
			if (playerPos.y < (float)(minY + 1))
			{
				return true;
			}
			if (flag && playerPos.x < (float)(minX + 1))
			{
				return true;
			}
			break;
		case DungeonData.Direction.EAST:
			if (playerPos.y > (float)maxY)
			{
				return true;
			}
			if (playerPos.y < (float)(minY + 1))
			{
				return true;
			}
			if (flag && playerPos.x < (float)(minX + 1))
			{
				return true;
			}
			break;
		case DungeonData.Direction.SOUTH:
			if (playerPos.x > (float)maxX)
			{
				return true;
			}
			if (playerPos.y > (float)maxY)
			{
				return true;
			}
			if (flag && playerPos.x < (float)(minX + 1))
			{
				return true;
			}
			break;
		case DungeonData.Direction.WEST:
			if (playerPos.x > (float)maxX)
			{
				return true;
			}
			if (playerPos.y > (float)maxY)
			{
				return true;
			}
			if (playerPos.y < (float)(minY + 1))
			{
				return true;
			}
			break;
		}
		return false;
	}

	public void Update()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			Vector2 playerPos = playerController.SpriteBottomCenter;
			if (playerController.CurrentRoom != m_room)
			{
				continue;
			}
			if (PlayerInTargetCell(playerPos))
			{
				if (m_mazes.IndexOf(this) == m_mazes.Count - 1)
				{
					RoomHandler roomHandler = null;
					foreach (RoomHandler room in GameManager.Instance.Dungeon.data.rooms)
					{
						if (room.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
						{
							roomHandler = room;
							break;
						}
					}
					for (int j = 0; j < roomHandler.connectedRooms.Count; j++)
					{
						if (roomHandler.connectedRooms[j].distanceFromEntrance <= roomHandler.distanceFromEntrance)
						{
							roomHandler = roomHandler.connectedRooms[j];
							break;
						}
					}
					playerController.WarpToPoint(roomHandler.Epicenter.ToVector2());
				}
				else
				{
					Vector2 targetPoint = m_mazes[m_mazes.IndexOf(this) + 1].transform.position.XY();
					playerController.WarpToPoint(targetPoint);
				}
			}
			else
			{
				if (!PlayerInFailCell(playerPos))
				{
					continue;
				}
				RoomHandler roomHandler2 = null;
				foreach (RoomHandler room2 in GameManager.Instance.Dungeon.data.rooms)
				{
					if (room2.area.PrototypeRoomName.Contains("FailRoom"))
					{
						roomHandler2 = room2;
						break;
					}
				}
				playerController.WarpToPoint(roomHandler2.Epicenter.ToVector2());
			}
		}
	}
}
