using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class SemioticDungeonGenSettings
	{
		[SerializeField]
		public List<DungeonFlow> flows;

		[SerializeField]
		public List<ExtraIncludedRoomData> mandatoryExtraRooms;

		[SerializeField]
		public List<ExtraIncludedRoomData> optionalExtraRooms;

		[SerializeField]
		public int MAX_GENERATION_ATTEMPTS = 25;

		[SerializeField]
		public bool DEBUG_RENDER_CANVASES_SEPARATELY;

		public DungeonFlow GetRandomFlow()
		{
			if (GameManager.Instance.BestGenerationDungeonPrefab != null && GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON && !GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_MET_PREVIOUSLY))
			{
				return flows[0];
			}
			float num = 0f;
			List<DungeonFlow> list = new List<DungeonFlow>();
			float num2 = 0f;
			List<DungeonFlow> list2 = new List<DungeonFlow>();
			for (int i = 0; i < flows.Count; i++)
			{
				if (GameStatsManager.Instance.QueryFlowDifferentiator(flows[i].name) > 0)
				{
					num += 1f;
					list.Add(flows[i]);
				}
				else
				{
					num2 += 1f;
					list2.Add(flows[i]);
				}
			}
			if (list2.Count <= 0 && list.Count > 0)
			{
				list2 = list;
				num2 = num;
			}
			if (list2.Count <= 0)
			{
				return null;
			}
			float num3 = BraveRandom.GenerationRandomValue() * num2;
			float num4 = 0f;
			for (int j = 0; j < list2.Count; j++)
			{
				num4 += 1f;
				if (num4 >= num3)
				{
					return list2[j];
				}
			}
			return flows[BraveRandom.GenerationRandomRange(0, flows.Count)];
		}
	}
}
