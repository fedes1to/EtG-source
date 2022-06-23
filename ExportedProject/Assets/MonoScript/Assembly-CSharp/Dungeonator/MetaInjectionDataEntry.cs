using System;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class MetaInjectionDataEntry
	{
		public SharedInjectionData injectionData;

		public float OverallChanceToTrigger = 1f;

		public bool UsesUnlockedChanceToTrigger;

		public MetaInjectionUnlockedChanceEntry[] UnlockedChancesToTrigger;

		public int MinToAppearPerRun;

		public int MaxToAppearPerRun = 2;

		public bool UsesWeightedNumberToAppearPerRun;

		public WeightedIntCollection WeightedNumberToAppear;

		public bool AllowBonusSecret;

		[ShowInInspectorIf("AllowBonusSecret", false)]
		public float ChanceForBonusSecret = 0.5f;

		public bool IsPartOfExcludedCastleSet;

		[EnumFlags]
		public GlobalDungeonData.ValidTilesets validTilesets;

		public float ModifiedChanceToTrigger
		{
			get
			{
				if (UsesUnlockedChanceToTrigger && GameStatsManager.HasInstance)
				{
					for (int num = UnlockedChancesToTrigger.Length - 1; num >= 0; num--)
					{
						MetaInjectionUnlockedChanceEntry metaInjectionUnlockedChanceEntry = UnlockedChancesToTrigger[num];
						bool flag = true;
						for (int i = 0; i < metaInjectionUnlockedChanceEntry.prerequisites.Length; i++)
						{
							if (!metaInjectionUnlockedChanceEntry.prerequisites[i].CheckConditionsFulfilled())
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							Debug.LogError("chance to trigger: " + num + "|" + metaInjectionUnlockedChanceEntry.ChanceToTrigger);
							return metaInjectionUnlockedChanceEntry.ChanceToTrigger;
						}
					}
				}
				return OverallChanceToTrigger;
			}
		}
	}
}
