using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Pathfinding;
using UnityEngine;

namespace Dungeonator
{
	public class RoomHandler
	{
		public enum ProceduralLockType
		{
			NONE,
			BASE_SHOP
		}

		public enum VisibilityStatus
		{
			OBSCURED = 0,
			VISITED = 1,
			CURRENT = 2,
			REOBSCURED = 3,
			INVALID = 99
		}

		public enum NPCSealState
		{
			SealNone,
			SealAll,
			SealPrior,
			SealNext
		}

		public delegate void OnEnteredEventHandler(PlayerController p);

		public enum CustomRoomState
		{
			NONE = 0,
			LICH_PHASE_THREE = 100
		}

		public delegate void OnExitedEventHandler();

		public delegate void OnBecameVisibleEventHandler(float delay);

		public delegate void OnBecameInvisibleEventHandler();

		public enum RewardLocationStyle
		{
			CameraCenter,
			PlayerCenter,
			Original
		}

		public enum ActiveEnemyType
		{
			All,
			RoomClear
		}

		private class TupleComparer : IComparer<Tuple<IntVector2, float>>
		{
			public int Compare(Tuple<IntVector2, float> a, Tuple<IntVector2, float> b)
			{
				if (a.Second < b.Second)
				{
					return -1;
				}
				if (b.Second > a.Second)
				{
					return 1;
				}
				return 0;
			}
		}

		public static bool DrawRandomCellLines = false;

		public int distanceFromEntrance;

		public bool IsLoopMember;

		public bool LoopIsUnidirectional;

		public Guid LoopGuid;

		public CellArea area;

		public Rect cameraBoundingRect;

		public RoomHandlerBoundingPolygon cameraBoundingPolygon;

		public RoomHandler parentRoom;

		public List<RoomHandler> childRooms;

		public FlowActionLine flowActionLine;

		public List<DungeonDoorController> connectedDoors;

		public List<DungeonDoorSubsidiaryBlocker> standaloneBlockers;

		public List<BossTriggerZone> bossTriggerZones;

		public List<RoomHandler> connectedRooms;

		public Dictionary<PrototypeRoomExit, RoomHandler> connectedRoomsByExit;

		public Dictionary<RuntimeRoomExitData, RuntimeExitDefinition> exitDefinitionsByExit;

		public RoomHandler injectionTarget;

		public List<RoomHandler> injectionFrameData;

		public Opulence opulence;

		public GameObject OptionalDoorTopDecorable;

		public RoomCreationStrategy.RoomType roomType;

		public bool CanReceiveCaps;

		[NonSerialized]
		public ProceduralLockType ProceduralLockingType;

		[NonSerialized]
		public bool ShouldAttemptProceduralLock;

		[NonSerialized]
		public float AttemptProceduralLockChance;

		public bool IsOnCriticalPath;

		public int DungeonWingID = -1;

		public bool PrecludeTilemapDrawing;

		public bool DrawPrecludedCeilingTiles;

		[NonSerialized]
		public bool PlayerHasTakenDamageInThisRoom;

		[NonSerialized]
		public bool ForcePreventChannels;

		[NonSerialized]
		public tk2dTileMap OverrideTilemap;

		[NonSerialized]
		public bool PreventMinimapUpdates;

		public VisibilityStatus OverrideVisibility = VisibilityStatus.INVALID;

		public bool PreventRevealEver;

		private VisibilityStatus m_visibility;

		public bool forceTeleportersActive;

		public bool hasEverBeenVisited;

		public bool CompletelyPreventLeaving;

		public Action OnRevealedOnMap;

		private bool m_forceRevealedOnMap;

		public Transform hierarchyParent;

		public IntVector2 Epicenter;

		public GameObject ExtantEmergencyCrate;

		public bool PreventStandardRoomReward;

		public static bool HasGivenRoomChestRewardThisRun = false;

		public static int NumberOfRoomsToPreventChestSpawning = 0;

		private int m_roomVisualType;

		private RoomMotionHandler m_roomMotionHandler;

		public Dictionary<IntVector2, float> OcclusionPerimeterCellMap;

		public SecretRoomManager secretRoomManager;

		private HashSet<IntVector2> rawRoomCells = new HashSet<IntVector2>();

		private List<IntVector2> roomCells = new List<IntVector2>();

		private List<IntVector2> roomCellsWithoutExits = new List<IntVector2>();

		private List<IntVector2> featureCells = new List<IntVector2>();

		private List<RoomEventTriggerArea> eventTriggerAreas;

		private Dictionary<int, RoomEventTriggerArea> eventTriggerMap;

		private float m_totalSpawnedEnemyHP;

		private float m_lastTotalSpawnedEnemyHP;

		private float m_activeDifficultyModifier = 1f;

		private List<AIActor> activeEnemies;

		private List<IPlayerInteractable> interactableObjects;

		private List<IAutoAimTarget> autoAimTargets;

		private List<PrototypeRoomObjectLayer> remainingReinforcementLayers;

		private Dictionary<PrototypeRoomObjectLayer, Dictionary<PrototypePlacedObjectData, GameObject>> preloadedReinforcementLayerData;

		public static List<IPlayerInteractable> unassignedInteractableObjects = new List<IPlayerInteractable>();

		public NPCSealState npcSealState;

		private bool m_isSealed;

		private bool m_currentlyVisible;

		private bool m_hasGivenReward;

		private GameObject m_secretRoomCoverObject;

		[NonSerialized]
		public RoomHandler TargetPitfallRoom;

		[NonSerialized]
		public bool ForcePitfallForFliers;

		public Action OnTargetPitfallRoom;

		public Action<PlayerController> OnPlayerReturnedFromPit;

		public Action OnDarkSoulsReset;

		[NonSerialized]
		private bool m_hasBeenEncountered;

		public CustomRoomState AdditionalRoomState;

		private bool m_allRoomsActive;

		[NonSerialized]
		public bool? ForcedActiveState;

		private int numberOfTimedWavesOnDeck;

		public Action<bool> OnChangedTerrifyingDarkState;

		public bool IsDarkAndTerrifying;

		private bool? m_cachedIsMaintenance;

		public Action OnEnemiesCleared;

		public Func<bool> PreEnemiesCleared;

		private List<SpeculativeRigidbody> m_roomMovingPlatforms = new List<SpeculativeRigidbody>();

		private List<DeadlyDeadlyGoopManager> m_goops;

		[NonSerialized]
		public GenericLootTable OverrideBossRewardTable;

		public bool EverHadEnemies;

		public List<RoomHandler> DarkSoulsRoomResetDependencies;

		public Action<bool> OnSealChanged;

		public Action<AIActor> OnEnemyRegistered;

		private static List<AIActor> s_tempActiveEnemies = new List<AIActor>();

		public VisibilityStatus visibility
		{
			get
			{
				if (OverrideVisibility != VisibilityStatus.INVALID)
				{
					return OverrideVisibility;
				}
				return m_visibility;
			}
			set
			{
				m_visibility = value;
				if (m_visibility == VisibilityStatus.OBSCURED || m_visibility == VisibilityStatus.REOBSCURED)
				{
					hasEverBeenVisited = false;
				}
				else if (m_visibility == VisibilityStatus.VISITED)
				{
					hasEverBeenVisited = true;
				}
			}
		}

		public bool TeleportersActive
		{
			get
			{
				return IsVisible || forceTeleportersActive;
			}
		}

		public bool IsVisible
		{
			get
			{
				return visibility != 0 && visibility != VisibilityStatus.REOBSCURED;
			}
		}

		public bool IsShop { get; set; }

		public bool IsWildWestEntrance
		{
			get
			{
				return false;
			}
		}

		public bool RevealedOnMap
		{
			get
			{
				return visibility != 0 || m_forceRevealedOnMap;
			}
			set
			{
				if (!m_forceRevealedOnMap && OnRevealedOnMap != null)
				{
					OnRevealedOnMap();
				}
				m_forceRevealedOnMap = value;
			}
		}

		public int RoomVisualSubtype
		{
			get
			{
				return m_roomVisualType;
			}
			set
			{
				m_roomVisualType = value;
			}
		}

		public DungeonMaterial RoomMaterial
		{
			get
			{
				return GameManager.Instance.Dungeon.roomMaterialDefinitions[RoomVisualSubtype];
			}
		}

		public List<IntVector2> Cells
		{
			get
			{
				return roomCells;
			}
		}

		public List<IntVector2> CellsWithoutExits
		{
			get
			{
				return roomCellsWithoutExits;
			}
		}

		public HashSet<IntVector2> RawCells
		{
			get
			{
				return rawRoomCells;
			}
		}

		public List<IntVector2> FeatureCells
		{
			get
			{
				return featureCells;
			}
		}

		public bool IsSealed
		{
			get
			{
				return m_isSealed;
			}
		}

		public IntVector2? OverrideBossPedestalLocation { get; set; }

		public bool IsGunslingKingChallengeRoom { get; set; }

		public bool IsWinchesterArcadeRoom { get; set; }

		public bool IsStartOfWarpWing
		{
			get
			{
				if (area.instanceUsedExits.Count == 0 && !area.IsProceduralRoom)
				{
					return true;
				}
				for (int i = 0; i < area.instanceUsedExits.Count; i++)
				{
					if (area.exitToLocalDataMap.ContainsKey(area.instanceUsedExits[i]) && area.exitToLocalDataMap[area.instanceUsedExits[i]].isWarpWingStart)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool IsStandardRoom
		{
			get
			{
				if (area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.NORMAL || area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.CONNECTOR || area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.HUB)
				{
					return true;
				}
				return false;
			}
		}

		public bool IsSecretRoom
		{
			get
			{
				if (secretRoomManager == null)
				{
					return false;
				}
				if (secretRoomManager.IsOpen)
				{
					return false;
				}
				return true;
			}
		}

		public bool WasEverSecretRoom
		{
			get
			{
				if (secretRoomManager == null)
				{
					return false;
				}
				return true;
			}
		}

		public List<SpeculativeRigidbody> RoomMovingPlatforms
		{
			get
			{
				return m_roomMovingPlatforms;
			}
		}

		public List<DeadlyDeadlyGoopManager> RoomGoops
		{
			get
			{
				return m_goops;
			}
		}

		public event OnEnteredEventHandler Entered;

		public event OnExitedEventHandler Exited;

		public event OnBecameVisibleEventHandler BecameVisible;

		public event OnBecameInvisibleEventHandler BecameInvisible;

		public RoomHandler(CellArea a)
		{
			area = a;
			if (GameManager.Instance.BestGenerationDungeonPrefab != null)
			{
				RoomVisualSubtype = GameManager.Instance.BestGenerationDungeonPrefab.decoSettings.standardRoomVisualSubtypes.SelectByWeight();
			}
			else
			{
				RoomVisualSubtype = GameManager.Instance.Dungeon.decoSettings.standardRoomVisualSubtypes.SelectByWeight();
			}
			if (area.prototypeRoom == null)
			{
				RoomVisualSubtype = 0;
			}
			if (a.prototypeRoom != null && a.prototypeRoom.overrideRoomVisualType >= 0)
			{
				RoomVisualSubtype = a.prototypeRoom.overrideRoomVisualType;
			}
			if (a.prototypeRoom != null)
			{
				Dungeon dungeon = ((!(GameManager.Instance.BestGenerationDungeonPrefab != null)) ? GameManager.Instance.Dungeon : GameManager.Instance.BestGenerationDungeonPrefab);
				DungeonMaterial dungeonMaterial = dungeon.roomMaterialDefinitions[m_roomVisualType];
				bool flag = a.prototypeRoom.ContainsPit();
				bool flag2 = false;
				for (int i = 0; i < dungeon.decoSettings.standardRoomVisualSubtypes.elements.Length; i++)
				{
					WeightedInt weightedInt = dungeon.decoSettings.standardRoomVisualSubtypes.elements[i];
					if (!(weightedInt.weight <= 0f) && dungeon.roomMaterialDefinitions[weightedInt.value].supportsPits)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					while (flag && !dungeonMaterial.supportsPits)
					{
						RoomVisualSubtype = dungeon.decoSettings.standardRoomVisualSubtypes.SelectByWeight();
						dungeonMaterial = dungeon.roomMaterialDefinitions[m_roomVisualType];
					}
				}
				PrecludeTilemapDrawing = a.prototypeRoom.precludeAllTilemapDrawing;
				DrawPrecludedCeilingTiles = a.prototypeRoom.drawPrecludedCeilingTiles;
			}
			if (GameManager.Instance.BestGenerationDungeonPrefab != null)
			{
				if (m_roomVisualType < 0 || m_roomVisualType >= GameManager.Instance.BestGenerationDungeonPrefab.roomMaterialDefinitions.Length)
				{
					m_roomVisualType = 0;
				}
			}
			else if (m_roomVisualType < 0 || m_roomVisualType >= GameManager.Instance.Dungeon.roomMaterialDefinitions.Length)
			{
				m_roomVisualType = 0;
			}
			roomType = RoomCreationStrategy.RoomType.PREDEFINED_ROOM;
			opulence = Opulence.FINE;
			childRooms = new List<RoomHandler>();
			connectedDoors = new List<DungeonDoorController>();
			standaloneBlockers = new List<DungeonDoorSubsidiaryBlocker>();
			connectedRooms = new List<RoomHandler>();
			connectedRoomsByExit = new Dictionary<PrototypeRoomExit, RoomHandler>();
			interactableObjects = new List<IPlayerInteractable>();
			autoAimTargets = new List<IAutoAimTarget>();
			OnEnemiesCleared = (Action)Delegate.Combine(OnEnemiesCleared, new Action(NotifyPlayerRoomCleared));
			OnEnemiesCleared = (Action)Delegate.Combine(OnEnemiesCleared, new Action(HandleRoomClearReward));
		}

		protected virtual void OnEntered(PlayerController p)
		{
			SetRoomActive(true);
			if (OverrideTilemap != null && PhysicsEngine.Instance.TileMap != OverrideTilemap)
			{
				PhysicsEngine.Instance.ClearAllCachedTiles();
				PhysicsEngine.Instance.TileMap = OverrideTilemap;
			}
			else if (OverrideTilemap == null && PhysicsEngine.Instance.TileMap != GameManager.Instance.Dungeon.MainTilemap)
			{
				PhysicsEngine.Instance.ClearAllCachedTiles();
				PhysicsEngine.Instance.TileMap = GameManager.Instance.Dungeon.MainTilemap;
			}
			if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.TUTORIAL)
			{
				GameManager.Instance.Dungeon.StartCoroutine(DeferredMarkVisibleRoomsActive(p));
			}
			if (!area.IsProceduralRoom && !m_hasBeenEncountered)
			{
				m_hasBeenEncountered = true;
				GameStatsManager.Instance.HandleEncounteredRoom(area.runtimePrototypeData);
			}
			if (!m_currentlyVisible)
			{
				OnBecameVisible(p);
			}
			if (GameManager.Instance.NumberOfLivingPlayers == 1 && !p.IsGhost)
			{
				Minimap.Instance.RevealMinimapRoom(this);
			}
			else if (p.IsPrimaryPlayer)
			{
				Minimap.Instance.RevealMinimapRoom(this);
			}
			if (m_secretRoomCoverObject != null)
			{
				m_secretRoomCoverObject.SetActive(false);
			}
			ProcessRoomEvents(RoomEventTriggerCondition.ON_ENTER);
			List<AIActor> list = GetActiveEnemies(ActiveEnemyType.RoomClear);
			int num = 0;
			if (list != null && list.Exists((AIActor a) => !a.healthHaver.IsDead))
			{
				num += list.Count;
				ProcessRoomEvents(RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES);
				if (remainingReinforcementLayers != null)
				{
					for (int i = 0; i < remainingReinforcementLayers.Count; i++)
					{
						num += remainingReinforcementLayers[i].placedObjects.Count;
						if (remainingReinforcementLayers[i].reinforcementTriggerCondition == RoomEventTriggerCondition.TIMER)
						{
							GameManager.Instance.StartCoroutine(HandleTimedReinforcementLayer(remainingReinforcementLayers[i]));
						}
					}
				}
			}
			if (this.Entered != null)
			{
				this.Entered(p);
			}
			bool flag = true;
			for (int j = 0; j < GameManager.Instance.Dungeon.data.rooms.Count; j++)
			{
				if (GameManager.Instance.Dungeon.data.rooms[j].visibility == VisibilityStatus.OBSCURED)
				{
					flag = false;
					break;
				}
			}
			if (GameManager.Instance.Dungeon.OnAnyRoomVisited != null)
			{
				GameManager.Instance.Dungeon.OnAnyRoomVisited();
			}
			if (flag)
			{
				GameManager.Instance.Dungeon.NotifyAllRoomsVisited();
			}
		}

		public IEnumerator DeferredMarkVisibleRoomsActive(PlayerController p)
		{
			bool shouldActiveAllRoomsForEntranceDelayPeriod = GameManager.Instance.Dungeon.data.Entrance == this;
			if (!GameManager.Instance.IsFoyer && GameManager.Instance.Dungeon.FrameDungeonGenerationFinished > 0 && Time.frameCount - GameManager.Instance.Dungeon.FrameDungeonGenerationFinished > 100)
			{
				shouldActiveAllRoomsForEntranceDelayPeriod = false;
			}
			if (shouldActiveAllRoomsForEntranceDelayPeriod)
			{
				for (int j = 0; j < connectedRooms.Count; j++)
				{
					if (connectedRooms[j].visibility != 0)
					{
						shouldActiveAllRoomsForEntranceDelayPeriod = false;
					}
				}
			}
			m_allRoomsActive = shouldActiveAllRoomsForEntranceDelayPeriod;
			int changedCounter = 0;
			for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
			{
				bool changedRoom = false;
				RoomHandler room = GameManager.Instance.Dungeon.data.rooms[i];
				RoomHandler otherCurrentRoom = null;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(p);
					if ((bool)otherPlayer && otherPlayer.CurrentRoom != this)
					{
						otherCurrentRoom = otherPlayer.CurrentRoom;
					}
				}
				if (room != this)
				{
					Rect rect = room.cameraBoundingRect;
					rect.xMin -= 1f;
					rect.xMax += 1f;
					rect.yMin -= 1f;
					rect.yMax += 3f;
					changedRoom = (shouldActiveAllRoomsForEntranceDelayPeriod ? room.SetRoomActive(true) : ((GameManager.Instance.Dungeon.data.Entrance == this && room.IsMaintenanceRoom()) ? room.SetRoomActive(true) : ((connectedRooms.Contains(room) || BraveMathCollege.DistBetweenRectangles(room.cameraBoundingRect.min, room.cameraBoundingRect.size, cameraBoundingRect.min, cameraBoundingRect.size) <= GameManager.Instance.MainCameraController.Camera.orthographicSize * GameManager.Instance.MainCameraController.Camera.aspect) ? ((!connectedRooms.Contains(room)) ? room.SetRoomActive(true && (room.visibility == VisibilityStatus.VISITED || room.visibility == VisibilityStatus.CURRENT)) : room.SetRoomActive(true)) : ((otherCurrentRoom == null || !(BraveMathCollege.DistBetweenRectangles(room.cameraBoundingRect.min, room.cameraBoundingRect.size, otherCurrentRoom.cameraBoundingRect.min, otherCurrentRoom.cameraBoundingRect.size) <= GameManager.Instance.MainCameraController.Camera.orthographicSize * GameManager.Instance.MainCameraController.Camera.aspect)) ? room.SetRoomActive(false) : ((!otherCurrentRoom.connectedRooms.Contains(room)) ? room.SetRoomActive(true && (room.visibility == VisibilityStatus.VISITED || room.visibility == VisibilityStatus.CURRENT)) : room.SetRoomActive(true))))));
				}
				if (changedRoom)
				{
					changedCounter++;
					if (changedCounter >= 3)
					{
						changedCounter = 0;
						yield return null;
					}
				}
			}
		}

		public bool SetRoomActive(bool active)
		{
			if (ForcedActiveState.HasValue && ForcedActiveState.Value != active)
			{
				return false;
			}
			if (m_roomMotionHandler != null && m_roomMotionHandler.gameObject.activeSelf != active)
			{
				m_roomMotionHandler.gameObject.SetActive(active);
				return true;
			}
			return false;
		}

		private IEnumerator HandleTimedReinforcementLayer(PrototypeRoomObjectLayer layer)
		{
			numberOfTimedWavesOnDeck++;
			yield return null;
			float elapsed = 0f;
			while (elapsed < layer.delayTime && GameManager.Instance.IsAnyPlayerInRoom(this) && HasActiveEnemies(ActiveEnemyType.RoomClear))
			{
				elapsed += BraveTime.DeltaTime;
				yield return null;
			}
			numberOfTimedWavesOnDeck--;
			if (GameManager.Instance.IsAnyPlayerInRoom(this) && remainingReinforcementLayers != null && remainingReinforcementLayers.Count > 0)
			{
				TriggerReinforcementLayer(remainingReinforcementLayers.IndexOf(layer));
			}
		}

		protected virtual void OnExited(PlayerController p)
		{
			if (m_currentlyVisible)
			{
				OnBecameInvisible(p);
			}
			if (ExtantEmergencyCrate != null)
			{
				EmergencyCrateController component = ExtantEmergencyCrate.GetComponent<EmergencyCrateController>();
				if ((bool)component)
				{
					component.ClearLandingTarget();
				}
				UnityEngine.Object.Destroy(ExtantEmergencyCrate);
				ExtantEmergencyCrate = null;
			}
			ProcessRoomEvents(RoomEventTriggerCondition.ON_EXIT);
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				if (!GameManager.Instance.GetOtherPlayer(p) || GameManager.Instance.GetOtherPlayer(p).CurrentRoom != this)
				{
					Minimap.Instance.DeflagPreviousRoom(this);
				}
			}
			else
			{
				Minimap.Instance.DeflagPreviousRoom(this);
			}
			if (this.Exited != null)
			{
				this.Exited();
			}
		}

		public bool RoomFallValidForMaintenance()
		{
			GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
			return GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE && this == GameManager.Instance.Dungeon.data.Entrance && (tilesetId == GlobalDungeonData.ValidTilesets.GUNGEON || tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON || tilesetId == GlobalDungeonData.ValidTilesets.CATACOMBGEON || tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON);
		}

		public void BecomeTerrifyingDarkRoom(float duration = 1f, float goalIntensity = 0.1f, float lightIntensity = 1f, string wwiseEvent = "Play_ENM_darken_world_01")
		{
			if (!IsDarkAndTerrifying && (area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS || GetActiveEnemiesCount(ActiveEnemyType.All) != 0))
			{
				if (OnChangedTerrifyingDarkState != null)
				{
					OnChangedTerrifyingDarkState(true);
				}
				GameManager.Instance.StartCoroutine(HandleBecomeTerrifyingDarkRoom(duration, goalIntensity, lightIntensity));
				AkSoundEngine.PostEvent(wwiseEvent, GameManager.Instance.PrimaryPlayer.gameObject);
			}
		}

		public void EndTerrifyingDarkRoom(float duration = 1f, float goalIntensity = 0.1f, float lightIntensity = 1f, string wwiseEvent = "Play_ENM_lighten_world_01")
		{
			if (IsDarkAndTerrifying)
			{
				if (OnChangedTerrifyingDarkState != null)
				{
					OnChangedTerrifyingDarkState(false);
				}
				GameManager.Instance.StartCoroutine(HandleBecomeTerrifyingDarkRoom(duration, goalIntensity, lightIntensity, true));
				AkSoundEngine.PostEvent(wwiseEvent, GameManager.Instance.PrimaryPlayer.gameObject);
			}
		}

