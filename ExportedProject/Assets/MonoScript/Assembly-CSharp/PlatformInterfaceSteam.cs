using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Steamworks;
using UnityEngine;

public class PlatformInterfaceSteam : PlatformInterface
{
	private class AchievementData
	{
		public Achievement achievement;

		public string name;

		public string description;

		public bool isUnlocked;

		public bool hasProgressStat;

		public PlatformStat progressStat;

		public int goalValue;

		public int[] subgoals;

		private string m_cachedApiKey;

		public string ApiKey
		{
			get
			{
				if (m_cachedApiKey == null)
				{
					m_cachedApiKey = achievement.ToString();
				}
				return m_cachedApiKey;
			}
		}

		public AchievementData(Achievement achievement, string name, string desc)
		{
			this.achievement = achievement;
			this.name = name;
			description = desc;
			isUnlocked = false;
		}

		public AchievementData(Achievement achievement, string name, string desc, PlatformStat progressStat, int goalValue, params int[] subgoals)
		{
			this.achievement = achievement;
			this.name = name;
			description = desc;
			isUnlocked = false;
			hasProgressStat = true;
			this.progressStat = progressStat;
			this.goalValue = goalValue;
			this.subgoals = subgoals;
		}
	}

	private class StatData
	{
		public PlatformStat stat;

		public int value;

		private string m_cachedApiKey;

		public string ApiKey
		{
			get
			{
				if (m_cachedApiKey == null)
				{
					m_cachedApiKey = stat.ToString();
				}
				return m_cachedApiKey;
			}
		}

		public StatData(PlatformStat stat)
		{
			this.stat = stat;
		}
	}

	private class DlcData
	{
		public PlatformDlc dlc;

		public AppId_t appId;

		public DlcData(PlatformDlc dlc, AppId_t appId)
		{
			this.dlc = dlc;
			this.appId = appId;
		}
	}

	private CGameID m_GameID;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	protected Callback<UserStatsStored_t> m_UserStatsStored;

	protected Callback<UserAchievementStored_t> m_UserAchievementStored;

	protected Callback<DlcInstalled_t> m_DLCInstalled;

