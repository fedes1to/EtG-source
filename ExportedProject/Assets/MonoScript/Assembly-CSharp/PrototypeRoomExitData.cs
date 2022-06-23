using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class PrototypeRoomExitData
{
	[SerializeField]
	public List<PrototypeRoomExit> exits;

	public PrototypeRoomExit this[int xOffset, int yOffset]
	{
		get
		{
			if (exits == null)
			{
				return null;
			}
			IntVector2 intVector = new IntVector2(xOffset, yOffset);
			for (int num = exits.Count - 1; num >= 0; num--)
			{
				PrototypeRoomExit prototypeRoomExit = exits[num];
				if (prototypeRoomExit == null || prototypeRoomExit.containedCells == null)
				{
					exits.RemoveAt(num);
				}
				else if (prototypeRoomExit.containedCells.Contains(intVector.ToVector2()))
				{
					return prototypeRoomExit;
				}
			}
			return null;
		}
	}

	public void MirrorData(PrototypeRoomExitData source, IntVector2 sourceDimensions)
	{
		exits = new List<PrototypeRoomExit>();
		for (int i = 0; i < source.exits.Count; i++)
		{
			exits.Add(PrototypeRoomExit.CreateMirror(source.exits[i], sourceDimensions));
		}
	}

	public bool HasDefinedExitGroups()
	{
		return GetDefinedExitGroups().Count > 1;
	}

	public List<PrototypeRoomExit.ExitGroup> GetDefinedExitGroups()
	{
		List<PrototypeRoomExit.ExitGroup> list = new List<PrototypeRoomExit.ExitGroup>();
		for (int i = 0; i < exits.Count; i++)
		{
			if (!list.Contains(exits[i].exitGroup))
			{
				list.Add(exits[i].exitGroup);
			}
		}
		return list;
	}

	public bool ProcessExitPosition(int ix, int iy, PrototypeDungeonRoom parent)
	{
		PrototypeRoomExit prototypeRoomExit = this[ix + 1, iy + 1];
		if (prototypeRoomExit == null)
		{
			AddExitPosition(ix, iy, parent);
			return true;
		}
		return RemoveExitPosition(ix, iy, parent);
	}

	public void AddExitPosition(int ix, int iy, PrototypeDungeonRoom parent)
	{
		IntVector2 intVector = new IntVector2(ix, iy);
		HashSet<IntVector2> hashSet = ExitToCellRepresentation();
		hashSet.Add(intVector + IntVector2.One);
		exits = CellToExitRepresentation(hashSet, parent);
		PrototypeDungeonRoomCellData[] fullCellData = parent.FullCellData;
		foreach (PrototypeDungeonRoomCellData prototypeDungeonRoomCellData in fullCellData)
		{
			prototypeDungeonRoomCellData.conditionalOnParentExit = false;
			prototypeDungeonRoomCellData.parentExitIndex = -1;
		}
	}

	public bool RemoveExitPosition(int ix, int iy, PrototypeDungeonRoom parent)
	{
		IntVector2 item = new IntVector2(ix + 1, iy + 1);
		HashSet<IntVector2> hashSet = ExitToCellRepresentation();
		bool result = hashSet.Remove(item);
		exits = CellToExitRepresentation(hashSet, parent);
		PrototypeDungeonRoomCellData[] fullCellData = parent.FullCellData;
		foreach (PrototypeDungeonRoomCellData prototypeDungeonRoomCellData in fullCellData)
		{
			prototypeDungeonRoomCellData.conditionalOnParentExit = false;
			prototypeDungeonRoomCellData.parentExitIndex = -1;
		}
		return result;
	}

	public void TranslateAllExits(int xOffset, int yOffset, PrototypeDungeonRoom parent)
	{
		IntVector2 intVector = new IntVector2(xOffset, yOffset);
		HashSet<IntVector2> collection = ExitToCellRepresentation();
		List<IntVector2> list = new List<IntVector2>(collection);
		for (int i = 0; i < list.Count; i++)
		{
			list[i] += intVector;
		}
		collection = new HashSet<IntVector2>(list);
		exits = CellToExitRepresentation(collection, parent);
	}

	public void HandleRowColumnShift(int rowXCoord, int xShift, int columnYCoord, int yShift, PrototypeDungeonRoom parent)
	{
		IntVector2 intVector = new IntVector2(xShift, yShift);
		HashSet<IntVector2> hashSet = ExitToCellRepresentation();
		if (hashSet.Count == 0)
		{
			return;
		}
		List<IntVector2> list = new List<IntVector2>(hashSet);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			IntVector2 intVector2 = list[num];
			if (intVector2.x > rowXCoord && intVector2.y > columnYCoord)
			{
				list[num] = intVector2 + intVector;
			}
			else if (intVector2.x == rowXCoord || intVector2.y == columnYCoord)
			{
				if (xShift == -1 || yShift == -1)
				{
					list.RemoveAt(num);
				}
				else
				{
					list[num] = intVector2 + intVector;
				}
			}
		}
		hashSet = new HashSet<IntVector2>(list);
		exits = CellToExitRepresentation(hashSet, parent);
	}

	private List<PrototypeRoomExit> CellToExitRepresentation(HashSet<IntVector2> cells, PrototypeDungeonRoom parent)
	{
		int num = parent.Width + 2;
		int num2 = parent.Height + 2;
		bool[,] array = new bool[num, num2];
		foreach (IntVector2 cell in cells)
		{
			array[cell.x, cell.y] = true;
		}
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		List<PrototypeRoomExit> list = new List<PrototypeRoomExit>();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				IntVector2 intVector = new IntVector2(i, j);
				if (hashSet.Contains(intVector))
				{
					continue;
				}
				hashSet.Add(intVector);
				if (!array[i, j])
				{
					continue;
				}
				DungeonData.Direction floorDirection = parent.GetFloorDirection(i - 1, j - 1);
				if (floorDirection == DungeonData.Direction.SOUTHWEST)
				{
					Debug.LogError("An exit was defined with no nearby floor tile. This is unsupported behavior.");
					continue;
				}
				DungeonData.Direction d = (DungeonData.Direction)((int)(floorDirection + 4) % 8);
				PrototypeRoomExit prototypeRoomExit = new PrototypeRoomExit(d, intVector.ToVector2());
				RecurseFindExits(array, intVector + IntVector2.Up, hashSet, prototypeRoomExit);
				RecurseFindExits(array, intVector + IntVector2.Right, hashSet, prototypeRoomExit);
				RecurseFindExits(array, intVector + IntVector2.Down, hashSet, prototypeRoomExit);
				RecurseFindExits(array, intVector + IntVector2.Left, hashSet, prototypeRoomExit);
				PrototypeRoomExit prototypeRoomExit2 = FindPreviouslyDefinedExit(prototypeRoomExit);
				if (prototypeRoomExit2 != null)
				{
					prototypeRoomExit.exitGroup = prototypeRoomExit2.exitGroup;
					prototypeRoomExit.exitType = prototypeRoomExit2.exitType;
					prototypeRoomExit.containsDoor = prototypeRoomExit2.containsDoor;
					prototypeRoomExit.specifiedDoor = prototypeRoomExit2.specifiedDoor;
				}
				list.Add(prototypeRoomExit);
			}
		}
		return list;
	}

	private PrototypeRoomExit FindPreviouslyDefinedExit(PrototypeRoomExit newExit)
	{
		for (int i = 0; i < newExit.containedCells.Count; i++)
		{
			for (int j = 0; j < exits.Count; j++)
			{
				if (exits[j].containedCells.Contains(newExit.containedCells[i]))
				{
					return exits[j];
				}
			}
		}
		return null;
	}

	private void RecurseFindExits(bool[,] exitMatrix, IntVector2 coords, HashSet<IntVector2> closedSet, PrototypeRoomExit currentExit)
	{
		if (coords.x >= 0 && coords.y >= 0 && coords.x < exitMatrix.GetLength(0) && coords.y < exitMatrix.GetLength(1) && !closedSet.Contains(coords))
		{
			if (exitMatrix[coords.x, coords.y])
			{
				currentExit.containedCells.Add(coords.ToVector2());
				closedSet.Add(coords);
				RecurseFindExits(exitMatrix, coords + IntVector2.Up, closedSet, currentExit);
				RecurseFindExits(exitMatrix, coords + IntVector2.Right, closedSet, currentExit);
				RecurseFindExits(exitMatrix, coords + IntVector2.Down, closedSet, currentExit);
				RecurseFindExits(exitMatrix, coords + IntVector2.Left, closedSet, currentExit);
			}
			else
			{
				closedSet.Add(coords);
			}
		}
	}

	private HashSet<IntVector2> ExitToCellRepresentation()
	{
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		for (int i = 0; i < exits.Count; i++)
		{
			PrototypeRoomExit prototypeRoomExit = exits[i];
			for (int j = 0; j < prototypeRoomExit.containedCells.Count; j++)
			{
				Vector2 vector = prototypeRoomExit.containedCells[j];
				IntVector2 item = new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
				hashSet.Add(item);
			}
		}
		return hashSet;
	}

	public List<PrototypeRoomExit> GetUnusedExitsFromInstance(CellArea instance)
	{
		List<PrototypeRoomExit> list = new List<PrototypeRoomExit>();
		for (int i = 0; i < exits.Count; i++)
		{
			if (exits[i].exitType != PrototypeRoomExit.ExitType.ENTRANCE_ONLY && !instance.instanceUsedExits.Contains(exits[i]))
			{
				list.Add(exits[i]);
			}
		}
		return list;
	}

	public List<PrototypeRoomExit> GetUnusedExitsOnSide(DungeonData.Direction exitDir)
	{
		List<PrototypeRoomExit> list = new List<PrototypeRoomExit>();
		for (int i = 0; i < exits.Count; i++)
		{
			if (exits[i].exitDirection == exitDir)
			{
				list.Add(exits[i]);
			}
		}
		return list;
	}
}
