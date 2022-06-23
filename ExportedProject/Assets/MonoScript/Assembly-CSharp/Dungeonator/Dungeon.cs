using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brave;
using HutongGames.PlayMaker.Actions;
using Pathfinding;
using tk2dRuntime.TileMap;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dungeonator
{
	public class Dungeon : MonoBehaviour
	{
		public static bool IsGenerating;

		public ContentSource contentSource;

		public int DungeonSeed;

		public string DungeonShortName = string.Empty;

		public string DungeonFloorName = "Gungeon";

		public string DungeonFloorLevelTextOverride = "no override";

		public GameManager.LevelOverrideState LevelOverrideType;

		public DebugDungeonSettings debugSettings;

		public SemioticDungeonGenSettings PatternSettings;

		public bool ForceRegenerationOfCharacters;

		public bool ActuallyGenerateTilemap = true;

		public TilemapDecoSettings decoSettings;

		public TileIndices tileIndices;

		[SerializeField]
		public DungeonMaterial[] roomMaterialDefinitions;

		[SerializeField]
		public DungeonWingDefinition[] dungeonWingDefinitions;

		[SerializeField]
		public List<TileIndexGrid> pathGridDefinitions;

		public DustUpVFX dungeonDustups;

		public DamageTypeEffectMatrix damageTypeEffectMatrix;

		public DungeonTileStampData stampData;

		[Header("Procedural Room Population")]
		public bool UsesCustomFloorIdea;

		public RobotDaveIdea FloorIdea;

		[Header("Doors")]
		public bool PlaceDoors;

		public DungeonPlaceable doorObjects;

		public DungeonPlaceable lockedDoorObjects;

		public DungeonPlaceable oneWayDoorObjects;

		public GameObject oneWayDoorPressurePlate;

		public DungeonPlaceable phantomBlockerDoorObjects;

		public DungeonPlaceable alternateDoorObjectsNakatomi;

		public bool UsesWallWarpWingDoors;

		public GameObject WarpWingDoorPrefab;

		public GenericLootTable baseChestContents;

		[Header("Secret Rooms")]
		public List<GameObject> SecretRoomSimpleTriggersFacewall;

		public List<GameObject> SecretRoomSimpleTriggersSidewall;

		public List<ComplexSecretRoomTrigger> SecretRoomComplexTriggers;

		public GameObject SecretRoomDoorSparkVFX;

		public GameObject SecretRoomHorizontalPoofVFX;

		public GameObject SecretRoomVerticalPoofVFX;

		public GameObject RatTrapdoor;

		[EnemyIdentifier]
		public string NormalRatGUID;

		public SharedDungeonSettings sharedSettingsPrefab;

		[PickupIdentifier]
		public int BossMasteryTokenItemId = -1;

		public bool UsesOverrideTertiaryBossSets;

		public List<TertiaryBossRewardSet> OverrideTertiaryRewardSets;

		public DungeonData data;

		public GameObject defaultPlayerPrefab;

		public Action OnAnyRoomVisited;

		public Action OnAllRoomsVisited;

		private TK2DDungeonAssembler assembler;

		private tk2dTileMap m_tilemap;

		[Header("Special Scene Options")]
		public bool StripPlayerOnArrival;

		public bool SuppressEmergencyCrates;

		public bool SetTutorialFlag;

		[NonSerialized]
		public bool PreventPlayerLightInDarkTerrifyingRooms;

		public bool PlayerIsLight;

		public Color PlayerLightColor = Color.white;

		public float PlayerLightIntensity = 3f;

		public float PlayerLightRadius = 5f;

		public GameObject[] PrefabsToAutoSpawn;

		[NonSerialized]
		public int FrameDungeonGenerationFinished = -1;

		[NonSerialized]
		public bool IsGlitchDungeon;

		[NonSerialized]
		public bool OverrideAmbientLight;

		[NonSerialized]
		public Color OverrideAmbientColor;

		public const int TOP_WALL_ENEMY_BULLET_BLOCKER_PIXEL_HEIGHT = 14;

		public const int TOP_WALL_ENEMY_BLOCKER_PIXEL_HEIGHT = 12;

		public const int TOP_WALL_PLAYER_BLOCKER_PIXEL_HEIGHT = 8;

		[NonSerialized]
		public bool HasGivenMasteryToken;

		[NonSerialized]
		public bool HasGivenBossrushGun;

		[NonSerialized]
		public bool IsEndTimes;

		[NonSerialized]
		public bool CurseReaperActive;

		[NonSerialized]
		public float GeneratedMagnificence;

		public static bool ShouldAttemptToLoadFromMidgameSave;

		private bool m_allRoomsVisited;

		private bool m_musicIsPlaying;

		public string musicEventName;

		private float m_newPlayerMultiplier = -1f;

		protected List<GeneratedEnemyData> m_generatedEnemyData = new List<GeneratedEnemyData>();

		private bool m_ambientVFXProcessingActive;

		public tk2dTileMap MainTilemap
		{
			get
			{
				return m_tilemap;
			}
		}

		public int Width
		{
			get
			{
				return data.Width;
			}
		}

		public int Height
		{
			get
			{
				return data.Height;
			}
		}

		public float TargetAmbientIntensity
		{
			get
			{
				if (GameManager.Instance.IsFoyer)
				{
					return 1f;
				}
				return (GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && GameManager.Options.ShaderQuality != 0) ? 1f : 1.15f;
			}
		}

		public float ExplosionBulletDeletionMultiplier { get; set; }

		public bool IsExplosionBulletDeletionRecovering { get; set; }

		public bool AllRoomsVisited
		{
			get
			{
				return m_allRoomsVisited;
			}
		}

		public event Action<Dungeon, PlayerController> OnDungeonGenerationComplete;

		public int GetDungeonSeed()
		{
			if (!BraveRandom.IsInitialized())
			{
				BraveRandom.InitializeRandom();
			}
			int num = GameManager.Instance.CurrentRunSeed;
			if (num == 0)
			{
				num = DungeonSeed;
			}
			if (num == 0)
			{
				num = BraveRandom.GenerationRandomRange(1, 1000000000);
			}
			else
			{
				GameManager.Instance.InitializeForRunWithSeed(num);
			}
			return num;
		}

		public DungeonWingDefinition SelectWingDefinition(bool criticalPath)
		{
			List<DungeonWingDefinition> list = new List<DungeonWingDefinition>();
			float num = 0f;
			for (int i = 0; i < dungeonWingDefinitions.Length; i++)
			{
				if ((dungeonWingDefinitions[i].canBeCriticalPath && criticalPath) || (dungeonWingDefinitions[i].canBeNoncriticalPath && !criticalPath))
				{
					list.Add(dungeonWingDefinitions[i]);
					num += dungeonWingDefinitions[i].weight;
				}
			}
			float num2 = num * BraveRandom.GenerationRandomValue();
			float num3 = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				num3 += list[j].weight;
				if (num3 >= num2)
				{
					return list[j];
				}
			}
			return list[0];
		}

		private IEnumerator Start()
		{
			AkSoundEngine.PostEvent("Play_AMB_sewer_loop_01", base.gameObject);
			bool flag = !GameStatsManager.Instance.IsInSession;
			if (PrefabsToAutoSpawn.Length > 0)
			{
				for (int i = 0; i < PrefabsToAutoSpawn.Length; i++)
				{
					UnityEngine.Object.Instantiate(PrefabsToAutoSpawn[i]);
				}
			}
			if (flag)
			{
				IEnumerator enumerator = Regenerate(false);
				while (enumerator.MoveNext())
				{
				}
			}
			else
			{
				StartCoroutine(Regenerate(false));
			}
			RenderSettings.ambientLight = ((GameManager.Options.LightingQuality != 0) ? decoSettings.ambientLightColor : decoSettings.lowQualityAmbientLightColor);
			RenderSettings.ambientIntensity = TargetAmbientIntensity;
			if (decoSettings.UsesAlienFXFloorColor)
			{
				PlatformInterface.SetAlienFXAmbientColor(decoSettings.AlienFXFloorColor);
			}
			else
			{
				PlatformInterface.SetAlienFXAmbientColor(RenderSettings.ambientLight);
			}
			yield break;
		}

		public void RegenerationCleanup()
		{
			GameObject gameObject = GameObject.Find("A*");
			if (gameObject != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
			GameObject gameObject2 = GameObject.Find("_Lights");
			if (gameObject2 != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject2);
			}
			GameObject gameObject3 = GameObject.Find("_Rooms");
			if (gameObject3 != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject3);
			}
			GameObject gameObject4 = GameObject.Find("_Doors");
			if (gameObject4 != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject4);
			}
			GameObject gameObject5 = GameObject.Find("_SpawnManager");
			if (gameObject5 != null)
			{
				UnityEngine.Object.DestroyImmediate(gameObject5);
			}
		}

		private void StartupFoyerChecks()
		{
			Debug.LogError("Doing startup foyer checks!");
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHOP_HAS_MET_BEETLE) && !GameStatsManager.Instance.GetFlag(GungeonFlags.SHOP_BEETLE_ACTIVE) && GameStatsManager.Instance.AccumulatedBeetleMerchantChance > 0f)
			{
				float num = GameStatsManager.Instance.AccumulatedBeetleMerchantChance + GameStatsManager.Instance.AccumulatedUsedBeetleMerchantChance;
				if (UnityEngine.Random.value < num)
				{
					GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 0f;
					GameStatsManager.Instance.AccumulatedUsedBeetleMerchantChance = 0f;
					GameStatsManager.Instance.SetFlag(GungeonFlags.SHOP_BEETLE_ACTIVE, true);
				}
				else
				{
					GameStatsManager.Instance.AccumulatedUsedBeetleMerchantChance += GameStatsManager.Instance.AccumulatedBeetleMerchantChance;
					GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 0f;
				}
			}
		}

		public IEnumerator Regenerate(bool cleanup)
		{
			if (cleanup)
			{
				RegenerationCleanup();
			}
			if (LevelOverrideType == GameManager.LevelOverrideState.FOYER)
			{
				StartupFoyerChecks();
			}
			FrameDungeonGenerationFinished = -1;
			IsGenerating = true;
			float elapsedTimeAtStartup = Time.realtimeSinceStartup;
			int frameAtStartup = Time.frameCount;
			GameManager.Instance.InTutorial = SetTutorialFlag;
			MidGameSaveData midgameSave = null;
			if (ShouldAttemptToLoadFromMidgameSave && !GameManager.VerifyAndLoadMidgameSave(out midgameSave))
			{
				midgameSave = null;
			}
			ShouldAttemptToLoadFromMidgameSave = false;
			if (midgameSave != null)
			{
				List<string> list = new List<string>(Brave.PlayerPrefs.GetStringArray("recent_mgs"));
				list.Insert(0, midgameSave.midGameSaveGuid);
				while (list.Count > 5)
				{
					list.RemoveAt(list.Count - 1);
				}
				Brave.PlayerPrefs.SetStringArray("recent_mgs", list.ToArray());
			}
			GeneratePlayerIfNecessary(midgameSave);
			int dSeed = GetDungeonSeed();
			UnityEngine.Random.InitState(dSeed);
			BraveRandom.InitializeWithSeed(dSeed);
			if (!MetaInjectionData.BlueprintGenerated && GameManager.Instance.GlobalInjectionData != null)
			{
				GameManager.Instance.GlobalInjectionData.PreprocessRun();
			}
			if (debugSettings.RAPID_DEBUG_DUNGEON_ITERATION)
			{
				for (int i = 0; i < debugSettings.RAPID_DEBUG_DUNGEON_COUNT; i++)
				{
					float realtimeSinceStartup = Time.realtimeSinceStartup;
					int num = (DungeonSeed = i + 1);
					LoopDungeonGenerator loopDungeonGenerator = new LoopDungeonGenerator(this, GetDungeonSeed());
					loopDungeonGenerator.RAPID_DEBUG_ITERATION_MODE = true;
					loopDungeonGenerator.RAPID_DEBUG_ITERATION_INDEX = i;
					loopDungeonGenerator.GenerateDungeonLayout();
					Debug.Log(string.Concat(str3: (Time.realtimeSinceStartup - realtimeSinceStartup).ToString(), str0: "seed #", str1: num.ToString(), str2: " took "));
				}
				yield break;
			}
			DungeonData d2 = null;
			if (GameManager.Instance.PregeneratedDungeonData == null)
			{
				LoopDungeonGenerator loopDungeonGenerator2 = new LoopDungeonGenerator(this, GetDungeonSeed());
				d2 = loopDungeonGenerator2.GenerateDungeonLayout();
			}
			else
			{
				d2 = GameManager.Instance.PregeneratedDungeonData;
				GameManager.Instance.PregeneratedDungeonData = null;
			}
			data = d2;
			BraveUtility.Log("Layout phase complete.", Color.green, BraveUtility.LogVerbosity.IMPORTANT);
			if (assembler == null)
			{
				assembler = new TK2DDungeonAssembler();
			}
			assembler.Initialize(tileIndices);
			if (m_tilemap == null)
			{
				GameObject gameObject = GameObject.Find("TileMap");
				m_tilemap = gameObject.GetComponent<tk2dTileMap>();
			}
			else
			{
				assembler.ClearData(m_tilemap);
			}
			BraveUtility.Log("Dungeon Data Phase 1 Complete @ " + (Time.realtimeSinceStartup - elapsedTimeAtStartup), Color.green, BraveUtility.LogVerbosity.IMPORTANT);
			DebugTime.Log(elapsedTimeAtStartup, frameAtStartup, "Dungeon Data Phase 1 Complete");
			IEnumerator ApplicationTracker = d2.Apply(tileIndices, decoSettings, m_tilemap);
			while (ApplicationTracker.MoveNext())
			{
				yield return null;
			}
			d2.CheckIntegrity();
			if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON && GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_BULLET_COMPLETE))
			{
				PlaceRatGrate();
			}
			if ((tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON || tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON || tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATACOMBGEON) && GameStatsManager.Instance.AnyPastBeaten() && !GameStatsManager.Instance.GetFlag(GungeonFlags.FLAG_EEVEE_UNLOCKED) && (!GameManager.Instance.PrimaryPlayer || !GameManager.Instance.PrimaryPlayer.IsTemporaryEeveeForUnlock) && UnityEngine.Random.value < 0.2f)
			{
				PlaceParadoxPortal();
			}
			PlaceWallMimics();
			TK2DDungeonAssembler.RuntimeResizeTileMap(m_tilemap, d2.Width, d2.Height, m_tilemap.partitionSizeX, m_tilemap.partitionSizeY);
			IEnumerator AssemblyTracker = assembler.ConstructTK2DDungeon(this, m_tilemap);
			while (AssemblyTracker.MoveNext())
			{
				yield return null;
			}
			d2.PostProcessFeatures();
			TelevisionQuestController.RemoveMaintenanceRoomBackpack();
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
			{
				TelevisionQuestController.HandlePuzzleSetup();
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				RobotArmQuestController.HandlePuzzleSetup();
			}
			BraveUtility.Log("Dungeon Data Phase 2 Complete @ " + (Time.realtimeSinceStartup - elapsedTimeAtStartup), Color.green, BraveUtility.LogVerbosity.IMPORTANT);
			DebugTime.Log(elapsedTimeAtStartup, frameAtStartup, "Dungeon Data Phase 2 Complete");
			if (Minimap.Instance != null)
			{
				Minimap.Instance.InitializeMinimap(d2);
			}
			PlacePlayerInRoom(startRoom: d2.Entrance, map: m_tilemap);
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_DEATHS) == 0f)
				{
					GameUIRoot.Instance.levelNameUI.ShowLevelName(this);
				}
			}
			else
			{
				GameUIRoot.Instance.levelNameUI.ShowLevelName(this);
			}
			Pathfinder.Instance.Initialize(d2);
			ShadowSystem.ForceAllLightsUpdate();
			GameManager.Instance.ClearGenerativeDungeonData();
			data.PostGenerationCleanup();
			if (midgameSave != null)
			{
				midgameSave.LoadPreGenDataFromMidGameSave();
			}
			FloorReached();
			IEnumerator PostGenerationTracker = PostDungeonGenerationCleanup();
			while (PostGenerationTracker.MoveNext())
			{
				yield return null;
			}
			if (midgameSave != null)
			{
				if (midgameSave.StaticShopData != null)
				{
					BaseShopController.LoadFromMidGameSave(midgameSave.StaticShopData);
				}
				StartCoroutine(FrameDelayedMidgameInitialization(midgameSave));
			}
			FrameDungeonGenerationFinished = Time.frameCount;
			IsGenerating = false;
			BossManager.PriorFloorSelectedBossRoom = null;
			if (this.OnDungeonGenerationComplete != null)
			{
				this.OnDungeonGenerationComplete(this, GameManager.Instance.PrimaryPlayer);
			}
			AssignCurrencyDrops();
			if (GameStatsManager.Instance.IsRainbowRun)
			{
				List<TeleporterController> componentsAbsoluteInRoom = data.Entrance.GetComponentsAbsoluteInRoom<TeleporterController>();
				Vector3? vector = null;
				if (componentsAbsoluteInRoom != null && componentsAbsoluteInRoom.Count > 0)
				{
					vector = componentsAbsoluteInRoom[0].transform.position + new Vector3(0.5f, 2f, 0f);
				}
				if (!vector.HasValue)
				{
					bool success = false;
					IntVector2 centeredVisibleClearSpot = data.Entrance.GetCenteredVisibleClearSpot(4, 4, out success, true);
					if (success)
					{
						vector = (centeredVisibleClearSpot + IntVector2.One).ToVector2().ToVector3ZisY();
					}
				}
				if (vector.HasValue)
				{
					Chest chest = Chest.Spawn(GameManager.Instance.RewardManager.A_Chest, vector.Value, data.Entrance, true);
					chest.IsRainbowChest = true;
					chest.BecomeRainbowChest();
				}
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController)
				{
					playerController.specRigidbody.PhysicsRegistration = SpeculativeRigidbody.RegistrationState.Unknown;
					PhysicsEngine.Instance.Register(playerController.specRigidbody);
				}
			}
		}

		private IEnumerator FrameDelayedMidgameInitialization(MidGameSaveData midgameSave)
		{
			yield return null;
			if (midgameSave != null)
			{
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					midgameSave.LoadDataFromMidGameSave(GameManager.Instance.PrimaryPlayer, GameManager.Instance.SecondaryPlayer);
				}
				else
				{
					midgameSave.LoadDataFromMidGameSave(GameManager.Instance.PrimaryPlayer, null);
				}
			}
		}

		public void AssignCurrencyDrops()
		{
			FloorRewardData currentRewardData = GameManager.Instance.RewardManager.CurrentRewardData;
			float randomByNormalDistribution = BraveMathCollege.GetRandomByNormalDistribution(currentRewardData.AverageCurrencyDropsThisFloor, currentRewardData.CurrencyDropsStandardDeviation);
			float min = Mathf.Max(20f, currentRewardData.MinimumCurrencyDropsThisFloor);
			int num = Mathf.CeilToInt(Mathf.Clamp(randomByNormalDistribution, min, 250f));
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE && (bool)GameManager.Instance.PrimaryPlayer && PrepareTakeSherpaPickup.IsOnSherpaMoneyStep && GameManager.Instance.PrimaryPlayer.carriedConsumables.KeyBullets > 1)
			{
				num = Mathf.CeilToInt(Mathf.Max(num, currentRewardData.AverageCurrencyDropsThisFloor + currentRewardData.CurrencyDropsStandardDeviation));
			}
			float bossGoldCoinChance = GameManager.Instance.RewardManager.BossGoldCoinChance;
			float powerfulGoldCoinChance = GameManager.Instance.RewardManager.PowerfulGoldCoinChance;
			float normalGoldCoinChance = GameManager.Instance.RewardManager.NormalGoldCoinChance;
			List<AIActor> list = new List<AIActor>();
			List<AIActor> list2 = new List<AIActor>();
			List<AIActor> list3 = new List<AIActor>();
			for (int i = 0; i < StaticReferenceManager.AllEnemies.Count; i++)
			{
				AIActor aIActor = StaticReferenceManager.AllEnemies[i];
				if ((bool)aIActor && aIActor.CanDropCurrency)
				{
					if ((bool)aIActor.healthHaver && aIActor.healthHaver.IsBoss)
					{
						list.Add(aIActor);
					}
					else if (aIActor.IsSignatureEnemy)
					{
						list2.Add(aIActor);
					}
					else
					{
						list3.Add(aIActor);
					}
				}
			}
			int totalEnemyCount = list3.Count + list2.Count;
			for (int j = 0; j < list3.Count; j++)
			{
				if (!(list3[j].EnemyGuid == GlobalEnemyGuids.GripMaster) && !list3[j].IsMimicEnemy)
				{
					RegisterGeneratedEnemyData(list3[j].EnemyGuid, totalEnemyCount, false);
				}
			}
			for (int k = 0; k < list2.Count; k++)
			{
				if (!(list2[k].EnemyGuid == GlobalEnemyGuids.GripMaster) && !list2[k].IsMimicEnemy)
				{
					RegisterGeneratedEnemyData(list2[k].EnemyGuid, totalEnemyCount, true);
				}
			}
			int num2 = ((list.Count > 0) ? BraveRandom.GenerationRandomRange(5, num / 4) : 0);
			int a = list2.Count * 10;
			int num3 = ((list2.Count > 0) ? Mathf.Min(a, Mathf.FloorToInt((float)(num - num2) * 0.5f)) : 0);
			int num4 = num - (num2 + num3);
			int num5 = Mathf.CeilToInt((float)num2 / (float)list.Count);
			int num6 = Mathf.CeilToInt((float)num3 / (float)list2.Count);
			for (int l = 0; l < list.Count; l++)
			{
				list[l].AssignedCurrencyToDrop = Mathf.Min(num5, num2);
				num2 -= num5;
				if (BraveRandom.GenerationRandomValue() < bossGoldCoinChance)
				{
					list[l].AssignedCurrencyToDrop += 50;
				}
			}
			for (int m = 0; m < list2.Count; m++)
			{
				list2[m].AssignedCurrencyToDrop = Mathf.Min(num3, num6);
				num3 -= num6;
				if (BraveRandom.GenerationRandomValue() < powerfulGoldCoinChance)
				{
					list2[m].AssignedCurrencyToDrop += 50;
				}
			}
			while (num4 > 0 && list3.Count > 0)
			{
				list3[BraveRandom.GenerationRandomRange(0, list3.Count)].AssignedCurrencyToDrop++;
				num4--;
			}
			for (int n = 0; n < list3.Count; n++)
			{
				if (BraveRandom.GenerationRandomValue() < normalGoldCoinChance)
				{
					list3[n].AssignedCurrencyToDrop += 50;
				}
			}
		}

		private IEnumerator ForceRegenerateDelayed()
		{
			yield return new WaitForSeconds(5f);
			GameManager.Instance.LoadCustomLevel("tt_castle");
		}

		private IEnumerator PostDungeonGenerationCleanup()
		{
			if ((Application.platform != RuntimePlatform.XboxOne && Application.platform != RuntimePlatform.MetroPlayerX64 && Application.platform != RuntimePlatform.MetroPlayerX86) || LevelOverrideType != GameManager.LevelOverrideState.FOYER)
			{
				for (int j = 0; j < PatternSettings.flows.Count; j++)
				{
					Resources.UnloadAsset(PatternSettings.flows[j]);
				}
				PatternSettings = null;
			}
			for (int i = 0; i < data.rooms.Count; i++)
			{
				data.rooms[i].PostGenerationCleanup();
				if (i % 5 == 0)
				{
					yield return null;
				}
			}
			HUDGC.SkipNextGC = true;
			Resources.UnloadUnusedAssets();
			BraveMemory.DoCollect();
		}

		public tk2dTileMap DestroyWallAtPosition(int ix, int iy, bool deferRebuild = true)
		{
			if (data.cellData[ix][iy] == null)
			{
				return null;
			}
			if (data.cellData[ix][iy].type != CellType.WALL)
			{
				return null;
			}
			if (!data.cellData[ix][iy].breakable)
			{
				return null;
			}
			data.cellData[ix][iy].type = CellType.FLOOR;
			if (data.isSingleCellWall(ix, iy + 1))
			{
				data[ix, iy + 1].type = CellType.FLOOR;
			}
			if (data.isSingleCellWall(ix, iy - 1))
			{
				data[ix, iy - 1].type = CellType.FLOOR;
			}
			RoomHandler parentRoom = data.cellData[ix][iy].parentRoom;
			tk2dTileMap tk2dTileMap = ((parentRoom == null || !(parentRoom.OverrideTilemap != null)) ? m_tilemap : parentRoom.OverrideTilemap);
			for (int i = -1; i < 2; i++)
			{
				for (int j = -2; j < 4; j++)
				{
					CellData cellData = data.cellData[ix + i][iy + j];
					if (cellData == null)
					{
						continue;
					}
					cellData.hasBeenGenerated = false;
					if (cellData.parentRoom != null)
					{
						List<GameObject> list = new List<GameObject>();
						for (int k = 0; k < cellData.parentRoom.hierarchyParent.childCount; k++)
						{
							Transform child = cellData.parentRoom.hierarchyParent.GetChild(k);
							if (child.name.StartsWith("Chunk_"))
							{
								list.Add(child.gameObject);
							}
						}
						for (int num = list.Count - 1; num >= 0; num--)
						{
							UnityEngine.Object.Destroy(list[num]);
						}
					}
					assembler.ClearTileIndicesForCell(this, tk2dTileMap, cellData.position.x, cellData.position.y);
					assembler.BuildTileIndicesForCell(this, tk2dTileMap, cellData.position.x, cellData.position.y);
					cellData.HasCachedPhysicsTile = false;
					cellData.CachedPhysicsTile = null;
				}
			}
			if (!deferRebuild)
			{
				RebuildTilemap(tk2dTileMap);
			}
			return tk2dTileMap;
		}

		public tk2dTileMap ConstructWallAtPosition(int ix, int iy, bool deferRebuild = true)
		{
			if (data.cellData[ix][iy].type == CellType.WALL)
			{
				return null;
			}
			data.cellData[ix][iy].type = CellType.WALL;
			RoomHandler parentRoom = data.cellData[ix][iy].parentRoom;
			tk2dTileMap tk2dTileMap = ((parentRoom == null || !(parentRoom.OverrideTilemap != null)) ? m_tilemap : parentRoom.OverrideTilemap);
			for (int i = -1; i < 2; i++)
			{
				for (int j = -2; j < 4; j++)
				{
					CellData cellData = data.cellData[ix + i][iy + j];
					if (cellData == null)
					{
						continue;
					}
					cellData.hasBeenGenerated = false;
					if (cellData.parentRoom != null)
					{
						List<GameObject> list = new List<GameObject>();
						for (int k = 0; k < cellData.parentRoom.hierarchyParent.childCount; k++)
						{
							Transform child = cellData.parentRoom.hierarchyParent.GetChild(k);
							if (child.name.StartsWith("Chunk_"))
							{
								list.Add(child.gameObject);
							}
						}
						for (int num = list.Count - 1; num >= 0; num--)
						{
							UnityEngine.Object.Destroy(list[num]);
						}
					}
					assembler.ClearTileIndicesForCell(this, tk2dTileMap, cellData.position.x, cellData.position.y);
					assembler.BuildTileIndicesForCell(this, tk2dTileMap, cellData.position.x, cellData.position.y);
					cellData.HasCachedPhysicsTile = false;
					cellData.CachedPhysicsTile = null;
				}
			}
			if (!deferRebuild)
			{
				RebuildTilemap(tk2dTileMap);
			}
			return tk2dTileMap;
		}

		public void RebuildTilemap(tk2dTileMap targetTilemap)
		{
			RenderMeshBuilder.CurrentCellXOffset = Mathf.RoundToInt(targetTilemap.renderData.transform.position.x);
			RenderMeshBuilder.CurrentCellYOffset = Mathf.RoundToInt(targetTilemap.renderData.transform.position.y);
			targetTilemap.Build();
			targetTilemap.renderData.transform.position = new Vector3(RenderMeshBuilder.CurrentCellXOffset, RenderMeshBuilder.CurrentCellYOffset, RenderMeshBuilder.CurrentCellYOffset);
			RenderMeshBuilder.CurrentCellXOffset = 0;
			RenderMeshBuilder.CurrentCellYOffset = 0;
		}

		public void InformRoomCleared(bool rewardDropped, bool rewardIsChest)
		{
			if (rewardDropped)
			{
				if (rewardIsChest)
				{
					GameManager.Instance.PrimaryPlayer.AdditionalChestSpawnChance = 0f;
					BraveUtility.Log("Spawning a chest: flooring chest spawn chance.", Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);
				}
				else
				{
					GameManager.Instance.PrimaryPlayer.AdditionalChestSpawnChance = 0f;
					BraveUtility.Log("Spawning a single item: flooring chest spawn chance.", Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);
				}
				return;
			}
			float num = GameManager.Instance.RewardManager.CurrentRewardData.ChestSystem_Increment;
			if (PassiveItem.IsFlagSetForCharacter(GameManager.Instance.PrimaryPlayer, typeof(AmazingChestAheadItem)))
			{
				num *= AmazingChestAheadItem.ChestIncrementMultiplier;
				int count = 0;
				if (PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.DOUBLE_CHEST_FRIENDS, out count))
				{
					num *= 1.25f;
				}
			}
			num = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? (num * GameManager.Instance.RewardManager.SinglePlayerPickupIncrementModifier) : (num * GameManager.Instance.RewardManager.CoopPickupIncrementModifier));
			Debug.Log("LootSystem::" + GameManager.Instance.PrimaryPlayer.AdditionalChestSpawnChance + " + " + num);
			GameManager.Instance.PrimaryPlayer.AdditionalChestSpawnChance += num;
		}

		public void FloorReached()
		{
			string presence = DungeonFloorName.Replace("#", string.Empty);
			GameManager.Instance.platformInterface.SetPresence(presence);
			if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.RATGEON)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_RATGEON, 1f);
			}
			if (GameManager.Instance.CurrentLevelOverrideState != 0)
			{
				return;
			}
			switch (tileIndices.tilesetId)
			{
			case GlobalDungeonData.ValidTilesets.CASTLEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.NUMBER_ATTEMPTS, 1f);
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.RUNS_PLAYED_POST_FTA, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.GUNGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_GUNGEON, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.MINEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_MINES, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_CATACOMBS, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.FORGEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_FORGE, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.HELLGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_BULLET_HELL, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.SEWERGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_SEWERS, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_CATHEDRAL, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.WESTGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_WEST, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.SPACEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_FUTURE, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.OFFICEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_NAKATOMI, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.JUNGLEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_JUNGLE, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.BELLYGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_BELLY, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.PHOBOSGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_PHOBOS, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.RATGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_REACHED_RATGEON, 1f);
				break;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
			{
				if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
				{
					GameStatsManager.Instance.isChump = true;
				}
				else
				{
					GameStatsManager.Instance.isChump = false;
				}
			}
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_UNLOCK_COMPANION_SHRINE) && GameStatsManager.Instance.GetNumberOfCompanionsUnlocked() >= 5)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_UNLOCK_COMPANION_SHRINE, true);
			}
			if (ChallengeManager.CHALLENGE_MODE_ACTIVE && tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.DAISUKE_CHALLENGE_HALFITEM_UNLOCK, true);
			}
			if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_05))
			{
				float num = 0.04f;
				switch (tileIndices.tilesetId)
				{
				case GlobalDungeonData.ValidTilesets.GUNGEON:
				case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
					num = 0.08f;
					break;
				case GlobalDungeonData.ValidTilesets.MINEGEON:
					num = 0.12f;
					break;
				case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
					num = 0.16f;
					break;
				case GlobalDungeonData.ValidTilesets.FORGEGEON:
					num = 0.2f;
					break;
				case GlobalDungeonData.ValidTilesets.HELLGEON:
					num = 0.25f;
					break;
				}
				if (GameStatsManager.Instance.AnyPastBeaten() && UnityEngine.Random.value < num)
				{
					List<int> input = Enumerable.Range(0, data.rooms.Count).ToList();
					input = input.Shuffle();
					for (int i = 0; i < input.Count && (data.rooms[input[i]].area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.NORMAL || !data.rooms[input[i]].EverHadEnemies || !data.rooms[input[i]].AddMysteriousBulletManToRoom()); i++)
					{
					}
				}
			}
			int numKeybulletMenForFloor = MetaInjectionData.GetNumKeybulletMenForFloor(tileIndices.tilesetId);
			if (numKeybulletMenForFloor > 0)
			{
				List<RoomHandler> list = new List<RoomHandler>();
				for (int j = 0; j < numKeybulletMenForFloor; j++)
				{
					List<int> input2 = Enumerable.Range(0, data.rooms.Count).ToList();
					input2 = input2.Shuffle();
					bool flag = false;
					for (int k = 0; k < 2; k++)
					{
						for (int l = 0; l < input2.Count; l++)
						{
							RoomHandler roomHandler = data.rooms[input2[l]];
							if ((!list.Contains(roomHandler) || !(UnityEngine.Random.value > 0.1f)) && (k == 1 || roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.HUB) && roomHandler.EverHadEnemies && roomHandler.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.BOSS)
							{
								flag = true;
								roomHandler.AddSpecificEnemyToRoomProcedurally(GameManager.Instance.RewardManager.KeybulletsChances.EnemyGuid);
								list.Add(roomHandler);
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
			}
			int numChanceBulletMenForFloor = MetaInjectionData.GetNumChanceBulletMenForFloor(tileIndices.tilesetId);
			if (numChanceBulletMenForFloor > 0)
			{
				List<RoomHandler> list2 = new List<RoomHandler>();
				for (int m = 0; m < numChanceBulletMenForFloor; m++)
				{
					List<int> input3 = Enumerable.Range(0, data.rooms.Count).ToList();
					input3 = input3.Shuffle();
					bool flag2 = false;
					for (int n = 0; n < 2; n++)
					{
						for (int num2 = 0; num2 < input3.Count; num2++)
						{
							RoomHandler roomHandler2 = data.rooms[input3[num2]];
							if ((!list2.Contains(roomHandler2) || !(UnityEngine.Random.value > 0.1f)) && (n == 1 || roomHandler2.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.HUB) && roomHandler2.EverHadEnemies && roomHandler2.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.BOSS)
							{
								flag2 = true;
								roomHandler2.AddSpecificEnemyToRoomProcedurally(GameManager.Instance.RewardManager.ChanceBulletChances.EnemyGuid);
								list2.Add(roomHandler2);
								break;
							}
						}
						if (flag2)
						{
							break;
						}
					}
				}
			}
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIME_PLAYED) > 18000f && UnityEngine.Random.value < GameManager.Instance.RewardManager.FacelessChancePerFloor)
			{
				List<int> input4 = Enumerable.Range(0, data.rooms.Count).ToList();
				input4 = input4.Shuffle();
				for (int num3 = 0; num3 < input4.Count; num3++)
				{
					RoomHandler roomHandler3 = data.rooms[input4[num3]];
					if (roomHandler3.EverHadEnemies && (roomHandler3.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.HUB || roomHandler3.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) > 6))
					{
						AIActor toughestEnemy = roomHandler3.GetToughestEnemy();
						if ((bool)toughestEnemy)
						{
							UnityEngine.Object.Destroy(toughestEnemy.gameObject);
						}
						roomHandler3.AddSpecificEnemyToRoomProcedurally(GameManager.Instance.RewardManager.FacelessCultistGuid);
						break;
					}
				}
			}
			HandleAGDInjection();
			for (int num4 = 0; num4 < GameManager.Instance.AllPlayers.Length; num4++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[num4];
				if ((bool)playerController)
				{
					playerController.HasReceivedNewGunThisFloor = false;
					playerController.HasTakenDamageThisFloor = false;
				}
			}
		}

		private void HandleAGDInjection()
		{
			List<AIActor> outList = new List<AIActor>();
			RunData runData = GameManager.Instance.RunData;
			if (runData == null)
			{
				runData = new RunData();
			}
			if (runData.AgdInjectionRunCounts == null || runData.AgdInjectionRunCounts.Length != GameManager.Instance.EnemyReplacementTiers.Count)
			{
				runData.AgdInjectionRunCounts = new int[GameManager.Instance.EnemyReplacementTiers.Count];
			}
			int[] agdInjectionRunCounts = runData.AgdInjectionRunCounts;
			List<RoomHandler> list = new List<RoomHandler>();
			if (data != null && data.rooms != null)
			{
				list.AddRange(data.rooms);
			}
			for (int i = 0; i < GameManager.Instance.EnemyReplacementTiers.Count; i++)
			{
				AGDEnemyReplacementTier aGDEnemyReplacementTier = GameManager.Instance.EnemyReplacementTiers[i];
				int num = 0;
				if (aGDEnemyReplacementTier == null || (tileIndices != null && (tileIndices.tilesetId & aGDEnemyReplacementTier.TargetTileset) != tileIndices.tilesetId) || aGDEnemyReplacementTier.ExcludeForPrereqs() || (aGDEnemyReplacementTier.MaxPerRun > 0 && agdInjectionRunCounts[i] >= aGDEnemyReplacementTier.MaxPerRun))
				{
					continue;
				}
				BraveUtility.RandomizeList(list);
				foreach (RoomHandler item in list)
				{
					if (!item.EverHadEnemies || !item.IsStandardRoom || aGDEnemyReplacementTier.ExcludeRoomForColumns(data, item) || aGDEnemyReplacementTier.ExcludeRoom(item))
					{
						continue;
					}
					item.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref outList);
					if (aGDEnemyReplacementTier.ExcludeRoomForEnemies(item, outList))
					{
						continue;
					}
					for (int j = 0; j < outList.Count; j++)
					{
						AIActor aIActor = outList[j];
						if (!aIActor || (aIActor.AdditionalSimpleItemDrops != null && aIActor.AdditionalSimpleItemDrops.Count > 0) || ((bool)aIActor.healthHaver && aIActor.healthHaver.IsBoss) || ((!aGDEnemyReplacementTier.TargetAllSignatureEnemies || !aIActor.IsSignatureEnemy) && (!aGDEnemyReplacementTier.TargetAllNonSignatureEnemies || aIActor.IsSignatureEnemy) && (aGDEnemyReplacementTier.TargetGuids == null || !aGDEnemyReplacementTier.TargetGuids.Contains(aIActor.EnemyGuid))) || !(UnityEngine.Random.value < aGDEnemyReplacementTier.ChanceToReplace))
						{
							continue;
						}
						Vector2? vector = null;
						if (aGDEnemyReplacementTier.RemoveAllOtherEnemies)
						{
							vector = item.area.Center;
							for (int num2 = outList.Count - 1; num2 >= 0; num2--)
							{
								AIActor aIActor2 = outList[j];
								if ((bool)aIActor2)
								{
									item.DeregisterEnemy(aIActor2, true);
									UnityEngine.Object.Destroy(aIActor2.gameObject);
								}
							}
						}
						else
						{
							if ((bool)aIActor.specRigidbody)
							{
								aIActor.specRigidbody.Initialize();
								vector = aIActor.specRigidbody.UnitBottomLeft;
							}
							item.DeregisterEnemy(aIActor, true);
							UnityEngine.Object.Destroy(aIActor.gameObject);
						}
						string enemyGuid = BraveUtility.RandomElement(aGDEnemyReplacementTier.ReplacementGuids);
						Vector2? goalPosition = vector;
						item.AddSpecificEnemyToRoomProcedurally(enemyGuid, false, goalPosition);
						num++;
						agdInjectionRunCounts[i]++;
						if ((aGDEnemyReplacementTier.MaxPerFloor > 0 && num >= aGDEnemyReplacementTier.MaxPerFloor) || (aGDEnemyReplacementTier.MaxPerRun > 0 && agdInjectionRunCounts[i] >= aGDEnemyReplacementTier.MaxPerRun) || aGDEnemyReplacementTier.RemoveAllOtherEnemies)
						{
							break;
						}
					}
					if ((aGDEnemyReplacementTier.MaxPerFloor <= 0 || num < aGDEnemyReplacementTier.MaxPerFloor) && (aGDEnemyReplacementTier.MaxPerRun <= 0 || agdInjectionRunCounts[i] < aGDEnemyReplacementTier.MaxPerRun))
					{
						continue;
					}
					break;
				}
			}
			CullEnemiesForNewPlayers();
		}

		private void CullEnemiesForNewPlayers()
		{
			List<AIActor> outList = new List<AIActor>();
			float newPlayerEnemyCullFactor = GameStatsManager.Instance.NewPlayerEnemyCullFactor;
			if (!(newPlayerEnemyCullFactor > 0f))
			{
				return;
			}
			foreach (RoomHandler room in data.rooms)
			{
				if (!room.EverHadEnemies || !room.IsStandardRoom || room.IsGunslingKingChallengeRoom)
				{
					continue;
				}
				if (room.area.runtimePrototypeData != null && room.area.runtimePrototypeData.roomEvents != null)
				{
					bool flag = false;
					for (int i = 0; i < room.area.runtimePrototypeData.roomEvents.Count; i++)
					{
						if (room.area.runtimePrototypeData.roomEvents[i].action == RoomEventTriggerAction.BECOME_TERRIFYING_AND_DARK)
						{
							flag = true;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				room.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref outList);
				for (int j = 0; j < outList.Count; j++)
				{
					AIActor aIActor = outList[j];
					if ((bool)aIActor && (aIActor.AdditionalSimpleItemDrops == null || aIActor.AdditionalSimpleItemDrops.Count <= 0) && aIActor.IsNormalEnemy && !aIActor.IsHarmlessEnemy && aIActor.IsWorthShootingAt && (!aIActor.healthHaver || !aIActor.healthHaver.IsBoss) && UnityEngine.Random.value < newPlayerEnemyCullFactor)
					{
						UnityEngine.Object.Destroy(aIActor.gameObject);
					}
				}
			}
		}

		private void PlaceFloorObjectInternal(DungeonPlaceableBehaviour prefabPlaceable, IntVector2 dimensions, Vector2 offset)
		{
			List<IntVector2> list = new List<IntVector2>();
			for (int i = 0; i < data.rooms.Count; i++)
			{
				RoomHandler roomHandler = data.rooms[i];
				if (roomHandler.area.IsProceduralRoom || roomHandler.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.NORMAL || (bool)roomHandler.OptionalDoorTopDecorable || roomHandler.area.prototypeRoom.UseCustomMusic)
				{
					continue;
				}
				for (int j = roomHandler.area.basePosition.x; j < roomHandler.area.basePosition.x + roomHandler.area.dimensions.x; j++)
				{
					for (int k = roomHandler.area.basePosition.y; k < roomHandler.area.basePosition.y + roomHandler.area.dimensions.y; k++)
					{
						if (ClearForFloorObject(dimensions.x, dimensions.y, j, k))
						{
							list.Add(new IntVector2(j, k));
						}
					}
				}
			}
			if (list.Count <= 0)
			{
				return;
			}
			IntVector2 intVector = list[BraveRandom.GenerationRandomRange(0, list.Count)];
			RoomHandler absoluteRoom = intVector.ToVector2().GetAbsoluteRoom();
			GameObject gameObject = prefabPlaceable.InstantiateObject(absoluteRoom, intVector - absoluteRoom.area.basePosition);
			gameObject.transform.position = gameObject.transform.position + offset.ToVector3ZUp();
			IPlayerInteractable[] interfacesInChildren = gameObject.GetInterfacesInChildren<IPlayerInteractable>();
			IPlayerInteractable[] array = interfacesInChildren;
			foreach (IPlayerInteractable ixable in array)
			{
				absoluteRoom.RegisterInteractable(ixable);
			}
			for (int m = 0; m < dimensions.x; m++)
			{
				for (int n = 0; n < dimensions.y; n++)
				{
					IntVector2 intVector2 = intVector + new IntVector2(m, n);
					if (data.CheckInBoundsAndValid(intVector2))
					{
						data[intVector2].cellVisualData.floorTileOverridden = true;
					}
				}
			}
		}

		private void PlaceParadoxPortal()
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
			{
				DungeonPlaceableBehaviour component = ((GameObject)BraveResources.Load("Global Prefabs/VFX_ParadoxPortal")).GetComponent<DungeonPlaceableBehaviour>();
				PlaceFloorObjectInternal(component, new IntVector2(4, 4), new Vector2(2f, 2f));
			}
		}

		private void PlaceRatGrate()
		{
			DungeonPlaceableBehaviour component = RatTrapdoor.GetComponent<DungeonPlaceableBehaviour>();
			PlaceFloorObjectInternal(component, new IntVector2(4, 4), Vector2.zero);
		}

		private bool ClearForFloorObject(int dmx, int dmy, int bpx, int bpy)
		{
			int num = -1;
			for (int i = 0; i < dmx; i++)
			{
				for (int j = 0; j < dmy; j++)
				{
					IntVector2 intVector = new IntVector2(bpx + i, bpy + j);
					if (!data.CheckInBoundsAndValid(intVector))
					{
						return false;
					}
					CellData cellData = data[intVector];
					if (num == -1)
					{
						num = cellData.cellVisualData.roomVisualTypeIndex;
						if (num != 0 && num != 1)
						{
							return false;
						}
					}
					if (cellData.parentRoom == null || cellData.parentRoom.IsMaintenanceRoom() || cellData.type != CellType.FLOOR || cellData.isOccupied || !cellData.IsPassable || cellData.containsTrap || cellData.IsTrapZone)
					{
						return false;
					}
					if (cellData.cellVisualData.roomVisualTypeIndex != num || cellData.HasPitNeighbor(data) || cellData.PreventRewardSpawn || cellData.cellVisualData.isPattern || cellData.cellVisualData.IsPhantomCarpet)
					{
						return false;
					}
					if (cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Water || cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Carpet || cellData.cellVisualData.floorTileOverridden)
					{
						return false;
					}
					if (cellData.doesDamage || cellData.cellVisualData.preventFloorStamping || cellData.cellVisualData.hasStampedPath || cellData.forceDisallowGoop)
					{
						return false;
					}
				}
			}
			return true;
		}

		public void PlaceWallMimics(RoomHandler debugRoom = null)
		{
			if (GameManager.Instance.CurrentLevelOverrideState != 0 && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.RESOURCEFUL_RAT)
			{
				return;
			}
			int numWallMimicsForFloor = MetaInjectionData.GetNumWallMimicsForFloor(tileIndices.tilesetId);
			if (numWallMimicsForFloor <= 0)
			{
				return;
			}
			List<int> input = Enumerable.Range(0, data.rooms.Count).ToList();
			input = input.Shuffle();
			if (debugRoom != null)
			{
				input = new List<int>(new int[1] { data.rooms.IndexOf(debugRoom) });
			}
			List<Tuple<IntVector2, DungeonData.Direction>> list = new List<Tuple<IntVector2, DungeonData.Direction>>();
			int num = 0;
			List<AIActor> outList = new List<AIActor>();
			for (int i = 0; i < input.Count && num < numWallMimicsForFloor; i++)
			{
				RoomHandler roomHandler = data.rooms[input[i]];
				if (roomHandler.IsShop || roomHandler.IsMaintenanceRoom() || (roomHandler.area.IsProceduralRoom && roomHandler.area.proceduralCells != null) || (roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && (PlayerStats.GetTotalCurse() < 5 || BraveUtility.RandomBool())) || roomHandler.GetRoomName().StartsWith("DraGunRoom"))
				{
					continue;
				}
				if (roomHandler.connectedRooms != null)
				{
					for (int j = 0; j < roomHandler.connectedRooms.Count; j++)
					{
						if (roomHandler.connectedRooms[j] == null || roomHandler.connectedRooms[j].area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
						{
						}
					}
				}
				if (debugRoom == null)
				{
					bool flag = false;
					roomHandler.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref outList);
					for (int k = 0; k < outList.Count; k++)
					{
						AIActor aIActor = outList[k];
						if ((bool)aIActor && aIActor.EnemyGuid == GameManager.Instance.RewardManager.WallMimicChances.EnemyGuid)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				list.Clear();
				for (int l = -1; l <= roomHandler.area.dimensions.x; l++)
				{
					for (int m = -1; m <= roomHandler.area.dimensions.y; m++)
					{
						int num2 = roomHandler.area.basePosition.x + l;
						int num3 = roomHandler.area.basePosition.y + m;
						if (!data.isWall(num2, num3))
						{
							continue;
						}
						int num4 = 0;
						if (data.isWall(num2 - 1, num3) && data.isWall(num2 - 1, num3 - 1) && data.isWall(num2 - 1, num3 - 2) && data.isWall(num2, num3) && data.isWall(num2, num3 - 1) && data.isWall(num2, num3 - 2) && data.isPlainEmptyCell(num2, num3 + 1) && data.isWall(num2 + 1, num3) && data.isWall(num2 + 1, num3 - 1) && data.isWall(num2 + 1, num3 - 2) && data.isPlainEmptyCell(num2 + 1, num3 + 1) && data.isWall(num2 + 2, num3) && data.isWall(num2 + 2, num3 - 1) && data.isWall(num2 + 2, num3 - 2))
						{
							list.Add(Tuple.Create(new IntVector2(num2, num3), DungeonData.Direction.NORTH));
							num4++;
						}
						else if (data.isWall(num2 - 1, num3) && data.isWall(num2 - 1, num3 + 1) && data.isWall(num2 - 1, num3 + 2) && data.isWall(num2, num3) && data.isWall(num2, num3 + 1) && data.isWall(num2, num3 + 2) && data.isPlainEmptyCell(num2, num3 - 1) && data.isWall(num2 + 1, num3) && data.isWall(num2 + 1, num3 + 1) && data.isWall(num2 + 1, num3 + 2) && data.isPlainEmptyCell(num2 + 1, num3 - 1) && data.isWall(num2 + 2, num3) && data.isWall(num2 + 2, num3 + 1) && data.isWall(num2 + 2, num3 + 2))
						{
							list.Add(Tuple.Create(new IntVector2(num2, num3), DungeonData.Direction.SOUTH));
							num4++;
						}
						else if (data.isWall(num2, num3 + 2) && data.isWall(num2, num3 + 1) && data.isWall(num2, num3 - 1) && data.isWall(num2, num3 - 2) && data.isWall(num2 - 1, num3) && data.isPlainEmptyCell(num2 + 1, num3) && data.isPlainEmptyCell(num2 + 1, num3 - 1))
						{
							list.Add(Tuple.Create(new IntVector2(num2, num3), DungeonData.Direction.EAST));
							num4++;
						}
						else if (data.isWall(num2, num3 + 2) && data.isWall(num2, num3 + 1) && data.isWall(num2, num3 - 1) && data.isWall(num2, num3 - 2) && data.isWall(num2 + 1, num3) && data.isPlainEmptyCell(num2 - 1, num3) && data.isPlainEmptyCell(num2 - 1, num3 - 1))
						{
							list.Add(Tuple.Create(new IntVector2(num2 - 1, num3), DungeonData.Direction.WEST));
							num4++;
						}
						if (num4 <= 0)
						{
							continue;
						}
						bool flag2 = true;
						for (int n = -5; n <= 5; n++)
						{
							if (!flag2)
							{
								break;
							}
							for (int num5 = -5; num5 <= 5; num5++)
							{
								if (!flag2)
								{
									break;
								}
								int x = num2 + n;
								int y = num3 + num5;
								if (data.CheckInBoundsAndValid(x, y))
								{
									CellData cellData = data[x, y];
									if (cellData != null && (cellData.type == CellType.PIT || cellData.diagonalWallType != 0))
									{
										flag2 = false;
									}
								}
							}
						}
						if (!flag2)
						{
							while (num4 > 0)
							{
								list.RemoveAt(list.Count - 1);
								num4--;
							}
						}
					}
				}
				if (debugRoom == null && list.Count > 0)
				{
					Tuple<IntVector2, DungeonData.Direction> tuple = BraveUtility.RandomElement(list);
					IntVector2 first = tuple.First;
					DungeonData.Direction second = tuple.Second;
					if (second != DungeonData.Direction.WEST)
					{
						roomHandler.RuntimeStampCellComplex(first.x, first.y, CellType.FLOOR, DiagonalWallType.NONE);
					}
					if (second != DungeonData.Direction.EAST)
					{
						roomHandler.RuntimeStampCellComplex(first.x + 1, first.y, CellType.FLOOR, DiagonalWallType.NONE);
					}
					AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(GameManager.Instance.RewardManager.WallMimicChances.EnemyGuid);
					AIActor.Spawn(orLoadByGuid, first, roomHandler, true);
					num++;
				}
			}
			if (num > 0)
			{
				PhysicsEngine.Instance.ClearAllCachedTiles();
			}
		}

		public void FloorCleared()
		{
			if (GameManager.Instance.CurrentLevelOverrideState != 0)
			{
				return;
			}
			switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
			{
			case GlobalDungeonData.ValidTilesets.CASTLEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_CASTLE, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.GUNGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_GUNGEON, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.MINEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_MINES, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_CATACOMBS, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.FORGEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_FORGE, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.HELLGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_BULLET_HELL, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.SEWERGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_SEWERS, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_CATHEDRAL, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.WESTGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_WEST, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.SPACEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_FUTURE, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.OFFICEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_NAKATOMI, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.JUNGLEGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_JUNGLE, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.BELLYGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_BELLY, 1f);
				break;
			case GlobalDungeonData.ValidTilesets.PHOBOSGEON:
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_CLEARED_PHOBOS, 1f);
				break;
			}
			if (ChallengeManager.CHALLENGE_MODE_ACTIVE && tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.DAISUKE_CHALLENGE_COMPLETE, true);
				GameStatsManager.Instance.SetFlag(GungeonFlags.DAISUKE_CHALLENGE_ITEM_UNLOCK, true);
				if (ChallengeManager.Instance.ChallengeMode == ChallengeModeType.ChallengeMegaMode)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.DAISUKE_MEGA_CHALLENGE_COMPLETE, true);
				}
			}
			if (GameStatsManager.Instance.GetFlag(GungeonFlags.SHOP_HAS_MET_BEETLE))
			{
				switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
				{
				case GlobalDungeonData.ValidTilesets.CASTLEGEON:
					GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 0.2f;
					break;
				case GlobalDungeonData.ValidTilesets.GUNGEON:
					GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 0.4f;
					break;
				case GlobalDungeonData.ValidTilesets.MINEGEON:
					GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 0.6f;
					break;
				case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
					GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 0.8f;
					break;
				case GlobalDungeonData.ValidTilesets.FORGEGEON:
					GameStatsManager.Instance.AccumulatedBeetleMerchantChance = 1f;
					break;
				}
			}
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FORGEGEON)
			{
				return;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if ((bool)playerController)
				{
					if (!playerController.HasFiredNonStartingGun)
					{
						GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_STARTING_GUN, true);
					}
					if (playerController.CharacterUsesRandomGuns)
					{
						GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_GAME_WITH_ENCHANTED_GUN);
					}
					if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
					{
						GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COMPLETE_GAME_WITH_CHALLENGE_MODE);
					}
				}
			}
		}

		private void GeneratePlayerIfNecessary(MidGameSaveData midgameSave)
		{
			bool isUsingAlternateCostume = false;
			bool temporaryEeveeSafeNoShader = false;
			if (ForceRegenerationOfCharacters)
			{
				if ((bool)GameManager.Instance.PrimaryPlayer)
				{
					isUsingAlternateCostume = GameManager.Instance.PrimaryPlayer.IsUsingAlternateCostume;
				}
				if ((bool)GameManager.Instance.PrimaryPlayer)
				{
					temporaryEeveeSafeNoShader = GameManager.Instance.PrimaryPlayer.IsTemporaryEeveeForUnlock;
				}
				GameManager.Instance.ClearPlayers();
			}
			PlayerController playerController = GameManager.Instance.PrimaryPlayer;
			if (!playerController || ForceRegenerationOfCharacters)
			{
				if (midgameSave != null)
				{
					GameManager.PlayerPrefabForNewGame = midgameSave.GetPlayerOnePrefab();
				}
				if (GameManager.PlayerPrefabForNewGame == null)
				{
					if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
					{
						return;
					}
					BraveUtility.Log("Dungeon generation complete with no Player! Creating placeholder...", Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);
					GameObject original = defaultPlayerPrefab;
					GameObject gameObject = UnityEngine.Object.Instantiate(original, Vector3.zero, Quaternion.identity);
					gameObject.SetActive(true);
					playerController = gameObject.GetComponent<PlayerController>();
					if (playerController is PlayerSpaceshipController)
					{
						playerController.IsUsingAlternateCostume = isUsingAlternateCostume;
						playerController.SetTemporaryEeveeSafeNoShader(temporaryEeveeSafeNoShader);
					}
				}
				else
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate(GameManager.PlayerPrefabForNewGame, Vector3.zero, Quaternion.identity);
					GameManager.PlayerPrefabForNewGame = null;
					gameObject2.SetActive(true);
					playerController = gameObject2.GetComponent<PlayerController>();
				}
				if (GameManager.ForceQuickRestartAlternateCostumeP1)
				{
					playerController.SwapToAlternateCostume();
					GameManager.ForceQuickRestartAlternateCostumeP1 = false;
				}
				if (GameManager.ForceQuickRestartAlternateGunP1)
				{
					playerController.UsingAlternateStartingGuns = true;
					GameManager.ForceQuickRestartAlternateGunP1 = false;
				}
				GameManager.Instance.RefreshAllPlayers();
				if (StripPlayerOnArrival)
				{
					playerController.startingGunIds = new List<int>();
					playerController.startingAlternateGunIds = new List<int>();
					playerController.startingActiveItemIds.Clear();
					playerController.startingPassiveItemIds.Clear();
				}
			}
			else if (StripPlayerOnArrival)
			{
				playerController.inventory.DestroyAllGuns();
				playerController.RemoveAllActiveItems();
				playerController.RemoveAllPassiveItems();
			}
			playerController.PlayerIDX = 0;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				if (GameManager.Instance.AllPlayers.Length < 2 || ForceRegenerationOfCharacters)
				{
					GameObject original2 = ((!(GameManager.CoopPlayerPrefabForNewGame == null)) ? GameManager.CoopPlayerPrefabForNewGame : (ResourceCache.Acquire("PlayerCoopCultist") as GameObject));
					if (ForceRegenerationOfCharacters)
					{
						original2 = ResourceCache.Acquire("PlayerCoopCultist") as GameObject;
					}
					if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST && GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Pilot && !GameManager.IsCoopPast)
					{
						original2 = BraveResources.Load("PlayerCoopShip") as GameObject;
					}
					GameObject gameObject3 = UnityEngine.Object.Instantiate(original2, Vector3.zero, Quaternion.identity);
					GameManager.CoopPlayerPrefabForNewGame = null;
					gameObject3.SetActive(true);
					PlayerController component = gameObject3.GetComponent<PlayerController>();
					component.ActorName = "Player ID 1";
					component.PlayerIDX = 1;
					if (GameManager.ForceQuickRestartAlternateCostumeP2)
					{
						component.SwapToAlternateCostume();
						GameManager.ForceQuickRestartAlternateCostumeP2 = false;
					}
					if (GameManager.ForceQuickRestartAlternateGunP2)
					{
						component.UsingAlternateStartingGuns = true;
						GameManager.ForceQuickRestartAlternateGunP2 = false;
					}
					if (StripPlayerOnArrival)
					{
						component.startingGunIds = new List<int>();
						component.startingAlternateGunIds = new List<int>();
						component.startingActiveItemIds.Clear();
						component.startingPassiveItemIds.Clear();
					}
					GameManager.Instance.RefreshAllPlayers();
				}
				else if (StripPlayerOnArrival)
				{
					GameManager.Instance.SecondaryPlayer.inventory.DestroyAllGuns();
					GameManager.Instance.SecondaryPlayer.RemoveAllActiveItems();
					GameManager.Instance.SecondaryPlayer.RemoveAllPassiveItems();
				}
			}
			if (!GameManager.Instance.InTutorial && (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.PrimaryPlayer.characterIdentity != PlayableCharacters.Convict))
			{
				return;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].healthHaver.IsAlive)
				{
					GameManager.Instance.AllPlayers[i].sprite.gameObject.SetLayerRecursively(LayerMask.NameToLayer("ShadowCaster"));
				}
			}
		}

		public void DarkSoulsReset(PlayerController targetPlayer, bool dropItems = true, int cursedHealthMaximum = -1)
		{
			StartCoroutine(HandleDarkSoulsReset_CR(targetPlayer, dropItems, cursedHealthMaximum));
		}

		private IEnumerator HandleDarkSoulsReset_CR(PlayerController p, bool dropItems, int cursedHealthMaximum)
		{
			GameManager.Instance.PauseRaw(true);
			float elapsed = 0f;
			float transitionTime = 0.5f;
			Pixelator.Instance.FadeToBlack(transitionTime);
			while (elapsed < transitionTime)
			{
				elapsed += GameManager.INVARIANT_DELTA_TIME;
				yield return null;
			}
			if (dropItems)
			{
				p.DropPileOfSouls();
				p.HandleDarkSoulsHollowTransition();
			}
			RoomHandler targetRoom = data.Entrance;
			if (ExtraLifeItem.LastActivatedBonfire != null)
			{
				targetRoom = ExtraLifeItem.LastActivatedBonfire.transform.position.GetAbsoluteRoom();
			}
			IntVector2 availableCell = targetRoom.GetCenteredVisibleClearSpot(3, 3);
			Vector3 playerPosition = new Vector3((float)availableCell.x + 0.5f, (float)availableCell.y + 0.5f, -0.1f);
			p.transform.position = playerPosition;
			p.Reinitialize();
			p.ForceChangeRoom(targetRoom);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameManager.Instance.GetOtherPlayer(p).ReuniteWithOtherPlayer(p);
				GameManager.Instance.GetOtherPlayer(p).ForceChangeRoom(targetRoom);
			}
			if (cursedHealthMaximum > 0)
			{
				p.healthHaver.CursedMaximum = cursedHealthMaximum;
				p.IsDarkSoulsHollow = true;
			}
			if (p.characterIdentity == PlayableCharacters.Robot)
			{
				p.healthHaver.Armor = 2f;
			}
			for (int i = 0; i < data.rooms.Count; i++)
			{
				data.rooms[i].ResetPredefinedRoomLikeDarkSouls();
			}
			GameUIRoot.Instance.bossController.DisableBossHealth();
			GameUIRoot.Instance.bossController2.DisableBossHealth();
			GameUIRoot.Instance.bossControllerSide.DisableBossHealth();
			GameManager.Instance.MainCameraController.ForceToPlayerPosition(p);
			GameManager.Instance.ForceUnpause();
			GameManager.Instance.PreventPausing = false;
			p.CurrentInputState = PlayerInputState.AllInput;
			p.DoInitialFallSpawn(0f);
			Pixelator.Instance.FadeToBlack(transitionTime, true);
		}

		private void PlacePlayerInRoom(tk2dTileMap map, RoomHandler startRoom)
		{
			PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
			if (allPlayers.Length == 0)
			{
				return;
			}
			int num = ((allPlayers.Length < 2) ? 1 : allPlayers.Length);
			for (int i = 0; i < num; i++)
			{
				PlayerController playerController = ((allPlayers.Length >= 2) ? allPlayers[i] : GameManager.Instance.PrimaryPlayer);
				EntranceController entranceController = UnityEngine.Object.FindObjectOfType<EntranceController>();
				ElevatorArrivalController elevatorArrivalController = UnityEngine.Object.FindObjectOfType<ElevatorArrivalController>();
				Vector2 zero = Vector2.zero;
				float num2 = 0.25f;
				if (GameManager.IsReturningToFoyerWithPlayer)
				{
					zero = GameObject.Find("ReturnToFoyerPoint").transform.position.XY();
					zero += Vector2.right * i;
					playerController.transform.position = zero.ToVector3ZUp(-0.1f);
					playerController.Reinitialize();
					continue;
				}
				if (elevatorArrivalController != null)
				{
					zero = elevatorArrivalController.spawnTransform.position.XY();
					num2 = 1f;
					elevatorArrivalController.DoArrival(playerController, num2);
					num2 += 0.4f;
				}
				else
				{
					if (entranceController != null)
					{
						zero = entranceController.spawnTransform.position.XY();
						zero += Vector2.right * i;
						playerController.transform.position = new Vector3(map.transform.position.x + zero.x - 0.5f, map.transform.position.y + zero.y, -0.1f);
						playerController.Reinitialize();
						num2 += 0.4f;
						playerController.DoSpinfallSpawn(num2);
						continue;
					}
					if (i == 1 && GameObject.Find("SecondaryPlayerSpawnPoint") != null)
					{
						zero = GameObject.Find("SecondaryPlayerSpawnPoint").transform.position.XY();
						zero += Vector2.right * i;
						playerController.transform.position = zero.ToVector3ZUp(-0.1f);
						playerController.Reinitialize();
						continue;
					}
					if (GameObject.Find("PlayerSpawnPoint") != null)
					{
						zero = GameObject.Find("PlayerSpawnPoint").transform.position.XY();
						zero += Vector2.right * i;
						playerController.transform.position = zero.ToVector3ZUp(-0.1f);
						playerController.Reinitialize();
						continue;
					}
					zero = startRoom.GetCenterCell().ToVector2();
					if (data[zero.ToIntVector2()].type == CellType.WALL || data[zero.ToIntVector2()].type == CellType.PIT)
					{
						zero = startRoom.Epicenter.ToVector2();
					}
					zero += Vector2.right * i;
				}
				Vector3 position = new Vector3(map.transform.position.x + zero.x + 0.5f, map.transform.position.y + zero.y + 0.5f, -0.1f);
				playerController.transform.position = position;
				playerController.Reinitialize();
				playerController.DoInitialFallSpawn(num2);
			}
			GameManager.IsReturningToFoyerWithPlayer = false;
			GameManager.Instance.MainCameraController.ForceToPlayerPosition(GameManager.Instance.PrimaryPlayer);
		}

		public bool CellExists(IntVector2 pos)
		{
			return pos.x >= 0 && pos.x < Width && pos.y >= 0 && pos.y < Height;
		}

		public bool CellExists(int x, int y)
		{
			return x >= 0 && x < Width && y >= 0 && y < Height;
		}

		public bool CellExists(Vector2 pos)
		{
			int num = (int)pos.x;
			int num2 = (int)pos.y;
			return num >= 0 && num < Width && num2 >= 0 && num2 < Height;
		}

		public bool CellIsNearPit(Vector3 position)
		{
			IntVector2 key = position.IntXY(VectorConversions.Floor);
			CellData cellData = data[key];
			if (cellData == null)
			{
				return false;
			}
			return cellData.type == CellType.PIT || cellData.HasPitNeighbor(data);
		}

		public bool CellIsPit(Vector3 position)
		{
			IntVector2 key = position.IntXY(VectorConversions.Floor);
			CellData cellData = data[key];
			return cellData.type == CellType.PIT;
		}

		public bool CellSupportsFalling(Vector3 position)
		{
			IntVector2 intVector = position.IntXY(VectorConversions.Floor);
			if (!data.CheckInBounds(intVector))
			{
				return false;
			}
			CellData cellData = data[intVector];
			if (cellData == null)
			{
				return false;
			}
			return cellData.type == CellType.PIT && !cellData.fallingPrevented;
		}

		public List<SpeculativeRigidbody> GetPlatformsAt(Vector3 position)
		{
			IntVector2 key = position.IntXY(VectorConversions.Floor);
			CellData cellData = data[key];
			return cellData.platforms;
		}

		public bool IsPixelOnPlatform(Vector3 position, out SpeculativeRigidbody platform)
		{
			return IsPixelOnPlatform(PhysicsEngine.UnitToPixel(position.XY()), out platform);
		}

		public bool IsPixelOnPlatform(IntVector2 pixel, out SpeculativeRigidbody platform)
		{
			platform = null;
			IntVector2 key = PhysicsEngine.PixelToUnitMidpoint(pixel).ToIntVector2(VectorConversions.Floor);
			CellData cellData = data[key];
			if (cellData.platforms != null)
			{
				for (int i = 0; i < cellData.platforms.Count; i++)
				{
					if (cellData.platforms[i].PrimaryPixelCollider.ContainsPixel(pixel))
					{
						platform = cellData.platforms[i];
						return true;
					}
				}
			}
			return false;
		}

		public bool PositionInCustomPitSRB(Vector3 position)
		{
			if (DebrisObject.SRB_Pits != null && DebrisObject.SRB_Pits.Count > 0)
			{
				for (int i = 0; i < DebrisObject.SRB_Pits.Count; i++)
				{
					if (DebrisObject.SRB_Pits[i].ContainsPoint(position.XY(), int.MaxValue, true))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ShouldReallyFall(Vector3 position)
		{
			bool flag = !CellSupportsFalling(position);
			if (PositionInCustomPitSRB(position))
			{
				flag = false;
			}
			if (flag)
			{
				return false;
			}
			SpeculativeRigidbody platform;
			return !IsPixelOnPlatform(position, out platform);
		}

		public void DoSplashDustupAtPosition(Vector2 bottomCenter)
		{
			DustUpVFX dustUpVFX = dungeonDustups;
			Color clear = Color.clear;
			GameObject gameObject = SpawnManager.SpawnVFX(dustUpVFX.waterDustup, bottomCenter, Quaternion.identity);
			if ((bool)gameObject)
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				if ((bool)component)
				{
					gameObject.GetComponent<tk2dBaseSprite>().OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
					component.material.SetColor("_OverrideColor", clear);
				}
			}
			if (dustUpVFX.additionalWaterDustup != null)
			{
				SpawnManager.SpawnVFX(dustUpVFX.additionalWaterDustup, bottomCenter, Quaternion.identity, true);
			}
		}

		public IntVector2 RandomCellInRandomRoom()
		{
			RoomHandler roomHandler = data.rooms[UnityEngine.Random.Range(0, data.rooms.Count)];
			return roomHandler.GetRandomAvailableCellDumb();
		}

		public RoomHandler GetRoomFromPosition(IntVector2 pos)
		{
			return data.GetAbsoluteRoomFromPosition(pos);
		}

		public CellVisualData.CellFloorType GetFloorTypeFromPosition(Vector2 pos)
		{
			for (int i = 0; i < StaticReferenceManager.AllGoops.Count; i++)
			{
				if (StaticReferenceManager.AllGoops[i].IsPositionInGoop(pos))
				{
					return (!StaticReferenceManager.AllGoops[i].IsPositionFrozen(pos)) ? CellVisualData.CellFloorType.Water : CellVisualData.CellFloorType.Ice;
				}
			}
			return data.GetFloorTypeFromPosition(pos.ToIntVector2(VectorConversions.Floor));
		}

		public IntVector2 RandomCellInArea(CellArea ca)
		{
			int num = UnityEngine.Random.Range(0, ca.dimensions.x);
			int num2 = UnityEngine.Random.Range(0, ca.dimensions.y);
			return new IntVector2(ca.basePosition.x + num, ca.basePosition.y + num2);
		}

		public void NotifyAllRoomsVisited()
		{
			if (!m_allRoomsVisited)
			{
				m_allRoomsVisited = true;
				if (OnAllRoomsVisited != null)
				{
					OnAllRoomsVisited();
				}
			}
		}

		public TertiaryBossRewardSet GetTertiaryRewardSet()
		{
			List<TertiaryBossRewardSet> list = null;
			list = ((!UsesOverrideTertiaryBossSets || OverrideTertiaryRewardSets.Count <= 0) ? GameManager.Instance.RewardManager.CurrentRewardData.TertiaryBossRewardSets : OverrideTertiaryRewardSets);
			float num = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				num += list[i].weight;
			}
			float num2 = UnityEngine.Random.value * num;
			float num3 = 0f;
			for (int j = 0; j < list.Count; j++)
			{
				num3 += list[j].weight;
				if (num3 >= num2)
				{
					return list[j];
				}
			}
			return list[list.Count - 1];
		}

		private void Update()
		{
			if (!m_ambientVFXProcessingActive)
			{
				StartCoroutine(HandleAmbientPitVFX());
				if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON)
				{
					StartCoroutine(HandleAmbientChannelVFX());
				}
			}
			if (!m_musicIsPlaying && Time.timeScale > 0f)
			{
				if (Foyer.DoMainMenu && SceneManager.GetSceneByName("tt_foyer").isLoaded)
				{
					m_musicIsPlaying = true;
				}
				else if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
				{
					m_musicIsPlaying = true;
					GameManager.Instance.DungeonMusicController.ResetForNewFloor(this);
				}
			}
			if (ExplosionBulletDeletionMultiplier < 1f)
			{
				if (ExplosionBulletDeletionMultiplier <= 0f)
				{
					IsExplosionBulletDeletionRecovering = true;
				}
				ExplosionBulletDeletionMultiplier = Mathf.Clamp01(ExplosionBulletDeletionMultiplier + BraveTime.DeltaTime / 3f);
			}
			else
			{
				IsExplosionBulletDeletionRecovering = false;
			}
		}

		public float GetNewPlayerSpeedMultiplier()
		{
			if ((bool)GameManager.Instance.Dungeon && GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON)
			{
				return 1f;
			}
			if (m_newPlayerMultiplier > 0f)
			{
				return m_newPlayerMultiplier;
			}
			float playerStatValue = GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_ATTEMPTS);
			float num = Mathf.Clamp01(0.2f - 0.025f * (playerStatValue - 1f));
			m_newPlayerMultiplier = 1f - num;
			return m_newPlayerMultiplier;
		}

		public RoomHandler RuntimeDuplicateChunk(IntVector2 basePosition, IntVector2 dimensions, int tilemapExpansion, RoomHandler sourceRoom = null, bool ignoreOtherRoomCells = false)
		{
			int num = tilemapExpansion + 3;
			IntVector2 intVector = new IntVector2(data.Width + num, num);
			int newWidth = data.Width + num * 2 + dimensions.x;
			int newHeight = Mathf.Max(data.Height, dimensions.y + num * 2);
			CellData[][] array = BraveUtility.MultidimensionalArrayResize(data.cellData, data.Width, data.Height, newWidth, newHeight);
			CellArea cellArea = new CellArea(intVector, dimensions);
			cellArea.IsProceduralRoom = true;
			data.cellData = array;
			data.ClearCachedCellData();
			RoomHandler roomHandler = new RoomHandler(cellArea);
			GameObject gameObject = GameObject.Find("_Rooms");
			Transform transform = new GameObject("Room_ChunkDuplicate").transform;
			transform.parent = gameObject.transform;
			roomHandler.hierarchyParent = transform;
			for (int i = -num; i < dimensions.x + num; i++)
			{
				for (int j = -num; j < dimensions.y + num; j++)
				{
					IntVector2 intVector2 = basePosition + new IntVector2(i, j);
					IntVector2 p = new IntVector2(i, j) + intVector;
					CellData cellData = ((!data.CheckInBoundsAndValid(intVector2)) ? null : data[intVector2]);
					CellData cellData2 = new CellData(p);
					if (cellData != null && sourceRoom != null && cellData.nearestRoom != sourceRoom)
					{
						cellData2.cellVisualData.roomVisualTypeIndex = sourceRoom.RoomVisualSubtype;
						cellData = null;
					}
					if (cellData != null && cellData.isExitCell && ignoreOtherRoomCells)
					{
						cellData2.cellVisualData.roomVisualTypeIndex = sourceRoom.RoomVisualSubtype;
						cellData = null;
					}
					cellData2.positionInTilemap = cellData2.positionInTilemap - intVector + new IntVector2(tilemapExpansion, tilemapExpansion);
					cellData2.parentArea = cellArea;
					cellData2.parentRoom = roomHandler;
					cellData2.nearestRoom = roomHandler;
					cellData2.occlusionData.overrideOcclusion = true;
					array[p.x][p.y] = cellData2;
					BraveUtility.DrawDebugSquare(p.ToVector2(), Color.yellow, 1000f);
					CellType type = ((cellData == null) ? CellType.WALL : cellData.type);
					roomHandler.RuntimeStampCellComplex(p.x, p.y, type, DiagonalWallType.NONE);
					if (cellData != null)
					{
						cellData2.distanceFromNearestRoom = cellData.distanceFromNearestRoom;
						cellData2.cellVisualData.CopyFrom(cellData.cellVisualData);
						if (cellData.cellVisualData.containsLight)
						{
							data.ReplicateLighting(cellData, cellData2);
						}
					}
				}
			}
			data.rooms.Add(roomHandler);
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("RuntimeTileMap"));
			tk2dTileMap component = gameObject2.GetComponent<tk2dTileMap>();
			component.Editor__SpriteCollection = tileIndices.dungeonCollection;
			TK2DDungeonAssembler.RuntimeResizeTileMap(component, dimensions.x + tilemapExpansion * 2, dimensions.y + tilemapExpansion * 2, m_tilemap.partitionSizeX, dimensions.y + tilemapExpansion * 2);
			for (int k = -tilemapExpansion; k < dimensions.x + tilemapExpansion; k++)
			{
				for (int l = -tilemapExpansion; l < dimensions.y + tilemapExpansion; l++)
				{
					IntVector2 intVector3 = basePosition + new IntVector2(k, l);
					IntVector2 intVector4 = new IntVector2(k, l) + intVector;
					bool flag = false;
					CellData cellData3 = ((!data.CheckInBoundsAndValid(intVector3)) ? null : data[intVector3]);
					if (ignoreOtherRoomCells && cellData3 != null)
					{
						bool flag2 = cellData3.isExitCell;
						if (!flag2 && sourceRoom != null && cellData3.parentRoom != sourceRoom)
						{
							flag2 = true;
						}
						if (!flag2 && cellData3.IsAnyFaceWall() && data.CheckInBoundsAndValid(cellData3.position + new IntVector2(0, -2)) && data[cellData3.position + new IntVector2(0, -2)].isExitCell)
						{
							flag2 = true;
						}
						if (!flag2 && cellData3.type == CellType.WALL && data.CheckInBoundsAndValid(cellData3.position + new IntVector2(0, -3)) && data[cellData3.position + new IntVector2(0, -3)].isExitCell)
						{
							flag2 = true;
						}
						if (!flag2 && cellData3.type == CellType.FLOOR && data.CheckInBoundsAndValid(cellData3.position + new IntVector2(0, -1)) && (data[cellData3.position + new IntVector2(0, -1)].isExitCell || data[cellData3.position + new IntVector2(0, -1)].GetExitNeighbor() != null))
						{
							flag2 = true;
						}
						if (!flag2 && (cellData3.IsAnyFaceWall() || cellData3.type == CellType.WALL) && cellData3.GetExitNeighbor() != null)
						{
							flag2 = true;
						}
						if (flag2)
						{
							BraveUtility.DrawDebugSquare(intVector4.ToVector2() + new Vector2(0.3f, 0.3f), intVector4.ToVector2() + new Vector2(0.7f, 0.7f), Color.cyan, 1000f);
							assembler.BuildTileIndicesForCell(this, component, intVector.x + k, intVector.y + l);
							flag = true;
						}
					}
					if (!flag && intVector3.x >= 0 && intVector3.y >= 0)
					{
						for (int m = 0; m < component.Layers.Length; m++)
						{
							int tile = MainTilemap.Layers[m].GetTile(intVector3.x, intVector3.y);
							component.Layers[m].SetTile(k + tilemapExpansion, l + tilemapExpansion, tile);
						}
					}
				}
			}
			RenderMeshBuilder.CurrentCellXOffset = intVector.x - tilemapExpansion;
			RenderMeshBuilder.CurrentCellYOffset = intVector.y - tilemapExpansion;
			component.Build();
			RenderMeshBuilder.CurrentCellXOffset = 0;
			RenderMeshBuilder.CurrentCellYOffset = 0;
			component.renderData.transform.position = new Vector3(intVector.x - tilemapExpansion, intVector.y - tilemapExpansion, intVector.y - tilemapExpansion);
			roomHandler.OverrideTilemap = component;
			roomHandler.PostGenerationCleanup();
			DeadlyDeadlyGoopManager.ReinitializeData();
			return roomHandler;
		}

		private void ConnectClusteredRuntimeRooms(RoomHandler first, RoomHandler second, PrototypeDungeonRoom firstPrototype, PrototypeDungeonRoom secondPrototype, int firstRoomExitIndex, int secondRoomExitIndex)
		{
			first.area.instanceUsedExits.Add(firstPrototype.exitData.exits[firstRoomExitIndex]);
			RuntimeRoomExitData runtimeRoomExitData = new RuntimeRoomExitData(firstPrototype.exitData.exits[firstRoomExitIndex]);
			first.area.exitToLocalDataMap.Add(firstPrototype.exitData.exits[firstRoomExitIndex], runtimeRoomExitData);
			second.area.instanceUsedExits.Add(secondPrototype.exitData.exits[secondRoomExitIndex]);
			RuntimeRoomExitData runtimeRoomExitData2 = new RuntimeRoomExitData(secondPrototype.exitData.exits[secondRoomExitIndex]);
			second.area.exitToLocalDataMap.Add(secondPrototype.exitData.exits[secondRoomExitIndex], runtimeRoomExitData2);
			first.connectedRooms.Add(second);
			first.connectedRoomsByExit.Add(firstPrototype.exitData.exits[firstRoomExitIndex], second);
			first.childRooms.Add(second);
			second.connectedRooms.Add(first);
			second.connectedRoomsByExit.Add(secondPrototype.exitData.exits[secondRoomExitIndex], first);
			second.parentRoom = first;
			runtimeRoomExitData.linkedExit = runtimeRoomExitData2;
			runtimeRoomExitData2.linkedExit = runtimeRoomExitData;
			runtimeRoomExitData.additionalExitLength = 3;
			runtimeRoomExitData2.additionalExitLength = 3;
		}

		public List<RoomHandler> AddRuntimeRoomCluster(List<PrototypeDungeonRoom> prototypes, List<IntVector2> basePositions, Action<RoomHandler> postProcessCellData = null, DungeonData.LightGenerationStyle lightStyle = DungeonData.LightGenerationStyle.FORCE_COLOR)
		{
			if (prototypes.Count != basePositions.Count)
			{
				Debug.LogError("Attempting to add a malformed room cluster at runtime!");
				return null;
			}
			List<RoomHandler> list = new List<RoomHandler>();
			int num = 6;
			int num2 = 3;
			IntVector2 intVector = new IntVector2(int.MaxValue, int.MaxValue);
			IntVector2 intVector2 = new IntVector2(int.MinValue, int.MinValue);
			for (int i = 0; i < prototypes.Count; i++)
			{
				intVector = IntVector2.Min(intVector, basePositions[i]);
				intVector2 = IntVector2.Max(intVector2, basePositions[i] + new IntVector2(prototypes[i].Width, prototypes[i].Height));
			}
			IntVector2 intVector3 = intVector2 - intVector;
			IntVector2 intVector4 = IntVector2.Min(IntVector2.Zero, -1 * intVector);
			intVector3 += intVector4;
			IntVector2 intVector5 = new IntVector2(data.Width + num, num);
			int newWidth = data.Width + num * 2 + intVector3.x;
			int newHeight = Mathf.Max(data.Height, intVector3.y + num * 2);
			CellData[][] array = BraveUtility.MultidimensionalArrayResize(data.cellData, data.Width, data.Height, newWidth, newHeight);
			data.cellData = array;
			data.ClearCachedCellData();
			for (int j = 0; j < prototypes.Count; j++)
			{
				IntVector2 d = new IntVector2(prototypes[j].Width, prototypes[j].Height);
				IntVector2 intVector6 = basePositions[j] + intVector4;
				IntVector2 intVector7 = intVector5 + intVector6;
				CellArea cellArea = new CellArea(intVector7, d);
				cellArea.prototypeRoom = prototypes[j];
				RoomHandler roomHandler = new RoomHandler(cellArea);
				for (int k = -num; k < d.x + num; k++)
				{
					for (int l = -num; l < d.y + num; l++)
					{
						IntVector2 p = new IntVector2(k, l) + intVector7;
						if ((k >= 0 && l >= 0 && k < d.x && l < d.y) || array[p.x][p.y] == null)
						{
							CellData cellData = new CellData(p);
							cellData.positionInTilemap = cellData.positionInTilemap - intVector5 + new IntVector2(num2, num2);
							cellData.parentArea = cellArea;
							cellData.parentRoom = roomHandler;
							cellData.nearestRoom = roomHandler;
							cellData.distanceFromNearestRoom = 0f;
							array[p.x][p.y] = cellData;
						}
					}
				}
				data.rooms.Add(roomHandler);
				list.Add(roomHandler);
			}
			for (int m = 1; m < list.Count; m++)
			{
				ConnectClusteredRuntimeRooms(list[m - 1], list[m], prototypes[m - 1], prototypes[m], (m != 1) ? 1 : 0, 0);
			}
			for (int n = 0; n < list.Count; n++)
			{
				RoomHandler roomHandler2 = list[n];
				roomHandler2.WriteRoomData(data);
				GameManager.Instance.Dungeon.data.GenerateLightsForRoom(GameManager.Instance.Dungeon.decoSettings, roomHandler2, GameObject.Find("_Lights").transform, lightStyle);
				if (postProcessCellData != null)
				{
					postProcessCellData(roomHandler2);
				}
				if (roomHandler2.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET)
				{
					roomHandler2.BuildSecretRoomCover();
				}
			}
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("RuntimeTileMap"));
			tk2dTileMap component = gameObject.GetComponent<tk2dTileMap>();
			string text = Guid.NewGuid().ToString();
			gameObject.name = "RuntimeTilemap_" + text;
			component.renderData.name = "RuntimeTilemap_" + text + " Render Data";
			component.Editor__SpriteCollection = tileIndices.dungeonCollection;
			TK2DDungeonAssembler.RuntimeResizeTileMap(component, intVector3.x + num2 * 2, intVector3.y + num2 * 2, m_tilemap.partitionSizeX, m_tilemap.partitionSizeY);
			for (int num3 = 0; num3 < prototypes.Count; num3++)
			{
				IntVector2 intVector8 = new IntVector2(prototypes[num3].Width, prototypes[num3].Height);
				IntVector2 intVector9 = basePositions[num3] + intVector4;
				IntVector2 intVector10 = intVector5 + intVector9;
				for (int num4 = -num2; num4 < intVector8.x + num2; num4++)
				{
					for (int num5 = -num2; num5 < intVector8.y + num2 + 2; num5++)
					{
						assembler.BuildTileIndicesForCell(this, component, intVector10.x + num4, intVector10.y + num5);
					}
				}
			}
			RenderMeshBuilder.CurrentCellXOffset = intVector5.x - num2;
			RenderMeshBuilder.CurrentCellYOffset = intVector5.y - num2;
			component.ForceBuild();
			RenderMeshBuilder.CurrentCellXOffset = 0;
			RenderMeshBuilder.CurrentCellYOffset = 0;
			component.renderData.transform.position = new Vector3(intVector5.x - num2, intVector5.y - num2, intVector5.y - num2);
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				list[num6].OverrideTilemap = component;
				for (int num7 = 0; num7 < list[num6].area.dimensions.x; num7++)
				{
					for (int num8 = 0; num8 < list[num6].area.dimensions.y + 2; num8++)
					{
						IntVector2 intVector11 = list[num6].area.basePosition + new IntVector2(num7, num8);
						if (data.CheckInBoundsAndValid(intVector11))
						{
							CellData currentCell = data[intVector11];
							TK2DInteriorDecorator.PlaceLightDecorationForCell(this, component, currentCell, intVector11);
						}
					}
				}
				Pathfinder.Instance.InitializeRegion(data, list[num6].area.basePosition + new IntVector2(-3, -3), list[num6].area.dimensions + new IntVector2(3, 3));
				list[num6].PostGenerationCleanup();
			}
			DeadlyDeadlyGoopManager.ReinitializeData();
			return list;
		}

		public RoomHandler AddRuntimeRoom(PrototypeDungeonRoom prototype, Action<RoomHandler> postProcessCellData = null, DungeonData.LightGenerationStyle lightStyle = DungeonData.LightGenerationStyle.FORCE_COLOR)
		{
			int num = 6;
			int num2 = 3;
			IntVector2 d = new IntVector2(prototype.Width, prototype.Height);
			IntVector2 intVector = new IntVector2(data.Width + num, num);
			int newWidth = data.Width + num * 2 + d.x;
			int newHeight = Mathf.Max(data.Height, d.y + num * 2);
			CellData[][] array = BraveUtility.MultidimensionalArrayResize(data.cellData, data.Width, data.Height, newWidth, newHeight);
			CellArea cellArea = new CellArea(intVector, d);
			cellArea.prototypeRoom = prototype;
			data.cellData = array;
			data.ClearCachedCellData();
			RoomHandler roomHandler = new RoomHandler(cellArea);
			for (int i = -num; i < d.x + num; i++)
			{
				for (int j = -num; j < d.y + num; j++)
				{
					IntVector2 p = new IntVector2(i, j) + intVector;
					CellData cellData = new CellData(p);
					cellData.positionInTilemap = cellData.positionInTilemap - intVector + new IntVector2(num2, num2);
					cellData.parentArea = cellArea;
					cellData.parentRoom = roomHandler;
					cellData.nearestRoom = roomHandler;
					cellData.distanceFromNearestRoom = 0f;
					array[p.x][p.y] = cellData;
				}
			}
			roomHandler.WriteRoomData(data);
			for (int k = -num; k < d.x + num; k++)
			{
				for (int l = -num; l < d.y + num; l++)
				{
					IntVector2 intVector2 = new IntVector2(k, l) + intVector;
					array[intVector2.x][intVector2.y].breakable = true;
				}
			}
			data.rooms.Add(roomHandler);
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("RuntimeTileMap"));
			tk2dTileMap component = gameObject.GetComponent<tk2dTileMap>();
			component.Editor__SpriteCollection = tileIndices.dungeonCollection;
			GameManager.Instance.Dungeon.data.GenerateLightsForRoom(GameManager.Instance.Dungeon.decoSettings, roomHandler, GameObject.Find("_Lights").transform, lightStyle);
			if (postProcessCellData != null)
			{
				postProcessCellData(roomHandler);
			}
			TK2DDungeonAssembler.RuntimeResizeTileMap(component, d.x + num2 * 2, d.y + num2 * 2, m_tilemap.partitionSizeX, m_tilemap.partitionSizeY);
			for (int m = -num2; m < d.x + num2; m++)
			{
				for (int n = -num2; n < d.y + num2; n++)
				{
					assembler.BuildTileIndicesForCell(this, component, intVector.x + m, intVector.y + n);
				}
			}
			RenderMeshBuilder.CurrentCellXOffset = intVector.x - num2;
			RenderMeshBuilder.CurrentCellYOffset = intVector.y - num2;
			component.Build();
			RenderMeshBuilder.CurrentCellXOffset = 0;
			RenderMeshBuilder.CurrentCellYOffset = 0;
			component.renderData.transform.position = new Vector3(intVector.x - num2, intVector.y - num2, intVector.y - num2);
			roomHandler.OverrideTilemap = component;
			Pathfinder.Instance.InitializeRegion(data, roomHandler.area.basePosition + new IntVector2(-3, -3), roomHandler.area.dimensions + new IntVector2(3, 3));
			roomHandler.PostGenerationCleanup();
			DeadlyDeadlyGoopManager.ReinitializeData();
			return roomHandler;
		}

		public RoomHandler AddRuntimeRoom(IntVector2 dimensions, GameObject roomPrefab)
		{
			IntVector2 intVector = new IntVector2(data.Width + 10, 10);
			int newWidth = data.Width + 10 + dimensions.x;
			int newHeight = Mathf.Max(data.Height, dimensions.y + 10);
			CellData[][] array = BraveUtility.MultidimensionalArrayResize(data.cellData, data.Width, data.Height, newWidth, newHeight);
			CellArea cellArea = new CellArea(intVector, dimensions);
			cellArea.IsProceduralRoom = true;
			data.cellData = array;
			data.ClearCachedCellData();
			RoomHandler roomHandler = new RoomHandler(cellArea);
			for (int i = 0; i < dimensions.x; i++)
			{
				for (int j = 0; j < dimensions.y; j++)
				{
					IntVector2 p = new IntVector2(i, j) + intVector;
					CellData cellData = new CellData(p, CellType.FLOOR);
					cellData.parentArea = cellArea;
					cellData.parentRoom = roomHandler;
					cellData.nearestRoom = roomHandler;
					array[p.x][p.y] = cellData;
					roomHandler.RuntimeStampCellComplex(p.x, p.y, CellType.FLOOR, DiagonalWallType.NONE);
				}
			}
			data.rooms.Add(roomHandler);
			UnityEngine.Object.Instantiate(roomPrefab, new Vector3(intVector.x, intVector.y, 0f), Quaternion.identity);
			DeadlyDeadlyGoopManager.ReinitializeData();
			return roomHandler;
		}

		public GeneratedEnemyData GetWeightedProceduralEnemy()
		{
			float num = 0f;
			float value = UnityEngine.Random.value;
			for (int i = 0; i < m_generatedEnemyData.Count; i++)
			{
				num += m_generatedEnemyData[i].percentOfEnemies;
				if (num > value)
				{
					return m_generatedEnemyData[i];
				}
			}
			return m_generatedEnemyData[m_generatedEnemyData.Count - 1];
		}

		protected void RegisterGeneratedEnemyData(string id, int totalEnemyCount, bool isSignature)
		{
			int num = -1;
			for (int i = 0; i < m_generatedEnemyData.Count; i++)
			{
				if (m_generatedEnemyData[i].enemyGuid == id)
				{
					num = i;
					break;
				}
			}
			if (num < 0)
			{
				GeneratedEnemyData item = new GeneratedEnemyData(id, 1f / (float)totalEnemyCount, isSignature);
				m_generatedEnemyData.Add(item);
			}
			else
			{
				GeneratedEnemyData value = m_generatedEnemyData[num];
				value.percentOfEnemies += 1f / (float)totalEnemyCount;
				m_generatedEnemyData[num] = value;
			}
		}

		public void SpawnCurseReaper()
		{
			if (GameManager.HasInstance && (bool)GameManager.Instance.BestActivePlayer && GameManager.Instance.BestActivePlayer.CurrentRoom != null)
			{
				CurseReaperActive = true;
				GameObject superReaper = PrefabDatabase.Instance.SuperReaper;
				Vector2 vector = GameManager.Instance.BestActivePlayer.CurrentRoom.GetRandomVisibleClearSpot(2, 2).ToVector2();
				SpeculativeRigidbody component = superReaper.GetComponent<SpeculativeRigidbody>();
				if ((bool)component)
				{
					PixelCollider primaryPixelCollider = component.PrimaryPixelCollider;
					Vector2 vector2 = PhysicsEngine.PixelToUnit(new IntVector2(primaryPixelCollider.ManualOffsetX, primaryPixelCollider.ManualOffsetY));
					Vector2 vector3 = PhysicsEngine.PixelToUnit(new IntVector2(primaryPixelCollider.ManualWidth, primaryPixelCollider.ManualHeight));
					Vector2 vector4 = new Vector2((new Vector2(Mathf.CeilToInt(vector3.x), Mathf.CeilToInt(vector3.y)).x - vector3.x) / 2f, 0f).Quantize(0.0625f);
					vector -= vector2 - vector4;
				}
				UnityEngine.Object.Instantiate(superReaper, vector.ToVector3ZUp(), Quaternion.identity);
			}
		}

		private IEnumerator HandleAmbientChannelVFX()
		{
			m_ambientVFXProcessingActive = true;
			while (m_ambientVFXProcessingActive)
			{
				if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW)
				{
					yield return null;
					continue;
				}
				if (!GameManager.Instance.IsLoadingLevel)
				{
					CameraController mainCameraController = GameManager.Instance.MainCameraController;
					if (!mainCameraController)
					{
						continue;
					}
					IntVector2 intVector = mainCameraController.MinVisiblePoint.ToIntVector2(VectorConversions.Floor);
					IntVector2 intVector2 = mainCameraController.MaxVisiblePoint.ToIntVector2(VectorConversions.Ceil);
					for (int i = intVector.x; i <= intVector2.x; i++)
					{
						for (int j = intVector.y; j <= intVector2.y; j++)
						{
							IntVector2 intVector3 = new IntVector2(i, j);
							if (data == null || !data.CheckInBounds(intVector3, 3))
							{
								continue;
							}
							CellData cellData = data[intVector3];
							if (cellData == null || (!cellData.cellVisualData.IsChannel && !cellData.doesDamage) || cellData.cellVisualData.precludeAllTileDrawing || !IsCentralChannel(cellData))
							{
								continue;
							}
							DungeonMaterial dungeonMaterial = roomMaterialDefinitions[cellData.cellVisualData.roomVisualTypeIndex];
							RoomHandler parentRoom = cellData.parentRoom;
							if (dungeonMaterial == null || !dungeonMaterial.UseChannelAmbientVFX || dungeonMaterial.AmbientChannelVFX == null)
							{
								continue;
							}
							if (cellData.cellVisualData.PitVFXCooldown > 0f)
							{
								cellData.cellVisualData.PitVFXCooldown -= BraveTime.DeltaTime;
								continue;
							}
							float num = 0.5f;
							if (UnityEngine.Random.value < num)
							{
								GameObject gameObject = dungeonMaterial.AmbientChannelVFX[UnityEngine.Random.Range(0, dungeonMaterial.AmbientChannelVFX.Count)];
								Vector3 position = gameObject.transform.position;
								SpawnManager.SpawnVFX(gameObject, cellData.position.ToVector2().ToVector3ZisY() + position + new Vector3(UnityEngine.Random.Range(0.25f, 0.75f), UnityEngine.Random.Range(0.25f, 0.75f), 2f), Quaternion.identity);
							}
							cellData.cellVisualData.PitVFXCooldown = UnityEngine.Random.Range(dungeonMaterial.ChannelVFXMinCooldown, dungeonMaterial.ChannelVFXMaxCooldown);
						}
					}
				}
				yield return null;
			}
		}

		private bool IsCentralChannel(CellData cell)
		{
			IntVector2 position = cell.position;
			for (int i = 0; i < IntVector2.Cardinals.Length; i++)
			{
				IntVector2 key = position + IntVector2.Cardinals[i];
				if (!data[key].cellVisualData.IsChannel && !data[key].doesDamage)
				{
					return false;
				}
			}
			return true;
		}

		private IEnumerator HandleAmbientPitVFX()
		{
			m_ambientVFXProcessingActive = true;
			while (m_ambientVFXProcessingActive)
			{
				if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW)
				{
					yield return null;
					continue;
				}
				if (!GameManager.Instance.IsLoadingLevel)
				{
					CameraController mainCameraController = GameManager.Instance.MainCameraController;
					if (!mainCameraController)
					{
						continue;
					}
					IntVector2 intVector = mainCameraController.MinVisiblePoint.ToIntVector2(VectorConversions.Floor);
					IntVector2 intVector2 = mainCameraController.MaxVisiblePoint.ToIntVector2(VectorConversions.Ceil);
					for (int i = intVector.x; i <= intVector2.x; i++)
					{
						for (int j = intVector.y; j <= intVector2.y; j++)
						{
							IntVector2 intVector3 = new IntVector2(i, j);
							if (data == null || !data.CheckInBounds(intVector3, 3))
							{
								continue;
							}
							CellData cellData = data[intVector3];
							CellData cellData2 = data[intVector3 + IntVector2.Up];
							CellData cellData3 = data[intVector3 + IntVector2.Down];
							bool flag = true;
							if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON)
							{
								CellData cellData4 = data[intVector3 + IntVector2.Up * 2];
								flag = cellData4 != null && cellData4.type == CellType.PIT;
							}
							if (cellData == null || !flag || cellData.type != CellType.PIT || cellData.fallingPrevented || cellData.cellVisualData.precludeAllTileDrawing || cellData2 == null || cellData2.type != CellType.PIT || cellData3 == null)
							{
								continue;
							}
							DungeonMaterial dungeonMaterial = roomMaterialDefinitions[cellData.cellVisualData.roomVisualTypeIndex];
							RoomHandler parentRoom = cellData.parentRoom;
							if (dungeonMaterial == null)
							{
								continue;
							}
							if (!cellData.cellVisualData.HasTriggeredPitVFX)
							{
								cellData.cellVisualData.HasTriggeredPitVFX = true;
								cellData.cellVisualData.PitVFXCooldown = UnityEngine.Random.Range(1f, dungeonMaterial.PitVFXMaxCooldown / 2f);
								cellData.cellVisualData.PitParticleCooldown = UnityEngine.Random.Range(0f, 1f);
							}
							if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON || (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON && dungeonMaterial.usesFacewallGrids))
							{
								cellData.cellVisualData.PitParticleCooldown -= BraveTime.DeltaTime;
								if (cellData.cellVisualData.PitParticleCooldown <= 0f)
								{
									Vector3 position = BraveUtility.RandomVector2(cellData.position.ToVector2(), cellData.position.ToVector2() + Vector2.one).ToVector3ZisY();
									cellData.cellVisualData.PitParticleCooldown = UnityEngine.Random.Range(0.35f, 0.95f);
									if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON && dungeonMaterial.usesFacewallGrids)
									{
										GlobalSparksDoer.DoSingleParticle(position, Vector3.zero, null, 0.375f, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
									}
									else if (tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON && parentRoom != null && parentRoom.area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.BOSS)
									{
										GlobalSparksDoer.DoSingleParticle(position, Vector3.up, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
									}
								}
							}
							if (!dungeonMaterial.UsePitAmbientVFX || dungeonMaterial.AmbientPitVFX == null || cellData3.type != CellType.PIT)
							{
								continue;
							}
							if (cellData.cellVisualData.PitVFXCooldown > 0f)
							{
								cellData.cellVisualData.PitVFXCooldown -= BraveTime.DeltaTime;
								continue;
							}
							float num = 1f;
							if (UnityEngine.Random.value < dungeonMaterial.ChanceToSpawnPitVFXOnCooldown * num)
							{
								GameObject gameObject = dungeonMaterial.AmbientPitVFX[UnityEngine.Random.Range(0, dungeonMaterial.AmbientPitVFX.Count)];
								Vector3 position2 = gameObject.transform.position;
								SpawnManager.SpawnVFX(gameObject, cellData.position.ToVector2().ToVector3ZisY() + position2 + new Vector3(UnityEngine.Random.Range(0.25f, 0.75f), UnityEngine.Random.Range(0.25f, 0.75f), 2f), Quaternion.identity);
							}
							cellData.cellVisualData.PitVFXCooldown = UnityEngine.Random.Range(dungeonMaterial.PitVFXMinCooldown, dungeonMaterial.PitVFXMaxCooldown);
						}
					}
				}
				yield return null;
			}
			m_ambientVFXProcessingActive = false;
		}
	}
}
