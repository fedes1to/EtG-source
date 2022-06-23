using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonTileStampData : ScriptableObject
{
	public enum StampPlacementRule
	{
		ON_LOWER_FACEWALL,
		ON_UPPER_FACEWALL,
		BELOW_LOWER_FACEWALL,
		ALONG_LEFT_WALLS,
		ON_TOPWALL,
		ON_ANY_FLOOR,
		ABOVE_UPPER_FACEWALL,
		ON_ANY_CEILING,
		ALONG_RIGHT_WALLS,
		BELOW_LOWER_FACEWALL_LEFT_CORNER,
		BELOW_LOWER_FACEWALL_RIGHT_CORNER
	}

	public enum StampSpace
	{
		OBJECT_SPACE,
		WALL_SPACE,
		BOTH_SPACES
	}

	public enum StampCategory
	{
		STRUCTURAL,
		NATURAL,
		MUNDANE,
		DECORATIVE
	}

	public enum IntermediaryMatchingStyle
	{
		ANY,
		COLUMN,
		WALL_HOLE,
		BANNER,
		PORTRAIT,
		WALL_HOLE_FILLER,
		SKELETON,
		ROCK
	}

	public float tileStampWeight = 1f;

	public float spriteStampWeight;

	public float objectStampWeight = 1f;

	public TileStampData[] stamps;

	public SpriteStampData[] spriteStamps;

	public ObjectStampData[] objectStamps;

	public float SymmetricFrameChance = 0.5f;

	public float SymmetricCompleteChance = 0.25f;

	public bool ContainsTileIndex(int index)
	{
		if (stamps == null)
		{
			return false;
		}
		TileStampData[] array = stamps;
		foreach (TileStampData tileStampData in array)
		{
			if (tileStampData.stampTileIndices.Contains(index))
			{
				return true;
			}
		}
		return false;
	}

	protected bool IsValidRoomType(StampDataBase s, int roomType)
	{
		if (s.roomTypeData.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < s.roomTypeData.Count; i++)
		{
			if (s.roomTypeData[i].roomSubType == roomType)
			{
				return true;
			}
		}
		return false;
	}

	public StampDataBase GetStampDataSimple(StampPlacementRule placement, Opulence oppan, int roomType, int maxWidth = 1000)
	{
		WeightedList<StampDataBase> weightedList = new WeightedList<StampDataBase>();
		for (int i = 0; i < stamps.Length; i++)
		{
			TileStampData tileStampData = stamps[i];
			if (!tileStampData.requiresForcedMatchingStyle && placement == tileStampData.placementRule && IsValidRoomType(tileStampData, roomType) && tileStampData.width <= maxWidth)
			{
				weightedList.Add(tileStampData, tileStampData.GetRelativeWeight(roomType) * tileStampWeight);
			}
		}
		for (int j = 0; j < spriteStamps.Length; j++)
		{
			SpriteStampData spriteStampData = spriteStamps[j];
			if (!spriteStampData.requiresForcedMatchingStyle && placement == spriteStampData.placementRule && IsValidRoomType(spriteStampData, roomType) && spriteStampData.width <= maxWidth)
			{
				weightedList.Add(spriteStampData, spriteStampData.GetRelativeWeight(roomType) * spriteStampWeight);
			}
		}
		for (int k = 0; k < objectStamps.Length; k++)
		{
			ObjectStampData objectStampData = objectStamps[k];
			if (!objectStampData.requiresForcedMatchingStyle && placement == objectStampData.placementRule && IsValidRoomType(objectStampData, roomType) && objectStampData.width <= maxWidth)
			{
				weightedList.Add(objectStampData, objectStampData.GetRelativeWeight(roomType) * objectStampWeight);
			}
		}
		if (weightedList.elements == null || weightedList.elements.Count == 0)
		{
			return null;
		}
		return weightedList.SelectByWeight();
	}

	public StampDataBase AttemptGetSimilarStampForRoomDuplication(StampDataBase source, List<StampDataBase> excluded, int roomType)
	{
		WeightedList<StampDataBase> weightedList = new WeightedList<StampDataBase>();
		for (int i = 0; i < stamps.Length; i++)
		{
			StampDataBase stampDataBase = stamps[i];
			if (IsValidRoomType(stampDataBase, roomType) && source.stampCategory == stampDataBase.stampCategory && source.placementRule == stampDataBase.placementRule && source.width == stampDataBase.width && source.height == stampDataBase.height && source.occupySpace == stampDataBase.occupySpace && source.preventRoomRepeats == stampDataBase.preventRoomRepeats && !excluded.Contains(stampDataBase))
			{
				weightedList.Add(stampDataBase, stampDataBase.GetRelativeWeight(roomType));
			}
		}
		if (weightedList.elements == null || weightedList.elements.Count == 0)
		{
			return null;
		}
		return weightedList.SelectByWeight();
	}

	public StampDataBase GetStampDataSimple(List<StampPlacementRule> placements, Opulence oppan, int roomType, int maxWidth, bool excludeWallSpace, List<StampDataBase> excluded)
	{
		WeightedList<StampDataBase> weightedList = new WeightedList<StampDataBase>();
		for (int i = 0; i < stamps.Length; i++)
		{
			TileStampData tileStampData = stamps[i];
			if ((!tileStampData.preventRoomRepeats || !excluded.Contains(tileStampData)) && !tileStampData.requiresForcedMatchingStyle)
			{
				bool flag = tileStampData.placementRule == StampPlacementRule.ALONG_LEFT_WALLS || tileStampData.placementRule == StampPlacementRule.ALONG_RIGHT_WALLS;
				if ((!excludeWallSpace || flag || tileStampData.height <= 1) && (!excludeWallSpace || tileStampData.width <= 1) && (!excludeWallSpace || tileStampData.occupySpace == StampSpace.OBJECT_SPACE) && placements.Contains(tileStampData.placementRule) && IsValidRoomType(tileStampData, roomType) && tileStampData.width <= maxWidth)
				{
					weightedList.Add(tileStampData, tileStampData.GetRelativeWeight(roomType) * tileStampWeight);
				}
			}
		}
		for (int j = 0; j < spriteStamps.Length; j++)
		{
			SpriteStampData spriteStampData = spriteStamps[j];
			if ((!spriteStampData.preventRoomRepeats || !excluded.Contains(spriteStampData)) && (!excludeWallSpace || spriteStampData.height <= 1) && (!excludeWallSpace || spriteStampData.width <= 1) && !spriteStampData.requiresForcedMatchingStyle && (!excludeWallSpace || spriteStampData.occupySpace == StampSpace.OBJECT_SPACE) && placements.Contains(spriteStampData.placementRule) && IsValidRoomType(spriteStampData, roomType) && spriteStampData.width <= maxWidth)
			{
				weightedList.Add(spriteStampData, spriteStampData.GetRelativeWeight(roomType) * spriteStampWeight);
			}
		}
		for (int k = 0; k < objectStamps.Length; k++)
		{
			ObjectStampData objectStampData = objectStamps[k];
			if (!objectStampData.preventRoomRepeats || !excluded.Contains(objectStampData))
			{
				bool flag2 = objectStampData.placementRule == StampPlacementRule.ALONG_LEFT_WALLS || objectStampData.placementRule == StampPlacementRule.ALONG_RIGHT_WALLS;
				if ((!excludeWallSpace || flag2 || objectStampData.height <= 1) && (!excludeWallSpace || objectStampData.width <= 1) && !objectStampData.requiresForcedMatchingStyle && (!excludeWallSpace || objectStampData.occupySpace == StampSpace.OBJECT_SPACE) && placements.Contains(objectStampData.placementRule) && IsValidRoomType(objectStampData, roomType) && objectStampData.width <= maxWidth)
				{
					weightedList.Add(objectStampData, objectStampData.GetRelativeWeight(roomType) * objectStampWeight);
				}
			}
		}
		if (weightedList.elements == null || weightedList.elements.Count == 0)
		{
			return null;
		}
		return weightedList.SelectByWeight();
	}

	public StampDataBase GetStampDataSimpleWithForcedRule(List<StampPlacementRule> placements, IntermediaryMatchingStyle forcedStyle, Opulence oppan, int roomType, int maxWidth = 1000, bool excludeWallSpace = false)
	{
		WeightedList<StampDataBase> weightedList = new WeightedList<StampDataBase>();
		for (int i = 0; i < stamps.Length; i++)
		{
			TileStampData tileStampData = stamps[i];
			bool flag = tileStampData.placementRule == StampPlacementRule.ALONG_LEFT_WALLS || tileStampData.placementRule == StampPlacementRule.ALONG_RIGHT_WALLS;
			if ((!excludeWallSpace || flag || tileStampData.height <= 1) && (!excludeWallSpace || tileStampData.width <= 1) && (!excludeWallSpace || tileStampData.occupySpace == StampSpace.OBJECT_SPACE) && placements.Contains(tileStampData.placementRule) && IsValidRoomType(tileStampData, roomType) && tileStampData.width <= maxWidth && tileStampData.intermediaryMatchingStyle == forcedStyle)
			{
				weightedList.Add(tileStampData, tileStampData.GetRelativeWeight(roomType) * tileStampWeight);
			}
		}
		for (int j = 0; j < spriteStamps.Length; j++)
		{
			SpriteStampData spriteStampData = spriteStamps[j];
			if ((!excludeWallSpace || spriteStampData.height <= 1) && (!excludeWallSpace || spriteStampData.width <= 1) && (!excludeWallSpace || spriteStampData.occupySpace == StampSpace.OBJECT_SPACE) && placements.Contains(spriteStampData.placementRule) && IsValidRoomType(spriteStampData, roomType) && spriteStampData.width <= maxWidth && spriteStampData.intermediaryMatchingStyle == forcedStyle)
			{
				weightedList.Add(spriteStampData, spriteStampData.GetRelativeWeight(roomType) * spriteStampWeight);
			}
		}
		for (int k = 0; k < objectStamps.Length; k++)
		{
			ObjectStampData objectStampData = objectStamps[k];
			bool flag2 = objectStampData.placementRule == StampPlacementRule.ALONG_LEFT_WALLS || objectStampData.placementRule == StampPlacementRule.ALONG_RIGHT_WALLS;
			if ((!excludeWallSpace || flag2 || objectStampData.height <= 1) && (!excludeWallSpace || objectStampData.width <= 1) && (!excludeWallSpace || objectStampData.occupySpace == StampSpace.OBJECT_SPACE) && placements.Contains(objectStampData.placementRule) && IsValidRoomType(objectStampData, roomType) && objectStampData.width <= maxWidth && objectStampData.intermediaryMatchingStyle == forcedStyle)
			{
				weightedList.Add(objectStampData, objectStampData.GetRelativeWeight(roomType) * objectStampWeight);
			}
		}
		if (weightedList.elements == null || weightedList.elements.Count == 0)
		{
			return null;
		}
		return weightedList.SelectByWeight();
	}

	public StampDataBase GetStampDataComplex(StampPlacementRule placement, StampSpace space, StampCategory category, Opulence oppan, int roomType, int maxWidth = 1000)
	{
		List<StampPlacementRule> list = new List<StampPlacementRule>();
		list.Add(placement);
		return GetStampDataComplex(list, space, category, oppan, roomType, maxWidth);
	}

	public StampDataBase GetStampDataComplex(List<StampPlacementRule> placements, StampSpace space, StampCategory category, Opulence oppan, int roomType, int maxWidth = 1000)
	{
		bool flag = placements.Contains(StampPlacementRule.ALONG_LEFT_WALLS) || placements.Contains(StampPlacementRule.ALONG_RIGHT_WALLS);
		WeightedList<StampDataBase> weightedList = new WeightedList<StampDataBase>();
		for (int i = 0; i < stamps.Length; i++)
		{
			TileStampData tileStampData = stamps[i];
			if (!tileStampData.requiresForcedMatchingStyle && placements.Contains(tileStampData.placementRule) && tileStampData.occupySpace == space && IsValidRoomType(tileStampData, roomType) && ((!flag) ? tileStampData.width : tileStampData.height) <= maxWidth)
			{
				weightedList.Add(tileStampData, tileStampData.GetRelativeWeight(roomType) * tileStampWeight);
			}
		}
		for (int j = 0; j < spriteStamps.Length; j++)
		{
			SpriteStampData spriteStampData = spriteStamps[j];
			if (!spriteStampData.requiresForcedMatchingStyle && placements.Contains(spriteStampData.placementRule) && spriteStampData.occupySpace == space && IsValidRoomType(spriteStampData, roomType) && ((!flag) ? spriteStampData.width : spriteStampData.height) <= maxWidth)
			{
				weightedList.Add(spriteStampData, spriteStampData.GetRelativeWeight(roomType) * spriteStampWeight);
			}
		}
		for (int k = 0; k < objectStamps.Length; k++)
		{
			ObjectStampData objectStampData = objectStamps[k];
			if (!objectStampData.requiresForcedMatchingStyle && placements.Contains(objectStampData.placementRule) && objectStampData.occupySpace == space && IsValidRoomType(objectStampData, roomType) && ((!flag) ? objectStampData.width : objectStampData.height) <= maxWidth)
			{
				weightedList.Add(objectStampData, objectStampData.GetRelativeWeight(roomType) * objectStampWeight);
			}
		}
		return weightedList.SelectByWeight();
	}
}