		private IEnumerator HandleBecomeTerrifyingDarkRoom(float duration, float goalIntensity, float lightIntensity = 1f, bool reverse = false)
		{
			float elapsed = 0f;
			IsDarkAndTerrifying = !reverse;
			while (elapsed < duration || duration == 0f)
			{
				elapsed += GameManager.INVARIANT_DELTA_TIME;
				float t = ((duration != 0f) ? Mathf.Clamp01(elapsed / duration) : 1f);
				if (reverse)
				{
					t = 1f - t;
				}
				float num2 = (RenderSettings.ambientIntensity = ((GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && GameManager.Options.ShaderQuality != 0) ? 1f : 1.25f));
				float targetAmbient = num2;
				RenderSettings.ambientIntensity = Mathf.Lerp(targetAmbient, goalIntensity, t);
				if (!GameManager.Instance.Dungeon.PreventPlayerLightInDarkTerrifyingRooms)
				{
					GameManager.Instance.Dungeon.PlayerIsLight = true;
					GameManager.Instance.Dungeon.PlayerLightColor = Color.white;
					GameManager.Instance.Dungeon.PlayerLightIntensity = Mathf.Lerp(0f, lightIntensity * 4.25f, t);
					GameManager.Instance.Dungeon.PlayerLightRadius = Mathf.Lerp(0f, lightIntensity * 7.25f, t);
				}
				Pixelator.Instance.pointLightMultiplier = Mathf.Lerp(1f, 0f, t);
				if (duration == 0f)
				{
					break;
				}
				yield return null;
			}
			if (!GameManager.Instance.Dungeon.PreventPlayerLightInDarkTerrifyingRooms && reverse)
			{
				GameManager.Instance.Dungeon.PlayerIsLight = false;
			}
		}

		public bool IsMaintenanceRoom()
		{
			if (!m_cachedIsMaintenance.HasValue)
			{
				m_cachedIsMaintenance = !string.IsNullOrEmpty(GetRoomName()) && GetRoomName().Contains("Maintenance");
			}
			return m_cachedIsMaintenance.Value;
		}

		public string GetRoomName()
		{
			return area.PrototypeRoomName;
		}

		public void PlayerEnter(PlayerController playerEntering)
		{
			if (Pathfinder.HasInstance)
			{
				Pathfinder.Instance.TryRecalculateRoomClearances(this);
			}
			OnEntered(playerEntering);
			GameManager.Instance.DungeonMusicController.NotifyEnteredNewRoom(this);
			Pixelator.Instance.ProcessRoomAdditionalExits(playerEntering.transform.position.IntXY(VectorConversions.Floor), this, false);
		}

		public void PlayerInCell(PlayerController p, IntVector2 playerCellPosition, Vector2 relevantCornerOfPlayer)
		{
			if (m_roomMotionHandler != null && !m_roomMotionHandler.gameObject.activeSelf)
			{
				m_roomMotionHandler.gameObject.SetActive(true);
				GameManager.Instance.Dungeon.StartCoroutine(DeferredMarkVisibleRoomsActive(p));
			}
			if (GameManager.Instance.Dungeon.data.Entrance == this && m_allRoomsActive && GameManager.Instance.Dungeon.FrameDungeonGenerationFinished > 0 && Time.frameCount - GameManager.Instance.Dungeon.FrameDungeonGenerationFinished > 100)
			{
				GameManager.Instance.Dungeon.StartCoroutine(DeferredMarkVisibleRoomsActive(p));
			}
			CellData cellData = GameManager.Instance.Dungeon.data[playerCellPosition];
			if (cellData != null && !cellData.isExitCell && cellData.parentRoom != null && !cellData.parentRoom.RevealedOnMap)
			{
				Minimap.Instance.RevealMinimapRoom(cellData.parentRoom, true);
			}
			if (ForcePitfallForFliers && (bool)p && !p.IsFalling && p.IsFlying && cellData != null && cellData.type == CellType.PIT && !cellData.fallingPrevented)
			{
				Rect rect = default(Rect);
				rect.min = PhysicsEngine.PixelToUnitMidpoint(p.specRigidbody.PrimaryPixelCollider.LowerLeft);
				rect.max = PhysicsEngine.PixelToUnitMidpoint(p.specRigidbody.PrimaryPixelCollider.UpperRight);
				Dungeon dungeon = GameManager.Instance.Dungeon;
				bool flag = dungeon.ShouldReallyFall(rect.min);
				bool flag2 = dungeon.ShouldReallyFall(new Vector3(rect.xMax, rect.yMin));
				bool flag3 = dungeon.ShouldReallyFall(new Vector3(rect.xMin, rect.yMax));
				bool flag4 = dungeon.ShouldReallyFall(rect.max);
				bool flag5 = dungeon.ShouldReallyFall(rect.center);
				if (flag && flag2 && flag5 && flag3 && flag4)
				{
					p.ForceFall();
				}
			}
			if (cellData.doesDamage && (cellData.damageDefinition.damageToPlayersPerTick > 0f || cellData.damageDefinition.isPoison) && p.IsGrounded && p.CurrentFloorDamageCooldown <= 0f && p.healthHaver.IsVulnerable)
			{
				bool flag6 = true;
				int tile = GameManager.Instance.Dungeon.MainTilemap.Layers[GlobalDungeonData.floorLayerIndex].GetTile(playerCellPosition.x, playerCellPosition.y);
				if (tile >= 0 && tile < GameManager.Instance.Dungeon.tileIndices.dungeonCollection.spriteDefinitions.Length)
				{
					tk2dSpriteDefinition tk2dSpriteDefinition = GameManager.Instance.Dungeon.tileIndices.dungeonCollection.spriteDefinitions[tile];
					BagelCollider[] array = ((tk2dSpriteDefinition == null) ? null : GameManager.Instance.Dungeon.tileIndices.dungeonCollection.GetBagelColliders(tile));
					if (array != null && array.Length > 0)
					{
						flag6 = false;
						BagelCollider bagelCollider = array[0];
						IntVector2 intVector = ((p.specRigidbody.PrimaryPixelCollider.UnitCenter - playerCellPosition.ToVector2()) * 16f).ToIntVector2(VectorConversions.Floor);
						if (intVector.x >= 0 && intVector.y >= 0 && intVector.x < 16 && intVector.y < 16 && bagelCollider[intVector.x, bagelCollider.height - intVector.y - 1])
						{
							flag6 = true;
						}
					}
				}
				if (flag6)
				{
					if (cellData.damageDefinition.isPoison || GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON)
					{
						p.IncreasePoison(BraveTime.DeltaTime / cellData.damageDefinition.tickFrequency);
						if (p.CurrentPoisonMeterValue >= 1f)
						{
							p.healthHaver.ApplyDamage(cellData.damageDefinition.damageToPlayersPerTick, Vector2.zero, StringTableManager.GetEnemiesString("#THEFLOOR"), cellData.damageDefinition.damageTypes, DamageCategory.Environment);
							p.CurrentPoisonMeterValue -= 1f;
						}
					}
					else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
					{
						p.IsOnFire = true;
					}
					else
					{
						p.healthHaver.ApplyDamage(cellData.damageDefinition.damageToPlayersPerTick, Vector2.zero, StringTableManager.GetEnemiesString("#THEFLOOR"), cellData.damageDefinition.damageTypes, DamageCategory.Environment);
						p.CurrentFloorDamageCooldown = cellData.damageDefinition.tickFrequency;
					}
				}
			}
			if (eventTriggerAreas != null)
			{
				for (int i = 0; i < eventTriggerAreas.Count; i++)
				{
					RoomEventTriggerArea roomEventTriggerArea = eventTriggerAreas[i];
					if (roomEventTriggerArea.triggerCells.Contains(playerCellPosition))
					{
						roomEventTriggerArea.Trigger(i);
					}
				}
			}
			if (activeEnemies != null)
			{
				for (int j = 0; j < activeEnemies.Count; j++)
				{
					if (!activeEnemies[j])
					{
						activeEnemies.RemoveAt(j);
						j--;
					}
				}
			}
			if (!HasActiveEnemies(ActiveEnemyType.RoomClear) && numberOfTimedWavesOnDeck <= 0 && (area.IsProceduralRoom || area.runtimePrototypeData.DoesUnsealOnClear()))
			{
				UnsealRoom();
			}
		}

		public void PlayerExit(PlayerController playerExiting)
		{
			OnExited(playerExiting);
		}

		public bool ContainsPosition(IntVector2 pos)
		{
			return rawRoomCells.Contains(pos);
		}

		public bool ContainsCell(CellData cell)
		{
			return roomCells.Contains(cell.position);
		}

		public virtual void OnBecameVisible(PlayerController p)
		{
			if (!m_currentlyVisible)
			{
				m_currentlyVisible = true;
				visibility = VisibilityStatus.CURRENT;
				float delay = UpdateOcclusionData(p, 1f);
				if (this.BecameVisible != null)
				{
					this.BecameVisible(delay);
				}
			}
		}

		public virtual void OnBecameInvisible(PlayerController p)
		{
			if (!m_currentlyVisible)
			{
				return;
			}
			bool flag = false;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (!GameManager.Instance.AllPlayers[i].healthHaver.IsDead && GameManager.Instance.AllPlayers[i].CurrentRoom == this)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				m_currentlyVisible = false;
				visibility = VisibilityStatus.VISITED;
				UpdateOcclusionData(0.3f, p.transform.position.IntXY(VectorConversions.Floor));
				if (this.BecameInvisible != null)
				{
					this.BecameInvisible();
				}
			}
		}

		public bool WillSealOnEntry()
		{
			List<AIActor> list = GetActiveEnemies(ActiveEnemyType.RoomClear);
			bool flag = list != null && list.Exists((AIActor a) => !a.healthHaver.IsDead);
			if (area.IsProceduralRoom)
			{
				if (flag)
				{
					return true;
				}
				return false;
			}
			if (area.runtimePrototypeData.roomEvents != null && area.runtimePrototypeData.roomEvents.Count > 0)
			{
				for (int i = 0; i < area.runtimePrototypeData.roomEvents.Count; i++)
				{
					RoomEventDefinition roomEventDefinition = area.runtimePrototypeData.roomEvents[i];
					if (roomEventDefinition.condition == RoomEventTriggerCondition.ON_ENTER && roomEventDefinition.action == RoomEventTriggerAction.SEAL_ROOM)
					{
						return true;
					}
					if (flag && roomEventDefinition.condition == RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES && roomEventDefinition.action == RoomEventTriggerAction.SEAL_ROOM)
					{
						return true;
					}
				}
			}
			return false;
		}

		protected virtual void ProcessRoomEvents(RoomEventTriggerCondition eventCondition)
		{
			if (!area.IsProceduralRoom && area.runtimePrototypeData.roomEvents != null && area.runtimePrototypeData.roomEvents.Count > 0)
			{
				for (int i = 0; i < area.runtimePrototypeData.roomEvents.Count; i++)
				{
					RoomEventDefinition roomEventDefinition = area.runtimePrototypeData.roomEvents[i];
					if (roomEventDefinition.condition == eventCondition)
					{
						HandleRoomAction(roomEventDefinition.action);
					}
				}
			}
			else if (area.IsProceduralRoom)
			{
				switch (eventCondition)
				{
				case RoomEventTriggerCondition.ON_ENTER_WITH_ENEMIES:
					HandleRoomAction(RoomEventTriggerAction.SEAL_ROOM);
					break;
				case RoomEventTriggerCondition.ON_ENEMIES_CLEARED:
					HandleRoomAction(RoomEventTriggerAction.UNSEAL_ROOM);
					break;
				}
			}
		}