	private AchievementData[] m_achievements = new AchievementData[54]
	{
		new AchievementData(Achievement.COLLECT_FIVE_MASTERY_TOKENS, "Lead God", "Collect five Master Rounds in one run"),
		new AchievementData(Achievement.SPEND_META_CURRENCY, "Patron", "Spend big at the Acquisitions Department", PlatformStat.META_SPENT_AT_STORE, 100, 25, 50, 75),
		new AchievementData(Achievement.COMPLETE_GAME_WITH_ENCHANTED_GUN, "Gun Game", "Complete the game with the Sorceress's Enchanted Gun"),
		new AchievementData(Achievement.BEAT_FLOOR_SIX, "Gungeon Master", "Clear the Sixth Chamber"),
		new AchievementData(Achievement.BUILD_BULLET, "Gunsmith", "Construct the Bullet that can kill the Past"),
		new AchievementData(Achievement.BEAT_PAST_ALL, "Historian", "Complete all 4 main character Pasts"),
		new AchievementData(Achievement.BEAT_PAST_ROGUE, "Wingman", "Kill the Pilot's Past"),
		new AchievementData(Achievement.BEAT_PAST_CONVICT, "Double Jeopardy", "Kill the Convict's Past"),
		new AchievementData(Achievement.BEAT_PAST_MARINE, "Squad Captain", "Kill the Marine's Past"),
		new AchievementData(Achievement.BEAT_PAST_GUIDE, "Deadliest Game", "Kill the Hunter's Past"),
		new AchievementData(Achievement.BEAT_FLOOR_FIVE, "Slayer", "Defeat the Boss of the Fifth Chamber"),
		new AchievementData(Achievement.BEAT_FLOOR_ONE_MULTI, "Castle Crasher", "Clear the First Chamber 50 Times", PlatformStat.FLOOR_ONE_CLEARS, 50, 25),
		new AchievementData(Achievement.BEAT_FLOOR_TWO_MULTI, "Dungeon Diver", "Clear the Second Chamber 40 Times", PlatformStat.FLOOR_TWO_CLEARS, 40, 20),
		new AchievementData(Achievement.BEAT_FLOOR_THREE_MULTI, "Mine Master", "Clear the Third Chamber 30 Times", PlatformStat.FLOOR_THREE_CLEARS, 30, 15),
		new AchievementData(Achievement.BEAT_FLOOR_FOUR_MULTI, "Hollowed Out", "Clear the Fourth Chamber 20 Times", PlatformStat.FLOOR_FOUR_CLEARS, 20, 10),
		new AchievementData(Achievement.BEAT_FLOOR_FIVE_MULTI, "Forger", "Clear the Fifth Chamber 10 Times", PlatformStat.FLOOR_FIVE_CLEARS, 10, 5),
		new AchievementData(Achievement.HAVE_MANY_COINS, "Biggest Wallet", "Carry a large amount of money at once"),
		new AchievementData(Achievement.MAP_MAIN_FLOORS, "Cartographer's Assistant", "Map the first Five Chambers for the lost adventurer", PlatformStat.MAIN_FLOORS_MAPPED, 5, 1, 2, 3, 4),
		new AchievementData(Achievement.REACH_SEWERS, "Grate Hall", "Access the Oubliette"),
		new AchievementData(Achievement.REACH_CATHEDRAL, "Reverence for the Dead", "Access the Temple"),
		new AchievementData(Achievement.COMPLETE_GOLEM_ARM, "Re-Armed", "Deliver the Golem's replacement arm"),
		new AchievementData(Achievement.COMPLETE_FRIFLE_MULTI, "Weird Tale", "Complete Frifle's Challenges", PlatformStat.FRIFLE_CORE_COMPLETED, 15, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14),
		new AchievementData(Achievement.ACE_WINCHESTER_MULTI, "Trickshot", "Ace Winchester's game 3 times", PlatformStat.WINCHESTER_ACED, 3, 1, 2),
		new AchievementData(Achievement.COMPLETE_GUNSLING_MULTI, "Hedge Slinger", "Win a wager against the Gunsling King 5 times", PlatformStat.GUNSLING_COMPLETED, 5, 1, 2, 3, 4),
		new AchievementData(Achievement.UNLOCK_BULLET, "Case Closed", "Unlock the Bullet"),
		new AchievementData(Achievement.UNLOCK_ROBOT, "Beep", "Unlock the Robot"),
		new AchievementData(Achievement.UNLOCK_FLOOR_TWO_SHORTCUT, "Going Down", "Open the shortcut to the Second Chamber"),
		new AchievementData(Achievement.UNLOCK_FLOOR_THREE_SHORTCUT, "Going Downer", "Open the shortcut to the Third Chamber"),
		new AchievementData(Achievement.UNLOCK_FLOOR_FOUR_SHORTCUT, "Going Downest", "Open the shortcut to the Fourth Chamber"),
		new AchievementData(Achievement.UNLOCK_FLOOR_FIVE_SHORTCUT, "Last Stop", "Open the shortcut to the Fifth Chamber"),
		new AchievementData(Achievement.BEAT_MANFREDS_RIVAL, "Sworn Gun", "Avenge Manuel"),
		new AchievementData(Achievement.BEAT_TUTORIAL, "Gungeon Acolyte", "Complete the Tutorial"),
		new AchievementData(Achievement.POPULATE_BREACH, "Great Hall", "Populate the Breach", PlatformStat.BREACH_POPULATION, 12, 3, 6, 9),
		new AchievementData(Achievement.PREFIRE_ON_MIMIC, "Not Just A Box", "Get the jump on a Mimic"),
		new AchievementData(Achievement.KILL_FROZEN_ENEMY_WITH_ROLL, "Demolition Man", "Kill a frozen enemy by rolling into it"),
		new AchievementData(Achievement.PUSH_TABLE_INTO_PIT, "I Knew Someone Would Do It", "Why"),
		new AchievementData(Achievement.STEAL_MULTI, "Woodsie Lord", "Steal 10 things", PlatformStat.ITEMS_STOLEN, 10, 5),
		new AchievementData(Achievement.KILL_BOSS_WITH_GLITTER, "Day Ruiner", "Kill a boss after covering it with glitter"),
		new AchievementData(Achievement.FALL_IN_END_TIMES, "Lion Leap", "Fall at the last second"),
		new AchievementData(Achievement.KILL_WITH_CHANDELIER_MULTI, "Money Pit", "Kill 100 enemies by dropping chandeliers", PlatformStat.CHANDELIER_KILLS, 100, 25, 50, 75),
		new AchievementData(Achievement.KILL_IN_MINE_CART_MULTI, "Rider", "Kill 100 enemies while riding in a mine cart", PlatformStat.MINECART_KILLS, 100, 25, 50, 75),
		new AchievementData(Achievement.KILL_WITH_PITS_MULTI, "Pit Lord", "Kill 100 enemies by knocking them into pits", PlatformStat.PIT_KILLS, 100, 25, 50, 75),
		new AchievementData(Achievement.DIE_IN_PAST, "Time Paradox", "Die in the Past"),
		new AchievementData(Achievement.BEAT_A_JAMMED_BOSS, "Exorcist", "Kill a Jammed Boss"),
		new AchievementData(Achievement.REACH_BLACK_MARKET, "The Password", "Accessed the Hidden Market"),
		new AchievementData(Achievement.HAVE_MAX_CURSE, "Jammed", "You've met with a terrible fate, haven't you"),
		new AchievementData(Achievement.FLIP_TABLES_MULTI, "Rage Mode", "Always be flipping. Guns are for flippers", PlatformStat.TABLES_FLIPPED, 500, 100, 200, 300, 400),
		new AchievementData(Achievement.COMPLETE_GAME_WITH_BEAST_MODE, "Beast Master", "Complete the game with Beast Mode on"),
		new AchievementData(Achievement.BEAT_PAST_ROBOT, "Terminated", "Kill the Robot's Past"),
		new AchievementData(Achievement.BEAT_PAST_BULLET, "Hero of Gun", "Kill the Bullet's Past"),
		new AchievementData(Achievement.COMPLETE_GAME_WITH_CHALLENGE_MODE, "Challenger", "Complete Daisuke's trial"),
		new AchievementData(Achievement.BEAT_ADVANCED_DRAGUN, "Advanced Slayer", "Defeat an Advanced Boss"),
		new AchievementData(Achievement.BEAT_METAL_GEAR_RAT, "Resourceful", "Take Revenge"),
		new AchievementData(Achievement.COMPLETE_GAME_WITH_TURBO_MODE, "Sledge-Dog", "Complete Tonic's Challenge")
	};

