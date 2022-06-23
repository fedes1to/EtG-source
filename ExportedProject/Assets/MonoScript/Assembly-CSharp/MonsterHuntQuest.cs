using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterHuntQuest
{
	[SerializeField]
	[LongEnum]
	public GungeonFlags QuestFlag;

	[SerializeField]
	public string QuestIntroString;

	[SerializeField]
	public string TargetStringKey;

	[EnemyIdentifier]
	[SerializeField]
	public List<string> ValidTargetMonsterGuids = new List<string>();

	[SerializeField]
	public int NumberKillsRequired;

	[SerializeField]
	[LongEnum]
	public List<GungeonFlags> FlagsToSetUponReward;

	public bool IsQuestComplete()
	{
		return GameStatsManager.Instance.GetFlag(QuestFlag);
	}

	public bool ContainsEnemy(string enemyGuid)
	{
		for (int i = 0; i < ValidTargetMonsterGuids.Count; i++)
		{
			if (ValidTargetMonsterGuids[i] == enemyGuid)
			{
				return true;
			}
		}
		return false;
	}

	public void UnlockRewards()
	{
		for (int i = 0; i < FlagsToSetUponReward.Count; i++)
		{
			GameStatsManager.Instance.SetFlag(FlagsToSetUponReward[i], true);
		}
	}
}
