using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameLevelDefinition
{
	public string dungeonSceneName;

	public string dungeonPrefabPath;

	public float priceMultiplier = 1f;

	public float secretDoorHealthMultiplier = 1f;

	public float enemyHealthMultiplier = 1f;

	public float damageCap = -1f;

	public float bossDpsCap = -1f;

	public List<DungeonFlowLevelEntry> flowEntries;

	public List<int> predefinedSeeds;

	[NonSerialized]
	public DungeonFlowLevelEntry lastSelectedFlowEntry;

	public DungeonFlowLevelEntry LovinglySelectDungeonFlow()
	{
		List<DungeonFlowLevelEntry> list = new List<DungeonFlowLevelEntry>();
		float num = 0f;
		List<DungeonFlowLevelEntry> list2 = new List<DungeonFlowLevelEntry>();
		float num2 = 0f;
		for (int i = 0; i < flowEntries.Count; i++)
		{
			bool flag = true;
			for (int j = 0; j < flowEntries[i].prerequisites.Length; j++)
			{
				if (!flowEntries[i].prerequisites[j].CheckConditionsFulfilled())
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			if (GameStatsManager.Instance.QueryFlowDifferentiator(flowEntries[i].flowPath) > 0)
			{
				num2 += flowEntries[i].weight;
				list2.Add(flowEntries[i]);
				continue;
			}
			if (flowEntries[i].forceUseIfAvailable)
			{
				return flowEntries[i];
			}
			num += flowEntries[i].weight;
			list.Add(flowEntries[i]);
		}
		if (list.Count <= 0 && list2.Count > 0)
		{
			list = list2;
			num = num2;
		}
		if (list.Count <= 0)
		{
			return null;
		}
		float num3 = UnityEngine.Random.value * num;
		float num4 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			num4 += list[k].weight;
			if (num4 >= num3)
			{
				return list[k];
			}
		}
		return null;
	}
}