	private StatData[] m_stats = new StatData[16]
	{
		new StatData(PlatformStat.META_SPENT_AT_STORE),
		new StatData(PlatformStat.FLOOR_ONE_CLEARS),
		new StatData(PlatformStat.FLOOR_TWO_CLEARS),
		new StatData(PlatformStat.FLOOR_THREE_CLEARS),
		new StatData(PlatformStat.FLOOR_FOUR_CLEARS),
		new StatData(PlatformStat.FLOOR_FIVE_CLEARS),
		new StatData(PlatformStat.MAIN_FLOORS_MAPPED),
		new StatData(PlatformStat.FRIFLE_CORE_COMPLETED),
		new StatData(PlatformStat.WINCHESTER_ACED),
		new StatData(PlatformStat.GUNSLING_COMPLETED),
		new StatData(PlatformStat.BREACH_POPULATION),
		new StatData(PlatformStat.ITEMS_STOLEN),
		new StatData(PlatformStat.CHANDELIER_KILLS),
		new StatData(PlatformStat.MINECART_KILLS),
		new StatData(PlatformStat.PIT_KILLS),
		new StatData(PlatformStat.TABLES_FLIPPED)
	};

	private DlcData[] m_dlc = new DlcData[2]
	{
		new DlcData(PlatformDlc.EARLY_MTX_GUN, (AppId_t)457842u),
		new DlcData(PlatformDlc.EARLY_COBALT_HAMMER, (AppId_t)457843u)
	};

