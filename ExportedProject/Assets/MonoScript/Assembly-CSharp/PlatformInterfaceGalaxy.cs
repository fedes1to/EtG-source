using System;
using System.IO;
using System.Text;
using Galaxy.Api;
using UnityEngine;

public class PlatformInterfaceGalaxy : PlatformInterface
{
	public class AchievementsReceivedListener : GlobalUserStatsAndAchievementsRetrieveListener
	{
		private PlatformInterfaceGalaxy m_platformInterface;

		public AchievementsReceivedListener(PlatformInterfaceGalaxy platformInterface)
		{
			m_platformInterface = platformInterface;
		}

		public override void OnUserStatsAndAchievementsRetrieveSuccess(GalaxyID userID)
		{
			Debug.Log("Received achievement data!");
			m_platformInterface.OnUserStatsReceived();
			m_platformInterface.CatchupAchievements();
		}

		public override void OnUserStatsAndAchievementsRetrieveFailure(GalaxyID userID, FailureReason failureReason)
		{
			Debug.LogErrorFormat("OnUserStatsAndAchievementsRetrieveFailure() Error: {0} ", failureReason);
		}
	}

	public class StatsStoredListener : GlobalStatsAndAchievementsStoreListener
	{
		public override void OnUserStatsAndAchievementsStoreSuccess()
		{
			Debug.Log("Stats and achievements stored!");
		}

		public override void OnUserStatsAndAchievementsStoreFailure(FailureReason failureReason)
		{
			Debug.LogErrorFormat("OnUserStatsAndAchievementsStoreFailure() Error: {0} ", failureReason);
		}
	}

	private class AchievementData
	{
		public Achievement achievement;

		public string name;

		public string description;

		public bool isUnlocked;

		public uint unlockTime;

		public bool hasProgressStat;

		public PlatformStat progressStat;

		public int goalValue;

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

