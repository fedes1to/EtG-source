using System;
using System.Collections.Generic;

[Serializable]
public class ChallengeDataEntry
{
	public string Annotation;

	public ChallengeModifier challenge;

	[EnumFlags]
	public GlobalDungeonData.ValidTilesets excludedTilesets;

	public List<GlobalDungeonData.ValidTilesets> tilesetsWithCustomValues;

	public List<int> CustomValues;

	public int GetWeightForFloor(GlobalDungeonData.ValidTilesets tileset)
	{
		if (tilesetsWithCustomValues.Contains(tileset))
		{
			return CustomValues[tilesetsWithCustomValues.IndexOf(tileset)];
		}
		return 1;
	}
}
