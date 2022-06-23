using System;
using System.Collections;
using System.Collections.Generic;
using Brave;
using Dungeonator;
using HutongGames.PlayMaker.Actions;
using InControl;
using Pathfinding;
using tk2dRuntime.TileMap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : BraveBehaviour
{
	public enum ControlType
	{
		KEYBOARD,
		CONTROLLER
	}

	public enum GameMode
	{
		NORMAL,
		SHORTCUT,
		BOSSRUSH,
		SUPERBOSSRUSH
	}

	public enum GameType
	{
		SINGLE_PLAYER,
		COOP_2_PLAYER
	}

	public enum LevelOverrideState
	{
		NONE,
		FOYER,
		TUTORIAL,
		RESOURCEFUL_RAT,
		END_TIMES,
		CHARACTER_PAST,
		DEBUG_TEST
	}

	public static bool BackgroundGenerationActive;

	public static bool DivertResourcesToGeneration;

	public static bool IsShuttingDown;

	public const int EEVEE_META_COST = 5;

	public const int GUNSLINGER_META_COST = 7;

	public const bool c_RESOURCEFUL_RAT_ACTIVE = false;

	public const float CUSTOM_CULL_SQR_DIST_THRESHOLD = 420f;

	private static string DEBUG_LABEL;

	public static string SEED_LABEL = string.Empty;

	public const float SCENE_TRANSITION_TIME = 0.15f;

	public static bool AUDIO_ENABLED;

	public static float PIT_DEPTH = -2.5f;

	public static float INVARIANT_DELTA_TIME;

	public static bool SKIP_FOYER;

	public static bool PVP_ENABLED;

	public static bool IsBossIntro;

	public PlatformInterface platformInterface;

	private Coroutine CurrentResolutionShiftCoroutine;

	private static GameObject m_playerPrefabForNewGame;

	private static GameObject m_coopPlayerPrefabForNewGame;

	public static GameObject LastUsedPlayerPrefab;

	public static GameObject LastUsedCoopPlayerPrefab;

	private static GameOptions m_options;

	private static GameManager mr_manager;

	private static InControlManager m_inputManager;

	private DungeonFloorMusicController m_dungeonMusicController;

	public static bool PreventGameManagerExistence;

	public RewardManager CurrentRewardManager;

	public RewardManager OriginalRewardManager;

	public AdvancedSynergyDatabase SynergyManager;

	public MetaInjectionData GlobalInjectionData;

	[NonSerialized]
	public InputDevice LastUsedInputDeviceForConversation;

	public tk2dFontData DefaultAlienConversationFont;

	public tk2dFontData DefaultNormalConversationFont;

	public int RandomIntForCurrentRun;

	[NonSerialized]
	public bool IsLoadingFirstShortcutFloor;

	public int LastShortcutFloorLoaded;

	private bool m_forceSeedUpdate;

	[NonSerialized]
	private int m_currentRunSeed;

	private bool m_paused;

	private bool m_unpausedThisFrame;

	private bool m_pauseLockedCamera;

	private bool m_loadingLevel;

	private bool m_isFoyer;

	private GameMode m_currentGameMode;

	public ControlType controlType;

	private GameType m_currentGameType;

	public bool IsSelectingCharacter;

	private LevelOverrideState? m_generatingLevelState;

	public static bool IsCoopPast;

	public static bool IsGunslingerPast;

	private Dungeon m_dungeon;

	public Dungeon CurrentlyGeneratingDungeonPrefab;

	public DungeonData PregeneratedDungeonData;

	public Dungeon DungeonToAutoLoad;

	private CameraController m_camera;

	private PlayerController[] m_players;

	public int LastPausingPlayerID = -1;

	private PlayerController m_player;

	private PlayerController m_secondaryPlayer;

	[NonSerialized]
	public List<string> ExtantShopTrackableGuids = new List<string>();

	public List<GameLevelDefinition> dungeonFloors;

	public List<GameLevelDefinition> customFloors;

	private GameLevelDefinition m_lastLoadedLevelDefinition;

	private int nextLevelIndex = 1;

	[NonSerialized]
	private string m_injectedFlowPath;

	[NonSerialized]
	private string m_injectedLevelName;

	private bool m_preventUnpausing;

	public RunData RunData = new RunData();

	protected static float m_deltaTime;

	protected static float m_lastFrameRealtime;

	private bool m_applicationHasFocus = true;

	private static Vector4 s_bossIntroTime;

	private static int s_bossIntroTimeId = -1;

	private const int c_framesToCount = 4;

	private CircularBuffer<float> m_frameTimes = new CircularBuffer<float>(4);

	private AkAudioListener m_audioListener;

	public int TargetQuickRestartLevel = -1;

	public static bool ForceQuickRestartAlternateCostumeP1;

	public static bool ForceQuickRestartAlternateCostumeP2;

	public static bool ForceQuickRestartAlternateGunP1;

	public static bool ForceQuickRestartAlternateGunP2;

	public static bool IsReturningToFoyerWithPlayer;

	private bool m_preparingToDestroyThisGameManagerOnCollision;

	private bool m_shouldDestroyThisGameManagerOnCollision;

	private AsyncOperation m_preDestroyAsyncHolder;

	private GameObject m_preDestroyLoadingHierarchyHolder;

	private Type[] BraveLevelLoadedListeners = new Type[10]
	{
		typeof(PlayerController),
		typeof(SpeculativeRigidbody),
		typeof(GameUIBlankController),
		typeof(AmmonomiconDeathPageController),
		typeof(GameUIHeartController),
		typeof(RingOfResourcefulRatItem),
		typeof(ReturnAmmoOnMissedShotItem),
		typeof(PlatinumBulletsItem),
		typeof(dfPoolManager),
		typeof(ChamberGunProcessor)
	};

	private const float PIXELATE_TIME = 0.15f;

	private const float PIXELATE_FADE_TARGET = 1f;

	private const float DEPIXELATE_TIME = 0.075f;

	private static float c_asyncSoundStartTime;

	private static int c_asyncSoundStartFrame;

	private static bool m_hasEnsuredHeapSize;

	private bool m_initializedDeviceCallbacks;

	public bool PREVENT_MAIN_MENU_TEXT;

	public bool DEBUG_UI_VISIBLE = true;

	public bool DAVE_INFO_VISIBLE;

	[Header("Convenient Balance Numbers")]
	public float COOP_ENEMY_HEALTH_MULTIPLIER = 1.25f;

	public float COOP_ENEMY_PROJECTILE_SPEED_MULTIPLIER = 0.9f;

	public float DUAL_WIELDING_DAMAGE_FACTOR = 0.75f;

	public float[] PierceDamageScaling;

	public BloodthirstSettings BloodthirstOptions;

	public List<AGDEnemyReplacementTier> EnemyReplacementTiers;

	[PickupIdentifier]
	public List<int> RainbowRunForceIncludedIDs;

	[PickupIdentifier]
	public List<int> RainbowRunForceExcludedIDs;

	private bool m_bgChecksActive;

	private HashSet<string> m_knownEncounterables = new HashSet<string>();

	private List<string> m_queuedUnlocks = new List<string>();

	private List<string> m_newQueuedUnlocks = new List<string>();

	private const int NUM_ENCOUNTERABLES_CHECKED_PER_FRAME = 20;

	public static GameObject PlayerPrefabForNewGame
	{
		get
		{
			return m_playerPrefabForNewGame;
		}
		set
		{
			m_playerPrefabForNewGame = value;
			if (m_playerPrefabForNewGame != null)
			{
				PlayableCharacters characterIdentity = m_playerPrefabForNewGame.GetComponent<PlayerController>().characterIdentity;
				if (characterIdentity != PlayableCharacters.Eevee && characterIdentity != PlayableCharacters.Gunslinger)
				{
					Options.LastPlayedCharacter = characterIdentity;
				}
				LastUsedPlayerPrefab = m_playerPrefabForNewGame;
				if ((bool)LastUsedPlayerPrefab && (bool)LastUsedPlayerPrefab.GetComponent<PlayerSpaceshipController>())
				{
					LastUsedPlayerPrefab = (GameObject)BraveResources.Load("PlayerRogue");
				}
			}
		}
	}

	public static GameObject CoopPlayerPrefabForNewGame
	{
		get
		{
			return BraveResources.Load("PlayerCoopCultist") as GameObject;
		}
		set
		{
			m_coopPlayerPrefabForNewGame = value;
			if (m_coopPlayerPrefabForNewGame != null)
			{
				LastUsedCoopPlayerPrefab = m_coopPlayerPrefabForNewGame;
			}
		}
	}

	public static GameOptions Options
	{
		get
		{
			if (m_options == null)
			{
				DebugTime.RecordStartTime();
				GameOptions.Load();
				DebugTime.Log("Load game options");
			}
			return m_options;
		}
		set
		{
			m_options = value;
		}
	}

	public DungeonFloorMusicController DungeonMusicController
	{
		get
		{
			return m_dungeonMusicController;
		}
	}

	public static GameManager Instance
	{
		get
		{
			if (PreventGameManagerExistence)
			{
				return null;
			}
			if (mr_manager == null)
			{
				mr_manager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
			}
			if (mr_manager == null)
			{
				Debug.Log("INSTANTRON");
				GameObject gameObject = new GameObject("_GameManager(temp)");
				mr_manager = gameObject.AddComponent<GameManager>();
			}
			return mr_manager;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return mr_manager != null && (bool)mr_manager;
		}
	}

	public static bool IsReturningToBreach { get; set; }

	public BossManager BossManager
	{
		get
		{
			return BraveResources.Load<BossManager>("AAA_BOSS_MANAGER", ".asset");
		}
	}

	public RewardManager RewardManager
	{
		get
		{
			if (Options.CurrentGameLootProfile == GameOptions.GameLootProfile.ORIGINAL)
			{
				return OriginalRewardManager;
			}
			return CurrentRewardManager;
		}
	}

	public int CurrentRunSeed
	{
		get
		{
			return m_currentRunSeed;
		}
		set
		{
			int currentRunSeed = m_currentRunSeed;
			Debug.LogError("SETTING GLOBAL RUN SEED TO: " + value);
			m_currentRunSeed = value;
			UnityEngine.Random.InitState(m_currentRunSeed);
			BraveRandom.IgnoreGenerationDifferentiator = true;
			BraveRandom.InitializeWithSeed(value);
			if (m_currentRunSeed != currentRunSeed || m_forceSeedUpdate)
			{
				m_forceSeedUpdate = false;
				Debug.LogError("DOING STARTUP SEED DATA");
				MetaInjectionData.ClearBlueprint();
				Instance.GlobalInjectionData.PreprocessRun();
				RewardManifest.ClearManifest(RewardManager);
				RewardManifest.Initialize(RewardManager);
			}
		}
	}

	public bool IsSeeded
	{
		get
		{
			return m_currentRunSeed != 0;
		}
	}

	public bool IsPaused
	{
		get
		{
			return m_paused;
		}
	}

	public bool UnpausedThisFrame
	{
		get
		{
			return m_unpausedThisFrame;
		}
	}

	public bool IsLoadingLevel
	{
		get
		{
			return m_loadingLevel;
		}
		private set
		{
			if (!value)
			{
				IsReturningToBreach = false;
			}
			m_loadingLevel = value;
		}
	}

	public bool IsFoyer
	{
		get
		{
			return m_isFoyer;
		}
		set
		{
			m_isFoyer = value;
			if (!m_isFoyer)
			{
				Foyer.ClearInstance();
			}
		}
	}

	public static bool IsTurboMode
	{
		get
		{
			if (GameStatsManager.HasInstance)
			{
				return GameStatsManager.Instance.isTurboMode;
			}
			return false;
		}
	}

	public GameMode CurrentGameMode
	{
		get
		{
			return m_currentGameMode;
		}
		set
		{
			m_currentGameMode = value;
		}
	}

	public GameType CurrentGameType
	{
		get
		{
			return m_currentGameType;
		}
		set
		{
			m_currentGameType = value;
		}
	}

	public LevelOverrideState GeneratingLevelOverrideState
	{
		get
		{
			if (m_generatingLevelState.HasValue)
			{
				return m_generatingLevelState.Value;
			}
			return CurrentLevelOverrideState;
		}
	}

	public LevelOverrideState CurrentLevelOverrideState
	{
		get
		{
			if (IsLoadingLevel && CurrentlyGeneratingDungeonPrefab != null)
			{
				return CurrentlyGeneratingDungeonPrefab.LevelOverrideType;
			}
			if (Dungeon == null)
			{
				if (BestGenerationDungeonPrefab != null)
				{
					return BestGenerationDungeonPrefab.LevelOverrideType;
				}
				return LevelOverrideState.NONE;
			}
			if (Dungeon.IsEndTimes)
			{
				return LevelOverrideState.END_TIMES;
			}
			return Dungeon.LevelOverrideType;
		}
	}

	public int CurrentFloor
	{
		get
		{
			if (IsFoyer)
			{
				return 0;
			}
			GameLevelDefinition lastLoadedLevelDefinition = GetLastLoadedLevelDefinition();
			int result = -1;
			if (lastLoadedLevelDefinition != null)
			{
				result = dungeonFloors.IndexOf(lastLoadedLevelDefinition);
			}
			return result;
		}
	}

	public Dungeon Dungeon
	{
		get
		{
			if (m_dungeon == null)
			{
				m_dungeon = UnityEngine.Object.FindObjectOfType<Dungeon>();
			}
			return m_dungeon;
		}
	}

	public Dungeon BestGenerationDungeonPrefab
	{
		get
		{
			if (IsLoadingLevel && CurrentlyGeneratingDungeonPrefab != null)
			{
				return CurrentlyGeneratingDungeonPrefab;
			}
			return m_dungeon;
		}
	}

	public CameraController MainCameraController
	{
		get
		{
			if (m_camera == null)
			{
				GameObject gameObject = GameObject.Find("Main Camera");
				if (gameObject != null)
				{
					m_camera = gameObject.GetComponent<CameraController>();
				}
				else if ((bool)Camera.main)
				{
					m_camera = Camera.main.GetComponent<CameraController>();
				}
			}
			return m_camera;
		}
	}

	public PlayerController[] AllPlayers
	{
		get
		{
			if (m_players != null)
			{
				for (int i = 0; i < m_players.Length; i++)
				{
					if (!m_players[i])
					{
						m_player = null;
						m_secondaryPlayer = null;
						m_players = null;
						break;
					}
				}
			}
			if (m_players == null || (m_players.Length == 0 && !IsSelectingCharacter))
			{
				List<PlayerController> list = new List<PlayerController>(UnityEngine.Object.FindObjectsOfType<PlayerController>());
				for (int j = 0; j < list.Count; j++)
				{
					if (!list[j])
					{
						list.RemoveAt(j);
						j--;
					}
				}
				m_players = list.ToArray();
				if (m_players != null && m_players.Length == 2 && m_players[1].IsPrimaryPlayer)
				{
					PlayerController playerController = m_players[0];
					m_players[0] = m_players[1];
					m_players[1] = playerController;
				}
			}
			return m_players;
		}
	}

	public int NumberOfLivingPlayers
	{
		get
		{
			int num = 0;
			for (int i = 0; i < AllPlayers.Length; i++)
			{
				if (!AllPlayers[i].IsGhost && !AllPlayers[i].healthHaver.IsDead)
				{
					num++;
				}
			}
			return num;
		}
	}

	public PlayerController PrimaryPlayer
	{
		get
		{
			if (IsSelectingCharacter && IsFoyer)
			{
				return null;
			}
			if (IsReturningToBreach && !IsReturningToFoyerWithPlayer && IsFoyer)
			{
				return null;
			}
			if (m_player == null)
			{
				PlayerController[] array = UnityEngine.Object.FindObjectsOfType<PlayerController>();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].IsPrimaryPlayer)
					{
						m_player = array[i];
						break;
					}
				}
			}
			return m_player;
		}
		set
		{
			m_player = value;
			if (m_players != null && m_players.Length > 0)
			{
				m_players[0] = value;
			}
		}
	}

	public PlayerController SecondaryPlayer
	{
		get
		{
			if (Instance.CurrentGameType == GameType.SINGLE_PLAYER)
			{
				return null;
			}
			if (m_secondaryPlayer == null)
			{
				for (int i = 0; i < AllPlayers.Length; i++)
				{
					if (!AllPlayers[i].IsPrimaryPlayer && AllPlayers[i].characterIdentity == PlayableCharacters.CoopCultist)
					{
						m_secondaryPlayer = AllPlayers[i];
						break;
					}
				}
			}
			return m_secondaryPlayer;
		}
		set
		{
			m_secondaryPlayer = value;
			if (m_players != null && m_players.Length > 1)
			{
				m_players[1] = value;
			}
			if (m_players != null && m_players.Length < 2)
			{
				m_players = null;
			}
		}
	}

	public PlayerController BestActivePlayer
	{
		get
		{
			if (!PrimaryPlayer && !SecondaryPlayer)
			{
				return null;
			}
			if (PrimaryPlayer.IsGhost || PrimaryPlayer.healthHaver.IsDead)
			{
				return SecondaryPlayer;
			}
			return PrimaryPlayer;
		}
	}

	public string InjectedFlowPath
	{
		get
		{
			return m_injectedFlowPath;
		}
		set
		{
			m_injectedFlowPath = value;
		}
	}

	public string InjectedLevelName
	{
		get
		{
			return m_injectedLevelName;
		}
		set
		{
			m_injectedLevelName = value;
		}
	}

	public bool PreventPausing
	{
		get
		{
			return m_preventUnpausing;
		}
		set
		{
			if (m_preventUnpausing != value)
			{
				m_preventUnpausing = value;
				if (!m_preventUnpausing && !m_applicationHasFocus && !m_loadingLevel && !m_paused)
				{
					Pause();
					GameStatsManager.Save();
				}
			}
		}
	}

	public bool InTutorial { get; set; }

	public bool ShouldDeleteSaveOnExit
	{
		get
		{
			return !IsFoyer && !SaveManager.PreventMidgameSaveDeletionOnExit && !BackgroundGenerationActive && !Dungeon.IsGenerating;
		}
	}

	public event Action OnNewLevelFullyLoaded;

	public void DoSetResolution(int newWidth, int newHeight, bool newFullscreen)
	{
		Debug.Log("Setting RESOLUTION internal to: " + newWidth + "|" + newHeight + "|" + newFullscreen);
		if (newFullscreen != Screen.fullScreen)
		{
			bool flag = newFullscreen == Screen.fullScreen;
			Screen.SetResolution(newWidth, newHeight, newFullscreen);
			if (flag)
			{
				if (CurrentResolutionShiftCoroutine != null)
				{
					StopCoroutine(CurrentResolutionShiftCoroutine);
				}
				CurrentResolutionShiftCoroutine = StartCoroutine(SetResolutionPostFullscreenChange(newWidth, newHeight));
			}
		}
		else
		{
			Screen.SetResolution(newWidth, newHeight, Screen.fullScreen, Screen.currentResolution.refreshRate);
		}
	}

	private IEnumerator SetResolutionPostFullscreenChange(int newWidth, int newHeight)
	{
		float delay = 0f;
		yield return null;
		yield return null;
		int lastW = Screen.width;
		int lastH = Screen.height;
		float ela = 0f;
		if (delay > 0f)
		{
			while (ela < delay)
			{
				ela += INVARIANT_DELTA_TIME;
				if (lastW != Screen.width || lastH != Screen.height)
				{
					Screen.SetResolution(newWidth, newHeight, Screen.fullScreen);
					break;
				}
				yield return null;
			}
		}
		else
		{
			Screen.SetResolution(newWidth, newHeight, Screen.fullScreen);
		}
		CurrentResolutionShiftCoroutine = null;
	}

	public static void LoadResolutionFromPS4()
	{
	}

	private static void LoadResolutionFromOptions()
	{
		if (IsReturningToBreach)
		{
			return;
		}
		GameOptions.PreferredFullscreenMode currentPreferredFullscreenMode = Options.CurrentPreferredFullscreenMode;
		if (Options.preferredResolutionX <= 50 || Options.preferredResolutionY <= 50 || Options.preferredResolutionX > 50000 || Options.preferredResolutionY > 50000)
		{
			Options.preferredResolutionX = -1;
			Options.preferredResolutionY = -1;
		}
		if (Options.preferredResolutionX <= 0)
		{
			Resolution resolution = Screen.resolutions[Screen.resolutions.Length - 1];
			Options.preferredResolutionX = resolution.width;
			Options.preferredResolutionY = resolution.height;
		}
		Resolution resolution2 = default(Resolution);
		resolution2.width = Options.preferredResolutionX;
		resolution2.height = Options.preferredResolutionY;
		resolution2.refreshRate = Screen.currentResolution.refreshRate;
		if (currentPreferredFullscreenMode != 0)
		{
			BraveOptionsMenuItem.WindowsResolutionManager.DisplayModes targetDisplayMode = BraveOptionsMenuItem.WindowsResolutionManager.DisplayModes.Fullscreen;
			if (currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.BORDERLESS)
			{
				targetDisplayMode = BraveOptionsMenuItem.WindowsResolutionManager.DisplayModes.Borderless;
			}
			if (currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.WINDOWED)
			{
				targetDisplayMode = BraveOptionsMenuItem.WindowsResolutionManager.DisplayModes.Windowed;
			}
			bool flag = Screen.fullScreen != (currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN);
			Debug.Log("Invoking standard WIN startup methods to set fullscreen: " + flag);
			if (flag)
			{
				BraveOptionsMenuItem componentInChildren = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.GetComponentInChildren<BraveOptionsMenuItem>();
				componentInChildren.StartCoroutine(componentInChildren.FrameDelayedWindowsShift(targetDisplayMode, resolution2));
			}
			else
			{
				BraveOptionsMenuItem.ResolutionManagerWin.TrySetDisplay(targetDisplayMode, resolution2, false, null);
			}
		}
		if (resolution2.width != Screen.width || resolution2.height != Screen.height || currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN != Screen.fullScreen)
		{
			Debug.Log("Invoking standard startup methods to set resolution: " + resolution2.width + "|" + resolution2.height + "||" + currentPreferredFullscreenMode.ToString());
			Instance.DoSetResolution(resolution2.width, resolution2.height, currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN);
		}
	}

	public static GameManager EnsureExistence()
	{
		if (Instance == null)
		{
			return Instance;
		}
		return Instance;
	}

	public void ClearGenerativeDungeonData()
	{
		CurrentlyGeneratingDungeonPrefab = null;
		PregeneratedDungeonData = null;
	}

	public bool HasPlayer(PlayerController p)
	{
		for (int i = 0; i < AllPlayers.Length; i++)
		{
			if (AllPlayers[i] == p)
			{
				return true;
			}
		}
		return false;
	}

	public bool PlayerIsInRoom(RoomHandler targetRoom)
	{
		for (int i = 0; i < AllPlayers.Length; i++)
		{
			if (AllPlayers[i].CurrentRoom == targetRoom)
			{
				return true;
			}
		}
		return false;
	}

	public bool PlayerIsNearRoom(RoomHandler targetRoom)
	{
		for (int i = 0; i < AllPlayers.Length; i++)
		{
			if (AllPlayers[i].CurrentRoom == targetRoom || (AllPlayers[i].CurrentRoom != null && AllPlayers[i].CurrentRoom.connectedRooms != null && AllPlayers[i].CurrentRoom.connectedRooms.Contains(targetRoom)))
			{
				return true;
			}
		}
		return false;
	}

	public void RefreshAllPlayers()
	{
		m_players = null;
		m_players = AllPlayers;
	}

	public PlayerController GetOtherPlayer(PlayerController p)
	{
		if (CurrentGameType == GameType.SINGLE_PLAYER)
		{
			return null;
		}
		return (!(p == PrimaryPlayer)) ? PrimaryPlayer : SecondaryPlayer;
	}

	public GlobalDungeonData.ValidTilesets GetNextTileset(GlobalDungeonData.ValidTilesets tilesetID)
	{
		switch (tilesetID)
		{
		case GlobalDungeonData.ValidTilesets.CASTLEGEON:
			return GlobalDungeonData.ValidTilesets.GUNGEON;
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			return GlobalDungeonData.ValidTilesets.MINEGEON;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			return GlobalDungeonData.ValidTilesets.CATACOMBGEON;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			return GlobalDungeonData.ValidTilesets.FORGEGEON;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			return GlobalDungeonData.ValidTilesets.HELLGEON;
		case GlobalDungeonData.ValidTilesets.RATGEON:
			return GlobalDungeonData.ValidTilesets.CATACOMBGEON;
		case GlobalDungeonData.ValidTilesets.OFFICEGEON:
			return GlobalDungeonData.ValidTilesets.FORGEGEON;
		case GlobalDungeonData.ValidTilesets.SEWERGEON:
			return GlobalDungeonData.ValidTilesets.GUNGEON;
		case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
			return GlobalDungeonData.ValidTilesets.MINEGEON;
		default:
			return GlobalDungeonData.ValidTilesets.CASTLEGEON;
		}
	}

	public int GetTargetLevelIndexFromSavedTileset(GlobalDungeonData.ValidTilesets tilesetID)
	{
		switch (tilesetID)
		{
		case GlobalDungeonData.ValidTilesets.CASTLEGEON:
			return 1;
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			return 2;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			return 3;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			return 4;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			return 5;
		case GlobalDungeonData.ValidTilesets.HELLGEON:
			return 6;
		case GlobalDungeonData.ValidTilesets.SEWERGEON:
			return 2;
		case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
			return 3;
		case GlobalDungeonData.ValidTilesets.RATGEON:
			return 4;
		case GlobalDungeonData.ValidTilesets.OFFICEGEON:
			return 5;
		case GlobalDungeonData.ValidTilesets.FINALGEON:
			return 6;
		default:
			return 1;
		}
	}

	public void SetNextLevelIndex(int index)
	{
		nextLevelIndex = index;
	}

	public void OnApplicationQuit()
	{
		IsShuttingDown = true;
		if (ShouldDeleteSaveOnExit)
		{
			SaveManager.DeleteCurrentSlotMidGameSave();
		}
	}

	public void OnApplicationFocus(bool focusStatus)
	{
		if (!Application.isEditor && !MemoryTester.HasInstance)
		{
			if (!focusStatus && PrimaryPlayer != null && !PreventPausing && !m_loadingLevel && !m_paused)
			{
				Pause();
				GameStatsManager.Save();
			}
			m_applicationHasFocus = focusStatus;
		}
	}

	protected void Update()
	{
		tk2dSpriteAnimator.InDungeonScene = m_dungeon != null;
		BraveTime.UpdateScaledTimeSinceStartup();
		if (IsBossIntro)
		{
			if (s_bossIntroTimeId < 0)
			{
				s_bossIntroTimeId = Shader.PropertyToID("_BossIntroTime");
			}
			s_bossIntroTime += new Vector4(0.05f, 1f, 2f, 3f) * INVARIANT_DELTA_TIME;
			Shader.SetGlobalVector(s_bossIntroTimeId, s_bossIntroTime);
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		m_deltaTime = Time.unscaledDeltaTime;
		m_lastFrameRealtime = realtimeSinceStartup;
		INVARIANT_DELTA_TIME = m_deltaTime;
		InvariantUpdate(m_deltaTime);
		m_frameTimes.Enqueue(INVARIANT_DELTA_TIME);
		for (int i = 0; i < StaticReferenceManager.AllClusteredTimeInvariantBehaviours.Count; i++)
		{
			ClusteredTimeInvariantMonoBehaviour clusteredTimeInvariantMonoBehaviour = StaticReferenceManager.AllClusteredTimeInvariantBehaviours[i];
			if (!clusteredTimeInvariantMonoBehaviour)
			{
				StaticReferenceManager.AllClusteredTimeInvariantBehaviours.RemoveAt(i);
				i--;
			}
			else
			{
				clusteredTimeInvariantMonoBehaviour.DoUpdate(INVARIANT_DELTA_TIME);
			}
		}
		if (AUDIO_ENABLED)
		{
			if (!m_player && !m_audioListener)
			{
				Debug.LogWarning("Adding a new GameManager audio listener");
				m_audioListener = base.gameObject.GetOrAddComponent<AkAudioListener>();
			}
			else if ((bool)m_player && (bool)m_audioListener)
			{
				Debug.LogWarning("Destroying the GameManager's audio listener");
				UnityEngine.Object.Destroy(m_audioListener);
				m_audioListener = null;
			}
		}
	}

	private void LateUpdate()
	{
		platformInterface.LateUpdate();
		m_unpausedThisFrame = false;
	}

	public PlayerController GetPlayerClosestToPoint(Vector2 point)
	{
		PlayerController result = null;
		float num = float.MaxValue;
		for (int i = 0; i < AllPlayers.Length; i++)
		{
			if (!AllPlayers[i].healthHaver.IsDead)
			{
				float num2 = Vector2.Distance(point, AllPlayers[i].CenterPosition);
				if (num2 < num)
				{
					num = num2;
					result = AllPlayers[i];
				}
			}
		}
		return result;
	}

	public PlayerController GetPlayerClosestToPoint(Vector2 point, out float range)
	{
		PlayerController result = null;
		float num = float.MaxValue;
		for (int i = 0; i < AllPlayers.Length; i++)
		{
			if (!AllPlayers[i].healthHaver.IsDead)
			{
				float num2 = Vector2.Distance(point, AllPlayers[i].CenterPosition);
				if (num2 < num)
				{
					num = num2;
					result = AllPlayers[i];
				}
			}
		}
		range = num;
		return result;
	}

	public PlayerController GetActivePlayerClosestToPoint(Vector2 point, bool allowStealth = false)
	{
		if (IsSelectingCharacter)
		{
			return null;
		}
		if (!IsReturningToFoyerWithPlayer && IsReturningToBreach && IsFoyer)
		{
			return null;
		}
		PlayerController result = null;
		float num = float.MaxValue;
		for (int i = 0; i < AllPlayers.Length; i++)
		{
			if (!AllPlayers[i].IsGhost && !AllPlayers[i].healthHaver.IsDead && !AllPlayers[i].IsFalling && !AllPlayers[i].IsCurrentlyCoopReviving && (allowStealth || !AllPlayers[i].IsStealthed))
			{
				float num2 = Vector2.Distance(point, AllPlayers[i].CenterPosition);
				if (num2 < num)
				{
					num = num2;
					result = AllPlayers[i];
				}
			}
		}
		return result;
	}

	public bool IsAnyPlayerInRoom(RoomHandler room)
	{
		for (int i = 0; i < AllPlayers.Length; i++)
		{
			if (!AllPlayers[i].healthHaver.IsDead && AllPlayers[i].CurrentRoom == room)
			{
				return true;
			}
		}
		return false;
	}

	public PlayerController GetRandomActivePlayer()
	{
		if (CurrentGameType == GameType.COOP_2_PLAYER)
		{
			if (PrimaryPlayer.healthHaver.IsAlive && SecondaryPlayer.healthHaver.IsAlive)
			{
				return (!(UnityEngine.Random.value > 0.5f)) ? SecondaryPlayer : PrimaryPlayer;
			}
			return BestActivePlayer;
		}
		return PrimaryPlayer;
	}

	public PlayerController GetRandomPlayer()
	{
		return AllPlayers[UnityEngine.Random.Range(0, AllPlayers.Length)];
	}

	public void DelayedQuickRestart(float duration, QuickRestartOptions options = default(QuickRestartOptions))
	{
		StartCoroutine(DelayedQuickRestart_CR(duration, options));
	}

	private IEnumerator DelayedQuickRestart_CR(float duration, QuickRestartOptions options)
	{
		if (!m_loadingLevel)
		{
			m_loadingLevel = true;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += INVARIANT_DELTA_TIME;
				yield return null;
			}
			QuickRestart(options);
		}
	}

	public void QuickRestart(QuickRestartOptions options = default(QuickRestartOptions))
	{
		if (m_paused)
		{
			ForceUnpause();
		}
		m_loadingLevel = true;
		ChallengeManager componentInChildren = GetComponentInChildren<ChallengeManager>();
		if ((bool)componentInChildren)
		{
			UnityEngine.Object.Destroy(componentInChildren.gameObject);
		}
		SaveManager.DeleteCurrentSlotMidGameSave();
		if (options.BossRush)
		{
			CurrentGameMode = GameMode.BOSSRUSH;
		}
		else if (CurrentGameMode == GameMode.BOSSRUSH)
		{
			CurrentGameMode = GameMode.NORMAL;
		}
		bool flag = Instance.CurrentGameMode == GameMode.SHORTCUT;
		if (PrimaryPlayer != null)
		{
			ForceQuickRestartAlternateCostumeP1 = PrimaryPlayer.IsUsingAlternateCostume;
			ForceQuickRestartAlternateGunP1 = PrimaryPlayer.UsingAlternateStartingGuns;
		}
		if (CurrentGameType == GameType.COOP_2_PLAYER && SecondaryPlayer != null)
		{
			ForceQuickRestartAlternateCostumeP2 = SecondaryPlayer.IsUsingAlternateCostume;
			ForceQuickRestartAlternateGunP2 = SecondaryPlayer.UsingAlternateStartingGuns;
		}
		ClearPerLevelData();
		FlushAudio();
		ClearActiveGameData(false, true);
		if (TargetQuickRestartLevel != -1)
		{
			nextLevelIndex = TargetQuickRestartLevel;
		}
		else
		{
			nextLevelIndex = 1;
			if (flag)
			{
				nextLevelIndex += LastShortcutFloorLoaded;
				IsLoadingFirstShortcutFloor = true;
			}
		}
		if (LastUsedPlayerPrefab != null)
		{
			PlayerPrefabForNewGame = LastUsedPlayerPrefab;
			PlayerController component = PlayerPrefabForNewGame.GetComponent<PlayerController>();
			GameStatsManager.Instance.BeginNewSession(component);
		}
		if (CurrentGameType == GameType.COOP_2_PLAYER && LastUsedCoopPlayerPrefab != null)
		{
			CoopPlayerPrefabForNewGame = LastUsedCoopPlayerPrefab;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		m_preventUnpausing = false;
		if (m_currentRunSeed != 0)
		{
			m_forceSeedUpdate = true;
			CurrentRunSeed = CurrentRunSeed;
		}
		Debug.Log("Quick Restarting...");
		if (CurrentGameMode == GameMode.BOSSRUSH)
		{
			SetNextLevelIndex(1);
			InstantLoadBossRushFloor(1);
			nextLevelIndex++;
		}
		else
		{
			Instance.LoadNextLevel();
		}
		StartCoroutine(PostQuickStartCR(options));
	}

	private IEnumerator PostQuickStartCR(QuickRestartOptions options)
	{
		while (IsLoadingLevel || Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (options.GunGame)
		{
			SetExoticPlayerFlag.SetGunGame(false);
		}
		if (options.ChallengeMode != 0)
		{
			ChallengeManager.ChallengeModeType = options.ChallengeMode;
		}
		GameStatsManager.Instance.SetStat(TrackedStats.TIME_PLAYED, 0f);
	}

	public AsyncOperation BeginAsyncLoadMainMenu()
	{
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("MainMenu");
		asyncOperation.allowSceneActivation = false;
		return asyncOperation;
	}

	public void EndAsyncLoadMainMenu(AsyncOperation loader)
	{
		if (m_paused)
		{
			ForceUnpause();
		}
		ClearPerLevelData();
		FlushAudio();
		ClearActiveGameData(true, false);
		m_preventUnpausing = false;
		loader.allowSceneActivation = true;
	}

	public void DelayedLoadMainMenu(float duration)
	{
		StartCoroutine(DelayedLoadMainMenu_CR(duration));
	}

	private IEnumerator DelayedLoadMainMenu_CR(float duration)
	{
		if (!m_loadingLevel)
		{
			m_loadingLevel = true;
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += INVARIANT_DELTA_TIME;
				yield return null;
			}
			LoadMainMenu();
		}
	}

	public void LoadMainMenu()
	{
		if (m_paused)
		{
			ForceUnpause();
		}
		m_loadingLevel = true;
		ClearPerLevelData();
		FlushAudio();
		ClearActiveGameData(true, true);
		BraveCameraUtility.OverrideAspect = 1.77777779f;
		m_preventUnpausing = false;
		IsLoadingLevel = false;
		Foyer.DoIntroSequence = false;
		Foyer.DoMainMenu = true;
		SceneManager.LoadScene("tt_foyer");
	}

	public void FrameDelayedEnteredFoyer(PlayerController p)
	{
		if (Foyer.Instance != null)
		{
			Foyer.Instance.ProcessPlayerEnteredFoyer(p);
		}
		StartCoroutine(HandleFrameDelayedEnteredFoyer(p));
	}

	private IEnumerator HandleFrameDelayedEnteredFoyer(PlayerController p)
	{
		yield return null;
		Foyer.Instance.ProcessPlayerEnteredFoyer(p);
	}

	public void DelayedReturnToFoyer(float delay)
	{
		m_preparingToDestroyThisGameManagerOnCollision = true;
		StartCoroutine(DelayedReturnToFoyer_CR(delay));
	}

	private IEnumerator DelayedReturnToFoyer_CR(float delay)
	{
		if (!m_loadingLevel)
		{
			m_loadingLevel = true;
			yield return new WaitForSeconds(delay);
			if (GameUIRoot.Instance != null)
			{
				GameUIRoot.Instance.ToggleUICamera(false);
				yield return null;
			}
			ReturnToFoyer();
		}
	}

	public void ReturnToFoyer()
	{
		if (m_paused)
		{
			ForceUnpause();
		}
		IsReturningToFoyerWithPlayer = true;
		ClearPerLevelData();
		FlushAudio();
		nextLevelIndex = 1;
		ClearActiveGameData(false, true);
		if (LastUsedPlayerPrefab != null)
		{
			PlayerPrefabForNewGame = LastUsedPlayerPrefab;
			PlayerController component = PlayerPrefabForNewGame.GetComponent<PlayerController>();
			GameStatsManager.Instance.BeginNewSession(component);
		}
		else
		{
			Debug.LogError("Attempting to clear player data on foyer return, but LastUsedPlayer is null!");
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		m_preventUnpausing = false;
		LoadNextLevel();
	}

	public void LoadCustomFlowForDebug(string flowpath, string dungeonPrefab = "", string sceneName = "")
	{
		DungeonFlow orLoadByName = FlowDatabase.GetOrLoadByName(flowpath);
		if (orLoadByName == null)
		{
			orLoadByName = FlowDatabase.GetOrLoadByName("Boss Rooms/" + flowpath);
		}
		if (orLoadByName == null)
		{
			orLoadByName = FlowDatabase.GetOrLoadByName("Boss Rush Flows/" + flowpath);
		}
		if (orLoadByName == null)
		{
			orLoadByName = FlowDatabase.GetOrLoadByName("Testing/" + flowpath);
		}
		if (orLoadByName == null)
		{
			return;
		}
		m_loadingLevel = true;
		FlushAudio();
		ClearPerLevelData();
		float priceMultiplier = 1f;
		float enemyHealthMultiplier = 1f;
		if (!string.IsNullOrEmpty(sceneName))
		{
			for (int i = 0; i < customFloors.Count; i++)
			{
				if (customFloors[i].dungeonSceneName == sceneName)
				{
					priceMultiplier = customFloors[i].priceMultiplier;
					enemyHealthMultiplier = customFloors[i].enemyHealthMultiplier;
					break;
				}
			}
			for (int j = 0; j < dungeonFloors.Count; j++)
			{
				if (dungeonFloors[j].dungeonSceneName == sceneName)
				{
					priceMultiplier = dungeonFloors[j].priceMultiplier;
					enemyHealthMultiplier = dungeonFloors[j].enemyHealthMultiplier;
					break;
				}
			}
		}
		GameLevelDefinition gameLevelDefinition = new GameLevelDefinition();
		gameLevelDefinition.dungeonPrefabPath = ((!string.IsNullOrEmpty(dungeonPrefab)) ? dungeonPrefab : "Base_Gungeon");
		gameLevelDefinition.dungeonSceneName = ((!string.IsNullOrEmpty(sceneName)) ? sceneName : "BB_Beholster");
		gameLevelDefinition.priceMultiplier = priceMultiplier;
		gameLevelDefinition.enemyHealthMultiplier = enemyHealthMultiplier;
		Debug.Log(gameLevelDefinition.dungeonPrefabPath + "|" + gameLevelDefinition.dungeonSceneName);
		gameLevelDefinition.predefinedSeeds = new List<int>();
		gameLevelDefinition.flowEntries = new List<DungeonFlowLevelEntry>();
		DungeonFlowLevelEntry dungeonFlowLevelEntry = new DungeonFlowLevelEntry();
		dungeonFlowLevelEntry.flowPath = flowpath;
		dungeonFlowLevelEntry.forceUseIfAvailable = true;
		dungeonFlowLevelEntry.prerequisites = new DungeonPrerequisite[0];
		dungeonFlowLevelEntry.weight = 1f;
		gameLevelDefinition.flowEntries.Add(dungeonFlowLevelEntry);
		StartCoroutine(LoadNextLevelAsync_CR(gameLevelDefinition));
	}

	public void LoadCustomLevel(string custom)
	{
		if (dungeonFloors == null || dungeonFloors.Count == 0)
		{
			dungeonFloors = new List<GameLevelDefinition>();
			GameLevelDefinition gameLevelDefinition = new GameLevelDefinition();
			gameLevelDefinition.dungeonSceneName = SceneManager.GetActiveScene().name;
			dungeonFloors.Add(gameLevelDefinition);
		}
		m_loadingLevel = true;
		FlushAudio();
		ClearPerLevelData();
		GameLevelDefinition gameLevelDefinition2 = null;
		int num = -1;
		for (int i = 0; i < dungeonFloors.Count; i++)
		{
			if (dungeonFloors[i].dungeonSceneName == custom)
			{
				gameLevelDefinition2 = dungeonFloors[i];
				num = i + 1;
				break;
			}
		}
		if (gameLevelDefinition2 == null)
		{
			for (int j = 0; j < customFloors.Count; j++)
			{
				if (customFloors[j].dungeonSceneName == custom)
				{
					gameLevelDefinition2 = customFloors[j];
					break;
				}
			}
		}
		if (gameLevelDefinition2 != null && gameLevelDefinition2.dungeonPrefabPath == string.Empty)
		{
			if (gameLevelDefinition2.dungeonSceneName == "MainMenu")
			{
				LoadMainMenu();
				nextLevelIndex = 0;
			}
			else if (gameLevelDefinition2.dungeonSceneName == "Foyer")
			{
				SceneManager.LoadScene(gameLevelDefinition2.dungeonSceneName);
				IsLoadingLevel = false;
				nextLevelIndex = 1;
			}
			else
			{
				SceneManager.LoadScene(gameLevelDefinition2.dungeonSceneName);
				IsLoadingLevel = false;
			}
		}
		else
		{
			StartCoroutine(LoadNextLevelAsync_CR(gameLevelDefinition2));
			if (gameLevelDefinition2.dungeonSceneName == "tt_tutorial")
			{
				num = 0;
			}
			if (num != -1)
			{
				nextLevelIndex = num;
			}
		}
	}

	public static void InvalidateMidgameSave(bool saveStats)
	{
		MidGameSaveData midgameSave = null;
		if (VerifyAndLoadMidgameSave(out midgameSave, false))
		{
			midgameSave.Invalidate();
			SaveManager.Save(midgameSave, SaveManager.MidGameSave, GameStatsManager.Instance.PlaytimeMin);
			GameStatsManager.Instance.midGameSaveGuid = midgameSave.midGameSaveGuid;
			if (saveStats)
			{
				GameStatsManager.Save();
			}
		}
	}

	public static void RevalidateMidgameSave()
	{
		MidGameSaveData midgameSave = null;
		if (VerifyAndLoadMidgameSave(out midgameSave, false))
		{
			midgameSave.Revalidate();
			SaveManager.Save(midgameSave, SaveManager.MidGameSave, GameStatsManager.Instance.PlaytimeMin);
			GameStatsManager.Instance.midGameSaveGuid = midgameSave.midGameSaveGuid;
			GameStatsManager.Save();
		}
	}

	public static void DoMidgameSave(GlobalDungeonData.ValidTilesets tileset)
	{
		string midGameSaveGuid = Guid.NewGuid().ToString();
		MidGameSaveData obj = new MidGameSaveData(Instance.PrimaryPlayer, Instance.SecondaryPlayer, tileset, midGameSaveGuid);
		SaveManager.Save(obj, SaveManager.MidGameSave, GameStatsManager.Instance.PlaytimeMin);
		GameStatsManager.Instance.midGameSaveGuid = midGameSaveGuid;
		GameStatsManager.Save();
	}

	public static bool HasValidMidgameSave()
	{
		MidGameSaveData midgameSave;
		return VerifyAndLoadMidgameSave(out midgameSave);
	}

	public static bool VerifyAndLoadMidgameSave(out MidGameSaveData midgameSave, bool checkValidity = true)
	{
		if (!SaveManager.Load<MidGameSaveData>(SaveManager.MidGameSave, out midgameSave, true))
		{
			Debug.LogError("No mid game save found");
			return false;
		}
		if (midgameSave == null)
		{
			Debug.LogError("Failed to load mid game save (0)");
			return false;
		}
		if (checkValidity && !midgameSave.IsValid())
		{
			return false;
		}
		if (GameStatsManager.Instance.midGameSaveGuid == null || GameStatsManager.Instance.midGameSaveGuid != midgameSave.midGameSaveGuid)
		{
			Debug.LogError("Failed to load mid game save (1)");
			return false;
		}
		List<string> list = new List<string>(Brave.PlayerPrefs.GetStringArray("recent_mgs"));
		if (list.Contains(midgameSave.midGameSaveGuid))
		{
			Debug.LogError("Failed to load mid game save (2)");
			Debug.LogError(Brave.PlayerPrefs.GetString("recent_mgs"));
			Debug.LogError(midgameSave.midGameSaveGuid);
			return false;
		}
		return true;
	}

	public void DelayedLoadMidgameSave(float delay, MidGameSaveData saveToContinue)
	{
		switch (saveToContinue.levelSaved)
		{
		case GlobalDungeonData.ValidTilesets.SEWERGEON:
			DelayedLoadCustomLevel(delay, "tt_sewer");
			break;
		case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
			DelayedLoadCustomLevel(delay, "tt_cathedral");
			break;
		case GlobalDungeonData.ValidTilesets.RATGEON:
			DelayedLoadCustomLevel(delay, "ss_resourcefulrat");
			break;
		case GlobalDungeonData.ValidTilesets.OFFICEGEON:
			DelayedLoadCustomLevel(delay, "tt_nakatomi");
			break;
		case GlobalDungeonData.ValidTilesets.FINALGEON:
			switch (saveToContinue.playerOneData.CharacterIdentity)
			{
			case PlayableCharacters.Convict:
				DelayedLoadCustomLevel(delay, "fs_convict");
				break;
			case PlayableCharacters.Pilot:
				DelayedLoadCustomLevel(delay, "fs_pilot");
				break;
			case PlayableCharacters.Guide:
				DelayedLoadCustomLevel(delay, "fs_guide");
				break;
			case PlayableCharacters.Soldier:
				DelayedLoadCustomLevel(delay, "fs_soldier");
				break;
			case PlayableCharacters.Bullet:
				DelayedLoadCustomLevel(delay, "fs_bullet");
				break;
			case PlayableCharacters.Robot:
				DelayedLoadCustomLevel(delay, "fs_robot");
				break;
			case PlayableCharacters.Gunslinger:
				IsGunslingerPast = true;
				DelayedLoadCustomLevel(delay, "tt_bullethell");
				break;
			case PlayableCharacters.Ninja:
			case PlayableCharacters.Cosmonaut:
			case PlayableCharacters.CoopCultist:
			case PlayableCharacters.Eevee:
				break;
			}
			break;
		default:
			DelayedLoadNextLevel(0.25f);
			break;
		}
	}

	public void GeneratePlayersFromMidGameSave(MidGameSaveData loadedSave)
	{
		PlayerPrefabForNewGame = loadedSave.GetPlayerOnePrefab();
		GameObject gameObject = UnityEngine.Object.Instantiate(PlayerPrefabForNewGame, Vector3.zero, Quaternion.identity);
		PlayerPrefabForNewGame = null;
		gameObject.SetActive(true);
		PlayerController component = gameObject.GetComponent<PlayerController>();
		component.ActorName = "Player ID 0";
		component.PlayerIDX = 0;
		if (loadedSave.playerOneData.CostumeID == 1)
		{
			component.SwapToAlternateCostume();
		}
		CurrentGameType = loadedSave.savedGameType;
		if (loadedSave != null && loadedSave.playerOneData != null)
		{
			if (loadedSave.playerOneData.passiveItems != null)
			{
				for (int i = 0; i < loadedSave.playerOneData.passiveItems.Count; i++)
				{
					if (loadedSave.playerOneData.passiveItems[i].PickupID == GlobalItemIds.SevenLeafClover)
					{
						PassiveItem.IncrementFlag(component, typeof(SevenLeafCloverItem));
					}
				}
			}
			component.MasteryTokensCollectedThisRun = loadedSave.playerOneData.MasteryTokensCollected;
		}
		RefreshAllPlayers();
		if (CurrentGameType != GameType.COOP_2_PLAYER)
		{
			return;
		}
		GameObject original = ResourceCache.Acquire("PlayerCoopCultist") as GameObject;
		GameObject gameObject2 = UnityEngine.Object.Instantiate(original, Vector3.zero, Quaternion.identity);
		CoopPlayerPrefabForNewGame = null;
		gameObject2.SetActive(true);
		PlayerController component2 = gameObject2.GetComponent<PlayerController>();
		component2.ActorName = "Player ID 1";
		component2.PlayerIDX = 1;
		if (loadedSave.playerTwoData.CostumeID == 1)
		{
			component2.SwapToAlternateCostume();
		}
		if (loadedSave != null && loadedSave.playerTwoData != null)
		{
			if (loadedSave.playerTwoData.passiveItems != null)
			{
				for (int j = 0; j < loadedSave.playerTwoData.passiveItems.Count; j++)
				{
					if (loadedSave.playerTwoData.passiveItems[j].PickupID == GlobalItemIds.SevenLeafClover)
					{
						PassiveItem.IncrementFlag(component2, typeof(SevenLeafCloverItem));
					}
				}
			}
			component2.MasteryTokensCollectedThisRun = loadedSave.playerTwoData.MasteryTokensCollected;
		}
		RefreshAllPlayers();
	}

	public void DelayedLoadCharacterSelect(float delay, bool unloadGameData = false, bool doMainMenu = false)
	{
		StartCoroutine(DelayedLoadCharacterSelect_CR(delay, unloadGameData, doMainMenu));
	}

	private IEnumerator DelayedLoadCharacterSelect_CR(float delay, bool unloadGameData, bool doMainMenu)
	{
		if (!m_loadingLevel)
		{
			m_loadingLevel = true;
			float elapsed = 0f;
			while (elapsed < delay)
			{
				elapsed += INVARIANT_DELTA_TIME;
				yield return null;
			}
			if (GameUIRoot.Instance != null)
			{
				GameUIRoot.Instance.ToggleUICamera(false);
				yield return null;
			}
			LoadCharacterSelect(unloadGameData, doMainMenu);
		}
	}

	public void ClearPrimaryPlayer()
	{
		if (m_player != null)
		{
			UnityEngine.Object.Destroy(m_player.gameObject);
		}
		m_player = null;
	}

	public void ClearSecondaryPlayer()
	{
		if (m_secondaryPlayer != null)
		{
			UnityEngine.Object.Destroy(m_secondaryPlayer.gameObject);
		}
		m_secondaryPlayer = null;
		m_players = null;
	}

	public void ClearPlayers()
	{
		if (m_players != null)
		{
			for (int i = 0; i < m_players.Length; i++)
			{
				PlayerController playerController = m_players[i];
				if ((bool)playerController)
				{
					playerController.specRigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Unknown;
					UnityEngine.Object.Destroy(playerController.gameObject);
				}
			}
			m_players = null;
			m_player = null;
			m_secondaryPlayer = null;
		}
		else
		{
			if (m_player != null)
			{
				m_player.specRigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Unknown;
				UnityEngine.Object.Destroy(m_player.gameObject);
			}
			m_player = null;
			m_secondaryPlayer = null;
		}
	}

	public void LoadCharacterSelect(bool unloadGameData = false, bool doMainMenu = false)
	{
		if (m_paused)
		{
			ForceUnpause();
		}
		m_loadingLevel = true;
		IsReturningToBreach = true;
		ClearPerLevelData();
		FlushAudio();
		ClearActiveGameData(false, true);
		m_preventUnpausing = false;
		Foyer.DoIntroSequence = false;
		Foyer.DoMainMenu = doMainMenu;
		IsReturningToBreach = true;
		SKIP_FOYER = false;
		InjectedLevelName = string.Empty;
		SetNextLevelIndex(0);
		m_preparingToDestroyThisGameManagerOnCollision = true;
		LoadNextLevel();
	}

	public void DelayedLoadBossrushFloor(float delay)
	{
		int bossrushTargetFloor = nextLevelIndex;
		nextLevelIndex++;
		StartCoroutine(DelayedLoadBossrushFloor_CR(delay, bossrushTargetFloor));
	}

	public void DebugLoadBossrushFloor(int target)
	{
		StartCoroutine(DelayedLoadBossrushFloor_CR(0.5f, target));
	}

	private IEnumerator DelayedLoadBossrushFloor_CR(float delay, int bossrushTargetFloor)
	{
		if (!m_loadingLevel)
		{
			m_loadingLevel = true;
			yield return new WaitForSeconds(delay);
			InstantLoadBossRushFloor(bossrushTargetFloor);
		}
	}

	private void InstantLoadBossRushFloor(int bossrushTargetFloor)
	{
		m_loadingLevel = true;
		if (CurrentGameMode == GameMode.SUPERBOSSRUSH)
		{
			switch (bossrushTargetFloor)
			{
			case 1:
				LoadCustomFlowForDebug("Bossrush_01_Castle", "Base_Castle", "tt_castle");
				break;
			case 2:
				LoadCustomFlowForDebug("Bossrush_01a_Sewer", "Base_Sewer", "tt_sewer");
				break;
			case 3:
				LoadCustomFlowForDebug("Bossrush_02_Gungeon", "Base_Gungeon", "tt5");
				break;
			case 4:
				LoadCustomFlowForDebug("Bossrush_02a_Cathedral", "Base_Cathedral", "tt_cathedral");
				break;
			case 5:
				LoadCustomFlowForDebug("Bossrush_03_Mines", "Base_Mines", "tt_mines");
				break;
			case 6:
				LoadCustomFlowForDebug("Bossrush_04_Catacombs", "Base_Catacombs", "tt_catacombs");
				break;
			case 7:
				LoadCustomFlowForDebug("Bossrush_05_Forge", "Base_Forge", "tt_forge");
				break;
			case 8:
				LoadCustomFlowForDebug("Bossrush_06_BulletHell", "Base_BulletHell", "tt_bullethell");
				break;
			default:
				LoadMainMenu();
				break;
			}
		}
		else
		{
			switch (bossrushTargetFloor)
			{
			case 1:
				LoadCustomFlowForDebug("Bossrush_01_Castle", "Base_Castle", "tt_castle");
				break;
			case 2:
				LoadCustomFlowForDebug("Bossrush_02_Gungeon", "Base_Gungeon", "tt5");
				break;
			case 3:
				LoadCustomFlowForDebug("Bossrush_03_Mines", "Base_Mines", "tt_mines");
				break;
			case 4:
				LoadCustomFlowForDebug("Bossrush_04_Catacombs", "Base_Catacombs", "tt_catacombs");
				break;
			case 5:
				LoadCustomFlowForDebug("Bossrush_05_Forge", "Base_Forge", "tt_forge");
				break;
			default:
				LoadMainMenu();
				break;
			}
		}
	}

	public void DelayedLoadCustomLevel(float delay, string customLevel)
	{
		StartCoroutine(DelayedLoadCustomLevel_CR(delay, customLevel));
	}

	private IEnumerator DelayedLoadCustomLevel_CR(float delay, string customLevel)
	{
		if (!m_loadingLevel)
		{
			m_loadingLevel = true;
			yield return new WaitForSeconds(delay);
			LoadCustomLevel(customLevel);
		}
	}

	public void DelayedLoadNextLevel(float delay)
	{
		StartCoroutine(DelayedLoadNextLevel_CR(delay));
	}

	private IEnumerator DelayedLoadNextLevel_CR(float delay)
	{
		if (!m_loadingLevel)
		{
			m_loadingLevel = true;
			yield return new WaitForSeconds(delay);
			if (GameUIRoot.Instance != null)
			{
				GameUIRoot.Instance.ToggleUICamera(false);
			}
			yield return null;
			LoadNextLevel();
		}
	}

	private IEnumerator LoadLevelByIndex(int nextIndex)
	{
		SceneManager.LoadScene(dungeonFloors[nextLevelIndex].dungeonSceneName);
		yield return null;
		IsLoadingLevel = false;
		nextLevelIndex = nextIndex;
	}

	public void LoadNextLevel()
	{
		if (!string.IsNullOrEmpty(InjectedLevelName))
		{
			LoadCustomLevel(InjectedLevelName);
			InjectedLevelName = string.Empty;
			return;
		}
		if (SKIP_FOYER && nextLevelIndex == 0)
		{
			nextLevelIndex = 1;
		}
		if (dungeonFloors == null || dungeonFloors.Count == 0)
		{
			dungeonFloors = new List<GameLevelDefinition>();
			GameLevelDefinition gameLevelDefinition = new GameLevelDefinition();
			gameLevelDefinition.dungeonSceneName = SceneManager.GetActiveScene().name;
			dungeonFloors.Add(gameLevelDefinition);
		}
		if (nextLevelIndex >= dungeonFloors.Count)
		{
			nextLevelIndex = 0;
		}
		m_loadingLevel = true;
		ClearPerLevelData();
		if (dungeonFloors[nextLevelIndex].dungeonPrefabPath == string.Empty)
		{
			if (dungeonFloors[nextLevelIndex].dungeonSceneName == "MainMenu")
			{
				LoadMainMenu();
				nextLevelIndex = 0;
			}
			else if (dungeonFloors[nextLevelIndex].dungeonSceneName == "Foyer")
			{
				StartCoroutine(LoadLevelByIndex(nextLevelIndex + 1));
			}
			else
			{
				StartCoroutine(LoadLevelByIndex(0));
			}
		}
		else
		{
			StartCoroutine(LoadNextLevelAsync_CR(dungeonFloors[nextLevelIndex]));
			nextLevelIndex++;
		}
	}

	public void DoGameOver(string gameOverSource = "")
	{
		PauseRaw(true);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		AmmonomiconController.Instance.OpenAmmonomicon(true, false);
	}

	private IEnumerator LoadGameOver_CR(string gameOverSource)
	{
		BraveTime.ClearAllMultipliers();
		SceneManager.LoadScene("GameOver");
		while (BraveUtility.isLoadingLevel)
		{
			yield return null;
		}
		GameObject gameOverTextObject = GameObject.Find("GameOverTextLabel");
		dfLabel gameOverTextLabel = gameOverTextObject.GetComponent<dfLabel>();
		if (string.IsNullOrEmpty(gameOverSource))
		{
			gameOverTextLabel.Text = "You died.";
		}
		else
		{
			gameOverTextLabel.Text = "You were killed by: " + gameOverSource + " ";
		}
		gameOverTextLabel.Invalidate();
		IsLoadingLevel = false;
	}

	public GameLevelDefinition GetLevelDefinitionFromName(string levelName)
	{
		for (int i = 0; i < dungeonFloors.Count; i++)
		{
			if (dungeonFloors[i].dungeonSceneName == levelName)
			{
				return dungeonFloors[i];
			}
		}
		for (int j = 0; j < customFloors.Count; j++)
		{
			if (customFloors[j].dungeonSceneName == levelName)
			{
				return customFloors[j];
			}
		}
		return null;
	}

	public GameLevelDefinition GetLastLoadedLevelDefinition()
	{
		if (m_lastLoadedLevelDefinition == null)
		{
			return GetLevelDefinitionFromName(SceneManager.GetActiveScene().name);
		}
		return m_lastLoadedLevelDefinition;
	}

	private IEnumerator EndLoadNextLevelAsync_CR(AsyncOperation async, GameObject loadingSceneHierarchy, bool isHandoff = false)
	{
		IsLoadingLevel = true;
		while (!async.isDone || !async.allowSceneActivation)
		{
			yield return null;
		}
		for (int i = 0; i < BraveLevelLoadedListeners.Length; i++)
		{
			Type type = BraveLevelLoadedListeners[i];
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(type);
			for (int j = 0; j < array.Length; j++)
			{
				ILevelLoadedListener levelLoadedListener = array[j] as ILevelLoadedListener;
				if (levelLoadedListener != null)
				{
					levelLoadedListener.BraveOnLevelWasLoaded();
				}
			}
		}
		yield return null;
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (m_player != null)
		{
			m_player.BraveOnLevelWasLoaded();
		}
		if (m_secondaryPlayer != null)
		{
			m_secondaryPlayer.BraveOnLevelWasLoaded();
		}
		AmmonomiconController.EnsureExistence();
		yield return null;
		Image fadeImage2 = null;
		float temporo = 0.15f;
		GameObject canvasObj = loadingSceneHierarchy.transform.Find("FadeCanvas").gameObject;
		fadeImage2 = canvasObj.GetComponentInChildren<Image>();
		canvasObj.transform.SetParent(null);
		float elapsed = 0f;
		while (elapsed < temporo)
		{
			elapsed += INVARIANT_DELTA_TIME;
			float t = elapsed / temporo;
			if (fadeImage2 != null && (bool)fadeImage2)
			{
				fadeImage2.color = new Color(0f, 0f, 0f, t);
			}
			yield return null;
		}
		FlushAudio();
		UnityEngine.Object.Destroy(loadingSceneHierarchy);
		BraveTime.ClearAllMultipliers();
		yield return null;
		IsLoadingLevel = false;
		if (this.OnNewLevelFullyLoaded != null)
		{
			this.OnNewLevelFullyLoaded();
		}
		float defadeElapsed = 0f;
		while (defadeElapsed < temporo)
		{
			defadeElapsed += INVARIANT_DELTA_TIME;
			float t2 = defadeElapsed / temporo;
			fadeImage2.color = new Color(0f, 0f, 0f, 1f - t2);
			yield return null;
		}
		UnityEngine.Object.Destroy(fadeImage2.transform.parent.gameObject);
	}

	private IEnumerator LoadNextLevelAsync_CR(GameLevelDefinition gld)
	{
		SceneManager.LoadScene("LoadingDungeon");
		if (Time.timeScale != 0f)
		{
			BraveTime.ClearAllMultipliers();
		}
		if (m_preventUnpausing)
		{
			m_preventUnpausing = false;
		}
		if (m_paused)
		{
			m_paused = false;
		}
		while (BraveUtility.isLoadingLevel)
		{
			yield return null;
		}
		yield return null;
		AsyncOperation async;
		if (gld.dungeonSceneName == "tt_foyer")
		{
			AssetBundle assetBundle = ResourceManager.LoadAssetBundle("foyer_001");
			async = ResourceManager.LoadSceneAsyncFromBundle(assetBundle, LoadSceneMode.Additive);
		}
		else
		{
			AssetBundle assetBundle2 = ResourceManager.LoadAssetBundle("dungeon_scene_001");
			async = ResourceManager.LoadSceneAsyncFromBundle(assetBundle2, LoadSceneMode.Additive);
		}
		async.allowSceneActivation = false;
		if (gld.dungeonSceneName == "tt_foyer")
		{
			IsFoyer = true;
		}
		m_lastLoadedLevelDefinition = gld;
		BackgroundGenerationActive = false;
		gld.lastSelectedFlowEntry = null;
		if (!string.IsNullOrEmpty(gld.dungeonPrefabPath))
		{
			CurrentlyGeneratingDungeonPrefab = DungeonDatabase.GetOrLoadByName(gld.dungeonPrefabPath);
		}
		if (CurrentlyGeneratingDungeonPrefab != null)
		{
			int dungeonSeed = CurrentlyGeneratingDungeonPrefab.GetDungeonSeed();
			UnityEngine.Random.InitState(dungeonSeed);
			BraveRandom.InitializeWithSeed(dungeonSeed);
			DungeonFlowLevelEntry flowEntry = null;
			DungeonFlow targetFlow = null;
			if (!string.IsNullOrEmpty(InjectedFlowPath))
			{
				targetFlow = FlowDatabase.GetOrLoadByName(InjectedFlowPath);
				InjectedFlowPath = null;
			}
			if (gld.flowEntries.Count > 0)
			{
				flowEntry = gld.LovinglySelectDungeonFlow();
				if (flowEntry != null)
				{
					DungeonFlow orLoadByName = FlowDatabase.GetOrLoadByName(flowEntry.flowPath);
					if (orLoadByName == null)
					{
						orLoadByName = FlowDatabase.GetOrLoadByName("Boss Rooms/" + flowEntry.flowPath);
					}
					if (orLoadByName == null)
					{
						orLoadByName = FlowDatabase.GetOrLoadByName("Boss Rush Flows/" + flowEntry.flowPath);
					}
					if (orLoadByName == null)
					{
						orLoadByName = FlowDatabase.GetOrLoadByName("Testing/" + flowEntry.flowPath);
					}
					if (targetFlow == null)
					{
						targetFlow = orLoadByName;
					}
				}
			}
			LoopDungeonGenerator ldg = new LoopDungeonGenerator(CurrentlyGeneratingDungeonPrefab, dungeonSeed);
			if (targetFlow != null)
			{
				ldg.AssignFlow(targetFlow);
			}
			gld.lastSelectedFlowEntry = flowEntry;
			IEnumerator tracker = ldg.GenerateDungeonLayoutDeferred().GetEnumerator();
			while (tracker.MoveNext())
			{
				yield return null;
			}
			AkSoundEngine.PostEvent("Stop_Foyer_Fade_01", Instance.gameObject);
			PregeneratedDungeonData = ldg.DeferredGeneratedData;
			DungeonToAutoLoad = CurrentlyGeneratingDungeonPrefab;
			CurrentlyGeneratingDungeonPrefab = null;
		}
		else
		{
			for (int i = 0; i < AllPlayers.Length; i++)
			{
				if ((bool)AllPlayers[i])
				{
					UnityEngine.Object.Destroy(AllPlayers[i].gameObject);
				}
			}
		}
		GameObject loadingSceneHierarchy = GameObject.Find("LoadingMonster");
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		if (m_preparingToDestroyThisGameManagerOnCollision)
		{
			m_shouldDestroyThisGameManagerOnCollision = true;
			m_preDestroyAsyncHolder = async;
			m_preDestroyLoadingHierarchyHolder = loadingSceneHierarchy;
		}
		async.allowSceneActivation = true;
		if (!m_shouldDestroyThisGameManagerOnCollision)
		{
			yield return StartCoroutine(EndLoadNextLevelAsync_CR(async, loadingSceneHierarchy));
		}
	}

	public void Pause()
	{
		Minimap.Instance.ToggleMinimap(false);
		GameUIRoot.Instance.ShowPauseMenu();
		BraveMemory.HandleGamePaused();
		GameStatsManager.Instance.MoveSessionStatsToSavedSessionStats();
		PauseRaw();
		if (Options.CurrentFullscreenStyle == GameOptions.FullscreenStyle.BORDERLESS)
		{
			GameCursorController component = GameUIRoot.Instance.GetComponent<GameCursorController>();
			if (component != null)
			{
				component.ToggleClip(false);
			}
		}
		StartCoroutine(PixelateCR());
	}

	public void PauseRaw(bool preventUnpausing = false)
	{
		GameUIRoot.Instance.levelNameUI.BanishLevelNameText();
		GameUIRoot.Instance.ForceClearReload();
		GameUIRoot.Instance.ToggleLowerPanels(false, false, "gm_pause");
		GameUIRoot.Instance.HideCoreUI("gm_pause");
		GameUIRoot.Instance.ToggleAllDefaultLabels(false, "pause");
		BraveTime.RegisterTimeScaleMultiplier(0f, base.gameObject);
		if (!IsSelectingCharacter)
		{
			if (!MainCameraController.ManualControl)
			{
				MainCameraController.OverridePosition = MainCameraController.transform.position;
				MainCameraController.SetManualControl(true, false);
				m_pauseLockedCamera = true;
			}
			else
			{
				m_pauseLockedCamera = false;
			}
		}
		if (preventUnpausing)
		{
			m_preventUnpausing = true;
		}
		m_paused = true;
	}

	public void ReturnToBasePauseState()
	{
		GameUIRoot.Instance.ReturnToBasePause();
	}

	public void Unpause()
	{
		m_paused = false;
		m_unpausedThisFrame = true;
		if (m_pauseLockedCamera)
		{
			MainCameraController.SetManualControl(false);
		}
		GameUIRoot.Instance.ToggleLowerPanels(true, false, "gm_pause");
		GameUIRoot.Instance.ShowCoreUI("gm_pause");
		GameUIRoot.Instance.HidePauseMenu();
		GameUIRoot.Instance.ToggleAllDefaultLabels(true, "pause");
		BraveInput.FlushAll();
		if (AllPlayers != null)
		{
			for (int i = 0; i < AllPlayers.Length; i++)
			{
				if ((bool)AllPlayers[i])
				{
					AllPlayers[i].WasPausedThisFrame = true;
				}
			}
		}
		if (Options.CurrentFullscreenStyle == GameOptions.FullscreenStyle.BORDERLESS)
		{
			GameCursorController component = GameUIRoot.Instance.GetComponent<GameCursorController>();
			if (component != null)
			{
				component.ToggleClip(true);
			}
		}
		if (Pixelator.Instance.saturation != 1f)
		{
			StartCoroutine(DepixelateCR());
		}
	}

	public void ForceUnpause()
	{
		m_paused = false;
		if ((bool)MainCameraController && m_pauseLockedCamera)
		{
			MainCameraController.SetManualControl(false);
		}
		if (GameUIRoot.Instance != null)
		{
			GameUIRoot.Instance.ToggleLowerPanels(true, false, "gm_pause");
			GameUIRoot.Instance.ShowCoreUI("gm_pause");
			GameUIRoot.Instance.HidePauseMenu();
		}
		BraveInput.FlushAll();
		if (Pixelator.Instance != null)
		{
			Options.OverrideMotionEnhancementModeForPause = false;
			Pixelator.Instance.OnChangedMotionEnhancementMode(Options.MotionEnhancementMode);
			Pixelator.Instance.overrideTileScale = 1;
			Pixelator.Instance.saturation = 1f;
		}
		BraveTime.ClearMultiplier(base.gameObject);
	}

	private IEnumerator PixelateCR()
	{
		float elapsed = 0f;
		float duration = 0.15f;
		Options.OverrideMotionEnhancementModeForPause = true;
		Pixelator.Instance.OnChangedMotionEnhancementMode(Options.MotionEnhancementMode);
		while (elapsed < duration && m_paused)
		{
			elapsed += m_deltaTime;
			Pixelator.Instance.saturation = 1f - Mathf.Clamp01(elapsed / duration);
			Pixelator.Instance.SetFadeColor(Color.black);
			Pixelator.Instance.fade = 1f - 1f * Mathf.Clamp01(elapsed / duration);
			yield return null;
		}
	}

	private IEnumerator DepixelateCR()
	{
		float elapsed2 = 0f;
		float duration = 0.075f;
		while (elapsed2 < 0.05f)
		{
			elapsed2 += m_deltaTime;
			yield return null;
		}
		elapsed2 = 0f;
		while (elapsed2 < duration && !m_paused)
		{
			elapsed2 += m_deltaTime;
			Pixelator.Instance.saturation = Mathf.Clamp01(elapsed2 / duration);
			Pixelator.Instance.fade = 1f * Mathf.Clamp01(elapsed2 / duration);
			yield return null;
		}
		Options.OverrideMotionEnhancementModeForPause = false;
		Pixelator.Instance.OnChangedMotionEnhancementMode(Options.MotionEnhancementMode);
		Pixelator.Instance.overrideTileScale = 1;
		Pixelator.Instance.saturation = 1f;
		Pixelator.Instance.fade = 1f;
		BraveTime.ClearMultiplier(base.gameObject);
	}

	public static void AttemptSoundEngineInitialization()
	{
		if (!AUDIO_ENABLED)
		{
			DebugTime.RecordStartTime();
			uint out_bankID;
			AkSoundEngine.LoadBank("SFX.bnk", -1, out out_bankID);
			DebugTime.Log("GameManager.ASEI.LoadBank(SFX)");
			Debug.LogError("loaded bank id: " + out_bankID);
			AkChannelConfig akChannelConfig = new AkChannelConfig();
			akChannelConfig.SetStandard(uint.MaxValue);
			AkSoundEngine.SetListenerSpatialization(null, true, akChannelConfig);
			AUDIO_ENABLED = true;
		}
	}

	public static void AttemptSoundEngineInitializationAsync()
	{
		c_asyncSoundStartTime = Time.realtimeSinceStartup;
		c_asyncSoundStartFrame = Time.frameCount;
		uint out_bankID;
		AkSoundEngine.LoadBank("SFX.bnk", BankCallback, null, -1, out out_bankID);
		Debug.LogError("async loading bank id: " + out_bankID);
	}

	private static void BankCallback(uint in_bankID, IntPtr in_InMemoryBankPtr, AKRESULT in_eLoadResult, uint in_memPoolId, object in_Cookie)
	{
		DebugTime.Log(c_asyncSoundStartTime, c_asyncSoundStartFrame, "GameManager.ASEI.LoadBank(SFX)");
		AkChannelConfig akChannelConfig = new AkChannelConfig();
		akChannelConfig.SetStandard(uint.MaxValue);
		AkSoundEngine.SetListenerSpatialization(null, true, akChannelConfig);
		AUDIO_ENABLED = true;
	}

	protected void LoadDungeonFloorsFromTargetPrefab(string resourcePath, bool assignNextLevelIndex)
	{
		GameManager component = (BraveResources.Load(resourcePath) as GameObject).GetComponent<GameManager>();
		GlobalInjectionData = component.GlobalInjectionData;
		CurrentRewardManager = component.CurrentRewardManager;
		OriginalRewardManager = component.OriginalRewardManager;
		SynergyManager = component.SynergyManager;
		DefaultAlienConversationFont = component.DefaultAlienConversationFont;
		DefaultNormalConversationFont = component.DefaultNormalConversationFont;
		dungeonFloors = component.dungeonFloors;
		customFloors = component.customFloors;
		if (assignNextLevelIndex)
		{
			for (int i = 0; i < dungeonFloors.Count; i++)
			{
				if (SceneManager.GetActiveScene().name == dungeonFloors[i].dungeonSceneName)
				{
					nextLevelIndex = i + 1;
					break;
				}
			}
		}
		COOP_ENEMY_HEALTH_MULTIPLIER = component.COOP_ENEMY_HEALTH_MULTIPLIER;
		COOP_ENEMY_PROJECTILE_SPEED_MULTIPLIER = component.COOP_ENEMY_PROJECTILE_SPEED_MULTIPLIER;
		PierceDamageScaling = component.PierceDamageScaling;
		BloodthirstOptions = component.BloodthirstOptions;
		EnemyReplacementTiers = component.EnemyReplacementTiers;
	}

	public void InitializeForRunWithSeed(int seed)
	{
		CurrentRunSeed = seed;
	}

	private void Awake()
	{
		DebugTime.Log("GameManager.Awake()");
		base.gameObject.AddComponent<EarlyUpdater>();
		if (!Application.isEditor && !m_hasEnsuredHeapSize && SystemInfo.systemMemorySize > 1000)
		{
			if (SystemInfo.systemMemorySize > 3500)
			{
				BraveMemory.EnsureHeapSize(204800);
				m_hasEnsuredHeapSize = true;
			}
			else
			{
				BraveMemory.EnsureHeapSize(102400);
				m_hasEnsuredHeapSize = true;
			}
		}
		try
		{
			Debug.Log("Version: " + VersionManager.UniqueVersionNumber);
			Debug.LogFormat("Now: {0:MM.dd.yyyy HH:mm}", DateTime.Now);
		}
		catch (Exception)
		{
		}
		if (platformInterface == null)
		{
			if (PlatformInterfaceSteam.IsSteamBuild())
			{
				platformInterface = new PlatformInterfaceSteam();
			}
			else if (PlatformInterfaceGalaxy.IsGalaxyBuild())
			{
				platformInterface = new PlatformInterfaceGalaxy();
			}
			else
			{
				platformInterface = new PlatformInterfaceGenericPC();
			}
		}
		platformInterface.Start();
		if (Options == null)
		{
			GameOptions.Load();
		}
		string path = "_GameManager";
		DebugTime.RecordStartTime();
		if (dungeonFloors == null)
		{
			dungeonFloors = new List<GameLevelDefinition>();
			GameManager component = (BraveResources.Load(path) as GameObject).GetComponent<GameManager>();
			GlobalInjectionData = component.GlobalInjectionData;
			CurrentRewardManager = component.RewardManager;
			SynergyManager = component.SynergyManager;
			DefaultAlienConversationFont = component.DefaultAlienConversationFont;
			DefaultNormalConversationFont = component.DefaultNormalConversationFont;
		}
		DebugTime.Log("GameManager.Awake() load dungeon floors");
		GameManager[] array = UnityEngine.Object.FindObjectsOfType<GameManager>();
		if (array.Length > 1)
		{
			GameManager gameManager = null;
			GameManager gameManager2 = null;
			for (int i = 0; i < array.Length; i++)
			{
				if ((bool)array[i] && (gameManager == null || array[i].dungeonFloors.Count > gameManager.dungeonFloors.Count) && !array[i].m_shouldDestroyThisGameManagerOnCollision)
				{
					gameManager = array[i];
				}
				if (array[i].m_shouldDestroyThisGameManagerOnCollision)
				{
					gameManager2 = array[i];
				}
			}
			if (gameManager != null && gameManager2 != null)
			{
				Debug.Log("continuing from where my father left off");
				if (!IsReturningToFoyerWithPlayer)
				{
					IsSelectingCharacter = true;
				}
				gameManager.StartCoroutine(gameManager.EndLoadNextLevelAsync_CR(gameManager2.m_preDestroyAsyncHolder, gameManager2.m_preDestroyLoadingHierarchyHolder, true));
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != gameManager)
				{
					UnityEngine.Object.Destroy(array[j].gameObject);
				}
			}
			mr_manager = gameManager;
			if (!this)
			{
				return;
			}
		}
		if (m_inputManager == null)
		{
			InControlManager inControlManager = UnityEngine.Object.FindObjectOfType<InControlManager>();
			if ((bool)inControlManager)
			{
				m_inputManager = inControlManager;
				UnityEngine.Object.DontDestroyOnLoad(m_inputManager.gameObject);
				InputManager.Enabled = true;
			}
			else
			{
				GameObject gameObject = new GameObject("_InputManager");
				m_inputManager = gameObject.AddComponent<InControlManager>();
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				InputManager.Enabled = true;
			}
		}
		if (m_dungeonMusicController == null)
		{
			m_dungeonMusicController = GetComponent<DungeonFloorMusicController>();
			if (!m_dungeonMusicController)
			{
				m_dungeonMusicController = base.gameObject.AddComponent<DungeonFloorMusicController>();
			}
		}
		DebugTime.RecordStartTime();
		GameStatsManager.Load();
		DebugTime.Log("GameManager.Awake() load game stats");
		if (!AUDIO_ENABLED)
		{
			AttemptSoundEngineInitialization();
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Debug.Log("Post GameManager.Awake.AudioInit");
		if (GameStatsManager.Instance.IsInSession)
		{
			StartEncounterableUnlockedChecks();
		}
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.MetroPlayerX64 || Application.platform == RuntimePlatform.MetroPlayerX86)
		{
			LoadResolutionFromOptions();
		}
		else if (Application.platform == RuntimePlatform.PS4)
		{
			LoadResolutionFromPS4();
		}
		Debug.Log("Post GameManager.Awake.Resolution");
		StringTableManager.LoadTablesIfNecessary();
		RandomIntForCurrentRun = UnityEngine.Random.Range(0, 1000);
		Debug.Log("Terminating GameManager Awake()");
	}

	private IEnumerator Start()
	{
		DebugTime.Log("GameManager.Start()");
		Options.MusicVolume = Options.MusicVolume;
		Options.SoundVolume = Options.SoundVolume;
		Options.UIVolume = Options.UIVolume;
		Options.AudioHardware = Options.AudioHardware;
		Gun.s_DualWieldFactor = DUAL_WIELDING_DAMAGE_FACTOR;
		yield return null;
		if (GameOptions.RequiresLanguageReinitialization)
		{
			Options.CurrentLanguage = platformInterface.GetPreferredLanguage();
			GameOptions.RequiresLanguageReinitialization = false;
		}
		if (!m_initializedDeviceCallbacks)
		{
			UnityInputDeviceManager.OnAllDevicesReattached = (Action)Delegate.Combine(UnityInputDeviceManager.OnAllDevicesReattached, new Action(HandleDeviceShift));
		}
		if (Options.CurrentLanguage != 0)
		{
			StringTableManager.CurrentLanguage = Options.CurrentLanguage;
		}
	}

	private void HandleDeviceShift()
	{
		m_initializedDeviceCallbacks = true;
		BraveInput.ReassignAllControllers();
	}

	private void OnEnable()
	{
		m_lastFrameRealtime = Time.realtimeSinceStartup;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Shader.SetGlobalFloat("_MeduziReflectionsEnabled", 0f);
		GameManager[] array = UnityEngine.Object.FindObjectsOfType<GameManager>();
		if (array.Length < 1 || (array.Length == 1 && array[0] == this))
		{
			if (GameStatsManager.Instance.IsInSession)
			{
				GameStatsManager.Instance.EndSession(true);
			}
			if (SaveManager.ResetSaveSlot)
			{
				GameStatsManager.DANGEROUS_ResetAllStats();
			}
			GameStatsManager.Save();
			if (Options != null)
			{
				GameOptions.Save();
			}
			Debug.LogWarning("SAVING DATA");
			Options = null;
			if (SaveManager.TargetSaveSlot.HasValue)
			{
				SaveManager.ChangeSlot(SaveManager.TargetSaveSlot.Value);
				SaveManager.TargetSaveSlot = null;
			}
		}
		UnityInputDeviceManager.OnAllDevicesReattached = (Action)Delegate.Remove(UnityInputDeviceManager.OnAllDevicesReattached, new Action(HandleDeviceShift));
	}

	protected void InvariantUpdate(float realDeltaTime)
	{
		if (!m_bgChecksActive && GameStatsManager.Instance.IsInSession)
		{
			StartEncounterableUnlockedChecks();
		}
		if (!(m_dungeon != null) || m_preventUnpausing || IsLoadingLevel || (Foyer.DoMainMenu && IsFoyer))
		{
			return;
		}
		int num = AllPlayers.Length;
		if (IsSelectingCharacter)
		{
			num = 1;
		}
		for (int i = 0; i < num; i++)
		{
			int num2 = ((!IsSelectingCharacter) ? AllPlayers[i].PlayerIDX : 0);
			BraveInput braveInput = ((!IsSelectingCharacter) ? BraveInput.GetInstanceForPlayer(num2) : BraveInput.PlayerlessInstance);
			if (braveInput == null || braveInput.ActiveActions == null)
			{
				continue;
			}
			bool flag = braveInput.ActiveActions.PauseAction.WasPressed;
			if (Minimap.HasInstance && Minimap.Instance.HeldOpen)
			{
				flag = false;
			}
			if (braveInput.ActiveActions.EquipmentMenuAction.WasPressed)
			{
				bool flag2 = IsSelectingCharacter && Foyer.IsCurrentlyPlayingCharacterSelect;
				if (!m_paused && !AmmonomiconController.Instance.IsOpen && !flag2)
				{
					LastPausingPlayerID = num2;
					Pause();
					GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().DoShowBestiary(null, null);
				}
				else if ((!m_paused || AmmonomiconController.Instance.IsOpen) && m_paused && AmmonomiconController.Instance.IsOpen && !AmmonomiconController.Instance.IsClosing && !AmmonomiconController.Instance.IsOpening)
				{
					AmmonomiconController.Instance.CloseAmmonomicon();
					ReturnToBasePauseState();
					dfGUIManager.PushModal(GameUIRoot.Instance.PauseMenuPanel);
					Unpause();
				}
			}
			else if (flag)
			{
				if (m_paused)
				{
					if (GameUIRoot.Instance.AreYouSurePanel.IsVisible || GameUIRoot.Instance.KeepMetasIsVisible)
					{
						continue;
					}
					if (GameUIRoot.Instance.HasOpenPauseSubmenu())
					{
						PauseMenuController component = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>();
						if (!component.OptionsMenu.ModalKeyBindingDialog.IsVisible)
						{
							if (component.OptionsMenu.IsVisible || component.OptionsMenu.ModalKeyBindingDialog.IsVisible)
							{
								component.OptionsMenu.CloseAndMaybeApplyChangesWithPrompt();
								continue;
							}
							component.ForceMaterialVisibility();
							ReturnToBasePauseState();
						}
					}
					else if (AmmonomiconController.Instance.IsOpen)
					{
						if (!AmmonomiconController.Instance.IsTurningPage && !AmmonomiconController.Instance.IsOpening && !AmmonomiconController.Instance.IsClosing)
						{
							AmmonomiconController.Instance.CloseAmmonomicon();
							ReturnToBasePauseState();
							dfGUIManager.PushModal(GameUIRoot.Instance.PauseMenuPanel);
						}
					}
					else
					{
						Unpause();
					}
				}
				else
				{
					LastPausingPlayerID = num2;
					Pause();
				}
			}
			else
			{
				if (!m_paused || !braveInput.ActiveActions.CancelAction.WasPressed || GameUIRoot.Instance.AreYouSurePanel.IsVisible || GameUIRoot.Instance.KeepMetasIsVisible)
				{
					continue;
				}
				if (AmmonomiconController.Instance.IsOpen && !AmmonomiconController.Instance.IsClosing && !AmmonomiconController.Instance.BookmarkHasFocus)
				{
					AmmonomiconController.Instance.ReturnFocusToBookmark();
				}
				else if (GameUIRoot.Instance.HasOpenPauseSubmenu())
				{
					PauseMenuController component2 = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>();
					if (!component2.OptionsMenu.ModalKeyBindingDialog.IsVisible)
					{
						if (component2.OptionsMenu.IsVisible || component2.OptionsMenu.ModalKeyBindingDialog.IsVisible)
						{
							component2.OptionsMenu.CloseAndMaybeApplyChangesWithPrompt();
						}
						else
						{
							ReturnToBasePauseState();
						}
					}
				}
				else if (AmmonomiconController.Instance.IsOpen && !AmmonomiconController.Instance.IsClosing)
				{
					if (!AmmonomiconController.Instance.IsTurningPage)
					{
						AmmonomiconController.Instance.CloseAmmonomicon();
						ReturnToBasePauseState();
						dfGUIManager.PushModal(GameUIRoot.Instance.PauseMenuPanel);
					}
				}
				else
				{
					Unpause();
				}
			}
		}
	}

	public void FlushMusicAudio()
	{
		AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
		if ((bool)m_dungeonMusicController)
		{
			m_dungeonMusicController.ClearCoreMusicEventID();
		}
	}

	public void FlushAudio()
	{
		AkSoundEngine.PostEvent("Stop_SND_All", base.gameObject);
		AkSoundEngine.ClearPreparedEvents();
		AkSoundEngine.StopAll();
		if ((bool)m_dungeonMusicController)
		{
			m_dungeonMusicController.ClearCoreMusicEventID();
		}
	}

	public void ClearPerLevelData()
	{
		BraveCameraUtility.OverrideAspect = null;
		SuperReaperController.PreventShooting = false;
		BossKillCam.BossDeathCamRunning = false;
		PickupObject.ItemIsBeingTakenByRat = false;
		LastUsedInputDeviceForConversation = null;
		BossManager.PriorFloorSelectedBossRoom = null;
		if (Instance.Dungeon != null)
		{
			for (int i = 0; i < Instance.Dungeon.data.rooms.Count; i++)
			{
				Instance.Dungeon.data.rooms[i].SetRoomActive(true);
			}
		}
		m_dungeon = null;
		m_camera = null;
		GameUIRoot.Instance = null;
		if (m_player != null)
		{
			m_player.ClearPerLevelData();
		}
		CheckEntireFloorVisited.IsQuestCallbackActive = false;
		SunglassesItem.SunglassesActive = false;
		AmmonomiconController.Instance = null;
		TileVFXManager.Instance = null;
		InTutorial = false;
		BossKillCam.ClearPerLevelData();
		LootEngine.ClearPerLevelData();
		RoomHandler.unassignedInteractableObjects.Clear();
		ShadowSystem.ClearPerLevelData();
		SecretRoomUtility.ClearPerLevelData();
		DeadlyDeadlyGoopManager.ClearPerLevelData();
		BroController.ClearPerLevelData();
		GlobalSparksDoer.CleanupOnSceneTransition();
		SilencerInstance.s_MaxRadiusLimiter = null;
		TextBoxManager.ClearPerLevelData();
		SpawnManager.LastPrefabPool = null;
		TimeTubeCreditsController.ClearPerLevelData();
		PVP_ENABLED = false;
		if (TK2DTilemapChunkAnimator.PositionToAnimatorMap != null)
		{
			TK2DTilemapChunkAnimator.PositionToAnimatorMap.Clear();
		}
		SecretRoomDoorBeer.AllSecretRoomDoors = null;
		DebrisObject.ClearPerLevelData();
		ExplosionManager.ClearPerLevelData();
		StaticReferenceManager.ClearStaticPerLevelData();
		CollisionData.Pool.Clear();
		LinearCastResult.Pool.Clear();
		RaycastResult.Pool.Clear();
		SpriteChunk.ClearPerLevelData();
		AIActor.ClearPerLevelData();
		TalkDoerLite.ClearPerLevelData();
		Pathfinder.ClearPerLevelData();
		TakeCoverBehavior.ClearPerLevelData();
		if (Pixelator.Instance != null)
		{
			Pixelator.Instance.ClearCachedFrame();
		}
		ExtantShopTrackableGuids.Clear();
		EnemyDatabase.Instance.DropReferences();
		EncounterDatabase.Instance.DropReferences();
		GameStatsManager.Instance.MoveSessionStatsToSavedSessionStats();
		GameStatsManager.Save();
	}

	public void ClearActiveGameData(bool destroyGameManager, bool endSession)
	{
		GameStatsManager.Instance.CurrentEeveeEquipSeed = -1;
		PickupObject.RatBeatenAtPunchout = false;
		BraveCameraUtility.OverrideAspect = null;
		SuperReaperController.PreventShooting = false;
		BossKillCam.BossDeathCamRunning = false;
		ClearPlayers();
		IsCoopPast = false;
		IsGunslingerPast = false;
		Exploder.OnExplosionTriggered = null;
		MetaInjectionData.ClearBlueprint();
		RewardManifest.ClearManifest(RewardManager);
		RewardManager.AdditionalHeartTierMagnificence = 0f;
		BossManager.HasOverriddenCoreBoss = false;
		RoomHandler.HasGivenRoomChestRewardThisRun = false;
		if (PassiveItem.ActiveFlagItems != null)
		{
			PassiveItem.ActiveFlagItems.Clear();
		}
		PVP_ENABLED = false;
		Gun.ActiveReloadActivated = false;
		Gun.ActiveReloadActivatedPlayerTwo = false;
		SecretHandshakeItem.NumActive = 0;
		BossKillCam.ClearPerLevelData();
		BaseShopController.HasLockedShopProcedurally = false;
		Chest.HasDroppedResourcefulRatNoteThisSession = false;
		Chest.DoneResourcefulRatMimicThisSession = false;
		Chest.HasDroppedSerJunkanThisSession = false;
		Chest.ToggleCoopChests(false);
		PlayerItem.AllowDamageCooldownOnActive = false;
		AIActor.HealthModifier = 1f;
		Projectile.BaseEnemyBulletSpeedMultiplier = 1f;
		Projectile.ResetGlobalProjectileDepth();
		BasicBeamController.ResetGlobalBeamHeight();
		if ((bool)MainCameraController)
		{
			MainCameraController.enabled = false;
		}
		m_lastLoadedLevelDefinition = null;
		Cursor.visible = true;
		nextLevelIndex = 0;
		StaticReferenceManager.ForceClearAllStaticMemory();
		if (endSession)
		{
			GameStatsManager.Instance.EndSession(true);
		}
		if (destroyGameManager)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public static void BroadcastRoomFsmEvent(string eventName)
	{
		List<PlayMakerFSM> componentsAbsoluteInRoom = Instance.BestActivePlayer.CurrentRoom.GetComponentsAbsoluteInRoom<PlayMakerFSM>();
		for (int i = 0; i < componentsAbsoluteInRoom.Count; i++)
		{
			componentsAbsoluteInRoom[i].SendEvent(eventName);
		}
	}

	public static void BroadcastRoomFsmEvent(string eventName, RoomHandler targetRoom)
	{
		List<PlayMakerFSM> componentsAbsoluteInRoom = targetRoom.GetComponentsAbsoluteInRoom<PlayMakerFSM>();
		for (int i = 0; i < componentsAbsoluteInRoom.Count; i++)
		{
			componentsAbsoluteInRoom[i].SendEvent(eventName);
		}
	}

	public static void BroadcastRoomTalkDoerFsmEvent(string eventName)
	{
		for (int i = 0; i < StaticReferenceManager.AllNpcs.Count; i++)
		{
			TalkDoerLite talkDoerLite = StaticReferenceManager.AllNpcs[i];
			if ((bool)talkDoerLite && (bool)Instance.BestActivePlayer && Instance.BestActivePlayer.CurrentRoom == talkDoerLite.ParentRoom)
			{
				talkDoerLite.SendPlaymakerEvent(eventName);
			}
		}
	}

	public void StartEncounterableUnlockedChecks()
	{
		m_bgChecksActive = true;
		ConstructSetOfKnownUnlocks();
		StartCoroutine(EncounterableUnlockedChecks());
	}

	private void ConstructSetOfKnownUnlocks()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < EncounterDatabase.Instance.Entries.Count; i++)
		{
			EncounterDatabaseEntry encounterDatabaseEntry = EncounterDatabase.Instance.Entries[i];
			if (encounterDatabaseEntry == null || encounterDatabaseEntry.journalData.SuppressInAmmonomicon || EncounterDatabase.IsProxy(encounterDatabaseEntry.myGuid))
			{
				continue;
			}
			num++;
			if (encounterDatabaseEntry.PrerequisitesMet())
			{
				num2++;
				if (encounterDatabaseEntry.prerequisites == null || encounterDatabaseEntry.prerequisites.Length == 0 || GameStatsManager.Instance.QueryEncounterableAnnouncement(encounterDatabaseEntry.myGuid))
				{
					m_knownEncounterables.Add(encounterDatabaseEntry.myGuid);
				}
				else
				{
					m_queuedUnlocks.Add(encounterDatabaseEntry.myGuid);
				}
			}
			else if (encounterDatabaseEntry.prerequisites != null && encounterDatabaseEntry.prerequisites.Length > 0)
			{
				PickupObject byId = PickupObjectDatabase.GetById(encounterDatabaseEntry.pickupObjectId);
				if (byId == null || byId.quality == PickupObject.ItemQuality.EXCLUDED)
				{
					num2++;
				}
				else if (encounterDatabaseEntry.prerequisites.Length == 1 && encounterDatabaseEntry.prerequisites[0].requireFlag && encounterDatabaseEntry.prerequisites[0].saveFlagToCheck == GungeonFlags.COOP_PAST_REACHED)
				{
					num2++;
				}
			}
		}
		if (num <= num2 + 1)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE, true);
		}
	}

	public List<EncounterDatabaseEntry> GetQueuedTrackables()
	{
		List<EncounterDatabaseEntry> list = new List<EncounterDatabaseEntry>(m_queuedUnlocks.Count + m_newQueuedUnlocks.Count);
		for (int i = 0; i < m_queuedUnlocks.Count; i++)
		{
			list.Add(EncounterDatabase.GetEntry(m_queuedUnlocks[i]));
		}
		for (int j = 0; j < m_newQueuedUnlocks.Count; j++)
		{
			list.Add(EncounterDatabase.GetEntry(m_newQueuedUnlocks[j]));
		}
		return list;
	}

	public void AcknowledgeKnownTrackable(EncounterDatabaseEntry trackable)
	{
		GameStatsManager.Instance.MarkEncounterableAnnounced(trackable);
		m_queuedUnlocks.Remove(trackable.myGuid);
		m_newQueuedUnlocks.Remove(trackable.myGuid);
		m_knownEncounterables.Add(trackable.myGuid);
	}

	private IEnumerator EncounterableUnlockedChecks()
	{
		int currentEncounterableIndex = 0;
		List<EncounterDatabaseEntry> allTrackables = EncounterDatabase.Instance.Entries;
		while (true)
		{
			for (int i = 0; i < 20; i++)
			{
				currentEncounterableIndex = (currentEncounterableIndex + 1) % allTrackables.Count;
				if (allTrackables[currentEncounterableIndex] != null)
				{
					EncounterDatabaseEntry encounterDatabaseEntry = allTrackables[currentEncounterableIndex];
					if (encounterDatabaseEntry.prerequisites != null && encounterDatabaseEntry.prerequisites.Length != 0 && !encounterDatabaseEntry.journalData.SuppressInAmmonomicon && !m_knownEncounterables.Contains(encounterDatabaseEntry.myGuid) && !m_queuedUnlocks.Contains(encounterDatabaseEntry.myGuid) && !m_newQueuedUnlocks.Contains(encounterDatabaseEntry.myGuid) && encounterDatabaseEntry.PrerequisitesMet())
					{
						BraveUtility.Log(encounterDatabaseEntry.name + " has been unlocked!!!", Color.cyan, BraveUtility.LogVerbosity.IMPORTANT);
						m_newQueuedUnlocks.Add(encounterDatabaseEntry.myGuid);
					}
				}
			}
			if (!m_paused && PrimaryPlayer != null && PrimaryPlayer.CurrentRoom != null && !PrimaryPlayer.CurrentRoom.IsSealed && m_newQueuedUnlocks.Count > 0 && !GameUIRoot.Instance.notificationController.IsDoingNotification)
			{
				EncounterDatabaseEntry entry = EncounterDatabase.GetEntry(m_newQueuedUnlocks[0]);
				tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.Instance.EncounterIconCollection;
				int spriteIdByName = encounterIconCollection.GetSpriteIdByName(entry.journalData.AmmonomiconSprite);
				GameUIRoot.Instance.notificationController.DoCustomNotification(entry.journalData.GetPrimaryDisplayName(), StringTableManager.GetString("#SMALL_NOTIFICATION_UNLOCKED"), encounterIconCollection, spriteIdByName, UINotificationController.NotificationColor.GOLD);
				m_queuedUnlocks.Add(m_newQueuedUnlocks[0]);
				m_newQueuedUnlocks.RemoveAt(0);
			}
			yield return null;
		}
	}

	public void LaunchTimedEvent(float allowedTime, Action<bool> valueSetter)
	{
		StartCoroutine(HandleCustomTimer(allowedTime, valueSetter));
	}

	private IEnumerator HandleCustomTimer(float allowedTime, Action<bool> valueSetter)
	{
		valueSetter(true);
		float elapsed = 0f;
		while (elapsed < allowedTime)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		valueSetter(false);
	}

	public static int GetHashByComputerID()
	{
		int savedSystemHash = GameStatsManager.Instance.savedSystemHash;
		if (savedSystemHash != -1)
		{
			return savedSystemHash;
		}
		savedSystemHash = SystemInfo.deviceUniqueIdentifier.GetHashCode();
		GameStatsManager.Instance.savedSystemHash = savedSystemHash;
		return savedSystemHash;
	}

	public static DungeonData.Direction[] GetResourcefulRatSolution()
	{
		int hashByComputerID = GetHashByComputerID();
		System.Random random = new System.Random(hashByComputerID);
		DungeonData.Direction[] array = new DungeonData.Direction[6];
		for (int i = 0; i < 6; i++)
		{
			int num = random.Next(0, 4);
			if (i == 0 && num == 3)
			{
				num = random.Next(0, 3);
			}
			switch (num)
			{
			case 0:
				array[i] = DungeonData.Direction.NORTH;
				break;
			case 1:
				array[i] = DungeonData.Direction.EAST;
				break;
			case 2:
				array[i] = DungeonData.Direction.SOUTH;
				break;
			case 3:
				array[i] = DungeonData.Direction.WEST;
				break;
			default:
				Debug.LogError("Error in RR Solution");
				break;
			}
		}
		return array;
	}
}