		public virtual void HandleRoomAction(RoomEventTriggerAction action)
		{
			switch (action)
			{
			case RoomEventTriggerAction.SEAL_ROOM:
				SealRoom();
				break;
			case RoomEventTriggerAction.UNSEAL_ROOM:
				UnsealRoom();
				break;
			case RoomEventTriggerAction.BECOME_TERRIFYING_AND_DARK:
				BecomeTerrifyingDarkRoom();
				break;
			case RoomEventTriggerAction.END_TERRIFYING_AND_DARK:
				EndTerrifyingDarkRoom();
				break;
			default:
				BraveUtility.Log("RoomHandler received event that is triggering undefined RoomEventTriggerAction.", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
				break;
			}
		}

		protected void PreLoadReinforcements()
		{
			if (area.runtimePrototypeData == null || area.runtimePrototypeData.additionalObjectLayers == null || area.runtimePrototypeData.additionalObjectLayers.Count == 0)
			{
				return;
			}
			if (preloadedReinforcementLayerData == null)
			{
				preloadedReinforcementLayerData = new Dictionary<PrototypeRoomObjectLayer, Dictionary<PrototypePlacedObjectData, GameObject>>();
			}
			int i = 0;
			int num = 0;
			for (; i < area.runtimePrototypeData.additionalObjectLayers.Count; i++)
			{
				PrototypeRoomObjectLayer prototypeRoomObjectLayer = area.runtimePrototypeData.additionalObjectLayers[i];
				if (!prototypeRoomObjectLayer.layerIsReinforcementLayer)
				{
					continue;
				}
				List<Vector2> list = null;
				if (prototypeRoomObjectLayer.shuffle)
				{
					list = new List<Vector2>(prototypeRoomObjectLayer.placedObjectBasePositions);
					for (int num2 = list.Count - 1; num2 > 0; num2--)
					{
						int num3 = UnityEngine.Random.Range(0, num2 + 1);
						if (num2 != num3)
						{
							Vector2 value = list[num2];
							list[num2] = list[num3];
							list[num3] = value;
						}
					}
				}
				else
				{
					list = prototypeRoomObjectLayer.placedObjectBasePositions;
				}
				for (int j = 0; j < prototypeRoomObjectLayer.placedObjects.Count; j++)
				{
					if (remainingReinforcementLayers == null)
					{
						break;
					}
					if (!remainingReinforcementLayers.Contains(prototypeRoomObjectLayer))
					{
						break;
					}
					GameObject gameObject = PreloadReinforcementObject(prototypeRoomObjectLayer.placedObjects[j], list[j].ToIntVector2(), prototypeRoomObjectLayer.suppressPlayerChecks);
					if (gameObject != null)
					{
						num++;
					}
					if (!preloadedReinforcementLayerData.ContainsKey(prototypeRoomObjectLayer))
					{
						preloadedReinforcementLayerData.Add(prototypeRoomObjectLayer, new Dictionary<PrototypePlacedObjectData, GameObject>());
					}
					preloadedReinforcementLayerData[prototypeRoomObjectLayer].Add(prototypeRoomObjectLayer.placedObjects[j], gameObject);
				}
			}
		}

		protected IEnumerator HandleReinforcementPreloading()
		{
			while (Time.timeSinceLevelLoad < 1f)
			{
				yield return null;
			}
			if (area.runtimePrototypeData == null || area.runtimePrototypeData.additionalObjectLayers == null || area.runtimePrototypeData.additionalObjectLayers.Count == 0)
			{
				yield break;
			}
			if (preloadedReinforcementLayerData == null)
			{
				preloadedReinforcementLayerData = new Dictionary<PrototypeRoomObjectLayer, Dictionary<PrototypePlacedObjectData, GameObject>>();
			}
			int targetLayerIndex = 0;
			int roomIndex = Mathf.Max(0, GameManager.Instance.Dungeon.data.rooms.IndexOf(this));
			int totalRooms = GameManager.Instance.Dungeon.data.rooms.Count;
			int nonNullInstantiations = 0;
			for (; targetLayerIndex < area.runtimePrototypeData.additionalObjectLayers.Count; targetLayerIndex++)
			{
				PrototypeRoomObjectLayer currentLayer = area.runtimePrototypeData.additionalObjectLayers[targetLayerIndex];
				if (!currentLayer.layerIsReinforcementLayer)
				{
					continue;
				}
				List<Vector2> modifiedPlacedObjectPositions2 = null;
				if (currentLayer.shuffle)
				{
					modifiedPlacedObjectPositions2 = new List<Vector2>(currentLayer.placedObjectBasePositions);
					for (int num = modifiedPlacedObjectPositions2.Count - 1; num > 0; num--)
					{
						int num2 = UnityEngine.Random.Range(0, num + 1);
						if (num != num2)
						{
							Vector2 value = modifiedPlacedObjectPositions2[num];
							modifiedPlacedObjectPositions2[num] = modifiedPlacedObjectPositions2[num2];
							modifiedPlacedObjectPositions2[num2] = value;
						}
					}
				}
				else
				{
					modifiedPlacedObjectPositions2 = currentLayer.placedObjectBasePositions;
				}
				for (int objectIndex = 0; objectIndex < currentLayer.placedObjects.Count; objectIndex++)
				{
					while (Time.frameCount % totalRooms != roomIndex)
					{
						yield return null;
					}
					if (remainingReinforcementLayers == null || !remainingReinforcementLayers.Contains(currentLayer))
					{
						break;
					}
					GameObject preloadedObject = PreloadReinforcementObject(currentLayer.placedObjects[objectIndex], modifiedPlacedObjectPositions2[objectIndex].ToIntVector2(), currentLayer.suppressPlayerChecks);
					if (preloadedObject != null)
					{
						nonNullInstantiations++;
					}
					if (!preloadedReinforcementLayerData.ContainsKey(currentLayer))
					{
						preloadedReinforcementLayerData.Add(currentLayer, new Dictionary<PrototypePlacedObjectData, GameObject>());
					}
					preloadedReinforcementLayerData[currentLayer].Add(currentLayer.placedObjects[objectIndex], preloadedObject);
					yield return null;
				}
			}
		}

		public int GetEnemiesInReinforcementLayer(int index)
		{
			if (remainingReinforcementLayers == null)
			{
				return 0;
			}
			if (index >= remainingReinforcementLayers.Count)
			{
				return 0;
			}
			return remainingReinforcementLayers[index].placedObjects.Count;
		}

		public bool TriggerReinforcementLayer(int index, bool removeLayer = true, bool disableDrops = false, int specifyObjectIndex = -1, int specifyObjectCount = -1, bool instant = false)
		{
			if (remainingReinforcementLayers == null || index < 0 || index >= remainingReinforcementLayers.Count)
			{
				return false;
			}
			PrototypeRoomObjectLayer prototypeRoomObjectLayer = remainingReinforcementLayers[index];
			if (removeLayer)
			{
				remainingReinforcementLayers.RemoveAt(index);
			}
			float activeDifficultyModifier = m_activeDifficultyModifier;
			m_activeDifficultyModifier = 1f;
			int num = ((activeEnemies != null) ? activeEnemies.Count : 0);
			List<GameObject> sourceObjects = PlaceObjectsFromLayer(prototypeRoomObjectLayer.placedObjects, prototypeRoomObjectLayer, prototypeRoomObjectLayer.placedObjectBasePositions, null, !instant, prototypeRoomObjectLayer.shuffle, prototypeRoomObjectLayer.randomize, prototypeRoomObjectLayer.suppressPlayerChecks, disableDrops, specifyObjectIndex, specifyObjectCount);
			bool result = activeEnemies.Count > num;
			if (activeDifficultyModifier != 1f)
			{
				MakeRoomMoreDifficult(activeDifficultyModifier, sourceObjects);
			}
			if (GameManager.Instance.DungeonMusicController.CurrentState != DungeonFloorMusicController.DungeonMusicState.ACTIVE_SIDE_A && GameManager.Instance.DungeonMusicController.CurrentState != DungeonFloorMusicController.DungeonMusicState.ACTIVE_SIDE_B && !GameManager.Instance.DungeonMusicController.MusicOverridden)
			{
				GameManager.Instance.DungeonMusicController.NotifyRoomSuddenlyHasEnemies(this);
			}
			ResetEnemyHPPercentage();
			return result;
		}

		public void TriggerNextReinforcementLayer()
		{
			if (remainingReinforcementLayers != null && remainingReinforcementLayers.Count > 0)
			{
				TriggerReinforcementLayer(0);
			}
		}

		public void ClearReinforcementLayers()
		{
			remainingReinforcementLayers = null;
		}

		public void RegisterGoopManagerInRoom(DeadlyDeadlyGoopManager manager)
		{
			if (m_goops == null)
			{
				m_goops = new List<DeadlyDeadlyGoopManager>();
			}
			if (!m_goops.Contains(manager))
			{
				m_goops.Add(manager);
			}
		}

		public RoomHandler GetRandomDownstreamRoom()
		{
			List<RoomHandler> list = new List<RoomHandler>();
			for (int i = 0; i < connectedRooms.Count; i++)
			{
				if (connectedRooms[i].distanceFromEntrance > distanceFromEntrance)
				{
					list.Add(connectedRooms[i]);
				}
			}
			if (list.Count == 0)
			{
				return null;
			}
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		public HashSet<IntVector2> GetCellsAndAllConnectedExitsSlow(bool includeSecret = false)
		{
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>(RawCells);
			List<IntVector2> list = new List<IntVector2>();
			if (area != null && area.instanceUsedExits != null)
			{
				for (int i = 0; i < area.instanceUsedExits.Count; i++)
				{
					RuntimeRoomExitData value;
					RuntimeExitDefinition value2;
					if (!area.exitToLocalDataMap.TryGetValue(area.instanceUsedExits[i], out value) || !exitDefinitionsByExit.TryGetValue(value, out value2) || value2 == null || ((value2.downstreamRoom.IsSecretRoom || value2.upstreamRoom.IsSecretRoom) && !includeSecret))
					{
						continue;
					}
					HashSet<IntVector2> cellsForRoom = value2.GetCellsForRoom(this);
					HashSet<IntVector2> cellsForOtherRoom = value2.GetCellsForOtherRoom(this);
					foreach (IntVector2 item in cellsForRoom)
					{
						hashSet.Add(item);
					}
					foreach (IntVector2 item2 in cellsForOtherRoom)
					{
						hashSet.Add(item2);
					}
				}
			}
			DungeonData data = GameManager.Instance.BestGenerationDungeonPrefab.data;
			foreach (IntVector2 item3 in hashSet)
			{
				if (data[item3] != null && data[item3].isWallMimicHideout)
				{
					list.Add(item3);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				hashSet.Remove(list[j]);
			}
			return hashSet;
		}

		private List<Tuple<IntVector2, float>> GetGoodSpotsInternal(int dx, int dy, bool restrictive = false)
		{
			List<Tuple<IntVector2, float>> list = new List<Tuple<IntVector2, float>>();
			for (int i = 0; i < CellsWithoutExits.Count; i++)
			{
				bool flag = true;
				CellData cellData = GameManager.Instance.Dungeon.data[CellsWithoutExits[i]];
				float num = 0f;
				for (int j = 0; j < dx; j++)
				{
					for (int k = 0; k < dy; k++)
					{
						CellData cellData2 = GameManager.Instance.Dungeon.data[cellData.position + new IntVector2(j, k)];
						if (cellData2.IsTopWall())
						{
							num -= 5f;
						}
						num = ((!cellData2.HasWallNeighbor()) ? (num + 2f) : (num - 2f));
						if (GameManager.Instance.Dungeon.data[cellData.position + new IntVector2(j, k - 1)].type == CellType.PIT)
						{
							num -= 50f;
						}
						if (cellData2.type != CellType.FLOOR || cellData2.isOccupied)
						{
							flag = false;
							break;
						}
						if (restrictive && (cellData2.doesDamage || cellData2.cellVisualData.IsPhantomCarpet || cellData2.containsTrap))
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
				int num2 = Math.Abs(area.basePosition.x + area.dimensions.x - (cellData.position.x + dx / 2) - (cellData.position.x + dx / 2 - area.basePosition.x));
				int num3 = Math.Abs(area.basePosition.y + area.dimensions.y - (cellData.position.y + dy / 2) - (cellData.position.y + dy / 2 - area.basePosition.y));
				if (num2 <= 3 && num3 <= 3)
				{
					num += 10f;
				}
				else if (num2 <= 5 && num3 <= 5)
				{
					num += 5f;
				}
				if (flag)
				{
					float second = 1f + num;
					Tuple<IntVector2, float> item = new Tuple<IntVector2, float>(cellData.position, second);
					list.Add(item);
				}
			}
			return list;
		}

		public IntVector2 GetRandomVisibleClearSpot(int dx, int dy)
		{
			List<Tuple<IntVector2, float>> goodSpotsInternal = GetGoodSpotsInternal(dx, dy);
			if (goodSpotsInternal.Count == 0)
			{
				return IntVector2.Zero;
			}
			return goodSpotsInternal[UnityEngine.Random.Range(0, goodSpotsInternal.Count)].First;
		}

		public IntVector2 GetCenteredVisibleClearSpot(int dx, int dy, out bool success, bool restrictive = false)
		{
			List<Tuple<IntVector2, float>> goodSpotsInternal = GetGoodSpotsInternal(dx, dy, restrictive);
			float num = float.MinValue;
			IntVector2 result = Epicenter;
			success = false;
			for (int i = 0; i < goodSpotsInternal.Count; i++)
			{
				if (goodSpotsInternal[i].Second > num)
				{
					result = goodSpotsInternal[i].First;
					num = goodSpotsInternal[i].Second;
					success = true;
				}
			}
			return result;
		}

		public IntVector2 GetCenteredVisibleClearSpot(int dx, int dy)
		{
			bool success = false;
			return GetCenteredVisibleClearSpot(dx, dy, out success);
		}

		protected virtual void HandleBossClearReward()
		{
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT)
			{
				GameStatsManager.Instance.CurrentResRatShopSeed = UnityEngine.Random.Range(1, 1000000);
			}
			GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
			if (!PlayerHasTakenDamageInThisRoom && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
			{
				switch (tilesetId)
				{
				case GlobalDungeonData.ValidTilesets.CASTLEGEON:
					GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_CASTLE, true);
					break;
				case GlobalDungeonData.ValidTilesets.GUNGEON:
					GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_GUNGEON, true);
					break;
				case GlobalDungeonData.ValidTilesets.MINEGEON:
					GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_MINES, true);
					break;
				case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
					GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_HOLLOW, true);
					break;
				case GlobalDungeonData.ValidTilesets.FORGEGEON:
					GameStatsManager.Instance.SetFlag(GungeonFlags.ACHIEVEMENT_NOBOSSDAMAGE_FORGE, true);
					break;
				}
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON || tilesetId == GlobalDungeonData.ValidTilesets.RATGEON)
			{
				return;
			}
			for (int i = 0; i < connectedRooms.Count; i++)
			{
				if (connectedRooms[i].area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.EXIT)
				{
					connectedRooms[i].OnBecameVisible(GameManager.Instance.BestActivePlayer);
				}
			}
			IntVector2 zero = IntVector2.Zero;
			if (OverrideBossPedestalLocation.HasValue)
			{
				zero = OverrideBossPedestalLocation.Value;
			}
			else if (!area.IsProceduralRoom && area.runtimePrototypeData.rewardChestSpawnPosition != IntVector2.NegOne)
			{
				zero = area.basePosition + area.runtimePrototypeData.rewardChestSpawnPosition;
			}
			else
			{
				Debug.LogWarning("BOSS REWARD PEDESTALS SHOULD REALLY HAVE FIXED LOCATIONS! The spawn code was written by dave, so no guarantees...");
				zero = GetCenteredVisibleClearSpot(2, 2);
			}
			GameObject gameObject = GameManager.Instance.Dungeon.sharedSettingsPrefab.ChestsForBosses.SelectByWeight();
			Chest chest = gameObject.GetComponent<Chest>();
			if (GameStatsManager.Instance.IsRainbowRun)
			{
				chest = null;
			}
			if (chest != null)
			{
				Chest chest2 = Chest.Spawn(chest, zero, this);
				chest2.RegisterChestOnMinimap(this);
				return;
			}
			DungeonData data = GameManager.Instance.Dungeon.data;
			RewardPedestal component = gameObject.GetComponent<RewardPedestal>();
			if ((bool)component)
			{
				bool flag = tilesetId != GlobalDungeonData.ValidTilesets.FORGEGEON;
				bool flag2 = !PlayerHasTakenDamageInThisRoom && GameManager.Instance.Dungeon.BossMasteryTokenItemId >= 0 && !GameManager.Instance.Dungeon.HasGivenMasteryToken;
				if (flag && flag2)
				{
					zero += IntVector2.Left;
				}
				if (flag)
				{
					RewardPedestal rewardPedestal = RewardPedestal.Spawn(component, zero, this);
					rewardPedestal.IsBossRewardPedestal = true;
					rewardPedestal.lootTable.lootTable = OverrideBossRewardTable;
					rewardPedestal.RegisterChestOnMinimap(this);
					data[zero].isOccupied = true;
					data[zero + IntVector2.Right].isOccupied = true;
					data[zero + IntVector2.Up].isOccupied = true;
					data[zero + IntVector2.One].isOccupied = true;
					if (flag2)
					{
						rewardPedestal.OffsetTertiarySet = true;
					}
					if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.NumberOfLivingPlayers == 1)
					{
						rewardPedestal.ReturnCoopPlayerOnLand = true;
					}
					if (area.PrototypeRoomName == "DoubleBeholsterRoom01")
					{
						for (int j = 0; j < 8; j++)
						{
							IntVector2 centeredVisibleClearSpot = GetCenteredVisibleClearSpot(2, 2);
							RewardPedestal rewardPedestal2 = RewardPedestal.Spawn(component, centeredVisibleClearSpot, this);
							rewardPedestal2.IsBossRewardPedestal = true;
							rewardPedestal2.lootTable.lootTable = OverrideBossRewardTable;
							data[centeredVisibleClearSpot].isOccupied = true;
							data[centeredVisibleClearSpot + IntVector2.Right].isOccupied = true;
							data[centeredVisibleClearSpot + IntVector2.Up].isOccupied = true;
							data[centeredVisibleClearSpot + IntVector2.One].isOccupied = true;
						}
					}
				}
				else if (tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.NumberOfLivingPlayers == 1)
				{
					PlayerController playerController = ((!GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) ? GameManager.Instance.SecondaryPlayer : GameManager.Instance.PrimaryPlayer);
					playerController.specRigidbody.enabled = true;
					playerController.gameObject.SetActive(true);
					playerController.ResurrectFromBossKill();
				}
				if (flag2)
				{
					GameStatsManager.Instance.RegisterStatChange(TrackedStats.MASTERY_TOKENS_RECEIVED, 1f);
					GameManager.Instance.PrimaryPlayer.MasteryTokensCollectedThisRun++;
					if (flag)
					{
						zero += new IntVector2(2, 0);
					}
					RewardPedestal rewardPedestal3 = RewardPedestal.Spawn(component, zero, this);
					data[zero].isOccupied = true;
					data[zero + IntVector2.Right].isOccupied = true;
					data[zero + IntVector2.Up].isOccupied = true;
					data[zero + IntVector2.One].isOccupied = true;
					GameManager.Instance.Dungeon.HasGivenMasteryToken = true;
					rewardPedestal3.SpawnsTertiarySet = false;
					rewardPedestal3.contents = PickupObjectDatabase.GetById(GameManager.Instance.Dungeon.BossMasteryTokenItemId);
					rewardPedestal3.MimicGuid = null;
				}
			}
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CATHEDRALGEON || GameManager.Options.CurrentGameLootProfile != 0)
			{
				return;
			}
			IntVector2? randomAvailableCell = GetRandomAvailableCell(IntVector2.One * 4, CellTypes.FLOOR);
			IntVector2? intVector = ((!randomAvailableCell.HasValue) ? null : new IntVector2?(randomAvailableCell.GetValueOrDefault() + IntVector2.One));
			if (intVector.HasValue)
			{
				Chest chest3 = Chest.Spawn(GameManager.Instance.RewardManager.Synergy_Chest, intVector.Value);
				if ((bool)chest3)
				{
					chest3.RegisterChestOnMinimap(this);
				}
			}
		}

		public Chest SpawnRoomRewardChest(WeightedGameObjectCollection chestCollection, IntVector2 pos)
		{
			Chest chest = null;
			if (chestCollection != null)
			{
				GameObject gameObject = chestCollection.SelectByWeight();
				chest = Chest.Spawn(gameObject.GetComponent<Chest>(), pos, this);
			}
			else
			{
				chest = GameManager.Instance.RewardManager.SpawnRoomClearChestAt(pos);
			}
			if (chest != null)
			{
				chest.RegisterChestOnMinimap(this);
			}
			return chest;
		}

		public IntVector2 GetBestRewardLocation(IntVector2 rewardSize, RewardLocationStyle locationStyle = RewardLocationStyle.CameraCenter, bool giveChestBuffer = true)
		{
			return GetBestRewardLocation(idealPoint: (locationStyle == RewardLocationStyle.CameraCenter && !GameManager.Instance.InTutorial) ? ((Vector2)BraveUtility.ScreenCenterWorldPoint()) : ((locationStyle == RewardLocationStyle.CameraCenter && !GameManager.Instance.InTutorial) ? ((!GameManager.Instance.PrimaryPlayer.healthHaver.IsDead) ? GameManager.Instance.PrimaryPlayer.CenterPosition : ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER || GameManager.Instance.SecondaryPlayer.healthHaver.IsDead) ? ((Vector2)BraveUtility.ScreenCenterWorldPoint()) : GameManager.Instance.SecondaryPlayer.CenterPosition)) : ((locationStyle == RewardLocationStyle.PlayerCenter) ? ((!GameManager.Instance.PrimaryPlayer.healthHaver.IsDead || GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? GameManager.Instance.PrimaryPlayer.CenterPosition : GameManager.Instance.SecondaryPlayer.CenterPosition) : ((!area.IsProceduralRoom && area.runtimePrototypeData != null && area.runtimePrototypeData.rewardChestSpawnPosition != IntVector2.NegOne) ? ((area.basePosition + area.runtimePrototypeData.rewardChestSpawnPosition).ToVector2() + rewardSize.ToVector2() / 2f) : ((area.IsProceduralRoom || !(area.prototypeRoom != null) || !(area.prototypeRoom.rewardChestSpawnPosition != IntVector2.NegOne)) ? (area.basePosition.ToVector2() + area.dimensions.ToVector2() / 2f) : ((area.basePosition + area.prototypeRoom.rewardChestSpawnPosition).ToVector2() + rewardSize.ToVector2() / 2f))))), rewardSize: rewardSize, giveChestBuffer: giveChestBuffer);
		}

		public IntVector2 GetBestRewardLocation(IntVector2 rewardSize, Vector2 idealPoint, bool giveChestBuffer = true)
		{
			IntVector2[] playerPos = new IntVector2[GameManager.Instance.AllPlayers.Length];
			IntVector2[] playerDim = new IntVector2[playerPos.Length];
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PixelCollider hitboxPixelCollider = GameManager.Instance.AllPlayers[i].specRigidbody.HitboxPixelCollider;
				playerPos[i] = hitboxPixelCollider.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
				playerDim[i] = hitboxPixelCollider.UnitTopRight.ToIntVector2(VectorConversions.Floor) - playerPos[i] + IntVector2.One;
			}
			IntVector2 modifiedRewardSize = ((!giveChestBuffer) ? rewardSize : (rewardSize + new IntVector2(2, 2)));
			CellValidator cellValidator = delegate(IntVector2 c)
			{
				IntVector2 posB = ((!giveChestBuffer) ? c : (c + new IntVector2(1, 2)));
				for (int j = 0; j < playerPos.Length; j++)
				{
					if (IntVector2.AABBOverlap(playerPos[j], playerDim[j], posB, rewardSize))
					{
						return false;
					}
				}
				for (int k = 0; k < modifiedRewardSize.x; k++)
				{
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + k, c.y))
					{
						return false;
					}
					for (int l = 0; l < modifiedRewardSize.y; l++)
					{
						if (!GameManager.Instance.Dungeon.data.CheckInBounds(c.x + k, c.y + l))
						{
							return false;
						}
						if (GameManager.Instance.Dungeon.data.isWall(c.x + k, c.y + l))
						{
							return false;
						}
						CellData cellData = GameManager.Instance.Dungeon.data.cellData[c.x + k][c.y + l];
						if (cellData.containsTrap || cellData.PreventRewardSpawn)
						{
							return false;
						}
					}
				}
				return true;
			};
			IntVector2? intVector = GetNearestAvailableCell(idealPoint, modifiedRewardSize, CellTypes.FLOOR, false, cellValidator);
			if (intVector.HasValue)
			{
				if (giveChestBuffer)
				{
					intVector = intVector.Value + new IntVector2(1, 2);
				}
				return intVector.Value;
			}
			IntVector2 zero = IntVector2.Zero;
			if (!area.IsProceduralRoom && area.runtimePrototypeData != null && area.runtimePrototypeData.rewardChestSpawnPosition != IntVector2.NegOne)
			{
				return area.basePosition + area.runtimePrototypeData.rewardChestSpawnPosition;
			}
			if (!area.IsProceduralRoom && area.prototypeRoom != null && area.prototypeRoom.rewardChestSpawnPosition != IntVector2.NegOne)
			{
				return area.basePosition + area.prototypeRoom.rewardChestSpawnPosition;
			}
			return GetCenteredVisibleClearSpot(3, 2);
		}

		public virtual void HandleRoomClearReward()
		{
			if (GameManager.Instance.IsFoyer || GameManager.Instance.InTutorial || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || m_hasGivenReward || area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.REWARD)
			{
				return;
			}
			m_hasGivenReward = true;
			if (area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && area.PrototypeRoomBossSubcategory == PrototypeDungeonRoom.RoomBossSubCategory.FLOOR_BOSS)
			{
				HandleBossClearReward();
			}
			else
			{
				if (PreventStandardRoomReward)
				{
					return;
				}
				FloorRewardData currentRewardData = GameManager.Instance.RewardManager.CurrentRewardData;
				LootEngine.AmmoDropType AmmoToDrop = LootEngine.AmmoDropType.DEFAULT_AMMO;
				bool flag = LootEngine.DoAmmoClipCheck(currentRewardData, out AmmoToDrop);
				string path = ((AmmoToDrop != LootEngine.AmmoDropType.SPREAD_AMMO) ? "Ammo_Pickup" : "Ammo_Pickup_Spread");
				float value = UnityEngine.Random.value;
				float chestSystem_ChestChanceLowerBound = currentRewardData.ChestSystem_ChestChanceLowerBound;
				float num = GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.Coolness) / 100f;
				float num2 = 0f - GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.Curse) / 100f;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					num += GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.Coolness) / 100f;
					num2 -= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.Curse) / 100f;
				}
				if (PassiveItem.IsFlagSetAtAll(typeof(ChamberOfEvilItem)))
				{
					num2 *= -2f;
				}
				chestSystem_ChestChanceLowerBound = Mathf.Clamp(chestSystem_ChestChanceLowerBound + GameManager.Instance.PrimaryPlayer.AdditionalChestSpawnChance, currentRewardData.ChestSystem_ChestChanceLowerBound, currentRewardData.ChestSystem_ChestChanceUpperBound) + num + num2;
				bool flag2 = currentRewardData.SingleItemRewardTable != null;
				bool flag3 = false;
				float num3 = 0.1f;
				if (!HasGivenRoomChestRewardThisRun && MetaInjectionData.ForceEarlyChest)
				{
					flag3 = true;
				}
				if (flag3)
				{
					if (!HasGivenRoomChestRewardThisRun && (GameManager.Instance.CurrentFloor == 1 || GameManager.Instance.CurrentFloor == -1))
					{
						flag2 = false;
						chestSystem_ChestChanceLowerBound += num3;
						if ((bool)GameManager.Instance.PrimaryPlayer && GameManager.Instance.PrimaryPlayer.NumRoomsCleared > 4)
						{
							chestSystem_ChestChanceLowerBound = 1f;
						}
					}
					if (!HasGivenRoomChestRewardThisRun && distanceFromEntrance < NumberOfRoomsToPreventChestSpawning)
					{
						GameManager.Instance.Dungeon.InformRoomCleared(false, false);
						return;
					}
				}
				BraveUtility.Log("Current chest spawn chance: " + chestSystem_ChestChanceLowerBound, Color.yellow, BraveUtility.LogVerbosity.IMPORTANT);
				if (value > chestSystem_ChestChanceLowerBound)
				{
					if (flag)
					{
						LootEngine.SpawnItem(spawnPosition: GetBestRewardLocation(new IntVector2(1, 1)).ToVector3(), item: (GameObject)BraveResources.Load(path), spawnDirection: Vector2.up, force: 1f, invalidUntilGrounded: true, doDefaultItemPoof: true);
					}
					GameManager.Instance.Dungeon.InformRoomCleared(false, false);
					return;
				}
				if (flag2)
				{
					float num4 = currentRewardData.PercentOfRoomClearRewardsThatAreChests;
					if (PassiveItem.IsFlagSetAtAll(typeof(AmazingChestAheadItem)))
					{
						num4 *= 2f;
						num4 = Mathf.Max(0.5f, num4);
					}
					flag2 = UnityEngine.Random.value > num4;
				}
				if (flag2)
				{
					GameObject gameObject = null;
					float num5 = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? GameManager.Instance.RewardManager.SinglePlayerPickupIncrementModifier : GameManager.Instance.RewardManager.CoopPickupIncrementModifier);
					gameObject = ((!(UnityEngine.Random.value < 1f / num5)) ? ((!(UnityEngine.Random.value < 0.9f)) ? GameManager.Instance.RewardManager.FullHeartPrefab.gameObject : GameManager.Instance.RewardManager.HalfHeartPrefab.gameObject) : currentRewardData.SingleItemRewardTable.SelectByWeight());
					Debug.Log(gameObject.name + "SPAWNED");
					DebrisObject debrisObject = LootEngine.SpawnItem(gameObject, GetBestRewardLocation(new IntVector2(1, 1)).ToVector3() + new Vector3(0.25f, 0f, 0f), Vector2.up, 1f, true, true);
					Exploder.DoRadialPush(debrisObject.sprite.WorldCenter.ToVector3ZUp(debrisObject.sprite.WorldCenter.y), 8f, 3f);
					AkSoundEngine.PostEvent("Play_OBJ_item_spawn_01", debrisObject.gameObject);
					GameManager.Instance.Dungeon.InformRoomCleared(true, false);
				}
				else
				{
					IntVector2 bestRewardLocation = GetBestRewardLocation(new IntVector2(2, 1));
					if (GameStatsManager.Instance.IsRainbowRun)
					{
						LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteChest, bestRewardLocation.ToCenterVector2(), this, true);
						HasGivenRoomChestRewardThisRun = true;
					}
					else
					{
						Chest chest = SpawnRoomRewardChest(null, bestRewardLocation);
						if ((bool)chest)
						{
							HasGivenRoomChestRewardThisRun = true;
						}
					}
					GameManager.Instance.Dungeon.InformRoomCleared(true, true);
				}
				if (flag)
				{
					LootEngine.DelayedSpawnItem(spawnPosition: GetBestRewardLocation(new IntVector2(1, 1)).ToVector3() + new Vector3(0.25f, 0f, 0f), delay: 1f, item: (GameObject)BraveResources.Load(path), spawnDirection: Vector2.up, force: 1f, invalidUntilGrounded: true, doDefaultItemPoof: true);
				}
			}
		}

		protected virtual void NotifyPlayerRoomCleared()
		{
			if (!(GameManager.Instance == null) && !(GameManager.Instance.PrimaryPlayer == null))
			{
				GameManager.Instance.PrimaryPlayer.OnRoomCleared();
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)GameManager.Instance.SecondaryPlayer)
				{
					GameManager.Instance.SecondaryPlayer.OnRoomCleared();
				}
			}
		}

		public void AssignRoomVisualType(int type, bool respectPrototypeRooms = false)
		{
			if (!respectPrototypeRooms || area == null || !(area.prototypeRoom != null) || area.prototypeRoom.overrideRoomVisualType <= -1 || area.prototypeRoom.overrideRoomVisualTypeForSecretRooms)
			{
				RoomVisualSubtype = type;
			}
		}

		public void CalculateOpulence()
		{
			if (area.prototypeRoom != null && (area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.BOSS || area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.REWARD || area.prototypeRoom.category == PrototypeDungeonRoom.RoomCategory.SPECIAL))
			{
				opulence++;
			}
			if (distanceFromEntrance > 12)
			{
				opulence++;
			}
		}

		public RoomEventTriggerArea GetEventTriggerAreaFromObject(IEventTriggerable triggerable)
		{
			for (int i = 0; i < eventTriggerAreas.Count; i++)
			{
				RoomEventTriggerArea roomEventTriggerArea = eventTriggerAreas[i];
				if (roomEventTriggerArea.events.Contains(triggerable))
				{
					return roomEventTriggerArea;
				}
			}
			return null;
		}

		public void RegisterConnectedRoom(RoomHandler other, RuntimeRoomExitData usedExit)
		{
			area.instanceUsedExits.Add(usedExit.referencedExit);
			area.exitToLocalDataMap.Add(usedExit.referencedExit, usedExit);
			connectedRooms.Add(other);
			connectedRoomsByExit.Add(usedExit.referencedExit, other);
		}

		public void DeregisterConnectedRoom(RoomHandler other, RuntimeRoomExitData usedExit)
		{
			area.instanceUsedExits.Remove(usedExit.referencedExit);
			area.exitToLocalDataMap.Remove(usedExit.referencedExit);
			connectedRooms.Remove(other);
			connectedRoomsByExit.Remove(usedExit.referencedExit);
		}

		public DungeonData.Direction GetDirectionToConnectedRoom(RoomHandler other)
		{
			if (area.IsProceduralRoom)
			{
				PrototypeRoomExit exitConnectedToRoom = other.GetExitConnectedToRoom(this);
				return (DungeonData.Direction)((int)(exitConnectedToRoom.exitDirection + 4) % 8);
			}
			PrototypeRoomExit exitConnectedToRoom2 = GetExitConnectedToRoom(other);
			return exitConnectedToRoom2.exitDirection;
		}

		public void TransferInteractableOwnershipToDungeon(IPlayerInteractable ixable)
		{
			DeregisterInteractable(ixable);
			unassignedInteractableObjects.Remove(ixable);
			unassignedInteractableObjects.Add(ixable);
		}

		public void RegisterInteractable(IPlayerInteractable ixable)
		{
			if (!interactableObjects.Contains(ixable))
			{
				interactableObjects.Add(ixable);
			}
		}

		public bool IsRegistered(IPlayerInteractable ixable)
		{
			return interactableObjects.Contains(ixable);
		}

		public void DeregisterInteractable(IPlayerInteractable ixable)
		{
			if (interactableObjects.Contains(ixable))
			{
				interactableObjects.Remove(ixable);
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					GameManager.Instance.AllPlayers[i].RemoveBrokenInteractable(ixable);
				}
			}
			else
			{
				Debug.LogError("Deregistering an unregistered interactable. Talk to Brent.");
			}
		}

		public void RegisterAutoAimTarget(IAutoAimTarget target)
		{
			if (!autoAimTargets.Contains(target))
			{
				autoAimTargets.Add(target);
			}
		}

		public List<IAutoAimTarget> GetAutoAimTargets()
		{
			return autoAimTargets;
		}

		public void DeregisterAutoAimTarget(IAutoAimTarget target)
		{
			if (autoAimTargets.Contains(target))
			{
				autoAimTargets.Remove(target);
			}
		}

		public List<T> GetComponentsInRoom<T>() where T : Behaviour
		{
			T[] array = UnityEngine.Object.FindObjectsOfType<T>();
			List<T> list = new List<T>();
			for (int i = 0; i < array.Length; i++)
			{
				if (GameManager.Instance.Dungeon.GetRoomFromPosition(array[i].transform.position.IntXY(VectorConversions.Floor)) == this)
				{
					list.Add(array[i]);
				}
			}
			return list;
		}

		public List<T> GetComponentsAbsoluteInRoom<T>() where T : Behaviour
		{
			T[] array = UnityEngine.Object.FindObjectsOfType<T>();
			List<T> list = new List<T>();
			for (int i = 0; i < array.Length; i++)
			{
				if (GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(array[i].transform.position.IntXY(VectorConversions.Floor)) == this)
				{
					list.Add(array[i]);
				}
			}
			return list;
		}

		public void MakeRoomMoreDifficult(float difficultyMultiplier, List<GameObject> sourceObjects = null)
		{
			if (activeEnemies == null || activeEnemies.Count == 0)
			{
				return;
			}
			m_activeDifficultyModifier *= difficultyMultiplier;
			if (!(difficultyMultiplier > 1f))
			{
				return;
			}
			List<AIActor> list = null;
			if (sourceObjects != null)
			{
				list = new List<AIActor>();
				for (int i = 0; i < sourceObjects.Count; i++)
				{
					AIActor component = sourceObjects[i].GetComponent<AIActor>();
					if ((bool)component)
					{
						list.Add(component);
					}
				}
			}
			else
			{
				list = new List<AIActor>(activeEnemies);
			}
			list = list.Shuffle();
			int num = Mathf.FloorToInt((float)list.Count * difficultyMultiplier);
			num -= list.Count;
			for (int j = 0; j < num; j++)
			{
				AIActor enemyToDuplicate = list[j % list.Count];
				IntVector2? targetCenter = null;
				if ((bool)enemyToDuplicate.TargetRigidbody)
				{
					targetCenter = enemyToDuplicate.TargetRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
				}
				CellValidator cellValidator = delegate(IntVector2 c)
				{
					for (int k = 0; k < enemyToDuplicate.Clearance.x; k++)
					{
						for (int l = 0; l < enemyToDuplicate.Clearance.y; l++)
						{
							if (GameManager.Instance.Dungeon.data.isTopWall(c.x + k, c.y + l))
							{
								return false;
							}
							if (targetCenter.HasValue && IntVector2.DistanceSquared(targetCenter.Value, c.x + k, c.y + l) < 16f)
							{
								return false;
							}
						}
					}
					return true;
				};
				IntVector2? randomAvailableCell = GetRandomAvailableCell(enemyToDuplicate.Clearance, enemyToDuplicate.PathableTiles, false, cellValidator);
				if (randomAvailableCell.HasValue)
				{
					AIActor aIActor = AIActor.Spawn(EnemyDatabase.GetOrLoadByGuid(enemyToDuplicate.EnemyGuid), randomAvailableCell.Value, this, true, AIActor.AwakenAnimationType.Default, false);
					if (GameManager.Instance.BestActivePlayer.CurrentRoom == this)
					{
						aIActor.HandleReinforcementFallIntoRoom();
					}
				}
			}
		}

		public virtual void WriteRoomData(DungeonData data)
		{
			if (area.prototypeRoom != null)
			{
				MakePredefinedRoom();
				StampAdditionalAppearanceData();
			}
			else if (area.proceduralCells != null)
			{
				MakeCustomProceduralRoom();
			}
			else
			{
				BraveUtility.Log("STAMPING RECTANGULAR ROOM", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
				RobotDaveIdea idea = ((!GameManager.Instance.Dungeon.UsesCustomFloorIdea) ? GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultProceduralIdea : GameManager.Instance.Dungeon.FloorIdea);
				PrototypeDungeonRoom prototypeRoom = RobotDave.RuntimeProcessIdea(idea, area.dimensions);
				area.prototypeRoom = prototypeRoom;
				MakePredefinedRoom();
				area.prototypeRoom = null;
				area.IsProceduralRoom = true;
			}
			DefineRoomBorderCells();
			cameraBoundingPolygon = new RoomHandlerBoundingPolygon(GetPolygonDecomposition(), GameManager.Instance.MainCameraController.controllerCamera.VisibleBorder);
			cameraBoundingRect = GetBoundingRect();
			if (area.prototypeRoom != null && area.prototypeRoom.associatedMinimapIcon != null)
			{
				Minimap.Instance.RegisterRoomIcon(this, area.prototypeRoom.associatedMinimapIcon);
			}
		}

		private void PreprocessVisualData()
		{
			if (area.prototypeRoom == null && area.proceduralCells != null)
			{
				return;
			}
			DungeonMaterial dungeonMaterial = GameManager.Instance.Dungeon.roomMaterialDefinitions[RoomVisualSubtype];
			if (!dungeonMaterial.usesInternalMaterialTransitions)
			{
				return;
			}
			int num = UnityEngine.Random.Range(0, 5);
			for (int i = 0; i < num; i++)
			{
				int num2 = area.basePosition.x + UnityEngine.Random.Range(0, area.dimensions.x - 3);
				int num3 = area.basePosition.y + UnityEngine.Random.Range(0, area.dimensions.y - 3);
				int num4 = UnityEngine.Random.Range(3, area.dimensions.x - (num2 - area.basePosition.x));
				int num5 = UnityEngine.Random.Range(3, area.dimensions.y - (num3 - area.basePosition.y));
				for (int j = num2; j < num2 + num4; j++)
				{
					for (int k = num3; k < num3 + num5; k++)
					{
						CellData cellData = GameManager.Instance.Dungeon.data[j, k];
						if (cellData.type != CellType.WALL && !cellData.IsTopWall())
						{
							cellData.cellVisualData.roomVisualTypeIndex = dungeonMaterial.internalMaterialTransitions[0].materialTransition;
						}
					}
				}
			}
		}

		public void PostGenerationCleanup()
		{
			if (area.IsProceduralRoom)
			{
				return;
			}
			if (area.prototypeRoom != null)
			{
				area.runtimePrototypeData = new RuntimePrototypeRoomData(area.prototypeRoom);
				if (!area.runtimePrototypeData.usesCustomAmbient)
				{
					area.runtimePrototypeData.usesCustomAmbient = true;
					area.runtimePrototypeData.usesDifferentCustomAmbientLowQuality = true;
					area.runtimePrototypeData.customAmbient = Color.Lerp(GameManager.Instance.Dungeon.decoSettings.ambientLightColor, GameManager.Instance.Dungeon.decoSettings.ambientLightColorTwo, UnityEngine.Random.value);
					area.runtimePrototypeData.customAmbientLowQuality = Color.Lerp(GameManager.Instance.Dungeon.decoSettings.lowQualityAmbientLightColor, GameManager.Instance.Dungeon.decoSettings.lowQualityAmbientLightColorTwo, UnityEngine.Random.value);
				}
				PreLoadReinforcements();
				area.prototypeRoom = null;
			}
			else if (area.runtimePrototypeData == null)
			{
				area.IsProceduralRoom = true;
			}
		}

		private void DefineRoomBorderCells()
		{
			HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
			DungeonData data = GameManager.Instance.Dungeon.data;
			IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
			foreach (IntVector2 roomCellsWithoutExit in roomCellsWithoutExits)
			{
				CellData cellData = data[roomCellsWithoutExit];
				cellData.nearestRoom = this;
				cellData.distanceFromNearestRoom = 0f;
				data[cellData.position + IntVector2.Up].nearestRoom = this;
				data[cellData.position + IntVector2.Up].distanceFromNearestRoom = 0f;
				data[cellData.position + IntVector2.Up * 2].nearestRoom = this;
				data[cellData.position + IntVector2.Up * 2].distanceFromNearestRoom = 0f;
				for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
				{
					if (!data.CheckInBounds(cellData.position + cardinalsAndOrdinals[i]))
					{
						continue;
					}
					CellData cellData2 = data[cellData.position + cardinalsAndOrdinals[i]];
					if (cellData2 != null)
					{
						if (i == 0 || i == 1 || i == 7)
						{
							cellData2 = data[cellData2.position + IntVector2.Up * 2];
						}
						if (cellData2.type == CellType.WALL || cellData2.isExitCell)
						{
							cellData2.distanceFromNearestRoom = 1f;
							cellData2.nearestRoom = this;
							hashSet.Add(cellData2.position);
						}
					}
				}
			}
			DefineEpicenter(hashSet);
		}

		private void DebugDrawCross(Vector3 centerPoint, Color crosscolor)
		{
			Debug.DrawLine(centerPoint + new Vector3(-0.5f, 0f, 0f), centerPoint + new Vector3(0.5f, 0f, 0f), crosscolor, 1000f);
			Debug.DrawLine(centerPoint + new Vector3(0f, -0.5f, 0f), centerPoint + new Vector3(0f, 0.5f, 0f), crosscolor, 1000f);
		}

		private float UpdateOcclusionData(PlayerController p, float visibility, bool useFloodFill = true)
		{
			IntVector2 startingPosition = ((!(p != null)) ? GameManager.Instance.MainCameraController.Camera.transform.position.IntXY(VectorConversions.Floor) : p.transform.position.IntXY(VectorConversions.Floor));
			return Pixelator.Instance.ProcessOcclusionChange(startingPosition, visibility, this, useFloodFill);
		}

		private float UpdateOcclusionData(float visibility, IntVector2 startPosition, bool useFloodFill = true)
		{
			return Pixelator.Instance.ProcessOcclusionChange(startPosition, visibility, this, useFloodFill);
		}

		public AIActor GetToughestEnemy()
		{
			AIActor result = null;
			float num = 0f;
			if (activeEnemies != null)
			{
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if ((bool)activeEnemies[i] && activeEnemies[i].IsNormalEnemy && (bool)activeEnemies[i].healthHaver && !activeEnemies[i].healthHaver.IsBoss)
					{
						float num2 = activeEnemies[i].healthHaver.GetMaxHealth() + (float)(activeEnemies[i].IsSignatureEnemy ? 1000 : 0);
						if (num2 > num)
						{
							result = activeEnemies[i];
							num = num2;
						}
					}
				}
			}
			return result;
		}

		public bool AddMysteriousBulletManToRoom()
		{
			if (GameStatsManager.Instance.AnyPastBeaten())
			{
				DungeonPlaceable dungeonPlaceable = BraveResources.Load("MysteriousBullet", ".asset") as DungeonPlaceable;
				if (dungeonPlaceable == null)
				{
					return false;
				}
				CellValidator cellValidator = (IntVector2 a) => !GameManager.Instance.Dungeon.data[a].IsTopWall();
				IntVector2? randomAvailableCell = GetRandomAvailableCell(new IntVector2(2, 2), CellTypes.FLOOR, false, cellValidator);
				if (randomAvailableCell.HasValue)
				{
					dungeonPlaceable.InstantiateObject(this, randomAvailableCell.Value - area.basePosition);
					return true;
				}
			}
			return false;
		}

		public void AddSpecificEnemyToRoomProcedurally(string enemyGuid, bool reinforcementSpawn = false, Vector2? goalPosition = null)
		{
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(enemyGuid);
			IntVector2 clearance = orLoadByGuid.specRigidbody.UnitDimensions.ToIntVector2(VectorConversions.Ceil);
			CellValidator cellValidator = delegate(IntVector2 c)
			{
				for (int i = 0; i < clearance.x; i++)
				{
					int x = c.x + i;
					for (int j = 0; j < clearance.y; j++)
					{
						int y = c.y + j;
						if (GameManager.Instance.Dungeon.data.isTopWall(x, y))
						{
							return false;
						}
					}
				}
				return true;
			};
			IntVector2? intVector = ((!goalPosition.HasValue) ? GetRandomAvailableCell(clearance, CellTypes.FLOOR, false, cellValidator) : GetNearestAvailableCell(goalPosition.Value, clearance, CellTypes.FLOOR, false, cellValidator));
			if (intVector.HasValue)
			{
				AIActor aIActor = AIActor.Spawn(orLoadByGuid, intVector.Value, this, true, AIActor.AwakenAnimationType.Spawn, false);
				if ((bool)aIActor && reinforcementSpawn)
				{
					if ((bool)aIActor.specRigidbody)
					{
						aIActor.specRigidbody.CollideWithOthers = false;
					}
					aIActor.HandleReinforcementFallIntoRoom();
				}
			}
			else
			{
				Debug.LogError("failed placement");
			}
		}

		private void MakeCustomProceduralRoom()
		{
			for (int i = 0; i < area.proceduralCells.Count; i++)
			{
				IntVector2 intVector = area.basePosition + area.proceduralCells[i];
				bool flag = StampCellComplex(intVector.x, intVector.y, CellType.FLOOR, DiagonalWallType.NONE);
			}
			if (GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON)
			{
				AssignRoomVisualType(3);
				DungeonData data = GameManager.Instance.BestGenerationDungeonPrefab.data;
				for (int j = 0; j < area.proceduralCells.Count; j++)
				{
					IntVector2 intVector2 = area.basePosition + area.proceduralCells[j];
					HandleStampedCellVisualData(intVector2.x, intVector2.y, null);
				}
			}
		}

		private GameObject PreloadReinforcementObject(PrototypePlacedObjectData objectData, IntVector2 pos, bool suppressPlayerChecks = false)
		{
			if (objectData.spawnChance < 1f && UnityEngine.Random.value > objectData.spawnChance)
			{
				return null;
			}
			if (objectData.instancePrerequisites != null && objectData.instancePrerequisites.Length > 0)
			{
				bool flag = true;
				for (int i = 0; i < objectData.instancePrerequisites.Length; i++)
				{
					if (!objectData.instancePrerequisites[i].CheckConditionsFulfilled())
					{
						flag = false;
					}
				}
				if (!flag)
				{
					return null;
				}
			}
			GameObject gameObject = null;
			if (objectData.placeableContents != null)
			{
				gameObject = objectData.placeableContents.InstantiateObject(this, pos, false, true);
			}
			if (objectData.nonenemyBehaviour != null)
			{
				gameObject = objectData.nonenemyBehaviour.InstantiateObject(this, pos, true);
				gameObject.GetComponent<DungeonPlaceableBehaviour>().PlacedPosition = pos + area.basePosition;
			}
			if (!string.IsNullOrEmpty(objectData.enemyBehaviourGuid))
			{
				AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(objectData.enemyBehaviourGuid);
				gameObject = orLoadByGuid.InstantiateObject(this, pos, true);
			}
			if (gameObject != null)
			{
				AIActor component = gameObject.GetComponent<AIActor>();
				if ((bool)component)
				{
					component.IsInReinforcementLayer = true;
					if (suppressPlayerChecks)
					{
						component.HasDonePlayerEnterCheck = true;
					}
					if ((bool)component.healthHaver && component.healthHaver.IsBoss)
					{
						component.HasDonePlayerEnterCheck = true;
					}
					component.PlacedPosition = pos + area.basePosition;
				}
				HandleFields(objectData, gameObject);
				gameObject.transform.parent = hierarchyParent;
				gameObject.SetActive(false);
			}
			return gameObject;
		}

		public void HandleFields(PrototypePlacedObjectData objectData, GameObject instantiatedObject)
		{
			if (!(objectData.nonenemyBehaviour != null) && string.IsNullOrEmpty(objectData.enemyBehaviourGuid))
			{
				return;
			}
			object[] components = instantiatedObject.GetComponents<IHasDwarfConfigurables>();
			bool flag = false;
			for (int i = 0; i < components.Length; i++)
			{
				if (flag)
				{
					continue;
				}
				object obj = components[i];
				Type type = obj.GetType();
				for (int j = 0; j < objectData.fieldData.Count; j++)
				{
					FieldInfo field = type.GetField(objectData.fieldData[j].fieldName);
					if (field == null)
					{
						continue;
					}
					flag = true;
					if (objectData.fieldData[j].fieldType == PrototypePlacedObjectFieldData.FieldType.FLOAT)
					{
						if (field.FieldType == typeof(int))
						{
							float floatValue = objectData.fieldData[j].floatValue;
							int num = (int)floatValue;
							field.SetValue(obj, num);
						}
						else
						{
							field.SetValue(obj, objectData.fieldData[j].floatValue);
						}
					}
					else
					{
						field.SetValue(obj, objectData.fieldData[j].boolValue);
					}
				}
				if (obj is ConveyorBelt)
				{
					(obj as ConveyorBelt).PostFieldConfiguration(this);
				}
			}
		}

		private void ForceConfigure(GameObject instantiated)
		{
			Component[] componentsInChildren = instantiated.GetComponentsInChildren(typeof(IPlaceConfigurable));
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				IPlaceConfigurable placeConfigurable = componentsInChildren[i] as IPlaceConfigurable;
				if (placeConfigurable != null)
				{
					placeConfigurable.ConfigureOnPlacement(this);
				}
			}
		}

		private List<GameObject> PlaceObjectsFromLayer(List<PrototypePlacedObjectData> placedObjectList, PrototypeRoomObjectLayer sourceLayer, List<Vector2> placedObjectPositions, Dictionary<int, RoomEventTriggerArea> eventTriggerMap, bool spawnPoofs = false, bool shuffleSpawns = false, int randomizeSpawns = 0, bool suppressPlayerChecks = false, bool disableDrops = false, int specifyObjectIndex = -1, int specifyObjectCount = -1)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			List<Vector2> list;
			if (shuffleSpawns)
			{
				list = new List<Vector2>(placedObjectPositions);
				for (int num = list.Count - 1; num > 0; num--)
				{
					int num2 = UnityEngine.Random.Range(0, num + 1);
					if (num != num2)
					{
						Vector2 value = list[num];
						list[num] = list[num2];
						list[num2] = value;
					}
				}
			}
			else
			{
				list = placedObjectPositions;
			}
			List<GameObject> list2 = new List<GameObject>();
			Dictionary<PrototypePlacedObjectData, GameObject> dictionary = null;
			if (sourceLayer != null && preloadedReinforcementLayerData != null && preloadedReinforcementLayerData.ContainsKey(sourceLayer))
			{
				dictionary = preloadedReinforcementLayerData[sourceLayer];
			}
			int num3 = 0;
			for (int i = 0; i < placedObjectList.Count; i++)
			{
				if (specifyObjectIndex >= 0 && i < specifyObjectIndex)
				{
					continue;
				}
				if (specifyObjectCount >= 0)
				{
					if (num3 >= specifyObjectCount)
					{
						break;
					}
					num3++;
				}
				PrototypePlacedObjectData prototypePlacedObjectData = placedObjectList[i];
				GameObject gameObject = null;
				if (dictionary != null && dictionary.ContainsKey(prototypePlacedObjectData))
				{
					gameObject = dictionary[prototypePlacedObjectData];
					if (gameObject == null)
					{
						continue;
					}
				}
				if (gameObject == null)
				{
					if (prototypePlacedObjectData.spawnChance < 1f && UnityEngine.Random.value > prototypePlacedObjectData.spawnChance)
					{
						continue;
					}
					if (prototypePlacedObjectData.instancePrerequisites != null && prototypePlacedObjectData.instancePrerequisites.Length > 0)
					{
						bool flag = true;
						for (int j = 0; j < prototypePlacedObjectData.instancePrerequisites.Length; j++)
						{
							if (!prototypePlacedObjectData.instancePrerequisites[j].CheckConditionsFulfilled())
							{
								flag = false;
							}
						}
						if (!flag)
						{
							continue;
						}
					}
				}
				GameObject gameObject2 = null;
				IntVector2 instantiatedDimensions = IntVector2.Zero;
				if (i >= list.Count)
				{
					Debug.LogError("i > modifiedPlacedObjectPositions.Count, this is very bad!");
				}
				IntVector2 intVector = list[i].ToIntVector2();
				bool flag2 = true;
				if (gameObject != null)
				{
					AIActor component = gameObject.GetComponent<AIActor>();
					intVector = ((!component) ? (gameObject.transform.position.IntXY() - area.basePosition) : (component.PlacedPosition - area.basePosition));
					gameObject2 = gameObject;
					gameObject2.SetActive(true);
					if (prototypePlacedObjectData.placeableContents != null)
					{
						DungeonPlaceable placeableContents = prototypePlacedObjectData.placeableContents;
						instantiatedDimensions = new IntVector2(placeableContents.width, placeableContents.height);
						flag2 = placeableContents.isPassable;
					}
					if (prototypePlacedObjectData.nonenemyBehaviour != null)
					{
						DungeonPlaceableBehaviour nonenemyBehaviour = prototypePlacedObjectData.nonenemyBehaviour;
						instantiatedDimensions = new IntVector2(nonenemyBehaviour.placeableWidth, nonenemyBehaviour.placeableHeight);
						flag2 = nonenemyBehaviour.isPassable;
					}
					if (!string.IsNullOrEmpty(prototypePlacedObjectData.enemyBehaviourGuid))
					{
						DungeonPlaceableBehaviour orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(prototypePlacedObjectData.enemyBehaviourGuid);
						instantiatedDimensions = new IntVector2(orLoadByGuid.placeableWidth, orLoadByGuid.placeableHeight);
						flag2 = orLoadByGuid.isPassable;
					}
					ForceConfigure(gameObject2);
				}
				else
				{
					if (prototypePlacedObjectData.placeableContents != null)
					{
						DungeonPlaceable placeableContents2 = prototypePlacedObjectData.placeableContents;
						instantiatedDimensions = new IntVector2(placeableContents2.width, placeableContents2.height);
						flag2 = placeableContents2.isPassable;
						gameObject2 = prototypePlacedObjectData.placeableContents.InstantiateObject(this, intVector);
					}
					if (prototypePlacedObjectData.nonenemyBehaviour != null)
					{
						DungeonPlaceableBehaviour nonenemyBehaviour2 = prototypePlacedObjectData.nonenemyBehaviour;
						instantiatedDimensions = new IntVector2(nonenemyBehaviour2.placeableWidth, nonenemyBehaviour2.placeableHeight);
						flag2 = nonenemyBehaviour2.isPassable;
						gameObject2 = prototypePlacedObjectData.nonenemyBehaviour.InstantiateObject(this, intVector);
						gameObject2.GetComponent<DungeonPlaceableBehaviour>().PlacedPosition = intVector + area.basePosition;
					}
					if (!string.IsNullOrEmpty(prototypePlacedObjectData.enemyBehaviourGuid))
					{
						AIActor orLoadByGuid2 = EnemyDatabase.GetOrLoadByGuid(prototypePlacedObjectData.enemyBehaviourGuid);
						if (orLoadByGuid2 == null)
						{
							Debug.LogError(prototypePlacedObjectData.enemyBehaviourGuid + "|" + area.prototypeRoom.name);
						}
						instantiatedDimensions = new IntVector2(orLoadByGuid2.placeableWidth, orLoadByGuid2.placeableHeight);
						flag2 = orLoadByGuid2.isPassable;
						gameObject2 = orLoadByGuid2.InstantiateObject(this, intVector);
					}
				}
				if (!(gameObject2 != null))
				{
					continue;
				}
				AIActor component2 = gameObject2.GetComponent<AIActor>();
				if ((bool)component2)
				{
					if (suppressPlayerChecks)
					{
						component2.HasDonePlayerEnterCheck = true;
					}
					if ((bool)component2.healthHaver && component2.healthHaver.IsBoss)
					{
						component2.HasDonePlayerEnterCheck = true;
					}
					component2.PlacedPosition = intVector + area.basePosition;
					if ((bool)component2.specRigidbody)
					{
						component2.specRigidbody.Initialize();
						instantiatedDimensions = component2.Clearance;
					}
				}
				list2.Add(gameObject2);
				AIActor component3 = gameObject2.GetComponent<AIActor>();
				if (disableDrops && (bool)component3)
				{
					component3.CanDropCurrency = false;
					component3.CanDropItems = false;
				}
				if (randomizeSpawns > 0)
				{
					float sqrMinDist = 8f;
					Vector2 playerPosition = GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter;
					IntVector2 truePlaceablePosition = intVector + area.basePosition;
					CellValidator cellValidator = delegate(IntVector2 c)
					{
						if (GameManager.Instance.Dungeon.data[c + IntVector2.Down] != null && GameManager.Instance.Dungeon.data[c + IntVector2.Down].isExitCell)
						{
							return false;
						}
						if (c.x < truePlaceablePosition.x - randomizeSpawns || c.x > truePlaceablePosition.x + randomizeSpawns || c.y < truePlaceablePosition.y - randomizeSpawns || c.y > truePlaceablePosition.y + randomizeSpawns)
						{
							return false;
						}
						if ((playerPosition - Pathfinder.GetClearanceOffset(c, instantiatedDimensions)).sqrMagnitude <= sqrMinDist)
						{
							return false;
						}
						for (int num4 = 0; num4 < instantiatedDimensions.x; num4++)
						{
							for (int num5 = 0; num5 < instantiatedDimensions.y; num5++)
							{
								if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(new IntVector2(c.x + num4, c.y + num5)))
								{
									return false;
								}
								if (GameManager.Instance.Dungeon.data.isTopWall(c.x + num4, c.y + num5))
								{
									return false;
								}
								if (!GameManager.Instance.Dungeon.data[c.x + num4, c.y + num5].isGridConnected)
								{
									return false;
								}
							}
						}
						return true;
					};
					CellTypes value2 = CellTypes.FLOOR;
					if ((bool)component3)
					{
						value2 = component3.PathableTiles;
					}
					IntVector2? randomAvailableCell = GetRandomAvailableCell(instantiatedDimensions, value2, false, cellValidator);
					if (randomAvailableCell.HasValue)
					{
						gameObject2.transform.position += (randomAvailableCell.Value - truePlaceablePosition).ToVector3();
						if (gameObject != null)
						{
							SpeculativeRigidbody component4 = gameObject2.GetComponent<SpeculativeRigidbody>();
							if ((bool)component4)
							{
								component4.Reinitialize();
							}
						}
					}
				}
				if (spawnPoofs)
				{
					AIActor component5 = gameObject2.GetComponent<AIActor>();
					if (component5 != null)
					{
						float delay = 0f;
						if (sourceLayer != null && specifyObjectIndex == -1 && specifyObjectIndex == -1)
						{
							delay = 0.25f * (float)i;
						}
						component5.HandleReinforcementFallIntoRoom(delay);
					}
				}
				if (prototypePlacedObjectData.xMPxOffset != 0 || prototypePlacedObjectData.yMPxOffset != 0)
				{
					Vector2 vector = new Vector2((float)prototypePlacedObjectData.xMPxOffset * 0.0625f, (float)prototypePlacedObjectData.yMPxOffset * 0.0625f);
					gameObject2.transform.position = gameObject2.transform.position + vector.ToVector3ZUp();
					SpeculativeRigidbody componentInChildren = gameObject2.GetComponentInChildren<SpeculativeRigidbody>();
					if ((bool)componentInChildren)
					{
						componentInChildren.Reinitialize();
					}
				}
				for (int k = 0; k < instantiatedDimensions.x; k++)
				{
					for (int l = 0; l < instantiatedDimensions.y; l++)
					{
						IntVector2 vec = new IntVector2(area.basePosition.x + intVector.x + k, area.basePosition.y + intVector.y + l);
						if (data.CheckInBoundsAndValid(vec))
						{
							CellData cellData = data.cellData[vec.x][vec.y];
							cellData.isOccupied = !flag2;
						}
					}
				}
				IPlayerInteractable[] interfacesInChildren = gameObject2.GetInterfacesInChildren<IPlayerInteractable>();
				for (int m = 0; m < interfacesInChildren.Length; m++)
				{
					interactableObjects.Add(interfacesInChildren[m]);
				}
				SurfaceDecorator component6 = gameObject2.GetComponent<SurfaceDecorator>();
				if (component6 != null)
				{
					component6.Decorate(this);
				}
				if (gameObject == null)
				{
					HandleFields(prototypePlacedObjectData, gameObject2);
					gameObject2.transform.parent = hierarchyParent;
				}
				if (prototypePlacedObjectData.linkedTriggerAreaIDs != null && prototypePlacedObjectData.linkedTriggerAreaIDs.Count > 0)
				{
					for (int n = 0; n < prototypePlacedObjectData.linkedTriggerAreaIDs.Count; n++)
					{
						int key = prototypePlacedObjectData.linkedTriggerAreaIDs[n];
						if (eventTriggerMap != null && eventTriggerMap.ContainsKey(key))
						{
							eventTriggerMap[key].AddGameObject(gameObject2);
						}
					}
				}
				if (prototypePlacedObjectData.assignedPathIDx != -1)
				{
					PathMover component7 = gameObject2.GetComponent<PathMover>();
					if (component7 != null && area.prototypeRoom.paths.Count > prototypePlacedObjectData.assignedPathIDx && prototypePlacedObjectData.assignedPathIDx >= 0)
					{
						component7.Path = area.prototypeRoom.paths[prototypePlacedObjectData.assignedPathIDx];
						component7.PathStartNode = prototypePlacedObjectData.assignedPathStartNode;
						component7.RoomHandler = this;
					}
				}
			}
			if (sourceLayer != null && preloadedReinforcementLayerData != null && preloadedReinforcementLayerData.ContainsKey(sourceLayer))
			{
				preloadedReinforcementLayerData.Remove(sourceLayer);
			}
			return list2;
		}

		public void AddDarkSoulsRoomResetDependency(RoomHandler room)
		{
			if (DarkSoulsRoomResetDependencies == null)
			{
				DarkSoulsRoomResetDependencies = new List<RoomHandler>();
			}
			if (!DarkSoulsRoomResetDependencies.Contains(room))
			{
				DarkSoulsRoomResetDependencies.Add(room);
			}
		}

		public bool CanBeEscaped()
		{
			return true;
		}

		public void ResetPredefinedRoomLikeDarkSouls()
		{
			if (GameManager.Instance.PrimaryPlayer.CurrentRoom == this || visibility == VisibilityStatus.OBSCURED || (area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && m_hasGivenReward))
			{
				if (DarkSoulsRoomResetDependencies != null)
				{
					for (int i = 0; i < DarkSoulsRoomResetDependencies.Count; i++)
					{
						DarkSoulsRoomResetDependencies[i].m_hasGivenReward = false;
						DarkSoulsRoomResetDependencies[i].ResetPredefinedRoomLikeDarkSouls();
					}
				}
				return;
			}
			if (OnDarkSoulsReset != null)
			{
				OnDarkSoulsReset();
			}
			if (activeEnemies != null)
			{
				for (int num = activeEnemies.Count - 1; num >= 0; num--)
				{
					AIActor aIActor = activeEnemies[num];
					if ((bool)aIActor)
					{
						if ((bool)aIActor.behaviorSpeculator)
						{
							aIActor.behaviorSpeculator.InterruptAndDisable();
						}
						if (aIActor.healthHaver.IsBoss && aIActor.healthHaver.IsAlive)
						{
							aIActor.healthHaver.EndBossState(false);
						}
						UnityEngine.Object.Destroy(aIActor.gameObject);
					}
				}
				activeEnemies.Clear();
			}
			if (GameManager.Instance.InTutorial)
			{
				List<TalkDoerLite> componentsInRoom = GetComponentsInRoom<TalkDoerLite>();
				for (int j = 0; j < componentsInRoom.Count; j++)
				{
					DeregisterInteractable(componentsInRoom[j]);
					IEventTriggerable @interface = componentsInRoom[j].gameObject.GetInterface<IEventTriggerable>();
					for (int k = 0; k < eventTriggerAreas.Count; k++)
					{
						eventTriggerAreas[k].events.Remove(@interface);
					}
					UnityEngine.Object.Destroy(componentsInRoom[j].gameObject);
				}
				npcSealState = NPCSealState.SealNone;
			}
			else
			{
				List<TalkDoerLite> componentsInRoom2 = GetComponentsInRoom<TalkDoerLite>();
				for (int l = 0; l < componentsInRoom2.Count; l++)
				{
					componentsInRoom2[l].SendPlaymakerEvent("resetRoomLikeDarkSouls");
				}
			}
			if (bossTriggerZones != null)
			{
				for (int m = 0; m < bossTriggerZones.Count; m++)
				{
					bossTriggerZones[m].HasTriggered = false;
				}
			}
			if (remainingReinforcementLayers != null)
			{
				remainingReinforcementLayers.Clear();
			}
			UnsealRoom();
			visibility = VisibilityStatus.REOBSCURED;
			for (int n = 0; n < connectedDoors.Count; n++)
			{
				connectedDoors[n].Close();
			}
			PreventStandardRoomReward = true;
			if (area.IsProceduralRoom)
			{
				if (area.proceduralCells != null)
				{
				}
			}
			else
			{
				for (int num2 = -1; num2 < area.runtimePrototypeData.additionalObjectLayers.Count; num2++)
				{
					if (num2 != -1 && area.runtimePrototypeData.additionalObjectLayers[num2].layerIsReinforcementLayer)
					{
						PrototypeRoomObjectLayer prototypeRoomObjectLayer = area.runtimePrototypeData.additionalObjectLayers[num2];
						if (prototypeRoomObjectLayer.numberTimesEncounteredRequired > 0)
						{
							if (area.prototypeRoom != null)
							{
								if (GameStatsManager.Instance.QueryRoomEncountered(area.prototypeRoom.GUID) < prototypeRoomObjectLayer.numberTimesEncounteredRequired)
								{
									continue;
								}
							}
							else if (area.runtimePrototypeData != null && GameStatsManager.Instance.QueryRoomEncountered(area.runtimePrototypeData.GUID) < prototypeRoomObjectLayer.numberTimesEncounteredRequired)
							{
								continue;
							}
						}
						if (!(prototypeRoomObjectLayer.probability < 1f) || !(UnityEngine.Random.value > prototypeRoomObjectLayer.probability))
						{
							if (remainingReinforcementLayers == null)
							{
								remainingReinforcementLayers = new List<PrototypeRoomObjectLayer>();
							}
							if (area.runtimePrototypeData.additionalObjectLayers[num2].placedObjects.Count > 0)
							{
								remainingReinforcementLayers.Add(area.runtimePrototypeData.additionalObjectLayers[num2]);
							}
						}
						continue;
					}
					List<PrototypePlacedObjectData> list = ((num2 != -1) ? area.runtimePrototypeData.additionalObjectLayers[num2].placedObjects : area.runtimePrototypeData.placedObjects);
					List<Vector2> list2 = ((num2 != -1) ? area.runtimePrototypeData.additionalObjectLayers[num2].placedObjectBasePositions : area.runtimePrototypeData.placedObjectPositions);
					for (int num3 = 0; num3 < list.Count; num3++)
					{
						PrototypePlacedObjectData prototypePlacedObjectData = list[num3];
						if (prototypePlacedObjectData.spawnChance < 1f && UnityEngine.Random.value > prototypePlacedObjectData.spawnChance)
						{
							continue;
						}
						GameObject gameObject = null;
						IntVector2 location = list2[num3].ToIntVector2();
						if (prototypePlacedObjectData.placeableContents != null)
						{
							DungeonPlaceable placeableContents = prototypePlacedObjectData.placeableContents;
							gameObject = placeableContents.InstantiateObject(this, location, true);
						}
						if (prototypePlacedObjectData.nonenemyBehaviour != null)
						{
							DungeonPlaceableBehaviour nonenemyBehaviour = prototypePlacedObjectData.nonenemyBehaviour;
							gameObject = ((GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.TUTORIAL || !(nonenemyBehaviour.GetComponent<TalkDoerLite>() != null)) ? nonenemyBehaviour.InstantiateObjectOnlyActors(this, location) : nonenemyBehaviour.InstantiateObject(this, location));
						}
						if (!string.IsNullOrEmpty(prototypePlacedObjectData.enemyBehaviourGuid))
						{
							DungeonPlaceableBehaviour orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(prototypePlacedObjectData.enemyBehaviourGuid);
							gameObject = orLoadByGuid.InstantiateObjectOnlyActors(this, location);
						}
						if (gameObject != null)
						{
							AIActor component = gameObject.GetComponent<AIActor>();
							if ((bool)component)
							{
								if ((bool)component.healthHaver && component.healthHaver.IsBoss)
								{
									component.HasDonePlayerEnterCheck = true;
								}
								if (component.EnemyGuid == GlobalEnemyGuids.GripMaster)
								{
									UnityEngine.Object.Destroy(component.gameObject);
									continue;
								}
							}
							if (prototypePlacedObjectData.xMPxOffset != 0 || prototypePlacedObjectData.yMPxOffset != 0)
							{
								Vector2 vector = new Vector2((float)prototypePlacedObjectData.xMPxOffset * 0.0625f, (float)prototypePlacedObjectData.yMPxOffset * 0.0625f);
								gameObject.transform.position = gameObject.transform.position + vector.ToVector3ZUp();
							}
							IPlayerInteractable[] interfacesInChildren = gameObject.GetInterfacesInChildren<IPlayerInteractable>();
							for (int num4 = 0; num4 < interfacesInChildren.Length; num4++)
							{
								interactableObjects.Add(interfacesInChildren[num4]);
							}
							HandleFields(prototypePlacedObjectData, gameObject);
							gameObject.transform.parent = hierarchyParent;
						}
						if (prototypePlacedObjectData.linkedTriggerAreaIDs != null && prototypePlacedObjectData.linkedTriggerAreaIDs.Count > 0 && gameObject != null)
						{
							for (int num5 = 0; num5 < prototypePlacedObjectData.linkedTriggerAreaIDs.Count; num5++)
							{
								int key = prototypePlacedObjectData.linkedTriggerAreaIDs[num5];
								if (eventTriggerMap != null && eventTriggerMap.ContainsKey(key))
								{
									eventTriggerMap[key].AddGameObject(gameObject);
								}
							}
						}
						if (prototypePlacedObjectData.assignedPathIDx != -1 && (bool)gameObject)
						{
							PathMover component2 = gameObject.GetComponent<PathMover>();
							if (component2 != null)
							{
								component2.Path = area.runtimePrototypeData.paths[prototypePlacedObjectData.assignedPathIDx];
								component2.PathStartNode = prototypePlacedObjectData.assignedPathStartNode;
								component2.RoomHandler = this;
							}
						}
					}
				}
			}
			Pixelator.Instance.ProcessOcclusionChange(IntVector2.Zero, 0f, this, false);
			if (DarkSoulsRoomResetDependencies != null)
			{
				for (int num6 = 0; num6 < DarkSoulsRoomResetDependencies.Count; num6++)
				{
					DarkSoulsRoomResetDependencies[num6].m_hasGivenReward = false;
					DarkSoulsRoomResetDependencies[num6].ResetPredefinedRoomLikeDarkSouls();
				}
			}
		}

		private void HandleCellDungeonMaterialOverride(int ix, int iy, int overrideMaterialIndex)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			int num = 0;
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.MINEGEON)
			{
				num = 1;
			}
			for (int i = -1 * num; i < num + 1; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					CellData cellData = data.cellData[ix + i][iy + j];
					if (cellData == null || ((i != 0 || j != 0) && cellData.type != CellType.WALL))
					{
						break;
					}
					cellData.cellVisualData.roomVisualTypeIndex = overrideMaterialIndex;
				}
			}
		}

		private IntVector2 GetFirstCellOfSpecificQuality(int xStart, int yStart, int xDim, int yDim, Func<CellData, bool> validator)
		{
			for (int i = yStart; i < yStart + yDim; i++)
			{
				for (int j = xStart; j < xStart + xDim; j++)
				{
					if (validator(GameManager.Instance.Dungeon.data[j, i]))
					{
						return new IntVector2(j, i);
					}
				}
			}
			return IntVector2.NegOne;
		}

		public void EnsureUpstreamLocksUnlocked()
		{
			if (IsOnCriticalPath)
			{
				return;
			}
			for (int i = 0; i < connectedRooms.Count; i++)
			{
				if (connectedRooms[i].distanceFromEntrance < distanceFromEntrance)
				{
					RuntimeExitDefinition exitDefinitionForConnectedRoom = GetExitDefinitionForConnectedRoom(connectedRooms[i]);
					if (exitDefinitionForConnectedRoom != null && exitDefinitionForConnectedRoom.linkedDoor != null && exitDefinitionForConnectedRoom.linkedDoor.isLocked)
					{
						exitDefinitionForConnectedRoom.linkedDoor.Unlock();
					}
					connectedRooms[i].EnsureUpstreamLocksUnlocked();
				}
			}
		}

		private void HandleProceduralLocking()
		{
			if (IsOnCriticalPath || !ShouldAttemptProceduralLock)
			{
				return;
			}
			float value = UnityEngine.Random.value;
			if (!(value < AttemptProceduralLockChance))
			{
				return;
			}
			if (ProceduralLockingType == ProceduralLockType.BASE_SHOP)
			{
				BaseShopController.HasLockedShopProcedurally = true;
			}
			for (int i = 0; i < connectedDoors.Count; i++)
			{
				RoomHandler roomHandler = ((connectedDoors[i].upstreamRoom != this) ? connectedDoors[i].upstreamRoom : connectedDoors[i].downstreamRoom);
				if (roomHandler != null && roomHandler.distanceFromEntrance < distanceFromEntrance)
				{
					connectedDoors[i].isLocked = true;
					connectedDoors[i].ForceBecomeLockedDoor();
				}
			}
		}

		public void PostProcessFeatures()
		{
			if (!(area.prototypeRoom != null) || area.prototypeRoom.rectangularFeatures == null)
			{
				return;
			}
			for (int i = 0; i < area.prototypeRoom.rectangularFeatures.Count; i++)
			{
				PrototypeRectangularFeature feature = area.prototypeRoom.rectangularFeatures[i];
				GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
				if (tilesetId == GlobalDungeonData.ValidTilesets.SEWERGEON)
				{
					PostProcessSewersFeature(feature);
				}
			}
		}

		public void ProcessFeatures()
		{
			HandleProceduralLocking();
			if (area.prototypeRoom != null && area.prototypeRoom.rectangularFeatures != null)
			{
				for (int i = 0; i < area.prototypeRoom.rectangularFeatures.Count; i++)
				{
					PrototypeRectangularFeature feature = area.prototypeRoom.rectangularFeatures[i];
					switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
					{
					case GlobalDungeonData.ValidTilesets.WESTGEON:
						ProcessWestgeonFeature(feature);
						break;
					case GlobalDungeonData.ValidTilesets.SEWERGEON:
						ProcessSewersFeature(feature);
						break;
					}
				}
			}
			if (!area.IsProceduralRoom)
			{
				return;
			}
			for (int j = -1; j < area.dimensions.x + 2; j++)
			{
				for (int k = -1; k < area.dimensions.y + 2; k++)
				{
					IntVector2 intVector = area.basePosition + new IntVector2(j, k);
					CellData cellData = GameManager.Instance.Dungeon.data[intVector];
					if (cellData == null || !cellData.isExitCell)
					{
						continue;
					}
					for (int l = 0; l < 4; l++)
					{
						IntVector2 intVector2 = IntVector2.Cardinals[l];
						CellData cellData2 = GameManager.Instance.Dungeon.data[intVector + intVector2];
						while (cellData2 != null && !cellData2.isExitCell && cellData2.parentRoom == this && cellData2.type != CellType.WALL)
						{
							cellData2.type = CellType.FLOOR;
							cellData2 = GameManager.Instance.Dungeon.data[cellData2.position + intVector2];
						}
					}
				}
			}
		}

		private void PostProcessSewersFeature(PrototypeRectangularFeature feature)
		{
			for (int i = feature.basePosition.x; i < feature.basePosition.x + feature.dimensions.x; i++)
			{
				for (int j = feature.basePosition.y; j < feature.basePosition.y + feature.dimensions.y; j++)
				{
					IntVector2 key = area.basePosition + new IntVector2(i, j);
					CellData cellData = GameManager.Instance.Dungeon.data[key];
					cellData.type = CellType.FLOOR;
				}
			}
		}

		private void ProcessSewersFeature(PrototypeRectangularFeature feature)
		{
			for (int i = feature.basePosition.x; i < feature.basePosition.x + feature.dimensions.x; i++)
			{
				for (int j = feature.basePosition.y; j < feature.basePosition.y + feature.dimensions.y; j++)
				{
					IntVector2 intVector = area.basePosition + new IntVector2(i, j);
					CellData cellData = GameManager.Instance.Dungeon.data[intVector];
					cellData.fallingPrevented = true;
					int customIndexOverride = 91;
					if (RoomMaterial.bridgeGrid != null)
					{
						bool[] array = new bool[8];
						for (int k = 0; k < array.Length; k++)
						{
							IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection((DungeonData.Direction)k);
							IntVector2 intVector2 = intVector + intVector2FromDirection - area.basePosition;
							if (intVector2.x >= feature.basePosition.x && intVector2.x < feature.basePosition.x + feature.dimensions.x && intVector2.y >= feature.basePosition.y && intVector2.y < feature.basePosition.y + feature.dimensions.y)
							{
								array[k] = true;
							}
						}
						customIndexOverride = RoomMaterial.bridgeGrid.GetIndexGivenEightSides(array);
					}
					cellData.cellVisualData.UsesCustomIndexOverride01 = true;
					cellData.cellVisualData.CustomIndexOverride01 = customIndexOverride;
					cellData.cellVisualData.CustomIndexOverride01Layer = GlobalDungeonData.patternLayerIndex;
				}
			}
		}

		private void ProcessWestgeonFeature(PrototypeRectangularFeature feature)
		{
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.WESTGEON)
			{
				return;
			}
			int num = UnityEngine.Random.Range(1, 3);
			DungeonMaterial dungeonMaterial = GameManager.Instance.Dungeon.roomMaterialDefinitions[num];
			for (int i = feature.basePosition.x; i < feature.basePosition.x + feature.dimensions.x; i++)
			{
				for (int j = feature.basePosition.y; j < feature.basePosition.y + feature.dimensions.y; j++)
				{
					IntVector2 intVector = area.basePosition + new IntVector2(i, j);
					if (GameManager.Instance.Dungeon.data[intVector].nearestRoom == this)
					{
						GameManager.Instance.Dungeon.data[intVector].cellVisualData.IsFeatureCell = true;
						featureCells.Add(intVector);
						GameManager.Instance.Dungeon.data[intVector].cellVisualData.roomVisualTypeIndex = num;
					}
				}
			}
			IntVector2 firstCellOfSpecificQuality = GetFirstCellOfSpecificQuality(area.basePosition.x + feature.basePosition.x, area.basePosition.y + feature.basePosition.y, feature.dimensions.x, feature.dimensions.y, (CellData a) => a.IsUpperFacewall());
			if (!(firstCellOfSpecificQuality != IntVector2.NegOne))
			{
				return;
			}
			int num2 = 0;
			for (IntVector2 key = firstCellOfSpecificQuality; GameManager.Instance.Dungeon.data[key].IsUpperFacewall(); key += IntVector2.Right)
			{
				if (key.x >= area.basePosition.x + feature.basePosition.x + feature.dimensions.x)
				{
					break;
				}
				num2++;
			}
			if (num2 <= 3)
			{
				return;
			}
			int num3 = UnityEngine.Random.Range(0, num2 - 3);
			num2 -= num3;
			firstCellOfSpecificQuality = firstCellOfSpecificQuality.WithX(firstCellOfSpecificQuality.x + UnityEngine.Random.Range(0, num3));
			for (int k = firstCellOfSpecificQuality.x; k < firstCellOfSpecificQuality.x + num2; k++)
			{
				for (int l = firstCellOfSpecificQuality.y + 1; l <= firstCellOfSpecificQuality.y + 2; l++)
				{
					GameManager.Instance.Dungeon.data[k, l].cellVisualData.UsesCustomIndexOverride01 = true;
					GameManager.Instance.Dungeon.data[k, l].cellVisualData.CustomIndexOverride01Layer = GlobalDungeonData.aboveBorderLayerIndex;
					GameManager.Instance.Dungeon.data[k, l].cellVisualData.CustomIndexOverride01 = dungeonMaterial.facadeTopGrid.GetIndexGivenSides(l == firstCellOfSpecificQuality.y + 2, l == firstCellOfSpecificQuality.y + 2 && k == firstCellOfSpecificQuality.x + num2 - 1, k == firstCellOfSpecificQuality.x + num2 - 1, l == firstCellOfSpecificQuality.y + 1 && k == firstCellOfSpecificQuality.x + num2 - 1, l == firstCellOfSpecificQuality.y + 1, l == firstCellOfSpecificQuality.y + 1 && k == firstCellOfSpecificQuality.x, k == firstCellOfSpecificQuality.x, l == firstCellOfSpecificQuality.y + 2 && k == firstCellOfSpecificQuality.x);
				}
			}
		}

		private void StampAdditionalAppearanceData()
		{
			float num = UnityEngine.Random.Range(0f, 0.05f);
			float num2 = UnityEngine.Random.Range(0f, 0.05f);
			for (int i = area.basePosition.x; i < area.basePosition.x + area.dimensions.x; i++)
			{
				for (int j = area.basePosition.y; j < area.basePosition.y + area.dimensions.y; j++)
				{
					int num3 = i - area.basePosition.x;
					int num4 = j - area.basePosition.y;
					PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = area.prototypeRoom.ForceGetCellDataAtPoint(num3, num4);
					if (!prototypeDungeonRoomCellData.doesDamage)
					{
						DungeonMaterial dungeonMaterial = GameManager.Instance.Dungeon.roomMaterialDefinitions[RoomVisualSubtype];
						if (prototypeDungeonRoomCellData.appearance.overrideDungeonMaterialIndex != -1)
						{
							HandleCellDungeonMaterialOverride(i, j, prototypeDungeonRoomCellData.appearance.overrideDungeonMaterialIndex);
						}
						else if (dungeonMaterial.usesInternalMaterialTransitions && dungeonMaterial.usesProceduralMaterialTransitions && Mathf.PerlinNoise(num + (float)num3 / 10f, num2 + (float)num4 / 10f) > dungeonMaterial.internalMaterialTransitions[0].proceduralThreshold)
						{
							HandleCellDungeonMaterialOverride(i, j, dungeonMaterial.internalMaterialTransitions[0].materialTransition);
						}
					}
				}
			}
		}

		private bool NonenemyPlaceableBehaviorIsEnemylike(DungeonPlaceableBehaviour dpb)
		{
			return dpb is ForgeHammerController;
		}

		private void CleanupPrototypeRoomLayers()
		{
			area.prototypeRoom.runtimeAdditionalObjectLayers = new List<PrototypeRoomObjectLayer>();
			for (int i = 0; i < area.prototypeRoom.additionalObjectLayers.Count; i++)
			{
				PrototypeRoomObjectLayer prototypeRoomObjectLayer = area.prototypeRoom.additionalObjectLayers[i];
				if (!prototypeRoomObjectLayer.layerIsReinforcementLayer)
				{
					area.prototypeRoom.runtimeAdditionalObjectLayers.Add(prototypeRoomObjectLayer);
					continue;
				}
				Action<PrototypeRoomObjectLayer, PrototypeRoomObjectLayer> action = delegate(PrototypeRoomObjectLayer source, PrototypeRoomObjectLayer target)
				{
					target.shuffle = source.shuffle;
					target.randomize = source.randomize;
					target.suppressPlayerChecks = source.suppressPlayerChecks;
					target.delayTime = source.delayTime;
					target.reinforcementTriggerCondition = source.reinforcementTriggerCondition;
					target.probability = source.probability;
					target.numberTimesEncounteredRequired = source.numberTimesEncounteredRequired;
				};
				bool flag = false;
				bool flag2 = false;
				PrototypeRoomObjectLayer prototypeRoomObjectLayer2 = null;
				PrototypeRoomObjectLayer prototypeRoomObjectLayer3 = null;
				for (int j = 0; j < prototypeRoomObjectLayer.placedObjects.Count; j++)
				{
					if (prototypeRoomObjectLayer.placedObjects[j].placeableContents != null)
					{
						if (prototypeRoomObjectLayer.placedObjects[j].placeableContents.ContainsEnemy || prototypeRoomObjectLayer.placedObjects[j].placeableContents.ContainsEnemylikeObjectForReinforcement)
						{
							flag = true;
							if (prototypeRoomObjectLayer2 == null)
							{
								prototypeRoomObjectLayer2 = new PrototypeRoomObjectLayer();
							}
							prototypeRoomObjectLayer2.placedObjects.Add(prototypeRoomObjectLayer.placedObjects[j]);
							prototypeRoomObjectLayer2.placedObjectBasePositions.Add(prototypeRoomObjectLayer.placedObjectBasePositions[j]);
						}
						else
						{
							flag2 = true;
							if (prototypeRoomObjectLayer3 == null)
							{
								prototypeRoomObjectLayer3 = new PrototypeRoomObjectLayer();
							}
							prototypeRoomObjectLayer3.placedObjects.Add(prototypeRoomObjectLayer.placedObjects[j]);
							prototypeRoomObjectLayer3.placedObjectBasePositions.Add(prototypeRoomObjectLayer.placedObjectBasePositions[j]);
						}
					}
					else if (prototypeRoomObjectLayer.placedObjects[j].nonenemyBehaviour != null)
					{
						if (NonenemyPlaceableBehaviorIsEnemylike(prototypeRoomObjectLayer.placedObjects[j].nonenemyBehaviour))
						{
							flag = true;
							if (prototypeRoomObjectLayer2 == null)
							{
								prototypeRoomObjectLayer2 = new PrototypeRoomObjectLayer();
							}
							prototypeRoomObjectLayer2.placedObjects.Add(prototypeRoomObjectLayer.placedObjects[j]);
							prototypeRoomObjectLayer2.placedObjectBasePositions.Add(prototypeRoomObjectLayer.placedObjectBasePositions[j]);
						}
						else
						{
							flag2 = true;
							if (prototypeRoomObjectLayer3 == null)
							{
								prototypeRoomObjectLayer3 = new PrototypeRoomObjectLayer();
							}
							prototypeRoomObjectLayer3.placedObjects.Add(prototypeRoomObjectLayer.placedObjects[j]);
							prototypeRoomObjectLayer3.placedObjectBasePositions.Add(prototypeRoomObjectLayer.placedObjectBasePositions[j]);
						}
					}
					else if (!string.IsNullOrEmpty(prototypeRoomObjectLayer.placedObjects[j].enemyBehaviourGuid))
					{
						flag = true;
						if (prototypeRoomObjectLayer2 == null)
						{
							prototypeRoomObjectLayer2 = new PrototypeRoomObjectLayer();
						}
						prototypeRoomObjectLayer2.placedObjects.Add(prototypeRoomObjectLayer.placedObjects[j]);
						prototypeRoomObjectLayer2.placedObjectBasePositions.Add(prototypeRoomObjectLayer.placedObjectBasePositions[j]);
					}
				}
				if (flag && flag2)
				{
					action(prototypeRoomObjectLayer, prototypeRoomObjectLayer2);
					action(prototypeRoomObjectLayer, prototypeRoomObjectLayer3);
					prototypeRoomObjectLayer2.layerIsReinforcementLayer = prototypeRoomObjectLayer.layerIsReinforcementLayer;
					prototypeRoomObjectLayer3.layerIsReinforcementLayer = false;
					area.prototypeRoom.runtimeAdditionalObjectLayers.Add(prototypeRoomObjectLayer2);
					area.prototypeRoom.runtimeAdditionalObjectLayers.Add(prototypeRoomObjectLayer3);
				}
				else if (flag2)
				{
					action(prototypeRoomObjectLayer, prototypeRoomObjectLayer3);
					prototypeRoomObjectLayer3.layerIsReinforcementLayer = false;
					area.prototypeRoom.runtimeAdditionalObjectLayers.Add(prototypeRoomObjectLayer3);
				}
				else
				{
					area.prototypeRoom.runtimeAdditionalObjectLayers.Add(prototypeRoomObjectLayer);
				}
			}
		}

		public void RegisterExternalReinforcementLayer(PrototypeDungeonRoom source, int layerIndex)
		{
			if (remainingReinforcementLayers == null)
			{
				remainingReinforcementLayers = new List<PrototypeRoomObjectLayer>();
			}
			if (source.runtimeAdditionalObjectLayers[layerIndex].placedObjects.Count > 0)
			{
				remainingReinforcementLayers.Add(area.prototypeRoom.runtimeAdditionalObjectLayers[layerIndex]);
			}
		}

		private void MakePredefinedRoom()
		{
			CleanupPrototypeRoomLayers();
			DungeonData data = GameManager.Instance.Dungeon.data;
			GameObject gameObject = GameObject.Find("_Rooms");
			if (gameObject == null)
			{
				gameObject = new GameObject("_Rooms");
			}
			Transform transform = new GameObject("Room_" + area.prototypeRoom.name).transform;
			transform.parent = gameObject.transform;
			m_roomMotionHandler = transform.gameObject.AddComponent<RoomMotionHandler>();
			m_roomMotionHandler.Initialize(this);
			hierarchyParent = transform;
			List<IntVector2> list = new List<IntVector2>();
			if (area.prototypeRoom.ContainsEnemies)
			{
				EverHadEnemies = true;
			}
			for (int i = area.basePosition.x; i < area.basePosition.x + area.dimensions.x; i++)
			{
				for (int j = area.basePosition.y; j < area.basePosition.y + area.dimensions.y; j++)
				{
					int ix = i - area.basePosition.x;
					int iy = j - area.basePosition.y;
					PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = area.prototypeRoom.ForceGetCellDataAtPoint(ix, iy);
					if (!area.prototypeRoom.HasNonWallNeighborWithDiagonals(ix, iy) && !prototypeDungeonRoomCellData.breakable)
					{
						continue;
					}
					bool flag = true;
					if (prototypeDungeonRoomCellData.conditionalOnParentExit && !area.instanceUsedExits.Contains(area.prototypeRoom.exitData.exits[prototypeDungeonRoomCellData.parentExitIndex]))
					{
						if (prototypeDungeonRoomCellData.conditionalCellIsPit && StampCellComplex(i, j, CellType.PIT, DiagonalWallType.NONE))
						{
							HandleStampedCellVisualData(i, j, prototypeDungeonRoomCellData);
						}
						continue;
					}
					if (prototypeDungeonRoomCellData.state != CellType.WALL)
					{
						flag = StampCellComplex(i, j, prototypeDungeonRoomCellData.state, prototypeDungeonRoomCellData.diagonalWallType);
						if (flag)
						{
							HandleStampedCellVisualData(i, j, prototypeDungeonRoomCellData);
						}
					}
					else if (prototypeDungeonRoomCellData.state == CellType.WALL)
					{
						flag = StampCellComplex(i, j, prototypeDungeonRoomCellData.state, prototypeDungeonRoomCellData.diagonalWallType, prototypeDungeonRoomCellData.breakable);
					}
					if (!flag)
					{
						continue;
					}
					CellData cellData = data.cellData[i][j];
					if (prototypeDungeonRoomCellData != null)
					{
						cellData.cellVisualData.IsPhantomCarpet = prototypeDungeonRoomCellData.appearance.IsPhantomCarpet;
						cellData.forceDisallowGoop = prototypeDungeonRoomCellData.appearance.ForceDisallowGoop;
						if ((prototypeDungeonRoomCellData.appearance.OverrideFloorType != CellVisualData.CellFloorType.Ice || RoomMaterial.supportsIceSquares) && prototypeDungeonRoomCellData.appearance.OverrideFloorType != 0)
						{
							cellData.cellVisualData.floorType = prototypeDungeonRoomCellData.appearance.OverrideFloorType;
							if (cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Water)
							{
								cellData.cellVisualData.absorbsDebris = true;
							}
						}
					}
					List<int> overridesForTilemap = prototypeDungeonRoomCellData.appearance.GetOverridesForTilemap(area.prototypeRoom, GameManager.Instance.Dungeon.tileIndices.tilesetId);
					if (overridesForTilemap != null && overridesForTilemap.Count != 0)
					{
						int num = Mathf.FloorToInt(cellData.UniqueHash * (float)overridesForTilemap.Count);
						if (num == overridesForTilemap.Count)
						{
							num--;
						}
						cellData.cellVisualData.inheritedOverrideIndex = overridesForTilemap[num];
						cellData.cellVisualData.floorTileOverridden = true;
					}
					if (prototypeDungeonRoomCellData.doesDamage && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.JUNGLEGEON)
					{
						list.Add(cellData.position);
					}
					else if (prototypeDungeonRoomCellData.doesDamage && GameManager.Instance.Dungeon.roomMaterialDefinitions[m_roomVisualType].supportsLavaOrLavalikeSquares)
					{
						cellData.doesDamage = true;
						cellData.damageDefinition = prototypeDungeonRoomCellData.damageDefinition;
						cellData.cellVisualData.floorType = CellVisualData.CellFloorType.Water;
					}
					if (prototypeDungeonRoomCellData.ForceTileNonDecorated)
					{
						cellData.cellVisualData.containsObjectSpaceStamp = true;
						cellData.cellVisualData.containsWallSpaceStamp = true;
						data.cellData[i][j + 1].cellVisualData.containsObjectSpaceStamp = true;
						data.cellData[i][j + 1].cellVisualData.containsWallSpaceStamp = true;
						data.cellData[i][j + 2].cellVisualData.containsObjectSpaceStamp = true;
						data.cellData[i][j + 2].cellVisualData.containsWallSpaceStamp = true;
					}
				}
			}
			for (int k = 0; k < area.prototypeRoom.paths.Count; k++)
			{
				area.prototypeRoom.paths[k].StampPathToTilemap(this);
			}
			eventTriggerAreas = new List<RoomEventTriggerArea>();
			eventTriggerMap = new Dictionary<int, RoomEventTriggerArea>();
			for (int l = 0; l < area.prototypeRoom.eventTriggerAreas.Count; l++)
			{
				PrototypeEventTriggerArea prototype = area.prototypeRoom.eventTriggerAreas[l];
				RoomEventTriggerArea roomEventTriggerArea = new RoomEventTriggerArea(prototype, area.basePosition);
				eventTriggerAreas.Add(roomEventTriggerArea);
				eventTriggerMap.Add(l, roomEventTriggerArea);
			}
			for (int m = -1; m < area.prototypeRoom.runtimeAdditionalObjectLayers.Count; m++)
			{
				if (m != -1 && area.prototypeRoom.runtimeAdditionalObjectLayers[m].layerIsReinforcementLayer)
				{
					PrototypeRoomObjectLayer prototypeRoomObjectLayer = area.prototypeRoom.runtimeAdditionalObjectLayers[m];
					if ((prototypeRoomObjectLayer.numberTimesEncounteredRequired <= 0 || GameStatsManager.Instance.QueryRoomEncountered(area.prototypeRoom.GUID) >= prototypeRoomObjectLayer.numberTimesEncounteredRequired) && (!(prototypeRoomObjectLayer.probability < 1f) || !(UnityEngine.Random.value > prototypeRoomObjectLayer.probability)))
					{
						if (remainingReinforcementLayers == null)
						{
							remainingReinforcementLayers = new List<PrototypeRoomObjectLayer>();
						}
						if (area.prototypeRoom.runtimeAdditionalObjectLayers[m].placedObjects.Count > 0)
						{
							remainingReinforcementLayers.Add(area.prototypeRoom.runtimeAdditionalObjectLayers[m]);
						}
					}
					continue;
				}
				List<PrototypePlacedObjectData> placedObjectList = ((m != -1) ? area.prototypeRoom.runtimeAdditionalObjectLayers[m].placedObjects : area.prototypeRoom.placedObjects);
				List<Vector2> placedObjectPositions = ((m != -1) ? area.prototypeRoom.runtimeAdditionalObjectLayers[m].placedObjectBasePositions : area.prototypeRoom.placedObjectPositions);
				if (m != -1)
				{
					PrototypeRoomObjectLayer prototypeRoomObjectLayer2 = area.prototypeRoom.runtimeAdditionalObjectLayers[m];
					if ((prototypeRoomObjectLayer2.numberTimesEncounteredRequired > 0 && GameStatsManager.Instance.QueryRoomEncountered(area.prototypeRoom.GUID) < prototypeRoomObjectLayer2.numberTimesEncounteredRequired) || (prototypeRoomObjectLayer2.probability < 1f && UnityEngine.Random.value > prototypeRoomObjectLayer2.probability))
					{
						continue;
					}
				}
				PlaceObjectsFromLayer(placedObjectList, null, placedObjectPositions, eventTriggerMap);
			}
			GameObject gameObject2 = GameObject.Find("_Doors");
			if (gameObject2 == null)
			{
				gameObject2 = new GameObject("_Doors");
			}
			for (int n = 0; n < area.instanceUsedExits.Count; n++)
			{
				PrototypeRoomExit prototypeRoomExit = area.instanceUsedExits[n];
				RuntimeRoomExitData runtimeRoomExitData = area.exitToLocalDataMap[prototypeRoomExit];
				bool isSecretConnection = false;
				RoomHandler roomHandler = null;
				if (connectedRoomsByExit[prototypeRoomExit].area.prototypeRoom != null)
				{
					isSecretConnection = connectedRoomsByExit[prototypeRoomExit].area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET;
				}
				roomHandler = connectedRoomsByExit[prototypeRoomExit];
				RuntimeExitDefinition runtimeExitDefinition = null;
				if (exitDefinitionsByExit != null && exitDefinitionsByExit.ContainsKey(runtimeRoomExitData))
				{
					runtimeExitDefinition = exitDefinitionsByExit[runtimeRoomExitData];
					foreach (IntVector2 item in runtimeExitDefinition.GetCellsForRoom(this))
					{
						StampCellAsExit(item.x, item.y, prototypeRoomExit.exitDirection, roomHandler, isSecretConnection);
					}
					runtimeExitDefinition.StampCellVisualTypes(data);
					runtimeExitDefinition.ProcessExitDecorables();
					continue;
				}
				runtimeExitDefinition = new RuntimeExitDefinition(runtimeRoomExitData, runtimeRoomExitData.linkedExit, this, roomHandler);
				if (exitDefinitionsByExit == null)
				{
					exitDefinitionsByExit = new Dictionary<RuntimeRoomExitData, RuntimeExitDefinition>();
				}
				if (roomHandler.exitDefinitionsByExit == null)
				{
					roomHandler.exitDefinitionsByExit = new Dictionary<RuntimeRoomExitData, RuntimeExitDefinition>();
				}
				exitDefinitionsByExit.Add(runtimeRoomExitData, runtimeExitDefinition);
				if (runtimeRoomExitData.linkedExit != null)
				{
					roomHandler.exitDefinitionsByExit.Add(runtimeRoomExitData.linkedExit, runtimeExitDefinition);
				}
				foreach (IntVector2 item2 in runtimeExitDefinition.GetCellsForRoom(this))
				{
					StampCellAsExit(item2.x, item2.y, prototypeRoomExit.exitDirection, roomHandler, isSecretConnection);
				}
				if (runtimeRoomExitData.linkedExit == null)
				{
					foreach (IntVector2 item3 in runtimeExitDefinition.GetCellsForRoom(roomHandler))
					{
						roomHandler.StampCellAsExit(item3.x, item3.y, prototypeRoomExit.exitDirection, this);
						data[item3].parentRoom = roomHandler;
						data[item3].occlusionData.sharedRoomAndExitCell = true;
					}
					runtimeExitDefinition.StampCellVisualTypes(data);
				}
				if (runtimeExitDefinition.IntermediaryCells != null)
				{
					foreach (IntVector2 intermediaryCell in runtimeExitDefinition.IntermediaryCells)
					{
						if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intermediaryCell))
						{
							if (!Cells.Contains(intermediaryCell))
							{
								Cells.Add(intermediaryCell);
							}
							GameManager.Instance.Dungeon.data[intermediaryCell].parentRoom = null;
							GameManager.Instance.Dungeon.data[intermediaryCell].isDoorFrameCell = true;
						}
					}
				}
				runtimeExitDefinition.GenerateDoorsForExit(data, gameObject2.transform);
			}
			if (list.Count > 0 && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.JUNGLEGEON)
			{
				GameObject gameObject3 = new GameObject("grass patch");
				TallGrassPatch tallGrassPatch = gameObject3.AddComponent<TallGrassPatch>();
				tallGrassPatch.cells = list;
				tallGrassPatch.BuildPatch();
			}
		}

		public void AddProceduralTeleporterToRoom()
		{
			if (Minimap.Instance.HasTeleporterIcon(this))
			{
				return;
			}
			GameObject objectToInstantiate = ResourceCache.Acquire("Global Prefabs/Teleporter_Gungeon_01") as GameObject;
			DungeonData dungeonData = GameManager.Instance.Dungeon.data;
			bool isStrict = true;
			Func<CellData, bool> canContainTeleporter = delegate(CellData a)
			{
				if (a == null || a.isOccupied || a.doesDamage || a.containsTrap || a.IsTrapZone || a.cellVisualData.hasStampedPath)
				{
					return false;
				}
				return (!isStrict || !a.HasPitNeighbor(dungeonData)) && a.type == CellType.FLOOR;
			};
			ProcessTeleporterTiles(canContainTeleporter);
			Func<CellData, bool> isInvalidFunction = (CellData a) => a == null || !a.cachedCanContainTeleporter || a.parentRoom != this;
			Tuple<IntVector2, IntVector2> tuple = Carpetron.RawMaxSubmatrix(dungeonData.cellData, area.basePosition, area.dimensions, isInvalidFunction);
			if (tuple.Second.x < 3 || tuple.Second.y < 3)
			{
				isStrict = false;
				ProcessTeleporterTiles(canContainTeleporter);
				tuple = Carpetron.RawMaxSubmatrix(dungeonData.cellData, area.basePosition, area.dimensions, isInvalidFunction);
			}
			BraveUtility.DrawDebugSquare(tuple.First.ToVector2(), tuple.Second.ToVector2(), Color.red, 1000f);
			if (tuple.Second.x >= 3 && tuple.Second.y >= 3)
			{
				IntVector2 first = tuple.First;
				IntVector2 intVector = tuple.Second - tuple.First;
				int x = ((intVector.x % 2 != 1 && intVector.x != 4) ? (-1) : 0);
				int y = ((intVector.y % 2 != 1 && intVector.y != 4) ? (-1) : 0);
				while (intVector.x > 3)
				{
					first.x++;
					intVector.x -= 2;
				}
				while (intVector.y > 3)
				{
					first.y++;
					intVector.y -= 2;
				}
				first += new IntVector2(x, y);
				GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(objectToInstantiate, this, first, false);
				TeleporterController component = gameObject.GetComponent<TeleporterController>();
				RegisterInteractable(component);
			}
		}

		private void ProcessTeleporterTiles(Func<CellData, bool> canContainTeleporter)
		{
			IntVector2 basePosition = area.basePosition;
			IntVector2 intVector = area.basePosition + area.dimensions - IntVector2.One;
			DungeonData data = GameManager.Instance.Dungeon.data;
			for (int i = basePosition.x; i <= intVector.x; i++)
			{
				for (int j = basePosition.y; j <= intVector.y; j++)
				{
					if (data[i, j] != null)
					{
						data[i, j].cachedCanContainTeleporter = false;
					}
				}
			}
			for (int k = basePosition.x; k <= intVector.x; k++)
			{
				for (int l = basePosition.y; l <= intVector.y; l++)
				{
					bool flag = true;
					for (int m = 0; m < 4; m++)
					{
						if (!flag)
						{
							break;
						}
						for (int n = 0; n < 4; n++)
						{
							if (!flag)
							{
								break;
							}
							if (!data.CheckInBounds(k + m, l + n) || !canContainTeleporter(data[k + m, l + n]))
							{
								flag = false;
								break;
							}
						}
					}
					if (!flag)
					{
						continue;
					}
					for (int num = 0; num < 4; num++)
					{
						if (!flag)
						{
							break;
						}
						for (int num2 = 0; num2 < 4; num2++)
						{
							if (!flag)
							{
								break;
							}
							data[k + num, l + num2].cachedCanContainTeleporter = true;
						}
					}
				}
			}
		}

		protected IntVector2 GetDoorPositionForExit(RuntimeRoomExitData exit)
		{
			IntVector2 intVector = exit.ExitOrigin - IntVector2.One;
			IntVector2 result = intVector + area.basePosition;
			if (exit.jointedExit)
			{
				if (exit.TotalExitLength > exit.linkedExit.TotalExitLength)
				{
					intVector = exit.HalfExitAttachPoint - IntVector2.One;
					result = intVector + area.basePosition;
				}
				else
				{
					intVector = exit.linkedExit.HalfExitAttachPoint - IntVector2.One;
					result = intVector + connectedRoomsByExit[exit.referencedExit].area.basePosition;
				}
			}
			return result;
		}

		protected void AttachDoorControllerToAllConnectedExitCells(DungeonDoorController controller, IntVector2 exitCellPosition)
		{
			Queue<CellData> queue = new Queue<CellData>();
			queue.Enqueue(GameManager.Instance.Dungeon.data[exitCellPosition]);
			while (queue.Count > 0)
			{
				CellData cellData = queue.Dequeue();
				cellData.exitDoor = controller;
				List<CellData> cellNeighbors = GameManager.Instance.Dungeon.data.GetCellNeighbors(cellData);
				for (int i = 0; i < cellNeighbors.Count; i++)
				{
					CellData cellData2 = cellNeighbors[i];
					if (!(cellData2.exitDoor == controller) && cellData2.isExitCell)
					{
						queue.Enqueue(cellData2);
					}
				}
			}
		}

		public bool UnsealConditionsMet()
		{
			if (!area.IsProceduralRoom && area.runtimePrototypeData.roomEvents != null && area.runtimePrototypeData.roomEvents.Count > 0)
			{
				bool result = true;
				for (int i = 0; i < area.runtimePrototypeData.roomEvents.Count; i++)
				{
					RoomEventDefinition roomEventDefinition = area.runtimePrototypeData.roomEvents[i];
					if (roomEventDefinition.action == RoomEventTriggerAction.UNSEAL_ROOM && roomEventDefinition.condition == RoomEventTriggerCondition.ON_ENEMIES_CLEARED && HasActiveEnemies(ActiveEnemyType.RoomClear))
					{
						result = false;
					}
				}
				return result;
			}
			return true;
		}

		public bool CanTeleportFromRoom()
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
			{
				if (HasActiveEnemies(ActiveEnemyType.RoomClear))
				{
					return false;
				}
			}
			else
			{
				for (int i = 0; i < connectedDoors.Count; i++)
				{
					if (HasActiveEnemies(ActiveEnemyType.RoomClear))
					{
						return false;
					}
					if (connectedDoors[i].IsSealed && connectedDoors[i].Mode != DungeonDoorController.DungeonDoorMode.FINAL_BOSS_DOOR && connectedDoors[i].Mode != DungeonDoorController.DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS && connectedDoors[i].Mode != DungeonDoorController.DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool CanTeleportToRoom()
		{
			if (!TeleportersActive)
			{
				return false;
			}
			for (int i = 0; i < connectedDoors.Count; i++)
			{
				if (connectedDoors[i].IsSealed && connectedDoors[i].Mode != DungeonDoorController.DungeonDoorMode.FINAL_BOSS_DOOR && connectedDoors[i].Mode != DungeonDoorController.DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS && connectedDoors[i].Mode != DungeonDoorController.DungeonDoorMode.ONE_WAY_DOOR_ONLY_UNSEALS)
				{
					return false;
				}
			}
			return true;
		}

		public void SealRoom()
		{
			if (m_isSealed)
			{
				return;
			}
			m_isSealed = true;
			for (int i = 0; i < connectedDoors.Count; i++)
			{
				if (!connectedDoors[i].OneWayDoor && npcSealState == NPCSealState.SealNext)
				{
					RoomHandler roomHandler = ((connectedDoors[i].upstreamRoom != this) ? connectedDoors[i].upstreamRoom : connectedDoors[i].downstreamRoom);
					if (roomHandler.distanceFromEntrance >= distanceFromEntrance)
					{
						connectedDoors[i].DoSeal(this);
					}
				}
				else
				{
					connectedDoors[i].DoSeal(this);
				}
			}
			for (int j = 0; j < standaloneBlockers.Count; j++)
			{
				if ((bool)standaloneBlockers[j])
				{
					standaloneBlockers[j].Seal();
				}
			}
			for (int k = 0; k < connectedRooms.Count; k++)
			{
				if (connectedRooms[k].secretRoomManager != null)
				{
					connectedRooms[k].secretRoomManager.DoSeal();
				}
			}
			if (GameManager.Instance.AllPlayers.Length > 1)
			{
				PlayerController playerController = null;
				for (int l = 0; l < GameManager.Instance.AllPlayers.Length; l++)
				{
					if (GameManager.Instance.AllPlayers[l].CurrentRoom == this)
					{
						playerController = GameManager.Instance.AllPlayers[l];
						break;
					}
				}
				for (int m = 0; m < GameManager.Instance.AllPlayers.Length; m++)
				{
					if (GameManager.Instance.AllPlayers[m] == playerController)
					{
					}
				}
			}
			if (OnSealChanged != null)
			{
				OnSealChanged(true);
			}
		}

		public void UnsealRoom()
		{
			if (npcSealState == NPCSealState.SealAll || (npcSealState == NPCSealState.SealNone && !m_isSealed))
			{
				return;
			}
			m_isSealed = false;
			for (int i = 0; i < connectedDoors.Count; i++)
			{
				if (!connectedDoors[i].IsSealed && (!(connectedDoors[i].subsidiaryBlocker != null) || !connectedDoors[i].subsidiaryBlocker.isSealed) && (!(connectedDoors[i].subsidiaryDoor != null) || !connectedDoors[i].subsidiaryDoor.IsSealed))
				{
					continue;
				}
				if (!connectedDoors[i].OneWayDoor)
				{
					if (npcSealState == NPCSealState.SealNone)
					{
						connectedDoors[i].DoUnseal(this);
					}
					else if (npcSealState == NPCSealState.SealPrior)
					{
						RoomHandler roomHandler = ((connectedDoors[i].upstreamRoom != this) ? connectedDoors[i].upstreamRoom : connectedDoors[i].downstreamRoom);
						if (roomHandler.distanceFromEntrance >= distanceFromEntrance)
						{
							connectedDoors[i].DoUnseal(this);
						}
					}
					else if (npcSealState == NPCSealState.SealNext)
					{
						RoomHandler roomHandler2 = ((connectedDoors[i].upstreamRoom != this) ? connectedDoors[i].upstreamRoom : connectedDoors[i].downstreamRoom);
						if (roomHandler2.distanceFromEntrance < distanceFromEntrance)
						{
							connectedDoors[i].DoUnseal(this);
						}
					}
				}
				else
				{
					if (connectedDoors[i].subsidiaryDoor != null)
					{
						connectedDoors[i].subsidiaryDoor.DoUnseal(this);
					}
					if (connectedDoors[i].subsidiaryBlocker != null)
					{
						connectedDoors[i].subsidiaryBlocker.Unseal();
					}
				}
			}
			for (int j = 0; j < standaloneBlockers.Count; j++)
			{
				if ((bool)standaloneBlockers[j])
				{
					standaloneBlockers[j].Unseal();
				}
			}
			for (int k = 0; k < connectedRooms.Count; k++)
			{
				if (connectedRooms[k].secretRoomManager != null)
				{
					connectedRooms[k].secretRoomManager.DoUnseal();
				}
			}
			if (OnSealChanged != null)
			{
				OnSealChanged(false);
			}
		}

		public IPlayerInteractable GetNearestInteractable(Vector2 position, float maxDistance, PlayerController player)
		{
			IPlayerInteractable result = null;
			float num = float.MaxValue;
			bool flag = GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.GetOtherPlayer(player).IsTalking;
			for (int i = 0; i < interactableObjects.Count; i++)
			{
				IPlayerInteractable playerInteractable = interactableObjects[i];
				if (!(playerInteractable as MonoBehaviour) || (flag && (playerInteractable is TalkDoer || playerInteractable is TalkDoerLite)))
				{
					continue;
				}
				if (!player.IsPrimaryPlayer && playerInteractable is TalkDoerLite)
				{
					TalkDoerLite talkDoerLite = playerInteractable as TalkDoerLite;
					if (talkDoerLite.PreventCoopInteraction)
					{
						continue;
					}
				}
				float distanceToPoint = playerInteractable.GetDistanceToPoint(position);
				float num2 = playerInteractable.GetOverrideMaxDistance();
				if (num2 <= 0f)
				{
					num2 = maxDistance;
				}
				if (distanceToPoint < num2 && distanceToPoint < num)
				{
					result = playerInteractable;
					num = distanceToPoint;
				}
			}
			if (unassignedInteractableObjects != null)
			{
				for (int j = 0; j < unassignedInteractableObjects.Count; j++)
				{
					IPlayerInteractable playerInteractable2 = unassignedInteractableObjects[j];
					if ((bool)(playerInteractable2 as MonoBehaviour) && (!flag || (!(playerInteractable2 is TalkDoer) && !(playerInteractable2 is TalkDoerLite))))
					{
						float distanceToPoint2 = playerInteractable2.GetDistanceToPoint(position);
						float num3 = playerInteractable2.GetOverrideMaxDistance();
						if (num3 <= 0f)
						{
							num3 = maxDistance;
						}
						if (distanceToPoint2 < num3 && distanceToPoint2 < num)
						{
							result = playerInteractable2;
							num = distanceToPoint2;
						}
					}
				}
			}
			return result;
		}

		public ReadOnlyCollection<IPlayerInteractable> GetRoomInteractables()
		{
			return interactableObjects.AsReadOnly();
		}

		public List<IPlayerInteractable> GetNearbyInteractables(Vector2 position, float maxDistance)
		{
			List<IPlayerInteractable> list = new List<IPlayerInteractable>();
			for (int i = 0; i < interactableObjects.Count; i++)
			{
				IPlayerInteractable playerInteractable = interactableObjects[i];
				if (playerInteractable.GetDistanceToPoint(position) < maxDistance)
				{
					list.Add(playerInteractable);
				}
			}
			return list;
		}

		public void RegisterEnemy(AIActor enemy)
		{
			EverHadEnemies = true;
			if (activeEnemies == null)
			{
				activeEnemies = new List<AIActor>();
			}
			if (activeEnemies.Contains(enemy))
			{
				BraveUtility.Log("Registering an enemy to a RoomHandler twice!", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
				return;
			}
			activeEnemies.Add(enemy);
			m_totalSpawnedEnemyHP += enemy.healthHaver.GetMaxHealth();
			m_lastTotalSpawnedEnemyHP += enemy.healthHaver.GetMaxHealth();
			RegisterAutoAimTarget(enemy);
			if (OnEnemyRegistered != null)
			{
				OnEnemyRegistered(enemy);
			}
		}

		public AIActor GetRandomActiveEnemy(bool allowHarmless = true)
		{
			if (activeEnemies == null || activeEnemies.Count <= 0)
			{
				return null;
			}
			if (!allowHarmless)
			{
				s_tempActiveEnemies.Clear();
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if ((bool)activeEnemies[i] && !activeEnemies[i].IsHarmlessEnemy)
					{
						s_tempActiveEnemies.Add(activeEnemies[i]);
					}
				}
				if (s_tempActiveEnemies.Count == 0)
				{
					return null;
				}
				AIActor result = s_tempActiveEnemies[UnityEngine.Random.Range(0, s_tempActiveEnemies.Count)];
				s_tempActiveEnemies.Clear();
				return result;
			}
			return activeEnemies[UnityEngine.Random.Range(0, activeEnemies.Count)];
		}

		public List<AIActor> GetActiveEnemies(ActiveEnemyType type)
		{
			if (type == ActiveEnemyType.RoomClear)
			{
				if (activeEnemies == null)
				{
					return null;
				}
				return new List<AIActor>(activeEnemies.Where((AIActor a) => !a.IgnoreForRoomClear));
			}
			return activeEnemies;
		}

		public void GetActiveEnemies(ActiveEnemyType type, ref List<AIActor> outList)
		{
			outList.Clear();
			if (activeEnemies == null)
			{
				return;
			}
			if (type == ActiveEnemyType.RoomClear)
			{
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if (!activeEnemies[i].IgnoreForRoomClear)
					{
						outList.Add(activeEnemies[i]);
					}
				}
			}
			else
			{
				outList.AddRange(activeEnemies);
			}
		}

		public int GetActiveEnemiesCount(ActiveEnemyType type)
		{
			if (activeEnemies == null)
			{
				return 0;
			}
			if (type == ActiveEnemyType.RoomClear)
			{
				return activeEnemies.Count((AIActor a) => !a.IgnoreForRoomClear);
			}
			return activeEnemies.Count;
		}

		public AIActor GetNearestEnemy(Vector2 position, out float nearestDistance, bool includeBosses = true, bool excludeDying = false)
		{
			AIActor result = null;
			nearestDistance = float.MaxValue;
			if (activeEnemies == null)
			{
				return null;
			}
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if ((includeBosses || !activeEnemies[i].healthHaver.IsBoss) && (!excludeDying || !activeEnemies[i].healthHaver.IsDead))
				{
					float num = Vector2.Distance(position, activeEnemies[i].CenterPosition);
					if (num < nearestDistance)
					{
						nearestDistance = num;
						result = activeEnemies[i];
					}
				}
			}
			return result;
		}

		public AIActor GetNearestEnemyInDirection(Vector2 position, Vector2 direction, float angleTolerance, out float nearestDistance, bool includeBosses = true)
		{
			AIActor result = null;
			nearestDistance = float.MaxValue;
			float current = direction.ToAngle();
			if (activeEnemies == null)
			{
				return null;
			}
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if (!includeBosses && activeEnemies[i].healthHaver.IsBoss)
				{
					continue;
				}
				Vector2 vector = activeEnemies[i].CenterPosition - position;
				float target = vector.ToAngle();
				float num = Mathf.Abs(Mathf.DeltaAngle(current, target));
				if (num < angleTolerance)
				{
					float magnitude = vector.magnitude;
					if (magnitude < nearestDistance)
					{
						nearestDistance = magnitude;
						result = activeEnemies[i];
					}
				}
			}
			return result;
		}

		public AIActor GetNearestEnemyInDirection(Vector2 position, Vector2 direction, float angleTolerance, out float nearestDistance, bool includeBosses, AIActor excludeActor)
		{
			AIActor result = null;
			nearestDistance = float.MaxValue;
			float current = direction.ToAngle();
			if (activeEnemies == null)
			{
				return null;
			}
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if ((!includeBosses && activeEnemies[i].healthHaver.IsBoss) || activeEnemies[i] == excludeActor)
				{
					continue;
				}
				Vector2 vector = activeEnemies[i].CenterPosition - position;
				float target = vector.ToAngle();
				float num = Mathf.Abs(Mathf.DeltaAngle(current, target));
				if (num < angleTolerance)
				{
					float magnitude = vector.magnitude;
					if (magnitude < nearestDistance)
					{
						nearestDistance = magnitude;
						result = activeEnemies[i];
					}
				}
			}
			return result;
		}

		public void ApplyActionToNearbyEnemies(Vector2 position, float radius, Action<AIActor, float> lambda)
		{
			float num = radius * radius;
			if (activeEnemies == null)
			{
				return;
			}
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if ((bool)activeEnemies[i])
				{
					bool flag = radius < 0f;
					Vector2 vector = activeEnemies[i].CenterPosition - position;
					if (!flag)
					{
						flag = vector.sqrMagnitude < num;
					}
					if (flag)
					{
						lambda(activeEnemies[i], vector.magnitude);
					}
				}
			}
		}

		public bool HasOtherBoss(AIActor boss)
		{
			if (activeEnemies == null)
			{
				return false;
			}
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if (!(activeEnemies[i] == boss) && activeEnemies[i].healthHaver.IsBoss)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasActiveEnemies(ActiveEnemyType type)
		{
			if (activeEnemies == null)
			{
				return false;
			}
			if (type == ActiveEnemyType.RoomClear)
			{
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if (!activeEnemies[i].IgnoreForRoomClear)
					{
						return true;
					}
				}
				return false;
			}
			return activeEnemies.Count > 0;
		}

		public bool TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition condition, bool instant = false)
		{
			bool result = false;
			if (remainingReinforcementLayers == null)
			{
				return false;
			}
			for (int i = 0; i < remainingReinforcementLayers.Count; i++)
			{
				if (remainingReinforcementLayers[i].reinforcementTriggerCondition == condition)
				{
					int index = i;
					bool instant2 = instant;
					result = TriggerReinforcementLayer(index, true, false, -1, -1, instant2);
					break;
				}
			}
			return result;
		}

		public void ResetEnemyHPPercentage()
		{
			m_totalSpawnedEnemyHP = 0f;
			m_lastTotalSpawnedEnemyHP = 0f;
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if (!activeEnemies[i].IgnoreForRoomClear)
				{
					m_totalSpawnedEnemyHP += activeEnemies[i].healthHaver.GetCurrentHealth();
				}
			}
			m_lastTotalSpawnedEnemyHP = m_totalSpawnedEnemyHP;
		}

		private void CheckEnemyHPPercentageEvents()
		{
			float num = 0f;
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if (!activeEnemies[i].IgnoreForRoomClear)
				{
					num += activeEnemies[i].healthHaver.GetCurrentHealth();
				}
			}
			float num2 = m_lastTotalSpawnedEnemyHP / m_totalSpawnedEnemyHP;
			float num3 = num / m_totalSpawnedEnemyHP;
			if (num2 > 0.75f && num3 <= 0.75f)
			{
				ProcessRoomEvents(RoomEventTriggerCondition.ON_ONE_QUARTER_ENEMY_HP_DEPLETED);
				TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.ON_ONE_QUARTER_ENEMY_HP_DEPLETED);
			}
			if (num2 > 0.5f && num3 <= 0.5f)
			{
				ProcessRoomEvents(RoomEventTriggerCondition.ON_HALF_ENEMY_HP_DEPLETED);
				TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.ON_HALF_ENEMY_HP_DEPLETED);
			}
			if (num2 > 0.25f && num3 <= 0.25f)
			{
				ProcessRoomEvents(RoomEventTriggerCondition.ON_THREE_QUARTERS_ENEMY_HP_DEPLETED);
				TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.ON_THREE_QUARTERS_ENEMY_HP_DEPLETED);
			}
			if (num2 > 0.1f && num3 <= 0.1f)
			{
				ProcessRoomEvents(RoomEventTriggerCondition.ON_NINETY_PERCENT_ENEMY_HP_DEPLETED);
				TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.ON_NINETY_PERCENT_ENEMY_HP_DEPLETED);
			}
			m_lastTotalSpawnedEnemyHP = num;
		}

		public void DeregisterEnemy(AIActor enemy, bool suppressClearChecks = false)
		{
			if (activeEnemies == null || !activeEnemies.Contains(enemy))
			{
				DeregisterAutoAimTarget(enemy);
				return;
			}
			activeEnemies.Remove(enemy);
			if (!enemy.IgnoreForRoomClear && !suppressClearChecks)
			{
				CheckEnemyHPPercentageEvents();
				if (!HasActiveEnemies(ActiveEnemyType.RoomClear))
				{
					bool flag = false;
					if (remainingReinforcementLayers != null && remainingReinforcementLayers.Count > 0)
					{
						flag = TriggerReinforcementLayersOnEvent(RoomEventTriggerCondition.ON_ENEMIES_CLEARED);
					}
					flag = flag || numberOfTimedWavesOnDeck > 0 || SpawnSequentialReinforcementWaves();
					if (PreEnemiesCleared != null)
					{
						bool flag2 = PreEnemiesCleared();
						flag = flag || flag2;
					}
					if (!flag || !HasActiveEnemies(ActiveEnemyType.RoomClear))
					{
						ProcessRoomEvents(RoomEventTriggerCondition.ON_ENEMIES_CLEARED);
						GameManager.Instance.DungeonMusicController.NotifyCurrentRoomEnemiesCleared();
						OnEnemiesCleared();
						GameManager.BroadcastRoomTalkDoerFsmEvent("roomCleared");
					}
				}
			}
			DeregisterAutoAimTarget(enemy);
		}

		private bool SpawnSequentialReinforcementWaves()
		{
			int num = -1;
			if (remainingReinforcementLayers == null)
			{
				return false;
			}
			for (int i = 0; i < remainingReinforcementLayers.Count; i++)
			{
				if (remainingReinforcementLayers[i].reinforcementTriggerCondition == RoomEventTriggerCondition.SHRINE_WAVE_A)
				{
					return false;
				}
			}
			for (int j = 0; j < remainingReinforcementLayers.Count; j++)
			{
				if (remainingReinforcementLayers[j].reinforcementTriggerCondition == RoomEventTriggerCondition.SEQUENTIAL_WAVE_TRIGGER)
				{
					num = j;
					break;
				}
			}
			if (num >= 0)
			{
				return TriggerReinforcementLayer(num);
			}
			return false;
		}

		public void BuildSecretRoomCover()
		{
			m_secretRoomCoverObject = SecretRoomBuilder.BuildRoomCover(this, GameManager.Instance.Dungeon.data.tilemap, GameManager.Instance.Dungeon.data);
		}

		private void StampCell(CellData cell, bool isSecretConnection = false)
		{
			cell.type = CellType.FLOOR;
			cell.parentArea = area;
			cell.parentRoom = this;
			if (area.prototypeRoom != null && (area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET || isSecretConnection))
			{
				cell.isSecretRoomCell = true;
			}
			roomCells.Add(cell.position);
			roomCellsWithoutExits.Add(cell.position);
			rawRoomCells.Add(cell.position);
		}

		private void StampCell(int ix, int iy, bool isSecretConnection = false)
		{
			if (ix >= GameManager.Instance.Dungeon.data.Width || iy >= GameManager.Instance.Dungeon.data.Height)
			{
				Debug.LogError("Attempting to stamp " + ix + "," + iy + " in cellData of lengths " + GameManager.Instance.Dungeon.data.Width + "," + GameManager.Instance.Dungeon.data.Height);
			}
			StampCell(GameManager.Instance.Dungeon.data.cellData[ix][iy], isSecretConnection);
		}

		private void StampCellAsExit(int ix, int iy, DungeonData.Direction exitDirection, RoomHandler connectedRoom, bool isSecretConnection = false)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			if (ix < 0 || ix >= data.Width || iy < 0 || iy >= data.Height)
			{
				Debug.LogWarningFormat("Invalid StampCellAsExit({0}, {1}, {2}, {3}, {4}) call!", ix, iy, exitDirection, (connectedRoom != null) ? connectedRoom.ToString() : "null", isSecretConnection);
				return;
			}
			CellData cellData = data.cellData[ix][iy];
			StampCell(ix, iy, isSecretConnection);
			roomCellsWithoutExits.Remove(cellData.position);
			cellData.cellVisualData.roomVisualTypeIndex = m_roomVisualType;
			if (exitDirection == DungeonData.Direction.NORTH || exitDirection == DungeonData.Direction.SOUTH)
			{
				IntVector2 intVector = new IntVector2(ix - 1, iy + 2);
				if (data.CheckInBoundsAndValid(intVector) && data[intVector].type == CellType.WALL)
				{
					data[intVector].cellVisualData.roomVisualTypeIndex = m_roomVisualType;
				}
				IntVector2 intVector2 = new IntVector2(ix + 1, iy + 2);
				if (data.CheckInBoundsAndValid(intVector2) && data[intVector2].type == CellType.WALL)
				{
					data[intVector2].cellVisualData.roomVisualTypeIndex = m_roomVisualType;
				}
			}
			else
			{
				for (int i = -1; i < 4; i++)
				{
					IntVector2 intVector3 = new IntVector2(ix, iy + i);
					if (data.CheckInBoundsAndValid(intVector3) && data[intVector3].type == CellType.WALL)
					{
						data[intVector3].cellVisualData.roomVisualTypeIndex = m_roomVisualType;
					}
					if (!area.PrototypeLostWoodsRoom || GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.RATGEON || !data.CheckInBoundsAndValid(intVector3))
					{
						continue;
					}
					CellData cellData2 = data[intVector3];
					if (data.isAnyFaceWall(intVector3.x, intVector3.y))
					{
						TilesetIndexMetadata.TilesetFlagType key = TilesetIndexMetadata.TilesetFlagType.FACEWALL_UPPER;
						if (data.isFaceWallLower(intVector3.x, intVector3.y))
						{
							key = TilesetIndexMetadata.TilesetFlagType.FACEWALL_LOWER;
						}
						int indexFromTupleArray = SecretRoomUtility.GetIndexFromTupleArray(cellData2, SecretRoomUtility.metadataLookupTableRef[key], cellData2.cellVisualData.roomVisualTypeIndex, 0f);
						cellData2.cellVisualData.faceWallOverrideIndex = indexFromTupleArray;
					}
				}
			}
			cellData.isExitCell = true;
			cellData.exitDirection = exitDirection;
			cellData.connectedRoom2 = connectedRoom;
			cellData.connectedRoom1 = this;
		}

		public void UpdateCellVisualData(int ix, int iy)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			for (int i = -1; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					if (data.CheckInBoundsAndValid(new IntVector2(ix + i, iy + j)))
					{
						data.cellData[ix + i][iy + j].cellVisualData.roomVisualTypeIndex = m_roomVisualType;
					}
				}
			}
		}

		private void HandleStampedCellVisualData(int ix, int iy, PrototypeDungeonRoomCellData sourceCell)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			for (int i = -1; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					if (data.CheckInBounds(ix + i, iy + j))
					{
						data.cellData[ix + i][iy + j].cellVisualData.roomVisualTypeIndex = m_roomVisualType;
					}
				}
			}
		}

		public void RuntimeStampCellComplex(int ix, int iy, CellType type, DiagonalWallType diagonalWallType)
		{
			StampCellComplex(ix, iy, type, diagonalWallType);
		}

		private bool StampCellComplex(int ix, int iy, CellType type, DiagonalWallType diagonalWallType, bool breakable = false)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			if (!data.CheckInBounds(new IntVector2(ix, iy)))
			{
				return false;
			}
			CellData cellData = data.cellData[ix][iy];
			if (type == CellType.WALL && cellData.type != CellType.WALL)
			{
				Debug.LogError("Attempted to stamp intersecting rooms at: " + ix + "," + iy + ". This is a CATEGORY FOUR problem. Talk to Brent.");
				return false;
			}
			cellData.type = type;
			if (!GameManager.Instance.Dungeon.roomMaterialDefinitions[RoomVisualSubtype].supportsDiagonalWalls)
			{
				diagonalWallType = DiagonalWallType.NONE;
			}
			if (cellData.diagonalWallType == DiagonalWallType.NONE || diagonalWallType != 0)
			{
				cellData.diagonalWallType = diagonalWallType;
			}
			if (cellData.diagonalWallType != 0 && cellData.diagonalWallType == diagonalWallType)
			{
				data.cellData[ix][iy + 1].diagonalWallType = diagonalWallType;
				data.cellData[ix][iy + 2].diagonalWallType = diagonalWallType;
			}
			cellData.breakable = breakable;
			if (!GlobalDungeonData.GUNGEON_EXPERIMENTAL && cellData.breakable)
			{
				cellData.breakable = false;
				cellData.type = CellType.FLOOR;
			}
			if (area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET)
			{
				cellData.isSecretRoomCell = true;
			}
			cellData.parentArea = area;
			cellData.parentRoom = this;
			if (data.CheckInBoundsAndValid(ix, iy + 1) && data.cellData[ix][iy + 1].type == CellType.WALL)
			{
				data.cellData[ix][iy + 1].parentRoom = this;
			}
			if (data.CheckInBoundsAndValid(ix, iy + 2) && data.cellData[ix][iy + 2].type == CellType.WALL)
			{
				data.cellData[ix][iy + 2].parentRoom = this;
			}
			if (data.CheckInBoundsAndValid(ix, iy + 3) && data.cellData[ix][iy + 3].type == CellType.WALL)
			{
				data.cellData[ix][iy + 3].parentRoom = this;
			}
			if (type == CellType.PIT)
			{
				cellData.cellVisualData.containsObjectSpaceStamp = true;
			}
			if (type == CellType.FLOOR || type == CellType.PIT || (type == CellType.WALL && cellData.breakable))
			{
				rawRoomCells.Add(cellData.position);
				roomCells.Add(cellData.position);
				roomCellsWithoutExits.Add(cellData.position);
			}
			if (area.prototypeRoom != null && area.prototypeRoom.usesProceduralDecoration)
			{
				if (!area.prototypeRoom.allowWallDecoration)
				{
					cellData.cellVisualData.containsWallSpaceStamp = true;
				}
				if (!area.prototypeRoom.allowFloorDecoration)
				{
					cellData.cellVisualData.containsObjectSpaceStamp = true;
				}
			}
			return true;
		}

		private bool PointInsidePolygon(List<Vector2> points, Vector2 position)
		{
			int index = points.Count - 1;
			bool flag = false;
			for (int i = 0; i < points.Count; i++)
			{
				if (((points[i].y < position.y && points[index].y >= position.y) || (points[index].y < position.y && points[i].y >= position.y)) && (points[i].x <= position.x || points[index].x <= position.x))
				{
					flag ^= points[i].x + (position.y - points[i].y) / (points[index].y - points[i].y) * (points[index].x - points[i].x) < position.x;
				}
				index = i;
			}
			return flag;
		}

		public IntVector2 GetCellAdjacentToExit(RuntimeExitDefinition exitDef)
		{
			IntVector2 result = IntVector2.Zero;
			for (int i = 0; i < CellsWithoutExits.Count; i++)
			{
				CellData cellData = GameManager.Instance.Dungeon.data[CellsWithoutExits[i]];
				CellData exitNeighbor = cellData.GetExitNeighbor();
				if (exitNeighbor != null && (exitNeighbor.position - cellData.position).sqrMagnitude == 1 && exitDef.ContainsPosition(exitNeighbor.position))
				{
					result = cellData.position;
					break;
				}
			}
			return result;
		}

		public RuntimeExitDefinition GetExitDefinitionForConnectedRoom(RoomHandler otherRoom)
		{
			if (!area.IsProceduralRoom)
			{
				return exitDefinitionsByExit[area.exitToLocalDataMap[GetExitConnectedToRoom(otherRoom)]];
			}
			return otherRoom.exitDefinitionsByExit[otherRoom.area.exitToLocalDataMap[otherRoom.GetExitConnectedToRoom(this)]];
		}

		public PrototypeRoomExit GetExitConnectedToRoom(RoomHandler otherRoom)
		{
			for (int i = 0; i < area.instanceUsedExits.Count; i++)
			{
				RoomHandler roomHandler = connectedRoomsByExit[area.instanceUsedExits[i]];
				if (roomHandler == otherRoom)
				{
					return area.instanceUsedExits[i];
				}
			}
			return null;
		}

		public CellData GetNearestFloorFacewall(IntVector2 startPosition)
		{
			CellData result = null;
			float num = float.MaxValue;
			for (int i = 0; i < CellsWithoutExits.Count; i++)
			{
				CellData cellData = GameManager.Instance.Dungeon.data[CellsWithoutExits[i]];
				if (GameManager.Instance.Dungeon.data[cellData.position + IntVector2.Up].IsAnyFaceWall() && cellData.type == CellType.FLOOR)
				{
					float num2 = Vector2.Distance(cellData.position.ToCenterVector2(), startPosition.ToCenterVector2());
					if (num2 < num)
					{
						num = num2;
						result = cellData;
					}
				}
			}
			return result;
		}

		public CellData GetNearestFaceOrSidewall(IntVector2 startPosition)
		{
			CellData result = null;
			float num = float.MaxValue;
			for (int i = 0; i < CellsWithoutExits.Count; i++)
			{
				CellData cellData = GameManager.Instance.Dungeon.data[CellsWithoutExits[i]];
				if (GameManager.Instance.Dungeon.data[cellData.position + IntVector2.Up].IsAnyFaceWall() || cellData.IsSideWallAdjacent())
				{
					float num2 = Vector2.Distance(cellData.position.ToCenterVector2(), startPosition.ToCenterVector2());
					if (num2 < num)
					{
						num = num2;
						result = cellData;
					}
				}
			}
			return result;
		}

		private List<Vector2> GetPolygonDecomposition()
		{
			List<Vector2> list = new List<Vector2>();
			if (!area.IsProceduralRoom)
			{
				Rect boundingRect = GetBoundingRect();
				list.Add(boundingRect.min);
				list.Add(new Vector2(boundingRect.xMin, boundingRect.yMax));
				list.Add(boundingRect.max);
				list.Add(new Vector2(boundingRect.xMax, boundingRect.yMin));
			}
			else
			{
				Rect boundingRect2 = GetBoundingRect();
				list.Add(boundingRect2.min);
				list.Add(new Vector2(boundingRect2.xMin, boundingRect2.yMax));
				list.Add(boundingRect2.max);
				list.Add(new Vector2(boundingRect2.xMax, boundingRect2.yMin));
			}
			return list;
		}

		private Rect GetBoundingRect()
		{
			return new Rect(area.basePosition.x, area.basePosition.y, area.dimensions.x, area.dimensions.y);
		}

		public CellData GetNearestCellToPosition(Vector2 position)
		{
			CellData result = null;
			float num = float.MaxValue;
			for (int i = 0; i < roomCells.Count; i++)
			{
				float num2 = Vector2.Distance(position, roomCells[i].ToVector2());
				if (num2 < num)
				{
					result = GameManager.Instance.Dungeon.data[roomCells[i]];
					num = num2;
				}
			}
			return result;
		}

		public IntVector2 GetRandomAvailableCellDumb()
		{
			for (int i = 0; i < 1000; i++)
			{
				int x = UnityEngine.Random.Range(area.basePosition.x, area.basePosition.x + area.dimensions.x);
				int y = UnityEngine.Random.Range(area.basePosition.y, area.basePosition.y + area.dimensions.y);
				IntVector2 intVector = new IntVector2(x, y);
				if (CheckCellArea(intVector, IntVector2.One))
				{
					return intVector;
				}
			}
			Debug.LogError("No available cells. Error.");
			return IntVector2.Zero;
		}

		public IntVector2? GetOffscreenCell(IntVector2? footprint = null, CellTypes? passableCellTypes = null, bool canPassOccupied = false, Vector2? idealPosition = null)
		{
			if (!footprint.HasValue)
			{
				footprint = IntVector2.One;
			}
			if (!passableCellTypes.HasValue)
			{
				passableCellTypes = (CellTypes)2147483647;
			}
			Dungeon dungeon = GameManager.Instance.Dungeon;
			List<IntVector2> list = new List<IntVector2>();
			for (int i = area.basePosition.x; i < area.basePosition.x + area.dimensions.x; i++)
			{
				for (int j = area.basePosition.y; j < area.basePosition.y + area.dimensions.y; j++)
				{
					IntVector2 intVector = new IntVector2(i, j);
					if (dungeon.data.CheckInBoundsAndValid(intVector) && !GameManager.Instance.MainCameraController.PointIsVisible(intVector.ToCenterVector2()) && Pathfinder.Instance.IsPassable(intVector, footprint, passableCellTypes, canPassOccupied))
					{
						list.Add(intVector);
					}
				}
			}
			if (idealPosition.HasValue)
			{
				if (list.Count > 0)
				{
					list.Sort((IntVector2 a, IntVector2 b) => Mathf.Abs((float)a.y - idealPosition.Value.y).CompareTo(Mathf.Abs((float)b.y - idealPosition.Value.y)));
					return list[0];
				}
			}
			else if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			return null;
		}

		public IntVector2? GetRandomAvailableCell(IntVector2? footprint = null, CellTypes? passableCellTypes = null, bool canPassOccupied = false, CellValidator cellValidator = null)
		{
			if (!footprint.HasValue)
			{
				footprint = IntVector2.One;
			}
			if (!passableCellTypes.HasValue)
			{
				passableCellTypes = (CellTypes)2147483647;
			}
			DungeonData data = GameManager.Instance.Dungeon.data;
			List<IntVector2> list = new List<IntVector2>();
			for (int i = area.basePosition.x; i < area.basePosition.x + area.dimensions.x; i++)
			{
				for (int j = area.basePosition.y; j < area.basePosition.y + area.dimensions.y; j++)
				{
					CellData cellData = data[i, j];
					if (cellData != null && cellData.parentRoom == this && !cellData.isExitCell && (canPassOccupied || !cellData.containsTrap))
					{
						IntVector2 intVector = new IntVector2(i, j);
						if (Pathfinder.Instance.IsPassable(intVector, footprint, passableCellTypes, canPassOccupied, cellValidator))
						{
							list.Add(intVector);
						}
					}
				}
			}
			if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, list.Count)];
			}
			return null;
		}

		public IntVector2? GetNearestAvailableCell(Vector2 nearbyPoint, IntVector2? footprint = null, CellTypes? passableCellTypes = null, bool canPassOccupied = false, CellValidator cellValidator = null)
		{
			if (!footprint.HasValue)
			{
				footprint = IntVector2.One;
			}
			if (!passableCellTypes.HasValue)
			{
				passableCellTypes = (CellTypes)2147483647;
			}
			DungeonData data = GameManager.Instance.Dungeon.data;
			Vector2 vector = footprint.Value.ToVector2() / 2f;
			float num = float.MaxValue;
			IntVector2? result = null;
			for (int i = area.basePosition.x; i < area.basePosition.x + area.dimensions.x; i++)
			{
				for (int j = area.basePosition.y; j < area.basePosition.y + area.dimensions.y; j++)
				{
					CellData cellData = data[i, j];
					if (cellData == null || cellData.parentRoom != this || cellData.isExitCell)
					{
						continue;
					}
					IntVector2 intVector = new IntVector2(i, j);
					if (Pathfinder.Instance.IsPassable(intVector, footprint, passableCellTypes, canPassOccupied, cellValidator))
					{
						Vector2 vector2 = intVector.ToVector2() + vector;
						float sqrMagnitude = (nearbyPoint - vector2).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							result = intVector;
						}
					}
				}
			}
			return result;
		}

		public IntVector2? GetRandomWeightedAvailableCell(IntVector2? footprint = null, CellTypes? passableCellTypes = null, bool canPassOccupied = false, CellValidator cellValidator = null, Func<IntVector2, float> cellWeightFinder = null, float percentageBounds = 1f)
		{
			if (!footprint.HasValue)
			{
				footprint = IntVector2.One;
			}
			if (!passableCellTypes.HasValue)
			{
				passableCellTypes = (CellTypes)2147483647;
			}
			DungeonData data = GameManager.Instance.Dungeon.data;
			List<Tuple<IntVector2, float>> list = new List<Tuple<IntVector2, float>>();
			for (int i = area.basePosition.x; i < area.basePosition.x + area.dimensions.x; i++)
			{
				for (int j = area.basePosition.y; j < area.basePosition.y + area.dimensions.y; j++)
				{
					CellData cellData = data[i, j];
					if (cellData != null && cellData.parentRoom == this && !cellData.isExitCell)
					{
						IntVector2 intVector = new IntVector2(i, j);
						if (Pathfinder.Instance.IsPassable(intVector, footprint, passableCellTypes, canPassOccupied, cellValidator))
						{
							list.Add(Tuple.Create(intVector, cellWeightFinder(intVector)));
						}
					}
				}
			}
			list.Sort(new TupleComparer());
			if (list.Count > 0)
			{
				return list[UnityEngine.Random.Range(0, Mathf.RoundToInt((float)list.Count * percentageBounds))].First;
			}
			return null;
		}

		public IntVector2 GetCenterCell()
		{
			return new IntVector2(area.basePosition.x + Mathf.FloorToInt(area.dimensions.x / 2), area.basePosition.y + Mathf.FloorToInt(area.dimensions.y / 2));
		}

		public void DefineEpicenter(HashSet<IntVector2> startingBorder)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			bool flag = true;
			HashSet<IntVector2> hashSet = startingBorder;
			HashSet<IntVector2> hashSet2 = new HashSet<IntVector2>();
			HashSet<IntVector2> hashSet3 = new HashSet<IntVector2>();
			while (flag)
			{
				flag = false;
				IntVector2[] cardinals = IntVector2.Cardinals;
				foreach (IntVector2 item in hashSet)
				{
					if (data[item].isExitCell)
					{
						continue;
					}
					hashSet3.Add(item);
					for (int i = 0; i < cardinals.Length; i++)
					{
						IntVector2 intVector = item + cardinals[i];
						if (!hashSet3.Contains(intVector) && !hashSet2.Contains(intVector) && data.CheckInBoundsAndValid(intVector))
						{
							CellData cellData = data[intVector];
							if (cellData != null && !cellData.isExitCell && cellData.type != CellType.WALL && cellData.parentRoom == this)
							{
								hashSet2.Add(intVector);
								Epicenter = intVector;
								flag = true;
							}
						}
					}
				}
				hashSet = hashSet2;
				hashSet2 = new HashSet<IntVector2>();
			}
		}

		private List<IntVector2> CollectRandomValidCells(IntVector2 objDimensions, int offset)
		{
			List<IntVector2> list = new List<IntVector2>();
			for (int i = area.basePosition.x + offset; i < area.basePosition.x + area.dimensions.x - offset - (objDimensions.x - 1); i++)
			{
				for (int j = area.basePosition.y + offset; j < area.basePosition.y + area.dimensions.y - offset - (objDimensions.y - 1); j++)
				{
					IntVector2 intVector = new IntVector2(i, j);
					if (CheckCellArea(intVector, objDimensions))
					{
						list.Add(intVector);
					}
				}
			}
			return list;
		}

		public List<TK2DInteriorDecorator.WallExpanse> GatherExpanses(DungeonData.Direction dir, bool breakAfterFirst = true, bool debugMe = false, bool disallowPits = false)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			List<TK2DInteriorDecorator.WallExpanse> list = new List<TK2DInteriorDecorator.WallExpanse>(12);
			bool flag = false;
			TK2DInteriorDecorator.WallExpanse item = default(TK2DInteriorDecorator.WallExpanse);
			IntVector2 intVector2FromDirection = DungeonData.GetIntVector2FromDirection(dir);
			IntVector2 intVector = -1 * intVector2FromDirection;
			int num = ((intVector.x == 0) ? (-1) : ((intVector.x >= 0) ? (-1) : area.dimensions.x));
			int num2 = ((intVector.y == 0) ? (-1) : ((intVector.y >= 0) ? (-1) : area.dimensions.y));
			int num3 = ((intVector.x == 0) ? 1 : ((intVector.x >= 0) ? 1 : (-1)));
			int num4 = ((intVector.y == 0) ? 1 : ((intVector.y >= 0) ? 1 : (-1)));
			bool flag2 = intVector.x != 0;
			IntVector2 intVector2 = ((!flag2) ? IntVector2.Right : IntVector2.Up);
			if (flag2)
			{
				for (int i = num2; i <= area.dimensions.y && i >= -1; i += num4)
				{
					bool flag3 = false;
					for (int j = num; j <= area.dimensions.x && j >= -1; j += num3)
					{
						IntVector2 intVector3 = area.basePosition + new IntVector2(j, i);
						CellData cellData = data[intVector3];
						bool flag4 = cellData == null || cellData.type == CellType.WALL;
						CellData cellData2 = data[intVector3 + intVector2FromDirection];
						bool flag5 = cellData2 == null || ((cellData2.type == CellType.WALL || data.isAnyFaceWall(cellData2.position.x, cellData2.position.y)) && !cellData2.isExitCell);
						if (flag5 && cellData2 != null && cellData2.diagonalWallType != 0)
						{
							flag5 = false;
						}
						if (flag4 || (disallowPits && cellData.type == CellType.PIT) || cellData.parentRoom != this || !flag5)
						{
							continue;
						}
						flag3 = true;
						if (cellData.isExitCell)
						{
							if (flag)
							{
								list.Add(item);
							}
							flag = false;
							break;
						}
						if (!flag)
						{
							flag = true;
							item = new TK2DInteriorDecorator.WallExpanse(new IntVector2(j, i), 1);
						}
						else if (flag2 && item.basePosition.x == j)
						{
							item.width++;
						}
						else if (!flag2 && item.basePosition.y == i)
						{
							item.width++;
						}
						else
						{
							list.Add(item);
							item = new TK2DInteriorDecorator.WallExpanse(new IntVector2(j, i), 1);
						}
						if (breakAfterFirst)
						{
							break;
						}
					}
					if (!flag3)
					{
						if (flag)
						{
							list.Add(item);
						}
						flag = false;
					}
				}
			}
			else
			{
				for (int k = num; k <= area.dimensions.x && k >= -1; k += num3)
				{
					bool flag6 = false;
					for (int l = num2; l <= area.dimensions.y && l >= -1; l += num4)
					{
						IntVector2 intVector4 = area.basePosition + new IntVector2(k, l);
						CellData cellData3 = data[intVector4];
						bool flag7 = cellData3 == null || cellData3.type == CellType.WALL;
						CellData cellData4 = data[intVector4 + intVector2FromDirection];
						bool flag8 = cellData4 == null || ((cellData4.type == CellType.WALL || data.isAnyFaceWall(cellData4.position.x, cellData4.position.y)) && !cellData4.isExitCell);
						if (flag8 && cellData4 != null && cellData4.diagonalWallType != 0)
						{
							flag8 = false;
						}
						if (flag7 || cellData3.parentRoom != this || !flag8)
						{
							continue;
						}
						flag6 = true;
						if (cellData3.isExitCell)
						{
							if (flag)
							{
								list.Add(item);
							}
							flag = false;
							break;
						}
						if (!flag)
						{
							flag = true;
							item = new TK2DInteriorDecorator.WallExpanse(new IntVector2(k, l), 1);
						}
						else if (flag2 && item.basePosition.x == k)
						{
							item.width++;
						}
						else if (!flag2 && item.basePosition.y == l)
						{
							item.width++;
						}
						else
						{
							list.Add(item);
							item = new TK2DInteriorDecorator.WallExpanse(new IntVector2(k, l), 1);
						}
						if (breakAfterFirst)
						{
							break;
						}
					}
					if (!flag6)
					{
						if (flag)
						{
							list.Add(item);
						}
						flag = false;
					}
				}
			}
			if (flag && !list.Contains(item))
			{
				list.Add(item);
			}
			if (debugMe)
			{
				foreach (TK2DInteriorDecorator.WallExpanse item2 in list)
				{
					for (int m = 0; m < item2.width; m++)
					{
						BraveUtility.DrawDebugSquare(area.basePosition + item2.basePosition + intVector2 * m, Color.yellow);
					}
				}
				return list;
			}
			return list;
		}

		public List<IntVector2> GatherPitLighting(TilemapDecoSettings decoSettings, List<IntVector2> existingLights)
		{
			float num = decoSettings.lightOverlapRadius - 2;
			List<IntVector2> list = new List<IntVector2>();
			for (int i = 0; i < Cells.Count; i++)
			{
				IntVector2 intVector = Cells[i];
				bool flag = true;
				for (int j = 0; j < list.Count; j++)
				{
					if ((float)IntVector2.ManhattanDistance(intVector, list[j] + area.basePosition) <= num)
					{
						flag = false;
					}
				}
				for (int k = 0; k < existingLights.Count; k++)
				{
					if ((float)IntVector2.ManhattanDistance(intVector, existingLights[k] + area.basePosition) <= num)
					{
						flag = false;
					}
				}
				if (flag && GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
				{
					CellData cellData = GameManager.Instance.Dungeon.data[intVector];
					if (cellData.type == CellType.PIT && !cellData.SurroundedByPits())
					{
						list.Add(intVector - area.basePosition);
					}
				}
			}
			return list;
		}

		private List<IntVector2> GatherLightPositionForDirection(TilemapDecoSettings decoSettings, DungeonData.Direction dir)
		{
			float num = decoSettings.lightOverlapRadius;
			List<IntVector2> list = new List<IntVector2>();
			IntVector2 intVector = ((dir != 0 && dir != DungeonData.Direction.SOUTH) ? IntVector2.Up : IntVector2.Right);
			List<TK2DInteriorDecorator.WallExpanse> list2 = GatherExpanses(dir);
			for (int i = 0; i < list2.Count; i++)
			{
				TK2DInteriorDecorator.WallExpanse wallExpanse = list2[i];
				if (wallExpanse.width < decoSettings.minLightExpanseWidth)
				{
					continue;
				}
				if ((float)wallExpanse.width < num * 2f)
				{
					IntVector2 item = wallExpanse.basePosition + intVector * Mathf.FloorToInt((float)wallExpanse.width / 2f);
					list.Add(item);
					continue;
				}
				int num2 = Mathf.FloorToInt((float)wallExpanse.width / num);
				int num3 = Mathf.FloorToInt(((float)wallExpanse.width - (float)(num2 - 1) * num) / 2f);
				for (int j = 0; j < num2; j++)
				{
					int num4 = num3 + Mathf.FloorToInt(num) * j;
					IntVector2 item2 = wallExpanse.basePosition + intVector * num4;
					list.Add(item2);
				}
			}
			return list;
		}

		public List<IntVector2> GatherManualLightPositions()
		{
			List<IntVector2> list = new List<IntVector2>();
			for (int i = area.basePosition.x; i < area.basePosition.x + area.dimensions.x; i++)
			{
				for (int j = area.basePosition.y; j < area.basePosition.y + area.dimensions.y; j++)
				{
					int ix = i - area.basePosition.x;
					int iy = j - area.basePosition.y;
					PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = area.prototypeRoom.ForceGetCellDataAtPoint(ix, iy);
					if (prototypeDungeonRoomCellData.containsManuallyPlacedLight)
					{
						list.Add(new IntVector2(i, j) - area.basePosition);
					}
				}
			}
			return list;
		}

		public List<IntVector2> GatherOptimalLightPositions(TilemapDecoSettings decoSettings)
		{
			List<IntVector2> list = GatherLightPositionForDirection(decoSettings, DungeonData.Direction.NORTH);
			list.AddRange(GatherLightPositionForDirection(decoSettings, DungeonData.Direction.EAST));
			list.AddRange(GatherLightPositionForDirection(decoSettings, DungeonData.Direction.SOUTH));
			list.AddRange(GatherLightPositionForDirection(decoSettings, DungeonData.Direction.WEST));
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (i == j)
					{
						continue;
					}
					float num = Vector2.Distance(list[i].ToVector2(), list[j].ToVector2());
					if (num < decoSettings.nearestAllowedLight)
					{
						if (list[i].y < list[j].y)
						{
							list.RemoveAt(i);
							i--;
							break;
						}
						list.RemoveAt(j);
						j--;
						if (i > j)
						{
							i--;
						}
					}
				}
			}
			return list;
		}

		private bool CheckCellArea(IntVector2 basePosition, IntVector2 objDimensions)
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			bool result = true;
			for (int i = basePosition.x; i < basePosition.x + objDimensions.x; i++)
			{
				int num = basePosition.y;
				while (num < basePosition.y + objDimensions.y)
				{
					CellData cellData = data.cellData[i][num];
					if (cellData.IsPassable)
					{
						num++;
						continue;
					}
					goto IL_0044;
				}
				continue;
				IL_0044:
				result = false;
				break;
			}
			return result;
		}

		private bool CellWithinRadius(Vector2 center, float radius, IntVector2 cubePos)
		{
			Vector2 b = new Vector2((float)cubePos.x + 0.5f, (float)cubePos.y + 0.5f);
			float num = Vector2.Distance(center, b);
			if (radius >= num)
			{
				return true;
			}
			return false;
		}
	}
}
