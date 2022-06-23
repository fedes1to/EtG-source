using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BonusEnemySpawns
{
	public DungeonPrerequisite[] Prereqs;

	[EnemyIdentifier]
	public string EnemyGuid;

	public WeightedIntCollection NumSpawnedChances;

	public float CastleChance = 0.2f;

	public float SewerChance;

	public float GungeonChance = 0.175f;

	public float CathedralChance;

	public float MinegeonChance = 0.15f;

	public float CatacombgeonChance = 0.125f;

	public float ForgegeonChance = 0.1f;

	public float BulletHellChance;

	public void Select(string name, Dictionary<GlobalDungeonData.ValidTilesets, int> numAssignedToFloors)
	{
		if (!DungeonPrerequisite.CheckConditionsFulfilled(Prereqs))
		{
			return;
		}
		int num = NumSpawnedChances.SelectByWeight();
		float num2 = CastleChance;
		float num3 = SewerChance;
		float num4 = GungeonChance;
		float num5 = CathedralChance;
		float num6 = MinegeonChance;
		float num7 = CatacombgeonChance;
		float num8 = ForgegeonChance;
		float num9 = BulletHellChance;
		for (int i = 0; i < num; i++)
		{
			float num10 = UnityEngine.Random.value * (num2 + num3 + num4 + num5 + num6 + num7 + num8 + num9);
			GlobalDungeonData.ValidTilesets validTilesets = GlobalDungeonData.ValidTilesets.CASTLEGEON;
			if (num10 < num2)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.CASTLEGEON;
				num2 = 0.05f;
			}
			else if (num10 < num2 + num3)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.SEWERGEON;
				num3 = 0.05f;
			}
			else if (num10 < num2 + num3 + num4)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.GUNGEON;
				num4 = 0.05f;
			}
			else if (num10 < num2 + num3 + num4 + num5)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.CATHEDRALGEON;
				num5 = 0.05f;
			}
			else if (num10 < num2 + num3 + num4 + num5 + num6)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.MINEGEON;
				num6 = 0.05f;
			}
			else if (num10 < num2 + num3 + num4 + num5 + num6 + num7)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.CATACOMBGEON;
				num7 = 0.05f;
			}
			else if (num10 < num2 + num3 + num4 + num5 + num6 + num7 + num8)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.FORGEGEON;
				num8 = 0.05f;
			}
			else
			{
				validTilesets = GlobalDungeonData.ValidTilesets.HELLGEON;
				num9 = 0.05f;
			}
			if (numAssignedToFloors.ContainsKey(validTilesets))
			{
				numAssignedToFloors[validTilesets]++;
			}
			else
			{
				numAssignedToFloors.Add(validTilesets, 1);
			}
		}
	}
}