	private readonly Dictionary<string, StringTableManager.GungeonSupportedLanguages> SteamDefaultLanguageToGungeonLanguage = new Dictionary<string, StringTableManager.GungeonSupportedLanguages>
	{
		{
			"english",
			StringTableManager.GungeonSupportedLanguages.ENGLISH
		},
		{
			"french",
			StringTableManager.GungeonSupportedLanguages.FRENCH
		},
		{
			"german",
			StringTableManager.GungeonSupportedLanguages.GERMAN
		},
		{
			"italian",
			StringTableManager.GungeonSupportedLanguages.ITALIAN
		},
		{
			"japanese",
			StringTableManager.GungeonSupportedLanguages.JAPANESE
		},
		{
			"korean",
			StringTableManager.GungeonSupportedLanguages.KOREAN
		},
		{
			"portuguese",
			StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE
		},
		{
			"brazilian",
			StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE
		},
		{
			"spanish",
			StringTableManager.GungeonSupportedLanguages.SPANISH
		},
		{
			"russian",
			StringTableManager.GungeonSupportedLanguages.RUSSIAN
		},
		{
			"polish",
			StringTableManager.GungeonSupportedLanguages.POLISH
		},
		{
			"chinese",
			StringTableManager.GungeonSupportedLanguages.CHINESE
		}
	};

	public static bool IsSteamBuild()
	{
		if (File.Exists(Path.Combine(Application.dataPath, "../steam_api64.dll")) || File.Exists(Path.Combine(Application.dataPath, "../steam_api.dll")))
		{
			return true;
		}
		return false;
	}

	protected override void OnStart()
	{
		Debug.Log("Starting Steam platform interface.");
		if (!SteamManager.Initialized)
		{
			return;
		}
		m_GameID = new CGameID(SteamUtils.GetAppID());
		UnlockedDlc.Clear();
		m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
		m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
		m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
		m_DLCInstalled = Callback<DlcInstalled_t>.Create(OnDlcInstalled);
		for (int i = 0; i < m_dlc.Length; i++)
		{
			if (SteamApps.BIsDlcInstalled(m_dlc[i].appId))
			{
				UnlockedDlc.Add(m_dlc[i].dlc);
			}
		}
		m_bRequestedStats = false;
		m_bStatsValid = false;
	}

