using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Dungeonator;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpeculativeRigidbody))]
[RequireComponent(typeof(HitEffectHandler))]
public class AIActor : GameActor, IPlaceConfigurable
{
	public enum ReinforceType
	{
		FullVfx,
		SkipVfx,
		Instant
	}

	public enum ShadowDeathType
	{
		Fade = 10,
		Scale = 20,
		None = 30
	}

	public enum EnemyTypeIdentifier
	{
		UNIDENTIFIED,
		SNIPER_TYPE
	}

	public enum EnemyChampionType
	{
		NORMAL,
		JAMMED,
		KTHULIBER_JAMMED
	}

	public delegate void CustomPitHandlingDelegate(AIActor actor, ref bool suppressDamage);

	public enum ActorState
	{
		Inactive,
		Awakening,
		Normal
	}

	public enum AwakenAnimationType
	{
		Default,
		Awaken,
		Spawn
	}

	[Serializable]
	public class HealthOverride
	{
		public float HealthPercentage;

		public string Stat;

		public float Value;

		[NonSerialized]
		public bool HasBeenUsed;
	}

	private static readonly string[] s_floorTypeNames;

	private static float m_healthModifier;

	[HideInInspector]
	public int EnemyId = -1;

	[DisableInInspector]
	public string EnemyGuid;

	[DisableInInspector]
	public int ForcedPositionInAmmonomicon = -1;

	[Header("Flags")]
	public bool SetsFlagOnDeath;

	[LongEnum]
	public GungeonFlags FlagToSetOnDeath;

	public bool SetsFlagOnActivation;

	[LongEnum]
	public GungeonFlags FlagToSetOnActivation;

	public bool SetsCharacterSpecificFlagOnDeath;

	[LongEnum]
	public CharacterSpecificGungeonFlags CharacterSpecificFlagToSetOnDeath;

	[Header("Core Enemy Stats")]
	public bool IsNormalEnemy = true;

	public bool IsSignatureEnemy;

	public bool IsHarmlessEnemy;

	[HideInInspectorIf("IsNormalEnemy", false)]
	public ActorCompanionSettings CompanionSettings;

	[NonSerialized]
	public bool ForceBlackPhantom;

	[NonSerialized]
	public bool PreventBlackPhantom;

	[NonSerialized]
	public bool IsInReinforcementLayer;

	[NonSerialized]
	public PlayerController CompanionOwner;

	[FormerlySerializedAs("m_movementSpeed")]
	[SerializeField]
	public float MovementSpeed = 2f;

	[EnumFlags]
	public CellTypes PathableTiles = CellTypes.FLOOR;

	public GameObject LosPoint;

	[Header("Collision Data")]
	public bool DiesOnCollison;

	public float CollisionDamage = 1f;

	public float CollisionKnockbackStrength = 5f;

	public CoreDamageTypes CollisionDamageTypes;

	public float EnemyCollisionKnockbackStrengthOverride = -1f;

	public VFXPool CollisionVFX;

	public VFXPool NonActorCollisionVFX;

	public bool CollisionSetsPlayerOnFire;

	public bool TryDodgeBullets = true;

	public float AvoidRadius = 4f;

	public bool ReflectsProjectilesWhileInvulnerable;

	public bool HitByEnemyBullets;

	public bool HasOverrideDodgeRollDeath;

	[ShowInInspectorIf("HasOverrideDodgeRollDeath", false)]
	public string OverrideDodgeRollDeath;

	[Header("Loot Settings")]
	public bool CanDropCurrency = true;

	public float AdditionalSingleCoinDropChance;

	[NonSerialized]
	public int AssignedCurrencyToDrop;

	public bool CanDropItems = true;

	[ShowInInspectorIf("CanDropCurrency", true)]
	public GenericLootTable CustomLootTable;

	public bool CanDropDuplicateItems;

	public int CustomLootTableMinDrops = 1;

	public int CustomLootTableMaxDrops = 1;

	public GenericLootTable CustomChestTable;

	public float ChanceToDropCustomChest;

	public bool IgnoreForRoomClear;

	[NonSerialized]
	[HideInInspector]
	public List<PickupObject> AdditionalSimpleItemDrops = new List<PickupObject>();

	[NonSerialized]
	[HideInInspector]
	public List<PickupObject> AdditionalSafeItemDrops = new List<PickupObject>();

	public bool SpawnLootAtRewardChestPos;

	[Header("Extra Visual Settings")]
	public GameObject CorpseObject;

	[ShowInInspectorIf("CorpseObject", true)]
	public bool CorpseShadow = true;

	[ShowInInspectorIf("CorpseObject", true)]
	public bool TransferShadowToCorpse;

	public ShadowDeathType shadowDeathType = ShadowDeathType.Fade;

	public bool PreventDeathKnockback;

	public VFXPool OnCorpseVFX;

	public GameObject OnEngagedVFX;

	public tk2dBaseSprite.Anchor OnEngagedVFXAnchor;

	public float shadowHeightOffset;

	public bool invisibleUntilAwaken;

	public bool procedurallyOutlined = true;

	public bool forceUsesTrimmedBounds = true;

	public ReinforceType reinforceType;

	public Texture2D optionalPalette;

	public bool UsesVaryingEmissiveShaderPropertyBlock;

	public Transform OverrideBuffEffectPosition;

	[Header("Audio")]
	public string EnemySwitchState;

	public string OverrideSpawnReticleAudio;

	public string OverrideSpawnAppearAudio;

	public bool UseMovementAudio;

	[ShowInInspectorIf("UseMovementAudio", true)]
	public string StartMovingEvent;

	[ShowInInspectorIf("UseMovementAudio", true)]
	public string StopMovingEvent;

	private bool m_audioMovedLastFrame;

	[SerializeField]
	public List<ActorAudioEvent> animationAudioEvents;

	[Header("Other")]
	public List<HealthOverride> HealthOverrides;

	public EnemyTypeIdentifier IdentifierForEffects;

	[HideInInspector]
	public bool BehaviorOverridesVelocity;

	[HideInInspector]
	public Vector2 BehaviorVelocity = Vector2.zero;

	public Vector2? OverridePathVelocity;

	public bool AlwaysShowOffscreenArrow;

	[NonSerialized]
	public float BaseMovementSpeed;

	[NonSerialized]
	public float LocalTimeScale = 1f;

	[NonSerialized]
	public bool UniquePlayerTargetFlag;

	private bool? m_cachedIsMimicEnemy;

	[NonSerialized]
	public bool HasBeenBloodthirstProcessed;

	[NonSerialized]
	public bool CanBeBloodthirsted;

	private Vector2 m_currentlyAppliedEnemyScale = Vector2.one;

	private bool m_canTargetPlayers = true;

	private bool m_canTargetEnemies;

	public BlackPhantomProperties BlackPhantomProperties;

	public bool ForceBlackPhantomParticles;

	public bool OverrideBlackPhantomParticlesCollider;

	[ShowInInspectorIf("OverrideBlackPhantomParticlesCollider", true)]
	public int BlackPhantomParticlesCollider;

	private EnemyChampionType m_championType;

	private bool? m_isWorthShootingAt;

	public bool PreventFallingInPitsEver;

	private bool m_isPaletteSwapped;

	[NonSerialized]
	private Color? OverrideOutlineColor;

	private bool m_cachedTurboness;

	private bool m_turboWake;

	private int m_cachedBodySpriteCount;

	private Shader m_cachedBodySpriteShader;

	private Shader m_cachedGunSpriteShader;

	private bool? ShouldDoBlackPhantomParticles;

	private const float c_particlesPerSecond = 40f;

	private List<IntVector2> m_upcomingPathTiles = new List<IntVector2>();

	private bool m_cachedHasLineOfSightToTarget;

	private SpeculativeRigidbody m_cachedLosTarget;

	private int m_cachedLosFrame;

	private Vector2 m_lastPosition;

	private IntVector2? m_clearance;

	private CellVisualData.CellFloorType? m_prevFloorType;

	protected Action OnPostStartInitialization;

	private bool m_hasGivenRewards;

	public Action OnHandleRewards;

	private bool m_isSafeMoving;

	private float m_safeMoveTimer;

	private float m_safeMoveTime;

	private Vector2? m_safeMoveStartPos;

	private Vector2? m_safeMoveEndPos;

	private CustomEngageDoer m_customEngageDoer;

	private CustomReinforceDoer m_customReinforceDoer;

	private Func<SpeculativeRigidbody, bool> m_rigidbodyExcluder;

	private Vector3 m_spriteDimensions;

	private Vector3 m_spawnPosition;

	private RoomHandler parentRoom;

	private int m_currentPhase;

	private bool m_isReadyForRepath = true;

	private Path m_currentPath;

	private Vector2? m_overridePathEnd;

	private int m_strafeDirection = 1;

	private bool m_hasBeenEngaged;

	private string m_awakenAnimation;

	protected bool? m_forcedOutlines;

	private Vector2 m_knockbackVelocity;

	public const float c_minStartingDistanceFromPlayer = 8f;

	public const float c_maxCloseStartingDistanceFromPlayer = 15f;

