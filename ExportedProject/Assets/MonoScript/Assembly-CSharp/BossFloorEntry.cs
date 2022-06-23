using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossFloorEntry
{
	public string Annotation;

	[EnumFlags]
	public GlobalDungeonData.ValidTilesets AssociatedTilesets;

	[SerializeField]
	public List<IndividualBossFloorEntry> Bosses;

	public IndividualBossFloorEntry SelectBoss()
	{
		List<IndividualBossFloorEntry> list = new List<IndividualBossFloorEntry>();
		float num = 0f;
		for (int i = 0; i < Bosses.Count; i++)
		{
			if (Bosses[i].GlobalPrereqsValid())
			{
				list.Add(Bosses[i]);
				Debug.LogWarning("Adding valid boss: " + Bosses[i].TargetRoomTable.name + "|" + Bosses[i].GetWeightModifier());
				num += Bosses[i].GetWeightModifier() * Bosses[i].BossWeight;
			}
		}
		float num2 = BraveRandom.GenerationRandomValue() * num;
		float num3 = 0f;
		for (int j = 0; j < list.Count; j++)
		{
			num3 += Bosses[j].GetWeightModifier() * list[j].BossWeight;
			if (num3 >= num2)
			{
				Debug.LogWarning("Returning valid boss: " + list[j].TargetRoomTable.name + "|" + list[j].GetWeightModifier());
				return list[j];
			}
		}
		Debug.LogWarning("Returning fallback boss boss: " + list[list.Count - 1].TargetRoomTable.name + "|" + list[list.Count - 1].GetWeightModifier());
		return list[list.Count - 1];
	}
}
