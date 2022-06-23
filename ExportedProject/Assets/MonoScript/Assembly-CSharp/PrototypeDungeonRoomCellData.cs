using System;
using System.Collections.Generic;
using Dungeonator;

[Serializable]
public class PrototypeDungeonRoomCellData
{
	public CellType state;

	public DiagonalWallType diagonalWallType;

	public bool breakable;

	public string str;

	public bool conditionalOnParentExit;

	public bool conditionalCellIsPit;

	public int parentExitIndex = -1;

	public bool containsManuallyPlacedLight;

	public int lightStampIndex;

	public int lightPixelsOffsetY;

	public bool doesDamage;

	public CellDamageDefinition damageDefinition;

	public int placedObjectRUBELIndex = -1;

	public List<int> additionalPlacedObjectIndices = new List<int>();

	public PrototypeDungeonRoomCellAppearance appearance;

	public bool ForceTileNonDecorated;

	public bool IsOccupied
	{
		get
		{
			if (placedObjectRUBELIndex >= 0)
			{
				return true;
			}
			return false;
		}
	}

	public PrototypeDungeonRoomCellData()
	{
	}

	public PrototypeDungeonRoomCellData(string s, CellType st)
	{
		str = s;
		state = st;
	}

	public bool HasChanges()
	{
		return diagonalWallType != 0 || breakable || !string.IsNullOrEmpty(str) || conditionalOnParentExit || conditionalCellIsPit || parentExitIndex != -1 || containsManuallyPlacedLight || lightStampIndex != 0 || lightPixelsOffsetY != 0 || doesDamage || damageDefinition.HasChanges() || placedObjectRUBELIndex != -1 || additionalPlacedObjectIndices.Count != 0 || (appearance != null && appearance.HasChanges()) || ForceTileNonDecorated;
	}

	public bool IsOccupiedAtLayer(int layerIndex)
	{
		if (state == CellType.WALL)
		{
			return true;
		}
		if (additionalPlacedObjectIndices.Count > layerIndex && additionalPlacedObjectIndices[layerIndex] >= 0)
		{
			return true;
		}
		return false;
	}

	public void MirrorData(PrototypeDungeonRoomCellData source)
	{
		state = source.state;
		switch (source.diagonalWallType)
		{
		case DiagonalWallType.NONE:
			diagonalWallType = DiagonalWallType.NONE;
			break;
		case DiagonalWallType.NORTHEAST:
			diagonalWallType = DiagonalWallType.NORTHWEST;
			break;
		case DiagonalWallType.NORTHWEST:
			diagonalWallType = DiagonalWallType.NORTHEAST;
			break;
		case DiagonalWallType.SOUTHEAST:
			diagonalWallType = DiagonalWallType.SOUTHWEST;
			break;
		case DiagonalWallType.SOUTHWEST:
			diagonalWallType = DiagonalWallType.SOUTHEAST;
			break;
		}
		breakable = source.breakable;
		str = source.str;
		conditionalOnParentExit = source.conditionalOnParentExit;
		conditionalCellIsPit = source.conditionalCellIsPit;
		parentExitIndex = source.parentExitIndex;
		containsManuallyPlacedLight = source.containsManuallyPlacedLight;
		lightStampIndex = source.lightStampIndex;
		lightPixelsOffsetY = source.lightPixelsOffsetY;
		doesDamage = source.doesDamage;
		damageDefinition = source.damageDefinition;
		placedObjectRUBELIndex = source.placedObjectRUBELIndex;
		additionalPlacedObjectIndices = new List<int>();
		for (int i = 0; i < source.additionalPlacedObjectIndices.Count; i++)
		{
			additionalPlacedObjectIndices.Add(source.additionalPlacedObjectIndices[i]);
		}
		appearance = new PrototypeDungeonRoomCellAppearance();
		appearance.MirrorData(source.appearance);
		ForceTileNonDecorated = source.ForceTileNonDecorated;
	}
}
