using System;
using System.Collections.Generic;
using FullSerializer;
using UnityEngine;

[fsObject]
public class GameStatsManager
{
	[fsProperty]
	public Dictionary<PlayableCharacters, GameStats> m_characterStats = new Dictionary<PlayableCharacters, GameStats>(new PlayableCharactersComparer());

	[fsProperty]
	private Dictionary<string, EncounteredObjectData> m_encounteredTrackables = new Dictionary<string, EncounteredObjectData>();

	[fsProperty]
	public Dictionary<string, int> m_encounteredFlows = new Dictionary<string, int>();

	[fsProperty]
	public Dictionary<string, EncounteredObjectData> m_encounteredRooms = new Dictionary<string, EncounteredObjectData>();

	[fsProperty]
	public HashSet<GungeonFlags> m_flags = new HashSet<GungeonFlags>(new GungeonFlagsComparer());

	[fsProperty]
	public Dictionary<string, int> m_persistentStringsLastIndices = new Dictionary<string, int>();

	[fsProperty]
	public Dictionary<int, int> m_encounteredSynergiesByID = new Dictionary<int, int>();

	[fsProperty]
	public MonsterHuntProgress huntProgress;

	[fsProperty]
	public int CurrentResRatShopSeed = -1;

	[fsProperty]
	public int CurrentEeveeEquipSeed = -1;

	[fsProperty]
	public int CurrentAccumulatedGunderfuryExperience;

	[fsProperty]
	public int CurrentRobotArmFloor = 5;

	[fsProperty]
	public int NumberRunsValidCellWithoutSpawn;

	[fsProperty]
	public float AccumulatedBeetleMerchantChance;

	[fsProperty]
	public float AccumulatedUsedBeetleMerchantChance;

	[fsProperty]
	public Dictionary<GlobalDungeonData.ValidTilesets, string> LastBossEncounteredMap = new Dictionary<GlobalDungeonData.ValidTilesets, string>();

	[fsProperty]
	private HashSet<string> forcedUnlocks = new HashSet<string>();

	[fsProperty]
	public string midGameSaveGuid;

	[fsProperty]
	public int savedSystemHash = -1;

	[fsProperty]
	public bool isChump;

	[fsProperty]
	public bool isTurboMode;

	[fsProperty]
	public bool rainbowRunToggled;

	private int m_numCharacters = -1;

	private PlayableCharacters m_sessionCharacter;

	private GameStats m_sessionStats;

	private GameStats m_savedSessionStats;

	private HashSet<int> m_sessionSynergies = new HashSet<int>();

	private static GameStatsManager m_instance;

	private static List<GungeonFlags> s_pastFlags;

	private static List<GungeonFlags> s_npcFoyerFlags;

	private static List<GungeonFlags> s_frifleHuntFlags;

	[fsIgnore]
	private List<string> m_singleProcessedEncounterTrackables = new List<string>();

	public bool IsRainbowRun
	{
		get
		{
			GameManager instance = GameManager.Instance;
			if (instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
			{
				return false;
			}
			return rainbowRunToggled;
		}
	}

	public static GameStatsManager Instance
	{
		get
		{
			if (m_instance == null)
			{
				Debug.LogError("Trying to access GameStatsManager before it has been initialized.");
			}
			return m_instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance != null;
		}
	}

	public int PlaytimeMin
	{
		get
		{
			return Mathf.RoundToInt(GetPlayerStatValue(TrackedStats.TIME_PLAYED) / 60f);
		}
	}

	public float NewPlayerEnemyCullFactor
	{
		get
		{
			if (GetFlag(GungeonFlags.BOSSKILLED_DRAGUN))
			{
				return 0f;
			}
			int num = Mathf.RoundToInt(GetPlayerStatValue(TrackedStats.TIMES_REACHED_FORGE));
			float num2 = 0.1f;
			return Mathf.Clamp(num2 - (float)num * 0.02f, 0f, 0.1f);
		}
	}

	[fsIgnore]
	public bool IsInSession
	{
		get
		{
			return m_sessionStats != null;
		}
	}

	public static void Load()
	{
		SaveManager.Init();
		if (!SaveManager.Load<GameStatsManager>(SaveManager.GameSave, out m_instance, true))
		{
			m_instance = new GameStatsManager();
		}
		if (m_instance.huntProgress != null)
		{
			m_instance.huntProgress.OnLoaded();
		}
		else
		{
			m_instance.huntProgress = new MonsterHuntProgress();
			m_instance.huntProgress.OnLoaded();
		}
		if (s_pastFlags == null)
		{
			s_pastFlags = new List<GungeonFlags>();
			s_pastFlags.Add(GungeonFlags.BOSSKILLED_ROGUE_PAST);
			s_pastFlags.Add(GungeonFlags.BOSSKILLED_CONVICT_PAST);
			s_pastFlags.Add(GungeonFlags.BOSSKILLED_SOLDIER_PAST);
			s_pastFlags.Add(GungeonFlags.BOSSKILLED_GUIDE_PAST);
		}
		if (s_npcFoyerFlags == null)
		{
			s_npcFoyerFlags = new List<GungeonFlags>();
			s_npcFoyerFlags.Add(GungeonFlags.META_SHOP_ACTIVE_IN_FOYER);
			s_npcFoyerFlags.Add(GungeonFlags.GUNSLING_KING_ACTIVE_IN_FOYER);
			s_npcFoyerFlags.Add(GungeonFlags.SORCERESS_ACTIVE_IN_FOYER);
			s_npcFoyerFlags.Add(GungeonFlags.LOST_ADVENTURER_ACTIVE_IN_FOYER);
			s_npcFoyerFlags.Add(GungeonFlags.TUTORIAL_TALKED_AFTER_RIVAL_KILLED);
			s_npcFoyerFlags.Add(GungeonFlags.SHOP_TRUCK_ACTIVE);
			s_npcFoyerFlags.Add(GungeonFlags.SHERPA_ACTIVE_IN_ELEVATOR_ROOM);
			s_npcFoyerFlags.Add(GungeonFlags.WINCHESTER_MET_PREVIOUSLY);
			s_npcFoyerFlags.Add(GungeonFlags.LEDGEGOBLIN_ACTIVE_IN_FOYER);
			s_npcFoyerFlags.Add(GungeonFlags.FRIFLE_ACTIVE_IN_FOYER);
		}
		if (s_frifleHuntFlags == null)
		{
			s_frifleHuntFlags = new List<GungeonFlags>();
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_01_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_02_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_03_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_04_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_05_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_06_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_07_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_08_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_09_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_10_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_11_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_12_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_13_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_MONSTERHUNT_14_COMPLETE);
			s_frifleHuntFlags.Add(GungeonFlags.FRIFLE_CORE_HUNTS_COMPLETE);
		}
	}

