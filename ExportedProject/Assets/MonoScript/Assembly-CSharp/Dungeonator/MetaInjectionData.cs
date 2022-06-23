using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeonator
{
	public class MetaInjectionData : ScriptableObject
	{
		public static bool CastleExcludedSetUsed = false;

		public static bool BlueprintGenerated = false;

		public static Dictionary<GlobalDungeonData.ValidTilesets, List<RuntimeInjectionMetadata>> CurrentRunBlueprint;

		public static List<ProceduralFlowModifierData> InjectionSetsUsedThisRun = new List<ProceduralFlowModifierData>();

		public static Dictionary<GlobalDungeonData.ValidTilesets, int> KeybulletsAssignedToFloors = new Dictionary<GlobalDungeonData.ValidTilesets, int>();

		public static Dictionary<GlobalDungeonData.ValidTilesets, int> ChanceBulletsAssignedToFloors = new Dictionary<GlobalDungeonData.ValidTilesets, int>();

		public static Dictionary<GlobalDungeonData.ValidTilesets, int> WallMimicsAssignedToFloors = new Dictionary<GlobalDungeonData.ValidTilesets, int>();

		public static bool ForceEarlyChest = false;

		public static bool CellGeneratedForCurrentBlueprint = false;

		public List<MetaInjectionDataEntry> entries;

		public static void ClearBlueprint()
		{
			BlueprintGenerated = false;
			CastleExcludedSetUsed = false;
			if (CurrentRunBlueprint != null)
			{
				CurrentRunBlueprint.Clear();
			}
			CellGeneratedForCurrentBlueprint = false;
			if (InjectionSetsUsedThisRun != null)
			{
				InjectionSetsUsedThisRun.Clear();
			}
			KeybulletsAssignedToFloors.Clear();
			ChanceBulletsAssignedToFloors.Clear();
			WallMimicsAssignedToFloors.Clear();
		}

		public static int GetNumKeybulletMenForFloor(GlobalDungeonData.ValidTilesets tileset)
		{
			if (KeybulletsAssignedToFloors.ContainsKey(tileset))
			{
				return KeybulletsAssignedToFloors[tileset];
			}
			return 0;
		}

		public static int GetNumChanceBulletMenForFloor(GlobalDungeonData.ValidTilesets tileset)
		{
			if (ChanceBulletsAssignedToFloors.ContainsKey(tileset))
			{
				return ChanceBulletsAssignedToFloors[tileset];
			}
			return 0;
		}

		public static int GetNumWallMimicsForFloor(GlobalDungeonData.ValidTilesets tileset)
		{
			if (WallMimicsAssignedToFloors.ContainsKey(tileset))
			{
				return WallMimicsAssignedToFloors[tileset];
			}
			return 0;
		}

		public void PreprocessRun(bool doDebug = false)
		{
			if (CurrentRunBlueprint == null)
			{
				CurrentRunBlueprint = new Dictionary<GlobalDungeonData.ValidTilesets, List<RuntimeInjectionMetadata>>();
			}
			CurrentRunBlueprint.Clear();
			KeybulletsAssignedToFloors.Clear();
			ChanceBulletsAssignedToFloors.Clear();
			WallMimicsAssignedToFloors.Clear();
			RewardManager rewardManager = GameManager.Instance.RewardManager;
			rewardManager.KeybulletsChances.Select("keybulletmen", KeybulletsAssignedToFloors);
			rewardManager.ChanceBulletChances.Select("chancebulletmen", ChanceBulletsAssignedToFloors);
			rewardManager.WallMimicChances.Select("wallmimics", WallMimicsAssignedToFloors);
			GlobalDungeonData.ValidTilesets[] array = Enum.GetValues(typeof(GlobalDungeonData.ValidTilesets)) as GlobalDungeonData.ValidTilesets[];
			for (int i = 0; i < entries.Count; i++)
			{
				float modifiedChanceToTrigger = entries[i].ModifiedChanceToTrigger;
				if (modifiedChanceToTrigger < 1f && UnityEngine.Random.value > modifiedChanceToTrigger)
				{
					continue;
				}
				int num = BraveRandom.GenerationRandomRange(entries[i].MinToAppearPerRun, entries[i].MaxToAppearPerRun + 1);
				if (entries[i].UsesWeightedNumberToAppearPerRun)
				{
					num = entries[i].WeightedNumberToAppear.SelectByWeight();
				}
				List<GlobalDungeonData.ValidTilesets> list = new List<GlobalDungeonData.ValidTilesets>();
				for (int j = 0; j < array.Length; j++)
				{
					if ((!entries[i].IsPartOfExcludedCastleSet || !CastleExcludedSetUsed || array[j] != GlobalDungeonData.ValidTilesets.CASTLEGEON) && (entries[i].validTilesets | array[j]) == entries[i].validTilesets)
					{
						list.Add(array[j]);
					}
				}
				List<int> input = Enumerable.Range(0, list.Count).ToList();
				input = input.GenerationShuffle();
				for (int k = 0; k < num; k++)
				{
					GlobalDungeonData.ValidTilesets validTilesets = list[input[k]];
					if (!CurrentRunBlueprint.ContainsKey(validTilesets))
					{
						CurrentRunBlueprint.Add(validTilesets, new List<RuntimeInjectionMetadata>());
					}
					if (validTilesets == GlobalDungeonData.ValidTilesets.CASTLEGEON && entries[i].IsPartOfExcludedCastleSet)
					{
						CastleExcludedSetUsed = true;
					}
					CurrentRunBlueprint[validTilesets].Add(new RuntimeInjectionMetadata(entries[i].injectionData));
				}
				if (entries[i].AllowBonusSecret && num < input.Count && BraveRandom.GenerationRandomValue() < entries[i].ChanceForBonusSecret)
				{
					GlobalDungeonData.ValidTilesets key = list[input[num]];
					if (!CurrentRunBlueprint.ContainsKey(key))
					{
						CurrentRunBlueprint.Add(key, new List<RuntimeInjectionMetadata>());
					}
					RuntimeInjectionMetadata runtimeInjectionMetadata = new RuntimeInjectionMetadata(entries[i].injectionData);
					runtimeInjectionMetadata.forceSecret = true;
					CurrentRunBlueprint[key].Add(runtimeInjectionMetadata);
				}
			}
			if (GameStatsManager.Instance.isChump)
			{
				ForceEarlyChest = false;
			}
			else if (UnityEngine.Random.value < rewardManager.EarlyChestChanceIfNotChump)
			{
				ForceEarlyChest = true;
			}
			BlueprintGenerated = true;
		}
	}
}
