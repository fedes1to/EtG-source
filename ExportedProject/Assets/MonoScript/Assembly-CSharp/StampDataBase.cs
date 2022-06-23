using System;
using System.Collections.Generic;

[Serializable]
public abstract class StampDataBase
{
	public int width = 1;

	public int height = 1;

	public float relativeWeight = 1f;

	public DungeonTileStampData.StampPlacementRule placementRule;

	public DungeonTileStampData.StampSpace occupySpace;

	public DungeonTileStampData.StampCategory stampCategory;

	public int preferredIntermediaryStamps;

	public DungeonTileStampData.IntermediaryMatchingStyle intermediaryMatchingStyle;

	public bool requiresForcedMatchingStyle;

	public Opulence opulence;

	public List<StampPerRoomPlacementSettings> roomTypeData;

	public int indexOfSymmetricPartner = -1;

	public bool preventRoomRepeats;

	public float GetRelativeWeight(int roomSubType)
	{
		for (int i = 0; i < roomTypeData.Count; i++)
		{
			if (roomTypeData[i].roomSubType == roomSubType)
			{
				return roomTypeData[i].roomRelativeWeight;
			}
		}
		return relativeWeight;
	}
}