	public static bool Save()
	{
		GameManager.Instance.platformInterface.StoreStats();
		bool result = false;
		try
		{
			result = SaveManager.Save(m_instance, SaveManager.GameSave, m_instance.PlaytimeMin);
			return result;
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("SAVE FAILED: {0}", ex);
			return result;
		}
	}

	public static void DANGEROUS_ResetAllStats()
	{
		m_instance = new GameStatsManager();
		m_instance.huntProgress = new MonsterHuntProgress();
		m_instance.huntProgress.OnLoaded();
		SaveManager.DeleteAllBackups(SaveManager.GameSave);
		SaveManager.ResetSaveSlot = false;
	}

	public void BeginNewSession(PlayerController player)
	{
		if (IsInSession)
		{
			BraveUtility.Log("MODIFYING CHARACTER FOR SESSION STATS", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
			m_sessionCharacter = player.characterIdentity;
			m_sessionSynergies.Clear();
			if (!m_characterStats.ContainsKey(player.characterIdentity))
			{
				m_characterStats.Add(player.characterIdentity, new GameStats());
			}
			foreach (int startingGunId in player.startingGunIds)
			{
				Gun gun = PickupObjectDatabase.GetById(startingGunId) as Gun;
				EncounterTrackable component = gun.GetComponent<EncounterTrackable>();
				if ((bool)component && QueryEncounterableDifferentiator(component) < 1)
				{
					HandleEncounteredObject(component);
					SetEncounterableDifferentiator(component, 1);
				}
			}
		}
		else
		{
			BraveUtility.Log("CREATING NEW SESSION STATS", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
			m_sessionCharacter = player.characterIdentity;
			m_sessionSynergies.Clear();
			m_sessionStats = new GameStats();
			m_savedSessionStats = new GameStats();
			if (!m_characterStats.ContainsKey(player.characterIdentity))
			{
				m_characterStats.Add(player.characterIdentity, new GameStats());
			}
			foreach (int startingGunId2 in player.startingGunIds)
			{
				Gun gun2 = PickupObjectDatabase.GetById(startingGunId2) as Gun;
				EncounterTrackable component2 = gun2.GetComponent<EncounterTrackable>();
				if ((bool)component2 && QueryEncounterableDifferentiator(component2) < 1)
				{
					HandleEncounteredObject(component2);
					SetEncounterableDifferentiator(component2, 1);
				}
			}
		}
		if (!GetFlag(GungeonFlags.TONIC_ACTIVE_IN_FOYER) && GameManager.IsTurboMode)
		{
			Instance.isTurboMode = false;
		}
		if (!GetFlag(GungeonFlags.BOWLER_ACTIVE_IN_FOYER) && Instance.rainbowRunToggled)
		{
			Instance.rainbowRunToggled = false;
		}
	}

	public void AssignMidGameSavedSessionStats(GameStats source)
	{
		if (IsInSession && m_savedSessionStats != null)
		{
			m_savedSessionStats.AddStats(source);
		}
	}

	public GameStats MoveSessionStatsToSavedSessionStats()
	{
		if (!IsInSession)
		{
			return null;
		}
		if (m_sessionStats != null)
		{
			m_sessionStats.SetMax(TrackedMaximums.MOST_ENEMIES_KILLED, m_sessionStats.GetStatValue(TrackedStats.ENEMIES_KILLED) + m_savedSessionStats.GetStatValue(TrackedStats.ENEMIES_KILLED));
			if (m_characterStats.ContainsKey(m_sessionCharacter))
			{
				m_characterStats[m_sessionCharacter].AddStats(m_sessionStats);
			}
			m_savedSessionStats.AddStats(m_sessionStats);
			m_sessionStats.ClearAllState();
		}
		return m_savedSessionStats;
	}

	public void EndSession(bool recordSessionStats, bool decrementDifferentiator = true)
	{
		if (!IsInSession)
		{
			return;
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
		{
			decrementDifferentiator = false;
		}
		BraveUtility.Log("ENDING SESSION. RIGHT NOW: " + decrementDifferentiator, Color.red, BraveUtility.LogVerbosity.IMPORTANT);
		if (m_sessionStats != null)
		{
			if (recordSessionStats)
			{
				m_sessionStats.SetMax(TrackedMaximums.MOST_ENEMIES_KILLED, m_sessionStats.GetStatValue(TrackedStats.ENEMIES_KILLED));
				if (m_characterStats.ContainsKey(m_sessionCharacter))
				{
					m_characterStats[m_sessionCharacter].AddStats(m_sessionStats);
				}
				else
				{
					Debug.LogWarning(string.Concat("Character stats for ", m_sessionCharacter, " were not found; session stats are being thrown away."));
				}
			}
			m_sessionStats = null;
			m_savedSessionStats = null;
		}
		if (m_singleProcessedEncounterTrackables != null)
		{
			m_singleProcessedEncounterTrackables.Clear();
		}
		if (!decrementDifferentiator)
		{
			return;
		}
		foreach (string key in m_encounteredTrackables.Keys)
		{
			if (m_encounteredTrackables[key].differentiator > 0)
			{
				m_encounteredTrackables[key].differentiator = Mathf.Min(m_encounteredTrackables[key].differentiator - 1, 3);
			}
		}
		foreach (string key2 in m_encounteredRooms.Keys)
		{
			if (m_encounteredRooms[key2].differentiator > 0)
			{
				m_encounteredRooms[key2].differentiator = Mathf.Min(3, m_encounteredRooms[key2].differentiator - 1);
			}
		}
		List<string> list = new List<string>();
		foreach (string key3 in m_encounteredFlows.Keys)
		{
			if (m_encounteredFlows[key3] > 0)
			{
				list.Add(key3);
			}
		}
		foreach (string item in list)
		{
			m_encounteredFlows[item] -= 1;
		}
	}

	public void ClearAllStatsGlobal()
	{
		m_sessionStats.ClearAllState();
		m_savedSessionStats.ClearAllState();
		if (m_numCharacters <= 0)
		{
			m_numCharacters = Enum.GetValues(typeof(PlayableCharacters)).Length;
		}
		for (int i = 0; i < m_numCharacters; i++)
		{
			GameStats value;
			if (m_characterStats.TryGetValue((PlayableCharacters)i, out value))
			{
				value.ClearAllState();
			}
		}
	}

	public void ClearStatValueGlobal(TrackedStats stat)
	{
		m_sessionStats.SetStat(stat, 0f);
		m_savedSessionStats.SetStat(stat, 0f);
		if (m_numCharacters <= 0)
		{
			m_numCharacters = Enum.GetValues(typeof(PlayableCharacters)).Length;
		}
		for (int i = 0; i < m_numCharacters; i++)
		{
			GameStats value;
			if (m_characterStats.TryGetValue((PlayableCharacters)i, out value))
			{
				value.SetStat(stat, 0f);
			}
		}
	}

	public void UpdateMaximum(TrackedMaximums stat, float val)
	{
		if (!float.IsNaN(val) && !float.IsInfinity(val) && m_sessionStats != null)
		{
			m_sessionStats.SetMax(stat, val);
		}
	}

	public void SetStat(TrackedStats stat, float value)
	{
		if (!float.IsNaN(value) && !float.IsInfinity(value) && m_sessionStats != null)
		{
			m_sessionStats.SetStat(stat, value);
			HandleStatAchievements(stat);
			HandleSetPlatformStat(stat, GetPlayerStatValue(stat));
		}
	}

	public void RegisterStatChange(TrackedStats stat, float value)
	{
		if (m_sessionStats == null)
		{
			Debug.LogError("No session stats active and we're registering a stat change!");
		}
		else if (!float.IsNaN(value) && !float.IsInfinity(value) && !(Mathf.Abs(value) > 10000f))
		{
			float playerStatValue = GetPlayerStatValue(stat);
			m_sessionStats.IncrementStat(stat, value);
			HandleStatAchievements(stat);
			HandleIncrementPlatformStat(stat, value, playerStatValue);
			GameManager.Instance.platformInterface.SendEvent(PlatformInterface.GetTrackedStatEventString(stat), 1);
		}
	}

	public void SetNextFlag(params GungeonFlags[] flagList)
	{
		for (int i = 0; i < flagList.Length; i++)
		{
			if (!GetFlag(flagList[i]))
			{
				SetFlag(flagList[i], true);
				break;
			}
		}
	}

	public void SetFlag(GungeonFlags flag, bool value)
	{
		if (flag == GungeonFlags.NONE)
		{
			Debug.LogError("Something is attempting to set a NONE save flag!");
			return;
		}
		if (value)
		{
			m_flags.Add(flag);
		}
		else
		{
			m_flags.Remove(flag);
		}
		if (value)
		{
			HandleFlagAchievements(flag);
		}
		if (value && flag == GungeonFlags.BOSSKILLED_DRAGUN && GameManager.Options.m_beastmode)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_GAME_WITH_BEAST_MODE);
		}
	}

	public void SetCharacterSpecificFlag(CharacterSpecificGungeonFlags flag, bool value)
	{
		SetCharacterSpecificFlag(m_sessionCharacter, flag, value);
	}

	public void SetCharacterSpecificFlag(PlayableCharacters character, CharacterSpecificGungeonFlags flag, bool value)
	{
		if (flag == CharacterSpecificGungeonFlags.NONE)
		{
			Debug.LogError("Something is attempting to set a NONE character-specific save flag!");
			return;
		}
		if (!m_characterStats.ContainsKey(character))
		{
			m_characterStats.Add(character, new GameStats());
		}
		if (m_sessionStats != null && m_sessionCharacter == character)
		{
			m_sessionStats.SetFlag(flag, value);
		}
		else
		{
			m_characterStats[character].SetFlag(flag, value);
		}
		if (flag != CharacterSpecificGungeonFlags.KILLED_PAST)
		{
			return;
		}
		PlayerController playerController = GameManager.Instance.PrimaryPlayer;
		if (character == PlayableCharacters.CoopCultist)
		{
			playerController = GameManager.Instance.SecondaryPlayer;
		}
		else if ((bool)playerController && playerController.IsTemporaryEeveeForUnlock)
		{
			Instance.SetFlag(GungeonFlags.FLAG_EEVEE_UNLOCKED, true);
		}
		if ((bool)playerController && playerController.IsUsingAlternateCostume)
		{
			if (m_sessionStats != null && m_sessionCharacter == character)
			{
				m_sessionStats.SetFlag(CharacterSpecificGungeonFlags.KILLED_PAST_ALTERNATE_COSTUME, value);
			}
			else
			{
				m_characterStats[character].SetFlag(CharacterSpecificGungeonFlags.KILLED_PAST_ALTERNATE_COSTUME, value);
			}
			if (value)
			{
				SetFlag(GungeonFlags.ITEMSPECIFIC_ALTERNATE_GUNS_UNLOCKED, true);
			}
		}
	}

	public void SetPersistentStringLastIndex(string key, int value)
	{
		m_persistentStringsLastIndices[key] = value;
	}

	public void ForceUnlock(string encounterGuid)
	{
		forcedUnlocks.Add(encounterGuid);
	}

	public bool IsForceUnlocked(string encounterGuid)
	{
		return forcedUnlocks.Contains(encounterGuid);
	}

	public float GetPlayerMaximum(TrackedMaximums stat)
	{
		if (m_numCharacters <= 0)
		{
			m_numCharacters = Enum.GetValues(typeof(PlayableCharacters)).Length;
		}
		float num = 0f;
		if (m_sessionStats != null)
		{
			num = Mathf.Max(num, m_sessionStats.GetMaximumValue(stat), m_savedSessionStats.GetMaximumValue(stat));
		}
		for (int i = 0; i < m_numCharacters; i++)
		{
			GameStats value;
			if (m_characterStats.TryGetValue((PlayableCharacters)i, out value))
			{
				num = Mathf.Max(num, value.GetMaximumValue(stat));
			}
		}
		return num;
	}

	public float GetSessionStatValue(TrackedStats stat)
	{
		return m_sessionStats.GetStatValue(stat) + m_savedSessionStats.GetStatValue(stat);
	}

	public float GetCharacterStatValue(TrackedStats stat)
	{
		return GetCharacterStatValue(GetCurrentCharacter(), stat);
	}

	public float GetCharacterStatValue(PlayableCharacters character, TrackedStats stat)
	{
		float num = 0f;
		if (m_sessionCharacter == character)
		{
			num += m_sessionStats.GetStatValue(stat);
		}
		if (m_characterStats.ContainsKey(character))
		{
			num += m_characterStats[character].GetStatValue(stat);
		}
		return num;
	}

	public float GetPlayerStatValue(TrackedStats stat)
	{
		if (m_numCharacters <= 0)
		{
			m_numCharacters = Enum.GetValues(typeof(PlayableCharacters)).Length;
		}
		float num = 0f;
		if (m_sessionStats != null)
		{
			num += m_sessionStats.GetStatValue(stat);
		}
		for (int i = 0; i < m_numCharacters; i++)
		{
			GameStats value;
			if (m_characterStats.TryGetValue((PlayableCharacters)i, out value))
			{
				num += value.GetStatValue(stat);
			}
		}
		return num;
	}

	public bool TestPastBeaten(PlayableCharacters character)
	{
		switch (character)
		{
		case PlayableCharacters.Pilot:
			return GetCharacterSpecificFlag(character, CharacterSpecificGungeonFlags.KILLED_PAST);
		case PlayableCharacters.Convict:
			return GetCharacterSpecificFlag(character, CharacterSpecificGungeonFlags.KILLED_PAST);
		case PlayableCharacters.Soldier:
			return GetCharacterSpecificFlag(character, CharacterSpecificGungeonFlags.KILLED_PAST);
		case PlayableCharacters.Guide:
			return GetCharacterSpecificFlag(character, CharacterSpecificGungeonFlags.KILLED_PAST);
		case PlayableCharacters.CoopCultist:
			return GetCharacterSpecificFlag(character, CharacterSpecificGungeonFlags.KILLED_PAST);
		case PlayableCharacters.Robot:
			return GetCharacterSpecificFlag(character, CharacterSpecificGungeonFlags.KILLED_PAST);
		case PlayableCharacters.Bullet:
			return GetCharacterSpecificFlag(character, CharacterSpecificGungeonFlags.KILLED_PAST);
		default:
			return false;
		}
	}

	public bool AllCorePastsBeaten()
	{
		return GetCharacterSpecificFlag(PlayableCharacters.Pilot, CharacterSpecificGungeonFlags.KILLED_PAST) && GetCharacterSpecificFlag(PlayableCharacters.Convict, CharacterSpecificGungeonFlags.KILLED_PAST) && GetCharacterSpecificFlag(PlayableCharacters.Soldier, CharacterSpecificGungeonFlags.KILLED_PAST) && GetCharacterSpecificFlag(PlayableCharacters.Guide, CharacterSpecificGungeonFlags.KILLED_PAST);
	}

	public bool CheckLameyCostumeUnlocked()
	{
		return false;
	}

	public bool CheckGunslingerCostumeUnlocked()
	{
		bool flag = Instance.GetFlag(GungeonFlags.BOSSKILLED_DRAGUN);
		flag &= Instance.GetFlag(GungeonFlags.BOSSKILLED_LICH);
		flag &= Instance.GetCharacterSpecificFlag(PlayableCharacters.Bullet, CharacterSpecificGungeonFlags.KILLED_PAST);
		flag &= Instance.GetCharacterSpecificFlag(PlayableCharacters.Convict, CharacterSpecificGungeonFlags.KILLED_PAST);
		flag &= Instance.GetCharacterSpecificFlag(PlayableCharacters.Guide, CharacterSpecificGungeonFlags.KILLED_PAST);
		flag &= Instance.GetCharacterSpecificFlag(PlayableCharacters.Pilot, CharacterSpecificGungeonFlags.KILLED_PAST);
		flag &= Instance.GetCharacterSpecificFlag(PlayableCharacters.Robot, CharacterSpecificGungeonFlags.KILLED_PAST);
		return flag & Instance.GetCharacterSpecificFlag(PlayableCharacters.Soldier, CharacterSpecificGungeonFlags.KILLED_PAST);
	}

	public int GetNumberPastsBeaten()
	{
		int num = 0;
		if (GetCharacterSpecificFlag(PlayableCharacters.Pilot, CharacterSpecificGungeonFlags.KILLED_PAST))
		{
			num++;
		}
		if (GetCharacterSpecificFlag(PlayableCharacters.Convict, CharacterSpecificGungeonFlags.KILLED_PAST))
		{
			num++;
		}
		if (GetCharacterSpecificFlag(PlayableCharacters.Soldier, CharacterSpecificGungeonFlags.KILLED_PAST))
		{
			num++;
		}
		if (GetCharacterSpecificFlag(PlayableCharacters.Guide, CharacterSpecificGungeonFlags.KILLED_PAST))
		{
			num++;
		}
		if (GetCharacterSpecificFlag(PlayableCharacters.Robot, CharacterSpecificGungeonFlags.KILLED_PAST))
		{
			num++;
		}
		if (GetCharacterSpecificFlag(PlayableCharacters.Bullet, CharacterSpecificGungeonFlags.KILLED_PAST))
		{
			num++;
		}
		return num;
	}

	public bool AnyPastBeaten()
	{
		return GetCharacterSpecificFlag(PlayableCharacters.Pilot, CharacterSpecificGungeonFlags.KILLED_PAST) || GetCharacterSpecificFlag(PlayableCharacters.Convict, CharacterSpecificGungeonFlags.KILLED_PAST) || GetCharacterSpecificFlag(PlayableCharacters.Soldier, CharacterSpecificGungeonFlags.KILLED_PAST) || GetCharacterSpecificFlag(PlayableCharacters.Guide, CharacterSpecificGungeonFlags.KILLED_PAST) || GetCharacterSpecificFlag(PlayableCharacters.Robot, CharacterSpecificGungeonFlags.KILLED_PAST) || GetCharacterSpecificFlag(PlayableCharacters.Bullet, CharacterSpecificGungeonFlags.KILLED_PAST);
	}

	public int GetNumberOfCompanionsUnlocked()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < PickupObjectDatabase.Instance.Objects.Count; i++)
		{
			PickupObject pickupObject = PickupObjectDatabase.Instance.Objects[i];
			if ((bool)pickupObject && pickupObject is CompanionItem && pickupObject.quality != PickupObject.ItemQuality.EXCLUDED && pickupObject.contentSource != ContentSource.EXCLUDED)
			{
				num++;
				if (!pickupObject.encounterTrackable || pickupObject.encounterTrackable.PrerequisitesMet())
				{
					num2++;
				}
			}
		}
		return num2;
	}

	public bool HasPast(PlayableCharacters id)
	{
		switch (id)
		{
		case PlayableCharacters.Cosmonaut:
			return false;
		case PlayableCharacters.Gunslinger:
			return false;
		case PlayableCharacters.Eevee:
			return false;
		default:
			return true;
		}
	}

	public bool GetFlag(GungeonFlags flag)
	{
		if (flag == GungeonFlags.NONE)
		{
			Debug.LogError("Something is attempting to get a NONE save flag!");
			return false;
		}
		return m_flags.Contains(flag);
	}

	public bool GetCharacterSpecificFlag(CharacterSpecificGungeonFlags flag)
	{
		return GetCharacterSpecificFlag(m_sessionCharacter, flag);
	}

	public bool GetCharacterSpecificFlag(PlayableCharacters character, CharacterSpecificGungeonFlags flag)
	{
		if (flag == CharacterSpecificGungeonFlags.NONE)
		{
			Debug.LogError("Something is attempting to get a NONE character-specific save flag!");
			return false;
		}
		if (m_sessionStats != null && m_sessionCharacter == character)
		{
			if (m_sessionStats.GetFlag(flag))
			{
				return true;
			}
			if (m_savedSessionStats.GetFlag(flag))
			{
				return true;
			}
		}
		GameStats value;
		if (m_characterStats.TryGetValue(character, out value))
		{
			return value.GetFlag(flag);
		}
		return false;
	}

	public int GetPersistentStringLastIndex(string key)
	{
		int value;
		if (m_persistentStringsLastIndices.TryGetValue(key, out value))
		{
			return value;
		}
		return -1;
	}

	public void EncounterFlow(string flowName)
	{
		Debug.Log("ENCOUNTERING FLOW: " + flowName);
		if (!m_encounteredFlows.ContainsKey(flowName))
		{
			m_encounteredFlows.Add(flowName, 2);
		}
		else
		{
			m_encounteredFlows[flowName] += 2;
		}
	}

	public int QueryFlowDifferentiator(string flowName)
	{
		if (BraveRandom.IgnoreGenerationDifferentiator)
		{
			return 0;
		}
		if (m_encounteredFlows.ContainsKey(flowName))
		{
			int num = m_encounteredFlows[flowName];
			if (num < 0 || num > 1000000)
			{
				m_encounteredFlows[flowName] = 0;
				num = 0;
			}
			return num;
		}
		return 0;
	}

	public int QueryRoomEncountered(PrototypeDungeonRoom prototype)
	{
		if (m_encounteredRooms.ContainsKey(prototype.GUID))
		{
			return m_encounteredRooms[prototype.GUID].encounterCount;
		}
		return 0;
	}

	public int QueryRoomEncountered(string GUID)
	{
		if (string.IsNullOrEmpty(GUID))
		{
			return 0;
		}
		if (m_encounteredRooms.ContainsKey(GUID))
		{
			return m_encounteredRooms[GUID].encounterCount;
		}
		return 0;
	}

	public int QueryRoomDifferentiator(PrototypeDungeonRoom prototype)
	{
		if (BraveRandom.IgnoreGenerationDifferentiator)
		{
			return 0;
		}
		if (string.IsNullOrEmpty(prototype.GUID))
		{
			return 0;
		}
		if (m_encounteredRooms.ContainsKey(prototype.GUID))
		{
			int num = m_encounteredRooms[prototype.GUID].differentiator;
			if (num < 0 || num > 1000000)
			{
				m_encounteredRooms[prototype.GUID].differentiator = 0;
				num = 0;
			}
			return num;
		}
		return 0;
	}

	public void ClearAllDifferentiatorHistory(bool doYouReallyWantToDoThis = false)
	{
		ClearAllPickupDifferentiators(doYouReallyWantToDoThis);
		ClearAllRoomDifferentiators();
	}

	public void ClearAllPickupDifferentiators(bool doYouReallyWantToDoThis = false)
	{
		if (!doYouReallyWantToDoThis)
		{
			return;
		}
		foreach (string key in m_encounteredTrackables.Keys)
		{
			m_encounteredTrackables[key].differentiator = 0;
		}
	}

	public void ClearAllEncounterableHistory(bool doYouReallyWantToDoThis = false)
	{
		if (!doYouReallyWantToDoThis)
		{
			return;
		}
		foreach (string key in m_encounteredTrackables.Keys)
		{
			m_encounteredTrackables[key].differentiator = 0;
		}
		m_encounteredTrackables.Clear();
	}

	public int QueryEncounterable(EncounterTrackable et)
	{
		if (m_encounteredTrackables.ContainsKey(et.EncounterGuid))
		{
			return m_encounteredTrackables[et.EncounterGuid].encounterCount;
		}
		return 0;
	}

	public int QueryEncounterable(EncounterDatabaseEntry et)
	{
		if (m_encounteredTrackables.ContainsKey(et.myGuid))
		{
			return m_encounteredTrackables[et.myGuid].encounterCount;
		}
		return 0;
	}

	public int QueryEncounterable(string encounterGuid)
	{
		if (m_encounteredTrackables.ContainsKey(encounterGuid))
		{
			return m_encounteredTrackables[encounterGuid].encounterCount;
		}
		return 0;
	}

	public void SetEncounterableDifferentiator(EncounterTrackable et, int val)
	{
		if (!et.IgnoreDifferentiator && m_encounteredTrackables.ContainsKey(et.EncounterGuid))
		{
			m_encounteredTrackables[et.EncounterGuid].differentiator = val;
		}
	}

	public void MarkEncounterableAnnounced(EncounterDatabaseEntry et)
	{
		if (m_encounteredTrackables.ContainsKey(et.myGuid))
		{
			m_encounteredTrackables[et.myGuid].hasBeenAmmonomiconAnnounced = true;
			return;
		}
		EncounteredObjectData encounteredObjectData = new EncounteredObjectData();
		encounteredObjectData.hasBeenAmmonomiconAnnounced = true;
		m_encounteredTrackables.Add(et.myGuid, encounteredObjectData);
	}

	public bool QueryEncounterableAnnouncement(string guid)
	{
		if (m_encounteredTrackables.ContainsKey(guid))
		{
			return m_encounteredTrackables[guid].hasBeenAmmonomiconAnnounced;
		}
		return false;
	}

	public int QueryEncounterableDifferentiator(EncounterTrackable et)
	{
		return QueryEncounterableDifferentiator(et.EncounterGuid, et.IgnoreDifferentiator);
	}

	public int QueryEncounterableDifferentiator(EncounterDatabaseEntry encounterData)
	{
		return QueryEncounterableDifferentiator(encounterData.myGuid, encounterData.IgnoreDifferentiator);
	}

	public int QueryEncounterableDifferentiator(string encounterGuid, bool ignoreDifferentiator)
	{
		if (m_encounteredTrackables.ContainsKey(encounterGuid))
		{
			if (ignoreDifferentiator)
			{
				return 0;
			}
			int num = m_encounteredTrackables[encounterGuid].differentiator;
			if (num < 0 || num > 1000000)
			{
				m_encounteredTrackables[encounterGuid].differentiator = 0;
				num = 0;
			}
			return num;
		}
		return 0;
	}

	public void ClearAllRoomDifferentiators()
	{
		foreach (string key in m_encounteredRooms.Keys)
		{
			m_encounteredRooms[key].differentiator = 0;
		}
	}

	public void HandleEncounteredRoom(RuntimePrototypeRoomData prototype)
	{
		EncounteredObjectData encounteredObjectData = null;
		if (prototype != null && !string.IsNullOrEmpty(prototype.GUID) && GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH && GameManager.Instance.CurrentGameMode != GameManager.GameMode.SUPERBOSSRUSH)
		{
			if (m_encounteredRooms.ContainsKey(prototype.GUID))
			{
				encounteredObjectData = m_encounteredRooms[prototype.GUID];
			}
			else
			{
				encounteredObjectData = new EncounteredObjectData();
				m_encounteredRooms.Add(prototype.GUID, encounteredObjectData);
			}
			m_encounteredRooms[prototype.GUID].encounterCount++;
			m_encounteredRooms[prototype.GUID].differentiator += 2;
		}
	}

	public void HandleEncounteredObjectRaw(string encounterGuid)
	{
		EncounteredObjectData encounteredObjectData = null;
		if (m_encounteredTrackables.ContainsKey(encounterGuid))
		{
			encounteredObjectData = m_encounteredTrackables[encounterGuid];
		}
		else
		{
			encounteredObjectData = new EncounteredObjectData();
			m_encounteredTrackables.Add(encounterGuid, encounteredObjectData);
		}
		encounteredObjectData.encounterCount++;
	}

	public void SingleIncrementDifferentiator(EncounterTrackable et)
	{
		EncounteredObjectData encounteredObjectData = null;
		if (m_encounteredTrackables.ContainsKey(et.EncounterGuid))
		{
			encounteredObjectData = m_encounteredTrackables[et.EncounterGuid];
		}
		else
		{
			encounteredObjectData = new EncounteredObjectData();
			m_encounteredTrackables.Add(et.EncounterGuid, encounteredObjectData);
		}
		if ((bool)et && !string.IsNullOrEmpty(et.EncounterGuid) && !m_singleProcessedEncounterTrackables.Contains(et.EncounterGuid))
		{
			m_singleProcessedEncounterTrackables.Add(et.EncounterGuid);
		}
		if (!et.IgnoreDifferentiator)
		{
			m_encounteredTrackables[et.EncounterGuid].differentiator++;
		}
	}

	public int GetNumberOfSynergiesEncounteredThisRun()
	{
		return m_sessionSynergies.Count;
	}

	public void HandleEncounteredSynergy(int index)
	{
		if (index > 0)
		{
			if (!m_encounteredSynergiesByID.ContainsKey(index))
			{
				m_encounteredSynergiesByID.Add(index, 0);
			}
			m_sessionSynergies.Add(index);
			m_encounteredSynergiesByID[index] += 1;
		}
	}

	public void HandleEncounteredObject(EncounterTrackable et)
	{
		EncounteredObjectData encounteredObjectData = null;
		if (m_encounteredTrackables.ContainsKey(et.EncounterGuid))
		{
			encounteredObjectData = m_encounteredTrackables[et.EncounterGuid];
		}
		else
		{
			encounteredObjectData = new EncounteredObjectData();
			m_encounteredTrackables.Add(et.EncounterGuid, encounteredObjectData);
		}
		encounteredObjectData.encounterCount++;
		if (!et.IgnoreDifferentiator)
		{
			if (m_singleProcessedEncounterTrackables != null && m_singleProcessedEncounterTrackables.Contains(et.EncounterGuid))
			{
				m_encounteredTrackables[et.EncounterGuid].differentiator++;
			}
			else
			{
				m_encounteredTrackables[et.EncounterGuid].differentiator += 2;
			}
		}
	}

	public GlobalDungeonData.ValidTilesets GetCurrentRobotArmTileset()
	{
		switch (CurrentRobotArmFloor)
		{
		case 5:
			return GlobalDungeonData.ValidTilesets.FORGEGEON;
		case 4:
			return GlobalDungeonData.ValidTilesets.CATACOMBGEON;
		case 3:
			return GlobalDungeonData.ValidTilesets.MINEGEON;
		case 2:
			return GlobalDungeonData.ValidTilesets.GUNGEON;
		case 1:
			return GlobalDungeonData.ValidTilesets.CASTLEGEON;
		case 0:
			return (GlobalDungeonData.ValidTilesets)(-1);
		default:
			return (GlobalDungeonData.ValidTilesets)(-1);
		}
	}

	private PlayableCharacters GetCurrentCharacter()
	{
		return GameManager.Instance.PrimaryPlayer.characterIdentity;
	}

	public void HandleStatAchievements(TrackedStats stat)
	{
		if (stat == TrackedStats.GUNBERS_MUNCHED && GetPlayerStatValue(stat) >= 20f)
		{
			SetFlag(GungeonFlags.MUNCHER_MUTANT_ARM_UNLOCKED, true);
			SetFlag(GungeonFlags.MUNCHER_COLD45_UNLOCKED, true);
		}
		switch (stat)
		{
		case TrackedStats.TIMES_REACHED_SEWERS:
			if (GetPlayerStatValue(stat) >= 1f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.REACH_SEWERS);
			}
			break;
		case TrackedStats.TIMES_REACHED_CATHEDRAL:
			if (GetPlayerStatValue(stat) >= 1f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.REACH_CATHEDRAL);
			}
			break;
		case TrackedStats.TIMES_CLEARED_CASTLE:
			if (GetPlayerStatValue(stat) >= 50f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_FLOOR_ONE_MULTI);
			}
			break;
		case TrackedStats.TIMES_CLEARED_GUNGEON:
			if (GetPlayerStatValue(stat) >= 40f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_FLOOR_TWO_MULTI);
			}
			break;
		case TrackedStats.TIMES_CLEARED_MINES:
			if (GetPlayerStatValue(stat) >= 30f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_FLOOR_THREE_MULTI);
			}
			break;
		case TrackedStats.TIMES_CLEARED_CATACOMBS:
			if (GetPlayerStatValue(stat) >= 20f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_FLOOR_FOUR_MULTI);
			}
			break;
		case TrackedStats.TIMES_CLEARED_FORGE:
			if (GetPlayerStatValue(stat) >= 10f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_FLOOR_FIVE_MULTI);
			}
			break;
		case TrackedStats.WINCHESTER_GAMES_ACED:
			if (GetPlayerStatValue(stat) >= 3f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.ACE_WINCHESTER_MULTI);
			}
			break;
		case TrackedStats.GUNSLING_KING_CHALLENGES_COMPLETED:
			if (GetPlayerStatValue(stat) >= 5f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_GUNSLING_MULTI);
			}
			break;
		case TrackedStats.META_CURRENCY_SPENT_AT_META_SHOP:
			if (GetPlayerStatValue(stat) >= 100f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.SPEND_META_CURRENCY);
			}
			break;
		case TrackedStats.MERCHANT_ITEMS_STOLEN:
			if (GetPlayerStatValue(stat) >= 10f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.STEAL_MULTI);
			}
			break;
		case TrackedStats.ENEMIES_KILLED_WITH_CHANDELIERS:
			if (GetPlayerStatValue(stat) >= 100f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.KILL_WITH_CHANDELIER_MULTI);
			}
			break;
		case TrackedStats.ENEMIES_KILLED_WHILE_IN_CARTS:
			if (GetPlayerStatValue(stat) >= 100f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.KILL_IN_MINE_CART_MULTI);
			}
			break;
		case TrackedStats.ENEMIES_KILLED_WITH_PITS:
			if (GetPlayerStatValue(stat) >= 100f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.KILL_WITH_PITS_MULTI);
			}
			break;
		case TrackedStats.TABLES_FLIPPED:
			if (GetPlayerStatValue(stat) >= 500f)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.FLIP_TABLES_MULTI);
			}
			break;
		case TrackedStats.MERCHANT_PURCHASES_GOOP:
			UpdateAllNpcsAchievement();
			break;
		}
	}

	private PlatformStat? ConvertToPlatformStat(TrackedStats stat)
	{
		switch (stat)
		{
		case TrackedStats.META_CURRENCY_SPENT_AT_META_SHOP:
			return PlatformStat.META_SPENT_AT_STORE;
		case TrackedStats.TIMES_CLEARED_CASTLE:
			return PlatformStat.FLOOR_ONE_CLEARS;
		case TrackedStats.TIMES_CLEARED_GUNGEON:
			return PlatformStat.FLOOR_TWO_CLEARS;
		case TrackedStats.TIMES_CLEARED_MINES:
			return PlatformStat.FLOOR_THREE_CLEARS;
		case TrackedStats.TIMES_CLEARED_CATACOMBS:
			return PlatformStat.FLOOR_FOUR_CLEARS;
		case TrackedStats.TIMES_CLEARED_FORGE:
			return PlatformStat.FLOOR_FIVE_CLEARS;
		case TrackedStats.WINCHESTER_GAMES_ACED:
			return PlatformStat.WINCHESTER_ACED;
		case TrackedStats.GUNSLING_KING_CHALLENGES_COMPLETED:
			return PlatformStat.GUNSLING_COMPLETED;
		case TrackedStats.MERCHANT_ITEMS_STOLEN:
			return PlatformStat.ITEMS_STOLEN;
		case TrackedStats.ENEMIES_KILLED_WITH_CHANDELIERS:
			return PlatformStat.CHANDELIER_KILLS;
		case TrackedStats.ENEMIES_KILLED_WHILE_IN_CARTS:
			return PlatformStat.MINECART_KILLS;
		case TrackedStats.ENEMIES_KILLED_WITH_PITS:
			return PlatformStat.PIT_KILLS;
		case TrackedStats.TABLES_FLIPPED:
			return PlatformStat.TABLES_FLIPPED;
		default:
			return null;
		}
	}

	private void HandleSetPlatformStat(TrackedStats stat, float value)
	{
		PlatformStat? platformStat = ConvertToPlatformStat(stat);
		if (platformStat.HasValue)
		{
			GameManager.Instance.platformInterface.SetStat(platformStat.Value, Mathf.RoundToInt(value));
		}
	}

	private void HandleIncrementPlatformStat(TrackedStats stat, float delta, float previousValue)
	{
		PlatformStat? platformStat = ConvertToPlatformStat(stat);
		if (platformStat.HasValue)
		{
			GameManager.Instance.platformInterface.SetStat(platformStat.Value, Mathf.RoundToInt(previousValue));
			GameManager.Instance.platformInterface.IncrementStat(platformStat.Value, Mathf.RoundToInt(delta));
		}
	}

	public void HandleFlagAchievements(GungeonFlags flag)
	{
		switch (flag)
		{
		case GungeonFlags.BOSSKILLED_ROGUE_PAST:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_PAST_ROGUE);
			break;
		case GungeonFlags.BOSSKILLED_CONVICT_PAST:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_PAST_CONVICT);
			break;
		case GungeonFlags.BOSSKILLED_SOLDIER_PAST:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_PAST_MARINE);
			break;
		case GungeonFlags.BOSSKILLED_GUIDE_PAST:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_PAST_GUIDE);
			break;
		case GungeonFlags.BOSSKILLED_ROBOT_PAST:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_PAST_ROBOT);
			break;
		case GungeonFlags.BOSSKILLED_BULLET_PAST:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_PAST_BULLET);
			break;
		case GungeonFlags.BOSSKILLED_DRAGUN:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_FLOOR_FIVE);
			break;
		case GungeonFlags.BOSSKILLED_LICH:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_FLOOR_SIX);
			break;
		case GungeonFlags.BOSSKILLED_HIGHDRAGUN:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_ADVANCED_DRAGUN);
			break;
		case GungeonFlags.BOSSKILLED_RESOURCEFULRAT:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_RESOURCEFUL_RAT);
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_METAL_GEAR_RAT);
			break;
		}
		switch (flag)
		{
		case GungeonFlags.BLACKSMITH_BULLET_COMPLETE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BUILD_BULLET);
			break;
		case GungeonFlags.LOST_ADVENTURER_ACHIEVEMENT_REWARD_GIVEN:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.MAP_MAIN_FLOORS);
			break;
		case GungeonFlags.META_SHOP_RECEIVED_ROBOT_ARM_REWARD:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_GOLEM_ARM);
			break;
		case GungeonFlags.FRIFLE_CORE_HUNTS_COMPLETE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_FRIFLE_MULTI);
			break;
		case GungeonFlags.TUTORIAL_KILLED_MANFREDS_RIVAL:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_MANFREDS_RIVAL);
			break;
		case GungeonFlags.TUTORIAL_COMPLETED:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_TUTORIAL);
			break;
		case GungeonFlags.DAISUKE_CHALLENGE_COMPLETE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_GAME_WITH_CHALLENGE_MODE);
			break;
		case GungeonFlags.BOSSKILLED_DRAGUN_TURBO_MODE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_GAME_WITH_TURBO_MODE);
			break;
		}
		switch (flag)
		{
		case GungeonFlags.SECRET_BULLETMAN_SEEN_05:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.UNLOCK_BULLET);
			break;
		case GungeonFlags.BLACKSMITH_RECEIVED_BUSTED_TELEVISION:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.UNLOCK_ROBOT);
			break;
		}
		switch (flag)
		{
		case GungeonFlags.SHERPA_UNLOCK1_COMPLETE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.UNLOCK_FLOOR_TWO_SHORTCUT);
			break;
		case GungeonFlags.SHERPA_UNLOCK2_COMPLETE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.UNLOCK_FLOOR_THREE_SHORTCUT);
			break;
		case GungeonFlags.SHERPA_UNLOCK3_COMPLETE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.UNLOCK_FLOOR_FOUR_SHORTCUT);
			break;
		case GungeonFlags.SHERPA_UNLOCK4_COMPLETE:
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.UNLOCK_FLOOR_FIVE_SHORTCUT);
			break;
		}
		if (s_pastFlags.Contains(flag))
		{
			bool flag2 = true;
			for (int i = 0; i < s_pastFlags.Count; i++)
			{
				if (!GetFlag(s_pastFlags[i]))
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_PAST_ALL);
			}
		}
		if (s_npcFoyerFlags.Contains(flag))
		{
			UpdateAllNpcsAchievement();
		}
		if (s_frifleHuntFlags.Contains(flag))
		{
			UpdateFrifleHuntAchievement();
		}
		if (flag == GungeonFlags.LOST_ADVENTURER_HELPED_CASTLE || flag == GungeonFlags.LOST_ADVENTURER_HELPED_GUNGEON || flag == GungeonFlags.LOST_ADVENTURER_HELPED_MINES || flag == GungeonFlags.LOST_ADVENTURER_HELPED_CATACOMBS || flag == GungeonFlags.LOST_ADVENTURER_HELPED_FORGE)
		{
			int num = 0;
			num += (GetFlag(GungeonFlags.LOST_ADVENTURER_HELPED_CASTLE) ? 1 : 0);
			num += (GetFlag(GungeonFlags.LOST_ADVENTURER_HELPED_GUNGEON) ? 1 : 0);
			num += (GetFlag(GungeonFlags.LOST_ADVENTURER_HELPED_MINES) ? 1 : 0);
			num += (GetFlag(GungeonFlags.LOST_ADVENTURER_HELPED_CATACOMBS) ? 1 : 0);
			num += (GetFlag(GungeonFlags.LOST_ADVENTURER_HELPED_FORGE) ? 1 : 0);
			GameManager.Instance.platformInterface.SetStat(PlatformStat.MAIN_FLOORS_MAPPED, num);
		}
	}

	private void UpdateAllNpcsAchievement()
	{
		bool flag = true;
		int num = 0;
		for (int i = 0; i < s_npcFoyerFlags.Count; i++)
		{
			if (GetFlag(s_npcFoyerFlags[i]))
			{
				num++;
			}
			else
			{
				flag = false;
			}
		}
		if (GetFlag(GungeonFlags.TUTORIAL_TALKED_AFTER_RIVAL_KILLED))
		{
			num++;
		}
		if (GetFlag(GungeonFlags.SORCERESS_ACTIVE_IN_FOYER))
		{
			SetFlag(GungeonFlags.DAISUKE_IS_UNLOCKABLE, true);
		}
		if (GetPlayerStatValue(TrackedStats.MERCHANT_PURCHASES_GOOP) >= 1f)
		{
			num++;
		}
		else
		{
			flag = false;
		}
		GameManager.Instance.platformInterface.SetStat(PlatformStat.BREACH_POPULATION, num);
		if (flag)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.POPULATE_BREACH);
		}
	}

	private void UpdateFrifleHuntAchievement()
	{
		bool flag = true;
		int num = 0;
		for (int i = 0; i < s_frifleHuntFlags.Count; i++)
		{
			if (GetFlag(s_frifleHuntFlags[i]))
			{
				num++;
			}
			else
			{
				flag = false;
			}
		}
		GameManager.Instance.platformInterface.SetStat(PlatformStat.FRIFLE_CORE_COMPLETED, num);
		if (flag)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_FRIFLE_MULTI);
		}
	}
}
