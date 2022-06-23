using System.Text;
using FullSerializer;
using UnityEngine;

[fsObject]
public class MonsterHuntProgress
{
	[fsIgnore]
	private static MonsterHuntData Data;

	[fsIgnore]
	public MonsterHuntQuest ActiveQuest;

	[fsProperty]
	public int CurrentActiveMonsterHuntID = -1;

	[fsProperty]
	public int CurrentActiveMonsterHuntProgress;

	[fsIgnore]
	private StringBuilder m_sb;

	public void OnLoaded()
	{
		if (Data == null)
		{
			Data = (MonsterHuntData)BraveResources.Load("Monster Hunt Data", ".asset");
		}
		if (CurrentActiveMonsterHuntID != -1)
		{
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.FRIFLE_CORE_HUNTS_COMPLETE) && GameStatsManager.Instance.GetFlag(GungeonFlags.FRIFLE_REWARD_GREY_MAUSER))
			{
				if (CurrentActiveMonsterHuntID < 0 || CurrentActiveMonsterHuntID >= Data.ProceduralQuests.Count)
				{
					CurrentActiveMonsterHuntID = -1;
					CurrentActiveMonsterHuntProgress = 0;
				}
				else
				{
					ActiveQuest = Data.ProceduralQuests[CurrentActiveMonsterHuntID];
				}
			}
			else if (CurrentActiveMonsterHuntID < 0 || CurrentActiveMonsterHuntID >= Data.OrderedQuests.Count)
			{
				CurrentActiveMonsterHuntID = -1;
				CurrentActiveMonsterHuntProgress = 0;
			}
			else
			{
				ActiveQuest = Data.OrderedQuests[CurrentActiveMonsterHuntID];
			}
		}
		else
		{
			CurrentActiveMonsterHuntProgress = 0;
		}
	}

	public int TriggerNextQuest()
	{
		int result = 0;
		if (ActiveQuest != null)
		{
			ActiveQuest.UnlockRewards();
			result = 5;
		}
		for (int i = 0; i < Data.OrderedQuests.Count; i++)
		{
			if (!GameStatsManager.Instance.GetFlag(Data.OrderedQuests[i].QuestFlag))
			{
				ActiveQuest = Data.OrderedQuests[i];
				CurrentActiveMonsterHuntID = i;
				CurrentActiveMonsterHuntProgress = 0;
				return result;
			}
		}
		int num = Random.Range(0, Data.ProceduralQuests.Count);
		ActiveQuest = Data.ProceduralQuests[num];
		CurrentActiveMonsterHuntID = num;
		CurrentActiveMonsterHuntProgress = 0;
		return result;
	}

	public void ProcessStatuesKill()
	{
		if (ActiveQuest != null && ActiveQuest.QuestFlag == GungeonFlags.FRIFLE_MONSTERHUNT_14_COMPLETE && CurrentActiveMonsterHuntProgress < ActiveQuest.NumberKillsRequired)
		{
			CurrentActiveMonsterHuntProgress++;
			if (CurrentActiveMonsterHuntProgress >= ActiveQuest.NumberKillsRequired)
			{
				Complete();
			}
		}
	}

	public void ForceIncrementKillCount()
	{
		if (ActiveQuest != null && CurrentActiveMonsterHuntProgress < ActiveQuest.NumberKillsRequired)
		{
			CurrentActiveMonsterHuntProgress++;
			if (CurrentActiveMonsterHuntProgress >= ActiveQuest.NumberKillsRequired)
			{
				Complete();
			}
		}
	}

	public void ProcessKill(AIActor target)
	{
		if (ActiveQuest != null && CurrentActiveMonsterHuntProgress < ActiveQuest.NumberKillsRequired && ActiveQuest.ContainsEnemy(target.EnemyGuid))
		{
			CurrentActiveMonsterHuntProgress++;
			if (CurrentActiveMonsterHuntProgress >= ActiveQuest.NumberKillsRequired)
			{
				Complete();
			}
		}
	}

	public string GetReplacementString()
	{
		return StringTableManager.GetEnemiesString(ActiveQuest.TargetStringKey);
	}

	public string GetDisplayString()
	{
		if (CurrentActiveMonsterHuntID < 0)
		{
			return string.Empty;
		}
		if (m_sb == null)
		{
			m_sb = new StringBuilder();
		}
		m_sb.Length = 0;
		string enemiesString = StringTableManager.GetEnemiesString(ActiveQuest.TargetStringKey);
		m_sb.Append(enemiesString);
		m_sb.Append(" ");
		m_sb.Append(CurrentActiveMonsterHuntProgress);
		m_sb.Append("/");
		m_sb.Append(ActiveQuest.NumberKillsRequired);
		return m_sb.ToString();
	}

	public bool IsQuestComplete()
	{
		return GameStatsManager.Instance.GetFlag(ActiveQuest.QuestFlag);
	}

	public void Complete()
	{
		GameStatsManager.Instance.SetFlag(ActiveQuest.QuestFlag, true);
		if (GameUIRoot.Instance != null && GameUIRoot.Instance.notificationController != null)
		{
			tk2dSprite component = (ResourceCache.Acquire("Global VFX/Frifle_VictoryIcon") as GameObject).GetComponent<tk2dSprite>();
			GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetString("#HUNT_COMPLETE_HEADER"), StringTableManager.GetString("#HUNT_COMPLETE_BODY"), component.Collection, component.spriteId, UINotificationController.NotificationColor.GOLD);
		}
	}
}
