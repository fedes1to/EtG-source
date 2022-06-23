using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public abstract class PlatformInterface
{
	private struct LightFXUnit
	{
		public Color32 TargetLightColor;

		public float remainingDuration;

		public float startDuration;

		public LightFXUnit(Color32 sourceColor, float sourceDuration)
		{
			TargetLightColor = sourceColor;
			remainingDuration = sourceDuration;
			startDuration = sourceDuration;
		}
	}

	private class DlcUnlockedItem
	{
		public PlatformDlc PlatformDlc;

		public string encounterGuid;

		public GungeonFlags flag;

		public DlcUnlockedItem(PlatformDlc PlatformDlc, string encounterGuid, GungeonFlags flag = GungeonFlags.NONE)
		{
			this.PlatformDlc = PlatformDlc;
			this.encounterGuid = encounterGuid;
			this.flag = flag;
		}
	}

	public List<PlatformDlc> UnlockedDlc = new List<PlatformDlc>();

	public static float LastManyCoinsUnlockTime = 0f;

	private float m_lastDlcCheckTime;

	private bool m_hasCaughtUpAchievements;

	private static bool m_useLightFX = false;

	private static List<LightFXUnit> m_AlienFXExtantEffects = new List<LightFXUnit>();

	private static LightFXUnit m_AlienFXAmbientEffect;

	private DlcUnlockedItem[] m_dlcUnlockedItems = new DlcUnlockedItem[2]
	{
		new DlcUnlockedItem(PlatformDlc.EARLY_MTX_GUN, "5c2241fc117740d59ad8e29f5324b773", GungeonFlags.BLUEPRINTMETA_MTXGUN),
		new DlcUnlockedItem(PlatformDlc.EARLY_COBALT_HAMMER, "2d91904ba70a4c0d861dac03a6417591")
	};

	private bool m_hasCheckedForGalaxyMtx;

	public void Start()
	{
		OnStart();
		InitializeAlienFXController();
	}

	public virtual void SignIn()
	{
	}

	public void AchievementUnlock(Achievement achievement, int playerIndex = 0)
	{
		SetGungeonFlagForAchievement(achievement);
		OnAchievementUnlock(achievement, playerIndex);
	}

	public virtual bool IsAchievementUnlocked(Achievement achievement)
	{
		return false;
	}

	public virtual void SetStat(PlatformStat stat, int value)
	{
	}

	public virtual void IncrementStat(PlatformStat stat, int delta)
	{
	}

	public virtual void SendEvent(string eventName, int value)
	{
	}

	public virtual void SetPresence(string presence)
	{
	}

	public virtual void StoreStats()
	{
	}

	public virtual void ResetStats(bool achievementsToo)
	{
	}

	public void ProcessDlcUnlocks()
	{
		if (Time.realtimeSinceStartup < m_lastDlcCheckTime + 1f)
		{
			return;
		}
		GalaxyMtxGunHack();
		for (int i = 0; i < UnlockedDlc.Count; i++)
		{
			PlatformDlc platformDlc = UnlockedDlc[i];
			for (int j = 0; j < m_dlcUnlockedItems.Length; j++)
			{
				if (m_dlcUnlockedItems[j].PlatformDlc == platformDlc)
				{
					DlcUnlockedItem dlcUnlockedItem = m_dlcUnlockedItems[j];
					EncounterDatabaseEntry entry = EncounterDatabase.GetEntry(dlcUnlockedItem.encounterGuid);
					if (entry != null && !entry.PrerequisitesMet())
					{
						GameStatsManager.Instance.ForceUnlock(dlcUnlockedItem.encounterGuid);
					}
					if (dlcUnlockedItem.flag != 0 && !GameStatsManager.Instance.GetFlag(dlcUnlockedItem.flag))
					{
						GameStatsManager.Instance.SetFlag(dlcUnlockedItem.flag, true);
					}
				}
			}
		}
		m_lastDlcCheckTime = Time.realtimeSinceStartup;
	}

	public void LateUpdate()
	{
		OnLateUpdate();
		UpdateAlienFXController();
	}

	public void CatchupAchievements()
	{
		if (m_hasCaughtUpAchievements)
		{
			return;
		}
		if (GameManager.Options.wipeAllAchievements)
		{
			ResetStats(true);
			GameManager.Options.wipeAllAchievements = false;
		}
		if (GameManager.Options.scanAchievementsForUnlocks)
		{
			if (IsAchievementUnlocked(Achievement.COLLECT_FIVE_MASTERY_TOKENS))
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_LEAD_GOD, true);
			}
			GameManager.Options.scanAchievementsForUnlocks = false;
		}
		IEnumerator enumerator = Enum.GetValues(typeof(TrackedStats)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				TrackedStats stat = (TrackedStats)enumerator.Current;
				GameStatsManager.Instance.HandleStatAchievements(stat);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		foreach (GungeonFlags flag in GameStatsManager.Instance.m_flags)
		{
			GameStatsManager.Instance.HandleFlagAchievements(flag);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_TABLE_PIT))
		{
			AchievementUnlock(Achievement.PUSH_TABLE_INTO_PIT);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_BEASTMODE))
		{
			AchievementUnlock(Achievement.COMPLETE_GAME_WITH_BEAST_MODE);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_CONSTRUCT_BULLET))
		{
			AchievementUnlock(Achievement.BUILD_BULLET);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_ACCESS_OUBLIETTE))
		{
			AchievementUnlock(Achievement.REACH_SEWERS);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_ACCESS_ABBEY))
		{
			AchievementUnlock(Achievement.REACH_CATHEDRAL);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_SURPRISE_MIMIC))
		{
			AchievementUnlock(Achievement.PREFIRE_ON_MIMIC);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_KILL_JAMMED_BOSS))
		{
			AchievementUnlock(Achievement.BEAT_A_JAMMED_BOSS);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_BIGGEST_WALLET))
		{
			AchievementUnlock(Achievement.HAVE_MANY_COINS);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ACHIEVEMENT_LEAD_GOD))
		{
			AchievementUnlock(Achievement.COLLECT_FIVE_MASTERY_TOKENS);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_RECEIVED_BUSTED_TELEVISION))
		{
			AchievementUnlock(Achievement.UNLOCK_ROBOT);
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_05))
		{
			AchievementUnlock(Achievement.UNLOCK_BULLET);
		}
		m_hasCaughtUpAchievements = true;
	}

	public StringTableManager.GungeonSupportedLanguages GetPreferredLanguage()
	{
		return OnGetPreferredLanguage();
	}

	public static void InitializeAlienFXController()
	{
	}

	public static void SetAlienFXAmbientColor(Color32 color)
	{
		if (m_useLightFX)
		{
			m_AlienFXAmbientEffect = new LightFXUnit(color, 1f);
		}
	}

	public static void SetAlienFXColor(Color32 color, float duration)
	{
		if (m_useLightFX)
		{
			LightFXUnit item = new LightFXUnit(color, duration);
			m_AlienFXExtantEffects.Add(item);
		}
	}

	public static void UpdateAlienFXController()
	{
		if (!m_useLightFX)
		{
			return;
		}
		if (m_AlienFXExtantEffects.Count > 0)
		{
			Color32 b = new Color32(0, 0, 0, 0);
			for (int i = 0; i < m_AlienFXExtantEffects.Count; i++)
			{
				LightFXUnit value = m_AlienFXExtantEffects[i];
				value.remainingDuration -= BraveTime.DeltaTime;
				if (value.remainingDuration <= 0f)
				{
					m_AlienFXExtantEffects.RemoveAt(i);
					i--;
					continue;
				}
				byte b2 = (byte)Mathf.Lerp(0f, (int)value.TargetLightColor.a, value.remainingDuration / value.startDuration);
				b.a = (byte)Mathf.Min(255, b.a + b2);
				b.r = (byte)Mathf.Min(255, b.r + value.TargetLightColor.r);
				b.g = (byte)Mathf.Min(255, b.g + value.TargetLightColor.g);
				b.b = (byte)Mathf.Min(255, b.b + value.TargetLightColor.b);
				m_AlienFXExtantEffects[i] = value;
			}
			float t = (float)(int)b.a / 255f;
			Color color = m_AlienFXAmbientEffect.TargetLightColor;
			if ((bool)GameManager.Instance.DungeonMusicController && GameManager.Instance.DungeonMusicController.ShouldPulseLightFX)
			{
				float num = (color.a = Mathf.Clamp01(Mathf.Lerp(color.a, color.a - 0.25f, Mathf.PingPong(Time.realtimeSinceStartup, 5f) / 5f)));
			}
			b = Color32.Lerp(color, b, t);
			AlienFXInterface._LFX_COLOR c = new AlienFXInterface._LFX_COLOR(b);
			uint numDevices = 0u;
			if (AlienFXInterface.LFX_GetNumDevices(ref numDevices) == 0)
			{
				for (uint num2 = 0u; num2 < numDevices; num2++)
				{
					uint numLights = 0u;
					if (AlienFXInterface.LFX_GetNumLights(num2, ref numLights) == 0)
					{
						for (uint num3 = 0u; num3 < numLights; num3++)
						{
							AlienFXInterface.LFX_SetLightColor(num2, num3, ref c);
						}
					}
				}
			}
		}
		else if (m_AlienFXAmbientEffect.TargetLightColor.a > 0)
		{
			Color color2 = m_AlienFXAmbientEffect.TargetLightColor;
			if ((bool)GameManager.Instance.DungeonMusicController && GameManager.Instance.DungeonMusicController.ShouldPulseLightFX)
			{
				float num4 = (color2.a = Mathf.Clamp01(Mathf.Lerp(color2.a, 0f, Mathf.PingPong(Time.realtimeSinceStartup, 2f) / 2f)));
			}
			AlienFXInterface._LFX_COLOR c2 = new AlienFXInterface._LFX_COLOR(color2);
			uint numDevices2 = 0u;
			if (AlienFXInterface.LFX_GetNumDevices(ref numDevices2) == 0)
			{
				for (uint num5 = 0u; num5 < numDevices2; num5++)
				{
					uint numLights2 = 0u;
					if (AlienFXInterface.LFX_GetNumLights(num5, ref numLights2) == 0)
					{
						for (uint num6 = 0u; num6 < numLights2; num6++)
						{
							AlienFXInterface.LFX_SetLightColor(num5, num6, ref c2);
						}
					}
				}
			}
		}
		else
		{
			AlienFXInterface.LFX_Reset();
		}
		AlienFXInterface.LFX_Update();
	}

	public static void CleanupAlienFXController()
	{
		if (m_useLightFX)
		{
			AlienFXInterface.LFX_Release();
		}
	}

	protected void SetGungeonFlagForAchievement(Achievement achievement)
	{
		switch (achievement)
		{
		case Achievement.COMPLETE_GAME_WITH_ENCHANTED_GUN:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_GUNGAME_COMPLETE, true);
			break;
		case Achievement.BEAT_FLOOR_ONE_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_CASTLE_MANYTIMES, true);
			break;
		case Achievement.BEAT_FLOOR_TWO_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_GUNGEON_MANYTIMES, true);
			break;
		case Achievement.BEAT_FLOOR_THREE_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_MINES_MANYTIMES, true);
			break;
		case Achievement.BEAT_FLOOR_FOUR_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_HOLLOW_MANYTIMES, true);
			break;
		case Achievement.BEAT_FLOOR_FIVE_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_FORGE_MANYTIMES, true);
			break;
		case Achievement.COMPLETE_GAME_WITH_STARTER_GUN:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_STARTING_GUN, true);
			break;
		case Achievement.STEAL_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_STEAL_THINGS, true);
			break;
		case Achievement.POPULATE_BREACH:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_BREACH_POPULATED, true);
			break;
		case Achievement.PUSH_TABLE_INTO_PIT:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_TABLE_PIT, true);
			break;
		case Achievement.COMPLETE_GAME_WITH_BEAST_MODE:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_BEASTMODE, true);
			break;
		case Achievement.FLIP_TABLES_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_TABLES_FLIPPED, true);
			break;
		case Achievement.BUILD_BULLET:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_CONSTRUCT_BULLET, true);
			break;
		case Achievement.BEAT_PAST_ALL:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_COMPLETE_FOUR_PASTS, true);
			break;
		case Achievement.REACH_SEWERS:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_ACCESS_OUBLIETTE, true);
			break;
		case Achievement.REACH_CATHEDRAL:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_ACCESS_ABBEY, true);
			break;
		case Achievement.PREFIRE_ON_MIMIC:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_SURPRISE_MIMIC, true);
			break;
		case Achievement.KILL_WITH_PITS_MULTI:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_PIT_LORD, true);
			break;
		case Achievement.BEAT_A_JAMMED_BOSS:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_KILL_JAMMED_BOSS, true);
			break;
		case Achievement.HAVE_MANY_COINS:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_BIGGEST_WALLET, true);
			break;
		case Achievement.COLLECT_FIVE_MASTERY_TOKENS:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_LEAD_GOD, true);
			break;
		case Achievement.COMPLETE_GAME_WITH_CHALLENGE_MODE:
			GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_CHALLENGE_MODE_COMPLETE, true);
			break;
		}
	}

	protected abstract void OnStart();

	protected abstract void OnAchievementUnlock(Achievement achievement, int playerIndex);

	protected abstract void OnLateUpdate();

	protected abstract StringTableManager.GungeonSupportedLanguages OnGetPreferredLanguage();

	private void GalaxyMtxGunHack()
	{
		if (m_hasCheckedForGalaxyMtx)
		{
			return;
		}
		m_hasCheckedForGalaxyMtx = true;
		if (PlatformInterfaceSteam.IsSteamBuild())
		{
			return;
		}
		string text = null;
		text = Path.Combine(Application.dataPath, "../Unlock MTX Gun.dat");
		if (text == null || !File.Exists(text))
		{
			return;
		}
		byte[] array = File.ReadAllBytes(text);
		byte[] array2 = StringToByteArray("e226 87d5 f590 279d 38f5 fe7b 07fe cdf5 41c8 1c7d 257f 6ad5 d293 985e 994e 3032 c91d 8d6e 5697 5abb 8ee6 15ab 9afc 12e2 f8cf d5dd 8339 f987 6bcb ba0e 6280 1386 2881 c560 5980 457f c52f 1378 18ad f5da c8ec a283 f32e 8e78 0970 ea11 213a ed71 66d2 6ab2 7124 2c4e 6778 0e61 ada5 f225 e921 6326 2126 cd37 183b db48 3110 c14b 3358 c772 fbce a89b bde0 5ba9 6458 3acf 9307 2496 3be6 825d 1d75 84db 379e c360 7da9 0342 1042 7f5f 89ba 77e3 e74c 1195 f896 ff9a b1db 1350 2dce b368 7884 d5ad 5e6e 5957 fe74 1980 fabe 0e90 bf57 e29d 0239 0355 8ca7 d212 450b c426 10c2 7098 63a6 769b e827 d5e0 0d65 d6d7 fb3c e531 d0e8 bf83 5d2a bc83 388d 4b8f 8a22 b424");
		if (array.Length != array2.Length)
		{
			return;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != array2[i])
			{
				return;
			}
		}
		if (!UnlockedDlc.Contains(PlatformDlc.EARLY_MTX_GUN))
		{
			UnlockedDlc.Add(PlatformDlc.EARLY_MTX_GUN);
		}
	}

	public static byte[] StringToByteArray(string hex)
	{
		hex = hex.Replace(" ", string.Empty);
		return (from x in Enumerable.Range(0, hex.Length)
			where x % 2 == 0
			select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
	}

	public static string GetTrackedStatEventString(TrackedStats stat)
	{
		string result = string.Empty;
		switch (stat)
		{
		case TrackedStats.ENEMIES_KILLED:
			result = "EnemiesKilled";
			break;
		case TrackedStats.TIMES_KILLED_PAST:
			result = "PastsKilled";
			break;
		case TrackedStats.NUMBER_DEATHS:
			result = "Deaths";
			break;
		case TrackedStats.TABLES_FLIPPED:
			result = "TablesFlipped";
			break;
		}
		return result;
	}
}
