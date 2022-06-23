using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PrototypeRoomPitEntry
{
	public enum PitBorderType
	{
		FLAT,
		RAISED,
		NONE
	}

	public List<Vector2> containedCells;

	public PitBorderType borderType;

	public PrototypeRoomPitEntry(IEnumerable<Vector2> cells)
	{
		containedCells = new List<Vector2>(cells);
	}

	public PrototypeRoomPitEntry(Vector2 cell)
	{
		containedCells = new List<Vector2>();
		containedCells.Add(cell);
	}

	public PrototypeRoomPitEntry CreateMirror(IntVector2 roomDimensions)
	{
		PrototypeRoomPitEntry prototypeRoomPitEntry = new PrototypeRoomPitEntry(Vector2.zero);
		prototypeRoomPitEntry.containedCells.Clear();
		prototypeRoomPitEntry.borderType = borderType;
		for (int i = 0; i < containedCells.Count; i++)
		{
			Vector2 item = containedCells[i];
			item.x = (float)roomDimensions.x - (item.x + 1f);
			prototypeRoomPitEntry.containedCells.Add(item);
		}
		return prototypeRoomPitEntry;
	}

	public bool IsAdjoining(Vector2 cell)
	{
		foreach (Vector2 containedCell in containedCells)
		{
			if (Mathf.Approximately(Vector2.Distance(cell, containedCell), 1f))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAdjoining(IEnumerable<Vector2> cells)
	{
		foreach (Vector2 cell in cells)
		{
			foreach (Vector2 containedCell in containedCells)
			{
				if (Mathf.Approximately(Vector2.Distance(cell, containedCell), 1f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddCells(IEnumerable<Vector2> cells)
	{
		foreach (Vector2 cell in cells)
		{
			if (!containedCells.Contains(cell))
			{
				containedCells.Add(cell);
			}
		}
	}
}