	protected override void OnAchievementUnlock(Achievement achievement, int playerIndex)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		switch (achievement)
		{
		case Achievement.BEAT_FLOOR_ONE_MULTI:
		case Achievement.BEAT_FLOOR_TWO_MULTI:
		case Achievement.BEAT_FLOOR_THREE_MULTI:
		case Achievement.BEAT_FLOOR_FOUR_MULTI:
		case Achievement.BEAT_FLOOR_FIVE_MULTI:
		case Achievement.MAP_MAIN_FLOORS:
		case Achievement.ACE_WINCHESTER_MULTI:
		case Achievement.COMPLETE_GUNSLING_MULTI:
		case Achievement.POPULATE_BREACH:
		case Achievement.STEAL_MULTI:
		case Achievement.KILL_WITH_CHANDELIER_MULTI:
		case Achievement.KILL_IN_MINE_CART_MULTI:
		case Achievement.KILL_WITH_PITS_MULTI:
		case Achievement.FLIP_TABLES_MULTI:
			return;
		case Achievement.COMPLETE_FRIFLE_MULTI:
			SetStat(PlatformStat.FRIFLE_CORE_COMPLETED, 15);
			return;
		case Achievement.SPEND_META_CURRENCY:
			SetStat(PlatformStat.META_SPENT_AT_STORE, 100);
			return;
		}
		AchievementData achievementData = null;
		for (int i = 0; i < m_achievements.Length; i++)
		{
			if (m_achievements[i].achievement == achievement)
			{
				achievementData = m_achievements[i];
			}
		}
		if (achievementData != null)
		{
			achievementData.isUnlocked = true;
			SteamUserStats.SetAchievement(achievementData.ApiKey);
			m_bStoreStats = true;
		}
	}

	public override bool IsAchievementUnlocked(Achievement achievement)
	{
		AchievementData achievementData = null;
		for (int i = 0; i < m_achievements.Length; i++)
		{
			if (m_achievements[i].achievement == achievement)
			{
				achievementData = m_achievements[i];
			}
		}
		if (achievementData == null)
		{
			return false;
		}
		return achievementData.isUnlocked;
	}

	public override void SetStat(PlatformStat stat, int value)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		StatData statData = null;
		for (int i = 0; i < m_stats.Length; i++)
		{
			if (m_stats[i].stat == stat)
			{
				statData = m_stats[i];
			}
		}
		if (statData != null)
		{
			int value2 = statData.value;
			statData.value = value;
			SteamUserStats.SetStat(statData.ApiKey, value);
			MaybeShowProgress(stat, value2, value);
			MaybeStoreStats(stat, value2, value);
		}
	}

	public override void IncrementStat(PlatformStat stat, int delta)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		StatData statData = null;
		for (int i = 0; i < m_stats.Length; i++)
		{
			if (m_stats[i].stat == stat)
			{
				statData = m_stats[i];
			}
		}
		if (statData != null)
		{
			int value = statData.value;
			statData.value = value + delta;
			SteamUserStats.SetStat(statData.ApiKey, statData.value);
			MaybeShowProgress(stat, value, statData.value);
			MaybeStoreStats(stat, value, statData.value);
		}
	}

	private void MaybeShowProgress(PlatformStat stat, int prevValue, int newValue)
	{
		for (int i = 0; i < m_achievements.Length; i++)
		{
			AchievementData achievementData = m_achievements[i];
			if (!achievementData.hasProgressStat || achievementData.progressStat != stat || achievementData.subgoals == null || newValue >= achievementData.goalValue)
			{
				continue;
			}
			if (achievementData.progressStat == PlatformStat.BREACH_POPULATION && prevValue == 0 && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				break;
			}
			for (int num = achievementData.subgoals.Length - 1; num >= 0; num--)
			{
				int num2 = achievementData.subgoals[num];
				if (prevValue < num2 && newValue >= num2)
				{
					SteamUserStats.IndicateAchievementProgress(achievementData.ApiKey, (uint)newValue, (uint)achievementData.goalValue);
					return;
				}
			}
		}
	}

	private void MaybeStoreStats(PlatformStat stat, int prevValue, int newValue)
	{
		if ((stat == PlatformStat.META_SPENT_AT_STORE && prevValue < 100 && newValue >= 100) || (stat == PlatformStat.FLOOR_ONE_CLEARS && prevValue < 50 && newValue >= 50) || (stat == PlatformStat.FLOOR_TWO_CLEARS && prevValue < 40 && newValue >= 40) || (stat == PlatformStat.FLOOR_THREE_CLEARS && prevValue < 30 && newValue >= 30) || (stat == PlatformStat.FLOOR_FOUR_CLEARS && prevValue < 20 && newValue >= 20) || (stat == PlatformStat.FLOOR_FIVE_CLEARS && prevValue < 10 && newValue >= 10) || stat == PlatformStat.MAIN_FLOORS_MAPPED || stat == PlatformStat.FRIFLE_CORE_COMPLETED || stat == PlatformStat.WINCHESTER_ACED || stat == PlatformStat.GUNSLING_COMPLETED || stat == PlatformStat.BREACH_POPULATION || stat == PlatformStat.ITEMS_STOLEN || (stat == PlatformStat.CHANDELIER_KILLS && prevValue < 100 && newValue >= 100) || (stat == PlatformStat.MINECART_KILLS && prevValue < 100 && newValue >= 100) || (stat == PlatformStat.PIT_KILLS && prevValue < 100 && newValue >= 100) || (stat == PlatformStat.TABLES_FLIPPED && prevValue < 500 && newValue >= 500))
		{
			m_bStoreStats = true;
		}
	}

	public override void StoreStats()
	{
		m_bStoreStats = true;
	}

	public override void ResetStats(bool achievementsToo)
	{
		SteamUserStats.ResetAllStats(achievementsToo);
	}

	protected override void OnLateUpdate()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (!m_bRequestedStats)
		{
			if (!SteamManager.Initialized)
			{
				m_bRequestedStats = true;
				return;
			}
			bool flag = (m_bRequestedStats = SteamUserStats.RequestCurrentStats());
		}
		if (m_bStatsValid && m_bStoreStats)
		{
			bool flag2 = SteamUserStats.StoreStats();
			m_bStoreStats = !flag2;
		}
	}

	protected override StringTableManager.GungeonSupportedLanguages OnGetPreferredLanguage()
	{
		string steamUILanguage = SteamUtils.GetSteamUILanguage();
		if (SteamDefaultLanguageToGungeonLanguage.ContainsKey(steamUILanguage))
		{
			return SteamDefaultLanguageToGungeonLanguage[steamUILanguage];
		}
		return StringTableManager.GungeonSupportedLanguages.ENGLISH;
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialized || (ulong)m_GameID != pCallback.m_nGameID)
		{
			return;
		}
		if (pCallback.m_eResult == EResult.k_EResultOK)
		{
			Debug.Log("Received stats and achievements from Steam\n");
			m_bStatsValid = true;
			AchievementData[] achievements = m_achievements;
			foreach (AchievementData achievementData in achievements)
			{
				if (SteamUserStats.GetAchievement(achievementData.ApiKey, out achievementData.isUnlocked))
				{
					achievementData.name = SteamUserStats.GetAchievementDisplayAttribute(achievementData.ApiKey, "name");
					achievementData.description = SteamUserStats.GetAchievementDisplayAttribute(achievementData.ApiKey, "desc");
				}
				else
				{
					Debug.LogWarning(string.Concat("SteamUserStats.GetAchievement failed for Achievement ", achievementData.achievement, "\nIs it registered in the Steam Partner site?"));
				}
			}
			StatData[] stats = m_stats;
			foreach (StatData statData in stats)
			{
				if (!SteamUserStats.GetStat(statData.ApiKey, out statData.value))
				{
					Debug.LogWarning(string.Concat("SteamUserStats.GetStat failed for Stat ", statData.stat, "\nIs it registered in the Steam Partner site?"));
				}
			}
			CatchupAchievements();
		}
		else
		{
			Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
		}
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_eResult == EResult.k_EResultOK)
			{
				Debug.Log("StoreStats - success");
			}
			else if (pCallback.m_eResult == EResult.k_EResultInvalidParam)
			{
				Debug.Log("StoreStats - some failed to validate");
				UserStatsReceived_t pCallback2 = default(UserStatsReceived_t);
				pCallback2.m_eResult = EResult.k_EResultOK;
				pCallback2.m_nGameID = (ulong)m_GameID;
				OnUserStatsReceived(pCallback2);
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameID == pCallback.m_nGameID)
		{
			if (pCallback.m_nMaxProgress == 0)
			{
				Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
		}
	}

	private void OnDlcInstalled(DlcInstalled_t pCallback)
	{
		for (int i = 0; i < m_dlc.Length; i++)
		{
			if (m_dlc[i].appId == pCallback.m_nAppID)
			{
				UnlockedDlc.Add(m_dlc[i].dlc);
			}
		}
	}

	public void DebugPrintAchievements()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < m_achievements.Length; i++)
		{
			AchievementData achievementData = m_achievements[i];
			stringBuilder.AppendFormat("[{0}] {1}\n", (!achievementData.isUnlocked) ? " " : "X", achievementData.name);
			stringBuilder.AppendFormat("{0}\n", achievementData.description);
			if (achievementData.hasProgressStat)
			{
				StatData statData = Array.Find(m_stats, (StatData s) => s.stat == achievementData.progressStat);
				stringBuilder.AppendFormat("{0} of {1}\n", statData.value, achievementData.goalValue);
			}
			stringBuilder.AppendLine();
		}
		Debug.Log(stringBuilder.ToString());
	}
}
