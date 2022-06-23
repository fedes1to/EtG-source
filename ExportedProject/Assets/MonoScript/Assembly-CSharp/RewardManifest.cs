using System;
using System.Collections.Generic;

public static class RewardManifest
{
	public static void Initialize(RewardManager manager)
	{
		manager.SeededRunManifests = new Dictionary<GlobalDungeonData.ValidTilesets, FloorRewardManifest>();
		GlobalDungeonData.ValidTilesets[] array = (GlobalDungeonData.ValidTilesets[])Enum.GetValues(typeof(GlobalDungeonData.ValidTilesets));
		for (int i = 0; i < manager.FloorRewardData.Count; i++)
		{
			FloorRewardData floorRewardData = manager.FloorRewardData[i];
			foreach (GlobalDungeonData.ValidTilesets validTilesets in array)
			{
				if ((floorRewardData.AssociatedTilesets & validTilesets) == validTilesets)
				{
					FloorRewardManifest value = GenerateManifestForFloor(manager, floorRewardData);
					if (!manager.SeededRunManifests.ContainsKey(validTilesets))
					{
						manager.SeededRunManifests.Add(validTilesets, value);
					}
				}
			}
		}
	}

	public static void Reinitialize(RewardManager manager)
	{
		GlobalDungeonData.ValidTilesets[] array = (GlobalDungeonData.ValidTilesets[])Enum.GetValues(typeof(GlobalDungeonData.ValidTilesets));
		for (int i = 0; i < manager.FloorRewardData.Count; i++)
		{
			FloorRewardData floorRewardData = manager.FloorRewardData[i];
			foreach (GlobalDungeonData.ValidTilesets validTilesets in array)
			{
				if ((floorRewardData.AssociatedTilesets & validTilesets) == validTilesets)
				{
					FloorRewardManifest floorRewardManifest = GenerateManifestForFloor(manager, floorRewardData);
					if (manager.SeededRunManifests.ContainsKey(validTilesets))
					{
						RegenerateManifest(manager, manager.SeededRunManifests[validTilesets]);
					}
				}
			}
		}
	}

	public static void ClearManifest(RewardManager manager)
	{
		manager.SeededRunManifests.Clear();
	}

	private static FloorRewardManifest GenerateManifestForFloor(RewardManager manager, FloorRewardData sourceData)
	{
		FloorRewardManifest floorRewardManifest = new FloorRewardManifest();
		floorRewardManifest.Initialize(manager);
		return floorRewardManifest;
	}

	private static void RegenerateManifest(RewardManager manager, FloorRewardManifest targetData)
	{
		targetData.Reinitialize(manager);
	}
}
