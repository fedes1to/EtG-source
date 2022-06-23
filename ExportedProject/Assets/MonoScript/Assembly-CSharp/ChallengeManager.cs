using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
	public static bool CHALLENGE_MODE_ACTIVE;

	private static ChallengeManager m_instance;

	public ChallengeModeType ChallengeMode = ChallengeModeType.ChallengeMode;

	[NonSerialized]
	public RoomHandler GunslingTargetRoom;

	public string ChallengeInSFX = "Play_UI_menu_pause_01";

	public dfAnimationClip ChallengeBurstClip;

	public List<ChallengeFloorData> FloorData = new List<ChallengeFloorData>();

	[Header("Remember the other _ChallengeManager too!")]
	public List<ChallengeDataEntry> PossibleChallenges = new List<ChallengeDataEntry>();

	public List<BossChallengeData> BossChallenges = new List<BossChallengeData>();

	private List<ChallengeModifier> m_activeChallenges = new List<ChallengeModifier>();

	private PlayerController m_primaryPlayer;

	private bool m_init;

	public static ChallengeManager Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = UnityEngine.Object.FindObjectOfType<ChallengeManager>();
			}
			return m_instance;
		}
	}

	public static ChallengeModeType ChallengeModeType
	{
		get
		{
			ChallengeManager instance = Instance;
			return instance ? instance.ChallengeMode : ChallengeModeType.None;
		}
		set
		{
			ChallengeManager instance = Instance;
			if ((bool)instance)
			{
				if (instance.ChallengeMode == value)
				{
					return;
				}
				UnityEngine.Object.Destroy(instance.gameObject);
			}
			switch (value)
			{
			case ChallengeModeType.GunslingKingTemporary:
			{
				GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)BraveResources.Load("Global Prefabs/_ChallengeManager"));
				gameObject.GetComponent<ChallengeManager>().ChallengeMode = ChallengeModeType.GunslingKingTemporary;
				break;
			}
			case ChallengeModeType.ChallengeMode:
				UnityEngine.Object.Instantiate((GameObject)BraveResources.Load("Global Prefabs/_ChallengeManager"));
				break;
			case ChallengeModeType.ChallengeMegaMode:
				UnityEngine.Object.Instantiate((GameObject)BraveResources.Load("Global Prefabs/_ChallengeMegaManager"));
				break;
			}
		}
	}

	public List<ChallengeModifier> ActiveChallenges
	{
		get
		{
			return m_activeChallenges;
		}
	}

	public bool SuppressChallengeStart { get; set; }

	private IEnumerator Start()
	{
		m_instance = this;
		if (ChallengeMode != ChallengeModeType.GunslingKingTemporary)
		{
			CHALLENGE_MODE_ACTIVE = true;
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.CHALLENGE_MODE_ATTEMPTS, 1f);
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.CHALLENGE_MODE_ATTEMPTS) >= 30f)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.DAISUKE_CHALLENGE_ITEM_UNLOCK, true);
			}
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			base.transform.parent = GameManager.Instance.transform;
		}
		while (GameManager.Instance.PrimaryPlayer == null)
		{
			yield return null;
		}
		m_init = true;
		m_primaryPlayer = GameManager.Instance.PrimaryPlayer;
		PlayerController primaryPlayer = m_primaryPlayer;
		primaryPlayer.OnEnteredCombat = (Action)Delegate.Combine(primaryPlayer.OnEnteredCombat, new Action(EnteredCombat));
	}

	private void Update()
	{
		if (m_activeChallenges.Count > 0 && !m_primaryPlayer.IsInCombat)
		{
			CleanupChallenges();
		}
		if (m_init && GameManager.Instance.IsFoyer && m_primaryPlayer != GameManager.Instance.PrimaryPlayer)
		{
			if ((bool)m_primaryPlayer)
			{
				PlayerController primaryPlayer = m_primaryPlayer;
				primaryPlayer.OnEnteredCombat = (Action)Delegate.Remove(primaryPlayer.OnEnteredCombat, new Action(EnteredCombat));
			}
			m_primaryPlayer = GameManager.Instance.PrimaryPlayer;
			PlayerController primaryPlayer2 = m_primaryPlayer;
			primaryPlayer2.OnEnteredCombat = (Action)Delegate.Combine(primaryPlayer2.OnEnteredCombat, new Action(EnteredCombat));
		}
	}

	private void OnDestroy()
	{
		CleanupChallenges();
		if ((bool)m_primaryPlayer)
		{
			PlayerController primaryPlayer = m_primaryPlayer;
			primaryPlayer.OnEnteredCombat = (Action)Delegate.Remove(primaryPlayer.OnEnteredCombat, new Action(EnteredCombat));
		}
		if (m_instance == this)
		{
			m_instance = null;
			CHALLENGE_MODE_ACTIVE = false;
		}
	}

	private ChallengeFloorData GetFloorData(GlobalDungeonData.ValidTilesets tilesetID)
	{
		for (int i = 0; i < FloorData.Count; i++)
		{
			if (FloorData[i].floorID == tilesetID)
			{
				return FloorData[i];
			}
		}
		return null;
	}

	public void EnteredCombat()
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.InTutorial || SuppressChallengeStart || (ChallengeMode == ChallengeModeType.GunslingKingTemporary && GunslingTargetRoom != null && GameManager.Instance.PrimaryPlayer.CurrentRoom != GunslingTargetRoom))
		{
			return;
		}
		CleanupChallenges();
		int num = 1;
		ChallengeFloorData floorData = GetFloorData(GameManager.Instance.Dungeon.tileIndices.tilesetId);
		if (floorData != null)
		{
			num = UnityEngine.Random.Range(floorData.minChallenges, floorData.maxChallenges + 1);
		}
		else
		{
			switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
			{
			case GlobalDungeonData.ValidTilesets.CASTLEGEON:
				num = 1;
				break;
			case GlobalDungeonData.ValidTilesets.GUNGEON:
				num = 2;
				break;
			case GlobalDungeonData.ValidTilesets.MINEGEON:
				num = 3;
				break;
			case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
				num = 4;
				break;
			case GlobalDungeonData.ValidTilesets.FORGEGEON:
				num = 5;
				break;
			default:
				num = 1;
				break;
			}
		}
		TriggerNewChallenges(num);
		StartCoroutine(HandleNewChallengeAnnouncements());
	}

	private IEnumerator HandleNewChallengeAnnouncements()
	{
		float elapsed = 0f;
		float duration2 = Mathf.Max(2.5f, m_activeChallenges.Count);
		if (GameManager.Options.SLOW_TIME_ON_CHALLENGE_MODE_REVEAL)
		{
			BraveTime.RegisterTimeScaleMultiplier(0.1f, base.gameObject);
		}
		Vector3[] startPositions = new Vector3[m_activeChallenges.Count];
		for (int i = 0; i < m_activeChallenges.Count; i++)
		{
			dfGUIManager manager = GameUIRoot.Instance.Manager;
			dfLabel dfLabel2 = manager.AddControl<dfLabel>();
			dfLabel2.Font = GameUIRoot.Instance.Manager.DefaultFont;
			dfLabel2.TextScale = 3f;
			dfLabel2.AutoSize = true;
			dfLabel2.Pivot = dfPivotPoint.TopRight;
			GameUIAmmoController ammoControllerForPlayerID = GameUIRoot.Instance.GetAmmoControllerForPlayerID(0);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				ammoControllerForPlayerID = GameUIRoot.Instance.GetAmmoControllerForPlayerID(1);
			}
			dfLabel2.RelativePosition = new Vector3(ammoControllerForPlayerID.DefaultAmmoFGSprite.GetAbsolutePosition().x + dfLabel2.Width, Mathf.FloorToInt(manager.GetScreenSize().y / 2f) - 198 + 60 * i, 0f);
			m_activeChallenges[i].IconLabel = dfLabel2;
			startPositions[i] = dfLabel2.RelativePosition;
			dfLabel2.IsLocalized = true;
			if (!string.IsNullOrEmpty(m_activeChallenges[i].AlternateLanguageDisplayName) && GameManager.Options.CurrentLanguage != 0)
			{
				dfLabel2.Text = StringTableManager.GetEnemiesString(m_activeChallenges[i].AlternateLanguageDisplayName);
			}
			else
			{
				dfLabel2.Text = m_activeChallenges[i].DisplayName;
			}
			dfSprite dfSprite2 = manager.AddControl<dfSprite>();
			dfSprite2.SpriteName = m_activeChallenges[i].AtlasSpriteName;
			dfSprite2.Size = dfSprite2.SpriteInfo.sizeInPixels * 3f;
			m_activeChallenges[i].IconSprite = dfSprite2;
			dfSprite2.BringToFront();
			dfLabel2.AddControl(dfSprite2);
			dfLabel2.BringToFront();
			dfSprite2.RelativePosition = new Vector3(6f + dfLabel2.Width, -3f, 0f);
		}
		while (elapsed < duration2)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			GameUIAmmoController ammoController = GameUIRoot.Instance.GetAmmoControllerForPlayerID(0);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				ammoController = GameUIRoot.Instance.GetAmmoControllerForPlayerID(1);
			}
			for (int j = 0; j < m_activeChallenges.Count; j++)
			{
				if (elapsed - 0.4f * (float)j > 0f && elapsed - GameManager.INVARIANT_DELTA_TIME - 0.4f * (float)j <= 0f)
				{
					AkSoundEngine.PostEvent(ChallengeInSFX, GameManager.Instance.PrimaryPlayer.gameObject);
				}
				dfLabel iconLabel = m_activeChallenges[j].IconLabel;
				iconLabel.RelativePosition = Vector3.Lerp(startPositions[j], new Vector3(ammoController.DefaultAmmoFGSprite.GetAbsolutePosition().x - iconLabel.Width - 42f, iconLabel.RelativePosition.y, iconLabel.RelativePosition.z), Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed - 0.4f * (float)j)));
			}
			if (GameManager.Options.SLOW_TIME_ON_CHALLENGE_MODE_REVEAL)
			{
				BraveTime.ClearMultiplier(base.gameObject);
				BraveTime.RegisterTimeScaleMultiplier(Mathf.Lerp(0.1f, 1f, elapsed - (duration2 - 1f)), base.gameObject);
			}
			yield return null;
		}
		for (int k = 0; k < m_activeChallenges.Count; k++)
		{
			dfLabel iconLabel2 = m_activeChallenges[k].IconLabel;
			dfSprite iconSprite = m_activeChallenges[k].IconSprite;
			iconLabel2.RemoveControl(iconSprite);
			iconSprite.AddControl(iconLabel2);
			iconSprite.BringToFront();
		}
		if (GameManager.Options.SLOW_TIME_ON_CHALLENGE_MODE_REVEAL)
		{
			BraveTime.ClearMultiplier(base.gameObject);
		}
		elapsed = 0f;
		duration2 = 2f;
		while (elapsed < duration2 && m_activeChallenges.Count > 0 && (!GameManager.Instance.IsPaused || !GameUIRoot.Instance.PauseMenuPanel.IsVisible))
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			for (int l = 0; l < m_activeChallenges.Count; l++)
			{
				m_activeChallenges[l].IconLabel.Opacity = 1f - elapsed / duration2;
			}
			yield return null;
		}
		while (m_activeChallenges.Count > 0)
		{
			if (GameManager.Instance.IsPaused && (bool)GameUIRoot.Instance.PauseMenuPanel && GameUIRoot.Instance.PauseMenuPanel.IsVisible)
			{
				for (int m = 0; m < m_activeChallenges.Count; m++)
				{
					m_activeChallenges[m].IconLabel.Opacity = 1f;
				}
			}
			else
			{
				for (int n = 0; n < m_activeChallenges.Count; n++)
				{
					m_activeChallenges[n].IconLabel.Opacity = 0f;
				}
			}
			yield return null;
		}
	}

	private void TriggerNewChallenges(int numToAdd)
	{
		if (GameManager.Instance.InTutorial)
		{
			return;
		}
		RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		if (currentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
		{
			BossChallengeData bossChallengeData = null;
			List<AIActor> activeEnemies = currentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if (!activeEnemies[i] || !activeEnemies[i].healthHaver || !activeEnemies[i].healthHaver.IsBoss)
				{
					continue;
				}
				for (int j = 0; j < BossChallenges.Count; j++)
				{
					BossChallengeData bossChallengeData2 = BossChallenges[j];
					for (int k = 0; k < bossChallengeData2.BossGuids.Length; k++)
					{
						if (bossChallengeData2.BossGuids[k] == activeEnemies[i].EnemyGuid)
						{
							bossChallengeData = bossChallengeData2;
							break;
						}
					}
				}
			}
			if (bossChallengeData != null)
			{
				numToAdd = bossChallengeData.NumToSelect;
				int num = 0;
				while (m_activeChallenges.Count < numToAdd && num < 10000)
				{
					num++;
					ChallengeModifier challengeModifier = bossChallengeData.Modifiers[UnityEngine.Random.Range(0, bossChallengeData.Modifiers.Length)];
					bool flag = challengeModifier.IsValid(currentRoom);
					for (int l = 0; l < m_activeChallenges.Count; l++)
					{
						if (flag && m_activeChallenges[l].DisplayName == challengeModifier.DisplayName)
						{
							flag = false;
						}
						if (flag && m_activeChallenges[l].MutuallyExclusive.Contains(challengeModifier))
						{
							flag = false;
						}
					}
					if (flag)
					{
						ChallengeModifier component = UnityEngine.Object.Instantiate(challengeModifier.gameObject).GetComponent<ChallengeModifier>();
						m_activeChallenges.Add(component);
					}
				}
			}
		}
		if (m_activeChallenges.Count != 0)
		{
			return;
		}
		int num2 = numToAdd;
		int num3 = 0;
		GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
		while (num2 > 0 && num3 < 10000)
		{
			num3++;
			ChallengeDataEntry challengeDataEntry = PossibleChallenges[UnityEngine.Random.Range(0, PossibleChallenges.Count)];
			ChallengeModifier challenge = challengeDataEntry.challenge;
			bool flag2 = challenge != null && challenge.IsValid(currentRoom) && challengeDataEntry.GetWeightForFloor(tilesetId) <= num2;
			if (flag2 && (challengeDataEntry.excludedTilesets | tilesetId) == challengeDataEntry.excludedTilesets)
			{
				flag2 = false;
			}
			if (flag2 && currentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && !challenge.ValidInBossChambers)
			{
				flag2 = false;
			}
			for (int m = 0; m < m_activeChallenges.Count; m++)
			{
				if (flag2 && m_activeChallenges[m].DisplayName == challenge.DisplayName)
				{
					flag2 = false;
				}
				if (flag2 && m_activeChallenges[m].MutuallyExclusive.Contains(challenge))
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				ChallengeModifier component2 = UnityEngine.Object.Instantiate(challenge.gameObject).GetComponent<ChallengeModifier>();
				m_activeChallenges.Add(component2);
				num2 -= challengeDataEntry.GetWeightForFloor(tilesetId);
			}
		}
	}

	public void ForceStop()
	{
		CleanupChallenges();
	}

	private void CleanupChallenges()
	{
		bool flag = false;
		if (m_activeChallenges.Count > 0 && (bool)GameManager.Instance.PrimaryPlayer)
		{
			flag = true;
			AkSoundEngine.PostEvent("Play_UI_challenge_clear_01", GameManager.Instance.PrimaryPlayer.gameObject);
		}
		for (int i = 0; i < m_activeChallenges.Count; i++)
		{
			if ((bool)m_activeChallenges[i])
			{
				m_activeChallenges[i].ShatterIcon(ChallengeBurstClip);
				UnityEngine.Object.Destroy(m_activeChallenges[i].gameObject);
			}
		}
		m_activeChallenges.Clear();
		if (flag && ChallengeMode == ChallengeModeType.GunslingKingTemporary)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