		public AchievementData(Achievement achievement, string name, string desc, PlatformStat progressStat, int goalValue)
		{
			this.achievement = achievement;
			this.name = name;
			description = desc;
			isUnlocked = false;
			hasProgressStat = true;
			this.progressStat = progressStat;
			this.goalValue = goalValue;
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

	private bool m_isInitialized;

	private bool m_bRequestedStats;

	private bool m_bStatsValid;

	private bool m_bStoreStats;

	private AchievementsReceivedListener m_achievementsReceivedListener;

	private StatsStoredListener m_storeStatsCallback;

	private AchievementData[] m_achievements = new AchievementData[54]
	{
		new AchievementData(Achievement.COLLECT_FIVE_MASTERY_TOKENS, "Lead God", "Collect five Master Rounds in one run"),
		new AchievementData(Achievement.SPEND_META_CURRENCY, "Patron", "Spend big at the Acquisitions Department", PlatformStat.META_SPENT_AT_STORE, 100),
		new AchievementData(Achievement.COMPLETE_GAME_WITH_ENCHANTED_GUN, "Gun Game", "Complete the game with the Sorceress's Enchanted Gun"),
		new AchievementData(Achievement.BEAT_FLOOR_SIX, "Gungeon Master", "Clear the Sixth Chamber"),
		new AchievementData(Achievement.BUILD_BULLET, "Gunsmith", "Construct the Bullet that can kill the Past"),
		new AchievementData(Achievement.BEAT_PAST_ALL, "Historian", "Complete all 4 main character Pasts"),
		new AchievementData(Achievement.BEAT_PAST_ROGUE, "Wingman", "Kill the Pilot's Past"),
		new AchievementData(Achievement.BEAT_PAST_CONVICT, "Double Jeopardy", "Kill the Convict's Past"),
		new AchievementData(Achievement.BEAT_PAST_MARINE, "Squad Captain", "Kill the Marine's Past"),
		new AchievementData(Achievement.BEAT_PAST_GUIDE, "Deadliest Game", "Kill the Hunter's Past"),
		new AchievementData(Achievement.BEAT_FLOOR_FIVE, "Slayer", "Defeat the Boss of the Fifth Chamber"),
		new AchievementData(Achievement.BEAT_FLOOR_ONE_MULTI, "Castle Crasher", "Clear the First Chamber 50 Times", PlatformStat.FLOOR_ONE_CLEARS, 50),
		new AchievementData(Achievement.BEAT_FLOOR_TWO_MULTI, "Dungeon Diver", "Clear the Second Chamber 40 Times", PlatformStat.FLOOR_TWO_CLEARS, 40),
		new AchievementData(Achievement.BEAT_FLOOR_THREE_MULTI, "Mine Master", "Clear the Third Chamber 30 Times", PlatformStat.FLOOR_THREE_CLEARS, 30),
		new AchievementData(Achievement.BEAT_FLOOR_FOUR_MULTI, "Hollowed Out", "Clear the Fourth Chamber 20 Times", PlatformStat.FLOOR_FOUR_CLEARS, 20),
		new AchievementData(Achievement.BEAT_FLOOR_FIVE_MULTI, "Forger", "Clear the Fifth Chamber 10 Times", PlatformStat.FLOOR_FIVE_CLEARS, 10),
		new AchievementData(Achievement.HAVE_MANY_COINS, "Biggest Wallet", "Carry a large amount of money at once"),
		new AchievementData(Achievement.MAP_MAIN_FLOORS, "Cartographer's Assistant", "Map the first Five Chambers for the lost adventurer", PlatformStat.MAIN_FLOORS_MAPPED, 5),
		new AchievementData(Achievement.REACH_SEWERS, "Grate Hall", "Access the Oubliette"),
		new AchievementData(Achievement.REACH_CATHEDRAL, "Reverence for the Dead", "Access the Temple"),
		new AchievementData(Achievement.COMPLETE_GOLEM_ARM, "Re-Armed", "Deliver the Golem's replacement arm"),
		new AchievementData(Achievement.COMPLETE_FRIFLE_MULTI, "Weird Tale", "Complete Frifle's Challenges", PlatformStat.FRIFLE_CORE_COMPLETED, 15),
		new AchievementData(Achievement.ACE_WINCHESTER_MULTI, "Trickshot", "Ace Winchester's game 3 times", PlatformStat.WINCHESTER_ACED, 3),
		new AchievementData(Achievement.COMPLETE_GUNSLING_MULTI, "Hedge Slinger", "Win a wager against the Gunsling King 5 times", PlatformStat.GUNSLING_COMPLETED, 5),
		new AchievementData(Achievement.UNLOCK_BULLET, "Case Closed", "Unlock the Bullet"),
		new AchievementData(Achievement.UNLOCK_ROBOT, "Beep", "Unlock the Robot"),
		new AchievementData(Achievement.UNLOCK_FLOOR_TWO_SHORTCUT, "Going Down", "Open the shortcut to the Second Chamber"),
		new AchievementData(Achievement.UNLOCK_FLOOR_THREE_SHORTCUT, "Going Downer", "Open the shortcut to the Third Chamber"),
		new AchievementData(Achievement.UNLOCK_FLOOR_FOUR_SHORTCUT, "Going Downest", "Open the shortcut to the Fourth Chamber"),
		new AchievementData(Achievement.UNLOCK_FLOOR_FIVE_SHORTCUT, "Last Stop", "Open the shortcut to the Fifth Chamber"),
		new AchievementData(Achievement.BEAT_MANFREDS_RIVAL, "Sworn Gun", "Avenge Manuel"),
		new AchievementData(Achievement.BEAT_TUTORIAL, "Gungeon Acolyte", "Complete the Tutorial"),
		new AchievementData(Achievement.POPULATE_BREACH, "Great Hall", "Populate the Breach", PlatformStat.BREACH_POPULATION, 12),
		new AchievementData(Achievement.PREFIRE_ON_MIMIC, "Not Just A Box", "Get the jump on a Mimic"),
		new AchievementData(Achievement.KILL_FROZEN_ENEMY_WITH_ROLL, "Demolition Man", "Kill a frozen enemy by rolling into it"),
		new AchievementData(Achievement.PUSH_TABLE_INTO_PIT, "I Knew Someone Would Do It", "Why"),
		new AchievementData(Achievement.STEAL_MULTI, "Woodsie Lord", "Steal 10 things", PlatformStat.ITEMS_STOLEN, 10),
		new AchievementData(Achievement.KILL_BOSS_WITH_GLITTER, "Day Ruiner", "Kill a boss after covering it with glitter"),
		new AchievementData(Achievement.FALL_IN_END_TIMES, "Lion Leap", "Fall at the last second"),
		new AchievementData(Achievement.KILL_WITH_CHANDELIER_MULTI, "Money Pit", "Kill 100 enemies by dropping chandeliers", PlatformStat.CHANDELIER_KILLS, 100),
		new AchievementData(Achievement.KILL_IN_MINE_CART_MULTI, "Rider", "Kill 100 enemies while riding in a mine cart", PlatformStat.MINECART_KILLS, 100),
		new AchievementData(Achievement.KILL_WITH_PITS_MULTI, "Pit Lord", "Kill 100 enemies by knocking them into pits", PlatformStat.PIT_KILLS, 100),
		new AchievementData(Achievement.DIE_IN_PAST, "Time Paradox", "Die in the Past"),
		new AchievementData(Achievement.BEAT_A_JAMMED_BOSS, "Exorcist", "Kill a Jammed Boss"),
		new AchievementData(Achievement.REACH_BLACK_MARKET, "The Password", "Accessed the Hidden Market"),
		new AchievementData(Achievement.HAVE_MAX_CURSE, "Jammed", "You've met with a terrible fate, haven't you"),
		new AchievementData(Achievement.FLIP_TABLES_MULTI, "Rage Mode", "Always be flipping. Guns are for flippers", PlatformStat.TABLES_FLIPPED, 500),
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

	public static bool IsGalaxyBuild()
	{
		if (File.Exists(Path.Combine(Application.dataPath, "../Galaxy.dll")))
		{
			return true;
		}
		return false;
	}

	protected override void OnStart()
	{
		Debug.Log("Starting GOG Galaxy platform interface.");
	}

	protected override void OnAchievementUnlock(Achievement achievement, int playerIndex)
	{
		if (!GalaxyManager.Initialized || !m_isInitialized)
		{
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
			try
			{
				GalaxyInstance.Stats().SetAchievement(achievementData.ApiKey);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
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
		if (!GalaxyManager.Initialized || !m_isInitialized)
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
		if (statData == null || value <= statData.value)
		{
			return;
		}
		statData.value = value;
		try
		{
			GalaxyInstance.Stats().SetStatInt(statData.ApiKey, value);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public override void IncrementStat(PlatformStat stat, int delta)
	{
		if (!GalaxyManager.Initialized || !m_isInitialized)
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
		if (statData == null)
		{
			return;
		}
		int value = statData.value;
		statData.value = value + delta;
		try
		{
			GalaxyInstance.Stats().SetStatInt(statData.ApiKey, statData.value);
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public override void StoreStats()
	{
		m_bStoreStats = true;
	}

	public override void ResetStats(bool achievementsToo)
	{
		try
		{
			GalaxyInstance.Stats().ResetStatsAndAchievements();
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		m_bRequestedStats = false;
		m_bStatsValid = false;
	}

	protected override void OnLateUpdate()
	{
		if (!GalaxyManager.Initialized)
		{
			return;
		}
		if (GalaxyManager.Initialized && !m_isInitialized)
		{
			m_achievementsReceivedListener = new AchievementsReceivedListener(this);
			m_storeStatsCallback = new StatsStoredListener();
			m_isInitialized = true;
			return;
		}
		if (!m_bRequestedStats)
		{
			if (!GalaxyManager.Initialized)
			{
				m_bRequestedStats = true;
				return;
			}
			try
			{
				GalaxyInstance.Stats().RequestUserStatsAndAchievements();
				m_bRequestedStats = true;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		if (!m_bStatsValid || !m_bStoreStats)
		{
			return;
		}
		try
		{
			GalaxyInstance.Stats().StoreStatsAndAchievements();
			m_bStoreStats = false;
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
		}
	}

	protected override StringTableManager.GungeonSupportedLanguages OnGetPreferredLanguage()
	{
		return StringTableManager.GungeonSupportedLanguages.ENGLISH;
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

	public void OnUserStatsReceived()
	{
		Debug.Log("Received stats and achievements from Galaxy\n");
		m_bStatsValid = true;
		AchievementData[] achievements = m_achievements;
		foreach (AchievementData achievementData in achievements)
		{
			try
			{
				GalaxyInstance.Stats().GetAchievement(achievementData.ApiKey, ref achievementData.isUnlocked, ref achievementData.unlockTime);
				achievementData.name = GalaxyInstance.Stats().GetAchievementDisplayName(achievementData.ApiKey);
				achievementData.description = GalaxyInstance.Stats().GetAchievementDescription(achievementData.ApiKey);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		StatData[] stats = m_stats;
		foreach (StatData statData in stats)
		{
			try
			{
				statData.value = GalaxyInstance.Stats().GetStatInt(statData.ApiKey);
			}
			catch (Exception message2)
			{
				Debug.LogError(message2);
			}
		}
	}
}
