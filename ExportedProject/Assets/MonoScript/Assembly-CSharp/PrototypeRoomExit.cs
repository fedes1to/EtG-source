using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class PrototypeRoomExit
{
	public enum ExitType
	{
		NO_RESTRICTION,
		ENTRANCE_ONLY,
		EXIT_ONLY
	}

	public enum ExitGroup
	{
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H
	}

	[SerializeField]
	public DungeonData.Direction exitDirection;

	[SerializeField]
	public ExitType exitType;

	[SerializeField]
	public ExitGroup exitGroup;

	[SerializeField]
	public bool containsDoor = true;

	[SerializeField]
	public DungeonPlaceable specifiedDoor;

	[SerializeField]
	public int exitLength = 1;

	[SerializeField]
	public List<Vector2> containedCells;

	public int ExitCellCount
	{
		get
		{
			return containedCells.Count;
		}
	}

	public PrototypeRoomExit(DungeonData.Direction d, Vector2 pos)
	{
		exitDirection = d;
		containedCells = new List<Vector2>();
		containedCells.Add(pos);
	}

	public IntVector2 GetHalfExitAttachPoint(int TotalExitLength)
	{
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		for (int i = 0; i < containedCells.Count; i++)
		{
			Vector2 vector2 = containedCells[i];
			if (exitDirection == DungeonData.Direction.EAST || exitDirection == DungeonData.Direction.WEST)
			{
				if (vector2.y < vector.y)
				{
					vector = vector2;
				}
			}
			else if (vector2.x < vector.x)
			{
				vector = vector2;
			}
		}
		IntVector2 intVector = vector.ToIntVector2();
		if (exitDirection == DungeonData.Direction.SOUTH)
		{
			intVector += IntVector2.Down;
		}
		if (exitLength <= 2)
		{
			exitLength = 3;
		}
		return intVector + DungeonData.GetIntVector2FromDirection(exitDirection) * 2;
	}

	public IntVector2 GetExitAttachPoint()
	{
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		for (int i = 0; i < containedCells.Count; i++)
		{
			Vector2 vector2 = containedCells[i];
			if (exitDirection == DungeonData.Direction.EAST || exitDirection == DungeonData.Direction.WEST)
			{
				if (vector2.y < vector.y)
				{
					vector = vector2;
				}
			}
			else if (vector2.x < vector.x)
			{
				vector = vector2;
			}
		}
		IntVector2 result = vector.ToIntVector2();
		if (exitDirection == DungeonData.Direction.SOUTH)
		{
		}
		return result;
	}

	public IntVector2 GetExitOrigin(int TotalExitLength)
	{
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		for (int i = 0; i < containedCells.Count; i++)
		{
			Vector2 vector2 = containedCells[i];
			if (exitDirection == DungeonData.Direction.EAST || exitDirection == DungeonData.Direction.WEST)
			{
				if (vector2.y < vector.y)
				{
					vector = vector2;
				}
			}
			else if (vector2.x < vector.x)
			{
				vector = vector2;
			}
		}
		IntVector2 intVector = vector.ToIntVector2();
		if (exitDirection == DungeonData.Direction.SOUTH)
		{
			intVector += IntVector2.Down;
		}
		if (TotalExitLength <= 2)
		{
			exitLength = 3;
			TotalExitLength = 3;
		}
		return intVector + DungeonData.GetIntVector2FromDirection(exitDirection) * (TotalExitLength - 1);
	}

	public static PrototypeRoomExit CreateMirror(PrototypeRoomExit source, IntVector2 sourceRoomDimensions)
	{
		PrototypeRoomExit prototypeRoomExit = new PrototypeRoomExit(source.exitDirection, Vector2.zero);
		prototypeRoomExit.containedCells.Clear();
		switch (source.exitDirection)
		{
		case DungeonData.Direction.NORTH:
			prototypeRoomExit.exitDirection = DungeonData.Direction.NORTH;
			break;
		case DungeonData.Direction.EAST:
			prototypeRoomExit.exitDirection = DungeonData.Direction.WEST;
			break;
		case DungeonData.Direction.SOUTH:
			prototypeRoomExit.exitDirection = DungeonData.Direction.SOUTH;
			break;
		case DungeonData.Direction.WEST:
			prototypeRoomExit.exitDirection = DungeonData.Direction.EAST;
			break;
		default:
			prototypeRoomExit.exitDirection = source.exitDirection;
			break;
		}
		prototypeRoomExit.exitType = source.exitType;
		prototypeRoomExit.exitGroup = source.exitGroup;
		prototypeRoomExit.containsDoor = source.containsDoor;
		prototypeRoomExit.specifiedDoor = source.specifiedDoor;
		prototypeRoomExit.exitLength = source.exitLength;
		for (int i = 0; i < source.containedCells.Count; i++)
		{
			Vector2 item = source.containedCells[i];
			item.x = (float)(sourceRoomDimensions.x + 2) - (item.x + 1f);
			prototypeRoomExit.containedCells.Add(item);
		}
		return prototypeRoomExit;
	}
}