	public static float BaseLevelHealthModifier
	{
		get
		{
			float num = 1f;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				num = GameManager.Instance.COOP_ENEMY_HEALTH_MULTIPLIER;
			}
			GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
			if (lastLoadedLevelDefinition != null)
			{
				num *= lastLoadedLevelDefinition.enemyHealthMultiplier;
			}
			return num;
		}
	}

	public static float HealthModifier
	{
		get
		{
			return m_healthModifier;
		}
		set
		{
			float healthModifier = m_healthModifier;
			m_healthModifier = value;
			for (int i = 0; i < StaticReferenceManager.AllEnemies.Count; i++)
			{
				if (StaticReferenceManager.AllEnemies[i] != null && (bool)StaticReferenceManager.AllEnemies[i])
				{
					HealthHaver healthHaver = StaticReferenceManager.AllEnemies[i].healthHaver;
					if (!healthHaver.healthIsNumberOfHits)
					{
						healthHaver.SetHealthMaximum(healthHaver.GetMaxHealth() / healthModifier * m_healthModifier);
					}
				}
			}
		}
	}

	public float EnemyCollisionKnockbackStrength
	{
		get
		{
			return (!(EnemyCollisionKnockbackStrengthOverride >= 0f)) ? CollisionKnockbackStrength : EnemyCollisionKnockbackStrengthOverride;
		}
	}

	public Vector2 VoluntaryMovementVelocity
	{
		get
		{
			if ((bool)base.behaviorSpeculator && base.behaviorSpeculator.IsStunned)
			{
				return Vector2.zero;
			}
			if (BehaviorOverridesVelocity)
			{
				return BehaviorVelocity;
			}
			return GetPathVelocityContribution();
		}
	}

	public bool IsMimicEnemy
	{
		get
		{
			bool? cachedIsMimicEnemy = m_cachedIsMimicEnemy;
			if (!cachedIsMimicEnemy.HasValue)
			{
				m_cachedIsMimicEnemy = false;
				if ((bool)base.encounterTrackable && !string.IsNullOrEmpty(base.encounterTrackable.EncounterGuid))
				{
					m_cachedIsMimicEnemy = base.encounterTrackable.EncounterGuid == GlobalEncounterGuids.Mimic;
				}
			}
			return m_cachedIsMimicEnemy.Value;
		}
	}

	public float LocalDeltaTime
	{
		get
		{
			if (IsBlackPhantom)
			{
				return BraveTime.DeltaTime * LocalTimeScale * BlackPhantomProperties.LocalTimeScaleMultiplier;
			}
			return BraveTime.DeltaTime * LocalTimeScale;
		}
	}

	public Vector2 EnemyScale
	{
		get
		{
			return m_currentlyAppliedEnemyScale;
		}
		set
		{
			m_currentlyAppliedEnemyScale = value;
			base.transform.localScale = value.ToVector3ZUp(1f);
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.UpdateCollidersOnScale = true;
				base.specRigidbody.RegenerateColliders = true;
			}
		}
	}

	[HideInInspector]
	public bool HasDamagedPlayer { get; set; }

	public bool CanTargetPlayers
	{
		get
		{
			return m_canTargetPlayers;
		}
		set
		{
			PlayerTarget = null;
			m_canTargetPlayers = value;
		}
	}

	public bool CanTargetEnemies
	{
		get
		{
			return m_canTargetEnemies;
		}
		set
		{
			PlayerTarget = null;
			m_canTargetEnemies = value;
		}
	}

	public bool OverrideHitEnemies
	{
		get
		{
			int num = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider);
			PixelCollider pixelCollider = base.specRigidbody.GetPixelCollider(ColliderType.Ground);
			return (pixelCollider.CollisionLayerCollidableOverride & num) == num;
		}
		set
		{
			int num = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider);
			PixelCollider pixelCollider = base.specRigidbody.GetPixelCollider(ColliderType.Ground);
			if (value)
			{
				pixelCollider.CollisionLayerCollidableOverride |= num;
			}
			else
			{
				pixelCollider.CollisionLayerCollidableOverride &= ~num;
			}
		}
	}

	public bool IsOverPit
	{
		get
		{
			Vector2 vector = ((!(base.specRigidbody != null)) ? base.CenterPosition : base.specRigidbody.GroundPixelCollider.UnitCenter);
			return GameManager.Instance.Dungeon.CellSupportsFalling(vector);
		}
	}

	public bool IsBlackPhantom
	{
		get
		{
			return m_championType == EnemyChampionType.JAMMED;
		}
	}

	public bool SuppressBlackPhantomCorpseBurn { get; set; }

	public Shader OverrideBlackPhantomShader { get; set; }

	public bool IsBuffEnemy { get; set; }

	public SpeculativeRigidbody TargetRigidbody
	{
		get
		{
			if (OverrideTarget != null)
			{
				if ((bool)OverrideTarget)
				{
					return OverrideTarget;
				}
				OverrideTarget = null;
			}
			if (PlayerTarget != null)
			{
				return PlayerTarget.specRigidbody;
			}
			return null;
		}
	}

	public Vector2 TargetVelocity
	{
		get
		{
			if ((bool)OverrideTarget)
			{
				PlayerController playerController = OverrideTarget.gameActor as PlayerController;
				if ((bool)playerController)
				{
					return playerController.AverageVelocity;
				}
				return OverrideTarget.Velocity;
			}
			if ((bool)PlayerTarget)
			{
				PlayerController playerController2 = PlayerTarget as PlayerController;
				if ((bool)playerController2)
				{
					return playerController2.AverageVelocity;
				}
				return PlayerTarget.specRigidbody.Velocity;
			}
			return Vector2.zero;
		}
	}

	public float SpeculatorDelayTime { get; set; }

	public bool IsWorthShootingAt
	{
		get
		{
			return (!m_isWorthShootingAt.HasValue) ? (!IsHarmlessEnemy) : m_isWorthShootingAt.Value;
		}
		set
		{
			m_isWorthShootingAt = value;
		}
	}

	public bool HasDonePlayerEnterCheck { get; set; }

	public bool PreventAutoKillOnBossDeath { get; set; }

	public string OverridePitfallAnim { get; set; }

	public GameActor PlayerTarget { get; set; }

	public SpeculativeRigidbody OverrideTarget { get; set; }

	public RoomHandler ParentRoom
	{
		get
		{
			return parentRoom;
		}
		set
		{
			parentRoom = value;
		}
	}

	public bool HasBeenGlittered { get; set; }

	public bool IsTransmogrified { get; set; }

	public ActorState State { get; set; }

	public bool HasBeenAwoken
	{
		get
		{
			return State != 0 && State != ActorState.Awakening;
		}
	}

	public AwakenAnimationType AwakenAnimType { get; set; }

	public virtual bool InBossAmmonomiconTab
	{
		get
		{
			return (bool)base.healthHaver && base.healthHaver.IsBoss && !base.healthHaver.IsSubboss;
		}
	}

	private Color OutlineColor
	{
		get
		{
			if (OverrideOutlineColor.HasValue)
			{
				return OverrideOutlineColor.Value;
			}
			return (!CanBeBloodthirsted) ? Color.black : Color.red;
		}
	}

	public Vector3 SpawnPosition
	{
		get
		{
			return m_spawnPosition;
		}
	}

	public IntVector2 SpawnGridPosition
	{
		get
		{
			return m_spawnPosition.IntXY(VectorConversions.Floor);
		}
	}

	public Vector3 Position
	{
		get
		{
			return base.specRigidbody.UnitCenter;
		}
	}

	public IntVector2 GridPosition
	{
		get
		{
			return base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);
		}
	}

	public IntVector2 PathTile
	{
		get
		{
			return base.specRigidbody.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
		}
	}

	public bool PathComplete
	{
		get
		{
			int result;
			if (m_currentPath == null || m_currentPath.Count == 0)
			{
				Vector2? overridePathEnd = m_overridePathEnd;
				result = ((!overridePathEnd.HasValue) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}
	}

	public Path Path
	{
		get
		{
			return m_currentPath;
		}
	}

	public float DistanceToTarget
	{
		get
		{
			SpeculativeRigidbody targetRigidbody = TargetRigidbody;
			if (TargetRigidbody == null)
			{
				return 0f;
			}
			return Vector2.Distance(base.specRigidbody.UnitCenter, targetRigidbody.GetUnitCenter(ColliderType.HitBox));
		}
	}

	public bool HasLineOfSightToTarget
	{
		get
		{
			if (TargetRigidbody != m_cachedLosTarget || Time.frameCount != m_cachedLosFrame)
			{
				m_cachedHasLineOfSightToTarget = HasLineOfSightToRigidbody(TargetRigidbody);
				m_cachedLosTarget = TargetRigidbody;
				m_cachedLosFrame = Time.frameCount;
			}
			return m_cachedHasLineOfSightToTarget;
		}
	}

	public float DesiredCombatDistance
	{
		get
		{
			if (base.behaviorSpeculator == null)
			{
				return -1f;
			}
			return base.behaviorSpeculator.GetDesiredCombatDistance();
		}
	}

	public IntVector2 Clearance
	{
		get
		{
			IntVector2? clearance = m_clearance;
			if (!clearance.HasValue)
			{
				m_clearance = base.specRigidbody.UnitDimensions.ToIntVector2(VectorConversions.Ceil);
			}
			return m_clearance.Value;
		}
	}

	public int CurrentPhase
	{
		get
		{
			return m_currentPhase;
		}
		set
		{
			m_currentPhase = value;
		}
	}

	public bool HasBeenEngaged
	{
		get
		{
			return m_hasBeenEngaged;
		}
		set
		{
			if (value && !m_hasBeenEngaged)
			{
				OnEngaged();
			}
		}
	}

	public bool IsReadyForRepath
	{
		get
		{
			return m_isReadyForRepath;
		}
	}

	public Vector2 KnockbackVelocity
	{
		get
		{
			return m_knockbackVelocity;
		}
		set
		{
			m_knockbackVelocity = value;
		}
	}

	public override Vector3 SpriteDimensions
	{
		get
		{
			return m_spriteDimensions;
		}
	}

	public override Gun CurrentGun
	{
		get
		{
			return (!(base.aiShooter != null)) ? null : base.aiShooter.CurrentGun;
		}
	}

	public override bool SpriteFlipped
	{
		get
		{
			return (!(base.aiAnimator != null)) ? base.sprite.FlipX : base.aiAnimator.SpriteFlipped;
		}
	}

	public override Transform GunPivot
	{
		get
		{
			return (!(base.aiShooter != null)) ? null : base.aiShooter.gunAttachPoint;
		}
	}

	public bool ManualKnockbackHandling { get; set; }

	public bool SuppressTargetSwitch { get; set; }

	public bool HasSplashed { get; set; }

	public event CustomPitHandlingDelegate CustomPitDeathHandling;

	static AIActor()
	{
		m_healthModifier = 1f;
		s_floorTypeNames = Enum.GetNames(typeof(CellVisualData.CellFloorType));
	}

	public static void ClearPerLevelData()
	{
		StaticReferenceManager.AllEnemies.Clear();
	}

	public static AIActor Spawn(AIActor prefabActor, IntVector2 position, RoomHandler source, bool correctForWalls = false, AwakenAnimationType awakenAnimType = AwakenAnimationType.Default, bool autoEngage = true)
	{
		if (!prefabActor)
		{
			return null;
		}
		GameObject realPrefab = prefabActor.gameObject;
		if (prefabActor is AIActorDummy)
		{
			realPrefab = (prefabActor as AIActorDummy).realPrefab;
		}
		GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(realPrefab, source, position - source.area.basePosition, false, awakenAnimType, autoEngage);
		if (!gameObject)
		{
			return null;
		}
		AIActor component = gameObject.GetComponent<AIActor>();
		if (!component)
		{
			return null;
		}
		component.specRigidbody.Initialize();
		if (correctForWalls)
		{
			component.CorrectForWalls();
		}
		return component;
	}

	public static AIActor Spawn(AIActor prefabActor, Vector2 position, RoomHandler source, bool correctForWalls = false, AwakenAnimationType awakenAnimType = AwakenAnimationType.Default, bool autoEngage = true)
	{
		GameObject realPrefab = prefabActor.gameObject;
		if (prefabActor is AIActorDummy)
		{
			realPrefab = (prefabActor as AIActorDummy).realPrefab;
		}
		IntVector2 intVector = position.ToIntVector2(VectorConversions.Floor);
		GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(realPrefab, source, intVector - source.area.basePosition, false, awakenAnimType, autoEngage);
		if (!gameObject)
		{
			return null;
		}
		AIActor component = gameObject.GetComponent<AIActor>();
		if (!component)
		{
			return null;
		}
		component.specRigidbody.Initialize();
		component.transform.position -= (Vector3)(component.specRigidbody.UnitCenter - position);
		component.specRigidbody.Reinitialize();
		if (correctForWalls)
		{
			component.CorrectForWalls();
		}
		return component;
	}

	private void CorrectForWalls()
	{
		if (!PhysicsEngine.Instance.OverlapCast(base.specRigidbody, null, true, false, null, null, false, null, null))
		{
			return;
		}
		Vector2 vector = base.transform.position.XY();
		IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
		int num = 0;
		int num2 = 1;
		do
		{
			for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
			{
				base.transform.position = vector + PhysicsEngine.PixelToUnit(cardinalsAndOrdinals[i] * num2);
				base.specRigidbody.Reinitialize();
				if (!PhysicsEngine.Instance.OverlapCast(base.specRigidbody, null, true, false, null, null, false, null, null))
				{
					return;
				}
			}
			num2++;
			num++;
		}
		while (num <= 200);
		Debug.LogError("FREEZE AVERTED!  TELL RUBEL!  (you're welcome) 147");
	}

	public override void Awake()
	{
		base.Awake();
		BaseMovementSpeed = MovementSpeed;
		m_currentlyAppliedEnemyScale = Vector2.one;
		StaticReferenceManager.AllEnemies.Add(this);
		if ((bool)base.healthHaver && base.healthHaver.healthIsNumberOfHits)
		{
			base.healthHaver.SetHealthMaximum(base.healthHaver.GetMaxHealth());
		}
		else
		{
			base.healthHaver.SetHealthMaximum(base.healthHaver.GetMaxHealth() * BaseLevelHealthModifier);
			base.healthHaver.SetHealthMaximum(base.healthHaver.GetMaxHealth() * HealthModifier);
		}
		if (GameManager.Instance.InTutorial && base.name.Contains("turret", true))
		{
			HasDonePlayerEnterCheck = true;
		}
		m_customEngageDoer = GetComponent<CustomEngageDoer>();
		m_customReinforceDoer = GetComponent<CustomReinforceDoer>();
		m_rigidbodyExcluder = RigidbodyBlocksLineOfSight;
		if (base.aiShooter != null)
		{
			base.aiShooter.Initialize();
		}
		InitializeCallbacks();
	}

	public override void Start()
	{
		base.Start();
		if (UsesVaryingEmissiveShaderPropertyBlock && base.sprite is tk2dSprite)
		{
			tk2dSprite tk2dSprite2 = base.sprite as tk2dSprite;
			tk2dSprite2.ApplyEmissivePropertyBlock = true;
		}
		if (GameManager.Instance.InTutorial && base.name.Contains("turret", true))
		{
			List<AIActor> activeEnemies = parentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			Transform transform = base.transform;
			Transform transform2 = base.transform;
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				AIActor aIActor = activeEnemies[i];
				if (aIActor.name.Contains("turret", true))
				{
					if (aIActor.transform.position.y < transform.position.y)
					{
						transform = aIActor.transform;
					}
					if (aIActor.transform.position.y > transform2.position.y)
					{
						transform2 = aIActor.transform;
					}
				}
			}
			if (transform != base.transform && transform2 != base.transform)
			{
				foreach (AIBulletBank.Entry bullet in base.bulletBank.Bullets)
				{
					bullet.PlayAudio = false;
				}
			}
		}
		if (PreventFallingInPitsEver && (bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(NoPitsMovementRestrictor));
		}
		if (!string.IsNullOrEmpty(EnemySwitchState))
		{
			AkSoundEngine.SetSwitch("CHR_Enemy", EnemySwitchState, base.gameObject);
		}
		m_spriteDimensions = base.sprite.GetUntrimmedBounds().size;
		if (forceUsesTrimmedBounds)
		{
			base.sprite.depthUsesTrimmedBounds = true;
		}
		base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y, -1f);
		DepthLookupManager.ProcessRenderer(base.renderer);
		m_spawnPosition = base.transform.position;
		if (HitByEnemyBullets)
		{
			base.specRigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.Projectile));
		}
		if (HasShadow)
		{
			if (!ShadowObject)
			{
				GenerateDefaultBlobShadow(shadowHeightOffset);
			}
			tk2dBaseSprite component = ShadowObject.GetComponent<tk2dSprite>();
			base.sprite.AttachRenderer(component);
			component.HeightOffGround = -0.05f;
			if ((bool)ShadowParent)
			{
				component.transform.parent = ShadowParent;
				component.transform.localPosition = Vector3.zero;
			}
			if (GameManager.Instance.InTutorial && base.name.Contains("turret", true))
			{
				component.renderer.enabled = false;
			}
		}
		base.gameObject.GetOrAddComponent<AkGameObj>();
		m_lastPosition = base.specRigidbody.UnitCenter;
		foreach (PixelCollider pixelCollider in base.specRigidbody.PixelColliders)
		{
			if (pixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.BagelCollider && pixelCollider.CollisionLayer == CollisionLayer.BulletBlocker)
			{
				SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
				speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(ReflectBulletPreCollision));
				break;
			}
		}
		if ((PathableTiles & CellTypes.PIT) == CellTypes.PIT)
		{
			SetIsFlying(true, "innate flight");
		}
		InitializePalette();
		CheckForBlackPhantomness();
		if (procedurallyOutlined)
		{
			bool? forcedOutlines = m_forcedOutlines;
			if (!forcedOutlines.HasValue || m_forcedOutlines.Value)
			{
				SetOutlines(true);
			}
		}
		if (invisibleUntilAwaken)
		{
			if (State == ActorState.Inactive)
			{
				ToggleRenderers(false);
			}
			if (!HasBeenAwoken)
			{
				base.specRigidbody.CollideWithOthers = false;
				base.IsGone = true;
				if ((bool)base.knockbackDoer)
				{
					base.knockbackDoer.SetImmobile(true, "awaken");
				}
			}
		}
		if (GameManager.Instance.InTutorial && base.name.StartsWith("BulletManTutorial"))
		{
			WanderHack();
		}
		if (OnPostStartInitialization != null)
		{
			OnPostStartInitialization();
		}
	}

	public void SetOverrideOutlineColor(Color c)
	{
		OverrideOutlineColor = c;
		if (SpriteOutlineManager.HasOutline(base.sprite))
		{
			Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(base.sprite);
			if (outlineMaterial != null)
			{
				outlineMaterial.SetColor("_OverrideColor", c);
			}
			HealthHaver healthHaver = base.healthHaver;
			if ((bool)healthHaver)
			{
				healthHaver.UpdateCachedOutlineColor(outlineMaterial, c);
			}
		}
	}

	public void ClearOverrideOutlineColor()
	{
		OverrideOutlineColor = null;
		if (SpriteOutlineManager.HasOutline(base.sprite))
		{
			Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(base.sprite);
			if (outlineMaterial != null)
			{
				outlineMaterial.SetColor("_OverrideColor", OutlineColor);
			}
		}
	}

	public void SetOutlines(bool value)
	{
		if (!procedurallyOutlined)
		{
			return;
		}
		if (value)
		{
			if (!SpriteOutlineManager.HasOutline(base.sprite))
			{
				SpriteOutlineManager.AddOutlineToSprite(base.sprite, OutlineColor, 0.1f);
			}
			m_forcedOutlines = true;
		}
		else if (!value)
		{
			if (SpriteOutlineManager.HasOutline(base.sprite))
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			}
			m_forcedOutlines = false;
		}
	}

	private void NoPitsMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if ((bool)specRigidbody && specRigidbody.GroundPixelCollider != null)
		{
			Vector2 vector = specRigidbody.GroundPixelCollider.UnitCenter + PhysicsEngine.PixelToUnit(pixelOffset);
			if (GameManager.Instance.Dungeon.CellSupportsFalling(vector))
			{
				validLocation = false;
			}
		}
	}

	public string GetActorName()
	{
		if (!string.IsNullOrEmpty(OverrideDisplayName))
		{
			return StringTableManager.GetEnemiesString(OverrideDisplayName);
		}
		if ((bool)base.encounterTrackable)
		{
			return base.encounterTrackable.journalData.GetPrimaryDisplayName();
		}
		return StringTableManager.GetEnemiesString("#KILLEDBYDEFAULT");
	}

	private void UpdateTurboMode()
	{
		if ((bool)CompanionOwner)
		{
			return;
		}
		if (m_cachedTurboness && !GameManager.IsTurboMode)
		{
			m_cachedTurboness = false;
			MovementSpeed /= TurboModeController.sEnemyMovementSpeedMultiplier;
			if ((bool)base.behaviorSpeculator)
			{
				base.behaviorSpeculator.CooldownScale /= TurboModeController.sEnemyCooldownMultiplier;
			}
		}
		else if (!m_cachedTurboness && GameManager.IsTurboMode)
		{
			m_cachedTurboness = true;
			MovementSpeed *= TurboModeController.sEnemyMovementSpeedMultiplier;
			if ((bool)base.behaviorSpeculator)
			{
				base.behaviorSpeculator.CooldownScale *= TurboModeController.sEnemyCooldownMultiplier;
			}
		}
		if (m_cachedTurboness && !m_turboWake && State == ActorState.Awakening)
		{
			m_turboWake = true;
			if ((bool)base.aiAnimator)
			{
				base.aiAnimator.FpsScale *= TurboModeController.sEnemyWakeTimeMultiplier;
			}
		}
		else if ((!m_cachedTurboness || State != ActorState.Awakening) && m_turboWake)
		{
			m_turboWake = false;
			if ((bool)base.aiAnimator)
			{
				base.aiAnimator.FpsScale /= TurboModeController.sEnemyWakeTimeMultiplier;
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (IsMimicEnemy)
		{
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
			if (absoluteRoomFromPosition != null && absoluteRoomFromPosition != parentRoom)
			{
				if (parentRoom != null)
				{
					parentRoom.DeregisterEnemy(this);
				}
				parentRoom = absoluteRoomFromPosition;
				parentRoom.RegisterEnemy(this);
			}
		}
		if (ReflectsProjectilesWhileInvulnerable && (bool)base.specRigidbody && (bool)base.spriteAnimator)
		{
			base.specRigidbody.ReflectProjectiles = base.spriteAnimator.QueryInvulnerabilityFrame();
			base.specRigidbody.ReflectBeams = base.spriteAnimator.QueryInvulnerabilityFrame();
		}
		if (State == ActorState.Awakening && (string.IsNullOrEmpty(m_awakenAnimation) || ((bool)base.aiAnimator && !base.aiAnimator.IsPlaying(m_awakenAnimation))))
		{
			if ((bool)base.aiShooter)
			{
				base.aiShooter.ToggleGunAndHandRenderers(true, "Reinforce");
				base.aiShooter.ToggleGunAndHandRenderers(true, "Awaken");
			}
			State = ActorState.Normal;
		}
		if (invisibleUntilAwaken)
		{
			if (State == ActorState.Inactive && base.renderer.enabled)
			{
				ToggleRenderers(false);
			}
			if (State == ActorState.Normal)
			{
				base.specRigidbody.CollideWithOthers = true;
				base.IsGone = false;
				if ((bool)base.knockbackDoer)
				{
					base.knockbackDoer.SetImmobile(false, "awaken");
				}
				invisibleUntilAwaken = false;
			}
		}
		if ((PathableTiles & CellTypes.PIT) != CellTypes.PIT)
		{
			HandlePitChecks();
		}
		if (base.healthHaver.IsDead)
		{
			base.specRigidbody.Velocity = ((!PreventDeathKnockback) ? m_knockbackVelocity : Vector2.zero);
			return;
		}
		CellVisualData.CellFloorType floorTypeFromPosition = GameManager.Instance.Dungeon.GetFloorTypeFromPosition(base.specRigidbody.UnitBottomCenter);
		if (!m_prevFloorType.HasValue || m_prevFloorType.Value != floorTypeFromPosition)
		{
			m_prevFloorType = floorTypeFromPosition;
			AkSoundEngine.SetSwitch("FS_Surfaces", s_floorTypeNames[(int)floorTypeFromPosition], base.gameObject);
		}
		if (base.aiShooter != null)
		{
			base.aiShooter.AimAtTarget();
		}
		if (base.isActiveAndEnabled)
		{
		}
		Vector2 voluntaryMovementVelocity = VoluntaryMovementVelocity;
		if (UseMovementAudio)
		{
			bool flag = voluntaryMovementVelocity != Vector2.zero;
			if (flag && !m_audioMovedLastFrame)
			{
				AkSoundEngine.PostEvent(StartMovingEvent, base.gameObject);
			}
			else if (!flag && m_audioMovedLastFrame)
			{
				AkSoundEngine.PostEvent(StopMovingEvent, base.gameObject);
			}
			m_audioMovedLastFrame = flag;
		}
		base.specRigidbody.Velocity = ApplyMovementModifiers(voluntaryMovementVelocity, m_knockbackVelocity) * LocalTimeScale;
		base.specRigidbody.Velocity += ImpartedVelocity;
		ImpartedVelocity = Vector2.zero;
		if (m_isSafeMoving)
		{
			m_safeMoveTimer += BraveTime.DeltaTime;
			base.transform.position = Vector2.Lerp(m_safeMoveStartPos.Value, m_safeMoveEndPos.Value, Mathf.Clamp01(m_safeMoveTimer / m_safeMoveTime));
			base.specRigidbody.Reinitialize();
			if (m_safeMoveTimer >= m_safeMoveTime)
			{
				m_isSafeMoving = false;
			}
		}
		m_lastPosition = base.specRigidbody.UnitCenter;
		if (IsBlackPhantom)
		{
			UpdateBlackPhantomShaders();
		}
		if (IsBlackPhantom || ForceBlackPhantomParticles)
		{
			UpdateBlackPhantomParticles();
		}
		ProcessHealthOverrides();
		if ((bool)base.healthHaver && base.healthHaver.IsBoss)
		{
			if (base.FreezeAmount > 0f)
			{
				float resistanceForEffectType = GetResistanceForEffectType(EffectResistanceType.Freeze);
				resistanceForEffectType = Mathf.Clamp(resistanceForEffectType + 0.01f * BraveTime.DeltaTime, 0.6f, 1f);
				SetResistance(EffectResistanceType.Freeze, resistanceForEffectType);
			}
			if (GetEffect(EffectResistanceType.Fire) != null)
			{
				float resistanceForEffectType2 = GetResistanceForEffectType(EffectResistanceType.Fire);
				resistanceForEffectType2 = Mathf.Clamp(resistanceForEffectType2 + 0.025f * BraveTime.DeltaTime, 0.25f, 0.75f);
				SetResistance(EffectResistanceType.Fire, resistanceForEffectType2);
			}
		}
	}

	private void UpdateBlackPhantomShaders()
	{
		if (base.healthHaver.bodySprites.Count == m_cachedBodySpriteCount)
		{
			return;
		}
		m_cachedBodySpriteCount = base.healthHaver.bodySprites.Count;
		for (int i = 0; i < base.healthHaver.bodySprites.Count; i++)
		{
			tk2dBaseSprite tk2dBaseSprite2 = base.healthHaver.bodySprites[i];
			tk2dBaseSprite2.usesOverrideMaterial = true;
			Material material = tk2dBaseSprite2.renderer.material;
			if (m_cachedBodySpriteShader == null)
			{
				m_cachedBodySpriteShader = material.shader;
			}
			if (IsBlackPhantom)
			{
				if (OverrideBlackPhantomShader != null)
				{
					material.shader = OverrideBlackPhantomShader;
				}
				else
				{
					material.shader = ShaderCache.Acquire("Brave/LitCutoutUberPhantom");
					material.SetFloat("_PhantomGradientScale", BlackPhantomProperties.GradientScale);
					material.SetFloat("_PhantomContrastPower", BlackPhantomProperties.ContrastPower);
					if (tk2dBaseSprite2 != base.sprite)
					{
						material.SetFloat("_ApplyFade", 0f);
					}
				}
			}
			else
			{
				material.shader = m_cachedBodySpriteShader;
			}
			tk2dBaseSprite2.renderer.material = material;
		}
		if (!base.aiShooter || !base.aiShooter.CurrentGun)
		{
			return;
		}
		tk2dBaseSprite tk2dBaseSprite3 = base.aiShooter.CurrentGun.GetSprite();
		tk2dBaseSprite3.usesOverrideMaterial = true;
		Material material2 = tk2dBaseSprite3.renderer.material;
		if (m_cachedGunSpriteShader == null)
		{
			m_cachedGunSpriteShader = material2.shader;
		}
		if (IsBlackPhantom)
		{
			if (OverrideBlackPhantomShader != null)
			{
				material2.shader = OverrideBlackPhantomShader;
			}
			else
			{
				material2.shader = ShaderCache.Acquire("Brave/LitCutoutUberPhantom");
				material2.SetFloat("_PhantomGradientScale", BlackPhantomProperties.GradientScale);
				material2.SetFloat("_PhantomContrastPower", BlackPhantomProperties.ContrastPower);
				material2.SetFloat("_ApplyFade", 0.3f);
			}
		}
		else
		{
			material2.shader = m_cachedBodySpriteShader;
		}
		tk2dBaseSprite3.renderer.material = material2;
	}

	private void UpdateBlackPhantomParticles()
	{
		if (!ShouldDoBlackPhantomParticles.HasValue)
		{
			if ((bool)GetComponent<DraGunDeathController>())
			{
				ShouldDoBlackPhantomParticles = false;
			}
			else
			{
				ShouldDoBlackPhantomParticles = true;
			}
		}
		if ((!ShouldDoBlackPhantomParticles.HasValue || ShouldDoBlackPhantomParticles.Value) && HasBeenEngaged && (!base.sprite || base.sprite.renderer.enabled) && GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			PixelCollider pixelCollider = ((!OverrideBlackPhantomParticlesCollider) ? base.specRigidbody.HitboxPixelCollider : base.specRigidbody.PixelColliders[BlackPhantomParticlesCollider]);
			Vector3 vector = pixelCollider.UnitBottomLeft.ToVector3ZisY();
			Vector3 vector2 = pixelCollider.UnitTopRight.ToVector3ZisY();
			float num = (vector2.y - vector.y) * (vector2.x - vector.x);
			float num2 = 40f * num;
			int num3 = Mathf.CeilToInt(Mathf.Max(1f, num2 * BraveTime.DeltaTime));
			GlobalSparksDoer.SparksType systemType = ((!ForceBlackPhantomParticles) ? GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE : GlobalSparksDoer.SparksType.DARK_MAGICKS);
			int num4 = num3;
			Vector3 minPosition = vector;
			Vector3 maxPosition = vector2;
			Vector3 direction = Vector3.up / 2f;
			float angleVariance = 120f;
			float magnitudeVariance = 0.2f;
			float? startLifetime = UnityEngine.Random.Range(1f, 1.65f);
			GlobalSparksDoer.DoRandomParticleBurst(num4, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, null, startLifetime, null, systemType);
			if (UnityEngine.Random.value < 0.5f)
			{
				num4 = 1;
				direction = vector;
				maxPosition = vector2.WithY(vector.y + 0.1f);
				minPosition = Vector3.right / 2f;
				magnitudeVariance = 25f;
				angleVariance = 0.2f;
				startLifetime = UnityEngine.Random.Range(1f, 1.65f);
				GlobalSparksDoer.DoRandomParticleBurst(num4, direction, maxPosition, minPosition, magnitudeVariance, angleVariance, null, startLifetime, null, systemType);
			}
			else
			{
				num4 = 1;
				minPosition = vector;
				maxPosition = vector2.WithY(vector.y + 0.1f);
				direction = Vector3.left / 2f;
				angleVariance = 25f;
				magnitudeVariance = 0.2f;
				startLifetime = UnityEngine.Random.Range(1f, 1.65f);
				GlobalSparksDoer.DoRandomParticleBurst(num4, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, null, startLifetime, null, systemType);
			}
		}
	}

	public void LateUpdate()
	{
		base.sprite.UpdateZDepth();
		UpdateTurboMode();
		if ((bool)base.renderer && (bool)base.renderer.material && HasOverrideColor() && !OverrideColorOverridden && (m_colorOverridenMaterial != base.renderer.material || m_colorOverridenShader != base.renderer.material.shader))
		{
			OnOverrideColorsChanged();
		}
	}

	protected virtual void OnWillRenderObject()
	{
		if (Pixelator.IsRenderingReflectionMap)
		{
			base.sprite.renderer.sharedMaterial.SetFloat("_ReflectionYOffset", base.sprite.GetBounds().min.y * 2f + actorReflectionAdditionalOffset);
		}
	}

	protected override void OnDestroy()
	{
		if (CurrentGun != null)
		{
			CurrentGun.DespawnVFX();
		}
		StaticReferenceManager.AllEnemies.Remove(this);
		bool flag = !base.healthHaver || !base.healthHaver.IsBoss;
		if (GameManager.IsShuttingDown || GameManager.IsReturningToBreach || !GameManager.HasInstance || GameManager.Instance.IsLoadingLevel)
		{
			flag = false;
		}
		if (ParentRoom != null && flag)
		{
			ParentRoom.DeregisterEnemy(this);
		}
		if (parentRoom != null)
		{
			parentRoom.Entered -= OnPlayerEntered;
		}
		DeregisterCallbacks();
	}

	public void CompanionWarp(Vector3 targetPosition)
	{
		GameObject prefab = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Breakable_Column_Puff");
		GameObject gameObject = SpawnManager.SpawnVFX(prefab);
		gameObject.GetComponent<tk2dSprite>().PlaceAtPositionByAnchor(base.specRigidbody.UnitBottomCenter, tk2dBaseSprite.Anchor.LowerCenter);
		Vector2 vector = base.specRigidbody.UnitBottomCenter - base.transform.position.XY();
		base.transform.position = targetPosition - vector.ToVector3ZUp();
		base.specRigidbody.Reinitialize();
		CorrectForWalls();
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		gameObject = SpawnManager.SpawnVFX(prefab);
		gameObject.GetComponent<tk2dSprite>().PlaceAtPositionByAnchor(base.specRigidbody.UnitBottomCenter, tk2dBaseSprite.Anchor.LowerCenter);
	}

	public void WanderHack()
	{
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.enabled = false;
		}
		StartCoroutine(WanderHackCR());
	}

	private IEnumerator WanderHackCR()
	{
		ClearPath();
		PlayerTarget = null;
		OverrideTarget = null;
		yield return new WaitForSeconds(1f);
		while (true)
		{
			IntVector2 targetPos = PathTile + new IntVector2(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2));
			if (GameManager.Instance.Dungeon.data.cellData[targetPos.x][targetPos.y].type == CellType.FLOOR && !GameManager.Instance.Dungeon.data.isTopWall(targetPos.x, targetPos.y))
			{
				PathfindToPosition(targetPos.ToCenterVector2());
				while (!PathComplete)
				{
					yield return null;
				}
				yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
			}
		}
	}

	public bool PathfindToPosition(Vector2 targetPosition, Vector2? overridePathEnd = null, bool smooth = true, CellValidator cellValidator = null, ExtraWeightingFunction extraWeightingFunction = null, CellTypes? overridePathableTiles = null, bool canPassOccupied = false)
	{
		bool result = false;
		Pathfinder.Instance.RemoveActorPath(m_upcomingPathTiles);
		CellTypes passableCellTypes = ((!overridePathableTiles.HasValue) ? PathableTiles : overridePathableTiles.Value);
		Path path = null;
		if (Pathfinder.Instance.GetPath(PathTile, targetPosition.ToIntVector2(VectorConversions.Floor), out path, Clearance, passableCellTypes, cellValidator, extraWeightingFunction, canPassOccupied))
		{
			m_currentPath = path;
			m_overridePathEnd = overridePathEnd;
			if (m_currentPath != null && m_currentPath.WillReachFinalGoal)
			{
				result = true;
			}
			if (m_currentPath.Count == 0)
			{
				m_currentPath = null;
			}
			else if (smooth)
			{
				path.Smooth(base.specRigidbody.UnitCenter, base.specRigidbody.UnitDimensions / 2f, passableCellTypes, canPassOccupied, Clearance);
			}
		}
		UpdateUpcomingPathTiles(2f);
		Pathfinder.Instance.UpdateActorPath(m_upcomingPathTiles);
		return result;
	}

	public void FakePathToPosition(Vector2 targetPosition)
	{
		Pathfinder.Instance.RemoveActorPath(m_upcomingPathTiles);
		m_currentPath = null;
		m_overridePathEnd = targetPosition;
	}

	public void ClearPath()
	{
		Pathfinder.Instance.RemoveActorPath(m_upcomingPathTiles);
		m_upcomingPathTiles.Clear();
		m_upcomingPathTiles.Add(PathTile);
		Pathfinder.Instance.UpdateActorPath(m_upcomingPathTiles);
		m_currentPath = null;
		m_overridePathEnd = null;
	}

	private bool GetNextTargetPosition(out Vector2 targetPos)
	{
		if (m_currentPath != null && m_currentPath.Count > 0)
		{
			targetPos = m_currentPath.GetFirstCenterVector2();
			return true;
		}
		Vector2? overridePathEnd = m_overridePathEnd;
		if (overridePathEnd.HasValue)
		{
			targetPos = m_overridePathEnd.Value;
			return true;
		}
		targetPos = Vector2.zero;
		return false;
	}

	private Vector2 GetPathTarget()
	{
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 result = unitCenter;
		float num = MovementSpeed * LocalDeltaTime;
		Vector2 vector = unitCenter;
		Vector2 targetPos = unitCenter;
		while (!(num <= 0f) && GetNextTargetPosition(out targetPos))
		{
			float num2 = Vector2.Distance(targetPos, unitCenter);
			if (num2 < num)
			{
				num -= num2;
				vector = targetPos;
				result = vector;
				if (m_currentPath != null && m_currentPath.Count > 0)
				{
					m_currentPath.RemoveFirst();
				}
				else
				{
					m_overridePathEnd = null;
				}
				continue;
			}
			result = (targetPos - vector).normalized * num + vector;
			break;
		}
		return result;
	}

	private Vector2 GetPathVelocityContribution()
	{
		Vector2? overridePathVelocity = OverridePathVelocity;
		if (overridePathVelocity.HasValue)
		{
			return OverridePathVelocity.Value;
		}
		if (m_currentPath == null || m_currentPath.Count == 0)
		{
			Vector2? overridePathEnd = m_overridePathEnd;
			if (!overridePathEnd.HasValue)
			{
				return Vector2.zero;
			}
		}
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 pathTarget = GetPathTarget();
		Vector2 vector = pathTarget - unitCenter;
		if (MovementSpeed * LocalDeltaTime > vector.magnitude)
		{
			return vector / LocalDeltaTime;
		}
		return MovementSpeed * vector.normalized;
	}

	private Vector2 GetPathVelocityContribution_Old()
	{
		if (m_currentPath == null || m_currentPath.Count == 0)
		{
			Vector2? overridePathEnd = m_overridePathEnd;
			if (!overridePathEnd.HasValue)
			{
				return Vector2.zero;
			}
		}
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		Vector2 vector = ((m_currentPath == null) ? m_overridePathEnd.Value : m_currentPath.GetFirstCenterVector2());
		int num = ((m_currentPath != null) ? m_currentPath.Count : 0);
		Vector2? overridePathEnd2 = m_overridePathEnd;
		bool flag = num + (overridePathEnd2.HasValue ? 1 : 0) == 1;
		bool flag2 = false;
		if (Vector2.Distance(unitCenter, vector) < PhysicsEngine.PixelToUnit(1))
		{
			flag2 = true;
		}
		else if (!flag)
		{
			Vector2 b = BraveMathCollege.ClosestPointOnLineSegment(vector, m_lastPosition, unitCenter);
			if (Vector2.Distance(vector, b) < PhysicsEngine.PixelToUnit(1))
			{
				flag2 = true;
			}
		}
		if (flag2)
		{
			if (m_currentPath != null && m_currentPath.Count > 0)
			{
				m_currentPath.RemoveFirst();
				if (m_currentPath.Count == 0)
				{
					m_currentPath = null;
					return Vector2.zero;
				}
			}
			else
			{
				Vector2? overridePathEnd3 = m_overridePathEnd;
				if (overridePathEnd3.HasValue)
				{
					m_overridePathEnd = null;
				}
			}
		}
		Vector2 vector2 = vector - unitCenter;
		if (flag && MovementSpeed * LocalDeltaTime > vector2.magnitude)
		{
			return vector2 / LocalDeltaTime;
		}
		return MovementSpeed * vector2.normalized;
	}

	public void ReflectBulletPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
	}

	protected bool CheckTableRaycast(SpeculativeRigidbody source, SpeculativeRigidbody target)
	{
		if (target == null || source == null)
		{
			return true;
		}
		Vector2 unitCenter = source.GetUnitCenter(ColliderType.Ground);
		Vector2 unitCenter2 = target.GetUnitCenter(ColliderType.Ground);
		Vector2 direction = unitCenter2 - unitCenter;
		RaycastResult result;
		if (PhysicsEngine.Instance.RaycastWithIgnores(unitCenter, direction, direction.magnitude, out result, false, true, CollisionMask.LayerToMask(CollisionLayer.LowObstacle, CollisionLayer.HighObstacle), null, false, null, new SpeculativeRigidbody[2] { source, target }))
		{
			RaycastResult.Pool.Free(ref result);
			return false;
		}
		return true;
	}

	protected virtual void OnCollision(CollisionData collision)
	{
		if (ManualKnockbackHandling)
		{
			return;
		}
		if (collision.collisionType == CollisionData.CollisionType.Rigidbody)
		{
			if (base.IsFrozen)
			{
				PlayerController component = collision.OtherRigidbody.GetComponent<PlayerController>();
				if ((bool)component && collision.Overlap)
				{
					component.specRigidbody.RegisterGhostCollisionException(base.specRigidbody);
				}
			}
			else
			{
				if (CanTargetPlayers)
				{
					PlayerController component2 = collision.OtherRigidbody.GetComponent<PlayerController>();
					if (!base.healthHaver.IsDead && component2 != null && CheckTableRaycast(collision.MyRigidbody, collision.OtherRigidbody))
					{
						Vector2 normalized = (component2.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter).normalized;
						if (component2.IsDodgeRolling)
						{
							component2.ApplyRollDamage(this);
						}
						if (component2.ReceivesTouchDamage)
						{
							float damage = CollisionDamage;
							if (IsBlackPhantom)
							{
								damage = 1f;
							}
							if (base.IsCheezen)
							{
								damage = 0f;
							}
							component2.healthHaver.ApplyDamage(damage, normalized, GetActorName(), CoreDamageTypes.None, (!IsBlackPhantom) ? DamageCategory.Collision : DamageCategory.BlackBullet);
							if (Mathf.Approximately(normalized.magnitude, 0f))
							{
								normalized = UnityEngine.Random.insideUnitCircle.normalized;
							}
							component2.knockbackDoer.ApplySourcedKnockback(normalized, CollisionKnockbackStrength, base.gameObject);
							if ((bool)base.knockbackDoer)
							{
								base.knockbackDoer.ApplySourcedKnockback(-normalized, component2.collisionKnockbackStrength, base.gameObject);
							}
						}
						else
						{
							if (Mathf.Approximately(normalized.magnitude, 0f))
							{
								normalized = UnityEngine.Random.insideUnitCircle.normalized;
							}
							component2.knockbackDoer.ApplySourcedKnockback(normalized, Mathf.Max(50f, CollisionKnockbackStrength), base.gameObject);
							if ((bool)base.knockbackDoer)
							{
								base.knockbackDoer.ApplySourcedKnockback(-normalized, Mathf.Max(50f, component2.collisionKnockbackStrength), base.gameObject);
							}
						}
						if (CollisionSetsPlayerOnFire)
						{
							component2.IsOnFire = true;
						}
						CollisionVFX.SpawnAtPosition(collision.Contact, 0f, null, Vector2.zero, Vector2.zero, 2f);
						if (collision.Overlap)
						{
							component2.specRigidbody.RegisterGhostCollisionException(base.specRigidbody);
						}
						if (DiesOnCollison)
						{
							base.healthHaver.ApplyDamage(1000f, -normalized, "Contact", CoreDamageTypes.None, DamageCategory.Unstoppable);
						}
					}
				}
				if (CanTargetEnemies || OverrideHitEnemies)
				{
					AIActor component3 = collision.OtherRigidbody.GetComponent<AIActor>();
					if (component3 != null && !base.healthHaver.IsDead && (IsNormalEnemy || component3.IsNormalEnemy))
					{
						Vector2 normalized2 = (component3.specRigidbody.UnitCenter - base.specRigidbody.UnitCenter).normalized;
						if (CanTargetEnemies)
						{
							component3.healthHaver.ApplyDamage(CollisionDamage * 5f, normalized2, GetActorName(), CollisionDamageTypes, DamageCategory.Collision);
						}
						if (Mathf.Approximately(normalized2.magnitude, 0f))
						{
							normalized2 = UnityEngine.Random.insideUnitCircle.normalized;
						}
						if ((bool)component3.knockbackDoer)
						{
							component3.knockbackDoer.ApplySourcedKnockback(normalized2, EnemyCollisionKnockbackStrength, base.gameObject);
						}
						if ((bool)base.knockbackDoer)
						{
							base.knockbackDoer.ApplySourcedKnockback(-normalized2, component3.EnemyCollisionKnockbackStrength, base.gameObject);
						}
						CollisionVFX.SpawnAtPosition(collision.Contact, 0f, null, Vector2.zero, Vector2.zero, 2f);
						if (collision.Overlap)
						{
							component3.specRigidbody.RegisterGhostCollisionException(base.specRigidbody);
						}
						if (DiesOnCollison)
						{
							base.healthHaver.ApplyDamage(1000f, -normalized2, "Contact", CoreDamageTypes.None, DamageCategory.Unstoppable);
						}
					}
				}
			}
		}
		if (!collision.OtherRigidbody || !collision.OtherRigidbody.gameActor)
		{
			NonActorCollisionVFX.SpawnAtPosition(collision.Contact, 0f, null, Vector2.zero, Vector2.zero, 2f);
		}
		m_strafeDirection *= -1;
	}

	public bool RigidbodyBlocksLineOfSight(SpeculativeRigidbody testRigidbody)
	{
		if (testRigidbody.gameObject.CompareTag("Intangible"))
		{
			return true;
		}
		return false;
	}

	public bool HasLineOfSightToRigidbody(SpeculativeRigidbody targetRigidbody)
	{
		if (targetRigidbody == null)
		{
			return false;
		}
		Vector2 unitCenter = targetRigidbody.GetUnitCenter(ColliderType.HitBox);
		Vector2 vector = ((!LosPoint) ? base.specRigidbody.UnitCenter : LosPoint.transform.position.XY());
		float dist = Vector2.Distance(vector, unitCenter);
		int complexEnemyVisibilityMask = CollisionMask.GetComplexEnemyVisibilityMask(CanTargetPlayers, CanTargetEnemies);
		RaycastResult result;
		if (!PhysicsEngine.Instance.Raycast(vector, unitCenter - vector, dist, out result, true, true, complexEnemyVisibilityMask, null, false, m_rigidbodyExcluder, base.specRigidbody))
		{
			RaycastResult.Pool.Free(ref result);
			return false;
		}
		if (result.SpeculativeRigidbody == null || result.SpeculativeRigidbody != targetRigidbody)
		{
			RaycastResult.Pool.Free(ref result);
			return false;
		}
		RaycastResult.Pool.Free(ref result);
		return true;
	}

	public bool HasLineOfSightToTargetFromPosition(Vector2 hypotheticalPosition)
	{
		if (TargetRigidbody == null)
		{
			return false;
		}
		Vector2 unitCenter = TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		float distanceToTarget = DistanceToTarget;
		int complexEnemyVisibilityMask = CollisionMask.GetComplexEnemyVisibilityMask(CanTargetPlayers, CanTargetEnemies);
		RaycastResult result;
		if (!PhysicsEngine.Instance.Raycast(hypotheticalPosition, unitCenter - hypotheticalPosition, distanceToTarget, out result, true, true, complexEnemyVisibilityMask, null, false, m_rigidbodyExcluder, base.specRigidbody))
		{
			RaycastResult.Pool.Free(ref result);
			return false;
		}
		if (result.SpeculativeRigidbody == null || result.SpeculativeRigidbody != TargetRigidbody)
		{
			RaycastResult.Pool.Free(ref result);
			return false;
		}
		RaycastResult.Pool.Free(ref result);
		return true;
	}

	private void CheckForBlackPhantomness()
	{
		if (!(CompanionOwner != null) && IsNormalEnemy && !PreventBlackPhantom)
		{
			int totalCurse = PlayerStats.GetTotalCurse();
			float num = 0f;
			num = ((totalCurse <= 0) ? 0f : ((totalCurse <= 2) ? 0.01f : ((totalCurse <= 4) ? 0.02f : ((totalCurse <= 6) ? 0.05f : ((totalCurse <= 8) ? 0.1f : ((totalCurse != 9) ? 0.5f : 0.25f))))));
			if (base.healthHaver.IsBoss)
			{
				num = ((totalCurse < 7) ? 0f : ((totalCurse < 9) ? 0.2f : ((totalCurse >= 10) ? 0.5f : 0.3f)));
			}
			if (ForceBlackPhantom || UnityEngine.Random.value < num)
			{
				BecomeBlackPhantom();
			}
		}
	}

	public void BecomeBlackPhantom()
	{
		if (IsBlackPhantom)
		{
			return;
		}
		m_championType = EnemyChampionType.JAMMED;
		m_cachedBodySpriteCount = -1;
		UpdateBlackPhantomShaders();
		if ((bool)base.healthHaver && !base.healthHaver.healthIsNumberOfHits)
		{
			float bonusHealthPercentIncrease = BlackPhantomProperties.BonusHealthPercentIncrease;
			float bonusHealthFlatIncrease = BlackPhantomProperties.BonusHealthFlatIncrease;
			if (base.healthHaver.IsBoss)
			{
				bonusHealthPercentIncrease += BlackPhantomProperties.GlobalBossPercentIncrease;
				bonusHealthFlatIncrease += BlackPhantomProperties.GlobalBossFlatIncrease;
			}
			else
			{
				bonusHealthPercentIncrease += BlackPhantomProperties.GlobalPercentIncrease;
				bonusHealthFlatIncrease += BlackPhantomProperties.GlobalFlatIncrease;
			}
			float num = base.healthHaver.GetMaxHealth() * (1f + bonusHealthPercentIncrease) + bonusHealthFlatIncrease;
			if (BlackPhantomProperties.MaxTotalHealth > 0f && !base.healthHaver.IsBoss)
			{
				num = Mathf.Min(num, BlackPhantomProperties.MaxTotalHealth * BaseLevelHealthModifier);
			}
			base.healthHaver.SetHealthMaximum(num, null, true);
		}
		MovementSpeed *= BlackPhantomProperties.MovementSpeedMultiplier;
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.CooldownScale /= BlackPhantomProperties.CooldownMultiplier;
		}
	}

	public void UnbecomeBlackPhantom()
	{
		if (!IsBlackPhantom)
		{
			return;
		}
		m_championType = EnemyChampionType.NORMAL;
		m_cachedBodySpriteCount = -1;
		UpdateBlackPhantomShaders();
		if ((bool)base.healthHaver)
		{
			float bonusHealthPercentIncrease = BlackPhantomProperties.BonusHealthPercentIncrease;
			float bonusHealthFlatIncrease = BlackPhantomProperties.BonusHealthFlatIncrease;
			if (base.healthHaver.IsBoss)
			{
				bonusHealthPercentIncrease += BlackPhantomProperties.GlobalBossPercentIncrease;
				bonusHealthFlatIncrease += BlackPhantomProperties.GlobalBossFlatIncrease;
			}
			else
			{
				bonusHealthPercentIncrease += BlackPhantomProperties.GlobalPercentIncrease;
				bonusHealthFlatIncrease += BlackPhantomProperties.GlobalFlatIncrease;
			}
			float num = (base.healthHaver.GetMaxHealth() - bonusHealthFlatIncrease) / (1f + bonusHealthPercentIncrease);
			if (BlackPhantomProperties.MaxTotalHealth > 0f && !base.healthHaver.IsBoss)
			{
				num = Mathf.Max(num, 10f);
			}
			base.healthHaver.SetHealthMaximum(num, null, true);
		}
		MovementSpeed /= BlackPhantomProperties.MovementSpeedMultiplier;
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.CooldownScale *= BlackPhantomProperties.CooldownMultiplier;
		}
	}

	private void InitializePalette()
	{
		if (optionalPalette != null)
		{
			m_isPaletteSwapped = true;
			base.sprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_COMPLEX;
			base.sprite.renderer.material.SetTexture("_PaletteTex", optionalPalette);
		}
	}

	private void ProcessHealthOverrides()
	{
		for (int i = 0; i < HealthOverrides.Count; i++)
		{
			HealthOverride healthOverride = HealthOverrides[i];
			if (healthOverride.HasBeenUsed || !(base.healthHaver.GetCurrentHealthPercentage() <= healthOverride.HealthPercentage))
			{
				continue;
			}
			FieldInfo[] fields = GetType().GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name == healthOverride.Stat)
				{
					fieldInfo.SetValue(this, healthOverride.Value);
					healthOverride.HasBeenUsed = true;
					break;
				}
			}
			if (!healthOverride.HasBeenUsed)
			{
				Debug.LogError("Failed to find the field " + healthOverride.Stat + " on AIActor.");
				healthOverride.HasBeenUsed = true;
			}
		}
	}

	private void UpdateUpcomingPathTiles(float time)
	{
		m_upcomingPathTiles.Clear();
		m_upcomingPathTiles.Add(PathTile);
		if (m_currentPath == null || m_currentPath.Count <= 0)
		{
			return;
		}
		float num = 0f;
		Vector2 vector = Position;
		LinkedListNode<IntVector2> linkedListNode = m_currentPath.Positions.First;
		Vector2 vector2 = linkedListNode.Value.ToCenterVector2();
		Vector2 vector3;
		for (; num < time; num += vector3.magnitude / MovementSpeed)
		{
			vector3 = vector2 - vector;
			if (vector3.sqrMagnitude > 0.04f)
			{
				vector3 = vector3.normalized * 0.2f;
			}
			vector += vector3;
			IntVector2 intVector = vector.ToIntVector2(VectorConversions.Floor);
			if (m_upcomingPathTiles[m_upcomingPathTiles.Count - 1] != intVector)
			{
				m_upcomingPathTiles.Add(intVector);
			}
			if (vector3.magnitude < 0.2f)
			{
				linkedListNode = linkedListNode.Next;
				if (linkedListNode == null)
				{
					break;
				}
				vector2 = linkedListNode.Value.ToCenterVector2();
			}
		}
	}

	private void OnEnable()
	{
		if (!invisibleUntilAwaken)
		{
			return;
		}
		if (State == ActorState.Inactive)
		{
			ToggleRenderers(false);
		}
		if (!HasBeenAwoken)
		{
			base.specRigidbody.CollideWithOthers = false;
			base.IsGone = true;
			if ((bool)base.knockbackDoer)
			{
				base.knockbackDoer.SetImmobile(true, "awaken");
			}
		}
	}

	private void OnDisable()
	{
	}

	public void ToggleRenderers(bool e)
	{
		tk2dSprite[] componentsInChildren = GetComponentsInChildren<tk2dSprite>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = e;
		}
		tk2dSpriteAnimator[] componentsInChildren2 = GetComponentsInChildren<tk2dSpriteAnimator>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].enabled = e;
		}
		Renderer[] componentsInChildren3 = GetComponentsInChildren<Renderer>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			componentsInChildren3[k].enabled = e;
		}
		if (e)
		{
			bool? forcedOutlines = m_forcedOutlines;
			if (forcedOutlines.HasValue && !m_forcedOutlines.Value)
			{
				for (int l = 0; l < componentsInChildren.Length; l++)
				{
					if (componentsInChildren[l].IsOutlineSprite)
					{
						componentsInChildren[l].renderer.enabled = false;
					}
				}
			}
		}
		if ((bool)base.aiShooter && e)
		{
			base.aiShooter.UpdateGunRenderers();
			base.aiShooter.UpdateHandRenderers();
		}
	}

	private void InitializeCallbacks()
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath += PreDeath;
			base.healthHaver.OnDeath += Die;
			base.healthHaver.OnDamaged += Damaged;
		}
		if ((bool)base.spriteAnimator)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		}
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		}
	}

	private void DeregisterCallbacks()
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath -= PreDeath;
			base.healthHaver.OnDeath -= Die;
			base.healthHaver.OnDamaged -= Damaged;
		}
		if ((bool)base.spriteAnimator)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		}
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		}
	}

	protected void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (GameManager.AUDIO_ENABLED)
		{
			for (int i = 0; i < animationAudioEvents.Count; i++)
			{
				if (animationAudioEvents[i].eventTag == frame.eventInfo)
				{
					AkSoundEngine.PostEvent(animationAudioEvents[i].eventName, base.gameObject);
				}
			}
		}
		if (procedurallyOutlined && frame.eventOutline != 0)
		{
			if (frame.eventOutline == tk2dSpriteAnimationFrame.OutlineModifier.TurnOn)
			{
				SetOutlines(true);
			}
			else if (frame.eventOutline == tk2dSpriteAnimationFrame.OutlineModifier.TurnOff)
			{
				SetOutlines(false);
			}
		}
		if ((State == ActorState.Inactive || State == ActorState.Awakening) && frame.finishedSpawning)
		{
			base.specRigidbody.CollideWithOthers = true;
			base.IsGone = false;
			if ((bool)base.knockbackDoer)
			{
				base.knockbackDoer.SetImmobile(false, "awaken");
			}
			base.healthHaver.IsVulnerable = true;
		}
	}

	public void SkipOnEngaged()
	{
		m_hasBeenEngaged = true;
	}

	public void DelayActions(float delay)
	{
		SpeculatorDelayTime += delay;
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		parentRoom = room;
		parentRoom.RegisterEnemy(this);
		parentRoom.Entered += OnPlayerEntered;
		if (base.healthHaver.IsBoss && !GameManager.Instance.InTutorial && GameManager.Instance.BestActivePlayer.CurrentRoom != room)
		{
			if (!CanDropItems && CustomLootTable != null)
			{
				room.OverrideBossRewardTable = CustomLootTable;
			}
			SpeculativeRigidbody[] componentsInChildren = GetComponentsInChildren<SpeculativeRigidbody>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].CollideWithOthers = false;
			}
			base.IsGone = true;
		}
	}

	private void OnPlayerEntered(PlayerController enterer)
	{
		if (!HasDonePlayerEnterCheck && isPassable)
		{
			base.specRigidbody.Initialize();
			Vector2 unitCenter = GameManager.Instance.PrimaryPlayer.specRigidbody.UnitCenter;
			bool flag = !Pathfinder.Instance.IsPassable(PathTile, Clearance, PathableTiles);
			if (flag)
			{
				Debug.LogErrorFormat("Tried to spawn a {0} in an invalid location in room {1}.", base.name, ParentRoom.GetRoomName());
			}
			if ((bool)GetComponent<KeyBulletManController>())
			{
				TeleportSomewhere(null, true);
			}
			else if (flag || (!IsHarmlessEnemy && Vector2.Distance(unitCenter, base.specRigidbody.UnitCenter) < 8f))
			{
				TeleportSomewhere();
			}
			HasDonePlayerEnterCheck = true;
		}
	}

	public void TeleportSomewhere(IntVector2? overrideClearance = null, bool keepClose = false)
	{
		float sqrMinDist = 64f;
		float sqrMaxDist = 225f;
		PlayerController primaryPlayer = GameManager.Instance.PrimaryPlayer;
		Vector2 playerPosition = primaryPlayer.specRigidbody.UnitCenter;
		Vector2? otherPlayerPosition = null;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(primaryPlayer);
			if ((bool)otherPlayer && (bool)otherPlayer.healthHaver && otherPlayer.healthHaver.IsAlive)
			{
				otherPlayerPosition = otherPlayer.specRigidbody.UnitCenter;
			}
		}
		IntVector2 clearance = ((!overrideClearance.HasValue) ? Clearance : overrideClearance.Value);
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			if ((playerPosition - c.ToCenterVector2()).sqrMagnitude <= sqrMinDist)
			{
				return false;
			}
			if (otherPlayerPosition.HasValue && (otherPlayerPosition.Value - c.ToCenterVector2()).sqrMagnitude <= sqrMinDist)
			{
				return false;
			}
			if (keepClose)
			{
				bool flag = false;
				if ((playerPosition - c.ToCenterVector2()).sqrMagnitude <= sqrMaxDist)
				{
					flag = true;
				}
				if (otherPlayerPosition.HasValue && (otherPlayerPosition.Value - c.ToCenterVector2()).sqrMagnitude <= sqrMaxDist)
				{
					flag = true;
				}
				if (!flag)
				{
					return false;
				}
			}
			for (int i = 0; i < clearance.x; i++)
			{
				for (int j = 0; j < clearance.y; j++)
				{
					if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(new IntVector2(c.x + i, c.y + j)))
					{
						return false;
					}
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
					{
						return false;
					}
					if (!GameManager.Instance.Dungeon.data[c.x + i, c.y + j].isGridConnected)
					{
						return false;
					}
				}
			}
			return true;
		};
		IntVector2? randomAvailableCell = ParentRoom.GetRandomAvailableCell(clearance, PathableTiles, false, cellValidator);
		if (randomAvailableCell.HasValue)
		{
			base.specRigidbody.Initialize();
			Vector2 vector = base.specRigidbody.UnitCenter - base.transform.position.XY();
			Vector2 vec = Pathfinder.GetClearanceOffset(randomAvailableCell.Value, Clearance) - vector;
			vec = BraveUtility.QuantizeVector(vec);
			base.transform.position = vec;
			base.specRigidbody.Reinitialize();
		}
	}

	public void HandleReinforcementFallIntoRoom(float delay = 0f)
	{
		HasDonePlayerEnterCheck = true;
		IsInReinforcementLayer = true;
		StartCoroutine(HandleReinforcementFall_CR(delay));
	}

	protected void DisableOutlinesPostStart()
	{
		OnPostStartInitialization = (Action)Delegate.Remove(OnPostStartInitialization, new Action(DisableOutlinesPostStart));
		if (procedurallyOutlined)
		{
			SetOutlines(false);
		}
		if (HasShadow)
		{
			ToggleShadowVisiblity(false);
		}
	}

	private IEnumerator HandleReinforcementFall_CR(float delay)
	{
		if ((bool)m_customReinforceDoer)
		{
			m_customReinforceDoer.StartIntro();
			while (!m_customReinforceDoer.IsFinished)
			{
				yield return null;
			}
			m_customReinforceDoer.OnCleanup();
		}
		else
		{
			if (reinforceType == ReinforceType.Instant)
			{
				ToggleRenderers(true);
				OnEngaged(true);
				yield break;
			}
			ToggleRenderers(false);
			invisibleUntilAwaken = true;
			base.specRigidbody.CollideWithOthers = false;
			base.IsGone = true;
			if ((bool)base.knockbackDoer)
			{
				base.knockbackDoer.SetImmobile(true, "awaken");
			}
			if ((bool)base.behaviorSpeculator)
			{
				base.behaviorSpeculator.enabled = false;
			}
			base.healthHaver.IsVulnerable = false;
			if ((bool)base.aiShooter)
			{
				base.aiShooter.ToggleGunAndHandRenderers(false, "Reinforce");
			}
			AIActor aIActor = this;
			aIActor.OnPostStartInitialization = (Action)Delegate.Combine(aIActor.OnPostStartInitialization, new Action(DisableOutlinesPostStart));
			float elapsed = 0f;
			while (elapsed < delay)
			{
				elapsed += BraveTime.DeltaTime;
				yield return null;
			}
			float duration2 = 1.5f;
			if (reinforceType == ReinforceType.FullVfx)
			{
				tk2dBaseSprite component = ((GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_SpawnEnemy_Reticle"))).GetComponent<tk2dBaseSprite>();
				Vector3 position = base.transform.position + base.sprite.GetRelativePositionFromAnchor(tk2dBaseSprite.Anchor.LowerCenter).ToVector3ZUp();
				AkSoundEngine.PostEvent(string.IsNullOrEmpty(OverrideSpawnReticleAudio) ? "Play_ENM_spawn_reticle_01" : OverrideSpawnReticleAudio, base.gameObject);
				component.transform.position = position;
				component.HeightOffGround = -1.5f;
				component.UpdateZDepth();
				tk2dSpriteAnimator component2 = component.GetComponent<tk2dSpriteAnimator>();
				if ((bool)component2)
				{
					duration2 = (float)component2.DefaultClip.frames.Length / component2.DefaultClip.fps;
				}
			}
			elapsed = 0f;
			while (elapsed < duration2)
			{
				elapsed += LocalDeltaTime;
				ToggleRenderers(false);
				if ((bool)base.aiShooter)
				{
					base.aiShooter.ToggleGunAndHandRenderers(false, "Reinforce");
				}
				yield return null;
			}
			if (reinforceType == ReinforceType.FullVfx)
			{
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_Bullet_Spawn"));
				AkSoundEngine.PostEvent(string.IsNullOrEmpty(OverrideSpawnAppearAudio) ? "Play_ENM_spawn_appear_01" : OverrideSpawnAppearAudio, base.gameObject);
				tk2dBaseSprite component3 = gameObject.GetComponent<tk2dBaseSprite>();
				gameObject.transform.localScale = new Vector3(Mathf.Sign(UnityEngine.Random.value - 0.5f), Mathf.Sign(UnityEngine.Random.value - 0.5f), 1f);
				component3.transform.position = base.specRigidbody.UnitCenter;
				component3.HeightOffGround = 0.5f;
				component3.UpdateZDepth();
			}
			duration2 = 0.125f;
			while (elapsed < duration2)
			{
				elapsed += LocalDeltaTime;
				ToggleRenderers(false);
				if ((bool)base.aiShooter)
				{
					base.aiShooter.ToggleGunAndHandRenderers(false, "Reinforce");
				}
				yield return null;
			}
			if (!invisibleUntilAwaken)
			{
				base.specRigidbody.CollideWithOthers = true;
				base.IsGone = false;
				if ((bool)base.knockbackDoer)
				{
					base.knockbackDoer.SetImmobile(false, "awaken");
				}
			}
			if ((bool)base.behaviorSpeculator)
			{
				base.behaviorSpeculator.enabled = true;
			}
			base.healthHaver.IsVulnerable = true;
			ToggleRenderers(true);
			if (procedurallyOutlined)
			{
				SetOutlines(true);
			}
			bool isPlayingAwaken = false;
			if ((bool)base.aiAnimator)
			{
				m_awakenAnimation = base.aiAnimator.PlayDefaultSpawnState(out isPlayingAwaken);
			}
			State = ((!string.IsNullOrEmpty(m_awakenAnimation)) ? ActorState.Awakening : ActorState.Normal);
			if ((bool)base.aiShooter)
			{
				bool value = isPlayingAwaken || State == ActorState.Normal;
				base.aiShooter.ToggleGunAndHandRenderers(value, "Reinforce");
			}
			OnEngaged(true);
		}
		if (!base.behaviorSpeculator)
		{
			yield break;
		}
		List<TargetBehaviorBase> targetBehaviors = base.behaviorSpeculator.TargetBehaviors;
		for (int i = 0; i < targetBehaviors.Count; i++)
		{
			TargetPlayerBehavior targetPlayerBehavior = targetBehaviors[i] as TargetPlayerBehavior;
			if (targetPlayerBehavior != null)
			{
				targetPlayerBehavior.LineOfSight = false;
				targetPlayerBehavior.Radius = 1000f;
				targetPlayerBehavior.ObjectPermanence = true;
			}
		}
	}

	private void OnEngaged(bool isReinforcement = false)
	{
		if (m_hasBeenEngaged)
		{
			return;
		}
		if (SetsFlagOnActivation)
		{
			GameStatsManager.Instance.SetFlag(FlagToSetOnActivation, true);
		}
		if (!isReinforcement && (bool)m_customEngageDoer && !m_customEngageDoer.IsFinished)
		{
			StartCoroutine(DoCustomEngage());
			return;
		}
		if (invisibleUntilAwaken)
		{
			ToggleRenderers(true);
		}
		if (base.aiAnimator != null && m_awakenAnimation == null)
		{
			if (AwakenAnimType == AwakenAnimationType.Spawn)
			{
				m_awakenAnimation = base.aiAnimator.PlayDefaultAwakenedState();
			}
			else
			{
				m_awakenAnimation = base.aiAnimator.PlayDefaultSpawnState();
			}
		}
		State = ((!string.IsNullOrEmpty(m_awakenAnimation)) ? ActorState.Awakening : ActorState.Normal);
		if ((bool)base.aiShooter && invisibleUntilAwaken && State == ActorState.Awakening)
		{
			base.aiShooter.ToggleGunAndHandRenderers(false, "Awaken");
		}
		if (base.healthHaver.IsBoss && base.healthHaver.HasHealthBar)
		{
			GameUIBossHealthController gameUIBossHealthController = (base.healthHaver.UsesVerticalBossBar ? GameUIRoot.Instance.bossControllerSide : (base.healthHaver.UsesSecondaryBossBar ? GameUIRoot.Instance.bossController2 : GameUIRoot.Instance.bossController));
			string bossName = GetActorName();
			if (!string.IsNullOrEmpty(base.healthHaver.overrideBossName))
			{
				bossName = StringTableManager.GetEnemiesString(base.healthHaver.overrideBossName);
			}
			gameUIBossHealthController.RegisterBossHealthHaver(base.healthHaver, bossName);
		}
		if (OnEngagedVFX != null)
		{
			Vector2 relativePositionFromAnchor = base.sprite.GetRelativePositionFromAnchor(OnEngagedVFXAnchor);
			GameObject gameObject = SpawnManager.SpawnVFX(OnEngagedVFX);
			Transform transform = gameObject.transform;
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			transform.parent = base.transform;
			transform.localPosition = relativePositionFromAnchor.ToVector3ZUp(-0.1f);
			component.automaticallyManagesDepth = false;
			base.sprite.AttachRenderer(component);
		}
		int count;
		if (IdentifierForEffects == EnemyTypeIdentifier.SNIPER_TYPE && PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.SNIPER_WOLF, out count))
		{
			ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect);
		}
		m_hasBeenEngaged = true;
	}

	private IEnumerator DoCustomEngage()
	{
		m_customEngageDoer.StartIntro();
		while (!m_customEngageDoer.IsFinished)
		{
			yield return null;
		}
		m_customEngageDoer.OnCleanup();
	}

	private void HandleLootPinata(int additionalMetas = 0)
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		Vector3 vector = ((!base.specRigidbody) ? base.transform.position : base.specRigidbody.UnitCenter.ToVector3ZUp(base.transform.position.z));
		if (SpawnLootAtRewardChestPos && parentRoom != null)
		{
			IntVector2 rewardChestSpawnPosition = parentRoom.area.runtimePrototypeData.rewardChestSpawnPosition;
			if (rewardChestSpawnPosition.x >= 0 && rewardChestSpawnPosition.y >= 0)
			{
				vector = (parentRoom.area.UnitBottomLeft + rewardChestSpawnPosition.ToCenterVector2()).ToVector3ZisY();
			}
		}
		if (base.healthHaver.IsBoss && !parentRoom.HasOtherBoss(this))
		{
			int num = 1;
			if (base.healthHaver.IsSubboss)
			{
				num = 1;
			}
			else
			{
				num = UnityEngine.Random.Range(GameManager.Instance.RewardManager.CurrentRewardData.MinMetaCurrencyFromBoss, GameManager.Instance.RewardManager.CurrentRewardData.MaxMetaCurrencyFromBoss + 1);
				GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.Dungeon.tileIndices.tilesetId;
				if (tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON || tilesetId == GlobalDungeonData.ValidTilesets.RATGEON)
				{
					num = 0;
				}
			}
			if (GameManager.Instance.InTutorial)
			{
				num = 0;
			}
			num += additionalMetas;
			if (GameManager.Instance.BestActivePlayer.CharacterUsesRandomGuns && (!ChallengeManager.CHALLENGE_MODE_ACTIVE || ChallengeManager.Instance.ChallengeMode != ChallengeModeType.ChallengeMegaMode))
			{
				num = 0;
			}
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT)
			{
				num = UnityEngine.Random.Range(1, 3);
			}
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH)
			{
				num = 0;
			}
			int num2 = AssignedCurrencyToDrop;
			if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH || GameManager.Instance.InTutorial)
			{
				num2 = 0;
			}
			if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON)
			{
				num2 = 0;
			}
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].IsDarkSoulsHollow && !GameManager.Instance.AllPlayers[i].IsGhost)
				{
					num2 = 0;
					num = 0;
				}
			}
			if (num > 0)
			{
				if (ParentRoom != null && !ParentRoom.PlayerHasTakenDamageInThisRoom)
				{
					num *= 2;
				}
				Vector2 value = Vector2.down * 1.5f;
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FORGEGEON && (bool)GameManager.Instance.BestActivePlayer && (bool)base.specRigidbody)
				{
					value = BraveUtility.GetMajorAxis(GameManager.Instance.BestActivePlayer.CenterPosition - vector.XY()).normalized * 1.5f;
				}
				float num3 = 0.05f;
				float startingZForce = 4f;
				if ((bool)base.specRigidbody)
				{
					num3 = Mathf.Max(0.05f, base.specRigidbody.UnitCenter.y - base.specRigidbody.UnitBottom);
					Debug.Log("assigning SZH: " + num3);
				}
				LootEngine.SpawnCurrency(vector.XY(), num, true, value, 45f, startingZForce, num3);
			}
			if (PassiveItem.IsFlagSetAtAll(typeof(BankBagItem)))
			{
				num2 *= 2;
			}
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if ((bool)playerController)
				{
					float statValue = playerController.stats.GetStatValue(PlayerStats.StatType.MoneyMultiplierFromEnemies);
					if (statValue != 1f && statValue > 0f)
					{
						num2 = Mathf.RoundToInt((float)num2 * statValue);
					}
				}
			}
			if (num2 > 0)
			{
				list.AddRange(GameManager.Instance.Dungeon.sharedSettingsPrefab.GetCurrencyToDrop(num2));
			}
		}
		else if ((CanDropCurrency || AdditionalSingleCoinDropChance > 0f) && !HasDamagedPlayer && !base.healthHaver.IsBoss)
		{
			GenericCurrencyDropSettings currencyDropSettings = GameManager.Instance.Dungeon.sharedSettingsPrefab.currencyDropSettings;
			int num4 = (CanDropCurrency ? AssignedCurrencyToDrop : 0);
			if (AdditionalSingleCoinDropChance > 0f && UnityEngine.Random.value < AdditionalSingleCoinDropChance)
			{
				num4++;
			}
			if (IsBlackPhantom)
			{
				num4 += currencyDropSettings.blackPhantomCoinDropChances.SelectByWeight();
			}
			for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
			{
				if (GameManager.Instance.AllPlayers[k].IsDarkSoulsHollow && !GameManager.Instance.AllPlayers[k].IsGhost)
				{
					num4 = 0;
				}
			}
			if (PassiveItem.IsFlagSetAtAll(typeof(BankBagItem)))
			{
				num4 *= 2;
			}
			for (int l = 0; l < GameManager.Instance.AllPlayers.Length; l++)
			{
				PlayerController playerController2 = GameManager.Instance.AllPlayers[l];
				if ((bool)playerController2)
				{
					float statValue2 = playerController2.stats.GetStatValue(PlayerStats.StatType.MoneyMultiplierFromEnemies);
					if (statValue2 != 1f && statValue2 > 0f)
					{
						num4 = Mathf.RoundToInt((float)num4 * statValue2);
					}
				}
			}
			if (num4 > 0)
			{
				list.AddRange(GameManager.Instance.Dungeon.sharedSettingsPrefab.GetCurrencyToDrop(num4));
			}
		}
		if (AdditionalSimpleItemDrops.Count > 0)
		{
			for (int m = 0; m < AdditionalSimpleItemDrops.Count; m++)
			{
				list.Add(AdditionalSimpleItemDrops[m].gameObject);
			}
		}
		float num5 = 360f / (float)list.Count;
		for (int n = 0; n < list.Count; n++)
		{
			Vector3 vector2 = Quaternion.Euler(0f, 0f, num5 * (float)n) * Vector3.up;
			vector2 *= 2f;
			float x = 0f;
			tk2dBaseSprite component = list[n].GetComponent<tk2dBaseSprite>();
			if (component != null)
			{
				x = -1f * component.GetBounds().center.x;
			}
			if ((bool)list[n].GetComponent<RobotArmBalloonsItem>() || (bool)list[n].GetComponent<RobotArmItem>())
			{
				LootEngine.SpawnItem(list[n], vector + new Vector3(x, 0f, 0f), Vector2.zero, 0.5f, true, true);
				continue;
			}
			GameObject gameObject = SpawnManager.SpawnDebris(list[n], vector + new Vector3(x, 0f, 0f), Quaternion.identity);
			DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
			orAddComponent.shouldUseSRBMotion = true;
			orAddComponent.angularVelocity = 0f;
			orAddComponent.Priority = EphemeralObject.EphemeralPriority.Critical;
			orAddComponent.Trigger(vector2.WithZ(4f), 0.05f);
			orAddComponent.canRotate = false;
		}
		if (CustomChestTable != null && UnityEngine.Random.value < ChanceToDropCustomChest)
		{
			GameObject gameObject2 = CustomChestTable.SelectByWeight();
			if ((bool)gameObject2)
			{
				IntVector2? randomAvailableCell = parentRoom.GetRandomAvailableCell(IntVector2.One * 4, CellTypes.FLOOR);
				IntVector2? intVector = ((!randomAvailableCell.HasValue) ? null : new IntVector2?(randomAvailableCell.GetValueOrDefault() + IntVector2.One));
				if (intVector.HasValue)
				{
					Chest chest = Chest.Spawn(gameObject2.GetComponent<Chest>(), intVector.Value);
					if ((bool)chest)
					{
						chest.RegisterChestOnMinimap(parentRoom);
					}
				}
			}
		}
		GameObject gameObject3 = null;
		if (CanDropItems && CustomLootTable != null)
		{
			int num6 = UnityEngine.Random.Range(CustomLootTableMinDrops, CustomLootTableMaxDrops);
			if (num6 == 1)
			{
				gameObject3 = CustomLootTable.SelectByWeight();
				if (gameObject3 != null)
				{
					LootEngine.SpawnItem(gameObject3, vector, Vector2.up, 1f, true, true);
				}
			}
			else
			{
				List<GameObject> list2 = new List<GameObject>();
				for (int num7 = 0; num7 < num6; num7++)
				{
					for (int num8 = 0; num8 < 3; num8++)
					{
						gameObject3 = CustomLootTable.SelectByWeight();
						if (CanDropDuplicateItems || gameObject3 == null || !list2.Contains(gameObject3))
						{
							break;
						}
					}
					if (gameObject3 != null && (CanDropDuplicateItems || !list2.Contains(gameObject3)))
					{
						list2.Add(gameObject3);
					}
				}
				LootEngine.SpewLoot(list2, vector);
			}
		}
		if (AdditionalSafeItemDrops.Count > 0 && GameStatsManager.Instance.IsRainbowRun && IsMimicEnemy)
		{
			Vector2 vector3 = vector.XY();
			RoomHandler absoluteRoom = vector3.GetAbsoluteRoom();
			LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteMimic, vector3, absoluteRoom, true);
			return;
		}
		for (int num9 = 0; num9 < AdditionalSafeItemDrops.Count; num9++)
		{
			RoomHandler absoluteRoomFromPosition = parentRoom;
			if (IsMimicEnemy)
			{
				IntVector2 pos = vector.XY().ToIntVector2(VectorConversions.Floor);
				absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetRoomFromPosition(pos);
				if (absoluteRoomFromPosition == null)
				{
					LootEngine.SpawnItem(AdditionalSafeItemDrops[num9].gameObject, vector, Vector2.up, 1f, true, true);
					continue;
				}
				absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(pos);
			}
			LootEngine.SpawnItem(spawnPosition: absoluteRoomFromPosition.GetBestRewardLocation(IntVector2.One, vector.XY()).ToCenterVector2(), item: AdditionalSafeItemDrops[num9].gameObject, spawnDirection: Vector2.zero, force: 0f);
		}
	}

	public void EraseFromExistenceWithRewards(bool suppressDeathSounds = false)
	{
		HandleRewards();
		EraseFromExistence(suppressDeathSounds);
	}

	public void EraseFromExistence(bool suppressDeathSounds = false)
	{
		base.StealthDeath = true;
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.InterruptAndDisable();
		}
		if (suppressDeathSounds)
		{
			base.healthHaver.SuppressDeathSounds = true;
		}
		base.healthHaver.ApplyDamage(1E+07f, Vector2.zero, "Erasure", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void PreDeath(Vector2 finalDamageDirection)
	{
		if ((bool)base.aiAnimator)
		{
			base.aiAnimator.FpsScale = 1f;
		}
		if (shadowDeathType != ShadowDeathType.None)
		{
			float num = 0f;
			if ((bool)base.aiAnimator && base.aiAnimator.HasDirectionalAnimation("death"))
			{
				num = base.aiAnimator.GetDirectionalAnimationLength("death");
			}
			else
			{
				tk2dSpriteAnimationClip deathClip = base.healthHaver.GetDeathClip(finalDamageDirection.ToAngle());
				if (deathClip != null)
				{
					num = (float)deathClip.frames.Length / deathClip.fps;
				}
			}
			if (num > 0f)
			{
				if (shadowDeathType == ShadowDeathType.Fade)
				{
					StartCoroutine(FadeShadowCR(num));
				}
				else
				{
					StartCoroutine(ScaleShadowCR(num));
				}
			}
		}
		if (base.healthHaver.IsBoss)
		{
			if (HasBeenGlittered)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.KILL_BOSS_WITH_GLITTER);
			}
			if (IsBlackPhantom)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.BEAT_A_JAMMED_BOSS);
			}
		}
		else if (IsBlackPhantom && !SuppressBlackPhantomCorpseBurn)
		{
			StartCoroutine(BurnBlackPhantomCorpse());
		}
		if (!IsNormalEnemy)
		{
			return;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && playerController.healthHaver.IsAlive && playerController.IsInMinecart)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.ENEMIES_KILLED_WHILE_IN_CARTS, 1f);
				break;
			}
		}
	}

	private IEnumerator FadeShadowCR(float scaleTime)
	{
		if ((bool)ShadowObject && (bool)base.aiAnimator)
		{
			float timer = 0f;
			tk2dSprite shadowSprite = ShadowObject.GetComponent<tk2dSprite>();
			while (timer < scaleTime)
			{
				yield return null;
				timer += BraveTime.DeltaTime;
				shadowSprite.color = shadowSprite.color.WithAlpha(1f - base.aiAnimator.CurrentClipProgress);
			}
		}
	}

	private IEnumerator ScaleShadowCR(float scaleTime)
	{
		if ((bool)ShadowObject)
		{
			float timer = 0f;
			while (timer < scaleTime)
			{
				yield return null;
				timer += BraveTime.DeltaTime;
				ShadowObject.transform.localScale = Vector3.one * Mathf.Clamp01(1f - timer / scaleTime);
			}
		}
	}

	public void Transmogrify(AIActor EnemyPrefab, GameObject EffectVFX)
	{
		if ((!IsTransmogrified || !(ActorName == EnemyPrefab.ActorName)) && !IsMimicEnemy && (bool)base.healthHaver && !base.healthHaver.IsBoss && base.healthHaver.IsVulnerable && parentRoom != null)
		{
			Vector2 centerPosition = base.CenterPosition;
			if (EffectVFX != null)
			{
				SpawnManager.SpawnVFX(EffectVFX, centerPosition, Quaternion.identity);
			}
			AIActor aIActor = Spawn(EnemyPrefab, centerPosition.ToIntVector2(VectorConversions.Floor), parentRoom, true);
			if ((bool)aIActor)
			{
				aIActor.IsTransmogrified = true;
			}
			if (EnemyPrefab.name == "Chicken")
			{
				AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
				AkSoundEngine.PostEvent("Play_PET_chicken_cluck_01", base.gameObject);
			}
			else if (EnemyPrefab.name == "Snake")
			{
				AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
			}
			else
			{
				AkSoundEngine.PostEvent("Play_ENM_wizardred_appear_01", base.gameObject);
			}
			HandleRewards();
			EraseFromExistence();
		}
	}

	private void Die(Vector2 finalDamageDirection)
	{
		ForceDeath(finalDamageDirection);
	}

	private IEnumerator BurnBlackPhantomCorpse()
	{
		Material targetMaterial = base.sprite.renderer.material;
		float ela = 0f;
		float dura = 0.5f;
		while (ela < dura)
		{
			ela += BraveTime.DeltaTime;
			float t = ela / dura;
			targetMaterial.SetFloat("_BurnAmount", t);
			yield return null;
		}
	}

	private void HandleRewards()
	{
		if (m_hasGivenRewards || IsTransmogrified)
		{
			return;
		}
		GameStatsManager.Instance.huntProgress.ProcessKill(this);
		int additionalMetas = 0;
		if (SetsFlagOnDeath)
		{
			if (FlagToSetOnDeath == GungeonFlags.TUTORIAL_COMPLETED && !GameStatsManager.Instance.GetFlag(GungeonFlags.TUTORIAL_RECEIVED_META_CURRENCY))
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.TUTORIAL_RECEIVED_META_CURRENCY, true);
				additionalMetas = 10;
			}
			GameStatsManager.Instance.SetFlag(FlagToSetOnDeath, true);
			if (FlagToSetOnDeath == GungeonFlags.BOSSKILLED_DRAGUN || FlagToSetOnDeath == GungeonFlags.BOSSKILLED_HIGHDRAGUN)
			{
				if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Robot)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_DRAGUN_WITH_ROBOT, true);
				}
				if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Bullet)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_DRAGUN_WITH_BULLET, true);
				}
				if (GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Eevee)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_DRAGUN_PARADOX, true);
				}
				if (GameManager.Instance.BestActivePlayer.CharacterUsesRandomGuns)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.SORCERESS_BLESSED_MODE_COMPLETE, true);
				}
				if (GameManager.IsTurboMode)
				{
					GameStatsManager.Instance.SetFlag(GungeonFlags.BOSSKILLED_DRAGUN_TURBO_MODE, true);
					GameStatsManager.Instance.SetFlag(GungeonFlags.TONIC_TURBO_MODE_COMPLETE, true);
				}
			}
		}
		if (SetsCharacterSpecificFlagOnDeath)
		{
			GameStatsManager.Instance.SetCharacterSpecificFlag(CharacterSpecificFlagToSetOnDeath, true);
		}
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.ENEMIES_KILLED, 1f);
		HandleLootPinata(additionalMetas);
		if (OnHandleRewards != null)
		{
			OnHandleRewards();
		}
		m_hasGivenRewards = true;
	}

	public void ForceDeath(Vector2 finalDamageDirection, bool allowCorpse = true)
	{
		EncounterTrackable component = GetComponent<EncounterTrackable>();
		if (component != null)
		{
			GameStatsManager.Instance.HandleEncounteredObject(component);
		}
		SpawnEnemyOnDeath component2 = GetComponent<SpawnEnemyOnDeath>();
		if ((bool)component2)
		{
			component2.ManuallyTrigger(finalDamageDirection);
		}
		HandleRewards();
		if (!base.StealthDeath)
		{
			OnCorpseVFX.SpawnAtPosition(base.specRigidbody.GetUnitCenter(ColliderType.HitBox), 0f, null, Vector2.zero, Vector2.zero);
		}
		if (CorpseObject != null && !m_isFalling && allowCorpse && !base.StealthDeath)
		{
			if (IsBlackPhantom)
			{
				if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && (bool)base.sprite)
				{
					Vector3 vector = base.sprite.WorldBottomLeft.ToVector3ZisY();
					Vector3 vector2 = base.sprite.WorldTopRight.ToVector3ZisY();
					Vector3 vector3 = vector2 - vector;
					vector += vector3 * 0.15f;
					vector2 -= vector3 * 0.15f;
					float num = (vector2.y - vector.y) * (vector2.x - vector.x);
					int num2 = Mathf.CeilToInt(40f * num);
					int num3 = num2;
					Vector3 minPosition = vector;
					Vector3 maxPosition = vector2;
					Vector3 direction = Vector3.up / 2f;
					float angleVariance = 120f;
					float magnitudeVariance = 0.2f;
					float? startLifetime = UnityEngine.Random.Range(1f, 1.65f);
					GlobalSparksDoer.DoRandomParticleBurst(num3, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
				}
				PlayEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_BlackPhantomDeath") as GameObject, Vector3.zero, false);
			}
			else
			{
				GameObject gameObject = SpawnManager.SpawnDebris(CorpseObject, base.transform.position, Quaternion.identity);
				DebrisObject component3 = gameObject.GetComponent<DebrisObject>();
				if ((bool)component3)
				{
					if (PassiveItem.IsFlagSetAtAll(typeof(CorpseExplodeActiveItem)))
					{
						component3.Priority = EphemeralObject.EphemeralPriority.Critical;
					}
					component3.IsCorpse = true;
				}
				StaticReferenceManager.AllCorpses.Add(gameObject);
				tk2dSprite component4 = gameObject.GetComponent<tk2dSprite>();
				CorpseSpawnController component5 = gameObject.GetComponent<CorpseSpawnController>();
				if ((bool)component5)
				{
					component5.Init(this);
				}
				bool flag = true;
				if (component4 != null && component5 == null)
				{
					Material sharedMaterial = base.sprite.renderer.sharedMaterial;
					component4.SetSprite(base.sprite.Collection, base.sprite.spriteId);
					component4.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_COMPLEX;
					if (CorpseShadow && !m_isPaletteSwapped)
					{
						Renderer renderer = component4.renderer;
						if (sharedMaterial.HasProperty("_OverrideColor") && sharedMaterial.GetColor("_OverrideColor").a > 0f)
						{
							renderer.material = sharedMaterial;
							Color value = base.CurrentOverrideColor;
							for (int i = 0; i < m_activeEffects.Count; i++)
							{
								if (m_activeEffects[i].AppliesDeathTint)
								{
									value = m_activeEffects[i].DeathTintColor;
									renderer.material.SetFloat("_ValueMaximum", 0.6f);
									renderer.material.SetFloat("_ValueMinimum", 0.2f);
									if (renderer.material.shader.name.Contains("PixelShadow"))
									{
										renderer.material.shader = ShaderCache.Acquire("Brave/LitCutoutUber");
									}
								}
							}
							renderer.material.SetColor("_OverrideColor", value);
							renderer.material.DisableKeyword("BRIGHTNESS_CLAMP_OFF");
							renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
							renderer.material.DisableKeyword("EMISSIVE_ON");
							renderer.material.EnableKeyword("EMISSIVE_OFF");
							flag = false;
						}
						else
						{
							renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutFastPixelShadow");
						}
					}
					else if (CorpseShadow && m_isPaletteSwapped)
					{
						Renderer renderer2 = component4.renderer;
						if (sharedMaterial.HasProperty("_OverrideColor") && sharedMaterial.GetColor("_OverrideColor").a > 0f)
						{
							renderer2.material = sharedMaterial;
							Color value2 = base.CurrentOverrideColor;
							for (int j = 0; j < m_activeEffects.Count; j++)
							{
								if (m_activeEffects[j].AppliesDeathTint)
								{
									value2 = m_activeEffects[j].DeathTintColor;
									renderer2.material.SetFloat("_ValueMaximum", 0.6f);
									renderer2.material.SetFloat("_ValueMinimum", 0.2f);
								}
							}
							renderer2.material.SetColor("_OverrideColor", value2);
							renderer2.material.DisableKeyword("BRIGHTNESS_CLAMP_OFF");
							renderer2.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
							renderer2.material.DisableKeyword("EMISSIVE_ON");
							renderer2.material.EnableKeyword("EMISSIVE_OFF");
							flag = false;
						}
						else
						{
							renderer2.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutFastPixelShadowPalette");
							renderer2.material.SetTexture("_MainTex", sharedMaterial.GetTexture("_MainTex"));
							renderer2.material.SetTexture("_PaletteTex", sharedMaterial.GetTexture("_PaletteTex"));
						}
					}
					else if (m_isPaletteSwapped)
					{
						component4.renderer.material = sharedMaterial;
					}
					if (TransferShadowToCorpse && (bool)ShadowObject)
					{
						ShadowObject.transform.parent = gameObject.transform;
					}
					component4.IsPerpendicular = false;
					component4.HeightOffGround = -1f;
					component4.UpdateZDepth();
				}
				if (component3 != null)
				{
					if (finalDamageDirection != Vector2.zero)
					{
						finalDamageDirection.Normalize();
					}
					component3.Trigger(finalDamageDirection, 0.1f);
					if (flag)
					{
						component3.FadeToOverrideColor(new Color(0f, 0f, 0f, 0.6f), 0.25f);
					}
					component3.AssignFinalWorldDepth(-1.25f);
				}
			}
		}
		if (IsMimicEnemy)
		{
			for (int k = 0; k < GameManager.Instance.AllPlayers.Length; k++)
			{
				if ((bool)GameManager.Instance.AllPlayers[k] && GameManager.Instance.AllPlayers[k].OnChestBroken != null)
				{
					GameManager.Instance.AllPlayers[k].OnChestBroken(GameManager.Instance.AllPlayers[k], null);
				}
			}
		}
		if (base.healthHaver.IsBoss && !base.healthHaver.IsSubboss && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.NONE)
		{
			bool flag2 = false;
			List<AIActor> activeEnemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			for (int l = 0; l < activeEnemies.Count; l++)
			{
				HealthHaver healthHaver = activeEnemies[l].healthHaver;
				if ((bool)healthHaver && healthHaver.IsBoss && healthHaver.IsAlive && activeEnemies[l] != base.aiActor)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON)
				{
					InfinilichDeathController component6 = GetComponent<InfinilichDeathController>();
					if ((bool)component6)
					{
						GameManager.Instance.Dungeon.FloorCleared();
					}
				}
				else
				{
					GameManager.Instance.Dungeon.FloorCleared();
				}
			}
		}
		if (parentRoom != null)
		{
			parentRoom.DeregisterEnemy(this);
		}
		else
		{
			Debug.LogError("An enemy who does not have a parent room is dying... this could be a problem.");
		}
		Pathfinder.Instance.RemoveActorPath(m_upcomingPathTiles);
	}

	private void Damaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (!HasBeenEngaged)
		{
			HasBeenEngaged = true;
		}
		if (damageCategory != DamageCategory.DamageOverTime && (bool)base.aiAnimator)
		{
			base.aiAnimator.PlayHitState(damageDirection);
		}
	}

	public void StrafeTarget(float targetDistance)
	{
	}

	public void JumpToPoint(Vector2 targetPoint, float speedMultiplier, float jumpHeight)
	{
		float num = Vector2.Distance(base.transform.position.XY(), targetPoint);
		float jumpTime = num / (MovementSpeed * speedMultiplier);
		StartCoroutine(HandleJumpToPoint(targetPoint, jumpTime, jumpHeight));
	}

	private IEnumerator HandleJumpToPoint(Vector2 flattenedEndPosition, float jumpTime, float jumpHeight)
	{
		m_isReadyForRepath = false;
		float elapsed = 0f;
		while (elapsed < jumpTime)
		{
			m_currentPath = null;
			yield return null;
		}
		m_isReadyForRepath = true;
	}

	public void SetAIMovementContribution(Vector2 vel)
	{
		m_currentPath = null;
	}

	private IEnumerator DashInDirection(Vector3 direction, float duration, float speedMultiplier)
	{
		m_isReadyForRepath = false;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += LocalDeltaTime;
			m_currentPath = null;
			yield return null;
		}
		m_isReadyForRepath = true;
	}

	private IEnumerator DashByDistance(Vector3 direction, float distance, float speedMultiplier)
	{
		m_isReadyForRepath = false;
		float velocityMagnitude = (direction.XY().normalized * MovementSpeed * speedMultiplier).magnitude;
		float elapsedDist = 0f;
		while (elapsedDist < distance)
		{
			elapsedDist += velocityMagnitude * LocalDeltaTime;
			m_currentPath = null;
			yield return null;
		}
		m_isReadyForRepath = true;
	}

	public void SimpleMoveToPosition(Vector3 targetPosition)
	{
		IntVector2 value = targetPosition.IntXY();
		Path path = null;
		path = new Path();
		path.Positions = new LinkedList<IntVector2>();
		path.Positions.AddFirst(value);
		m_currentPath = path;
	}

	private Vector2 CalculateTargetStrafeVelocity(Vector3 targetPosition, int direction, float targetDistance)
	{
		Vector2 vector = (Vector2)targetPosition - base.specRigidbody.UnitCenter;
		float magnitude = vector.magnitude;
		float num = 90f;
		if (magnitude > targetDistance)
		{
			num = 45f;
		}
		return (Quaternion.Euler(0f, 0f, num * Mathf.Sign(direction)) * new Vector3(vector.x, vector.y, 0f)).normalized * MovementSpeed;
	}

	private Vector2 CalculateSteering()
	{
		if (!TryDodgeBullets)
		{
			return Vector2.zero;
		}
		float num = 5f;
		Collider[] array = Physics.OverlapSphere(base.specRigidbody.UnitCenter, AvoidRadius);
		Vector2 vector = Vector2.zero;
		int num2 = 0;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.transform.parent != null && collider.transform.parent.GetComponent<Projectile>() != null)
			{
				SpeculativeRigidbody component = collider.transform.parent.GetComponent<SpeculativeRigidbody>();
				Vector2 velocity = component.Velocity;
				float num3 = Vector3.Distance(collider.transform.position, base.specRigidbody.UnitCenter);
				Vector3 vector2 = collider.transform.position + new Vector3(velocity.normalized.x * num3, velocity.normalized.y * num3, 0f);
				if (!(Vector3.Distance(base.specRigidbody.UnitCenter, vector2) > Vector3.Distance(base.specRigidbody.UnitCenter, collider.transform.position)))
				{
					int num4 = ((!(base.specRigidbody.UnitCenter.x < collider.transform.position.x)) ? 1 : (-1));
					Vector2 vector3 = (base.specRigidbody.UnitCenter - (Vector2)collider.transform.position) * num4;
					Vector2 vector4 = (vector2 - collider.transform.position) * num4;
					float num5 = Mathf.Atan2(vector3.y, vector3.x);
					float num6 = Mathf.Atan2(vector4.y, vector4.x);
					int num7 = ((!(num5 > num6)) ? (-90) : 90);
					float num8 = num3 / AvoidRadius;
					Vector3 vector5 = Quaternion.Euler(0f, 0f, num7) * new Vector3(velocity.x, velocity.y, 0f);
					Vector2 normalized = new Vector2(vector5.x, vector5.y).normalized;
					vector += normalized * (1f - num8);
					num2++;
				}
			}
		}
		if (num2 > 0)
		{
			vector = vector / num2 * num;
		}
		return vector;
	}

	protected override void Fall()
	{
		if (m_isFalling)
		{
			return;
		}
		base.Fall();
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.InterruptAndDisable();
		}
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(false, "Pitfall");
		}
		if ((bool)base.aiAnimator)
		{
			base.aiAnimator.FpsScale = 1f;
			if (!string.IsNullOrEmpty(OverridePitfallAnim))
			{
				base.aiAnimator.PlayUntilCancelled(OverridePitfallAnim);
			}
			else if (base.aiAnimator.HasDirectionalAnimation("pitfall"))
			{
				base.aiAnimator.PlayUntilCancelled("pitfall");
			}
			else if (base.spriteAnimator.GetClipByName("pitfall") != null)
			{
				base.aiAnimator.PlayUntilCancelled("pitfall");
			}
			else if (base.spriteAnimator.GetClipByName("pitfall_right") != null)
			{
				base.aiAnimator.PlayUntilCancelled("pitfall_right");
			}
		}
		StartCoroutine(FallDownCR());
	}

	private IEnumerator FallDownCR()
	{
		base.specRigidbody.CollideWithTileMap = false;
		base.specRigidbody.CollideWithOthers = false;
		base.IsGone = true;
		base.specRigidbody.Velocity = Vector2.zero;
		Vector2 accelVec = new Vector2(0f, -80f);
		float elapsed = 0f;
		Tribool readyForDepthSwap = Tribool.Unready;
		float m_cachedHeightOffGround = base.sprite.HeightOffGround;
		HasSplashed = false;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PitFallMovementRestrictor));
		while (base.renderer.enabled)
		{
			base.specRigidbody.Velocity = base.specRigidbody.Velocity + accelVec * LocalDeltaTime;
			bool isPlayingPitfall = (bool)base.aiAnimator && (base.aiAnimator.IsPlaying("pitfall") || base.aiAnimator.IsPlaying("pitfall_down") || base.aiAnimator.IsPlaying("pitfall_right"));
			if (!string.IsNullOrEmpty(OverridePitfallAnim) && (bool)base.aiAnimator)
			{
				isPlayingPitfall |= base.aiAnimator.IsPlaying(OverridePitfallAnim);
			}
			if (!isPlayingPitfall && elapsed > 0.1f)
			{
				base.renderer.enabled = false;
				SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
				base.specRigidbody.Velocity = Vector2.zero;
				accelVec = Vector2.zero;
				if (!HasSplashed)
				{
					HasSplashed = true;
					GameManager.Instance.Dungeon.tileIndices.DoSplashAtPosition(base.sprite.WorldCenter);
				}
			}
			if (!(readyForDepthSwap == Tribool.Complete) || !base.renderer.enabled)
			{
				if (readyForDepthSwap)
				{
					base.sprite.HeightOffGround = -4f;
					if (IsNormalEnemy)
					{
						TileSpriteClipper tileSpriteClipper = base.sprite.gameObject.AddComponent<TileSpriteClipper>();
						tileSpriteClipper.updateEveryFrame = true;
						tileSpriteClipper.doOptimize = false;
						tileSpriteClipper.clipMode = TileSpriteClipper.ClipMode.PitBounds;
						tileSpriteClipper.enabled = true;
						tk2dBaseSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(base.sprite);
						for (int i = 0; i < outlineSprites.Length; i++)
						{
							if ((bool)outlineSprites[i])
							{
								tileSpriteClipper = outlineSprites[i].gameObject.AddComponent<TileSpriteClipper>();
								tileSpriteClipper.updateEveryFrame = true;
								tileSpriteClipper.doOptimize = false;
								tileSpriteClipper.clipMode = TileSpriteClipper.ClipMode.PitBounds;
								tileSpriteClipper.enabled = true;
							}
						}
					}
					++readyForDepthSwap;
				}
				else if (!readyForDepthSwap)
				{
					Vector3 position = base.sprite.transform.position + base.sprite.GetBounds().center + new Vector3(0f, base.sprite.GetBounds().extents.y, 0f);
					if (GameManager.Instance.Dungeon.CellSupportsFalling(position))
					{
						++readyForDepthSwap;
					}
				}
			}
			base.sprite.UpdateZDepth();
			elapsed += LocalDeltaTime;
			yield return null;
		}
		base.sprite.HeightOffGround = m_cachedHeightOffGround;
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(PitFallMovementRestrictor));
		bool suppressDamage = false;
		if (this.CustomPitDeathHandling != null)
		{
			this.CustomPitDeathHandling(this, ref suppressDamage);
		}
		if (!suppressDamage)
		{
			base.healthHaver.IsVulnerable = true;
			base.healthHaver.minimumHealth = 0f;
			base.healthHaver.ApplyDamage(float.MaxValue, Vector2.zero, "enemy pit", CoreDamageTypes.None, DamageCategory.Unstoppable, true);
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.ENEMIES_KILLED_WITH_PITS, 1f);
		}
	}

	public override void RecoverFromFall()
	{
		base.RecoverFromFall();
		if ((bool)base.behaviorSpeculator)
		{
			base.behaviorSpeculator.enabled = true;
		}
		if ((bool)base.aiShooter)
		{
			base.aiShooter.ToggleGunAndHandRenderers(true, "Pitfall");
		}
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
		if ((bool)base.aiAnimator)
		{
			base.aiAnimator.EndAnimation();
		}
		base.specRigidbody.CollideWithTileMap = true;
		base.specRigidbody.CollideWithOthers = true;
		base.IsGone = false;
		base.renderer.enabled = true;
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
	}

	private void PitFallMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (validLocation)
		{
			PixelCollider hitboxPixelCollider = specRigidbody.HitboxPixelCollider;
			Vector2 vector = PhysicsEngine.PixelToUnitMidpoint(hitboxPixelCollider.UpperLeft);
			Vector2 vector2 = PhysicsEngine.PixelToUnitMidpoint(hitboxPixelCollider.UpperRight);
			Vector2 vector3 = PhysicsEngine.PixelToUnitMidpoint(hitboxPixelCollider.LowerLeft);
			Vector2 vector4 = PhysicsEngine.PixelToUnitMidpoint(hitboxPixelCollider.LowerRight);
			Vector2 vector5 = PhysicsEngine.PixelToUnit(prevPixelOffset);
			Vector2 vector6 = PhysicsEngine.PixelToUnit(pixelOffset);
			if ((GameManager.Instance.Dungeon.CellIsPit(vector + vector5) && !GameManager.Instance.Dungeon.CellIsPit(vector + vector6)) || (GameManager.Instance.Dungeon.CellIsPit(vector2 + vector5) && !GameManager.Instance.Dungeon.CellIsPit(vector2 + vector6)) || (GameManager.Instance.Dungeon.CellIsPit(vector3 + vector5) && !GameManager.Instance.Dungeon.CellIsPit(vector3 + vector6)) || (GameManager.Instance.Dungeon.CellIsPit(vector4 + vector5) && !GameManager.Instance.Dungeon.CellIsPit(vector4 + vector6)))
			{
				validLocation = false;
			}
		}
	}

	public void MoveToSafeSpot(float time)
	{
		m_isSafeMoving = false;
		if (!GameManager.HasInstance || GameManager.Instance.Dungeon == null)
		{
			return;
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		Vector2[] array = new Vector2[6]
		{
			base.specRigidbody.UnitBottomLeft,
			base.specRigidbody.UnitBottomCenter,
			base.specRigidbody.UnitBottomRight,
			base.specRigidbody.UnitTopLeft,
			base.specRigidbody.UnitTopCenter,
			base.specRigidbody.UnitTopRight
		};
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			IntVector2 intVector = array[i].ToIntVector2(VectorConversions.Floor);
			if (!data.CheckInBoundsAndValid(intVector) || data.isWall(intVector) || data.isTopWall(intVector.x, intVector.y) || data[intVector].isOccupied)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int j = 0; j < Clearance.x; j++)
			{
				int x = c.x + j;
				for (int k = 0; k < Clearance.y; k++)
				{
					int y = c.y + k;
					if (GameManager.Instance.Dungeon.data.isTopWall(x, y))
					{
						return false;
					}
				}
			}
			return true;
		};
		Vector2 vector = base.specRigidbody.UnitBottomCenter - base.transform.position.XY();
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		IntVector2? nearestAvailableCell = absoluteRoomFromPosition.GetNearestAvailableCell(base.specRigidbody.UnitCenter, Clearance, PathableTiles, false, cellValidator);
		if (nearestAvailableCell.HasValue)
		{
			m_isSafeMoving = true;
			m_safeMoveTimer = 0f;
			m_safeMoveTime = time;
			m_safeMoveStartPos = base.transform.position;
			m_safeMoveEndPos = Pathfinder.GetClearanceOffset(nearestAvailableCell.Value, Clearance).WithY(nearestAvailableCell.Value.y) - vector;
		}
		else
		{
			m_safeMoveStartPos = null;
			m_safeMoveEndPos = null;
		}
	}
}
