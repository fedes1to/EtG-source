using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;
using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : GameActor, ILevelLoadedListener
{
	public enum DodgeRollState
	{
		PreRollDelay,
		InAir,
		OnGround,
		None,
		AdditionalDelay,
		Blink
	}

	public enum EscapeSealedRoomStyle
	{
		DEATH_SEQUENCE,
		ESCAPE_SPIN,
		NONE,
		TELEPORTER,
		GRIP_MASTER
	}

	public const float c_averageVelocityPeriod = 0.5f;

	public const float s_dodgeRollBlinkMinPressTime = 0.2f;

	[Header("Player Properties")]
	public PlayableCharacters characterIdentity;

	[NonSerialized]
	private bool m_isTemporaryEeveeForUnlock;

	[NonSerialized]
	public Texture2D portalEeveeTex;

	[NonSerialized]
	public bool IsGhost;

	[NonSerialized]
	public bool IsDarkSoulsHollow;

	[Header("UI Stuff")]
	public string uiPortraitName;

	public float BosscardSpriteFPS;

	public List<Texture2D> BosscardSprites;

	public PerCharacterCoopPositionData CoopBosscardOffset;

	[Header("Stats")]
	public PlayerStats stats;

	public DodgeRollStats rollStats;

	public PitHelpers pitHelpers;

	public int MAX_GUNS_HELD = 3;

	public int MAX_ITEMS_HELD = 2;

	[NonSerialized]
	public bool UsingAlternateStartingGuns;

	[PickupIdentifier(typeof(Gun))]
	public List<int> startingGunIds;

	[PickupIdentifier(typeof(Gun))]
	public List<int> startingAlternateGunIds;

	[PickupIdentifier(typeof(Gun))]
	public List<int> finalFightGunIds;

	public PlayerConsumables carriedConsumables;

	public RandomStartingEquipmentSettings randomStartingEquipmentSettings;

	public bool AllowZeroHealthState;

	public bool ForceZeroHealthState;

	[NonSerialized]
	public bool HealthAndArmorSwapped;

	[NonSerialized]
	public List<LootModData> lootModData = new List<LootModData>();

	private int m_blanks;

	public Transform gunAttachPoint;

	[NonSerialized]
	public Transform secondaryGunAttachPoint;

	public Vector3 downwardAttachPointPosition;

	private Vector3 m_startingAttachPointPosition;

	public float collisionKnockbackStrength = 10f;

	public PlayerHandController primaryHand;

	public PlayerHandController secondaryHand;

	public Vector3 unadjustedAimPoint;

	private Vector2 m_lastVelocity;

	public Color outlineColor;

	public GameObject minimapIconPrefab;

	public tk2dSpriteAnimation AlternateCostumeLibrary;

	public List<ActorAudioEvent> animationAudioEvents;

	public string characterAudioSpeechTag;

	public bool usingForcedInput;

	public Vector2 forcedInput;

	public Vector2? forceAimPoint;

	public bool forceFire;

	public bool forceFireDown;

	public bool forceFireUp;

	public bool DrawAutoAim;

	[NonSerialized]
	public bool PastAccessible;

	public Action<PlayerController> OnIgnited;

	[NonSerialized]
	public bool WasPausedThisFrame;

	[NonSerialized]
	private bool m_isOnFire;

	[NonSerialized]
	private RuntimeGameActorEffectData m_onFireEffectData;

	[NonSerialized]
	public float CurrentFireMeterValue;

	[NonSerialized]
	public float CurrentPoisonMeterValue;

	[NonSerialized]
	public float CurrentDrainMeterValue;

	[NonSerialized]
	public float CurrentCurseMeterValue;

	[NonSerialized]
	public bool CurseIsDecaying = true;

	[NonSerialized]
	public float CurrentFloorDamageCooldown;

	[NonSerialized]
	public float CurrentStoneGunTimer;

	[NonSerialized]
	public List<IPlayerOrbital> orbitals = new List<IPlayerOrbital>();

	[NonSerialized]
	public List<PlayerOrbitalFollower> trailOrbitals = new List<PlayerOrbitalFollower>();

	[NonSerialized]
	public List<AIActor> companions = new List<AIActor>();

	private OverridableBool m_capableOfStealing = new OverridableBool(false);

	[NonSerialized]
	public bool IsEthereal;

	[NonSerialized]
	public bool HasGottenKeyThisRun;

	[NonSerialized]
	public int DeathsThisRun;

	private const float BasePoisonMeterDecayPerSecond = 0.5f;

	private const float BaseDrainMeterDecayPerSecond = 0.1f;

	private const float BaseCurseMeterDecayPerSecond = 0.5f;

	[NonSerialized]
	public Color baseFlatColorOverride = new Color(0f, 0f, 0f, 0f);

	[NonSerialized]
	public List<int> ActiveExtraSynergies = new List<int>();

	[NonSerialized]
	public List<CustomSynergyType> CustomEventSynergies = new List<CustomSynergyType>();

	[NonSerialized]
	public bool DeferredStatRecalculationRequired;

	public bool ForceMetalGearMenu;

	protected GungeonActions m_activeActions;

	[NonSerialized]
	public bool CharacterUsesRandomGuns;

	[NonSerialized]
	public bool UnderstandsGleepGlorp;

	private static float AAStickTime = 0f;

	private static float AANonStickTime = 0f;

	private static float AALastWarnTime = -1000f;

	private static bool AACanWarn = true;

	private const float AAStickMultiplier = 1.5f;

	private const float AAMinWarnDelay = 300f;

	private const float AATotalStickTime = 660f;

	private const float AAWarnTime = 300f;

	private const float AAActivateTime = 600f;

	public OverridableBool InfiniteAmmo = new OverridableBool(false);

	public OverridableBool OnlyFinalProjectiles = new OverridableBool(false);

	[NonSerialized]
	public bool IsStationary;

	[NonSerialized]
	public bool IsGunLocked;

	private TeleporterController m_returnTeleporter;

	private bool m_additionalReceivesTouchDamage = true;

	public bool IsTalking;

	private bool m_wasTalkingThisFrame;

	public TalkDoerLite TalkPartner;

	private bool m_isInCombat;

	public Action OnEnteredCombat;

	private float m_superDuperAutoAimTimer;

	private bool m_isVisible = true;

	[HideInInspector]
	public GunInventory inventory;

	[NonSerialized]
	private Gun m_cachedQuickEquipGun;

	[NonSerialized]
	private Gun m_backupCachedQuickEquipGun;

	[NonSerialized]
	public int maxActiveItemsHeld = 2;

	[NonSerialized]
	public int spiceCount;

	[PickupIdentifier(typeof(PlayerItem))]
	public List<int> startingActiveItemIds;

	[NonSerialized]
	public List<PlayerItem> activeItems = new List<PlayerItem>();

	[PickupIdentifier(typeof(PassiveItem))]
	public List<int> startingPassiveItemIds;

	[NonSerialized]
	public List<PassiveItem> passiveItems = new List<PassiveItem>();

	public List<StatModifier> ownerlessStatModifiers = new List<StatModifier>();

	[NonSerialized]
	public List<PickupObject> additionalItems = new List<PickupObject>();

	public bool ForceHandless;

	public bool HandsOnAltCostume;

	public bool SwapHandsOnAltCostume;

	public string altHandName;

	public bool hasArmorlessAnimations;

	public GameObject lostAllArmorVFX;

	public GameObject lostAllArmorAltVfx;

	public GameObject CustomDodgeRollEffect;

	public Func<Gun, Projectile, Projectile> OnPreFireProjectileModifier;

	private int m_enemiesKilled;

	private float m_gunGameDamageThreshold = 200f;

	private const float c_gunGameDamageThreshold = 200f;

	private float m_gunGameElapsed;

	private const float c_gunGameElapsedThreshold = 20f;

	private const float c_fireMeterChargeRate = 0.666666f;

	private const float c_fireMeterRollingChargeRate = 0.2f;

	private const float c_fireMeterRollDecrease = 0.5f;

	public Action<PlayerController, Chest> OnChestBroken;

	public Action<Projectile, PlayerController> OnHitByProjectile;

	public Action<PlayerController, Gun> OnReloadPressed;

	public Action<PlayerController, Gun> OnReloadedGun;

	public Action<FlippableCover> OnTableFlipped;

	public Action<FlippableCover> OnTableFlipCompleted;

	public Action<PlayerController> OnNewFloorLoaded;

	[HideInInspector]
	public Vector2 knockbackComponent;

	[HideInInspector]
	public Vector2 immutableKnockbackComponent;

	[HideInInspector]
	public OverridableBool ImmuneToPits = new OverridableBool(false);

	private MeshRenderer m_renderer;

	private CoinBloop m_blooper;

	private KeyBullet m_setupKeyBullet;

	public float RealtimeEnteredCurrentRoom;

	private RoomHandler m_currentRoom;

	private Vector3 m_spriteDimensions;

	private int m_equippedGunShift;

	private List<tk2dBaseSprite> m_attachedSprites = new List<tk2dBaseSprite>();

	private List<float> m_attachedSpriteDepths = new List<float>();

	[NonSerialized]
	public Dictionary<string, GameObject> SpawnedSubobjects = new Dictionary<string, GameObject>();

	private PlayerInputState m_inputState;

	private OverridableBool m_disableInput = new OverridableBool(false);

	protected bool m_shouldContinueFiring;

	protected bool m_handlingQueuedAnimation;

	private bool m_interruptingPitRespawn;

	private bool m_skipPitRespawn;

	private Vector2 lockedDodgeRollDirection;

	private int m_selectedItemIndex;

	private IPlayerInteractable m_lastInteractionTarget;

	private List<IPlayerInteractable> m_leapInteractables = new List<IPlayerInteractable>();

	private float m_currentGunAngle;

	private float? m_overrideGunAngle;

	[NonSerialized]
	public MineCartController currentMineCart;

	public MineCartController previousMineCart;

	protected DodgeRollState m_dodgeRollState = DodgeRollState.None;

	private float m_dodgeRollTimer;

	private bool m_isSlidingOverSurface;

	private Vector3 m_cachedAimDirection = Vector3.right;

	private bool m_cachedGrounded = true;

	private bool m_highAccuracyAimMode;

	private Vector2 m_previousAimVector;

	private int m_masteryTokensCollectedThisRun;

	[NonSerialized]
	public bool EverHadMap;

	[NonSerialized]
	public tk2dSpriteAnimation OverrideAnimationLibrary;

	[NonSerialized]
	private tk2dSpriteAnimation BaseAnimationLibrary;

	[NonSerialized]
	public bool PlayerIsRatTransformed;

	private string m_overridePlayerSwitchState;

	public int PlayerIDX = -1;

	[NonSerialized]
	public int NumRoomsCleared;

	[NonSerialized]
	public string LevelToLoadOnPitfall;

	[NonSerialized]
	private string m_cachedLevelToLoadOnPitfall;

	private const bool c_coopSynergies = true;

	[NonSerialized]
	public bool ZeroVelocityThisFrame;

	private float dx9counter;

	[NonSerialized]
	public bool IsUsingAlternateCostume;

	private bool m_usingCustomHandType;

	private int m_baseHandId;

	private tk2dSpriteCollectionData m_baseHandCollection;

	private StatModifier m_turboSpeedModifier;

	private StatModifier m_turboEnemyBulletModifier;

	private StatModifier m_turboRollSpeedModifier;

	public bool FlatColorOverridden;

	private bool m_usesRandomStartingEquipment;

	private bool m_randomStartingItemsInitialized;

	private int m_randomStartingEquipmentSeed = -1;

	public bool IsCurrentlyCoopReviving;

	private string[] confettiPaths;

	private float m_coopRoomTimer;

	private Material[] m_cachedOverrideMaterials;

	private bool m_isStartingTeleport;

	protected float m_elapsedNonalertTime;

	public Action<float, bool, HealthHaver> OnAnyEnemyReceivedDamage;

	private bool m_cloneWaitingForCoopDeath;

	public Action LostArmor;

	private bool m_revenging;

	private AfterImageTrailController m_hollowAfterImage;

	private Color m_ghostUnchargedColor = new Color(0f, 0f, 0f, 0f);

	private Color m_ghostChargedColor = new Color(0.2f, 0.3f, 1f, 1f);

	private bool m_isCoopArrowing;

	private bool m_isThreatArrowing;

	private AIActor m_threadArrowTarget;

	public Action<PlayerController> OnRealPlayerDeath;

	private bool m_suppressItemSwitchTo;

	protected GameObject BlankVFXPrefab;

	private Color m_alienDamageColor = new Color(1f, 0f, 0f, 1f);

	private Color m_alienBlankColor = new Color(0.35f, 0.35f, 1f, 1f);

	protected Coroutine m_currentActiveItemDestructionCoroutine;

	protected float m_postDodgeRollGunTimer;

	private const float AIM_VECTOR_MAGNITUDE_CUTOFF = 0.4f;

	public OverridableBool AdditionalCanDodgeRollWhileFlying = new OverridableBool(false);

	private bool m_handleDodgeRollStartThisFrame;

	private float m_timeHeldBlinkButton;

	private Vector2 m_cachedBlinkPosition;

	private tk2dSprite m_extantBlinkShadow;

	private int m_currentDodgeRollDepth;

	public Action<tk2dSprite> OnBlinkShadowCreated;

	public List<FlippableCover> TablesDamagedThisSlide = new List<FlippableCover>();

	private bool m_hasFiredWhileSliding;

	[NonSerialized]
	public bool LastFollowerVisibilityState = true;

	private bool m_gunChangePressedWhileMetalGeared;

	private int exceptionTracker;

	private bool m_interactedThisFrame;

	private bool m_preventItemSwitching;

	protected RoomHandler m_roomBeforeExit;

	protected RoomHandler m_previousExitLinkedRoom;

	protected bool m_inExitLastFrame;

	private List<IntVector2> m_bellygeonDepressedTiles = new List<IntVector2>();

	private static Dictionary<IntVector2, float> m_bellygeonDepressedTileTimers = new Dictionary<IntVector2, float>(new IntVector2EqualityComparer());

	private IntVector2 m_cachedLastCenterCellBellygeon = IntVector2.NegOne;

	private float m_highStressTimer;

	private Vector2 m_cachedTeleportSpot;

	private OverridableBool m_hideRenderers = new OverridableBool(false);

	private OverridableBool m_hideGunRenderers = new OverridableBool(false);

	private OverridableBool m_hideHandRenderers = new OverridableBool(false);

	private CellVisualData.CellFloorType? m_prevFloorType;

	protected List<AIActor> m_rollDamagedEnemies = new List<AIActor>();

	protected Vector2 m_playerCommandedDirection;

	private Vector2 m_lastNonzeroCommandedDirection;

	private float m_controllerSemiAutoTimer;

	private float m_startingMovementSpeed;

	private float m_maxIceFactor;

	private float m_blankCooldownTimer;

	public float gunReloadDisplayTimer;

	private float m_dropGunTimer;

	private float m_metalGearTimer;

	private int m_metalGearFrames;

	private bool m_gunWasDropped;

	private bool m_metalWasGeared;

	private float m_dropItemTimer;

	private bool m_itemWasDropped;

	private const float GunDropTimerThreshold = 0.5f;

	private const float MetalGearTimerThreshold = 0.175f;

	private const float CoopGhostBlankCooldown = 5f;

	private float c_iceVelocityMinClamp = 0.125f;

	private bool m_newFloorNoInput;

	private bool m_allowMoveAsAim;

	private float m_petDirection;

	public CompanionController m_pettingTarget;

	public bool IsTemporaryEeveeForUnlock
	{
		get
		{
			return m_isTemporaryEeveeForUnlock;
		}
		set
		{
			m_isTemporaryEeveeForUnlock = value;
			ClearOverrideShader();
			if (value)
			{
				Texture2D value2 = portalEeveeTex;
				if ((bool)this && (bool)base.sprite && (bool)base.sprite.renderer && (bool)base.sprite.renderer.material)
				{
					base.sprite.renderer.material.SetTexture("_EeveeTex", value2);
				}
			}
		}
	}

	public int Blanks
	{
		get
		{
			return m_blanks;
		}
		set
		{
			m_blanks = value;
			GameStatsManager.Instance.UpdateMaximum(TrackedMaximums.MOST_BLANKS_HELD, m_blanks);
			GameUIRoot.Instance.UpdatePlayerBlankUI(this);
			GameUIRoot.Instance.UpdatePlayerConsumables(carriedConsumables);
		}
	}

	public bool IsOnFire
	{
		get
		{
			return m_isOnFire;
		}
		set
		{
			if (value && HasActiveBonusSynergy(CustomSynergyType.FOSSIL_PHOENIX))
			{
				value = false;
			}
			if (value && stats.UsesFireSourceEffect)
			{
				if (m_onFireEffectData == null)
				{
					m_onFireEffectData = GameActorFireEffect.ApplyFlamesToTarget(this, stats.OnFireSourceEffect);
				}
			}
			else if (!value && stats.UsesFireSourceEffect && m_onFireEffectData != null)
			{
				GameActorFireEffect.DestroyFlames(m_onFireEffectData);
				m_onFireEffectData = null;
			}
			if (value && !m_isOnFire && OnIgnited != null)
			{
				OnIgnited(this);
			}
			m_isOnFire = value;
		}
	}

	public Vector2 SmoothedCameraCenter
	{
		get
		{
			if (!base.specRigidbody || base.specRigidbody.HitboxPixelCollider == null)
			{
				if ((bool)base.sprite)
				{
					return base.sprite.WorldCenter;
				}
				return base.transform.position.XY();
			}
			return base.specRigidbody.HitboxPixelCollider.UnitCenter + base.specRigidbody.Position.Remainder.Quantize(0.0625f / Pixelator.Instance.ScaleTileScale);
		}
	}

	public bool IsCapableOfStealing
	{
		get
		{
			return m_capableOfStealing.Value;
		}
	}

	public int KillsThisRun
	{
		get
		{
			return m_enemiesKilled;
		}
	}

	public override Gun CurrentGun
	{
		get
		{
			if (inventory == null)
			{
				return null;
			}
			if (IsGhost)
			{
				return null;
			}
			return inventory.CurrentGun;
		}
	}

	public Gun CurrentSecondaryGun
	{
		get
		{
			if (inventory == null)
			{
				return null;
			}
			if (!inventory.DualWielding)
			{
				return null;
			}
			if (IsGhost)
			{
				return null;
			}
			return inventory.CurrentSecondaryGun;
		}
	}

	public PlayerItem CurrentItem
	{
		get
		{
			if (m_selectedItemIndex <= 0 || m_selectedItemIndex >= activeItems.Count)
			{
				m_selectedItemIndex = 0;
			}
			if (activeItems.Count > 0)
			{
				return activeItems[m_selectedItemIndex];
			}
			return null;
		}
	}

	public override Transform GunPivot
	{
		get
		{
			return gunAttachPoint;
		}
	}

	public override Transform SecondaryGunPivot
	{
		get
		{
			if (secondaryGunAttachPoint == null)
			{
				GameObject gameObject = new GameObject("secondary attach point");
				secondaryGunAttachPoint = gameObject.transform;
				secondaryGunAttachPoint.parent = gunAttachPoint.parent;
				secondaryGunAttachPoint.localPosition = gunAttachPoint.localPosition;
			}
			return secondaryGunAttachPoint;
		}
	}

	public override Vector3 SpriteDimensions
	{
		get
		{
			return m_spriteDimensions;
		}
	}

	public override bool IsFlying
	{
		get
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				return false;
			}
			return m_isFlying.Value || IsGhost;
		}
	}

	public Vector3 LockedApproximateSpriteCenter
	{
		get
		{
			return base.CenterPosition;
		}
	}

	public Vector3 SpriteBottomCenter
	{
		get
		{
			return base.sprite.transform.position.WithX(base.sprite.transform.position.x + ((!base.sprite.FlipX) ? (m_spriteDimensions.x / 2f) : (-1f * m_spriteDimensions.x / 2f)));
		}
	}

	public override bool SpriteFlipped
	{
		get
		{
			return base.sprite.FlipX;
		}
	}

	public bool BossKillingMode { get; set; }

	public bool CanReturnTeleport
	{
		get
		{
			return m_returnTeleporter != null;
		}
	}

	public bool ReceivesTouchDamage
	{
		get
		{
			if (PassiveItem.ActiveFlagItems.ContainsKey(this))
			{
				Dictionary<Type, int> dictionary = PassiveItem.ActiveFlagItems[this];
				if (dictionary.ContainsKey(typeof(LiveAmmoItem)) || dictionary.ContainsKey(typeof(SpikedArmorItem)) || dictionary.ContainsKey(typeof(HelmetItem)))
				{
					return false;
				}
			}
			return m_additionalReceivesTouchDamage;
		}
		set
		{
			m_additionalReceivesTouchDamage = value;
		}
	}

	protected bool m_CanAttack
	{
		get
		{
			return (!IsDodgeRolling || IsSlidingOverSurface) && !IsGunLocked && CurrentStoneGunTimer <= 0f;
		}
	}

	public bool WasTalkingThisFrame
	{
		get
		{
			return m_wasTalkingThisFrame;
		}
	}

	public bool IgnoredByCamera { get; private set; }

	public bool IsDodgeRolling
	{
		get
		{
			return m_dodgeRollState != DodgeRollState.None && m_dodgeRollState != DodgeRollState.AdditionalDelay;
		}
	}

	public bool IsInMinecart
	{
		get
		{
			return currentMineCart;
		}
	}

	public bool IsInCombat
	{
		get
		{
			if (CurrentRoom == null)
			{
				return false;
			}
			return CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear);
		}
	}

	public bool CanBeGrabbed
	{
		get
		{
			return base.healthHaver.IsVulnerable && !base.IsFalling && !IsGhost && !IsEthereal;
		}
	}

	public bool IsThief { get; set; }

	public RoomHandler CurrentRoom
	{
		get
		{
			return m_currentRoom;
		}
	}

	public bool IsVisible
	{
		get
		{
			return m_isVisible;
		}
		set
		{
			if (value != m_isVisible)
			{
				m_isVisible = value;
				ToggleRenderer(m_isVisible, "isVisible");
				ToggleGunRenderers(m_isVisible, "isVisible");
				ToggleHandRenderers(m_isVisible, "isVisible");
			}
		}
	}

	public bool CanDetectHiddenEnemies
	{
		get
		{
			return (bool)CurrentGun && (bool)CurrentGun.GetComponent<PredatorGunController>();
		}
	}

	private IAutoAimTarget SuperAutoAimTarget { get; set; }

	private IAutoAimTarget SuperDuperAimTarget { get; set; }

	private Vector2 SuperDuperAimPoint { get; set; }

	public float BulletScaleModifier
	{
		get
		{
			return stats.GetStatValue(PlayerStats.StatType.PlayerBulletScale);
		}
	}

	public bool SuppressThisClick { get; set; }

	public bool InExitCell { get; set; }

	public CellData CurrentExitCell { get; set; }

	public Vector2 AverageVelocity { get; set; }

	public bool ArmorlessAnimations
	{
		get
		{
			return hasArmorlessAnimations && !GameManager.Instance.IsFoyer;
		}
		set
		{
			hasArmorlessAnimations = value;
		}
	}

	public bool UseArmorlessAnim
	{
		get
		{
			return ArmorlessAnimations && base.healthHaver.Armor == 0f && OverrideAnimationLibrary == null;
		}
	}

	public float AdditionalChestSpawnChance { get; set; }

	public PlayerInputState CurrentInputState
	{
		get
		{
			if (m_disableInput.Value)
			{
				return PlayerInputState.NoInput;
			}
			if (m_inputState == PlayerInputState.AllInput && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				return PlayerInputState.FoyerInputOnly;
			}
			if (m_inputState == PlayerInputState.AllInput && GameManager.Instance.IsFoyer)
			{
				return PlayerInputState.FoyerInputOnly;
			}
			return m_inputState;
		}
		set
		{
			m_inputState = value;
		}
	}

	public bool AcceptingAnyInput
	{
		get
		{
			return CurrentInputState != PlayerInputState.NoInput;
		}
	}

	public bool AcceptingNonMotionInput
	{
		get
		{
			return (CurrentInputState == PlayerInputState.AllInput || CurrentInputState == PlayerInputState.NoMovement) && !GameManager.Instance.PreventPausing;
		}
	}

	public bool IsInputOverridden
	{
		get
		{
			return m_disableInput.Value;
		}
	}

	public DodgeRollState CurrentRollState
	{
		get
		{
			return m_dodgeRollState;
		}
	}

	public bool IsSlidingOverSurface
	{
		get
		{
			return m_isSlidingOverSurface;
		}
		set
		{
			m_isSlidingOverSurface = value;
		}
	}

	private bool RenderBodyHand
	{
		get
		{
			return !ForceHandless && CurrentSecondaryGun == null && (CurrentGun == null || CurrentGun.Handedness != GunHandedness.TwoHanded);
		}
	}

	public bool IsFiring { get; set; }

	public bool ForceRefreshInteractable { get; set; }

	public bool HighAccuracyAimMode
	{
		get
		{
			return m_highAccuracyAimMode;
		}
		set
		{
			if (m_highAccuracyAimMode != value)
			{
				m_previousAimVector = Vector2.zero;
			}
			m_highAccuracyAimMode = value;
		}
	}

	public bool HasTakenDamageThisRun { get; set; }

	public bool HasTakenDamageThisFloor { get; set; }

	public bool HasReceivedNewGunThisFloor { get; set; }

	public bool HasFiredNonStartingGun { get; set; }

	public int MasteryTokensCollectedThisRun
	{
		get
		{
			return m_masteryTokensCollectedThisRun;
		}
		set
		{
			m_masteryTokensCollectedThisRun = value;
			if (m_masteryTokensCollectedThisRun >= 5)
			{
				GameManager.Instance.platformInterface.AchievementUnlock(Achievement.COLLECT_FIVE_MASTERY_TOKENS);
			}
		}
	}

	public string OverridePlayerSwitchState
	{
		get
		{
			return m_overridePlayerSwitchState;
		}
		set
		{
			m_overridePlayerSwitchState = value;
			AkSoundEngine.SetSwitch("CHR_Player", (m_overridePlayerSwitchState == null) ? characterIdentity.ToString() : m_overridePlayerSwitchState, base.gameObject);
		}
	}

	protected override float DustUpMultiplier
	{
		get
		{
			return stats.MovementSpeed / m_startingMovementSpeed;
		}
	}

	public bool IsPrimaryPlayer
	{
		get
		{
			return PlayerIDX == 0;
		}
	}

	public static string DefaultShaderName
	{
		get
		{
			if (!GameOptions.SupportsStencil)
			{
				return "Brave/PlayerShaderNoStencil";
			}
			return "Brave/PlayerShader";
		}
	}

	public string LocalShaderName
	{
		get
		{
			if (!GameOptions.SupportsStencil)
			{
				return "Brave/PlayerShaderNoStencil";
			}
			if (characterIdentity == PlayableCharacters.Eevee || IsTemporaryEeveeForUnlock)
			{
				return "Brave/PlayerShaderEevee";
			}
			return "Brave/PlayerShader";
		}
	}

	protected bool UseFakeSemiAutoCooldown
	{
		get
		{
			return true;
		}
	}

	protected virtual bool CanDodgeRollWhileFlying
	{
		get
		{
			if (AdditionalCanDodgeRollWhileFlying.Value)
			{
				return true;
			}
			return PassiveItem.ActiveFlagItems.ContainsKey(this) && PassiveItem.ActiveFlagItems[this].ContainsKey(typeof(WingsItem));
		}
	}

	public virtual bool DodgeRollIsBlink
	{
		get
		{
			if ((bool)GameManager.Instance.Dungeon && GameManager.Instance.Dungeon.IsEndTimes)
			{
				return false;
			}
			return PassiveItem.ActiveFlagItems.ContainsKey(this) && PassiveItem.ActiveFlagItems[this].ContainsKey(typeof(BlinkPassiveItem));
		}
	}

	private bool KeepChargingDuringRoll
	{
		get
		{
			return IsDodgeRolling && CurrentGun != null && CurrentGun.HasChargedProjectileReady;
		}
	}

	public Vector2 LastCommandedDirection
	{
		get
		{
			return m_playerCommandedDirection;
		}
	}

	public Vector2 NonZeroLastCommandedDirection
	{
		get
		{
			return (!(m_playerCommandedDirection != Vector2.zero)) ? m_lastNonzeroCommandedDirection : m_playerCommandedDirection;
		}
	}

	public bool IsPetting
	{
		get
		{
			return m_pettingTarget != null;
		}
	}

	public event Action OnPitfall;

	public event Action<Projectile, float> PostProcessProjectile;

	public event Action<BeamController> PostProcessBeam;

	public event Action<BeamController, SpeculativeRigidbody, float> PostProcessBeamTick;

	public event Action<BeamController> PostProcessBeamChanceTick;

	public event Action<Projectile> PostProcessThrownGun;

	public event Action<PlayerController, float> OnDealtDamage;

	public event Action<PlayerController, float, bool, HealthHaver> OnDealtDamageContext;

	public event Action<PlayerController> OnKilledEnemy;

	public event Action<PlayerController, HealthHaver> OnKilledEnemyContext;

	public event Action<PlayerController, PlayerItem> OnUsedPlayerItem;

	public event Action<PlayerController> OnTriedToInitiateAttack;

	public event Action<PlayerController, int> OnUsedBlank;

	public event Action<Gun, Gun, bool> GunChanged;

	public event Action<PlayerController> OnDidUnstealthyAction;

	public event Action<PlayerController> OnPreDodgeRoll;

	public event Action<PlayerController, Vector2> OnRollStarted;

	public event Action<PlayerController> OnIsRolling;

	public event Action<PlayerController, AIActor> OnRolledIntoEnemy;

	public event Action<Projectile> OnDodgedProjectile;

	public event Action<BeamController, PlayerController> OnDodgedBeam;

	public event Action<PlayerController> OnReceivedDamage;

	public event Action<PlayerController, ShopItemController> OnItemPurchased;

	public event Action<PlayerController, ShopItemController> OnItemStolen;

	public event Action<PlayerController> OnRoomClearEvent;

	public void SetTemporaryEeveeSafeNoShader(bool value)
	{
		m_isTemporaryEeveeForUnlock = value;
	}

	public void IncreaseFire(float amount)
	{
		if (!base.SuppressEffectUpdates)
		{
			CurrentFireMeterValue += amount * base.healthHaver.GetDamageModifierForType(CoreDamageTypes.Fire);
		}
	}

	public void IncreasePoison(float amount)
	{
		if (!base.SuppressEffectUpdates && !IsGhost && (!base.healthHaver || base.healthHaver.IsVulnerable))
		{
			CurrentPoisonMeterValue += amount * base.healthHaver.GetDamageModifierForType(CoreDamageTypes.Poison);
		}
	}

	public void SetCapableOfStealing(bool value, string reason, float? duration = null)
	{
		m_capableOfStealing.SetOverride(reason, value, duration);
		ForceRefreshInteractable = true;
	}

	public void SetInputOverride(string reason)
	{
		m_disableInput.AddOverride(reason);
	}

	public void ClearInputOverride(string reason)
	{
		m_disableInput.RemoveOverride(reason);
	}

	public void ClearAllInputOverrides()
	{
		m_disableInput.ClearOverrides();
		CurrentInputState = PlayerInputState.AllInput;
	}

	public IPlayerInteractable GetLastInteractable()
	{
		return m_lastInteractionTarget;
	}

	public bool IsCachedLeapInteractable(IPlayerInteractable ixable)
	{
		return m_leapInteractables.Contains(ixable);
	}

	protected override bool QueryGroundedFrame()
	{
		if (IsDodgeRolling && DodgeRollIsBlink && m_dodgeRollTimer < 5f / 9f * rollStats.GetModifiedTime(this))
		{
			return false;
		}
		if (IsDodgeRolling && m_dodgeRollTimer < 5f / 9f * rollStats.GetModifiedTime(this))
		{
			return false;
		}
		return base.spriteAnimator.QueryGroundedFrame();
	}

	public override void Awake()
	{
		base.Awake();
		m_overrideFlatColorID = Shader.PropertyToID("_FlatColor");
		m_specialFlagsID = Shader.PropertyToID("_SpecialFlags");
		m_stencilID = Shader.PropertyToID("_StencilVal");
		m_blooper = GetComponentInChildren<CoinBloop>();
		Transform transform = base.transform.Find("PlayerSprite");
		base.sprite = ((!(transform != null)) ? null : transform.GetComponent<tk2dSprite>());
		if (base.sprite == null)
		{
			base.sprite = base.transform.Find("PlayerRotatePoint").Find("PlayerSprite").GetComponent<tk2dSprite>();
		}
		m_renderer = base.sprite.GetComponent<MeshRenderer>();
		base.spriteAnimator = base.sprite.GetComponent<tk2dSpriteAnimator>();
		PlayerStats playerStats = base.gameObject.AddComponent<PlayerStats>();
		playerStats.CopyFrom(stats);
		stats = playerStats;
		stats.RecalculateStats(this, true);
		if (characterIdentity == PlayableCharacters.Eevee)
		{
			m_usesRandomStartingEquipment = true;
		}
		if ((bool)GameManager.Instance && (bool)GameManager.Instance.PrimaryPlayer)
		{
			if (characterIdentity == PlayableCharacters.CoopCultist && GameManager.Instance.PrimaryPlayer.characterIdentity == PlayableCharacters.Eevee)
			{
				m_usesRandomStartingEquipment = true;
			}
		}
		else if (characterIdentity == PlayableCharacters.CoopCultist)
		{
			PlayerController[] array = UnityEngine.Object.FindObjectsOfType<PlayerController>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].characterIdentity == PlayableCharacters.Eevee)
				{
					m_usesRandomStartingEquipment = true;
					break;
				}
			}
		}
		if (m_usesRandomStartingEquipment)
		{
			if ((bool)GameManager.Instance && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				GameStatsManager.Instance.CurrentEeveeEquipSeed = -1;
			}
			if (GameStatsManager.Instance.CurrentEeveeEquipSeed < 0)
			{
				GameStatsManager.Instance.CurrentEeveeEquipSeed = UnityEngine.Random.Range(1, 10000000);
			}
			m_randomStartingEquipmentSeed = GameStatsManager.Instance.CurrentEeveeEquipSeed;
			SetUpRandomStartingEquipment();
		}
	}

	public PickupObject.ItemQuality GetQualityFromChances(System.Random r, float dChance, float cChance, float bChance, float aChance, float sChance)
	{
		float num = (dChance + cChance + bChance + aChance + sChance) * (float)r.NextDouble();
		if (num < dChance)
		{
			return PickupObject.ItemQuality.D;
		}
		if (num < dChance + cChance)
		{
			return PickupObject.ItemQuality.C;
		}
		if (num < dChance + cChance + bChance)
		{
			return PickupObject.ItemQuality.B;
		}
		if (num < dChance + cChance + bChance + aChance)
		{
			return PickupObject.ItemQuality.A;
		}
		return PickupObject.ItemQuality.S;
	}

	private void SetUpRandomStartingEquipment()
	{
		startingGunIds.Clear();
		startingAlternateGunIds.Clear();
		startingPassiveItemIds.Clear();
		startingActiveItemIds.Clear();
		finalFightGunIds.Clear();
		System.Random random = new System.Random(m_randomStartingEquipmentSeed);
		PickupObject.ItemQuality qualityFromChances = GetQualityFromChances(random, randomStartingEquipmentSettings.D_CHANCE, randomStartingEquipmentSettings.C_CHANCE, randomStartingEquipmentSettings.B_CHANCE, randomStartingEquipmentSettings.A_CHANCE, randomStartingEquipmentSettings.S_CHANCE);
		PickupObject.ItemQuality qualityFromChances2 = GetQualityFromChances(random, randomStartingEquipmentSettings.D_CHANCE, randomStartingEquipmentSettings.C_CHANCE, randomStartingEquipmentSettings.B_CHANCE, randomStartingEquipmentSettings.A_CHANCE, randomStartingEquipmentSettings.S_CHANCE);
		Gun randomStartingGun = PickupObjectDatabase.GetRandomStartingGun(random);
		List<int> list = new List<int>(randomStartingEquipmentSettings.ExcludedPickups);
		list.Add(GlobalItemIds.Blasphemy);
		Gun randomGunOfQualities = PickupObjectDatabase.GetRandomGunOfQualities(random, list, qualityFromChances);
		PassiveItem randomPassiveOfQualities = PickupObjectDatabase.GetRandomPassiveOfQualities(random, randomStartingEquipmentSettings.ExcludedPickups, qualityFromChances2);
		startingGunIds.Add(randomStartingGun.PickupObjectId);
		if ((bool)randomGunOfQualities)
		{
			startingGunIds.Add(randomGunOfQualities.PickupObjectId);
		}
		if ((bool)randomPassiveOfQualities)
		{
			startingPassiveItemIds.Add(randomPassiveOfQualities.PickupObjectId);
		}
		if ((bool)randomGunOfQualities)
		{
			finalFightGunIds.Add(randomGunOfQualities.PickupObjectId);
		}
	}

	public override void Start()
	{
		base.Start();
		if (PassiveItem.ActiveFlagItems == null)
		{
			PassiveItem.ActiveFlagItems = new Dictionary<PlayerController, Dictionary<Type, int>>();
		}
		if (!PassiveItem.ActiveFlagItems.ContainsKey(this))
		{
			PassiveItem.ActiveFlagItems.Add(this, new Dictionary<Type, int>());
		}
		m_allowMoveAsAim = GameManager.Options.autofaceMovementDirection;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		AkSoundEngine.SetSwitch("CHR_Player", (m_overridePlayerSwitchState == null) ? characterIdentity.ToString() : m_overridePlayerSwitchState, base.gameObject);
		if (IsPrimaryPlayer)
		{
			AkAudioListener component = GetComponent<AkAudioListener>();
			if ((bool)component)
			{
				UnityEngine.Object.Destroy(component);
			}
			GameObject gameObject = new GameObject("listener");
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = (base.specRigidbody.UnitBottomCenter - base.transform.position.XY()).ToVector3ZUp(5f);
			AkAudioListener orAddComponent = gameObject.GetOrAddComponent<AkAudioListener>();
			orAddComponent.listenerId = ((!IsPrimaryPlayer) ? 1 : 0);
		}
		ActorName = "Player ID 0";
		base.spriteAnimator.AnimationCompleted = AnimationCompleteDelegate;
		m_spriteDimensions = base.sprite.GetUntrimmedBounds().size;
		m_startingAttachPointPosition = gunAttachPoint.localPosition;
		gunAttachPoint.localPosition = BraveUtility.QuantizeVector(gunAttachPoint.localPosition, 16f);
		stats.RecalculateStats(this);
		m_startingMovementSpeed = stats.MovementSpeed;
		Blanks = ((GameManager.Instance.CurrentGameType != 0) ? stats.NumBlanksPerFloorCoop : stats.NumBlanksPerFloor);
		InitializeInventory();
		InitializeCallbacks();
		if (HasShadow)
		{
			GameObject gameObject2 = GenerateDefaultBlobShadow();
			base.sprite.AttachRenderer(gameObject2.GetComponent<tk2dSprite>());
		}
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, outlineColor, 0.1f, 0f, (characterIdentity == PlayableCharacters.Eevee) ? SpriteOutlineManager.OutlineType.EEVEE : SpriteOutlineManager.OutlineType.NORMAL);
		OnGunChanged(null, CurrentGun, null, null, true);
		base.gameObject.AddComponent<AkGameObj>();
		if (!GameStatsManager.Instance.IsInSession)
		{
			GameStatsManager.Instance.BeginNewSession(this);
		}
		tk2dSpriteAttachPoint tk2dSpriteAttachPoint2 = base.sprite.GetComponent<tk2dSpriteAttachPoint>();
		if (tk2dSpriteAttachPoint2 == null)
		{
			tk2dSpriteAttachPoint2 = base.sprite.gameObject.AddComponent<tk2dSpriteAttachPoint>();
		}
		if (tk2dSpriteAttachPoint2.GetAttachPointByName("jetpack") == null)
		{
			tk2dSpriteAttachPoint2.ForceAddAttachPoint("jetpack");
		}
		tk2dSpriteAttachPoint2.centerUnusedAttachPoints = true;
		if (IsPrimaryPlayer)
		{
			carriedConsumables.Initialize();
			for (int i = 0; i < passiveItems.Count; i++)
			{
				if ((bool)passiveItems[i] && passiveItems[i] is BriefcaseFullOfCashItem)
				{
					carriedConsumables.Currency += (passiveItems[i] as BriefcaseFullOfCashItem).CurrencyAmount;
				}
			}
		}
		else
		{
			carriedConsumables = GameManager.Instance.PrimaryPlayer.carriedConsumables;
			lootModData = GameManager.Instance.PrimaryPlayer.lootModData;
		}
		unadjustedAimPoint = LockedApproximateSpriteCenter + new Vector3(5f, 0f);
		if ((bool)primaryHand)
		{
			primaryHand.InitializeWithPlayer(this, true);
		}
		if ((bool)secondaryHand)
		{
			secondaryHand.InitializeWithPlayer(this, false);
		}
		ProcessHandAttachment();
		base.sprite.usesOverrideMaterial = true;
		base.sprite.renderer.material.SetFloat("_Perpendicular", base.sprite.renderer.material.GetFloat("_Perpendicular"));
		if (characterIdentity == PlayableCharacters.Pilot || characterIdentity == PlayableCharacters.Robot || characterIdentity == PlayableCharacters.Guide)
		{
			base.sprite.renderer.material.SetFloat("_PlayerGhostAdjustFactor", 4f);
		}
		else
		{
			base.sprite.renderer.material.SetFloat("_PlayerGhostAdjustFactor", 3f);
		}
		base.healthHaver.RegisterBodySprite(primaryHand.sprite);
		base.healthHaver.RegisterBodySprite(secondaryHand.sprite);
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			GameManager.Instance.FrameDelayedEnteredFoyer(this);
		}
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
		if (GameUIRoot.Instance != null)
		{
			GameUIRoot.Instance.UpdatePlayerConsumables(carriedConsumables);
		}
		if ((GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER || characterIdentity == PlayableCharacters.Eevee) && (bool)base.sprite && (bool)base.sprite.renderer && !(this is PlayerSpaceshipController))
		{
			base.sprite.renderer.material.shader = ShaderCache.Acquire(LocalShaderName);
		}
	}

	private void Instance_OnNewLevelFullyLoaded()
	{
		GameManager.Instance.OnNewLevelFullyLoaded -= Instance_OnNewLevelFullyLoaded;
		StartCoroutine(FrameDelayedInitialDeath());
	}

	public void DieOnMidgameLoad()
	{
		StartCoroutine(FrameDelayedInitialDeath(true));
	}

	public static bool AnyoneHasActiveBonusSynergy(CustomSynergyType synergy, out int count)
	{
		count = 0;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if (!GameManager.Instance.AllPlayers[i].IsGhost)
			{
				count += GameManager.Instance.AllPlayers[i].CountActiveBonusSynergies(synergy);
			}
		}
		return count > 0;
	}

	public int CountActiveBonusSynergies(CustomSynergyType synergy)
	{
		if (stats != null)
		{
			int num = 0;
			for (int i = 0; i < stats.ActiveCustomSynergies.Count; i++)
			{
				if (stats.ActiveCustomSynergies[i] == synergy)
				{
					num++;
				}
			}
			for (int j = 0; j < CustomEventSynergies.Count; j++)
			{
				if (CustomEventSynergies[j] == synergy)
				{
					num++;
				}
			}
			return num;
		}
		return 0;
	}

	public bool HasActiveBonusSynergy(CustomSynergyType synergy, bool recursive = false)
	{
		if (CustomEventSynergies.Contains(synergy))
		{
			return true;
		}
		if (stats != null)
		{
			return stats.ActiveCustomSynergies.Contains(synergy);
		}
		return false;
	}

	private IEnumerator FrameDelayedInitialDeath(bool delayTilPostGeneration = false)
	{
		if (delayTilPostGeneration)
		{
			while (Dungeon.IsGenerating)
			{
				yield return null;
			}
		}
		yield return null;
		if (!delayTilPostGeneration)
		{
			m_isFalling = true;
		}
		base.healthHaver.ForceSetCurrentHealth(0f);
		StartCoroutine(HandleCoopDeath(true));
	}

	protected void HandlePostDodgeRollTimer()
	{
		if (m_postDodgeRollGunTimer > 0f)
		{
			m_postDodgeRollGunTimer -= BraveTime.DeltaTime;
			if (m_postDodgeRollGunTimer <= 0f)
			{
				ToggleGunRenderers(true, "postdodgeroll");
				ToggleHandRenderers(true, "postdodgeroll");
			}
		}
	}

	private IEnumerator DestroyEnemyBulletsInCircleForDuration(Vector2 center, float radius, float duration)
	{
		float ela = 0f;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			SilencerInstance.DestroyBulletsInRange(center, radius, true, false);
			yield return null;
		}
	}

	protected void EndBlinkDodge()
	{
		IsEthereal = false;
		IsVisible = true;
		m_dodgeRollState = DodgeRollState.AdditionalDelay;
		if (IsPrimaryPlayer)
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerOnePosition = false;
		}
		else
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerTwoPosition = false;
		}
		WarpToPoint(m_cachedBlinkPosition + (base.transform.position.XY() - base.specRigidbody.UnitCenter));
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		StartCoroutine(DestroyEnemyBulletsInCircleForDuration(base.specRigidbody.UnitCenter, 2f, 0.05f));
		previousMineCart = null;
		ClearBlinkShadow();
	}

	private void ClearDodgeRollState()
	{
		m_dodgeRollState = DodgeRollState.None;
		m_currentDodgeRollDepth = 0;
		m_leapInteractables.Clear();
	}

	public override void Update()
	{
		base.Update();
		if (GameManager.Instance.IsPaused || GameManager.Instance.UnpausedThisFrame || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		m_interactedThisFrame = false;
		if (IsPetting && (!base.spriteAnimator.IsPlaying("pet") || !m_pettingTarget || m_pettingTarget.m_pettingDoer != this || Vector2.Distance(base.specRigidbody.UnitCenter, m_pettingTarget.specRigidbody.UnitCenter) > 3f || IsDodgeRolling))
		{
			ToggleGunRenderers(true, "petting");
			ToggleHandRenderers(true, "petting");
			m_pettingTarget = null;
		}
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D9)
		{
			dx9counter += GameManager.INVARIANT_DELTA_TIME;
			if (dx9counter > 5f)
			{
				dx9counter = 0f;
				tk2dSprite[] componentsInChildren = GetComponentsInChildren<tk2dSprite>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].ForceBuild();
				}
			}
			if (Input.GetKeyDown(KeyCode.F8))
			{
				tk2dBaseSprite[] array = UnityEngine.Object.FindObjectsOfType<tk2dBaseSprite>();
				for (int j = 0; j < array.Length; j++)
				{
					if ((bool)array[j])
					{
						array[j].ForceBuild();
					}
				}
				ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
				for (int k = 0; k < allProjectiles.Count; k++)
				{
					Projectile projectile = allProjectiles[k];
					if ((bool)projectile && (bool)projectile.sprite)
					{
						projectile.sprite.ForceBuild();
					}
				}
			}
		}
		if (base.healthHaver.IsDead && !IsGhost)
		{
			return;
		}
		if (CharacterUsesRandomGuns && inventory != null)
		{
			while (inventory.AllGuns.Count > 1)
			{
				inventory.DestroyGun(inventory.AllGuns[0]);
			}
		}
		HandlePostDodgeRollTimer();
		m_activeActions = BraveInput.GetInstanceForPlayer(PlayerIDX).ActiveActions;
		if ((!AcceptingNonMotionInput || CurrentStoneGunTimer > 0f) && CurrentGun != null && CurrentGun.IsFiring && (!CurrentGun.IsCharging || (CurrentInputState != PlayerInputState.OnlyMovement && !GameManager.IsBossIntro)))
		{
			CurrentGun.CeaseAttack(false);
			if ((bool)CurrentSecondaryGun)
			{
				CurrentSecondaryGun.CeaseAttack(false);
			}
		}
		if (inventory != null)
		{
			inventory.FrameUpdate();
		}
		Projectile.UpdateEnemyBulletSpeedMultiplier();
		float num = Mathf.Clamp01(BraveTime.DeltaTime / 0.5f);
		if (num > 0f && num < 1f)
		{
			Vector2 value = AverageVelocity * (1f - num) + base.specRigidbody.Velocity * num;
			AverageVelocity = BraveMathCollege.ClampSafe(value, -20f, 20f);
		}
		if (m_isFalling)
		{
			return;
		}
		if ((IsDodgeRolling || m_dodgeRollState == DodgeRollState.AdditionalDelay) && m_dodgeRollTimer >= rollStats.GetModifiedTime(this))
		{
			if (DodgeRollIsBlink)
			{
				if (m_dodgeRollTimer > rollStats.GetModifiedTime(this) + 0.1f)
				{
					IsEthereal = false;
					IsVisible = true;
					ClearDodgeRollState();
					previousMineCart = null;
				}
				else if (m_dodgeRollTimer > rollStats.GetModifiedTime(this))
				{
					EndBlinkDodge();
				}
			}
			else
			{
				ClearDodgeRollState();
				previousMineCart = null;
			}
		}
		if (IsDodgeRolling && this.OnIsRolling != null)
		{
			this.OnIsRolling(this);
		}
		CellVisualData.CellFloorType cellFloorType = CellVisualData.CellFloorType.Stone;
		cellFloorType = GameManager.Instance.Dungeon.GetFloorTypeFromPosition(base.specRigidbody.UnitBottomCenter);
		if (!m_prevFloorType.HasValue || m_prevFloorType.Value != cellFloorType)
		{
			m_prevFloorType = cellFloorType;
			AkSoundEngine.SetSwitch("FS_Surfaces", cellFloorType.ToString(), base.gameObject);
		}
		m_playerCommandedDirection = Vector2.zero;
		IsFiring = false;
		if (!BraveUtility.isLoadingLevel && !GameManager.Instance.IsLoadingLevel)
		{
			ProcessDebugInput();
			if (GameUIRoot.Instance.MetalGearActive)
			{
				if (m_activeActions.GunDownAction.WasPressed || m_activeActions.GunUpAction.WasPressed)
				{
					m_gunChangePressedWhileMetalGeared = true;
				}
			}
			else
			{
				m_gunChangePressedWhileMetalGeared = false;
			}
			if (AcceptingAnyInput)
			{
				try
				{
					m_playerCommandedDirection = HandlePlayerInput();
				}
				catch (Exception ex)
				{
					throw new Exception(string.Format("Caught PlayerController.HandlePlayerInput() exception. i={0}, ex={1}", exceptionTracker, ex.ToString()));
				}
			}
			if (m_newFloorNoInput && m_playerCommandedDirection.magnitude > 0f)
			{
				m_newFloorNoInput = false;
			}
			if (usingForcedInput)
			{
				m_playerCommandedDirection = forcedInput;
			}
			if (m_playerCommandedDirection != Vector2.zero)
			{
				GameManager.Instance.platformInterface.ProcessDlcUnlocks();
			}
		}
		if (IsDodgeRolling || m_dodgeRollState == DodgeRollState.AdditionalDelay)
		{
			HandleContinueDodgeRoll();
		}
		if (PassiveItem.IsFlagSetForCharacter(this, typeof(HeavyBootsItem)))
		{
			knockbackComponent = Vector2.zero;
		}
		if (IsDodgeRolling)
		{
			if (usingForcedInput)
			{
				base.specRigidbody.Velocity = forcedInput.normalized * GetDodgeRollSpeed() + knockbackComponent + immutableKnockbackComponent;
			}
			else if (DodgeRollIsBlink)
			{
				base.specRigidbody.Velocity = Vector2.zero;
			}
			else
			{
				base.specRigidbody.Velocity = lockedDodgeRollDirection.normalized * GetDodgeRollSpeed() + knockbackComponent + immutableKnockbackComponent;
			}
		}
		else
		{
			float num2 = 1f;
			if (!IsInCombat && GameManager.Options.IncreaseSpeedOutOfCombat)
			{
				bool flag = true;
				List<AIActor> allEnemies = StaticReferenceManager.AllEnemies;
				if (allEnemies != null)
				{
					for (int l = 0; l < allEnemies.Count; l++)
					{
						AIActor aIActor = allEnemies[l];
						if ((bool)aIActor && aIActor.IsMimicEnemy && !aIActor.IsGone)
						{
							float num3 = Vector2.Distance(aIActor.CenterPosition, base.CenterPosition);
							if (num3 < 40f)
							{
								flag = false;
								break;
							}
						}
					}
				}
				if (flag)
				{
					num2 *= 1.5f;
				}
			}
			Vector2 voluntaryVel = m_playerCommandedDirection * stats.MovementSpeed * num2;
			Vector2 involuntaryVel = knockbackComponent;
			base.specRigidbody.Velocity = ApplyMovementModifiers(voluntaryVel, involuntaryVel) + immutableKnockbackComponent;
		}
		base.specRigidbody.Velocity += ImpartedVelocity;
		ImpartedVelocity = Vector2.zero;
		if (cellFloorType == CellVisualData.CellFloorType.Ice && !IsFlying && !PassiveItem.IsFlagSetForCharacter(this, typeof(HeavyBootsItem)))
		{
			m_maxIceFactor = Mathf.Clamp01(m_maxIceFactor + BraveTime.DeltaTime * 4f);
		}
		else if (IsFlying && !PassiveItem.IsFlagSetForCharacter(this, typeof(HeavyBootsItem)))
		{
			m_maxIceFactor = 0f;
		}
		else
		{
			m_maxIceFactor = Mathf.Clamp01(m_maxIceFactor - BraveTime.DeltaTime * 1.5f);
		}
		if (m_maxIceFactor > 0f)
		{
			float max = Mathf.Max(m_lastVelocity.magnitude, base.specRigidbody.Velocity.magnitude);
			float t = 1f - Mathf.Clamp01(Mathf.Abs(Vector2.Angle(m_lastVelocity, base.specRigidbody.Velocity)) / 180f);
			float num4 = Mathf.Lerp(1f / BraveTime.DeltaTime, Mathf.Lerp(0.5f, 1.5f, t), m_maxIceFactor);
			if (m_lastVelocity.magnitude < 0.25f)
			{
				num4 = Mathf.Min(1f / BraveTime.DeltaTime, Mathf.Max(num4 * (1f / (30f * BraveTime.DeltaTime)), num4));
			}
			base.specRigidbody.Velocity = Vector2.Lerp(m_lastVelocity, base.specRigidbody.Velocity, num4 * BraveTime.DeltaTime);
			base.specRigidbody.Velocity = base.specRigidbody.Velocity.normalized * Mathf.Clamp(base.specRigidbody.Velocity.magnitude, 0f, max);
			if (float.IsNaN(base.specRigidbody.Velocity.x) || float.IsNaN(base.specRigidbody.Velocity.y))
			{
				base.specRigidbody.Velocity = Vector2.zero;
				Debug.Log(string.Concat(m_lastVelocity, "|", m_lastVelocity.magnitude, "| NaN correction"));
			}
			if (base.specRigidbody.Velocity.magnitude < c_iceVelocityMinClamp)
			{
				base.specRigidbody.Velocity = Vector2.zero;
			}
		}
		if (ZeroVelocityThisFrame)
		{
			base.specRigidbody.Velocity = Vector2.zero;
			ZeroVelocityThisFrame = false;
		}
		HandleFlipping(m_currentGunAngle);
		HandleAnimations(m_playerCommandedDirection, m_currentGunAngle);
		if (!IsPrimaryPlayer)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
			if ((bool)otherPlayer)
			{
				float num5 = -0.55f;
				float heightOffGround = base.sprite.HeightOffGround;
				float z = otherPlayer.sprite.transform.position.z;
				float z2 = base.sprite.transform.position.z;
				if (z == z2)
				{
					if (heightOffGround == num5)
					{
						base.sprite.HeightOffGround = num5 + 0.1f;
					}
					else if (heightOffGround == num5 + 0.1f)
					{
						base.sprite.HeightOffGround = num5;
					}
					base.sprite.UpdateZDepth();
				}
			}
		}
		if (IsSlidingOverSurface)
		{
			if (base.sprite.HeightOffGround < 0f)
			{
				base.sprite.HeightOffGround = 1.5f;
			}
		}
		else if (base.sprite.HeightOffGround > 0f)
		{
			base.sprite.HeightOffGround = ((!IsPrimaryPlayer) ? (-0.55f) : (-0.5f));
		}
		HandleAttachedSpriteDepth(m_currentGunAngle);
		HandleShellCasingDisplacement();
		HandlePitChecks();
		HandleRoomProcessing();
		HandleGunAttachPoint();
		CheckSpawnEmergencyCrate();
		CheckSpawnAlertArrows();
		bool flag2 = QueryGroundedFrame() && !IsFlying;
		if (!m_cachedGrounded && flag2 && !m_isFalling && IsVisible)
		{
			GameManager.Instance.Dungeon.dungeonDustups.InstantiateLandDustup(base.specRigidbody.UnitCenter);
		}
		m_cachedGrounded = flag2;
		if (m_playerCommandedDirection != Vector2.zero)
		{
			m_lastNonzeroCommandedDirection = m_playerCommandedDirection;
		}
		base.transform.position = base.transform.position.WithZ(base.transform.position.y - base.sprite.HeightOffGround);
		if (CurrentGun != null)
		{
			CurrentGun.transform.position = CurrentGun.transform.position.WithZ(gunAttachPoint.position.z);
		}
		if (CurrentSecondaryGun != null && (bool)SecondaryGunPivot)
		{
			CurrentSecondaryGun.transform.position = CurrentSecondaryGun.transform.position.WithZ(SecondaryGunPivot.position.z);
		}
		if (m_capableOfStealing.UpdateTimers(BraveTime.DeltaTime))
		{
			ForceRefreshInteractable = true;
		}
		if (m_superDuperAutoAimTimer > 0f)
		{
			m_superDuperAutoAimTimer = Mathf.Max(0f, m_superDuperAutoAimTimer - BraveTime.DeltaTime);
		}
	}

	private void UpdatePlayerShadowPosition()
	{
		GameObject gameObject = GenerateDefaultBlobShadow();
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localPosition = new Vector3(SpriteBottomCenter.x - base.transform.position.x, 0f, 0.1f);
		gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
	}

	public void SwapToAlternateCostume(tk2dSpriteAnimation overrideTargetLibrary = null)
	{
		if (AlternateCostumeLibrary == null && overrideTargetLibrary == null)
		{
			return;
		}
		if (BaseAnimationLibrary != null)
		{
			ResetOverrideAnimationLibrary();
		}
		tk2dSpriteAnimation library = base.spriteAnimator.Library;
		base.spriteAnimator.Library = AlternateCostumeLibrary;
		AlternateCostumeLibrary = library;
		base.spriteAnimator.StopAndResetFrame();
		if (base.spriteAnimator.CurrentClip != null)
		{
			base.spriteAnimator.Play(base.spriteAnimator.CurrentClip.name);
		}
		IsUsingAlternateCostume = !IsUsingAlternateCostume;
		if (HandsOnAltCostume)
		{
			ForceHandless = !IsUsingAlternateCostume;
		}
		if (SwapHandsOnAltCostume)
		{
			RevertHandsToBaseType();
			tk2dSpriteCollectionData newCollection = base.sprite.Collection;
			tk2dSpriteAnimationClip clipByName = base.spriteAnimator.Library.GetClipByName(GetBaseAnimationName(Vector2.zero, 0f));
			if (clipByName != null && clipByName.frames != null && clipByName.frames.Length > 0)
			{
				newCollection = clipByName.frames[0].spriteCollection;
			}
			string spriteName = altHandName;
			if ((bool)primaryHand)
			{
				altHandName = primaryHand.sprite.GetCurrentSpriteDef().name;
				primaryHand.sprite.SetSprite(newCollection, spriteName);
			}
			if ((bool)secondaryHand)
			{
				secondaryHand.sprite.SetSprite(newCollection, spriteName);
			}
		}
		if ((bool)lostAllArmorVFX && (bool)lostAllArmorAltVfx)
		{
			GameObject gameObject = lostAllArmorVFX;
			lostAllArmorVFX = lostAllArmorAltVfx;
			lostAllArmorAltVfx = gameObject;
		}
		m_spriteDimensions = base.sprite.GetUntrimmedBounds().size;
		UpdatePlayerShadowPosition();
	}

	public void RevertHandsToBaseType()
	{
		if (m_usingCustomHandType)
		{
			m_usingCustomHandType = false;
			if ((bool)primaryHand)
			{
				primaryHand.sprite.SetSprite(m_baseHandCollection, m_baseHandId);
			}
			if ((bool)secondaryHand)
			{
				secondaryHand.sprite.SetSprite(m_baseHandCollection, m_baseHandId);
			}
			m_baseHandCollection = null;
		}
	}

	public void ChangeHandsToCustomType(tk2dSpriteCollectionData handCollection, int handId)
	{
		if (!m_usingCustomHandType)
		{
			m_baseHandId = primaryHand.sprite.spriteId;
			m_baseHandCollection = primaryHand.sprite.Collection;
		}
		m_usingCustomHandType = true;
		if ((bool)primaryHand)
		{
			primaryHand.sprite.SetSprite(handCollection, handId);
		}
		if ((bool)secondaryHand)
		{
			secondaryHand.sprite.SetSprite(handCollection, handId);
		}
	}

	private void ResetOverrideAnimationLibrary()
	{
		if (BaseAnimationLibrary != null && base.spriteAnimator.Library != BaseAnimationLibrary)
		{
			base.spriteAnimator.Library = BaseAnimationLibrary;
			base.spriteAnimator.StopAndResetFrame();
			base.spriteAnimator.Play(base.spriteAnimator.CurrentClip.name);
			BaseAnimationLibrary = null;
		}
	}

	private void UpdateTurboModeStats()
	{
		if (GameManager.IsTurboMode)
		{
			if (m_turboSpeedModifier == null)
			{
				m_turboSpeedModifier = StatModifier.Create(PlayerStats.StatType.MovementSpeed, StatModifier.ModifyMethod.MULTIPLICATIVE, TurboModeController.sPlayerSpeedMultiplier);
				m_turboSpeedModifier.ignoredForSaveData = true;
				ownerlessStatModifiers.Add(m_turboSpeedModifier);
			}
			if (m_turboRollSpeedModifier == null)
			{
				m_turboRollSpeedModifier = StatModifier.Create(PlayerStats.StatType.DodgeRollSpeedMultiplier, StatModifier.ModifyMethod.MULTIPLICATIVE, TurboModeController.sPlayerRollSpeedMultiplier);
				m_turboRollSpeedModifier.ignoredForSaveData = true;
				ownerlessStatModifiers.Add(m_turboRollSpeedModifier);
			}
			if (IsPrimaryPlayer)
			{
				if (m_turboEnemyBulletModifier == null)
				{
					m_turboEnemyBulletModifier = StatModifier.Create(PlayerStats.StatType.EnemyProjectileSpeedMultiplier, StatModifier.ModifyMethod.MULTIPLICATIVE, TurboModeController.sEnemyBulletSpeedMultiplier);
					m_turboEnemyBulletModifier.ignoredForSaveData = true;
					ownerlessStatModifiers.Add(m_turboEnemyBulletModifier);
					stats.RecalculateStats(this);
				}
			}
			else if (m_turboEnemyBulletModifier != null)
			{
				ownerlessStatModifiers.Remove(m_turboEnemyBulletModifier);
				m_turboEnemyBulletModifier = null;
				stats.RecalculateStats(this);
			}
			if ((m_turboEnemyBulletModifier != null && m_turboEnemyBulletModifier.amount != TurboModeController.sEnemyBulletSpeedMultiplier) || m_turboSpeedModifier.amount != TurboModeController.sPlayerSpeedMultiplier || m_turboRollSpeedModifier.amount != TurboModeController.sPlayerRollSpeedMultiplier)
			{
				m_turboRollSpeedModifier.amount = TurboModeController.sPlayerRollSpeedMultiplier;
				m_turboSpeedModifier.amount = TurboModeController.sPlayerSpeedMultiplier;
				m_turboEnemyBulletModifier.amount = TurboModeController.sEnemyBulletSpeedMultiplier;
				stats.RecalculateStats(this);
			}
		}
		else if (m_turboSpeedModifier != null || m_turboEnemyBulletModifier != null || m_turboRollSpeedModifier != null)
		{
			ownerlessStatModifiers.Remove(m_turboEnemyBulletModifier);
			m_turboEnemyBulletModifier = null;
			ownerlessStatModifiers.Remove(m_turboSpeedModifier);
			m_turboSpeedModifier = null;
			ownerlessStatModifiers.Remove(m_turboRollSpeedModifier);
			m_turboRollSpeedModifier = null;
			stats.RecalculateStats(this);
		}
	}

	private void LateUpdate()
	{
		UpdateTurboModeStats();
		WasPausedThisFrame = false;
		if (!m_handleDodgeRollStartThisFrame)
		{
			m_timeHeldBlinkButton = 0f;
		}
		if (DeferredStatRecalculationRequired)
		{
			stats.RecalculateStatsInternal(this);
		}
		m_wasTalkingThisFrame = IsTalking;
		m_lastVelocity = base.specRigidbody.Velocity;
		if (IsPrimaryPlayer && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER && !m_newFloorNoInput && !GameManager.Instance.IsPaused && !Dungeon.IsGenerating && !GameManager.Instance.IsLoadingLevel)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIME_PLAYED, Time.unscaledDeltaTime);
		}
		if (GameManager.Options.RealtimeReflections)
		{
			base.sprite.renderer.sharedMaterial.SetFloat("_ReflectionYOffset", actorReflectionAdditionalOffset);
		}
		if (GameManager.Instance.IsPaused || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if (CurrentRoom == null)
		{
			m_isInCombat = false;
		}
		else
		{
			bool isInCombat = m_isInCombat;
			m_isInCombat = CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear);
			if (OnEnteredCombat != null && m_isInCombat && !isInCombat)
			{
				OnEnteredCombat();
			}
		}
		if (!IsPrimaryPlayer && CharacterUsesRandomGuns != GameManager.Instance.GetOtherPlayer(this).CharacterUsesRandomGuns)
		{
			CharacterUsesRandomGuns = GameManager.Instance.GetOtherPlayer(this).CharacterUsesRandomGuns;
		}
		UpdateStencilVal();
		if (CharacterUsesRandomGuns)
		{
			m_gunGameElapsed += BraveTime.DeltaTime;
			if (CurrentGun != null && CurrentGun.CurrentAmmo == 0)
			{
				ChangeToRandomGun();
			}
			else if (CurrentRoom != null && m_gunGameElapsed > 20f && CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && IsInCombat)
			{
				ChangeToRandomGun();
			}
			else if (CurrentGun == null && !IsGhost && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES && !GameManager.Instance.IsLoadingLevel)
			{
				Debug.Log("Changing to random gun because we don't have any gun at all!");
				ChangeToRandomGun();
			}
		}
		if ((bool)base.specRigidbody)
		{
			float magnitude = (base.specRigidbody.Velocity * BraveTime.DeltaTime).magnitude;
			if (IsFlying)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.DISTANCE_FLOWN, magnitude);
			}
			else
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.DISTANCE_WALKED, magnitude);
			}
		}
		if (characterIdentity != PlayableCharacters.Eevee)
		{
			if (OverrideAnimationLibrary != null)
			{
				if (base.spriteAnimator.Library != OverrideAnimationLibrary)
				{
					BaseAnimationLibrary = base.spriteAnimator.Library;
					base.spriteAnimator.Library = OverrideAnimationLibrary;
					base.spriteAnimator.StopAndResetFrame();
					base.spriteAnimator.Play(base.spriteAnimator.CurrentClip.name);
				}
			}
			else if (BaseAnimationLibrary != null && base.spriteAnimator.Library != BaseAnimationLibrary)
			{
				ResetOverrideAnimationLibrary();
			}
		}
		CurrentFloorDamageCooldown = Mathf.Max(0f, CurrentFloorDamageCooldown - BraveTime.DeltaTime);
		if (m_blankCooldownTimer > 0f)
		{
			m_blankCooldownTimer = Mathf.Max(0f, m_blankCooldownTimer - BraveTime.DeltaTime);
			if (IsGhost && m_blankCooldownTimer <= 0f)
			{
				DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
			}
		}
		if (m_highStressTimer > 0f)
		{
			m_highStressTimer -= BraveTime.DeltaTime;
			if (m_highStressTimer <= 0f && (bool)base.healthHaver)
			{
				base.healthHaver.NextShotKills = false;
			}
		}
		if (!IsGhost)
		{
			DeregisterOverrideColor("player status effects");
			Color a = new Color(0f, 0f, 0f, 0f);
			Color targetColor = baseFlatColorOverride;
			float a2 = 0.25f + Mathf.PingPong(Time.timeSinceLevelLoad, 0.25f);
			GameUIRoot.Instance.SetAmmoCountColor(Color.white, this);
			if (CurrentDrainMeterValue > 0f)
			{
				if (m_currentGoop == null || !m_currentGoop.DrainsAmmo || !QueryGroundedFrame())
				{
					CurrentDrainMeterValue = Mathf.Max(0f, CurrentDrainMeterValue - BraveTime.DeltaTime * 0.1f);
				}
				GameUIRoot.Instance.SetAmmoCountColor(Color.Lerp(Color.white, new Color(1f, 0f, 0f, 1f), CurrentDrainMeterValue), this);
				if (CurrentDrainMeterValue >= 1f)
				{
					GameUIRoot.Instance.SetAmmoCountColor(new Color(1f, 0f, 0f, 1f), this);
				}
			}
			else
			{
				inventory.ClearAmmoDrain();
			}
			a = Color.Lerp(a, new Color(0.65f, 0f, 0.6f, a2), CurrentDrainMeterValue);
			if (IsOnFire && base.healthHaver.GetDamageModifierForType(CoreDamageTypes.Fire) > 0f && !IsEthereal && !IsTalking && !HasActiveBonusSynergy(CustomSynergyType.FIRE_IMMUNITY))
			{
				if (!IsDodgeRolling)
				{
					IncreaseFire(BraveTime.DeltaTime * 0.666666f);
				}
				else
				{
					IncreaseFire(BraveTime.DeltaTime * 0.2f);
				}
				if (CurrentFireMeterValue >= 1f)
				{
					CurrentFireMeterValue -= 1f;
					if (!m_isFalling)
					{
						base.healthHaver.ApplyDamage(0.5f, Vector2.zero, StringTableManager.GetEnemiesString("#FIRE"), CoreDamageTypes.Fire, DamageCategory.Environment, true);
					}
					int num = 12;
					Vector3 minPosition = base.specRigidbody.HitboxPixelCollider.UnitBottomLeft.ToVector3ZisY();
					Vector3 maxPosition = base.specRigidbody.HitboxPixelCollider.UnitTopRight.ToVector3ZisY();
					float angleVariance = 15f;
					float baseMagnitude = 2.25f;
					float magnitudeVariance = 1f;
					Color? startColor = Color.red;
					GlobalSparksDoer.DoRadialParticleBurst(num, minPosition, maxPosition, angleVariance, baseMagnitude, magnitudeVariance, null, null, startColor);
				}
				targetColor = new Color(1f, 0f, 0f, 0.7f);
			}
			else
			{
				CurrentFireMeterValue = 0f;
				IsOnFire = false;
			}
			if (CurrentPoisonMeterValue > 0f && base.healthHaver.GetDamageModifierForType(CoreDamageTypes.Poison) > 0f)
			{
				if (m_currentGoop == null || !m_currentGoop.damagesPlayers || !QueryGroundedFrame())
				{
					CurrentPoisonMeterValue = Mathf.Max(0f, CurrentPoisonMeterValue - BraveTime.DeltaTime * 0.5f);
				}
			}
			else
			{
				CurrentPoisonMeterValue = 0f;
			}
			a = Color.Lerp(a, new Color(0f, 1f, 0f, a2), CurrentPoisonMeterValue);
			if (CurrentCurseMeterValue > 0f && CurseIsDecaying)
			{
				CurrentCurseMeterValue = Mathf.Max(0f, CurrentCurseMeterValue - BraveTime.DeltaTime * 0.5f);
			}
			a = Color.Lerp(a, new Color(0f, 0f, 0f, a2), CurrentCurseMeterValue);
			if (CurrentStoneGunTimer > 0f)
			{
				CurrentStoneGunTimer -= BraveTime.DeltaTime;
				targetColor = new Color(0.4f, 0.4f, 0.33f, Mathf.Clamp01(CurrentStoneGunTimer / 0.25f));
			}
			RegisterOverrideColor(a, "player status effects");
			if (!FlatColorOverridden)
			{
				ChangeFlatColorOverride(targetColor);
			}
			GameUIRoot.Instance.UpdatePlayerHealthUI(this, base.healthHaver);
			GameUIRoot.Instance.UpdatePlayerBlankUI(this);
			if (GameUIRoot.Instance != null)
			{
				GameUIRoot.Instance.UpdateGunData(inventory, m_equippedGunShift, this);
				GameUIRoot.Instance.UpdateItemData(this, CurrentItem, activeItems);
				GameUIRoot.Instance.GetReloadBarForPlayer(this).UpdateStatusBars(this);
				for (int i = 0; i < activeItems.Count; i++)
				{
					if (activeItems[i] == null || !activeItems[i])
					{
						Debug.Log("We have encountered a null item at item index: " + i);
					}
				}
				if (CurrentItem == null)
				{
					m_selectedItemIndex = 0;
				}
			}
		}
		else
		{
			GameUIRoot.Instance.UpdateGhostUI(this);
			ToggleHandRenderers(false, "ghostliness");
			IsOnFire = false;
			CurrentPoisonMeterValue = 0f;
			CurrentFireMeterValue = 0f;
			CurrentDrainMeterValue = 0f;
			CurrentCurseMeterValue = 0f;
			float t = Mathf.Clamp01(m_blankCooldownTimer / 5f);
			ChangeFlatColorOverride(Color.Lerp(m_ghostChargedColor, m_ghostUnchargedColor, t));
			if (CurrentInputState != PlayerInputState.NoInput && !GameManager.Instance.MainCameraController.ManualControl)
			{
				if (!GameManager.Instance.MainCameraController.PointIsVisible(base.CenterPosition, 0.05f))
				{
					PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
					IntVector2? intVector = null;
					if ((bool)otherPlayer && otherPlayer.CurrentRoom != null)
					{
						CellValidator cellValidator = (IntVector2 p) => GameManager.Instance.MainCameraController.PointIsVisible(p.ToCenterVector2());
						Vector2 nearbyPoint = BraveMathCollege.ClosestPointOnRectangle(base.CenterPosition, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint - GameManager.Instance.MainCameraController.MinVisiblePoint);
						intVector = otherPlayer.CurrentRoom.GetNearestAvailableCell(nearbyPoint, IntVector2.One * 3, CellTypes.FLOOR | CellTypes.PIT, false, cellValidator);
					}
					if (intVector.HasValue)
					{
						LootEngine.DoDefaultPurplePoof(base.CenterPosition, true);
						WarpToPoint(intVector.Value.ToVector2() + Vector2.one);
						LootEngine.DoDefaultPurplePoof(base.CenterPosition, true);
					}
					else
					{
						LootEngine.DoDefaultPurplePoof(base.CenterPosition, true);
						ReuniteWithOtherPlayer(GameManager.Instance.GetOtherPlayer(this));
						LootEngine.DoDefaultPurplePoof(base.CenterPosition, true);
					}
				}
				else if (!GameManager.Instance.MainCameraController.PointIsVisible(base.CenterPosition, 0f))
				{
					Vector2 vector = BraveMathCollege.ClosestPointOnRectangle(base.CenterPosition, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint - GameManager.Instance.MainCameraController.MinVisiblePoint);
					Vector2 vector2 = vector - base.CenterPosition;
					IntVector2 impartedPixelsToMove = (vector2 * 16f).ToIntVector2();
					base.specRigidbody.ImpartedPixelsToMove = impartedPixelsToMove;
				}
			}
		}
		if (Minimap.Instance != null)
		{
			Minimap.Instance.UpdatePlayerPositionExact(base.transform.position, this);
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			HandleCoopSpecificTimers();
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			GameUIRoot.Instance.UpdatePlayerConsumables(carriedConsumables);
		}
	}

	private void SetStencilVal(int v)
	{
		if ((bool)base.sprite && (bool)base.sprite.renderer)
		{
			base.sprite.renderer.material.SetInt(m_stencilID, v);
		}
	}

	private void UpdateStencilVal()
	{
		if ((bool)base.sprite && (bool)base.sprite.renderer)
		{
			int @int = base.sprite.renderer.material.GetInt(m_stencilID);
			if (@int != 147 && @int != 146)
			{
				SetStencilVal(146);
			}
		}
	}

	public void ChangeSpecialShaderFlag(int flagIndex, float val)
	{
		Vector4 vector = base.healthHaver.bodySprites[0].renderer.material.GetVector(m_specialFlagsID);
		vector[flagIndex] = val;
		for (int i = 0; i < base.healthHaver.bodySprites.Count; i++)
		{
			base.healthHaver.bodySprites[i].usesOverrideMaterial = true;
			base.healthHaver.bodySprites[i].renderer.material.SetColor(m_specialFlagsID, vector);
		}
		if ((bool)primaryHand && (bool)primaryHand.sprite)
		{
			primaryHand.sprite.renderer.material.SetColor(m_specialFlagsID, vector);
		}
		if ((bool)secondaryHand && (bool)secondaryHand.sprite)
		{
			secondaryHand.sprite.renderer.material.SetColor(m_specialFlagsID, vector);
		}
	}

	public void ChangeFlatColorOverride(Color targetColor)
	{
		for (int i = 0; i < base.healthHaver.bodySprites.Count; i++)
		{
			base.healthHaver.bodySprites[i].usesOverrideMaterial = true;
			base.healthHaver.bodySprites[i].renderer.material.SetColor(m_overrideFlatColorID, targetColor);
		}
	}

	public void UpdateRandomStartingEquipmentCoop(bool shouldUseRandom)
	{
		if (shouldUseRandom && !m_usesRandomStartingEquipment)
		{
			m_usesRandomStartingEquipment = true;
			if ((bool)GameManager.Instance && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				GameStatsManager.Instance.CurrentEeveeEquipSeed = -1;
			}
			if (GameStatsManager.Instance.CurrentEeveeEquipSeed < 0)
			{
				GameStatsManager.Instance.CurrentEeveeEquipSeed = UnityEngine.Random.Range(1, 10000000);
			}
			m_randomStartingEquipmentSeed = GameStatsManager.Instance.CurrentEeveeEquipSeed;
			SetUpRandomStartingEquipment();
			m_turboEnemyBulletModifier = null;
			m_turboRollSpeedModifier = null;
			m_turboSpeedModifier = null;
			ResetToFactorySettings(false, false, true);
		}
		else if (!shouldUseRandom && m_usesRandomStartingEquipment)
		{
			m_usesRandomStartingEquipment = false;
			PlayerController component = GameManager.LastUsedCoopPlayerPrefab.GetComponent<PlayerController>();
			startingGunIds = new List<int>(component.startingGunIds);
			startingAlternateGunIds = new List<int>(component.startingAlternateGunIds);
			startingPassiveItemIds = new List<int>(component.startingPassiveItemIds);
			startingActiveItemIds = new List<int>(component.startingActiveItemIds);
			finalFightGunIds = new List<int>(component.finalFightGunIds);
			m_turboEnemyBulletModifier = null;
			m_turboRollSpeedModifier = null;
			m_turboSpeedModifier = null;
			ResetToFactorySettings(false, false, true);
		}
	}

	public void ResetToFactorySettings(bool includeFullHeal = false, bool useFinalFightGuns = false, bool forceAllItems = false)
	{
		if (!IsDarkSoulsHollow || useFinalFightGuns)
		{
			inventory.DestroyAllGuns();
		}
		if (useFinalFightGuns && finalFightGunIds != null && finalFightGunIds.Count > 0)
		{
			for (int i = 0; i < finalFightGunIds.Count; i++)
			{
				if (finalFightGunIds[i] >= 0)
				{
					inventory.AddGunToInventory(PickupObjectDatabase.GetById(finalFightGunIds[i]) as Gun, true);
				}
			}
		}
		else if (UsingAlternateStartingGuns)
		{
			for (int j = 0; j < startingAlternateGunIds.Count; j++)
			{
				Gun gun = PickupObjectDatabase.GetById(startingAlternateGunIds[j]) as Gun;
				if (forceAllItems || includeFullHeal || useFinalFightGuns || gun.PreventStartingOwnerFromDropping)
				{
					Gun gun2 = inventory.AddGunToInventory(gun, true);
				}
			}
		}
		else
		{
			for (int k = 0; k < startingGunIds.Count; k++)
			{
				Gun gun3 = PickupObjectDatabase.GetById(startingGunIds[k]) as Gun;
				if (forceAllItems || includeFullHeal || useFinalFightGuns || gun3.PreventStartingOwnerFromDropping)
				{
					Gun gun4 = inventory.AddGunToInventory(gun3, true);
				}
			}
		}
		for (int l = 0; l < passiveItems.Count; l++)
		{
			if (!passiveItems[l].PersistsOnDeath)
			{
				DebrisObject debrisObject = DropPassiveItem(passiveItems[l]);
				if (debrisObject != null)
				{
					UnityEngine.Object.Destroy(debrisObject.gameObject);
					l--;
				}
			}
		}
		for (int m = 0; m < activeItems.Count; m++)
		{
			if (!activeItems[m].PersistsOnDeath)
			{
				DebrisObject debrisObject2 = DropActiveItem(activeItems[m], 4f, true);
				if (debrisObject2 != null)
				{
					UnityEngine.Object.Destroy(debrisObject2.gameObject);
					m--;
				}
			}
		}
		for (int n = 0; n < startingActiveItemIds.Count; n++)
		{
			PlayerItem playerItem = PickupObjectDatabase.GetById(startingActiveItemIds[n]) as PlayerItem;
			if ((forceAllItems || !playerItem.consumable) && !HasActiveItem(playerItem.PickupObjectId) && (forceAllItems || includeFullHeal || useFinalFightGuns || playerItem.PreventStartingOwnerFromDropping))
			{
				EncounterTrackable.SuppressNextNotification = true;
				playerItem.Pickup(this);
				EncounterTrackable.SuppressNextNotification = false;
			}
		}
		for (int num = 0; num < startingPassiveItemIds.Count; num++)
		{
			PassiveItem passiveItem = PickupObjectDatabase.GetById(startingPassiveItemIds[num]) as PassiveItem;
			if (!HasPassiveItem(passiveItem.PickupObjectId))
			{
				EncounterTrackable.SuppressNextNotification = true;
				LootEngine.GivePrefabToPlayer(passiveItem.gameObject, this);
				EncounterTrackable.SuppressNextNotification = false;
			}
		}
		if (ownerlessStatModifiers != null)
		{
			if (useFinalFightGuns || includeFullHeal)
			{
				ownerlessStatModifiers.Clear();
			}
			else
			{
				for (int num2 = 0; num2 < ownerlessStatModifiers.Count; num2++)
				{
					if (!ownerlessStatModifiers[num2].PersistsOnCoopDeath)
					{
						ownerlessStatModifiers.RemoveAt(num2);
						num2--;
					}
				}
			}
		}
		stats.RecalculateStats(this);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(this).stats.RecalculateStats(GameManager.Instance.GetOtherPlayer(this));
		}
		if (useFinalFightGuns && characterIdentity == PlayableCharacters.Robot)
		{
			base.healthHaver.Armor = 6f;
		}
		if (includeFullHeal)
		{
			base.healthHaver.FullHeal();
		}
	}

	private IEnumerator CoopResurrectInternal(Vector3 targetPosition, tk2dSpriteAnimationClip clipToWaitFor, bool isChest = false)
	{
		GameManager.Instance.MainCameraController.IsLerping = true;
		m_cloneWaitingForCoopDeath = false;
		ForceBlank(5f, 0.5f, true, false, targetPosition.XY(), false);
		if (!isChest)
		{
			IsCurrentlyCoopReviving = true;
			SetInputOverride("revivepause");
			PlayEffectOnActor((GameObject)ResourceCache.Acquire("Global VFX/VFX_GhostRevive"), Vector3.zero, true, true);
			float elapsed = 0f;
			while (elapsed < 0.75f)
			{
				elapsed += BraveTime.DeltaTime;
				yield return null;
			}
			ClearInputOverride("revivepause");
			IsCurrentlyCoopReviving = false;
			GameManager.Instance.MainCameraController.OverrideRecoverySpeed = 7.5f;
			GameManager.Instance.MainCameraController.IsLerping = true;
		}
		ChangeSpecialShaderFlag(0, 0f);
		IsGhost = false;
		base.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
		m_blankCooldownTimer = 0f;
		GameUIRoot.Instance.TransitionToGhostUI(this);
		CurrentInputState = PlayerInputState.NoInput;
		m_cachedAimDirection = -Vector2.up;
		GameUIRoot.Instance.ReenableCoopPlayerUI(this);
		stats.RecalculateStats(this);
		base.transform.position = targetPosition;
		base.specRigidbody.CollideWithTileMap = true;
		base.specRigidbody.CollideWithOthers = true;
		base.specRigidbody.Reinitialize();
		base.healthHaver.FullHeal();
		if (characterIdentity == PlayableCharacters.Robot)
		{
			base.healthHaver.Armor = 6f;
		}
		DoVibration(Vibration.Time.Normal, Vibration.Strength.Medium);
		m_handlingQueuedAnimation = true;
		if (clipToWaitFor != null)
		{
			base.spriteAnimator.Play(clipToWaitFor);
			while (base.spriteAnimator.IsPlaying(clipToWaitFor))
			{
				yield return null;
			}
		}
		m_handlingQueuedAnimation = false;
		IsVisible = true;
		if (!SpriteOutlineManager.HasOutline(base.sprite))
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, outlineColor, 0.1f, 0f, (characterIdentity == PlayableCharacters.Eevee) ? SpriteOutlineManager.OutlineType.EEVEE : SpriteOutlineManager.OutlineType.NORMAL);
		}
		m_hideRenderers.ClearOverrides();
		m_hideGunRenderers.ClearOverrides();
		m_hideHandRenderers.ClearOverrides();
		ToggleShadowVisiblity(true);
		ToggleRenderer(true, string.Empty);
		ToggleRenderer(true, "isVisible");
		ToggleGunRenderers(true, string.Empty);
		ToggleGunRenderers(true, "isVisible");
		ToggleHandRenderers(true, string.Empty);
		ToggleHandRenderers(true, "isVisible");
		List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody);
		for (int i = 0; i < overlappingRigidbodies.Count; i++)
		{
			base.specRigidbody.RegisterGhostCollisionException(overlappingRigidbodies[i]);
			overlappingRigidbodies[i].RegisterGhostCollisionException(base.specRigidbody);
		}
		m_isFalling = false;
		ClearDodgeRollState();
		previousMineCart = null;
		m_interruptingPitRespawn = false;
		GameManager.Instance.GetOtherPlayer(this).stats.RecalculateStats(GameManager.Instance.GetOtherPlayer(this));
		CurrentInputState = PlayerInputState.AllInput;
		base.healthHaver.IsVulnerable = true;
	}

	public virtual void ResurrectFromBossKill()
	{
		PlayerController playerController = ((!(GameManager.Instance.PrimaryPlayer == this)) ? GameManager.Instance.PrimaryPlayer : GameManager.Instance.SecondaryPlayer);
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = null;
		tk2dSpriteAnimationClip2 = ((base.spriteAnimator.GetClipByName("chest_recover") == null) ? base.spriteAnimator.GetClipByName((!UseArmorlessAnim) ? "pitfall_return" : "pitfall_return_armorless") : base.spriteAnimator.GetClipByName((!UseArmorlessAnim) ? "chest_recover" : "chest_recover_armorless"));
		Chest.ToggleCoopChests(false);
		CellData cellData = null;
		IntVector2 key = base.transform.position.IntXY(VectorConversions.Floor);
		cellData = GameManager.Instance.Dungeon.data[key];
		Vector3 targetPosition = base.transform.position;
		if (cellData == null || cellData.type != CellType.FLOOR || cellData.IsPlayerInaccessible)
		{
			targetPosition = playerController.CurrentRoom.GetBestRewardLocation(IntVector2.One, RoomHandler.RewardLocationStyle.PlayerCenter).ToVector3();
		}
		StartCoroutine(CoopResurrectInternal(targetPosition, tk2dSpriteAnimationClip2));
	}

	public void ResurrectFromChest(Vector2 chestBottomCenter)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = null;
		tk2dSpriteAnimationClip2 = ((base.spriteAnimator.GetClipByName("chest_recover") == null) ? base.spriteAnimator.GetClipByName((!UseArmorlessAnim) ? "pitfall_return" : "pitfall_return_armorless") : base.spriteAnimator.GetClipByName((!UseArmorlessAnim) ? "chest_recover" : "chest_recover_armorless"));
		Chest.ToggleCoopChests(false);
		if (confettiPaths == null)
		{
			confettiPaths = new string[3] { "Global VFX/Confetti_Blue_001", "Global VFX/Confetti_Yellow_001", "Global VFX/Confetti_Green_001" };
		}
		Vector2 vector = chestBottomCenter + new Vector2(-0.75f, -0.25f);
		for (int i = 0; i < 8; i++)
		{
			GameObject original = (GameObject)ResourceCache.Acquire(confettiPaths[UnityEngine.Random.Range(0, 3)]);
			WaftingDebrisObject component = UnityEngine.Object.Instantiate(original).GetComponent<WaftingDebrisObject>();
			component.sprite.PlaceAtPositionByAnchor(vector.ToVector3ZUp() + new Vector3(0.5f, 0.5f, 0f), tk2dBaseSprite.Anchor.MiddleCenter);
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			insideUnitCircle.y = 0f - Mathf.Abs(insideUnitCircle.y);
			component.Trigger(insideUnitCircle.ToVector3ZUp(1.5f) * UnityEngine.Random.Range(0.5f, 2f), 0.5f, 0f);
		}
		StartCoroutine(CoopResurrectInternal(vector.ToVector3ZUp(), tk2dSpriteAnimationClip2, true));
	}

	private void HandleCoopSpecificTimers()
	{
		PlayerController playerController = ((!IsPrimaryPlayer) ? GameManager.Instance.PrimaryPlayer : GameManager.Instance.SecondaryPlayer);
		if (playerController != null && !playerController.healthHaver.IsDead && playerController.CurrentRoom != null && playerController.CurrentRoom.IsSealed && playerController.CurrentRoom != CurrentRoom)
		{
			m_coopRoomTimer += BraveTime.DeltaTime;
			if (m_coopRoomTimer > 1f)
			{
				if (IsGhost)
				{
					LootEngine.DoDefaultPurplePoof(base.CenterPosition, true);
				}
				ReuniteWithOtherPlayer(playerController);
				if (IsGhost)
				{
					LootEngine.DoDefaultPurplePoof(base.CenterPosition, true);
				}
				base.healthHaver.TriggerInvulnerabilityPeriod();
				m_coopRoomTimer = 0f;
			}
		}
		else
		{
			m_coopRoomTimer = 0f;
		}
	}

	public void DoPostProcessProjectile(Projectile p)
	{
		p.Owner = this;
		HandleShadowBulletStat(p);
		float arg = 1f;
		if ((bool)CurrentGun && CurrentGun.DefaultModule != null)
		{
			float num = 0f;
			if (CurrentGun.Volley != null)
			{
				List<ProjectileModule> projectiles = CurrentGun.Volley.projectiles;
				for (int i = 0; i < projectiles.Count; i++)
				{
					num += projectiles[i].GetEstimatedShotsPerSecond(CurrentGun.reloadTime);
				}
			}
			else if (CurrentGun.DefaultModule != null)
			{
				num += CurrentGun.DefaultModule.GetEstimatedShotsPerSecond(CurrentGun.reloadTime);
			}
			if (num > 0f)
			{
				arg = 3.5f / num;
			}
		}
		if (this.PostProcessProjectile != null)
		{
			this.PostProcessProjectile(p, arg);
		}
	}

	public void CustomPostProcessProjectile(Projectile p, float effectChanceScalar)
	{
		if (this.PostProcessProjectile != null)
		{
			this.PostProcessProjectile(p, effectChanceScalar);
		}
	}

	public void DoPostProcessThrownGun(Projectile p)
	{
		if (this.PostProcessThrownGun != null)
		{
			this.PostProcessThrownGun(p);
		}
	}

	public void SpawnShadowBullet(Projectile obj, bool shadowColoration)
	{
		float num = 0f;
		if ((bool)obj.sprite && obj.sprite.GetBounds().size.x > 0.5f)
		{
			num += obj.sprite.GetBounds().size.x / 10f;
		}
		num = Mathf.Max(num, 0.1f);
		StartCoroutine(SpawnShadowBullet(obj, num, shadowColoration));
	}

	protected IEnumerator SpawnShadowBullet(Projectile obj, float additionalDelay, bool shadowColoration)
	{
		Vector3 cachedSpawnPosition = obj.transform.position;
		Quaternion cachedSpawnRotation = obj.transform.rotation;
		if (additionalDelay > 0f)
		{
			float ela = 0f;
			while (ela < additionalDelay)
			{
				ela += BraveTime.DeltaTime;
				yield return null;
			}
		}
		if (!obj)
		{
			yield break;
		}
		bool flag = false;
		if (HasActiveBonusSynergy(CustomSynergyType.MR_SHADOW) && (bool)CurrentGun && CurrentGun.DefaultModule.usesOptionalFinalProjectile)
		{
			Projectile projectile = CurrentGun.DefaultModule.finalProjectile;
			if (CurrentGun.DefaultModule.finalVolley != null)
			{
				projectile = CurrentGun.DefaultModule.finalVolley.projectiles[0].GetCurrentProjectile();
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(projectile.gameObject, cachedSpawnPosition, cachedSpawnRotation);
			gameObject.transform.position += gameObject.transform.right * -0.5f;
			Projectile component = gameObject.GetComponent<Projectile>();
			component.specRigidbody.Reinitialize();
			component.collidesWithPlayer = false;
			component.Owner = obj.Owner;
			component.Shooter = obj.Shooter;
			if (shadowColoration)
			{
				component.ChangeColor(0f, new Color(0.35f, 0.25f, 0.65f, 1f));
			}
			flag = true;
		}
		if (!flag)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(obj.gameObject, cachedSpawnPosition, cachedSpawnRotation);
			gameObject2.transform.position += gameObject2.transform.right * -0.5f;
			Projectile component2 = gameObject2.GetComponent<Projectile>();
			component2.specRigidbody.Reinitialize();
			component2.collidesWithPlayer = false;
			component2.Owner = obj.Owner;
			component2.Shooter = obj.Shooter;
			component2.baseData.damage = obj.baseData.damage;
			component2.baseData.range = obj.baseData.range;
			component2.baseData.speed = obj.baseData.speed;
			component2.baseData.force = obj.baseData.force;
			if (shadowColoration)
			{
				component2.ChangeColor(0f, new Color(0.35f, 0.25f, 0.65f, 1f));
			}
		}
	}

	protected void HandleShadowBulletStat(Projectile obj)
	{
		float num = stats.GetStatValue(PlayerStats.StatType.ExtremeShadowBulletChance) / 100f;
		if (UnityEngine.Random.value < num)
		{
			StartCoroutine(SpawnShadowBullet(obj, 0.05f, false));
			if (!(UnityEngine.Random.value < 0.5f))
			{
				return;
			}
			StartCoroutine(SpawnShadowBullet(obj, 0.1f, false));
			if (UnityEngine.Random.value < 0.5f)
			{
				StartCoroutine(SpawnShadowBullet(obj, 0.15f, false));
				if (UnityEngine.Random.value < 0.5f)
				{
					StartCoroutine(SpawnShadowBullet(obj, 0.2f, false));
				}
			}
		}
		else
		{
			float num2 = stats.GetStatValue(PlayerStats.StatType.ShadowBulletChance) / 100f;
			if (UnityEngine.Random.value < num2)
			{
				SpawnShadowBullet(obj, true);
			}
		}
	}

	public void DoPostProcessBeam(BeamController beam)
	{
		int num = Mathf.FloorToInt(stats.GetStatValue(PlayerStats.StatType.AdditionalShotBounces));
		int num2 = Mathf.FloorToInt(stats.GetStatValue(PlayerStats.StatType.AdditionalShotPiercing));
		if ((num > 0 || num2 > 0) && beam is BasicBeamController)
		{
			BasicBeamController basicBeamController = beam as BasicBeamController;
			if (!basicBeamController.playerStatsModified)
			{
				basicBeamController.penetration += num2;
				basicBeamController.reflections += num;
				basicBeamController.playerStatsModified = true;
			}
		}
		if (this.PostProcessBeam != null)
		{
			this.PostProcessBeam(beam);
		}
	}

	public void DoPostProcessBeamTick(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickRate)
	{
		if ((!beam || !beam.projectile || beam.projectile.baseData.damage != 0f) && this.PostProcessBeamTick != null)
		{
			this.PostProcessBeamTick(beam, hitRigidbody, tickRate);
		}
	}

	public void DoPostProcessBeamChanceTick(BeamController beam)
	{
		if (this.PostProcessBeamChanceTick != null)
		{
			this.PostProcessBeamChanceTick(beam);
		}
	}

	public Material[] SetOverrideShader(Shader overrideShader)
	{
		if (m_cachedOverrideMaterials == null)
		{
			m_cachedOverrideMaterials = new Material[3];
		}
		for (int i = 0; i < m_cachedOverrideMaterials.Length; i++)
		{
			m_cachedOverrideMaterials[i] = null;
		}
		base.sprite.renderer.material.shader = overrideShader;
		m_cachedOverrideMaterials[0] = base.sprite.renderer.material;
		if ((bool)primaryHand && (bool)primaryHand.sprite)
		{
			m_cachedOverrideMaterials[1] = primaryHand.SetOverrideShader(overrideShader);
		}
		if ((bool)secondaryHand && (bool)secondaryHand.sprite)
		{
			m_cachedOverrideMaterials[2] = secondaryHand.SetOverrideShader(overrideShader);
		}
		return m_cachedOverrideMaterials;
	}

	public void ClearOverrideShader()
	{
		if ((bool)this && (bool)base.sprite && (bool)base.sprite.renderer && (bool)base.sprite.renderer.material)
		{
			base.sprite.renderer.material.shader = ShaderCache.Acquire(LocalShaderName);
		}
		if ((bool)primaryHand && (bool)primaryHand.sprite)
		{
			primaryHand.ClearOverrideShader();
		}
		if ((bool)secondaryHand && (bool)secondaryHand.sprite)
		{
			secondaryHand.ClearOverrideShader();
		}
	}

	public void Reinitialize()
	{
		base.specRigidbody.Reinitialize();
		WarpFollowersToPlayer();
	}

	public void ReinitializeGuns()
	{
		inventory.DestroyAllGuns();
		List<int> list = startingGunIds;
		if (UsingAlternateStartingGuns)
		{
			list = startingAlternateGunIds;
		}
		for (int i = 0; i < list.Count; i++)
		{
			Gun gun = PickupObjectDatabase.GetById(list[i]) as Gun;
			if ((bool)gun.encounterTrackable)
			{
				EncounterTrackable.SuppressNextNotification = true;
				gun.encounterTrackable.HandleEncounter();
				EncounterTrackable.SuppressNextNotification = false;
			}
			Gun gun2 = inventory.AddGunToInventory(gun, true);
		}
		inventory.ChangeGun(1);
	}

	private void InitializeInventory()
	{
		inventory = new GunInventory(this);
		inventory.maxGuns = MAX_GUNS_HELD + (int)stats.GetStatValue(PlayerStats.StatType.AdditionalGunCapacity);
		inventory.maxGuns = int.MaxValue;
		if (CharacterUsesRandomGuns)
		{
			inventory.maxGuns = 1;
		}
		List<int> list = startingGunIds;
		if (UsingAlternateStartingGuns)
		{
			list = startingAlternateGunIds;
		}
		for (int i = 0; i < list.Count; i++)
		{
			Gun gun = PickupObjectDatabase.GetById(list[i]) as Gun;
			if ((bool)gun.encounterTrackable)
			{
				EncounterTrackable.SuppressNextNotification = true;
				gun.encounterTrackable.HandleEncounter();
				EncounterTrackable.SuppressNextNotification = false;
			}
			Gun gun2 = inventory.AddGunToInventory(gun, true);
		}
		inventory.ChangeGun(1);
		if (!m_usesRandomStartingEquipment || GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			for (int j = 0; j < startingPassiveItemIds.Count; j++)
			{
				AcquirePassiveItemPrefabDirectly(PickupObjectDatabase.GetById(startingPassiveItemIds[j]) as PassiveItem);
			}
			for (int k = 0; k < startingActiveItemIds.Count; k++)
			{
				EncounterTrackable.SuppressNextNotification = true;
				PlayerItem playerItem = PickupObjectDatabase.GetById(startingActiveItemIds[k]) as PlayerItem;
				playerItem.Pickup(this);
				EncounterTrackable.SuppressNextNotification = false;
			}
		}
	}

	public DebrisObject ForceDropGun(Gun g)
	{
		if (!g.CanActuallyBeDropped(this))
		{
			return null;
		}
		if (inventory.GunLocked.Value)
		{
			return null;
		}
		bool flag = g == CurrentGun;
		g.HasEverBeenAcquiredByPlayer = true;
		inventory.RemoveGunFromInventory(g);
		g.ToggleRenderers(true);
		DebrisObject result = g.DropGun();
		if (flag)
		{
			ProcessHandAttachment();
		}
		return result;
	}

	public void UpdateInventoryMaxGuns()
	{
		if (inventory != null && inventory.maxGuns <= 1000)
		{
			inventory.maxGuns = MAX_GUNS_HELD + (int)stats.GetStatValue(PlayerStats.StatType.AdditionalGunCapacity);
			inventory.maxGuns = int.MaxValue;
			while (inventory.maxGuns < inventory.GunCountModified)
			{
				Gun currentGun = CurrentGun;
				currentGun.HasEverBeenAcquiredByPlayer = true;
				inventory.RemoveGunFromInventory(currentGun);
				currentGun.DropGun();
			}
		}
	}

	public void UpdateInventoryMaxItems()
	{
		if (activeItems != null)
		{
			maxActiveItemsHeld = MAX_ITEMS_HELD + (int)stats.GetStatValue(PlayerStats.StatType.AdditionalItemCapacity);
			while (maxActiveItemsHeld < activeItems.Count)
			{
				DropActiveItem(activeItems[activeItems.Count - 1]);
			}
		}
	}

	public void ResetTarnisherClipCapacity()
	{
		for (int num = ownerlessStatModifiers.Count - 1; num >= 0; num--)
		{
			if (ownerlessStatModifiers[num].statToBoost == PlayerStats.StatType.TarnisherClipCapacityMultiplier)
			{
				ownerlessStatModifiers.RemoveAt(num);
			}
		}
		stats.RecalculateStats(this);
	}

	public void ChangeAttachedSpriteDepth(tk2dBaseSprite targetSprite, float targetDepth)
	{
		if (m_attachedSprites.Contains(targetSprite))
		{
			int index = m_attachedSprites.IndexOf(targetSprite);
			m_attachedSpriteDepths[index] = targetDepth;
		}
	}

	public GameObject RegisterAttachedObject(GameObject prefab, string attachPoint, float depth = 0f)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		if (!string.IsNullOrEmpty(attachPoint))
		{
			tk2dSpriteAttachPoint orAddComponent = base.sprite.gameObject.GetOrAddComponent<tk2dSpriteAttachPoint>();
			gameObject.transform.parent = orAddComponent.GetAttachPointByName(attachPoint);
		}
		else
		{
			gameObject.transform.parent = base.sprite.transform;
		}
		gameObject.transform.localPosition = Vector3.zero;
		if (gameObject.transform.parent == null)
		{
			Debug.LogError("FAILED TO FIND ATTACHPOINT " + attachPoint + " ON PLAYER");
		}
		tk2dBaseSprite tk2dBaseSprite2 = gameObject.GetComponent<tk2dBaseSprite>();
		if (tk2dBaseSprite2 == null)
		{
			tk2dBaseSprite2 = gameObject.GetComponentInChildren<tk2dBaseSprite>();
		}
		base.sprite.AttachRenderer(tk2dBaseSprite2);
		tk2dBaseSprite[] componentsInChildren = gameObject.GetComponentsInChildren<tk2dBaseSprite>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			m_attachedSprites.Add(componentsInChildren[i]);
			m_attachedSpriteDepths.Add(depth);
		}
		return gameObject;
	}

	public void DeregisterAttachedObject(GameObject instance, bool completeDestruction = true)
	{
		if (!instance)
		{
			return;
		}
		tk2dBaseSprite[] componentsInChildren = instance.GetComponentsInChildren<tk2dBaseSprite>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if ((bool)componentsInChildren[i])
			{
				m_attachedSpriteDepths.RemoveAt(m_attachedSprites.IndexOf(componentsInChildren[i]));
				m_attachedSprites.Remove(componentsInChildren[i]);
			}
		}
		if (completeDestruction)
		{
			UnityEngine.Object.Destroy(instance);
		}
		else
		{
			instance.transform.parent = null;
		}
	}

	public void ForceStaticFaceDirection(Vector2 dir)
	{
		m_lastNonzeroCommandedDirection = dir;
		unadjustedAimPoint = base.CenterPosition + dir.normalized * 5f;
	}

	public void ForceIdleFacePoint(Vector2 dir, bool quadrantize = true)
	{
		float num = ((!quadrantize) ? BraveMathCollege.Atan2Degrees(dir) : ((float)(BraveMathCollege.VectorToQuadrant(dir) * 90)));
		string baseAnimationName = GetBaseAnimationName(Vector2.zero, num);
		if (!base.spriteAnimator.IsPlaying(baseAnimationName))
		{
			base.spriteAnimator.Play(baseAnimationName);
		}
		base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
		m_currentGunAngle = num;
		ForceStaticFaceDirection(dir);
		if ((bool)CurrentGun)
		{
			CurrentGun.HandleAimRotation(base.CenterPosition + dir);
		}
	}

	public void TeleportToPoint(Vector2 targetPoint, bool useDefaultTeleportVFX)
	{
		if (m_isStartingTeleport)
		{
			return;
		}
		m_isStartingTeleport = true;
		GameObject gameObject = null;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
			if ((bool)otherPlayer)
			{
				otherPlayer.TeleportToPoint(targetPoint, useDefaultTeleportVFX);
			}
		}
		m_isStartingTeleport = false;
		if (useDefaultTeleportVFX)
		{
			gameObject = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Teleport_Beam");
		}
		DoVibration(Vibration.Time.Normal, Vibration.Strength.Medium);
		StartCoroutine(HandleTeleportToPoint(targetPoint, gameObject, null, gameObject));
	}

	private IEnumerator HandleTeleportToPoint(Vector2 targetPoint, GameObject departureVFXPrefab, GameObject arrivalVFX1Prefab, GameObject arrivalVFX2Prefab)
	{
		base.healthHaver.IsVulnerable = false;
		CameraController cameraController = GameManager.Instance.MainCameraController;
		Vector2 offsetVector = cameraController.transform.position - base.transform.position;
		offsetVector -= cameraController.GetAimContribution();
		Minimap.Instance.ToggleMinimap(false);
		cameraController.SetManualControl(true, false);
		cameraController.OverridePosition = cameraController.transform.position;
		CurrentInputState = PlayerInputState.NoInput;
		yield return new WaitForSeconds(0.1f);
		ToggleRenderer(false, "arbitrary teleporter");
		ToggleGunRenderers(false, "arbitrary teleporter");
		ToggleHandRenderers(false, "arbitrary teleporter");
		if (departureVFXPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(departureVFXPrefab);
			gameObject.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(base.specRigidbody.UnitBottomCenter + new Vector2(0f, -0.5f), tk2dBaseSprite.Anchor.LowerCenter);
			gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
			gameObject.GetComponent<tk2dBaseSprite>().UpdateZDepth();
		}
		yield return new WaitForSeconds(0.4f);
		Pixelator.Instance.FadeToBlack(0.1f);
		yield return new WaitForSeconds(0.1f);
		base.specRigidbody.Position = new Position(targetPoint);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			cameraController.OverridePosition = cameraController.GetIdealCameraPosition();
		}
		else
		{
			cameraController.OverridePosition = (targetPoint + offsetVector).ToVector3ZUp();
		}
		Pixelator.Instance.MarkOcclusionDirty();
		if (arrivalVFX1Prefab != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(arrivalVFX1Prefab);
			gameObject2.transform.position = targetPoint;
			gameObject2.transform.position = gameObject2.transform.position.Quantize(0.0625f);
		}
		Pixelator.Instance.FadeToBlack(0.1f, true);
		yield return null;
		cameraController.SetManualControl(false);
		yield return new WaitForSeconds(0.75f);
		if (arrivalVFX2Prefab != null)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate(arrivalVFX2Prefab);
			gameObject3.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(base.specRigidbody.UnitBottomCenter + new Vector2(0f, -0.5f), tk2dBaseSprite.Anchor.LowerCenter);
			gameObject3.transform.position = gameObject3.transform.position.Quantize(0.0625f);
			gameObject3.GetComponent<tk2dBaseSprite>().UpdateZDepth();
		}
		DoVibration(Vibration.Time.Normal, Vibration.Strength.Medium);
		yield return new WaitForSeconds(0.25f);
		ToggleRenderer(true, "arbitrary teleporter");
		ToggleGunRenderers(true, "arbitrary teleporter");
		ToggleHandRenderers(true, "arbitrary teleporter");
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		CurrentInputState = PlayerInputState.AllInput;
		base.healthHaver.IsVulnerable = true;
	}

	public bool IsPositionObscuredByTopWall(Vector2 newPosition)
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				int x = newPosition.ToIntVector2(VectorConversions.Floor).x + i;
				int y = newPosition.ToIntVector2(VectorConversions.Floor).y + j;
				if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(newPosition.ToIntVector2(VectorConversions.Floor) + new IntVector2(i, j)) && (GameManager.Instance.Dungeon.data.isTopWall(x, y) || GameManager.Instance.Dungeon.data.isWall(x, y)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsValidPlayerPosition(Vector2 newPosition)
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(newPosition.ToIntVector2(VectorConversions.Floor) + new IntVector2(i, j)))
				{
					return false;
				}
			}
		}
		int value = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider, CollisionLayer.EnemyHitBox, CollisionLayer.Projectile);
		Func<SpeculativeRigidbody, bool> func = (SpeculativeRigidbody rigidbody) => rigidbody.minorBreakable;
		PhysicsEngine instance = PhysicsEngine.Instance;
		SpeculativeRigidbody rigidbody2 = base.specRigidbody;
		List<CollisionData> overlappingCollisions = null;
		bool collideWithTiles = true;
		bool collideWithRigidbodies = true;
		int? overrideCollisionMask = null;
		int? ignoreCollisionMask = value;
		Func<SpeculativeRigidbody, bool> rigidbodyExcluder = func;
		bool flag = instance.OverlapCast(rigidbody2, overlappingCollisions, collideWithTiles, collideWithRigidbodies, overrideCollisionMask, ignoreCollisionMask, false, newPosition, rigidbodyExcluder);
		return !flag;
	}

	public void WarpFollowersToPlayer(bool excludeCompanions = false)
	{
		for (int i = 0; i < orbitals.Count; i++)
		{
			orbitals[i].GetTransform().position = base.transform.position;
			orbitals[i].Reinitialize();
		}
		for (int j = 0; j < trailOrbitals.Count; j++)
		{
			trailOrbitals[j].transform.position = base.transform.position;
			trailOrbitals[j].specRigidbody.Reinitialize();
		}
		if (!excludeCompanions)
		{
			WarpCompanionsToPlayer();
		}
	}

	public void WarpCompanionsToPlayer(bool isRoomSealWarp = false)
	{
		Vector3 vector = base.transform.position;
		if (InExitCell && CurrentRoom != null)
		{
			vector = CurrentRoom.GetBestRewardLocation(new IntVector2(2, 2), RoomHandler.RewardLocationStyle.PlayerCenter, false).ToVector3() + new Vector3(1f, 1f, 0f);
		}
		for (int i = 0; i < companions.Count; i++)
		{
			Vector3 targetPosition = vector;
			if (isRoomSealWarp && companions[i].CompanionSettings.WarpsToRandomPoint)
			{
				IntVector2? randomAvailableCell = CurrentRoom.GetRandomAvailableCell(companions[i].Clearance * 3, CellTypes.FLOOR, false, Pathfinder.CellValidator_NoTopWalls);
				if (randomAvailableCell.HasValue)
				{
					targetPosition = (randomAvailableCell.Value + IntVector2.One).ToVector3();
				}
			}
			companions[i].CompanionWarp(targetPosition);
		}
	}

	public void WarpToPointAndBringCoopPartner(Vector2 targetPoint, bool useDefaultPoof = false, bool doFollowers = false)
	{
		WarpToPoint(targetPoint, useDefaultPoof, doFollowers);
		PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
		if ((bool)otherPlayer)
		{
			Vector2 vector = base.specRigidbody.UnitBottomLeft - base.transform.position.XY();
			Vector2 vector2 = otherPlayer.specRigidbody.UnitBottomLeft - otherPlayer.transform.position.XY();
			otherPlayer.WarpToPoint(targetPoint + (vector - vector2), useDefaultPoof, doFollowers);
		}
	}

	public void WarpToPoint(Vector2 targetPoint, bool useDefaultPoof = false, bool doFollowers = false)
	{
		if (useDefaultPoof)
		{
			LootEngine.DoDefaultItemPoof(base.CenterPosition, true);
		}
		base.transform.position = targetPoint;
		base.specRigidbody.Reinitialize();
		base.specRigidbody.RecheckTriggers = true;
		if ((bool)CurrentItem && CurrentItem is GrapplingHookItem)
		{
			GrapplingHookItem grapplingHookItem = CurrentItem as GrapplingHookItem;
			if ((bool)grapplingHookItem && grapplingHookItem.IsActive)
			{
				float destroyTime = -1f;
				grapplingHookItem.Use(this, out destroyTime);
			}
		}
		if (doFollowers)
		{
			WarpFollowersToPlayer();
		}
	}

	public void AttemptTeleportToRoom(RoomHandler targetRoom, bool force = false, bool noFX = false)
	{
		if (IsInMinecart)
		{
			return;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
			if ((bool)otherPlayer && otherPlayer.IsInMinecart)
			{
				return;
			}
		}
		bool flag = CurrentRoom != null && CurrentRoom.CanTeleportFromRoom() && targetRoom != null && targetRoom.CanTeleportToRoom();
		if (GameManager.Instance.InTutorial && !flag && CurrentRoom == targetRoom && targetRoom.GetRoomName().Equals("Tutorial_Room_0065_teleporter", StringComparison.OrdinalIgnoreCase))
		{
			flag = true;
		}
		if (force)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (this.OnDidUnstealthyAction != null)
		{
			this.OnDidUnstealthyAction(this);
		}
		AkSoundEngine.PostEvent("Play_OBJ_teleport_depart_01", base.gameObject);
		m_cachedTeleportSpot = ((!force) ? base.specRigidbody.Position.UnitPosition : CurrentRoom.GetCenteredVisibleClearSpot(2, 2).ToVector2());
		targetRoom.SetRoomActive(true);
		TeleporterController teleporterController = ((!targetRoom.hierarchyParent) ? null : targetRoom.hierarchyParent.GetComponentInChildren<TeleporterController>(true));
		if (!teleporterController)
		{
			List<TeleporterController> componentsInRoom = targetRoom.GetComponentsInRoom<TeleporterController>();
			if (componentsInRoom.Count > 0)
			{
				teleporterController = componentsInRoom[0];
			}
		}
		Vector2 targetSpot;
		if ((bool)teleporterController)
		{
			targetSpot = teleporterController.sprite.WorldCenter;
		}
		else
		{
			IntVector2? randomAvailableCell = targetRoom.GetRandomAvailableCell(IntVector2.One * 2, CellTypes.FLOOR);
			targetSpot = ((!randomAvailableCell.HasValue) ? targetRoom.GetCenterCell().ToVector2() : randomAvailableCell.Value.ToVector2());
		}
		targetSpot -= SpriteDimensions.XY().WithY(0f) / 2f;
		StartCoroutine(HandleTeleport(teleporterController, targetSpot, false, noFX));
	}

	public void AttemptReturnTeleport(TeleporterController teleporter)
	{
		if (CurrentRoom != null && CurrentRoom.CanTeleportFromRoom() && CanReturnTeleport && teleporter == m_returnTeleporter)
		{
			AkSoundEngine.PostEvent("Play_OBJ_teleport_depart_01", base.gameObject);
			StartCoroutine(HandleTeleport(teleporter, m_cachedTeleportSpot, true));
		}
	}

	private IEnumerator HandleTeleport(TeleporterController teleporter, Vector2 targetSpot, bool isReturnTeleport, bool noFX = false)
	{
		CameraController cameraController = GameManager.Instance.MainCameraController;
		Vector2 offsetVector = cameraController.transform.position - base.transform.position;
		offsetVector -= cameraController.GetAimContribution();
		Minimap.Instance.ToggleMinimap(false);
		cameraController.SetManualControl(true, false);
		cameraController.OverridePosition = cameraController.transform.position;
		CurrentInputState = PlayerInputState.NoInput;
		yield return new WaitForSeconds(0.1f);
		ToggleRenderer(false, "minimap teleporter");
		ToggleGunRenderers(false, "minimap teleporter");
		ToggleHandRenderers(false, "minimap teleporter");
		if (!noFX)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(teleporter.teleportDepartureVFX);
			gameObject.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(base.specRigidbody.UnitBottomCenter + new Vector2(0f, -0.5f), tk2dBaseSprite.Anchor.LowerCenter);
			gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
			gameObject.GetComponent<tk2dBaseSprite>().UpdateZDepth();
		}
		DoVibration(Vibration.Time.Normal, Vibration.Strength.Medium);
		yield return new WaitForSeconds(0.4f);
		Pixelator.Instance.FadeToBlack(0.1f);
		yield return new WaitForSeconds(0.1f);
		if (!cameraController.ManualControl)
		{
			cameraController.SetManualControl(true, false);
		}
		if (offsetVector.magnitude > 15f)
		{
			offsetVector = offsetVector.normalized * 15f;
		}
		base.specRigidbody.Position = new Position(targetSpot);
		base.specRigidbody.RecheckTriggers = true;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			cameraController.OverridePosition = cameraController.GetIdealCameraPosition();
		}
		else
		{
			cameraController.OverridePosition = (targetSpot + offsetVector).ToVector3ZUp();
		}
		Pixelator.Instance.MarkOcclusionDirty();
		GameObject arrivalVFX = UnityEngine.Object.Instantiate(teleporter.teleportArrivalVFX);
		arrivalVFX.transform.position = teleporter.transform.position;
		arrivalVFX.transform.position = arrivalVFX.transform.position.Quantize(0.0625f);
		BraveMemory.HandleTeleportation();
		if (isReturnTeleport)
		{
			RoomHandler absoluteRoom = targetSpot.GetAbsoluteRoom();
			if (absoluteRoom != null && absoluteRoom.visibility != 0 && absoluteRoom.visibility != RoomHandler.VisibilityStatus.REOBSCURED)
			{
				if (m_currentRoom != null)
				{
					m_currentRoom.PlayerExit(this);
				}
				m_currentRoom = absoluteRoom;
				m_currentRoom.PlayerEnter(this);
				EnteredNewRoom(m_currentRoom);
				GameManager.Instance.MainCameraController.AssignBoundingPolygon(m_currentRoom.cameraBoundingPolygon);
			}
		}
		Pixelator.Instance.FadeToBlack(0.1f, true);
		yield return null;
		cameraController.SetManualControl(false);
		yield return new WaitForSeconds(0.75f);
		GameObject arrivalVFX2 = UnityEngine.Object.Instantiate(teleporter.teleportDepartureVFX);
		arrivalVFX2.GetComponent<tk2dBaseSprite>().PlaceAtLocalPositionByAnchor(base.specRigidbody.UnitBottomCenter + new Vector2(0f, -0.5f), tk2dBaseSprite.Anchor.LowerCenter);
		arrivalVFX2.transform.position = arrivalVFX2.transform.position.Quantize(0.0625f);
		arrivalVFX2.GetComponent<tk2dBaseSprite>().UpdateZDepth();
		DoVibration(Vibration.Time.Normal, Vibration.Strength.Medium);
		yield return new WaitForSeconds(0.25f);
		ToggleRenderer(true, "minimap teleporter");
		ToggleGunRenderers(true, "minimap teleporter");
		ToggleHandRenderers(true, "minimap teleporter");
		if (isReturnTeleport)
		{
			teleporter.ClearReturnActive();
			m_returnTeleporter = null;
		}
		else
		{
			if (m_returnTeleporter != null)
			{
				m_returnTeleporter.ClearReturnActive();
			}
			teleporter.SetReturnActive();
			m_returnTeleporter = teleporter;
		}
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		CurrentInputState = PlayerInputState.AllInput;
		WarpFollowersToPlayer();
	}

	protected virtual void CheckSpawnAlertArrows()
	{
		if (!IsPrimaryPlayer)
		{
			return;
		}
		if (GameManager.IsReturningToBreach || GameManager.Instance.IsLoadingLevel || Dungeon.IsGenerating)
		{
			m_elapsedNonalertTime = 0f;
			m_isThreatArrowing = false;
			m_threadArrowTarget = null;
		}
		else if (GameManager.Instance.CurrentLevelOverrideState != 0 || CurrentRoom == null)
		{
			m_elapsedNonalertTime = 0f;
			m_isThreatArrowing = false;
			m_threadArrowTarget = null;
		}
		else
		{
			if (CurrentRoom == null || !IsInCombat)
			{
				return;
			}
			List<AIActor> activeEnemies = CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies == null || activeEnemies.Count == 0)
			{
				m_elapsedNonalertTime = 0f;
				m_isThreatArrowing = false;
				m_threadArrowTarget = null;
				return;
			}
			AIActor aIActor = null;
			bool flag = false;
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				AIActor aIActor2 = activeEnemies[i];
				if ((bool)aIActor2 && (!aIActor2.IgnoreForRoomClear || aIActor2.AlwaysShowOffscreenArrow) && (!aIActor2.healthHaver || !aIActor2.healthHaver.IsBoss))
				{
					if (GameManager.Instance.MainCameraController.PointIsVisible(aIActor2.CenterPosition))
					{
						flag = true;
					}
					else if (!aIActor || (!aIActor.AlwaysShowOffscreenArrow && aIActor2.AlwaysShowOffscreenArrow))
					{
						aIActor = aIActor2;
					}
				}
			}
			if ((bool)aIActor && (!flag || aIActor.AlwaysShowOffscreenArrow))
			{
				m_elapsedNonalertTime += BraveTime.DeltaTime;
				m_threadArrowTarget = aIActor;
				if ((m_elapsedNonalertTime > 3f || aIActor.AlwaysShowOffscreenArrow) && !m_isThreatArrowing)
				{
					StartCoroutine(HandleThreatArrow());
				}
			}
			else
			{
				m_elapsedNonalertTime = 0f;
				m_isThreatArrowing = false;
				m_threadArrowTarget = null;
			}
		}
	}

	protected virtual void CheckSpawnEmergencyCrate()
	{
		if (CurrentRoom == null || CurrentRoom.ExtantEmergencyCrate != null || GameManager.Instance.Dungeon.SuppressEmergencyCrates || (!CurrentRoom.IsSealed && !CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear)) || (!CurrentRoom.area.IsProceduralRoom && CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SECRET) || CharacterUsesRandomGuns)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < inventory.AllGuns.Count; i++)
		{
			if (inventory.AllGuns[i].CurrentAmmo > 0 || inventory.AllGuns[i].InfiniteAmmo)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			SpawnEmergencyCrate();
		}
	}

	public IntVector2 SpawnEmergencyCrate(GenericLootTable overrideTable = null)
	{
		GameObject original = (GameObject)BraveResources.Load("EmergencyCrate");
		GameObject gameObject = UnityEngine.Object.Instantiate(original);
		EmergencyCrateController component = gameObject.GetComponent<EmergencyCrateController>();
		if (overrideTable != null)
		{
			component.gunTable = overrideTable;
		}
		IntVector2 bestRewardLocation = CurrentRoom.GetBestRewardLocation(new IntVector2(1, 1));
		component.Trigger(new Vector3(-5f, -5f, -5f), bestRewardLocation.ToVector3() + new Vector3(15f, 15f, 15f), CurrentRoom, overrideTable == null);
		CurrentRoom.ExtantEmergencyCrate = gameObject;
		return bestRewardLocation;
	}

	public void ReinitializeMovementRestrictors()
	{
		base.specRigidbody.MovementRestrictor = null;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(RollPitMovementRestrictor));
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(CameraBoundsMovementRestrictor));
		}
	}

	private void InitializeCallbacks()
	{
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.OnDeath += Die;
		base.healthHaver.OnDamaged += Damaged;
		base.healthHaver.OnHealthChanged += HealthChanged;
		base.spriteAnimator.AnimationEventTriggered = HandleAnimationEvent;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody2.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(RollPitMovementRestrictor));
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
			speculativeRigidbody3.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(speculativeRigidbody3.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(CameraBoundsMovementRestrictor));
		}
		inventory.OnGunChanged += OnGunChanged;
	}

	private void CameraBoundsMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (validLocation)
		{
			IntVector2 intVector = PhysicsEngine.UnitToPixel(GameManager.Instance.MainCameraController.MinVisiblePoint);
			IntVector2 intVector2 = PhysicsEngine.UnitToPixel(GameManager.Instance.MainCameraController.MaxVisiblePoint);
			if (specRigidbody.PixelColliders[0].LowerLeft.x < intVector.x && pixelOffset.x < prevPixelOffset.x)
			{
				validLocation = false;
			}
			else if (specRigidbody.PixelColliders[0].UpperRight.x > intVector2.x && pixelOffset.x > prevPixelOffset.x)
			{
				validLocation = false;
			}
			else if (specRigidbody.PixelColliders[0].LowerLeft.y < intVector.y && pixelOffset.y < prevPixelOffset.y)
			{
				validLocation = false;
			}
			else if (specRigidbody.PixelColliders[1].UpperRight.y > intVector2.y && pixelOffset.y > prevPixelOffset.y)
			{
				validLocation = false;
			}
			if (!validLocation && StaticReferenceManager.ActiveMineCarts.ContainsKey(this))
			{
				StaticReferenceManager.ActiveMineCarts[this].EvacuateSpecificPlayer(this);
			}
		}
	}

	public void ReuniteWithOtherPlayer(PlayerController other, bool useDefaultVFX = false)
	{
		WarpToPoint(other.transform.position, useDefaultVFX);
	}

	public void HandleItemStolen(ShopItemController item)
	{
		if (this.OnItemStolen != null)
		{
			this.OnItemStolen(this, item);
		}
	}

	public void HandleItemPurchased(ShopItemController item)
	{
		if (this.OnItemPurchased != null)
		{
			this.OnItemPurchased(this, item);
		}
	}

	public void OnRoomCleared()
	{
		for (int i = 0; i < activeItems.Count; i++)
		{
			activeItems[i].ClearedRoom();
		}
		NumRoomsCleared++;
		if (CharacterUsesRandomGuns && m_gunGameElapsed > 20f)
		{
			ChangeToRandomGun();
		}
		if (this.OnRoomClearEvent != null)
		{
			this.OnRoomClearEvent(this);
		}
	}

	public void ChangeToRandomGun()
	{
		if (IsGhost)
		{
			return;
		}
		m_gunGameElapsed = 0f;
		m_gunGameDamageThreshold = 200f;
		if (inventory.GunLocked.Value || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
		{
			return;
		}
		Gun currentGun = CurrentGun;
		inventory.AddGunToInventory(PickupObjectDatabase.GetRandomGun());
		PlayEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_MagicFavor_Change") as GameObject, new Vector3(0f, -1f, 0f));
		if ((bool)currentGun)
		{
			if (currentGun.IsFiring)
			{
				currentGun.CeaseAttack();
			}
			inventory.RemoveGunFromInventory(currentGun);
			UnityEngine.Object.Destroy(currentGun.gameObject);
		}
	}

	public void OnAnyEnemyTookAnyDamage(float damageDone, bool fatal, HealthHaver target)
	{
		if (OnAnyEnemyReceivedDamage != null)
		{
			OnAnyEnemyReceivedDamage(damageDone, fatal, target);
		}
		AIActor aIActor = ((!target) ? null : target.aiActor);
		if (((bool)aIActor && !aIActor.IsNormalEnemy) || IsGhost || target.PreventCooldownGainFromDamage)
		{
			return;
		}
		for (int i = 0; i < activeItems.Count; i++)
		{
			activeItems[i].DidDamage(this, damageDone);
		}
		if (inventory == null || inventory.AllGuns == null)
		{
			return;
		}
		for (int j = 0; j < inventory.AllGuns.Count; j++)
		{
			if (inventory.AllGuns[j].UsesRechargeLikeActiveItem)
			{
				inventory.AllGuns[j].ApplyActiveCooldownDamage(this, damageDone);
			}
		}
	}

	public void OnDidDamage(float damageDone, bool fatal, HealthHaver target)
	{
		if (this.OnDealtDamage != null)
		{
			this.OnDealtDamage(this, damageDone);
		}
		if (this.OnDealtDamageContext != null)
		{
			this.OnDealtDamageContext(this, damageDone, fatal, target);
		}
		if (fatal)
		{
			m_enemiesKilled++;
			m_gunGameDamageThreshold = 200f;
			if (CharacterUsesRandomGuns && m_enemiesKilled % 5 == 0)
			{
				ChangeToRandomGun();
			}
		}
		if (CharacterUsesRandomGuns)
		{
			m_gunGameDamageThreshold -= Mathf.Max(damageDone, 3f);
			if (m_gunGameDamageThreshold < 0f)
			{
				ChangeToRandomGun();
			}
		}
		if (fatal && this.OnKilledEnemy != null)
		{
			this.OnKilledEnemy(this);
		}
		if (fatal && this.OnKilledEnemyContext != null)
		{
			this.OnKilledEnemyContext(this, target);
		}
	}

	protected void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		for (int i = 0; i < animationAudioEvents.Count; i++)
		{
			if (animationAudioEvents[i].eventTag == frame.eventInfo)
			{
				AkSoundEngine.PostEvent(animationAudioEvents[i].eventName, base.gameObject);
			}
		}
	}

	public void HandleDodgedBeam(BeamController beam)
	{
		if (this.OnDodgedBeam != null)
		{
			this.OnDodgedBeam(beam, this);
		}
	}

	protected virtual void OnPreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (DodgeRollIsBlink && IsDodgeRolling && (bool)otherRigidbody && ((bool)otherRigidbody.projectile || !otherRigidbody.GetComponent<DungeonDoorController>()))
		{
			PhysicsEngine.SkipCollision = true;
		}
		else if (IsGhost && (bool)otherRigidbody && (bool)otherRigidbody.aiActor)
		{
			PhysicsEngine.SkipCollision = true;
		}
		else
		{
			if (!IsDodgeRolling || !otherRigidbody)
			{
				return;
			}
			if ((bool)otherRigidbody.projectile && this.OnDodgedProjectile != null)
			{
				this.OnDodgedProjectile(otherRigidbody.projectile);
			}
			if (!otherRigidbody.aiActor)
			{
				return;
			}
			if (DodgeRollIsBlink)
			{
				PhysicsEngine.SkipCollision = true;
				return;
			}
			FreezeOnDeath component = otherRigidbody.GetComponent<FreezeOnDeath>();
			if ((bool)component && component.IsDeathFrozen)
			{
				return;
			}
			AIActor aIActor = otherRigidbody.aiActor;
			if ((bool)aIActor.healthHaver)
			{
				float num = stats.rollDamage * stats.GetStatValue(PlayerStats.StatType.DodgeRollDamage);
				if (aIActor.healthHaver.IsDead)
				{
					PhysicsEngine.SkipCollision = true;
				}
				else if (!m_rollDamagedEnemies.Contains(aIActor) && aIActor.healthHaver.GetCurrentHealth() < num && aIActor.healthHaver.CanCurrentlyBeKilled)
				{
					ApplyRollDamage(aIActor);
					PhysicsEngine.SkipCollision = true;
				}
			}
			if (aIActor.IsFrozen)
			{
				GameActorFreezeEffect gameActorFreezeEffect = aIActor.GetEffect("freeze") as GameActorFreezeEffect;
				float num2 = ((gameActorFreezeEffect == null) ? 0f : (aIActor.healthHaver.GetMaxHealth() * gameActorFreezeEffect.UnfreezeDamagePercent));
				if (gameActorFreezeEffect != null && num2 >= aIActor.healthHaver.GetCurrentHealth() && aIActor.healthHaver.CanCurrentlyBeKilled)
				{
					aIActor.healthHaver.ApplyDamage(num2, lockedDodgeRollDirection, "DODGEROLL OF AWESOME", CoreDamageTypes.None, DamageCategory.Collision, true);
					GameManager.Instance.platformInterface.AchievementUnlock(Achievement.KILL_FROZEN_ENEMY_WITH_ROLL);
					PhysicsEngine.SkipCollision = true;
				}
				else if ((bool)aIActor.knockbackDoer)
				{
					aIActor.knockbackDoer.ApplyKnockback(lockedDodgeRollDirection, 5f);
				}
			}
		}
	}

	public void ApplyRollDamage(AIActor actor)
	{
		if (m_rollDamagedEnemies.Contains(actor))
		{
			return;
		}
		bool flag = false;
		if (actor.HasOverrideDodgeRollDeath && string.IsNullOrEmpty(actor.healthHaver.overrideDeathAnimation))
		{
			flag = true;
			actor.healthHaver.overrideDeathAnimation = actor.OverrideDodgeRollDeath;
		}
		if ((bool)actor.specRigidbody && PassiveItem.ActiveFlagItems.ContainsKey(this) && (PassiveItem.ActiveFlagItems[this].ContainsKey(typeof(SpikedArmorItem)) || PassiveItem.ActiveFlagItems[this].ContainsKey(typeof(HelmetItem))))
		{
			PixelCollider hitboxPixelCollider = actor.specRigidbody.HitboxPixelCollider;
			if (hitboxPixelCollider != null)
			{
				Vector2 vector = BraveMathCollege.ClosestPointOnRectangle(base.specRigidbody.GetUnitCenter(ColliderType.HitBox), hitboxPixelCollider.UnitBottomLeft, hitboxPixelCollider.UnitDimensions);
				SpawnManager.SpawnVFX((GameObject)BraveResources.Load("Global VFX/VFX_DodgeRollHit"), vector, Quaternion.identity, true);
			}
		}
		actor.healthHaver.ApplyDamage(stats.rollDamage * stats.GetStatValue(PlayerStats.StatType.DodgeRollDamage), lockedDodgeRollDirection, "DODGEROLL");
		m_rollDamagedEnemies.Add(actor);
		if (this.OnRolledIntoEnemy != null)
		{
			this.OnRolledIntoEnemy(this, actor);
		}
		if (flag)
		{
			actor.healthHaver.overrideDeathAnimation = string.Empty;
		}
	}

	private void HealthChanged(float result, float max)
	{
		if (!(GameUIRoot.Instance == null))
		{
			Debug.Log("changing health to: " + result + "|" + max);
			GameUIRoot.Instance.UpdatePlayerHealthUI(this, base.healthHaver);
		}
	}

	public void HandleCloneItem(ExtraLifeItem source)
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
			if (otherPlayer.IsGhost)
			{
				DoCloneEffect();
			}
			else
			{
				m_cloneWaitingForCoopDeath = true;
			}
		}
		else
		{
			DoCloneEffect();
		}
	}

	private void DoCloneEffect()
	{
		StartCoroutine(HandleCloneEffect());
	}

	private IEnumerator HandleCloneEffect()
	{
		Pixelator.Instance.FadeToBlack(0.5f);
		GameUIRoot.Instance.ToggleUICamera(false);
		base.healthHaver.FullHeal();
		IsOnFire = false;
		CurrentFireMeterValue = 0f;
		CurrentPoisonMeterValue = 0f;
		CurrentCurseMeterValue = 0f;
		CurrentDrainMeterValue = 0f;
		if (characterIdentity == PlayableCharacters.Robot)
		{
			base.healthHaver.Armor = 6f;
		}
		float ela = 0f;
		while (ela < 0.5f)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		int targetLevelIndex = 1;
		if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT)
		{
			targetLevelIndex += GameManager.Instance.LastShortcutFloorLoaded;
		}
		GameManager.Instance.SetNextLevelIndex(targetLevelIndex);
		if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
		{
			GameManager.Instance.DelayedLoadBossrushFloor(0.5f);
		}
		else
		{
			GameManager.Instance.DelayedLoadNextLevel(0.5f);
		}
		m_cloneWaitingForCoopDeath = false;
		ExtraLifeItem cloneItem = null;
		for (int i = 0; i < passiveItems.Count; i++)
		{
			if (passiveItems[i] is ExtraLifeItem)
			{
				ExtraLifeItem extraLifeItem = passiveItems[i] as ExtraLifeItem;
				if (extraLifeItem.extraLifeMode == ExtraLifeItem.ExtraLifeMode.CLONE)
				{
					cloneItem = extraLifeItem;
				}
			}
		}
		if (cloneItem != null)
		{
			RemovePassiveItem(cloneItem.PickupObjectId);
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[j];
				if (playerController.IsGhost)
				{
					playerController.StartCoroutine(playerController.CoopResurrectInternal(playerController.transform.position, null, true));
				}
				playerController.healthHaver.FullHeal();
				playerController.specRigidbody.Velocity = Vector2.zero;
				playerController.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
				if (playerController.m_returnTeleporter != null)
				{
					playerController.m_returnTeleporter.ClearReturnActive();
					playerController.m_returnTeleporter = null;
				}
			}
			Chest.ToggleCoopChests(false);
		}
		else
		{
			base.healthHaver.FullHeal();
			base.specRigidbody.Velocity = Vector2.zero;
			base.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
			if (m_returnTeleporter != null)
			{
				m_returnTeleporter.ClearReturnActive();
				m_returnTeleporter = null;
			}
		}
		yield return new WaitForSeconds(1f);
		IsOnFire = false;
		CurrentFireMeterValue = 0f;
		CurrentPoisonMeterValue = 0f;
		CurrentCurseMeterValue = 0f;
		CurrentDrainMeterValue = 0f;
		base.healthHaver.FullHeal();
		if (characterIdentity == PlayableCharacters.Robot)
		{
			base.healthHaver.Armor = 6f;
		}
	}

	public void EscapeRoom(EscapeSealedRoomStyle escapeStyle, bool resetCurrentRoom, RoomHandler targetRoom = null)
	{
		RespawnInPreviousRoom(false, escapeStyle, resetCurrentRoom, targetRoom);
		if (targetRoom != null)
		{
			targetRoom.EnsureUpstreamLocksUnlocked();
		}
		base.specRigidbody.Velocity = Vector2.zero;
		base.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
	}

	public void RespawnInPreviousRoom(bool doFullHeal, EscapeSealedRoomStyle escapeStyle, bool resetCurrentRoom, RoomHandler targetRoom = null)
	{
		RoomHandler currentRoom = CurrentRoom;
		if (targetRoom == null)
		{
			targetRoom = GetPreviousRoom(CurrentRoom);
		}
		m_lastInteractionTarget = null;
		if (escapeStyle == EscapeSealedRoomStyle.TELEPORTER)
		{
			IntVector2? randomAvailableCell = targetRoom.GetRandomAvailableCell(new IntVector2(2, 2), CellTypes.FLOOR);
			if (randomAvailableCell.HasValue)
			{
				TeleportToPoint(randomAvailableCell.Value.ToCenterVector2(), true);
			}
			if (resetCurrentRoom && CurrentRoom != targetRoom)
			{
				StartCoroutine(DelayedRoomReset(currentRoom));
			}
		}
		else if (resetCurrentRoom)
		{
			StartCoroutine(HandleResetAndRespawn_CR(targetRoom, currentRoom, doFullHeal, escapeStyle));
		}
	}

	private RoomHandler GetPreviousRoom(RoomHandler currentRoom)
	{
		RoomHandler roomHandler = null;
		for (int i = 0; i < currentRoom.connectedRooms.Count; i++)
		{
			if (currentRoom.connectedRooms[i].visibility != 0 && currentRoom.distanceFromEntrance > currentRoom.connectedRooms[i].distanceFromEntrance)
			{
				roomHandler = currentRoom.connectedRooms[i];
				break;
			}
		}
		if (roomHandler == null)
		{
			for (int j = 0; j < currentRoom.connectedRooms.Count; j++)
			{
				if (currentRoom.connectedRooms[j].visibility != 0)
				{
					roomHandler = currentRoom.connectedRooms[j];
					break;
				}
			}
		}
		if (roomHandler != null && roomHandler.area.IsProceduralRoom && roomHandler.area.proceduralCells != null)
		{
			for (int k = 0; k < roomHandler.connectedRooms.Count; k++)
			{
				if (roomHandler.connectedRooms[k].visibility != 0 && roomHandler.distanceFromEntrance > roomHandler.connectedRooms[k].distanceFromEntrance && roomHandler.connectedRooms[k] != currentRoom)
				{
					roomHandler = roomHandler.connectedRooms[k];
					break;
				}
			}
		}
		if (roomHandler == null)
		{
			Debug.Log("Could not find a previous room that has been visited!");
			roomHandler = GameManager.Instance.Dungeon.data.Entrance;
		}
		return roomHandler;
	}

	private IEnumerator DelayedRoomReset(RoomHandler targetRoom)
	{
		if (GameManager.Instance.InTutorial)
		{
			targetRoom.npcSealState = RoomHandler.NPCSealState.SealNone;
		}
		while (CurrentRoom == targetRoom)
		{
			yield return null;
		}
		yield return null;
		if (targetRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear) || !targetRoom.EverHadEnemies || GameManager.Instance.InTutorial)
		{
			targetRoom.ResetPredefinedRoomLikeDarkSouls();
		}
		if (!targetRoom.EverHadEnemies)
		{
			targetRoom.forceTeleportersActive = true;
		}
		if (GameManager.Instance.InTutorial)
		{
			CurrentRoom.UnsealRoom();
		}
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int num = allProjectiles.Count - 1; num >= 0; num--)
		{
			if ((bool)allProjectiles[num])
			{
				allProjectiles[num].DieInAir();
			}
		}
	}

	private IEnumerator HandleResetAndRespawn_CR(RoomHandler roomToSpawnIn, RoomHandler roomToReset, bool doFullHeal, EscapeSealedRoomStyle escapeStyle)
	{
		if ((bool)CurrentGun)
		{
			CurrentGun.CeaseAttack(false);
		}
		if (doFullHeal)
		{
			base.healthHaver.FullHeal();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
				if (otherPlayer.healthHaver.IsAlive)
				{
					otherPlayer.healthHaver.FullHeal();
				}
				otherPlayer.CurrentInputState = PlayerInputState.NoInput;
			}
		}
		GameManager.Instance.PauseRaw(true);
		CurrentInputState = PlayerInputState.NoInput;
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		Transform cameraTransform = GameManager.Instance.MainCameraController.transform;
		Vector3 cameraStartPosition = cameraTransform.position;
		Vector3 cameraEndPosition = base.CenterPosition;
		if (escapeStyle == EscapeSealedRoomStyle.GRIP_MASTER)
		{
			cameraEndPosition += new Vector3(0f, 3f);
		}
		GameManager.Instance.MainCameraController.OverridePosition = cameraStartPosition;
		ToggleGunRenderers(false, "death");
		ToggleHandRenderers(false, "death");
		if (escapeStyle == EscapeSealedRoomStyle.GRIP_MASTER)
		{
			ToggleRenderer(false, "gripmaster");
		}
		float elapsed = 0f;
		float duration3 = 0.8f;
		Pixelator.Instance.LerpToLetterbox(0.35f, 0.8f);
		while (elapsed < duration3)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / duration3);
			GameManager.Instance.MainCameraController.OverridePosition = Vector3.Lerp(cameraStartPosition, cameraEndPosition, smoothT);
			yield return null;
		}
		elapsed = 0f;
		duration3 = 0f;
		switch (escapeStyle)
		{
		case EscapeSealedRoomStyle.DEATH_SEQUENCE:
			duration3 = 0.8f;
			break;
		case EscapeSealedRoomStyle.ESCAPE_SPIN:
			duration3 = 1.5f;
			break;
		case EscapeSealedRoomStyle.GRIP_MASTER:
			duration3 = 2.25f;
			break;
		case EscapeSealedRoomStyle.NONE:
			duration3 = 0.5f;
			break;
		}
		switch (escapeStyle)
		{
		case EscapeSealedRoomStyle.DEATH_SEQUENCE:
			base.spriteAnimator.Play((!UseArmorlessAnim) ? "death_shot" : "death_shot_armorless");
			break;
		case EscapeSealedRoomStyle.ESCAPE_SPIN:
			base.spriteAnimator.Play((!UseArmorlessAnim) ? "spinfall" : "spinfall_armorless");
			break;
		}
		while (elapsed < duration3)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float timeMod = ((escapeStyle != EscapeSealedRoomStyle.ESCAPE_SPIN) ? 1f : BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, elapsed / duration3));
			base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME * timeMod);
			yield return null;
		}
		Pixelator.Instance.FadeToBlack(1f);
		elapsed = 0f;
		duration3 = 1f;
		while (elapsed < duration3)
		{
			if (escapeStyle == EscapeSealedRoomStyle.ESCAPE_SPIN)
			{
				base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		m_interruptingPitRespawn = true;
		Pixelator.Instance.LerpToLetterbox(0.5f, 0f);
		IntVector2 availableCell = roomToSpawnIn.GetCenteredVisibleClearSpot(3, 3);
		base.transform.position = new Vector3((float)availableCell.x + 0.5f, (float)availableCell.y + 0.5f, -0.1f);
		ForceChangeRoom(roomToSpawnIn);
		Reinitialize();
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer2 = GameManager.Instance.GetOtherPlayer(this);
			if (otherPlayer2.healthHaver.IsAlive)
			{
				otherPlayer2.transform.position = base.transform.position + Vector3.right;
				otherPlayer2.ForceChangeRoom(roomToSpawnIn);
				otherPlayer2.Reinitialize();
			}
		}
		GameUIRoot.Instance.bossController.DisableBossHealth();
		GameUIRoot.Instance.bossController2.DisableBossHealth();
		GameUIRoot.Instance.bossControllerSide.DisableBossHealth();
		GameManager.Instance.MainCameraController.OverridePosition = base.CenterPosition;
		yield return null;
		ToggleGunRenderers(true, "death");
		ToggleHandRenderers(true, "death");
		if (escapeStyle == EscapeSealedRoomStyle.GRIP_MASTER)
		{
			ToggleRenderer(true, "gripmaster");
		}
		GameManager.Instance.ForceUnpause();
		GameManager.Instance.PreventPausing = false;
		CurrentInputState = PlayerInputState.AllInput;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer3 = GameManager.Instance.GetOtherPlayer(this);
			otherPlayer3.CurrentInputState = PlayerInputState.AllInput;
		}
		if (roomToReset != GameManager.Instance.Dungeon.data.Entrance && (roomToReset.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear) || !roomToReset.EverHadEnemies))
		{
			roomToReset.ResetPredefinedRoomLikeDarkSouls();
		}
		Pixelator.Instance.FadeToBlack(1f, true);
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int num = allProjectiles.Count - 1; num >= 0; num--)
		{
			if ((bool)allProjectiles[num])
			{
				allProjectiles[num].DieInAir();
			}
		}
	}

	public void OnLostArmor()
	{
		ForceBlank();
		if (lostAllArmorVFX != null && base.healthHaver.Armor == 0f)
		{
			GameObject gameObject = SpawnManager.SpawnDebris(lostAllArmorVFX, base.specRigidbody.UnitTopCenter, Quaternion.identity);
			gameObject.GetComponent<DebrisObject>().Trigger(Vector3.zero, 0.5f);
		}
		if (LostArmor != null)
		{
			LostArmor();
		}
	}

	private void Damaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		PlatformInterface.SetAlienFXColor(m_alienDamageColor, 1f);
		DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
		HasTakenDamageThisRun = true;
		HasTakenDamageThisFloor = true;
		if (CurrentRoom != null)
		{
			CurrentRoom.PlayerHasTakenDamageInThisRoom = true;
		}
		if ((bool)CurrentGun && CurrentGun.IsCharging)
		{
			CurrentGun.CeaseAttack(false);
		}
		Pixelator.Instance.HandleDamagedVignette(damageDirection);
		Exploder.DoRadialKnockback(base.CenterPosition, 50f, 3f);
		bool flag = base.healthHaver.Armor > 0f;
		if (resultValue <= 0f && !flag && !m_revenging && PassiveItem.IsFlagSetForCharacter(this, typeof(PoweredByRevengeItem)))
		{
			StartCoroutine(HandleFueledByRevenge());
			base.healthHaver.ApplyHealing(0.5f - resultValue);
			resultValue = 0.5f;
		}
		if (resultValue <= 0f && !flag && CurrentItem is RationItem)
		{
			RationItem rationItem = CurrentItem as RationItem;
			UseItem();
			resultValue += rationItem.healingAmount;
		}
		bool flag2 = resultValue <= 0f && !flag;
		if (damageCategory != DamageCategory.DamageOverTime && flag2)
		{
			ScreenShakeSettings shakesettings = new ScreenShakeSettings(0.25f, 7f, 0.1f, 0.3f);
			GameManager.Instance.MainCameraController.DoScreenShake(shakesettings, base.specRigidbody.UnitCenter);
		}
		bool flag3 = false;
		if ((GameManager.Instance.InTutorial || flag3) && resultValue <= 0f && !flag)
		{
			RespawnInPreviousRoom(true, EscapeSealedRoomStyle.DEATH_SEQUENCE, true);
			base.specRigidbody.Velocity = Vector2.zero;
			base.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
			{
				foreach (Gun allGun in inventory.AllGuns)
				{
					allGun.ammo = allGun.AdjustedMaxAmmo;
				}
				return;
			}
		}
		if (resultValue <= 0f && !flag)
		{
			if ((bool)CurrentGun)
			{
				CurrentGun.CeaseAttack(false);
			}
			CurrentInputState = PlayerInputState.NoInput;
			m_handlingQueuedAnimation = true;
			HandleDarkSoulsHollowTransition(false);
			return;
		}
		if (this.OnReceivedDamage != null)
		{
			this.OnReceivedDamage(this);
		}
		if (ownerlessStatModifiers == null)
		{
			return;
		}
		bool flag4 = false;
		for (int i = 0; i < ownerlessStatModifiers.Count; i++)
		{
			if (ownerlessStatModifiers[i].isMeatBunBuff)
			{
				flag4 = true;
				ownerlessStatModifiers.RemoveAt(i);
				i--;
			}
		}
		if (flag4 && stats != null)
		{
			Debug.LogError("Did remove meatbun buff!");
			stats.RecalculateStats(this);
		}
	}

	private IEnumerator HandleFueledByRevenge()
	{
		m_revenging = true;
		base.healthHaver.IsVulnerable = false;
		float ela = 0f;
		float duration = 3f;
		OnKilledEnemy += RevengeRevive;
		int cachedKills = m_enemiesKilled;
		Material vignetteMaterial = Pixelator.Instance.FadeMaterial;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			float t = Mathf.Lerp(0f, 1f, ela / duration);
			vignetteMaterial.SetColor("_VignetteColor", Color.red);
			vignetteMaterial.SetFloat("_VignettePower", Mathf.Lerp(0.5f, 2.5f, t));
			Pixelator.Instance.saturation = 1f - Mathf.Sqrt(t);
			if (m_enemiesKilled > cachedKills)
			{
				break;
			}
			yield return null;
		}
		vignetteMaterial.SetColor("_VignetteColor", Color.black);
		vignetteMaterial.SetFloat("_VignettePower", 1f);
		Pixelator.Instance.saturation = 1f;
		OnKilledEnemy -= RevengeRevive;
		base.healthHaver.IsVulnerable = true;
		if (m_enemiesKilled <= cachedKills)
		{
			base.healthHaver.ApplyDamage(100f, Vector2.zero, base.healthHaver.lastIncurredDamageSource, CoreDamageTypes.None, DamageCategory.Unstoppable, true);
		}
		m_revenging = false;
	}

	private void RevengeRevive(PlayerController obj)
	{
		base.healthHaver.FullHeal();
	}

	public void HandleDarkSoulsHollowTransition(bool isHollow = true)
	{
		if (isHollow)
		{
			IsDarkSoulsHollow = true;
			if (m_hollowAfterImage == null)
			{
				m_hollowAfterImage = base.sprite.gameObject.AddComponent<AfterImageTrailController>();
				m_hollowAfterImage.spawnShadows = true;
				m_hollowAfterImage.shadowTimeDelay = 0.05f;
				m_hollowAfterImage.shadowLifetime = 0.3f;
				m_hollowAfterImage.minTranslation = 0.05f;
				m_hollowAfterImage.maxEmission = 0f;
				m_hollowAfterImage.minEmission = 0f;
				m_hollowAfterImage.dashColor = new Color(0f, 0.44140625f, 0.55859375f);
				m_hollowAfterImage.OverrideImageShader = ShaderCache.Acquire("Brave/Internal/DownwellAfterImage");
			}
			else
			{
				m_hollowAfterImage.spawnShadows = true;
			}
			ChangeSpecialShaderFlag(2, 1f);
		}
		else
		{
			IsDarkSoulsHollow = false;
			if (m_hollowAfterImage != null)
			{
				m_hollowAfterImage.spawnShadows = false;
			}
			ChangeSpecialShaderFlag(2, 0f);
		}
	}

	public void TriggerDarkSoulsReset(bool dropItems = true, int cursedHealthMaximum = 1)
	{
		IsOnFire = false;
		CurrentFireMeterValue = 0f;
		CurrentPoisonMeterValue = 0f;
		CurrentCurseMeterValue = 0f;
		CurrentDrainMeterValue = 0f;
		AkSoundEngine.PostEvent("Stop_OBJ_paydaydrill_loop_01", GameManager.Instance.gameObject);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && !GameManager.Instance.GetOtherPlayer(this).IsGhost)
		{
			DropPileOfSouls();
			HandleDarkSoulsHollowTransition();
			StartCoroutine(HandleCoopDeath(m_isFalling));
			return;
		}
		m_interruptingPitRespawn = true;
		base.healthHaver.FullHeal();
		if (characterIdentity == PlayableCharacters.Robot)
		{
			base.healthHaver.Armor = 2f;
		}
		base.specRigidbody.Velocity = Vector2.zero;
		base.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
		if (m_returnTeleporter != null)
		{
			m_returnTeleporter.ClearReturnActive();
			m_returnTeleporter = null;
		}
		GameManager.Instance.Dungeon.DarkSoulsReset(this, dropItems, cursedHealthMaximum);
	}

	private void ContinueDarkSoulResetCoop()
	{
		StartCoroutine(CoopResurrectInternal(base.transform.position, null, true));
		base.healthHaver.FullHeal();
		base.specRigidbody.Velocity = Vector2.zero;
		base.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
		if (m_returnTeleporter != null)
		{
			m_returnTeleporter.ClearReturnActive();
			m_returnTeleporter = null;
		}
		GameManager.Instance.Dungeon.DarkSoulsReset(this, false, 1);
	}

	protected virtual void Die(Vector2 finalDamageDirection)
	{
		DeathsThisRun++;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.DIE_IN_PAST);
		}
		GameUIRoot.Instance.GetReloadBarForPlayer(this).UpdateStatusBars(this);
		bool flag = true;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
			if ((bool)otherPlayer && otherPlayer.healthHaver.IsAlive)
			{
				flag = false;
			}
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER || flag)
		{
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer2 = GameManager.Instance.GetOtherPlayer(this);
				if (otherPlayer2.m_cloneWaitingForCoopDeath)
				{
					otherPlayer2.DoCloneEffect();
					return;
				}
				if (otherPlayer2.IsDarkSoulsHollow && otherPlayer2.IsGhost)
				{
					otherPlayer2.ContinueDarkSoulResetCoop();
					StartCoroutine(HandleCoopDeath(m_isFalling));
					return;
				}
			}
			GameManager.Instance.PauseRaw(true);
			BraveTime.RegisterTimeScaleMultiplier(0f, GameManager.Instance.gameObject);
			AkSoundEngine.PostEvent("Stop_SND_All", base.gameObject);
			StartCoroutine(HandleDeath_CR());
			AkSoundEngine.PostEvent("Play_UI_gameover_start_01", base.gameObject);
		}
		else
		{
			StartCoroutine(HandleCoopDeath(m_isFalling));
		}
	}

	private void HandleCoopDeathItemDropping()
	{
		List<Gun> list = new List<Gun>();
		List<PickupObject> list2 = new List<PickupObject>();
		if ((bool)CurrentGun)
		{
			MimicGunController component = CurrentGun.GetComponent<MimicGunController>();
			if ((bool)component)
			{
				component.ForceClearMimic(true);
			}
		}
		for (int i = 0; i < inventory.AllGuns.Count; i++)
		{
			if (!inventory.AllGuns[i].CanActuallyBeDropped(this) || inventory.AllGuns[i].PersistsOnDeath)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < startingGunIds.Count; j++)
			{
				if (inventory.AllGuns[i].PickupObjectId == startingGunIds[j])
				{
					flag = true;
				}
			}
			for (int k = 0; k < startingAlternateGunIds.Count; k++)
			{
				if (inventory.AllGuns[i].PickupObjectId == startingAlternateGunIds[k])
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(inventory.AllGuns[i]);
				list2.Add(inventory.AllGuns[i]);
			}
		}
		for (int l = 0; l < passiveItems.Count; l++)
		{
			if (passiveItems[l].CanActuallyBeDropped(this) && !passiveItems[l].PersistsOnDeath && !(passiveItems[l] is ExtraLifeItem))
			{
				list2.Add(passiveItems[l]);
			}
		}
		for (int m = 0; m < activeItems.Count; m++)
		{
			if (activeItems[m].CanActuallyBeDropped(this) && !activeItems[m].PersistsOnDeath)
			{
				list2.Add(activeItems[m]);
			}
		}
		int count = list2.Count;
		for (int n = 0; n < count; n++)
		{
			if (n == 0 && list.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, list.Count);
				list2.Remove(list[index]);
				ForceDropGun(list[index]);
				list.RemoveAt(index);
			}
			else
			{
				if (list2.Count <= 0)
				{
					continue;
				}
				int index2 = UnityEngine.Random.Range(0, list2.Count);
				if (list2[index2] is Gun)
				{
					DebrisObject debrisObject = ForceDropGun(list2[index2] as Gun);
					PickupObject pickupObject = ((!debrisObject) ? null : debrisObject.GetComponentInChildren<PickupObject>());
					if ((bool)pickupObject)
					{
						pickupObject.IgnoredByRat = true;
						pickupObject.ClearIgnoredByRatFlagOnPickup = true;
					}
				}
				else if (list2[index2] is PassiveItem)
				{
					DebrisObject debrisObject2 = DropPassiveItem(list2[index2] as PassiveItem);
					PickupObject pickupObject2 = ((!debrisObject2) ? null : debrisObject2.GetComponentInChildren<PickupObject>());
					if ((bool)pickupObject2)
					{
						pickupObject2.IgnoredByRat = true;
						pickupObject2.ClearIgnoredByRatFlagOnPickup = true;
					}
				}
				else
				{
					DebrisObject debrisObject3 = DropActiveItem(list2[index2] as PlayerItem, 4f, true);
					PickupObject pickupObject3 = ((!debrisObject3) ? null : debrisObject3.GetComponentInChildren<PickupObject>());
					if ((bool)pickupObject3)
					{
						pickupObject3.IgnoredByRat = true;
						pickupObject3.ClearIgnoredByRatFlagOnPickup = true;
					}
				}
				list2.RemoveAt(index2);
			}
		}
	}

	public IEnumerator HandleCoopDeath(bool ignoreCorpse = false)
	{
		ResetOverrideAnimationLibrary();
		m_handlingQueuedAnimation = true;
		CurrentInputState = PlayerInputState.NoInput;
		if (!IsDarkSoulsHollow)
		{
			HandleCoopDeathItemDropping();
		}
		if (!GameManager.PVP_ENABLED)
		{
			ResetToFactorySettings();
		}
		m_turboSpeedModifier = null;
		m_turboRollSpeedModifier = null;
		m_turboEnemyBulletModifier = null;
		GameUIRoot.Instance.ClearGunName(IsPrimaryPlayer);
		GameUIRoot.Instance.ClearItemName(IsPrimaryPlayer);
		GameUIRoot.Instance.UpdateGunData(inventory, 0, this);
		GameUIRoot.Instance.UpdateItemData(this, CurrentItem, activeItems);
		GameUIRoot.Instance.DisableCoopPlayerUI(this);
		if (Minimap.Instance != null)
		{
			Minimap.Instance.UpdatePlayerPositionExact(base.transform.position, this, true);
		}
		Chest.ToggleCoopChests(true);
		GameManager.Instance.MainCameraController.IsLerping = true;
		base.specRigidbody.Velocity = Vector2.zero;
		base.specRigidbody.enabled = false;
		if (IsOnFire)
		{
			IsOnFire = false;
		}
		ToggleHandRenderers(false, string.Empty);
		ToggleGunRenderers(false, string.Empty);
		GameUIRoot.Instance.ForceClearReload(PlayerIDX);
		if (!ignoreCorpse)
		{
			string coopDeathAnimName = ((!UseArmorlessAnim) ? "death_coop" : "death_coop_armorless");
			base.spriteAnimator.Play(coopDeathAnimName);
			while (base.spriteAnimator.IsPlaying(coopDeathAnimName))
			{
				yield return null;
			}
			GameObject corpse = SpawnManager.SpawnDebris((GameObject)BraveResources.Load("Global Prefabs/PlayerCorpse"), base.transform.position, Quaternion.identity);
			tk2dSprite corpseSprite = corpse.GetComponent<tk2dSprite>();
			corpseSprite.SetSprite(base.sprite.Collection, base.sprite.spriteId);
			corpseSprite.scale = base.sprite.scale;
			corpse.transform.position = base.sprite.transform.position;
			corpseSprite.HeightOffGround = -3.5f;
			corpseSprite.UpdateZDepth();
		}
		BecomeGhost();
	}

	private void BecomeGhost()
	{
		IsGhost = true;
		GameManager.Instance.MainCameraController.IsLerping = true;
		ChangeSpecialShaderFlag(0, 1f);
		GameUIRoot.Instance.TransitionToGhostUI(this);
		ChangeFlatColorOverride(new Color(0.2f, 0.3f, 1f, 1f));
		base.specRigidbody.enabled = true;
		base.specRigidbody.CollideWithTileMap = true;
		base.specRigidbody.CollideWithOthers = true;
		base.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
		base.specRigidbody.Reinitialize();
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		ToggleShadowVisiblity(false);
		ToggleHandRenderers(false, "ghostliness");
		SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		m_handlingQueuedAnimation = false;
		CurrentInputState = PlayerInputState.AllInput;
	}

	public void DoCoopArrow()
	{
		if (!base.healthHaver.IsDead && base.gameObject.activeSelf && !m_isCoopArrowing)
		{
			m_isCoopArrowing = true;
			StartCoroutine(HandleCoopArrow());
		}
	}

	private IEnumerator HandleThreatArrow()
	{
		m_isThreatArrowing = true;
		GameObject extantArrow = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global VFX/Alert_Arrow"));
		extantArrow.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unoccluded"));
		extantArrow.transform.parent = base.sprite.transform;
		tk2dBaseSprite extantArrowSprite = extantArrow.GetComponent<tk2dBaseSprite>();
		extantArrowSprite.HeightOffGround = 8f;
		extantArrowSprite.UpdateZDepth();
		do
		{
			if (extantArrowSprite.GetCurrentSpriteDef().name != "blankframe")
			{
				Vector2 v = m_threadArrowTarget.CenterPosition - base.CenterPosition;
				if (v.magnitude < 3f)
				{
					break;
				}
				float num = BraveMathCollege.Atan2Degrees(v);
				num = Mathf.RoundToInt(num / 5f) * 5;
				v = Quaternion.Euler(0f, 0f, num) * Vector2.right;
				Vector2 result = Vector2.zero;
				if (BraveMathCollege.LineSegmentRectangleIntersection(base.CenterPosition, m_threadArrowTarget.CenterPosition, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint, ref result))
				{
					result -= v.normalized * 0.5f;
					extantArrow.transform.position = result.ToVector3ZUp();
					extantArrow.transform.position = extantArrow.transform.position.Quantize(0.0625f);
					extantArrow.transform.localRotation = Quaternion.Euler(0f, 0f, num);
				}
			}
			yield return null;
		}
		while (m_isThreatArrowing && (bool)m_threadArrowTarget && (m_threadArrowTarget.HasBeenEngaged || m_threadArrowTarget.AlwaysShowOffscreenArrow) && (bool)m_threadArrowTarget.healthHaver && m_threadArrowTarget.healthHaver.IsAlive);
		UnityEngine.Object.Destroy(extantArrow);
		m_isThreatArrowing = false;
		m_threadArrowTarget = null;
	}

	private IEnumerator HandleCoopArrow()
	{
		GameObject extantArrow = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global VFX/Coop_Arrow"));
		extantArrow.transform.parent = base.sprite.transform;
		tk2dBaseSprite extantArrowSprite = extantArrow.GetComponent<tk2dBaseSprite>();
		tk2dSpriteAnimator arrowAnimator = extantArrowSprite.spriteAnimator;
		do
		{
			if (extantArrowSprite.GetCurrentSpriteDef().name != "blankframe")
			{
				Vector2 v = ((!IsPrimaryPlayer) ? GameManager.Instance.PrimaryPlayer.CenterPosition : GameManager.Instance.SecondaryPlayer.CenterPosition) - base.CenterPosition;
				if (v.magnitude < 3f)
				{
					break;
				}
				float num = BraveMathCollege.Atan2Degrees(v);
				num = Mathf.RoundToInt(num / 5f) * 5;
				v = Quaternion.Euler(0f, 0f, num) * Vector2.right;
				extantArrow.transform.position = (base.CenterPosition + v * 2f).ToVector3ZUp();
				extantArrow.transform.position = extantArrow.transform.position.Quantize(0.0625f);
				extantArrow.transform.localRotation = Quaternion.Euler(0f, 0f, num);
			}
			yield return null;
		}
		while (arrowAnimator.Playing);
		UnityEngine.Object.Destroy(extantArrow);
		m_isCoopArrowing = false;
	}

	public void QueueSpecificAnimation(string animationName)
	{
		m_handlingQueuedAnimation = true;
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(QueuedAnimationComplete));
		base.spriteAnimator.Play(animationName);
	}

	protected void QueuedAnimationComplete(tk2dSpriteAnimator anima, tk2dSpriteAnimationClip clippy)
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(QueuedAnimationComplete));
		m_handlingQueuedAnimation = false;
	}

	private IEnumerator InvariantWait(float delay)
	{
		float elapsed = 0f;
		while (elapsed < delay)
		{
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			yield return null;
		}
	}

	protected void HandleDeathPhotography()
	{
		GameUIRoot.Instance.ForceClearReload();
		GameUIRoot.Instance.notificationController.ForceHide();
		Pixelator.Instance.CacheCurrentFrameToBuffer = true;
		Pixelator.Instance.CacheScreenSpacePositionsForDeathFrame(base.CenterPosition, base.CenterPosition);
	}

	private IEnumerator HandleDeath_CR()
	{
		bool wasPitFalling = base.IsFalling;
		Pixelator.Instance.DoFinalNonFadedLayer = true;
		if ((bool)CurrentGun)
		{
			CurrentGun.CeaseAttack(false);
		}
		CurrentInputState = PlayerInputState.NoInput;
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		ToggleGunRenderers(false, "death");
		ToggleHandRenderers(false, "death");
		ToggleAttachedRenderers(false);
		Transform cameraTransform = GameManager.Instance.MainCameraController.transform;
		Vector3 cameraStartPosition = cameraTransform.position;
		Vector3 cameraEndPosition = base.CenterPosition;
		GameManager.Instance.MainCameraController.OverridePosition = cameraStartPosition;
		if ((bool)CurrentGun)
		{
			CurrentGun.DespawnVFX();
		}
		HandleDeathPhotography();
		yield return null;
		ToggleHandRenderers(false, "death");
		if ((bool)CurrentGun)
		{
			CurrentGun.DespawnVFX();
		}
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unfaded"));
		GameUIRoot.Instance.ForceClearReload(PlayerIDX);
		GameUIRoot.Instance.notificationController.ForceHide();
		float elapsed = 0f;
		float duration = 0.8f;
		tk2dBaseSprite spotlightSprite = ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("DeathShadow"), base.specRigidbody.UnitCenter, Quaternion.identity)).GetComponent<tk2dBaseSprite>();
		spotlightSprite.spriteAnimator.ignoreTimeScale = true;
		spotlightSprite.spriteAnimator.Play();
		tk2dSpriteAnimator whooshAnimator = spotlightSprite.transform.GetChild(0).GetComponent<tk2dSpriteAnimator>();
		whooshAnimator.ignoreTimeScale = true;
		whooshAnimator.Play();
		Pixelator.Instance.CustomFade(0.6f, 0f, Color.white, Color.black, 0.1f, 0.5f);
		Pixelator.Instance.LerpToLetterbox(0.35f, 0.8f);
		BraveInput.AllowPausedRumble = true;
		DoVibration(Vibration.Time.Normal, Vibration.Strength.Hard);
		CompanionItem pigItem = null;
		tk2dSpriteAnimator pigVFX = null;
		bool isDoingPigSave = false;
		string pigMoveAnim = "pig_move_right";
		string pigSaveAnim = "pig_jump_right";
		for (int i = 0; i < passiveItems.Count; i++)
		{
			if (!(passiveItems[i] is CompanionItem))
			{
				continue;
			}
			CompanionItem companionItem = passiveItems[i] as CompanionItem;
			CompanionController companionController = ((!companionItem || !companionItem.ExtantCompanion) ? null : companionItem.ExtantCompanion.GetComponent<CompanionController>());
			if ((bool)companionController && companionController.name.StartsWith("Pig"))
			{
				pigVFX = (UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_HeroPig")) as GameObject).GetComponent<tk2dSpriteAnimator>();
				pigItem = companionItem;
				isDoingPigSave = true;
			}
			else if (companionItem.DisplayName == "Pig")
			{
				pigVFX = (UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_HeroPig")) as GameObject).GetComponent<tk2dSpriteAnimator>();
				pigItem = companionItem;
				isDoingPigSave = true;
			}
			if ((bool)companionItem.ExtantCompanion && (bool)companionItem.ExtantCompanion.GetComponent<SackKnightController>())
			{
				SackKnightController component = companionItem.ExtantCompanion.GetComponent<SackKnightController>();
				if (component.CurrentForm == SackKnightController.SackKnightPhase.HOLY_KNIGHT)
				{
					pigItem = companionItem;
					pigVFX = (UnityEngine.Object.Instantiate(ResourceCache.Acquire("Global VFX/VFX_HeroJunk")) as GameObject).GetComponent<tk2dSpriteAnimator>();
					isDoingPigSave = true;
					pigMoveAnim = "junk_shspcg_move_right";
					pigSaveAnim = "junk_shspcg_sacrifice_right";
				}
			}
		}
		if (!isDoingPigSave && OverrideAnimationLibrary != null)
		{
			OverrideAnimationLibrary = null;
			ResetOverrideAnimationLibrary();
			PlayEffectOnActor(BlankVFXPrefab = (GameObject)BraveResources.Load("Global VFX/VFX_BulletArmor_Death"), Vector3.zero);
		}
		while (elapsed < duration)
		{
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / duration;
			GameManager.Instance.MainCameraController.OverridePosition = Vector3.Lerp(cameraStartPosition, cameraEndPosition, t);
			base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			spotlightSprite.color = new Color(1f, 1f, 1f, t);
			Pixelator.Instance.saturation = Mathf.Clamp01(1f - t);
			yield return null;
		}
		spotlightSprite.color = Color.white;
		yield return StartCoroutine(InvariantWait(0.4f));
		Transform clockhairTransform = ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Clockhair"))).transform;
		ClockhairController clockhair = clockhairTransform.GetComponent<ClockhairController>();
		elapsed = 0f;
		duration = clockhair.ClockhairInDuration;
		Vector3 clockhairTargetPosition2 = base.CenterPosition;
		Vector3 clockhairStartPosition2 = clockhairTargetPosition2 + new Vector3(-20f, 5f, 0f);
		clockhair.renderer.enabled = false;
		clockhair.spriteAnimator.Play("clockhair_intro");
		clockhair.hourAnimator.Play("hour_hand_intro");
		clockhair.minuteAnimator.Play("minute_hand_intro");
		clockhair.secondAnimator.Play("second_hand_intro");
		if (!isDoingPigSave && (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER || GameManager.Instance.GetOtherPlayer(this).IsGhost) && OnRealPlayerDeath != null)
		{
			OnRealPlayerDeath(this);
		}
		bool hasWobbled = false;
		while (elapsed < duration)
		{
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t2 = elapsed / duration;
			float smoothT = Mathf.SmoothStep(0f, 1f, t2);
			Vector3 currentPosition = Vector3.Slerp(clockhairStartPosition2, clockhairTargetPosition2, smoothT);
			clockhairTransform.position = currentPosition.WithZ(0f);
			if (t2 > 0.5f)
			{
				clockhair.renderer.enabled = true;
				clockhair.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			}
			if (t2 > 0.75f)
			{
				clockhair.hourAnimator.GetComponent<Renderer>().enabled = true;
				clockhair.minuteAnimator.GetComponent<Renderer>().enabled = true;
				clockhair.secondAnimator.GetComponent<Renderer>().enabled = true;
				clockhair.hourAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				clockhair.minuteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				clockhair.secondAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			}
			if (!hasWobbled && clockhair.spriteAnimator.CurrentFrame == clockhair.spriteAnimator.CurrentClip.frames.Length - 1)
			{
				clockhair.spriteAnimator.Play("clockhair_wobble");
				hasWobbled = true;
			}
			clockhair.sprite.UpdateZDepth();
			base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			yield return null;
		}
		if (!hasWobbled)
		{
			clockhair.spriteAnimator.Play("clockhair_wobble");
		}
		clockhair.SpinToSessionStart(clockhair.ClockhairSpinDuration);
		elapsed = 0f;
		duration = clockhair.ClockhairSpinDuration + clockhair.ClockhairPauseBeforeShot;
		while (elapsed < duration)
		{
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			clockhair.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			yield return null;
		}
		if (isDoingPigSave)
		{
			elapsed = 0f;
			duration = 2f;
			Vector2 targetPosition = clockhairTargetPosition2;
			Vector2 startPosition = targetPosition + new Vector2(-18f, 0f);
			Vector2 pigOffset = pigVFX.sprite.WorldCenter - pigVFX.transform.position.XY();
			pigVFX.Play(pigMoveAnim);
			while (elapsed < duration)
			{
				Vector2 lerpPosition = Vector2.Lerp(startPosition, targetPosition, elapsed / duration);
				pigVFX.transform.position = (lerpPosition - pigOffset).ToVector3ZisY();
				pigVFX.sprite.UpdateZDepth();
				if (duration - elapsed < 0.1f && !pigVFX.IsPlaying(pigSaveAnim))
				{
					pigVFX.Play(pigSaveAnim);
				}
				pigVFX.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				if (GameManager.INVARIANT_DELTA_TIME == 0f)
				{
					elapsed += 0.05f;
				}
				elapsed += GameManager.INVARIANT_DELTA_TIME;
				yield return null;
			}
		}
		elapsed = 0f;
		duration = 0.1f;
		clockhairStartPosition2 = clockhairTransform.position;
		clockhairTargetPosition2 = clockhairStartPosition2 + new Vector3(0f, 12f, 0f);
		clockhair.spriteAnimator.Play("clockhair_fire");
		clockhair.hourAnimator.GetComponent<Renderer>().enabled = false;
		clockhair.minuteAnimator.GetComponent<Renderer>().enabled = false;
		clockhair.secondAnimator.GetComponent<Renderer>().enabled = false;
		DoVibration(Vibration.Time.Normal, Vibration.Strength.Hard);
		if (!isDoingPigSave)
		{
			base.spriteAnimator.Play((!UseArmorlessAnim) ? "death_shot" : "death_shot_armorless");
		}
		while (elapsed < duration)
		{
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			clockhair.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			if (!isDoingPigSave)
			{
				base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			}
			if (isDoingPigSave)
			{
				pigVFX.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				pigVFX.transform.position += new Vector3(6f * GameManager.INVARIANT_DELTA_TIME, 0f, 0f);
			}
			yield return null;
		}
		elapsed = 0f;
		duration = 1f;
		while (elapsed < duration)
		{
			if (GameManager.INVARIANT_DELTA_TIME == 0f)
			{
				elapsed += 0.05f;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			if (clockhair.spriteAnimator.CurrentFrame == clockhair.spriteAnimator.CurrentClip.frames.Length - 1)
			{
				clockhair.renderer.enabled = false;
			}
			else
			{
				clockhair.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			}
			base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
			if (isDoingPigSave)
			{
				pigVFX.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				pigVFX.transform.position += new Vector3(Mathf.Lerp(6f, 0f, elapsed / duration) * GameManager.INVARIANT_DELTA_TIME, 0f, 0f);
			}
			yield return null;
		}
		BraveInput.AllowPausedRumble = false;
		if (isDoingPigSave)
		{
			yield return StartCoroutine(InvariantWait(1f));
			Pixelator.Instance.saturation = 1f;
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_HERO_PIG, true);
			pigVFX.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Critical"));
			Pixelator.Instance.FadeToColor(0.25f, Pixelator.Instance.FadeColor, true);
			Pixelator.Instance.LerpToLetterbox(1f, 0.25f);
			UnityEngine.Object.Destroy(spotlightSprite.gameObject);
			Pixelator.Instance.DoFinalNonFadedLayer = false;
			base.healthHaver.FullHeal();
			if (ForceZeroHealthState)
			{
				base.healthHaver.Armor = 6f;
			}
			CurrentInputState = PlayerInputState.AllInput;
			if (pigItem.HasGunTransformationSacrificeSynergy && HasActiveBonusSynergy(pigItem.GunTransformationSacrificeSynergy))
			{
				GunFormeSynergyProcessor.AssignTemporaryOverrideGun(this, pigItem.SacrificeGunID, pigItem.SacrificeGunDuration);
			}
			RemovePassiveItem(pigItem.PickupObjectId);
			IsVisible = true;
			ToggleGunRenderers(true, "death");
			ToggleHandRenderers(true, "death");
			ToggleAttachedRenderers(true);
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
			GameManager.Instance.DungeonMusicController.ResetForNewFloor(GameManager.Instance.Dungeon);
			if (CurrentRoom != null)
			{
				GameManager.Instance.DungeonMusicController.NotifyEnteredNewRoom(CurrentRoom);
			}
			GameManager.Instance.ForceUnpause();
			GameManager.Instance.PreventPausing = false;
			BraveTime.ClearMultiplier(GameManager.Instance.gameObject);
			Exploder.DoRadialKnockback(base.CenterPosition, 50f, 5f);
			if (wasPitFalling)
			{
				StartCoroutine(PitRespawn(Vector2.zero));
			}
			base.healthHaver.IsVulnerable = true;
			base.healthHaver.TriggerInvulnerabilityPeriod();
		}
		else
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.NUMBER_DEATHS, 1f);
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_MINES) < 1f)
			{
				GameStatsManager.Instance.isChump = false;
			}
			AmmonomiconDeathPageController.LastKilledPlayerPrimary = IsPrimaryPlayer;
			GameManager.Instance.DoGameOver(base.healthHaver.lastIncurredDamageSource);
		}
	}

	public void ClearDeadFlags()
	{
		CurrentInputState = PlayerInputState.AllInput;
		m_handlingQueuedAnimation = false;
	}

	private void RollPitMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation || m_dodgeRollState != DodgeRollState.OnGround || IsFlying)
		{
			return;
		}
		Func<IntVector2, bool> func = delegate(IntVector2 pixel)
		{
			Vector2 vector = PhysicsEngine.PixelToUnitMidpoint(pixel);
			if (!GameManager.Instance.Dungeon.CellSupportsFalling(vector))
			{
				return false;
			}
			List<SpeculativeRigidbody> platformsAt = GameManager.Instance.Dungeon.GetPlatformsAt(vector);
			if (platformsAt != null)
			{
				for (int i = 0; i < platformsAt.Count; i++)
				{
					if (platformsAt[i].PrimaryPixelCollider.ContainsPixel(pixel))
					{
						return false;
					}
				}
			}
			return true;
		};
		PixelCollider primaryPixelCollider = specRigidbody.PrimaryPixelCollider;
		if (primaryPixelCollider != null)
		{
			IntVector2 intVector = pixelOffset - prevPixelOffset;
			if (intVector == IntVector2.Down && func(primaryPixelCollider.LowerLeft + pixelOffset) && func(primaryPixelCollider.LowerRight + pixelOffset) && (!func(primaryPixelCollider.UpperRight + prevPixelOffset) || !func(primaryPixelCollider.UpperLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Right && func(primaryPixelCollider.LowerRight + pixelOffset) && func(primaryPixelCollider.UpperRight + pixelOffset) && (!func(primaryPixelCollider.UpperLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Up && func(primaryPixelCollider.UpperRight + pixelOffset) && func(primaryPixelCollider.UpperLeft + pixelOffset) && (!func(primaryPixelCollider.LowerLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerRight + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Left && func(primaryPixelCollider.UpperLeft + pixelOffset) && func(primaryPixelCollider.LowerLeft + pixelOffset) && (!func(primaryPixelCollider.LowerRight + prevPixelOffset) || !func(primaryPixelCollider.UpperRight + prevPixelOffset)))
			{
				validLocation = false;
			}
		}
	}

	public void AcquirePuzzleItem(PickupObject item)
	{
		item.transform.parent = GunPivot;
		item.transform.localPosition = Vector3.zero;
		if ((bool)item && (bool)item.sprite)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(item.sprite, true);
		}
		additionalItems.Add(item);
	}

	public void UsePuzzleItem(PickupObject item)
	{
		if (additionalItems.Contains(item))
		{
			UnityEngine.Object.Destroy(item.gameObject);
			additionalItems.Remove(item);
		}
	}

	public PickupObject DropPuzzleItem(PickupObject item)
	{
		if (additionalItems.Contains(item) && item is NPCCellKeyItem)
		{
			additionalItems.Remove(item);
			item.transform.parent = null;
			(item as NPCCellKeyItem).DropLogic();
			GameUIRoot.Instance.UpdatePlayerConsumables(carriedConsumables);
			return item;
		}
		return null;
	}

	public void AcquirePassiveItemPrefabDirectly(PassiveItem item)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(item.gameObject);
		PassiveItem component = gameObject.GetComponent<PassiveItem>();
		EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
		if (component2 != null)
		{
			component2.DoNotificationOnEncounter = false;
		}
		component.suppressPickupVFX = true;
		component.Pickup(this);
	}

	public void AcquirePassiveItem(PassiveItem item)
	{
		AkSoundEngine.PostEvent("Play_OBJ_passive_get_01", base.gameObject);
		passiveItems.Add(item);
		item.transform.parent = GunPivot;
		item.transform.localPosition = Vector3.zero;
		item.renderer.enabled = false;
		if (item.GetComponent<DebrisObject>() != null)
		{
			UnityEngine.Object.Destroy(item.GetComponent<DebrisObject>());
		}
		if (item.GetComponent<SquishyBounceWiggler>() != null)
		{
			UnityEngine.Object.Destroy(item.GetComponent<SquishyBounceWiggler>());
		}
		GameUIRoot.Instance.AddPassiveItemToDock(item, this);
		stats.RecalculateStats(this);
	}

	public void DropPileOfSouls()
	{
		Vector3 position = base.specRigidbody.UnitBottomLeft.ToVector3ZUp();
		if (CurrentRoom != null)
		{
			position = CurrentRoom.GetBestRewardLocation(new IntVector2(2, 2), base.specRigidbody.UnitBottomLeft, false).ToVector3();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)BraveResources.Load("Global Prefabs/PileOfSouls"), position, Quaternion.identity);
		PileOfDarkSoulsPickup component = gameObject.GetComponent<PileOfDarkSoulsPickup>();
		component.TargetPlayerID = PlayerIDX;
		RoomHandler.unassignedInteractableObjects.Add(component);
		component.containedCurrency = carriedConsumables.Currency;
		carriedConsumables.Currency = 0;
		for (int i = 0; i < passiveItems.Count; i++)
		{
			if (passiveItems[i].CanActuallyBeDropped(this) && !passiveItems[i].PersistsOnDeath && passiveItems[i] is ExtraLifeItem && (passiveItems[i] as ExtraLifeItem).extraLifeMode == ExtraLifeItem.ExtraLifeMode.DARK_SOULS)
			{
				DebrisObject debrisObject = DropPassiveItem(passiveItems[i]);
				if ((bool)debrisObject)
				{
					component.passiveItems.Add(debrisObject.GetComponent<PassiveItem>());
					debrisObject.enabled = false;
					i--;
				}
			}
		}
		component.ToggleItems(false);
	}

	private void DontDontDestroyOnLoad(GameObject target)
	{
		if ((bool)target && (bool)GameManager.Instance.Dungeon && target.transform.parent == null)
		{
			target.transform.parent = GameManager.Instance.Dungeon.transform;
			target.transform.parent = null;
		}
	}

	public DebrisObject DropPassiveItem(PassiveItem item)
	{
		if ((bool)item && startingPassiveItemIds != null && characterIdentity != PlayableCharacters.Eevee)
		{
			for (int i = 0; i < startingPassiveItemIds.Count; i++)
			{
				if (startingPassiveItemIds[i] == item.PickupObjectId)
				{
					return null;
				}
			}
		}
		if (passiveItems.Contains(item))
		{
			passiveItems.Remove(item);
			GameUIRoot.Instance.RemovePassiveItemFromDock(item);
			DebrisObject debrisObject = item.Drop(this);
			stats.RecalculateStats(this);
			DontDontDestroyOnLoad(debrisObject.gameObject);
			return debrisObject;
		}
		Debug.LogError("Failed to drop item because the player doesn't have it? " + item.DisplayName);
		return null;
	}

	public DebrisObject DropActiveItem(PlayerItem item, float overrideForce = 4f, bool isDeathDrop = false)
	{
		if (isDeathDrop && (bool)item && startingActiveItemIds != null)
		{
			for (int i = 0; i < startingActiveItemIds.Count; i++)
			{
				PlayerItem playerItem = PickupObjectDatabase.GetById(startingActiveItemIds[i]) as PlayerItem;
				if (playerItem.PickupObjectId == item.PickupObjectId && !playerItem.CanActuallyBeDropped(this))
				{
					return null;
				}
			}
		}
		if (activeItems.Contains(item))
		{
			Debug.Log("DROPPING ACTIVE ITEM NOW");
			activeItems.Remove(item);
			DebrisObject result = item.Drop(this, overrideForce);
			UnityEngine.Object.Destroy(item.gameObject);
			return result;
		}
		Debug.LogError("Failed to drop item because the player doesn't have it? " + item.DisplayName);
		return null;
	}

	public void GetEquippedWith(PlayerItem item, bool switchTo = false)
	{
		if (m_preventItemSwitching)
		{
			RemoveActiveItemAt(m_selectedItemIndex);
			StopCoroutine(m_currentActiveItemDestructionCoroutine);
			m_currentActiveItemDestructionCoroutine = null;
			m_preventItemSwitching = false;
		}
		if (m_suppressItemSwitchTo)
		{
			switchTo = false;
		}
		item.transform.parent = GunPivot;
		item.transform.localPosition = Vector3.zero;
		int num = -1;
		for (int i = 0; i < activeItems.Count; i++)
		{
			if (activeItems[i].PickupObjectId == item.PickupObjectId)
			{
				num = i;
				break;
			}
		}
		int num2 = 0;
		for (int j = 0; j < item.passiveStatModifiers.Length; j++)
		{
			if (item.passiveStatModifiers[j].statToBoost == PlayerStats.StatType.AdditionalItemCapacity)
			{
				num2 += Mathf.RoundToInt(item.passiveStatModifiers[j].amount);
			}
		}
		if (item is TeleporterPrototypeItem)
		{
			for (int k = 0; k < activeItems.Count; k++)
			{
				if (activeItems[k] is ChestTeleporterItem)
				{
					num2++;
					break;
				}
			}
		}
		else if (item is ChestTeleporterItem)
		{
			for (int l = 0; l < activeItems.Count; l++)
			{
				if (activeItems[l] is TeleporterPrototypeItem)
				{
					num2++;
					break;
				}
			}
		}
		if (num == -1)
		{
			int num3 = MAX_ITEMS_HELD + (int)stats.GetStatValue(PlayerStats.StatType.AdditionalItemCapacity) + num2;
			if (stats != null)
			{
				int num4 = 0;
				while (activeItems.Count >= num3 && num4 < 100)
				{
					num4++;
					DropActiveItem(CurrentItem);
					stats.RecalculateStats(this);
					num3 = MAX_ITEMS_HELD + (int)stats.GetStatValue(PlayerStats.StatType.AdditionalItemCapacity) + num2;
				}
			}
			activeItems.Add(item);
			if (switchTo)
			{
				m_selectedItemIndex = activeItems.Count - 1;
			}
		}
		else
		{
			if (item.canStack)
			{
				activeItems[num].numberOfUses += item.numberOfUses;
				if (switchTo)
				{
					m_selectedItemIndex = num;
				}
			}
			UnityEngine.Object.Destroy(item.gameObject);
		}
		stats.RecalculateStats(this);
	}

	public void ForceConsumableBlank()
	{
		if (AcceptingNonMotionInput && Time.timeScale > 0f)
		{
			DoConsumableBlank();
		}
	}

	protected void DoConsumableBlank()
	{
		if (Blanks <= 0)
		{
			return;
		}
		Blanks--;
		PlatformInterface.SetAlienFXColor(m_alienBlankColor, 1f);
		ForceBlank();
		if (!IsInCombat)
		{
			for (int i = 0; i < StaticReferenceManager.AllAdvancedShrineControllers.Count; i++)
			{
				if (StaticReferenceManager.AllAdvancedShrineControllers[i].IsBlankShrine && StaticReferenceManager.AllAdvancedShrineControllers[i].transform.position.GetAbsoluteRoom() == CurrentRoom)
				{
					StaticReferenceManager.AllAdvancedShrineControllers[i].OnBlank();
				}
			}
		}
		for (int j = 0; j < StaticReferenceManager.AllRatTrapdoors.Count; j++)
		{
			if ((bool)StaticReferenceManager.AllRatTrapdoors[j])
			{
				StaticReferenceManager.AllRatTrapdoors[j].OnBlank();
			}
		}
		m_blankCooldownTimer = 0.5f;
		DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
		if (this.OnUsedBlank != null)
		{
			this.OnUsedBlank(this, Blanks);
		}
	}

	public void ForceBlank(float overrideRadius = 25f, float overrideTimeAtMaxRadius = 0.5f, bool silent = false, bool breaksWalls = true, Vector2? overrideCenter = null, bool breaksObjects = true, float overrideForce = -1f)
	{
		if (!silent)
		{
			if (BlankVFXPrefab == null)
			{
				BlankVFXPrefab = (GameObject)BraveResources.Load("Global VFX/BlankVFX");
			}
			AkSoundEngine.PostEvent("Play_OBJ_silenceblank_use_01", base.gameObject);
			AkSoundEngine.PostEvent("Stop_ENM_attack_cancel_01", base.gameObject);
		}
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		silencerInstance.TriggerSilencer((!overrideCenter.HasValue) ? base.CenterPosition : overrideCenter.Value, 50f, overrideRadius, (!silent) ? BlankVFXPrefab : null, (!silent) ? 0.15f : 0f, (!silent) ? 0.2f : 0f, (!silent) ? 50 : 0, (!silent) ? 10 : 0, silent ? 0f : ((!(overrideForce >= 0f)) ? 140f : overrideForce), breaksObjects ? ((!silent) ? 15 : 5) : 0, overrideTimeAtMaxRadius, this, breaksWalls);
		DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
	}

	protected void DoGhostBlank()
	{
		if (BlankVFXPrefab == null)
		{
			BlankVFXPrefab = (GameObject)BraveResources.Load("Global VFX/BlankVFX_Ghost");
		}
		PlatformInterface.SetAlienFXColor(m_alienBlankColor, 1f);
		AkSoundEngine.PostEvent("Play_OBJ_silenceblank_small_01", base.gameObject);
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		float additionalTimeAtMaxRadius = 0.25f;
		silencerInstance.TriggerSilencer(base.CenterPosition, 20f, 3f, BlankVFXPrefab, 0f, 3f, 50f, 4f, 30f, 3f, additionalTimeAtMaxRadius, this, false);
		QueueSpecificAnimation("ghost_sneeze_right");
		m_blankCooldownTimer = 5f;
		DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
	}

	protected void UseItem()
	{
		PlayerItem currentItem = CurrentItem;
		if (!(currentItem != null) || !currentItem.CanBeUsed(this))
		{
			return;
		}
		if (this.OnUsedPlayerItem != null && !currentItem.IsOnCooldown)
		{
			this.OnUsedPlayerItem(this, currentItem);
		}
		float destroyTime = -1f;
		if (currentItem.Use(this, out destroyTime))
		{
			if (destroyTime >= 0f)
			{
				m_currentActiveItemDestructionCoroutine = StartCoroutine(TimedRemoveActiveItem(m_selectedItemIndex, destroyTime));
			}
			else
			{
				RemoveActiveItemAt(m_selectedItemIndex);
			}
		}
		else if (currentItem.consumable && currentItem.numberOfUses <= 0)
		{
		}
		DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
	}

	private IEnumerator TimedRemoveActiveItem(int indexToRemove, float delay)
	{
		m_preventItemSwitching = true;
		yield return new WaitForSeconds(delay);
		m_currentActiveItemDestructionCoroutine = null;
		m_preventItemSwitching = false;
		RemoveActiveItemAt(indexToRemove);
	}

	public void RemoveAllActiveItems()
	{
		for (int num = activeItems.Count - 1; num >= 0; num--)
		{
			RemoveActiveItemAt(num);
		}
	}

	public void RemoveAllPassiveItems()
	{
		for (int num = passiveItems.Count - 1; num >= 0; num--)
		{
			RemovePassiveItemAt(num);
		}
	}

	public void RemoveActiveItem(int pickupId)
	{
		int num = activeItems.FindIndex((PlayerItem a) => a.PickupObjectId == pickupId);
		if (num >= 0)
		{
			RemoveActiveItemAt(num);
		}
		else
		{
			Debug.LogError("Failed to remove active item because the player doesn't have it? pickupId = " + pickupId);
		}
	}

	protected void RemoveActiveItemAt(int index)
	{
		if (index >= 0 && index < activeItems.Count)
		{
			UnityEngine.Object.Destroy(activeItems[index].gameObject);
			activeItems.RemoveAt(index);
			if (m_selectedItemIndex < 0 || m_selectedItemIndex >= activeItems.Count)
			{
				m_selectedItemIndex = 0;
			}
		}
	}

	public bool HasPickupID(int pickupId)
	{
		return HasGun(pickupId) || HasActiveItem(pickupId) || HasPassiveItem(pickupId);
	}

	public bool HasGun(int pickupId)
	{
		for (int i = 0; i < inventory.AllGuns.Count; i++)
		{
			if (inventory.AllGuns[i].PickupObjectId == pickupId)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasActiveItem(int pickupId)
	{
		int num = activeItems.FindIndex((PlayerItem a) => a.PickupObjectId == pickupId);
		return num >= 0;
	}

	public bool HasPassiveItem(int pickupId)
	{
		int num = passiveItems.FindIndex((PassiveItem a) => a.PickupObjectId == pickupId);
		return num >= 0;
	}

	public void RemovePassiveItem(int pickupId)
	{
		int num = passiveItems.FindIndex((PassiveItem p) => p.PickupObjectId == pickupId);
		if (num >= 0)
		{
			RemovePassiveItemAt(num);
		}
		else
		{
			Debug.LogError("Failed to remove passive item because the player doesn't have it? pickupId = " + pickupId);
		}
	}

	protected void RemovePassiveItemAt(int index)
	{
		PassiveItem passiveItem = passiveItems[index];
		passiveItems.RemoveAt(index);
		GameUIRoot.Instance.RemovePassiveItemFromDock(passiveItem);
		UnityEngine.Object.Destroy(passiveItem);
		stats.RecalculateStats(this);
	}

	public void BloopItemAboveHead(tk2dBaseSprite targetSprite, string overrideSprite = "")
	{
		m_blooper.DoBloop(targetSprite, overrideSprite, Color.white);
	}

	public void BloopItemAboveHead(tk2dBaseSprite targetSprite, string overrideSprite, Color tintColor, bool addOutline = false)
	{
		m_blooper.DoBloop(targetSprite, overrideSprite, tintColor, addOutline);
	}

	protected override void Fall()
	{
		if (m_isFalling || (IsDodgeRolling && DodgeRollIsBlink))
		{
			return;
		}
		base.Fall();
		if (this.OnPitfall != null)
		{
			this.OnPitfall();
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			GameManager.Instance.platformInterface.AchievementUnlock(Achievement.FALL_IN_END_TIMES);
		}
		if (GameManager.Instance.InTutorial)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerFellInPit");
			if (m_dodgeRollState == DodgeRollState.OnGround)
			{
				GameManager.BroadcastRoomTalkDoerFsmEvent("playerFellInPitEarly");
			}
			else
			{
				GameManager.BroadcastRoomTalkDoerFsmEvent("playerFellInPitLate");
			}
		}
		GameStatsManager.Instance.RegisterStatChange(TrackedStats.PITS_FALLEN_INTO, 1f);
		CurrentInputState = PlayerInputState.NoInput;
		base.healthHaver.IsVulnerable = false;
		base.healthHaver.EndFlashEffects();
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CATACOMBGEON && CurrentRoom != null && CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SPECIAL && (CurrentRoom.area.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.STANDARD_SHOP || CurrentRoom.area.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.WEIRD_SHOP))
		{
			LevelToLoadOnPitfall = "tt_nakatomi";
		}
		if (!string.IsNullOrEmpty(LevelToLoadOnPitfall) && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(this).m_inputState = PlayerInputState.NoInput;
		}
		m_cachedLevelToLoadOnPitfall = LevelToLoadOnPitfall;
		if (!string.IsNullOrEmpty(m_cachedLevelToLoadOnPitfall))
		{
			Pixelator.Instance.FadeToBlack(0.5f, false, 0.5f);
			GameUIRoot.Instance.HideCoreUI(string.Empty);
			GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		}
		LevelToLoadOnPitfall = string.Empty;
		StartCoroutine(FallDownCR());
	}

	protected override void ModifyPitVectors(ref Rect modifiedRect)
	{
		base.ModifyPitVectors(ref modifiedRect);
		if (m_dodgeRollState == DodgeRollState.OnGround)
		{
			if (Mathf.Abs(lockedDodgeRollDirection.x) > 0.01f)
			{
				if (lockedDodgeRollDirection.x > 0.01f)
				{
					modifiedRect.xMax += PhysicsEngine.PixelToUnit(pitHelpers.Landing.x);
				}
				else if (lockedDodgeRollDirection.x < -0.01f)
				{
					modifiedRect.xMin -= PhysicsEngine.PixelToUnit(pitHelpers.Landing.x);
				}
			}
			if (Mathf.Abs(lockedDodgeRollDirection.y) > 0.01f)
			{
				if (lockedDodgeRollDirection.y > 0.01f)
				{
					modifiedRect.yMax += PhysicsEngine.PixelToUnit(pitHelpers.Landing.y);
				}
				else if (lockedDodgeRollDirection.y < -0.01f)
				{
					modifiedRect.yMin -= PhysicsEngine.PixelToUnit(pitHelpers.Landing.y);
				}
			}
			return;
		}
		if (Mathf.Abs(m_playerCommandedDirection.x) > 0.01f)
		{
			if (m_playerCommandedDirection.x < -0.01f)
			{
				modifiedRect.xMax += PhysicsEngine.PixelToUnit(pitHelpers.PreJump.x);
			}
			else if (m_playerCommandedDirection.x > 0.01f)
			{
				modifiedRect.xMin -= PhysicsEngine.PixelToUnit(pitHelpers.PreJump.x);
			}
		}
		if (Mathf.Abs(m_playerCommandedDirection.y) > 0.01f)
		{
			if (m_playerCommandedDirection.y < -0.01f)
			{
				modifiedRect.yMax += PhysicsEngine.PixelToUnit(pitHelpers.PreJump.y);
				modifiedRect.yMax += PhysicsEngine.PixelToUnit(base.specRigidbody.PrimaryPixelCollider.Width - base.specRigidbody.PrimaryPixelCollider.Height);
			}
			else if (m_playerCommandedDirection.y > 0.01f)
			{
				modifiedRect.yMin -= PhysicsEngine.PixelToUnit(pitHelpers.PreJump.y);
			}
		}
	}

	public void PrepareForSceneTransition()
	{
		m_inputState = PlayerInputState.NoInput;
		IsVisible = false;
	}

	public void DoInitialFallSpawn(float invisibleDelay)
	{
		StartCoroutine(HandleFallSpawn(invisibleDelay));
	}

	public void DoSpinfallSpawn(float invisibleDelay)
	{
		if (!base.healthHaver.IsDead)
		{
			StartCoroutine(HandleSpinfallSpawn(invisibleDelay));
		}
	}

	protected IEnumerator HandleSpinfallSpawn(float invisibleDelay)
	{
		CurrentInputState = PlayerInputState.NoInput;
		yield return null;
		IsVisible = true;
		ToggleGunRenderers(false, string.Empty);
		ToggleHandRenderers(false, string.Empty);
		ToggleRenderer(false, "initial spawn");
		ToggleGunRenderers(false, string.Empty);
		ToggleHandRenderers(false, string.Empty);
		ToggleShadowVisiblity(false);
		yield return new WaitForSeconds(invisibleDelay);
		ToggleShadowVisiblity(false);
		AkSoundEngine.PostEvent("Play_Fall", base.gameObject);
		ToggleRenderer(true, "initial spawn");
		m_handlingQueuedAnimation = true;
		base.spriteAnimator.Play((!UseArmorlessAnim) ? "spinfall" : "spinfall_armorless");
		float startY = base.transform.position.y;
		SpawnManager.SpawnVFX((GameObject)BraveResources.Load("Global VFX/Spinfall_Shadow_VFX"), base.specRigidbody.UnitCenter, Quaternion.identity, true);
		float cachedHeightOffGround = base.sprite.HeightOffGround;
		bool m_cachedUpdateOffscreen = base.spriteAnimator.alwaysUpdateOffscreen;
		float elapsed = 1f;
		while (elapsed > 0f)
		{
			base.spriteAnimator.alwaysUpdateOffscreen = true;
			elapsed -= BraveTime.DeltaTime;
			float t = 1f - elapsed / 1f;
			float extraY = Mathf.Lerp(13f, 0f, t);
			base.sprite.transform.position = base.sprite.transform.position.WithY(startY + extraY);
			base.sprite.HeightOffGround = cachedHeightOffGround + extraY;
			base.sprite.UpdateZDepth();
			ToggleShadowVisiblity(false);
			yield return null;
		}
		base.sprite.HeightOffGround = cachedHeightOffGround;
		base.sprite.UpdateZDepth();
		base.spriteAnimator.alwaysUpdateOffscreen = m_cachedUpdateOffscreen;
		SpawnManager.SpawnVFX((GameObject)BraveResources.Load("Global VFX/Spinfall_Poof_VFX"), base.specRigidbody.UnitCenter, Quaternion.identity, true);
		DoVibration(Vibration.Time.Quick, Vibration.Strength.Hard);
		m_handlingQueuedAnimation = false;
		ToggleGunRenderers(true, string.Empty);
		ToggleHandRenderers(true, string.Empty);
		ToggleShadowVisiblity(true);
		CurrentInputState = PlayerInputState.AllInput;
	}

	protected IEnumerator HandleFallSpawn(float invisibleDelay)
	{
		CurrentInputState = PlayerInputState.NoInput;
		if (IsGhost)
		{
			ToggleRenderer(false, "initial spawn");
			yield return new WaitForSeconds(invisibleDelay);
			ToggleRenderer(true, "initial spawn");
			IsVisible = true;
			ToggleHandRenderers(false, "ghostliness");
		}
		else
		{
			yield return null;
			IsVisible = true;
			ToggleRenderer(false, "initial spawn");
			ToggleGunRenderers(false, string.Empty);
			ToggleHandRenderers(false, string.Empty);
			yield return new WaitForSeconds(invisibleDelay);
			ToggleRenderer(true, "initial spawn");
			m_handlingQueuedAnimation = true;
			if (UseArmorlessAnim)
			{
				base.spriteAnimator.Play("pitfall_return_armorless");
				while (base.spriteAnimator.IsPlaying("pitfall_return_armorless"))
				{
					yield return null;
				}
			}
			else
			{
				base.spriteAnimator.Play("pitfall_return");
				while (base.spriteAnimator.IsPlaying("pitfall_return"))
				{
					yield return null;
				}
			}
			m_handlingQueuedAnimation = false;
			if ((bool)base.knockbackDoer)
			{
				base.knockbackDoer.ClearContinuousKnockbacks();
			}
			ToggleGunRenderers(true, string.Empty);
			ToggleHandRenderers(true, string.Empty);
			ToggleFollowerRenderers(true);
		}
		CurrentInputState = PlayerInputState.AllInput;
		if (carriedConsumables != null)
		{
			carriedConsumables.ForceUpdateUI();
		}
	}

	public void DoSpitOut()
	{
		StartCoroutine(HandleSpitOut());
	}

	protected IEnumerator HandleSpitOut()
	{
		if (IsGhost)
		{
			yield break;
		}
		CurrentInputState = PlayerInputState.NoInput;
		IsVisible = true;
		ToggleGunRenderers(false, "spit out");
		ToggleHandRenderers(false, "spit out");
		m_handlingQueuedAnimation = true;
		if (UseArmorlessAnim)
		{
			base.spriteAnimator.Play("spit_out_armorless");
			while (base.spriteAnimator.IsPlaying("spit_out_armorless"))
			{
				yield return null;
			}
		}
		else
		{
			base.spriteAnimator.Play("spit_out");
			while (base.spriteAnimator.IsPlaying("spit_out"))
			{
				yield return null;
			}
		}
		m_handlingQueuedAnimation = false;
		if ((bool)base.knockbackDoer)
		{
			base.knockbackDoer.ClearContinuousKnockbacks();
		}
		ToggleGunRenderers(true, "spit out");
		ToggleHandRenderers(true, "spit out");
		CurrentInputState = PlayerInputState.AllInput;
		PlayEffectOnActor((GameObject)ResourceCache.Acquire("Global VFX/VFX_Tarnisher_Effect"), new Vector3(0f, 0.5f, 0f));
		if (carriedConsumables != null)
		{
			carriedConsumables.ForceUpdateUI();
		}
	}

	protected void TogglePitClipping(bool doClip)
	{
		if (!this || !base.sprite || !base.sprite.gameObject)
		{
			return;
		}
		TileSpriteClipper component = base.sprite.gameObject.GetComponent<TileSpriteClipper>();
		if ((bool)component)
		{
			component.enabled = doClip;
		}
		tk2dBaseSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(base.sprite);
		if (outlineSprites == null)
		{
			return;
		}
		for (int i = 0; i < outlineSprites.Length; i++)
		{
			if ((bool)outlineSprites[i])
			{
				component = outlineSprites[i].GetComponent<TileSpriteClipper>();
			}
			if ((bool)component)
			{
				component.enabled = doClip;
			}
		}
	}

	private RoomHandler GetCurrentCellPitfallTarget()
	{
		IntVector2 intVector = base.CenterPosition.ToIntVector2(VectorConversions.Floor);
		if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
		{
			CellData cellData = GameManager.Instance.Dungeon.data[intVector];
			return cellData.targetPitfallRoom;
		}
		return null;
	}

	protected IEnumerator PitRespawn(Vector2 splashPoint)
	{
		m_interruptingPitRespawn = false;
		base.healthHaver.IsVulnerable = true;
		actorReflectionAdditionalOffset = 0f;
		bool IsFallingIntoElevatorShaft = CurrentRoom != null && CurrentRoom.RoomFallValidForMaintenance();
		bool IsFallingIntoOtherRoom = (CurrentRoom != null && CurrentRoom.TargetPitfallRoom != null) || GetCurrentCellPitfallTarget() != null;
		bool DoLayerPass = GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.PVP_ENABLED || IsFallingIntoElevatorShaft || IsFallingIntoOtherRoom;
		if (DoLayerPass)
		{
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
			SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
		}
		TogglePitClipping(false);
		ToggleShadowVisiblity(false);
		SetStencilVal(146);
		Debug.Log(string.Concat(GameManager.Instance.CurrentLevelOverrideState, " clos"));
		if (m_skipPitRespawn)
		{
			m_skipPitRespawn = false;
			m_interruptingPitRespawn = true;
			yield return new WaitForSeconds(0.5f);
		}
		else if (!string.IsNullOrEmpty(m_cachedLevelToLoadOnPitfall))
		{
			m_interruptingPitRespawn = true;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameManager.Instance.GetOtherPlayer(this).m_inputState = PlayerInputState.NoInput;
				GameManager.Instance.GetOtherPlayer(this).m_interruptingPitRespawn = true;
			}
			Pixelator.Instance.FadeToBlack(0.5f);
			yield return new WaitForSeconds(0.5f);
		}
		else if (IsFallingIntoOtherRoom)
		{
			RoomHandler targetRoom = GetCurrentCellPitfallTarget();
			if (targetRoom == null)
			{
				targetRoom = CurrentRoom.TargetPitfallRoom;
			}
			if (targetRoom != null)
			{
				m_interruptingPitRespawn = true;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					PlayerController otherPlayer2 = GameManager.Instance.GetOtherPlayer(this);
					if (otherPlayer2.IsFalling)
					{
						otherPlayer2.m_skipPitRespawn = true;
					}
				}
				TogglePitClipping(true);
				if (!m_skipPitRespawn)
				{
					Pixelator.Instance.FadeToBlack(0.5f);
				}
				yield return new WaitForSeconds(0.5f);
				TogglePitClipping(false);
				bool succeeded = false;
				Transform[] childTransforms = targetRoom.hierarchyParent.GetComponentsInChildren<Transform>(true);
				for (int i = 0; i < childTransforms.Length; i++)
				{
					if (childTransforms[i].name == "Arrival" && !m_skipPitRespawn)
					{
						WarpToPoint(childTransforms[i].position.XY());
						succeeded = true;
					}
				}
				if (!succeeded)
				{
					WarpToPoint(targetRoom.GetRandomAvailableCell(IntVector2.One * 2, CellTypes.FLOOR).Value.ToCenterVector2());
				}
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					PlayerController otherPlayer3 = GameManager.Instance.GetOtherPlayer(this);
					if ((bool)otherPlayer3)
					{
						otherPlayer3.ReuniteWithOtherPlayer(this);
						if (DoLayerPass)
						{
							otherPlayer3.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
							SpriteOutlineManager.ToggleOutlineRenderers(otherPlayer3.sprite, true);
						}
					}
				}
				if (!m_skipPitRespawn)
				{
					Pixelator.Instance.FadeToBlack(0.5f, true);
				}
				m_skipPitRespawn = false;
				if (CurrentRoom.OnTargetPitfallRoom != null)
				{
					CurrentRoom.OnTargetPitfallRoom();
				}
			}
		}
		else if (IsFallingIntoElevatorShaft)
		{
			GameObject maintenanceRoomObject = GameObject.Find("MaintenanceRoom(Clone)");
			if (maintenanceRoomObject != null)
			{
				m_interruptingPitRespawn = true;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					PlayerController otherPlayer4 = GameManager.Instance.GetOtherPlayer(this);
					if (otherPlayer4.IsFalling)
					{
						otherPlayer4.m_skipPitRespawn = true;
					}
				}
				TogglePitClipping(true);
				if (!m_skipPitRespawn)
				{
					Pixelator.Instance.FadeToBlack(0.5f);
				}
				yield return new WaitForSeconds(0.5f);
				TogglePitClipping(false);
				if (!m_skipPitRespawn)
				{
					WarpToPoint(maintenanceRoomObject.GetComponentInChildren<UsableBasicWarp>().sprite.WorldBottomCenter);
				}
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					PlayerController otherPlayer5 = GameManager.Instance.GetOtherPlayer(this);
					otherPlayer5.ReuniteWithOtherPlayer(this);
				}
				if (!m_skipPitRespawn)
				{
					Pixelator.Instance.FadeToBlack(0.5f, true);
				}
				m_skipPitRespawn = false;
			}
		}
		else if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES && !ImmuneToPits.Value)
		{
			bool flag = false;
			if ((bool)CurrentGun && CurrentGun.gunName == "Mermaid Gun")
			{
				flag = true;
			}
			if (!flag)
			{
				base.healthHaver.ApplyDamage(0.5f, Vector2.zero, StringTableManager.GetEnemiesString("#PIT"), CoreDamageTypes.None, DamageCategory.Environment);
			}
		}
		if (m_interruptingPitRespawn)
		{
			m_isFalling = false;
			ClearDodgeRollState();
			previousMineCart = null;
			m_handlingQueuedAnimation = false;
			m_renderer.enabled = true;
			SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
			if ((bool)ShadowObject)
			{
				ShadowObject.GetComponent<Renderer>().enabled = true;
			}
			base.specRigidbody.CollideWithTileMap = true;
			base.specRigidbody.CollideWithOthers = true;
			ToggleGunRenderers(true, string.Empty);
			ToggleHandRenderers(true, string.Empty);
			ToggleShadowVisiblity(true);
			CurrentInputState = PlayerInputState.AllInput;
			m_interruptingPitRespawn = false;
			if (!string.IsNullOrEmpty(m_cachedLevelToLoadOnPitfall))
			{
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					PlayerController otherPlayer6 = GameManager.Instance.GetOtherPlayer(this);
					if ((bool)otherPlayer6)
					{
						otherPlayer6.m_cachedLevelToLoadOnPitfall = string.Empty;
						otherPlayer6.LevelToLoadOnPitfall = string.Empty;
					}
				}
				ToggleShadowVisiblity(false);
				ToggleGunRenderers(false, string.Empty);
				ToggleHandRenderers(false, string.Empty);
				if (m_cachedLevelToLoadOnPitfall != "midgamesave")
				{
					if (m_cachedLevelToLoadOnPitfall == "ss_resourcefulrat")
					{
						FoyerPreloader.IsRatLoad = true;
						GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.RATGEON);
					}
					if (m_cachedLevelToLoadOnPitfall == "tt_nakatomi")
					{
						GameManager.DoMidgameSave(GlobalDungeonData.ValidTilesets.OFFICEGEON);
					}
					GameManager.Instance.LoadCustomLevel(m_cachedLevelToLoadOnPitfall);
				}
			}
			if (IsFallingIntoOtherRoom)
			{
				GameManager.Instance.MainCameraController.transform.position = base.transform.position.WithZ(base.transform.position.y + GameManager.Instance.MainCameraController.CurrentZOffset);
				GameManager.Instance.MainCameraController.ForceToPlayerPosition(this, base.transform.position);
			}
			yield break;
		}
		m_cachedLevelToLoadOnPitfall = string.Empty;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER && base.healthHaver.IsDead)
		{
			m_renderer.enabled = true;
			SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
			if ((bool)ShadowObject)
			{
				ShadowObject.GetComponent<Renderer>().enabled = true;
			}
			yield break;
		}
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.GetOtherPlayer(this).healthHaver.IsAlive)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
				if (otherPlayer.IsInMinecart)
				{
					TogglePitClipping(true);
					IgnoredByCamera = true;
					while (true)
					{
						if (!otherPlayer.IsInMinecart && otherPlayer.IsGrounded)
						{
							yield return new WaitForSeconds(0.25f);
							while (otherPlayer.IsFalling)
							{
								yield return null;
							}
							if (!otherPlayer.IsInMinecart)
							{
								break;
							}
						}
						else
						{
							yield return null;
						}
					}
					IgnoredByCamera = false;
					TogglePitClipping(false);
				}
			}
			bool spawnPointOffscreen = !GameManager.Instance.MainCameraController.PointIsVisible(m_cachedPosition);
			if (m_cachedPosition.x <= 0f || m_cachedPosition.y <= 0f || IsPositionObscuredByTopWall(m_cachedPosition) || GameManager.Instance.Dungeon.CellSupportsFalling(m_cachedPosition.ToVector3ZUp()) || (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && spawnPointOffscreen))
			{
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.GetOtherPlayer(this).healthHaver.IsAlive && !GameManager.Instance.Dungeon.CellSupportsFalling(GameManager.Instance.GetOtherPlayer(this).SpriteBottomCenter))
				{
					m_cachedPosition = GameManager.Instance.GetOtherPlayer(this).transform.position;
				}
				else
				{
					IntVector2? nearestAvailableCell = CurrentRoom.GetNearestAvailableCell(m_cachedPosition, new IntVector2(2, 3), CellTypes.FLOOR);
					if (nearestAvailableCell.HasValue)
					{
						m_cachedPosition = nearestAvailableCell.Value.ToVector2() + new Vector2(0f, 1f);
					}
					else
					{
						m_cachedPosition = CurrentRoom.GetBestRewardLocation(IntVector2.One).ToVector2();
					}
				}
			}
		}
		base.transform.position = m_cachedPosition.ToVector3ZUp(base.transform.position.z);
		base.specRigidbody.Velocity = Vector2.zero;
		base.specRigidbody.Reinitialize();
		WarpFollowersToPlayer();
		m_isFalling = false;
		ClearDodgeRollState();
		previousMineCart = null;
		m_handlingQueuedAnimation = true;
		m_renderer.enabled = true;
		ToggleShadowVisiblity(true);
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
		if ((bool)ShadowObject)
		{
			ShadowObject.GetComponent<Renderer>().enabled = true;
		}
		if (UseArmorlessAnim)
		{
			base.spriteAnimator.Play("pitfall_return_armorless");
			while (base.spriteAnimator.IsPlaying("pitfall_return_armorless"))
			{
				yield return null;
			}
		}
		else
		{
			base.spriteAnimator.Play("pitfall_return");
			while (base.spriteAnimator.IsPlaying("pitfall_return"))
			{
				yield return null;
			}
		}
		m_handlingQueuedAnimation = false;
		base.specRigidbody.CollideWithTileMap = true;
		base.specRigidbody.CollideWithOthers = true;
		if (base.healthHaver.IsAlive)
		{
			ToggleGunRenderers(true, string.Empty);
			ToggleHandRenderers(true, string.Empty);
		}
		if (!GameManager.Instance.IsLoadingLevel)
		{
			List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody);
			for (int j = 0; j < overlappingRigidbodies.Count; j++)
			{
				base.specRigidbody.RegisterGhostCollisionException(overlappingRigidbodies[j]);
				overlappingRigidbodies[j].RegisterGhostCollisionException(base.specRigidbody);
			}
			base.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(1f);
			if (CurrentRoom.OnPlayerReturnedFromPit != null)
			{
				CurrentRoom.OnPlayerReturnedFromPit(this);
			}
		}
		CurrentInputState = PlayerInputState.AllInput;
	}

	private IEnumerator FallDownCR()
	{
		base.specRigidbody.CollideWithTileMap = false;
		base.specRigidbody.CollideWithOthers = false;
		base.specRigidbody.Velocity = Vector2.zero;
		if (!IsDodgeRolling)
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (UseArmorlessAnim)
		{
			base.spriteAnimator.Play((!IsDodgeRolling) ? "pitfall_armorless" : "pitfall_down_armorless");
		}
		else
		{
			base.spriteAnimator.Play((!IsDodgeRolling) ? "pitfall" : "pitfall_down");
		}
		ToggleGunRenderers(false, "pitfall");
		ToggleHandRenderers(false, "pitfall");
		Vector2 accelVec = new Vector2(0f, -22f);
		float elapsed = 0f;
		Tribool readyForDepthSwap = Tribool.Unready;
		float m_cachedHeightOffGround = base.sprite.HeightOffGround;
		bool hasSplashed = false;
		float startY = base.CenterPosition.y;
		bool IsFallingIntoElevatorShaft = CurrentRoom != null && CurrentRoom.RoomFallValidForMaintenance();
		bool IsFallingIntoOtherRoom = (CurrentRoom != null && CurrentRoom.TargetPitfallRoom != null) || GetCurrentCellPitfallTarget() != null;
		Vector2 splashPoint = Vector2.zero;
		while (elapsed < 2f)
		{
			base.specRigidbody.Velocity = base.specRigidbody.Velocity + accelVec * BraveTime.DeltaTime;
			bool swappyDoos = !base.spriteAnimator.IsPlaying("pitfall") && !base.spriteAnimator.IsPlaying("pitfall_down");
			if (UseArmorlessAnim)
			{
				swappyDoos = swappyDoos && !base.spriteAnimator.IsPlaying("pitfall_armorless") && !base.spriteAnimator.IsPlaying("pitfall_down_armorless");
			}
			if (swappyDoos && elapsed > 0.1f)
			{
				m_renderer.enabled = false;
				SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
				base.specRigidbody.Velocity = Vector2.zero;
				accelVec = Vector2.zero;
				if (!hasSplashed)
				{
					hasSplashed = true;
					splashPoint = base.sprite.WorldCenter;
					if (!IsFallingIntoOtherRoom)
					{
						GameManager.Instance.Dungeon.tileIndices.DoSplashAtPosition(splashPoint);
					}
					DoVibration(Vibration.Time.Normal, Vibration.Strength.Medium);
				}
			}
			if (!(readyForDepthSwap == Tribool.Complete) || !m_renderer.enabled)
			{
				if (readyForDepthSwap)
				{
					base.sprite.HeightOffGround = -5f;
					bool flag = GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.PVP_ENABLED || IsFallingIntoElevatorShaft || IsFallingIntoOtherRoom;
					bool flag2 = GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER && !GameManager.PVP_ENABLED;
					if (CurrentRoom != null && CurrentRoom.RoomMovingPlatforms != null && CurrentRoom.RoomMovingPlatforms.Count > 0)
					{
						SetStencilVal(147);
						SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
					}
					if (flag)
					{
						base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
						SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
					}
					if (flag2)
					{
						TileSpriteClipper orAddComponent = base.sprite.gameObject.GetOrAddComponent<TileSpriteClipper>();
						orAddComponent.updateEveryFrame = true;
						orAddComponent.doOptimize = false;
						orAddComponent.clipMode = TileSpriteClipper.ClipMode.PitBounds;
						orAddComponent.enabled = true;
						tk2dBaseSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(base.sprite);
						for (int i = 0; i < outlineSprites.Length; i++)
						{
							if ((bool)outlineSprites[i] && (bool)outlineSprites[i].gameObject)
							{
								orAddComponent = outlineSprites[i].gameObject.GetOrAddComponent<TileSpriteClipper>();
								orAddComponent.updateEveryFrame = true;
								orAddComponent.doOptimize = false;
								orAddComponent.clipMode = TileSpriteClipper.ClipMode.PitBounds;
								orAddComponent.enabled = true;
								if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER && !GameManager.PVP_ENABLED)
								{
									outlineSprites[i].gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
								}
							}
						}
					}
					++readyForDepthSwap;
				}
				else if (!readyForDepthSwap)
				{
					float num = Mathf.Lerp(0f, base.sprite.GetBounds().extents.y, elapsed / 0.2f);
					Vector3 position = base.sprite.transform.position + base.sprite.GetBounds().center + new Vector3(0f, base.sprite.GetBounds().extents.y - num, 0f);
					if (GameManager.Instance.Dungeon.CellSupportsFalling(position) || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.PVP_ENABLED)
					{
						++readyForDepthSwap;
					}
				}
			}
			actorReflectionAdditionalOffset = (startY - base.CenterPosition.y) * 1.25f;
			base.sprite.UpdateZDepth();
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		base.sprite.HeightOffGround = m_cachedHeightOffGround;
		StartCoroutine(PitRespawn(splashPoint));
	}

	protected void AnimationCompleteDelegate(tk2dSpriteAnimator anima, tk2dSpriteAnimationClip clippy)
	{
		if (clippy.name.ToLowerInvariant().Contains("dodge"))
		{
			ToggleGunRenderers(true, "dodgeroll");
			ToggleHandRenderers(true, "dodgeroll");
			if (CurrentGun == null || string.IsNullOrEmpty(CurrentGun.dodgeAnimation))
			{
				ToggleGunRenderers(false, "postdodgeroll");
				ToggleHandRenderers(false, "postdodgeroll");
				m_postDodgeRollGunTimer = 0.05f;
			}
		}
		if (clippy.name.ToLowerInvariant().Contains("item_get"))
		{
			CurrentInputState = PlayerInputState.AllInput;
			GetComponent<HealthHaver>().IsVulnerable = true;
			ToggleGunRenderers(true, "itemGet");
			ToggleHandRenderers(true, "itemGet");
		}
		m_handlingQueuedAnimation = false;
		m_overrideGunAngle = null;
	}

	public void TriggerItemAcquisition()
	{
		m_handlingQueuedAnimation = true;
		CurrentInputState = PlayerInputState.NoInput;
		base.specRigidbody.Velocity = Vector2.zero;
		ToggleGunRenderers(false, "itemGet");
		ToggleHandRenderers(false, "itemGet");
		GetComponent<HealthHaver>().IsVulnerable = false;
		base.spriteAnimator.Play((!UseArmorlessAnim) ? "item_get" : "item_get_armorless");
	}

	private void HandleAttachedSpriteDepth(float gunAngle)
	{
		float num = 1f;
		float num2 = 0.15f;
		if (IsDodgeRolling)
		{
			gunAngle = BraveMathCollege.Atan2Degrees(lockedDodgeRollDirection);
		}
		if (!(gunAngle <= 155f) || !(gunAngle >= 25f))
		{
			num2 = ((!(gunAngle <= -60f) || !(gunAngle >= -120f)) ? (-0.15f) : (-0.15f));
		}
		else
		{
			num = -1f;
			num2 = ((!(gunAngle < 120f) || !(gunAngle >= 60f)) ? 0.15f : 0.15f);
		}
		for (int i = 0; i < m_attachedSprites.Count; i++)
		{
			m_attachedSprites[i].HeightOffGround = num2 + num * m_attachedSpriteDepths[i];
		}
	}

	public void ForceWalkInDirectionWhilePaused(DungeonData.Direction direction, float thresholdValue)
	{
		StartCoroutine(HandleForceWalkInDirectionWhilePaused(direction, thresholdValue));
	}

	private IEnumerator HandleForceWalkInDirectionWhilePaused(DungeonData.Direction direction, float thresholdValue)
	{
		if (IsDodgeRolling)
		{
			ForceStopDodgeRoll();
		}
		Vector2 dirVec = DungeonData.GetIntVector2FromDirection(direction).ToVector2();
		Vector2 adjVelocity = dirVec * stats.MovementSpeed;
		m_handlingQueuedAnimation = true;
		base.knockbackDoer.TriggerTemporaryKnockbackInvulnerability(0.25f);
		switch (direction)
		{
		case DungeonData.Direction.NORTH:
			while (base.CenterPosition.y < thresholdValue && Time.timeScale == 0f)
			{
				float modDeltaTime = Mathf.Clamp(GameManager.INVARIANT_DELTA_TIME, 0f, 0.1f);
				base.transform.position = base.transform.position + (adjVelocity * modDeltaTime).ToVector3ZUp();
				base.specRigidbody.Reinitialize();
				string animationToPlay = GetBaseAnimationName(adjVelocity, 90f);
				if (!base.spriteAnimator.IsPlaying(animationToPlay))
				{
					base.spriteAnimator.Play(animationToPlay);
				}
				base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				ForceStaticFaceDirection(adjVelocity);
				yield return null;
			}
			break;
		case DungeonData.Direction.EAST:
			while (base.CenterPosition.x < thresholdValue && Time.timeScale == 0f)
			{
				float modDeltaTime2 = Mathf.Clamp(GameManager.INVARIANT_DELTA_TIME, 0f, 0.1f);
				base.transform.position = base.transform.position + (adjVelocity * modDeltaTime2).ToVector3ZUp();
				base.specRigidbody.Reinitialize();
				string animationToPlay2 = GetBaseAnimationName(adjVelocity, 0f);
				if (!base.spriteAnimator.IsPlaying(animationToPlay2))
				{
					base.spriteAnimator.Play(animationToPlay2);
				}
				base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				ForceStaticFaceDirection(adjVelocity);
				yield return null;
			}
			break;
		case DungeonData.Direction.SOUTH:
			while (base.CenterPosition.y > thresholdValue && Time.timeScale == 0f)
			{
				float modDeltaTime3 = Mathf.Clamp(GameManager.INVARIANT_DELTA_TIME, 0f, 0.1f);
				base.transform.position = base.transform.position + (adjVelocity * modDeltaTime3).ToVector3ZUp();
				base.specRigidbody.Reinitialize();
				string animationToPlay3 = GetBaseAnimationName(adjVelocity, -90f);
				if (!base.spriteAnimator.IsPlaying(animationToPlay3))
				{
					base.spriteAnimator.Play(animationToPlay3);
				}
				base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				ForceStaticFaceDirection(adjVelocity);
				yield return null;
			}
			break;
		case DungeonData.Direction.WEST:
			while (base.CenterPosition.x > thresholdValue && Time.timeScale == 0f)
			{
				float modDeltaTime4 = Mathf.Clamp(GameManager.INVARIANT_DELTA_TIME, 0f, 0.1f);
				base.transform.position = base.transform.position + (adjVelocity * modDeltaTime4).ToVector3ZUp();
				base.specRigidbody.Reinitialize();
				string animationToPlay4 = GetBaseAnimationName(adjVelocity, 179.9f);
				if (!base.spriteAnimator.IsPlaying(animationToPlay4))
				{
					base.spriteAnimator.Play(animationToPlay4);
				}
				base.spriteAnimator.UpdateAnimation(GameManager.INVARIANT_DELTA_TIME);
				ForceStaticFaceDirection(adjVelocity);
				yield return null;
			}
			break;
		}
		if (IsDodgeRolling)
		{
			ForceStopDodgeRoll();
		}
		base.specRigidbody.Velocity = Vector2.zero;
		m_handlingQueuedAnimation = false;
	}

	public bool IsBackfacing()
	{
		float num = ((!IsDodgeRolling || !m_handlingQueuedAnimation) ? m_currentGunAngle : BraveMathCollege.Atan2Degrees(lockedDodgeRollDirection));
		if (num <= 155f && num >= 25f)
		{
			return true;
		}
		return false;
	}

	public string GetBaseAnimationSuffix(bool useCardinal = false)
	{
		float num = ((!IsDodgeRolling || !m_handlingQueuedAnimation) ? m_currentGunAngle : BraveMathCollege.Atan2Degrees(lockedDodgeRollDirection));
		if (num <= 155f && num >= 25f)
		{
			if (num < 120f && num >= 60f)
			{
				return (!useCardinal) ? "_back" : "_north";
			}
			return (!useCardinal) ? "_back_right" : "_north_east";
		}
		if (num <= -60f && num >= -120f)
		{
			return (!useCardinal) ? "_front" : "_south";
		}
		return (!useCardinal) ? "_front_right" : "_south_east";
	}

	public int GetMirrorSpriteID()
	{
		float gunAngle = BraveMathCollege.Atan2Degrees(Vector2.Scale(BraveMathCollege.DegreesToVector(m_currentGunAngle), new Vector2(1f, -1f)));
		string baseAnimationName = GetBaseAnimationName(m_playerCommandedDirection.WithY(m_playerCommandedDirection.y * -1f), gunAngle, true);
		tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(baseAnimationName);
		int currentFrame = base.spriteAnimator.CurrentFrame;
		if (clipByName != null && currentFrame >= 0 && currentFrame < clipByName.frames.Length)
		{
			return clipByName.frames[currentFrame].spriteId;
		}
		return base.sprite.spriteId;
	}

	protected virtual string GetBaseAnimationName(Vector2 v, float gunAngle, bool invertThresholds = false, bool forceTwoHands = false)
	{
		string empty = string.Empty;
		bool flag = CurrentGun != null;
		if (flag && CurrentGun.Handedness == GunHandedness.NoHanded)
		{
			forceTwoHands = true;
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			flag = false;
		}
		float num = 155f;
		float num2 = 25f;
		if (invertThresholds)
		{
			num = -155f;
			num2 -= 50f;
		}
		float num3 = 120f;
		float num4 = 60f;
		float num5 = -60f;
		float num6 = -120f;
		bool flag2 = gunAngle <= num && gunAngle >= num2;
		if (invertThresholds)
		{
			flag2 = gunAngle <= num || gunAngle >= num2;
		}
		if (IsGhost)
		{
			if (flag2)
			{
				if (gunAngle < num3 && gunAngle >= num4)
				{
					empty = "ghost_idle_back";
				}
				else
				{
					float num7 = 105f;
					empty = ((!(Mathf.Abs(gunAngle) > num7)) ? "ghost_idle_back_right" : "ghost_idle_back_left");
				}
			}
			else if (gunAngle <= num5 && gunAngle >= num6)
			{
				empty = "ghost_idle_front";
			}
			else
			{
				float num8 = 105f;
				empty = ((!(Mathf.Abs(gunAngle) > num8)) ? "ghost_idle_right" : "ghost_idle_left");
			}
		}
		else if (IsFlying)
		{
			empty = (flag2 ? ((!(gunAngle < num3) || !(gunAngle >= num4)) ? "jetpack_right_bw" : "jetpack_up") : ((!(gunAngle <= num5) || !(gunAngle >= num6)) ? ((!RenderBodyHand) ? "jetpack_right" : "jetpack_right_hand") : ((!RenderBodyHand) ? "jetpack_down" : "jetpack_down_hand")));
		}
		else if (v == Vector2.zero || IsStationary)
		{
			if (IsPetting)
			{
				empty = "pet";
			}
			else if (flag2)
			{
				if (gunAngle < num3 && gunAngle >= num4)
				{
					string text = (((forceTwoHands || !flag) && !ForceHandless) ? "idle_backward_twohands" : ((!RenderBodyHand) ? "idle_backward" : "idle_backward_hand"));
					empty = text;
				}
				else
				{
					string text2 = (((!forceTwoHands && flag) || ForceHandless) ? "idle_bw" : "idle_bw_twohands");
					empty = text2;
				}
			}
			else if (gunAngle <= num5 && gunAngle >= num6)
			{
				string text3 = (((forceTwoHands || !flag) && !ForceHandless) ? "idle_forward_twohands" : ((!RenderBodyHand) ? "idle_forward" : "idle_forward_hand"));
				empty = text3;
			}
			else
			{
				string text4 = (((forceTwoHands || !flag) && !ForceHandless) ? "idle_twohands" : ((!RenderBodyHand) ? "idle" : "idle_hand"));
				empty = text4;
			}
		}
		else if (flag2)
		{
			string text5 = (((!forceTwoHands && flag) || ForceHandless) ? "run_right_bw" : "run_right_bw_twohands");
			if (gunAngle < num3 && gunAngle >= num4)
			{
				text5 = (((forceTwoHands || !flag) && !ForceHandless) ? "run_up_twohands" : ((!RenderBodyHand) ? "run_up" : "run_up_hand"));
			}
			empty = text5;
		}
		else
		{
			string text6 = "run_right";
			if (gunAngle <= num5 && gunAngle >= num6)
			{
				text6 = "run_down";
			}
			if ((forceTwoHands || !flag) && !ForceHandless)
			{
				text6 += "_twohands";
			}
			else if (RenderBodyHand)
			{
				text6 += "_hand";
			}
			empty = text6;
		}
		if (UseArmorlessAnim && !IsGhost)
		{
			empty += "_armorless";
		}
		return empty;
	}

	private void HandleAnimations(Vector2 v, float gunAngle)
	{
		if (!m_handlingQueuedAnimation)
		{
			if (CurrentGun == null || IsGhost)
			{
				gunAngle = BraveMathCollege.Atan2Degrees((!(m_playerCommandedDirection == Vector2.zero)) ? m_playerCommandedDirection : m_lastNonzeroCommandedDirection);
			}
			string baseAnimationName = GetBaseAnimationName(v, gunAngle);
			if (!base.spriteAnimator.IsPlaying(baseAnimationName))
			{
				base.spriteAnimator.Play(baseAnimationName);
			}
		}
	}

	protected bool IsKeyboardAndMouse()
	{
		return BraveInput.GetInstanceForPlayer(PlayerIDX).IsKeyboardAndMouse();
	}

	protected Vector3 DetermineAimPointInWorld()
	{
		if (Time.timeScale == 0f)
		{
			return unadjustedAimPoint;
		}
		Vector3 vector = Vector3.zero;
		Camera component = GameManager.Instance.MainCameraController.GetComponent<Camera>();
		Vector3 position = gunAttachPoint.position;
		Vector2? vector2 = forceAimPoint;
		if (vector2.HasValue)
		{
			unadjustedAimPoint = forceAimPoint.Value;
			vector = unadjustedAimPoint;
		}
		else if (IsKeyboardAndMouse())
		{
			Ray ray = component.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
			float enter;
			if (new Plane(Vector3.forward, position).Raycast(ray, out enter))
			{
				unadjustedAimPoint = ray.GetPoint(enter);
				vector = unadjustedAimPoint;
			}
		}
		else
		{
			bool flag = BraveInput.AutoAimMode == BraveInput.AutoAim.SuperAutoAim;
			Vector2 unitCenter = base.specRigidbody.HitboxPixelCollider.UnitCenter;
			Vector2 vector3 = m_activeActions.Aim.Vector;
			bool flag2 = BraveInput.GetInstanceForPlayer(PlayerIDX).GetButton(GungeonActions.GungeonActionType.Shoot) || BraveInput.GetInstanceForPlayer(PlayerIDX).GetButtonDown(GungeonActions.GungeonActionType.Shoot) || SuperAutoAimTarget != null;
			flag2 &= vector3.magnitude > 0.4f;
			bool flag3 = false;
			bool flag4 = false;
			switch (GameManager.Options.controllerAutoAim)
			{
			case GameOptions.ControllerAutoAim.ALWAYS:
				flag4 = true;
				break;
			case GameOptions.ControllerAutoAim.NEVER:
				flag4 = false;
				break;
			case GameOptions.ControllerAutoAim.COOP_ONLY:
				flag4 = PlayerIDX != 0;
				break;
			}
			if (GameManager.Options.controllerAutoAim == GameOptions.ControllerAutoAim.AUTO_DETECT && !IsKeyboardAndMouse() && IsPrimaryPlayer)
			{
				if (IsInCombat)
				{
					if (vector3.magnitude < 0.4f)
					{
						float aANonStickTime = AANonStickTime;
						AANonStickTime = Mathf.Min(AANonStickTime + BraveTime.DeltaTime, 660f);
						AAStickTime = Mathf.Min(AAStickTime, 660f - AANonStickTime);
					}
					else
					{
						AAStickTime = Mathf.Min(AAStickTime + BraveTime.DeltaTime * 1.5f, 660f);
						AANonStickTime = Mathf.Min(AANonStickTime, 660f - AAStickTime);
					}
					if (!AACanWarn && AANonStickTime < 300f && Time.realtimeSinceStartup > AALastWarnTime + 300f)
					{
						AACanWarn = true;
					}
				}
				else if (AANonStickTime > 600f)
				{
					DoAutoAimNotification(false);
					GameManager.Options.controllerAutoAim = GameOptions.ControllerAutoAim.ALWAYS;
					AAStickTime = 0f;
					AANonStickTime = 0f;
					AALastWarnTime = -1000f;
					AACanWarn = true;
				}
				else if (AACanWarn && AANonStickTime > 300f)
				{
					DoAutoAimNotification(true);
					AALastWarnTime = Time.realtimeSinceStartup;
					AACanWarn = false;
				}
			}
			flag4 = flag4 && vector3.magnitude < 0.4f;
			if (HighAccuracyAimMode)
			{
				if (!m_activeActions.HighAccuracyAimMode)
				{
					m_activeActions.HighAccuracyAimMode = true;
				}
				vector3 = ((!(vector3.magnitude < 0.2f)) ? (vector3.normalized * Mathf.Lerp(0.2f, 1f, vector3.magnitude)) : Vector2.zero);
				if (m_previousAimVector != Vector2.zero && (double)m_previousAimVector.magnitude > 0.8 && vector3 != Vector2.zero && vector3.magnitude < 0.6f)
				{
					float num = BraveMathCollege.AbsAngleBetween(vector3.ToAngle(), m_previousAimVector.ToAngle());
					if (num < 15f || num > 155f)
					{
						vector3 = m_previousAimVector.normalized * 0.5f;
					}
				}
				if (vector3 == Vector2.zero || m_previousAimVector == Vector2.zero || BraveMathCollege.AbsAngleBetween(vector3.ToAngle(), m_previousAimVector.ToAngle()) > 10f)
				{
					m_previousAimVector = vector3;
				}
				vector3 = (m_previousAimVector = BraveMathCollege.MovingAverage(m_previousAimVector, vector3, 3));
			}
			else
			{
				if (m_activeActions.HighAccuracyAimMode)
				{
					m_activeActions.HighAccuracyAimMode = false;
				}
				if (vector3.magnitude < 0.4f)
				{
					if (m_allowMoveAsAim)
					{
						vector3 = m_activeActions.Move.Vector;
					}
					else
					{
						flag3 = true;
					}
				}
				vector3 = AdjustInputVector(vector3, BraveInput.MagnetAngles.aimCardinal, BraveInput.MagnetAngles.aimOrdinal);
			}
			if (flag && !flag2)
			{
				SuperAutoAimTarget = null;
			}
			if (vector3.magnitude < 0.4f)
			{
				vector3 = m_cachedAimDirection;
			}
			m_cachedAimDirection = vector3;
			unadjustedAimPoint = position + (Vector3)(vector3.normalized * 6f);
			vector = position + (Vector3)(vector3.normalized * 150f);
			bool flag5 = false;
			float num2 = 20f;
			bool flag6 = (bool)CurrentGun || this is PlayerSpaceshipController;
			bool flag7 = !(CurrentGun == null) && CurrentGun.DefaultModule.shootStyle == ProjectileModule.ShootStyle.Beam;
			if ((GameManager.Options.controllerAimAssistMultiplier > 0f || flag4) && flag6 && (GameManager.Options.controllerBeamAimAssist || !flag7) && CurrentRoom != null && !flag3)
			{
				Vector2 vector4 = unitCenter + vector3.normalized * num2;
				float num3 = (vector4 - unitCenter).ToAngle();
				List<IAutoAimTarget> autoAimTargets = CurrentRoom.GetAutoAimTargets();
				if (CurrentRoom != null && (autoAimTargets != null || GameManager.PVP_ENABLED))
				{
					Projectile projectile = null;
					if ((bool)CurrentGun && CurrentGun.DefaultModule != null)
					{
						projectile = CurrentGun.DefaultModule.GetCurrentProjectile();
					}
					float num4 = ((!projectile) ? float.MaxValue : projectile.baseData.speed);
					if (num4 < 0f)
					{
						num4 = float.MaxValue;
					}
					IAutoAimTarget autoAimTarget = null;
					float num5 = 0f;
					float num6 = 0f;
					int num7 = ((autoAimTargets != null) ? autoAimTargets.Count : 0);
					num7 += (GameManager.PVP_ENABLED ? 1 : 0);
					for (int i = 0; i < num7; i++)
					{
						IAutoAimTarget autoAimTarget2 = null;
						autoAimTarget2 = ((autoAimTargets == null || i >= autoAimTargets.Count) ? GameManager.Instance.GetOtherPlayer(this) : autoAimTargets[i]);
						if (autoAimTarget2 == null || (autoAimTarget2 is Component && !(autoAimTarget2 as Component)) || !autoAimTarget2.IsValid)
						{
							continue;
						}
						Vector2 aimCenter = autoAimTarget2.AimCenter;
						if (!GameManager.Instance.MainCameraController.PointIsVisible(aimCenter, 0.05f))
						{
							continue;
						}
						float num8 = Vector2.Distance(unitCenter, aimCenter) / num4;
						Vector2 vector5 = aimCenter + autoAimTarget2.Velocity * num8;
						float num9 = (vector5 - unitCenter).ToAngle();
						float num10 = Mathf.Abs(BraveMathCollege.ClampAngle180(num9 - num3));
						if (flag && SuperAutoAimTarget == autoAimTarget2)
						{
							num10 *= BraveInput.ControllerAutoAimDegrees / BraveInput.ControllerSuperAutoAimDegrees;
							if (flag7)
							{
								num10 *= 3f;
							}
						}
						else if (flag7)
						{
							num10 *= 2f;
						}
						if (flag4)
						{
							Vector2 vector6 = vector5 - unitCenter;
							float num11 = ((!(vector6 == Vector2.zero)) ? vector6.magnitude : 0f);
							if (!autoAimTarget2.IgnoreForSuperDuperAutoAim && num11 >= autoAimTarget2.MinDistForSuperDuperAutoAim && (autoAimTarget == null || num11 < num6) && (m_superDuperAutoAimTimer <= 0f || autoAimTarget2 == SuperDuperAimTarget))
							{
								RaycastResult result;
								if (!PhysicsEngine.Instance.Raycast(unitCenter, vector6.normalized, vector6.magnitude - 2f, out result, true, false))
								{
									vector = vector5;
									flag5 = true;
									SuperDuperAimPoint = vector;
									autoAimTarget = autoAimTarget2;
									num6 = num11;
								}
								RaycastResult.Pool.Free(ref result);
							}
						}
						else if (num10 < BraveInput.ControllerAutoAimDegrees && (autoAimTarget == null || num10 < num5))
						{
							Vector2 vector7 = vector5 - unitCenter;
							RaycastResult result2;
							if (!PhysicsEngine.Instance.Raycast(unitCenter, vector7.normalized, vector7.magnitude - 2f, out result2, true, false))
							{
								vector = vector5;
								flag5 = true;
								autoAimTarget = autoAimTarget2;
								num5 = num10;
							}
							RaycastResult.Pool.Free(ref result2);
						}
					}
					if (flag4)
					{
						if (!flag5 && m_superDuperAutoAimTimer > 0f)
						{
							vector = SuperDuperAimPoint;
						}
						if (autoAimTarget != SuperDuperAimTarget)
						{
							SuperDuperAimTarget = autoAimTarget;
							m_superDuperAutoAimTimer = 0.5f;
						}
						else if (autoAimTarget == null && m_superDuperAutoAimTimer <= 0f)
						{
							SuperDuperAimTarget = null;
						}
					}
					if (flag)
					{
						if (SuperAutoAimTarget != null && SuperAutoAimTarget != autoAimTarget)
						{
							SuperAutoAimTarget = null;
						}
						else if (SuperAutoAimTarget == null && autoAimTarget != null && flag2)
						{
							SuperAutoAimTarget = autoAimTarget;
						}
					}
				}
			}
		}
		m_cachedAimDirection = vector - position;
		return vector;
	}

	public void ForceStopDodgeRoll()
	{
		m_handlingQueuedAnimation = false;
		m_dodgeRollTimer = rollStats.GetModifiedTime(this);
		ClearDodgeRollState();
		previousMineCart = null;
	}

	private IEnumerator HandleBlinkDodgeRoll()
	{
		if (IsDodgeRolling || (IsFlying && !CanDodgeRollWhileFlying))
		{
			yield break;
		}
		if (this.OnPreDodgeRoll != null)
		{
			this.OnPreDodgeRoll(this);
		}
		if (IsStationary)
		{
			yield break;
		}
		if ((bool)base.knockbackDoer)
		{
			base.knockbackDoer.ClearContinuousKnockbacks();
		}
		m_rollDamagedEnemies.Clear();
		m_dodgeRollTimer = 0f;
		m_dodgeRollState = DodgeRollState.Blink;
		m_currentDodgeRollDepth++;
		if (this.OnRollStarted != null)
		{
			this.OnRollStarted(this, lockedDodgeRollDirection);
		}
		IsEthereal = true;
		IsVisible = false;
		if (IsPrimaryPlayer)
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerOnePosition = true;
			GameManager.Instance.MainCameraController.OverridePlayerOnePosition = base.CenterPosition;
		}
		else
		{
			GameManager.Instance.MainCameraController.UseOverridePlayerTwoPosition = true;
			GameManager.Instance.MainCameraController.OverridePlayerTwoPosition = base.CenterPosition;
		}
		if (CurrentFireMeterValue > 0f)
		{
			CurrentFireMeterValue = Mathf.Max(0f, CurrentFireMeterValue -= 0.5f);
			if (CurrentFireMeterValue == 0f)
			{
				IsOnFire = false;
			}
		}
	}

	private bool CheckDodgeRollDepth()
	{
		if (IsSlidingOverSurface && !DodgeRollIsBlink)
		{
			if (CurrentRoom.IsShop)
			{
				return false;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL)
			{
				return false;
			}
			return true;
		}
		bool flag = PassiveItem.IsFlagSetForCharacter(this, typeof(PegasusBootsItem));
		int num = ((!flag) ? 1 : 2);
		if (flag && HasActiveBonusSynergy(CustomSynergyType.TRIPLE_JUMP))
		{
			num++;
		}
		if (DodgeRollIsBlink)
		{
			num = 1;
		}
		if (IsDodgeRolling && m_currentDodgeRollDepth >= num)
		{
			return false;
		}
		return true;
	}

	private bool StartDodgeRoll(Vector2 direction)
	{
		if (direction == Vector2.zero)
		{
			return false;
		}
		if (!CheckDodgeRollDepth())
		{
			return false;
		}
		if (IsFlying && !CanDodgeRollWhileFlying)
		{
			return false;
		}
		if (this.OnPreDodgeRoll != null)
		{
			this.OnPreDodgeRoll(this);
		}
		if (IsStationary)
		{
			return false;
		}
		if ((bool)base.knockbackDoer)
		{
			base.knockbackDoer.ClearContinuousKnockbacks();
		}
		lockedDodgeRollDirection = direction;
		m_rollDamagedEnemies.Clear();
		base.spriteAnimator.Stop();
		m_dodgeRollTimer = 0f;
		m_dodgeRollState = ((!rollStats.hasPreDodgeDelay) ? DodgeRollState.InAir : DodgeRollState.PreRollDelay);
		m_currentDodgeRollDepth++;
		if (this.OnRollStarted != null)
		{
			this.OnRollStarted(this, lockedDodgeRollDirection);
		}
		if (DodgeRollIsBlink)
		{
			IsEthereal = true;
			IsVisible = false;
			PlayDodgeRollAnimation(direction);
		}
		else
		{
			PlayDodgeRollAnimation(direction);
			if (CurrentGun != null)
			{
				CurrentGun.HandleDodgeroll(rollStats.GetModifiedTime(this));
			}
			if (CurrentGun == null || string.IsNullOrEmpty(CurrentGun.dodgeAnimation))
			{
				ToggleGunRenderers(false, "dodgeroll");
			}
			ToggleHandRenderers(false, "dodgeroll");
		}
		if (CurrentFireMeterValue > 0f)
		{
			CurrentFireMeterValue = Mathf.Max(0f, CurrentFireMeterValue -= 0.5f);
			if (CurrentFireMeterValue == 0f)
			{
				IsOnFire = false;
			}
		}
		return true;
	}

	public bool ForceStartDodgeRoll(Vector2 vec)
	{
		return StartDodgeRoll(vec);
	}

	public bool ForceStartDodgeRoll()
	{
		Vector2 direction = AdjustInputVector(m_activeActions.Move.Vector, BraveInput.MagnetAngles.movementCardinal, BraveInput.MagnetAngles.movementOrdinal);
		return StartDodgeRoll(direction);
	}

	protected bool CanBlinkToPoint(Vector2 point, Vector2 centerOffset)
	{
		bool flag = IsValidPlayerPosition(point + centerOffset);
		if (flag && CurrentRoom != null)
		{
			CellData cellData = GameManager.Instance.Dungeon.data[point.ToIntVector2(VectorConversions.Floor)];
			if (cellData == null)
			{
				return false;
			}
			RoomHandler nearestRoom = cellData.nearestRoom;
			if (cellData.type != CellType.FLOOR)
			{
				flag = false;
			}
			if (CurrentRoom.IsSealed && nearestRoom != CurrentRoom)
			{
				flag = false;
			}
			if (CurrentRoom.IsSealed && cellData.isExitCell)
			{
				flag = false;
			}
			if (nearestRoom.visibility == RoomHandler.VisibilityStatus.OBSCURED || nearestRoom.visibility == RoomHandler.VisibilityStatus.REOBSCURED)
			{
				flag = false;
			}
		}
		if (CurrentRoom == null)
		{
			flag = false;
		}
		return flag;
	}

	protected void UpdateBlinkShadow(Vector2 delta, bool canWarpDirectly)
	{
		if (m_extantBlinkShadow == null)
		{
			GameObject go = new GameObject("blinkshadow");
			m_extantBlinkShadow = tk2dSprite.AddComponent(go, base.sprite.Collection, base.sprite.spriteId);
			m_extantBlinkShadow.transform.position = m_cachedBlinkPosition + (base.sprite.transform.position.XY() - base.specRigidbody.UnitCenter);
			tk2dSpriteAnimator tk2dSpriteAnimator2 = m_extantBlinkShadow.gameObject.AddComponent<tk2dSpriteAnimator>();
			tk2dSpriteAnimator2.Library = base.spriteAnimator.Library;
			m_extantBlinkShadow.renderer.material.SetColor(m_overrideFlatColorID, (!canWarpDirectly) ? new Color(0.4f, 0f, 0f, 1f) : new Color(0.25f, 0.25f, 0.25f, 1f));
			m_extantBlinkShadow.usesOverrideMaterial = true;
			m_extantBlinkShadow.FlipX = base.sprite.FlipX;
			m_extantBlinkShadow.FlipY = base.sprite.FlipY;
			if (OnBlinkShadowCreated != null)
			{
				OnBlinkShadowCreated(m_extantBlinkShadow);
			}
		}
		else
		{
			if (delta == Vector2.zero)
			{
				m_extantBlinkShadow.spriteAnimator.Stop();
				m_extantBlinkShadow.SetSprite(base.sprite.Collection, base.sprite.spriteId);
			}
			else
			{
				string baseAnimationName = GetBaseAnimationName(delta, m_currentGunAngle, false, true);
				if (!string.IsNullOrEmpty(baseAnimationName) && !m_extantBlinkShadow.spriteAnimator.IsPlaying(baseAnimationName))
				{
					m_extantBlinkShadow.spriteAnimator.Play(baseAnimationName);
				}
			}
			m_extantBlinkShadow.renderer.material.SetColor(m_overrideFlatColorID, (!canWarpDirectly) ? new Color(0.4f, 0f, 0f, 1f) : new Color(0.25f, 0.25f, 0.25f, 1f));
			m_extantBlinkShadow.transform.position = m_cachedBlinkPosition + (base.sprite.transform.position.XY() - base.specRigidbody.UnitCenter);
		}
		m_extantBlinkShadow.FlipX = base.sprite.FlipX;
		m_extantBlinkShadow.FlipY = base.sprite.FlipY;
	}

	protected void ClearBlinkShadow()
	{
		if ((bool)m_extantBlinkShadow)
		{
			UnityEngine.Object.Destroy(m_extantBlinkShadow.gameObject);
			m_extantBlinkShadow = null;
		}
	}

	protected bool HandleStartDodgeRoll(Vector2 direction)
	{
		m_handleDodgeRollStartThisFrame = true;
		if (WasPausedThisFrame)
		{
			return false;
		}
		if (!CheckDodgeRollDepth())
		{
			return false;
		}
		if (m_dodgeRollState == DodgeRollState.AdditionalDelay)
		{
			return false;
		}
		if (!DodgeRollIsBlink && direction == Vector2.zero)
		{
			return false;
		}
		rollStats.blinkDistanceMultiplier = 1f;
		if (IsFlying && !CanDodgeRollWhileFlying)
		{
			return false;
		}
		bool flag = false;
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(PlayerIDX);
		if (DodgeRollIsBlink)
		{
			bool flag2 = false;
			if (instanceForPlayer.GetButtonDown(GungeonActions.GungeonActionType.DodgeRoll))
			{
				flag2 = true;
				base.healthHaver.TriggerInvulnerabilityPeriod(0.001f);
				instanceForPlayer.ConsumeButtonDown(GungeonActions.GungeonActionType.DodgeRoll);
			}
			if (instanceForPlayer.ActiveActions.DodgeRollAction.IsPressed)
			{
				m_timeHeldBlinkButton += BraveTime.DeltaTime;
				if (m_timeHeldBlinkButton < 0.2f)
				{
					m_cachedBlinkPosition = base.specRigidbody.UnitCenter;
				}
				else
				{
					Vector2 cachedBlinkPosition = m_cachedBlinkPosition;
					if (IsKeyboardAndMouse())
					{
						m_cachedBlinkPosition = unadjustedAimPoint.XY() - (base.CenterPosition - base.specRigidbody.UnitCenter);
					}
					else
					{
						m_cachedBlinkPosition += m_activeActions.Aim.Vector.normalized * BraveTime.DeltaTime * 15f;
					}
					m_cachedBlinkPosition = BraveMathCollege.ClampToBounds(m_cachedBlinkPosition, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint);
					UpdateBlinkShadow(m_cachedBlinkPosition - cachedBlinkPosition, CanBlinkToPoint(m_cachedBlinkPosition, base.transform.position.XY() - base.specRigidbody.UnitCenter));
				}
			}
			else if (instanceForPlayer.ActiveActions.DodgeRollAction.WasReleased || flag2)
			{
				if (direction != Vector2.zero || m_timeHeldBlinkButton >= 0.2f)
				{
					flag = true;
				}
			}
			else
			{
				m_timeHeldBlinkButton = 0f;
			}
		}
		else if (instanceForPlayer.GetButtonDown(GungeonActions.GungeonActionType.DodgeRoll))
		{
			instanceForPlayer.ConsumeButtonDown(GungeonActions.GungeonActionType.DodgeRoll);
			flag = true;
		}
		if (flag)
		{
			DidUnstealthyAction();
			if (GameManager.Instance.InTutorial)
			{
				GameManager.BroadcastRoomTalkDoerFsmEvent("playerDodgeRoll");
			}
			if (!DodgeRollIsBlink)
			{
				return StartDodgeRoll(direction);
			}
			if (m_timeHeldBlinkButton < 0.2f)
			{
				m_cachedBlinkPosition = base.specRigidbody.UnitCenter + direction.normalized * rollStats.GetModifiedDistance(this);
			}
			BlinkToPoint(m_cachedBlinkPosition);
			m_timeHeldBlinkButton = 0f;
		}
		return false;
	}

	public void BlinkToPoint(Vector2 targetPoint)
	{
		m_cachedBlinkPosition = targetPoint;
		lockedDodgeRollDirection = (m_cachedBlinkPosition - base.specRigidbody.UnitCenter).normalized;
		Vector2 centerOffset = base.transform.position.XY() - base.specRigidbody.UnitCenter;
		if (CanBlinkToPoint(m_cachedBlinkPosition, centerOffset))
		{
			StartCoroutine(HandleBlinkDodgeRoll());
			return;
		}
		Vector2 vector = base.specRigidbody.UnitCenter - m_cachedBlinkPosition;
		float num = vector.magnitude;
		Vector2? vector2 = null;
		float num2 = 0f;
		vector = vector.normalized;
		while (num > 0f)
		{
			num2 += 1f;
			num -= 1f;
			Vector2 vector3 = m_cachedBlinkPosition + vector * num2;
			if (CanBlinkToPoint(vector3, centerOffset))
			{
				vector2 = vector3;
				break;
			}
		}
		if (vector2.HasValue)
		{
			Vector2 normalized = (vector2.Value - base.specRigidbody.UnitCenter).normalized;
			float num3 = Vector2.Dot(normalized, lockedDodgeRollDirection);
			if (num3 > 0f)
			{
				m_cachedBlinkPosition = vector2.Value;
				StartCoroutine(HandleBlinkDodgeRoll());
			}
			else
			{
				ClearBlinkShadow();
			}
		}
	}

	public void DidUnstealthyAction()
	{
		if (this.OnDidUnstealthyAction != null)
		{
			this.OnDidUnstealthyAction(this);
		}
		if (IsPetting && (bool)m_pettingTarget)
		{
			m_pettingTarget.StopPet();
		}
	}

	protected void ContinueDodgeRollAnimation()
	{
		Vector2 vector = lockedDodgeRollDirection;
		vector.Normalize();
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = null;
		tk2dSpriteAnimationClip2 = ((!(Mathf.Abs(vector.x) < 0.1f)) ? base.spriteAnimator.GetClipByName(((!(vector.y > 0.1f)) ? "dodge_left" : "dodge_left_bw") + ((!UseArmorlessAnim) ? string.Empty : "_armorless")) : base.spriteAnimator.GetClipByName(((!(vector.y > 0.1f)) ? "dodge" : "dodge_bw") + ((!UseArmorlessAnim) ? string.Empty : "_armorless")));
		if (tk2dSpriteAnimationClip2 == null)
		{
			return;
		}
		float overrideFps = (float)tk2dSpriteAnimationClip2.frames.Length / rollStats.GetModifiedTime(this);
		base.spriteAnimator.Play(tk2dSpriteAnimationClip2, 0f, overrideFps);
		int num = 0;
		for (int i = 0; i < tk2dSpriteAnimationClip2.frames.Length; i++)
		{
			if (tk2dSpriteAnimationClip2.frames[i].groundedFrame)
			{
				num = i;
				break;
			}
		}
		m_dodgeRollTimer = (float)num / (float)tk2dSpriteAnimationClip2.frames.Length * rollStats.GetModifiedTime(this);
		base.spriteAnimator.SetFrame(num);
		m_handlingQueuedAnimation = true;
	}

	protected virtual void PlayDodgeRollAnimation(Vector2 direction)
	{
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = null;
		direction.Normalize();
		if (m_dodgeRollState != 0)
		{
			tk2dSpriteAnimationClip2 = ((!(Mathf.Abs(direction.x) < 0.1f)) ? base.spriteAnimator.GetClipByName(((!(direction.y > 0.1f)) ? "dodge_left" : "dodge_left_bw") + ((!UseArmorlessAnim) ? string.Empty : "_armorless")) : base.spriteAnimator.GetClipByName(((!(direction.y > 0.1f)) ? "dodge" : "dodge_bw") + ((!UseArmorlessAnim) ? string.Empty : "_armorless")));
			if (IsVisible)
			{
				Vector2 velocity = new Vector2(direction.x, direction.y);
				if (Mathf.Abs(velocity.x) < 0.01f)
				{
					velocity.x = 0f;
				}
				if (Mathf.Abs(velocity.y) < 0.01f)
				{
					velocity.y = 0f;
				}
				if (CustomDodgeRollEffect != null)
				{
					SpawnManager.SpawnVFX(CustomDodgeRollEffect, SpriteBottomCenter, Quaternion.identity);
				}
				else
				{
					GameManager.Instance.Dungeon.dungeonDustups.InstantiateDodgeDustup(velocity, SpriteBottomCenter);
				}
			}
		}
		if (tk2dSpriteAnimationClip2 != null)
		{
			float overrideFps = (float)tk2dSpriteAnimationClip2.frames.Length / rollStats.GetModifiedTime(this);
			base.spriteAnimator.Play(tk2dSpriteAnimationClip2, 0f, overrideFps);
			m_handlingQueuedAnimation = true;
		}
	}

	public void HandleContinueDodgeRoll()
	{
		m_dodgeRollTimer += BraveTime.DeltaTime;
		if (GameManager.Instance.InTutorial && GameManager.Instance.Dungeon.CellIsPit(base.specRigidbody.UnitCenter))
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerDodgeRollOverPit");
		}
		if (m_dodgeRollState == DodgeRollState.PreRollDelay)
		{
			if (m_dodgeRollTimer > rollStats.preDodgeDelay)
			{
				m_dodgeRollState = DodgeRollState.InAir;
				PlayDodgeRollAnimation(lockedDodgeRollDirection);
				m_dodgeRollTimer = BraveTime.DeltaTime;
			}
		}
		else if (m_dodgeRollState == DodgeRollState.InAir)
		{
			bool flag = false;
			if (IsSlidingOverSurface)
			{
				if (m_hasFiredWhileSliding)
				{
					ToggleGunRenderers(true, "dodgeroll");
				}
				flag = true;
				m_dodgeRollTimer -= BraveTime.DeltaTime;
				string text = "slide_right";
				if (lockedDodgeRollDirection.y > 0.1f)
				{
					text = "slide_up";
				}
				if (lockedDodgeRollDirection.y < -0.1f)
				{
					text = "slide_down";
				}
				if (UseArmorlessAnim)
				{
					text += "_armorless";
				}
				if (!base.spriteAnimator.IsPlaying(text) && base.spriteAnimator.GetClipByName(text) != null)
				{
					base.spriteAnimator.Play(text);
				}
				IsSlidingOverSurface = false;
				List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody);
				for (int i = 0; i < overlappingRigidbodies.Count; i++)
				{
					if (base.specRigidbody.Velocity.magnitude < 1f && (bool)overlappingRigidbodies[i].GetComponent<MajorBreakable>())
					{
						overlappingRigidbodies[i].GetComponent<MajorBreakable>().Break(Vector2.zero);
					}
					if ((bool)overlappingRigidbodies[i].GetComponent<SlideSurface>())
					{
						IsSlidingOverSurface = true;
						break;
					}
				}
			}
			if ((!flag || !IsSlidingOverSurface) && ((!DodgeRollIsBlink && !base.spriteAnimator.CurrentClip.name.Contains("dodge")) || QueryGroundedFrame()))
			{
				m_dodgeRollState = DodgeRollState.OnGround;
				DoVibration(Vibration.Time.Quick, Vibration.Strength.UltraLight);
				if (flag)
				{
					m_hasFiredWhileSliding = false;
					TablesDamagedThisSlide.Clear();
					m_dodgeRollTimer = rollStats.GetModifiedTime(this);
					ToggleHandRenderers(true, "dodgeroll");
					ToggleGunRenderers(true, "dodgeroll");
					m_handlingQueuedAnimation = false;
				}
			}
		}
		else if (m_dodgeRollState != DodgeRollState.OnGround && m_dodgeRollState == DodgeRollState.Blink)
		{
			float t = m_dodgeRollTimer / rollStats.GetModifiedTime(this);
			Vector2 vector = base.CenterPosition - base.specRigidbody.UnitCenter;
			if (IsPrimaryPlayer)
			{
				GameManager.Instance.MainCameraController.OverridePlayerOnePosition = Vector2.Lerp(base.specRigidbody.UnitCenter, m_cachedBlinkPosition, t) + vector;
			}
			else
			{
				GameManager.Instance.MainCameraController.OverridePlayerTwoPosition = Vector2.Lerp(base.specRigidbody.UnitCenter, m_cachedBlinkPosition, t) + vector;
			}
		}
	}

	private void ToggleOrbitals(bool value)
	{
		bool flag = value;
		for (int i = 0; i < orbitals.Count; i++)
		{
			orbitals[i].ToggleRenderer(flag);
		}
		for (int j = 0; j < trailOrbitals.Count; j++)
		{
			trailOrbitals[j].ToggleRenderer(flag);
		}
	}

	public void ToggleFollowerRenderers(bool value)
	{
		LastFollowerVisibilityState = value;
		if (orbitals != null)
		{
			for (int i = 0; i < orbitals.Count; i++)
			{
				orbitals[i].ToggleRenderer(value);
			}
		}
		if (trailOrbitals != null)
		{
			for (int j = 0; j < trailOrbitals.Count; j++)
			{
				trailOrbitals[j].ToggleRenderer(value);
			}
		}
		if (companions != null)
		{
			for (int k = 0; k < companions.Count; k++)
			{
				companions[k].ToggleRenderers(value);
			}
		}
	}

	public void ToggleRenderer(bool value, string reason = "")
	{
		if (string.IsNullOrEmpty(reason))
		{
			m_hideRenderers.ClearOverrides();
			if (!value)
			{
				m_hideRenderers.SetOverride("generic", true);
			}
		}
		else
		{
			m_hideRenderers.RemoveOverride("generic");
			m_hideRenderers.SetOverride(reason, !value);
		}
		bool value2 = !m_hideRenderers.Value;
		m_renderer.enabled = value2;
		ToggleAttachedRenderers(value2);
		if ((bool)ShadowObject)
		{
			ShadowObject.GetComponent<Renderer>().enabled = value2;
		}
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, value2);
	}

	private void ToggleAttachedRenderers(bool value)
	{
		for (int i = 0; i < m_attachedSprites.Count; i++)
		{
			m_attachedSprites[i].renderer.enabled = value;
		}
	}

	public void ToggleGunRenderers(bool value, string reason = "")
	{
		if (string.IsNullOrEmpty(reason))
		{
			m_hideGunRenderers.ClearOverrides();
			if (!value)
			{
				m_hideGunRenderers.SetOverride("generic", true);
			}
		}
		else
		{
			m_hideGunRenderers.RemoveOverride("generic");
			m_hideGunRenderers.SetOverride(reason, !value);
		}
		bool value2 = !m_hideGunRenderers.Value;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES && !ArkController.IsResettingPlayers && value)
		{
			value2 = false;
		}
		if (CurrentGun != null)
		{
			CurrentGun.ToggleRenderers(value2);
		}
		if (CurrentSecondaryGun != null)
		{
			CurrentSecondaryGun.ToggleRenderers(value2);
		}
	}

	public void ToggleHandRenderers(bool value, string reason = "")
	{
		if (string.IsNullOrEmpty(reason))
		{
			m_hideHandRenderers.ClearOverrides();
			if (!value)
			{
				m_hideHandRenderers.SetOverride("generic", true);
			}
		}
		else
		{
			m_hideHandRenderers.RemoveOverride("generic");
			m_hideHandRenderers.SetOverride(reason, !value);
		}
		bool flag = !m_hideHandRenderers.Value;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES && !ArkController.IsResettingPlayers && value)
		{
			flag = false;
		}
		primaryHand.ForceRenderersOff = !flag;
		secondaryHand.ForceRenderersOff = !flag;
		if ((bool)CurrentGun)
		{
			if (CurrentGun.additionalHandState == AdditionalHandState.HideBoth)
			{
				primaryHand.ForceRenderersOff = true;
				secondaryHand.ForceRenderersOff = true;
			}
			else if (CurrentGun.additionalHandState == AdditionalHandState.HidePrimary)
			{
				primaryHand.ForceRenderersOff = true;
			}
			else if (CurrentGun.additionalHandState == AdditionalHandState.HideSecondary)
			{
				secondaryHand.ForceRenderersOff = true;
			}
		}
	}

	protected virtual void HandleFlipping(float gunAngle)
	{
		bool flag = false;
		if (CurrentGun == null)
		{
			gunAngle = BraveMathCollege.Atan2Degrees((!(m_playerCommandedDirection == Vector2.zero)) ? m_playerCommandedDirection : m_lastNonzeroCommandedDirection);
		}
		if (IsGhost)
		{
			gunAngle = 0f;
		}
		if (!IsSlidingOverSurface)
		{
			if (IsDodgeRolling)
			{
				if (lockedDodgeRollDirection.x < -0.1f)
				{
					gunAngle = 180f;
				}
				else if (lockedDodgeRollDirection.x > 0.1f)
				{
					gunAngle = 0f;
				}
			}
			else if (IsPetting)
			{
				gunAngle = m_petDirection;
			}
			else if (m_handlingQueuedAnimation && !m_overrideGunAngle.HasValue)
			{
				return;
			}
		}
		float num = 75f;
		float num2 = 105f;
		if (gunAngle <= 155f && gunAngle >= 25f)
		{
			num = 75f;
			num2 = 105f;
		}
		if (!SpriteFlipped && Mathf.Abs(gunAngle) > num2)
		{
			base.sprite.FlipX = true;
			base.sprite.gameObject.transform.localPosition = new Vector3(m_spriteDimensions.x, 0f, 0f);
			if (CurrentGun != null)
			{
				CurrentGun.HandleSpriteFlip(true);
			}
			if (CurrentSecondaryGun != null)
			{
				CurrentSecondaryGun.HandleSpriteFlip(true);
			}
			flag = true;
		}
		else if (SpriteFlipped && Mathf.Abs(gunAngle) < num)
		{
			base.sprite.FlipX = false;
			base.sprite.gameObject.transform.localPosition = Vector3.zero;
			if (CurrentGun != null)
			{
				CurrentGun.HandleSpriteFlip(false);
			}
			if (CurrentSecondaryGun != null)
			{
				CurrentSecondaryGun.HandleSpriteFlip(false);
			}
			flag = true;
		}
		if (CurrentGun != null)
		{
			HandleGunDepthInternal(CurrentGun, gunAngle);
		}
		if (CurrentSecondaryGun != null)
		{
			HandleGunDepthInternal(CurrentSecondaryGun, gunAngle, true);
		}
		base.sprite.UpdateZDepth();
		if (flag)
		{
			ProcessHandAttachment();
		}
	}

	private void HandleGunDepthInternal(Gun targetGun, float gunAngle, bool isSecondary = false)
	{
		tk2dBaseSprite tk2dBaseSprite2 = targetGun.GetSprite();
		if (targetGun.preventRotation)
		{
			tk2dBaseSprite2.HeightOffGround = 0.4f;
		}
		else if (targetGun.usesDirectionalIdleAnimations)
		{
			float heightOffGround = -0.075f;
			if ((gunAngle > 0f && gunAngle <= 155f && gunAngle >= 25f) || (gunAngle <= -60f && gunAngle >= -120f))
			{
				heightOffGround = 0.075f;
			}
			tk2dBaseSprite2.HeightOffGround = heightOffGround;
		}
		else if (gunAngle > 0f && gunAngle <= 155f && gunAngle >= 25f)
		{
			tk2dBaseSprite2.HeightOffGround = -0.075f;
		}
		else
		{
			float heightOffGround2 = ((targetGun.Handedness != GunHandedness.TwoHanded) ? (-0.075f) : 0.075f);
			if (isSecondary)
			{
				heightOffGround2 = 0.075f;
			}
			tk2dBaseSprite2.HeightOffGround = heightOffGround2;
		}
		tk2dBaseSprite2.UpdateZDepth();
	}

	private float GetDodgeRollSpeed()
	{
		if (m_dodgeRollState == DodgeRollState.PreRollDelay)
		{
			return 0f;
		}
		float time = Mathf.Clamp01((m_dodgeRollTimer - BraveTime.DeltaTime) / rollStats.GetModifiedTime(this));
		float time2 = Mathf.Clamp01(m_dodgeRollTimer / rollStats.GetModifiedTime(this));
		float num = (Mathf.Clamp01(rollStats.speed.Evaluate(time2)) - Mathf.Clamp01(rollStats.speed.Evaluate(time))) * rollStats.GetModifiedDistance(this);
		return num / BraveTime.DeltaTime;
	}

	public void ProcessHandAttachment()
	{
		if (CurrentGun == null)
		{
			primaryHand.attachPoint = null;
			secondaryHand.attachPoint = null;
			return;
		}
		if (inventory.DualWielding && CurrentSecondaryGun != null)
		{
			primaryHand.attachPoint = ((!CurrentGun.IsPreppedForThrow) ? CurrentGun.PrimaryHandAttachPoint : CurrentGun.ThrowPrepTransform);
			secondaryHand.attachPoint = ((!CurrentSecondaryGun.IsPreppedForThrow) ? CurrentSecondaryGun.PrimaryHandAttachPoint : CurrentSecondaryGun.ThrowPrepTransform);
		}
		else if (CurrentGun.Handedness == GunHandedness.NoHanded)
		{
			primaryHand.attachPoint = null;
			secondaryHand.attachPoint = null;
		}
		else
		{
			if (CurrentGun.Handedness != GunHandedness.HiddenOneHanded)
			{
				primaryHand.attachPoint = ((!CurrentGun.IsPreppedForThrow) ? CurrentGun.PrimaryHandAttachPoint : CurrentGun.ThrowPrepTransform);
			}
			else
			{
				primaryHand.attachPoint = null;
			}
			if (CurrentGun.Handedness == GunHandedness.TwoHanded)
			{
				secondaryHand.attachPoint = CurrentGun.SecondaryHandAttachPoint;
			}
			else
			{
				secondaryHand.attachPoint = null;
			}
		}
		if (CurrentGun.additionalHandState == AdditionalHandState.None)
		{
			return;
		}
		switch (CurrentGun.additionalHandState)
		{
		case AdditionalHandState.HidePrimary:
			if ((bool)primaryHand)
			{
				primaryHand.attachPoint = null;
			}
			break;
		case AdditionalHandState.HideSecondary:
			if ((bool)secondaryHand)
			{
				secondaryHand.attachPoint = null;
			}
			break;
		case AdditionalHandState.HideBoth:
			if ((bool)primaryHand)
			{
				primaryHand.attachPoint = null;
			}
			if ((bool)secondaryHand)
			{
				secondaryHand.attachPoint = null;
			}
			break;
		}
	}

	private void HandleGunUnequipInternal(Gun previous)
	{
		if (previous != null)
		{
			tk2dBaseSprite tk2dBaseSprite2 = previous.GetSprite();
			base.sprite.DetachRenderer(tk2dBaseSprite2);
			tk2dBaseSprite2.DetachRenderer(primaryHand.sprite);
			tk2dBaseSprite2.DetachRenderer(secondaryHand.sprite);
			SpriteOutlineManager.RemoveOutlineFromSprite(previous.GetComponent<tk2dSprite>());
		}
	}

	private void HandleGunEquipInternal(Gun current, PlayerHandController hand)
	{
		if (current != null)
		{
			tk2dBaseSprite tk2dBaseSprite2 = current.GetSprite();
			base.sprite.AttachRenderer(tk2dBaseSprite2);
			tk2dBaseSprite2.AttachRenderer(hand.sprite);
			if (!inventory.DualWielding && (!RenderBodyHand || current.IsTrickGun))
			{
				tk2dBaseSprite2.AttachRenderer(secondaryHand.sprite);
			}
			if (!current.PreventOutlines)
			{
				SpriteOutlineManager.AddOutlineToSprite(current.GetComponent<tk2dSprite>(), outlineColor, 0.2f, 0.05f);
			}
			current.ToggleRenderers(!m_hideGunRenderers.Value);
		}
	}

	private void OnGunChanged(Gun previous, Gun current, Gun previousSecondary, Gun currentSecondary, bool newGun)
	{
		HandleGunUnequipInternal(previous);
		HandleGunUnequipInternal(previousSecondary);
		HandleGunEquipInternal(current, primaryHand);
		HandleGunEquipInternal(currentSecondary, secondaryHand);
		HandleGunAttachPoint();
		ProcessHandAttachment();
		stats.RecalculateStats(this);
		if ((bool)current && current.ammo > current.AdjustedMaxAmmo)
		{
			ArtfulDodgerGunController component = current.GetComponent<ArtfulDodgerGunController>();
			if (!component)
			{
				current.ammo = current.AdjustedMaxAmmo;
			}
		}
		if (this.GunChanged != null)
		{
			this.GunChanged(previous, current, newGun);
		}
	}

	protected Vector2 AdjustInputVector(Vector2 rawInput, float cardinalMagnetAngle, float ordinalMagnetAngle)
	{
		float num = BraveMathCollege.ClampAngle360(BraveMathCollege.Atan2Degrees(rawInput));
		float num2 = num % 90f;
		float num3 = (num + 45f) % 90f;
		float num4 = 0f;
		if (cardinalMagnetAngle > 0f)
		{
			if (num2 < cardinalMagnetAngle)
			{
				num4 = 0f - num2;
			}
			else if (num2 > 90f - cardinalMagnetAngle)
			{
				num4 = 90f - num2;
			}
		}
		if (ordinalMagnetAngle > 0f)
		{
			if (num3 < ordinalMagnetAngle)
			{
				num4 = 0f - num3;
			}
			else if (num3 > 90f - ordinalMagnetAngle)
			{
				num4 = 90f - num3;
			}
		}
		num += num4;
		return (Quaternion.Euler(0f, 0f, num) * Vector3.right).XY() * rawInput.magnitude;
	}

	protected void ProcessDebugInput()
	{
	}

	public void ForceMoveToPoint(Vector2 targetPosition, float initialDelay = 0f, float maximumTime = 2f)
	{
		StartCoroutine(HandleForcedMove(targetPosition, false, false, initialDelay, maximumTime));
	}

	public void ForceMoveInDirectionUntilThreshold(Vector2 direction, float axialThreshold, float initialDelay = 0f, float maximumTime = 1f, List<SpeculativeRigidbody> passThroughRigidbodies = null)
	{
		Vector2 centerPosition = base.CenterPosition;
		bool axialX = false;
		bool axialY = false;
		if (!Mathf.Approximately(direction.x, 0f))
		{
			centerPosition.x = axialThreshold;
			axialX = true;
		}
		if (!Mathf.Approximately(direction.y, 0f))
		{
			centerPosition.y = axialThreshold;
			axialY = true;
		}
		StartCoroutine(HandleForcedMove(centerPosition, axialX, axialY, initialDelay, maximumTime, passThroughRigidbodies));
	}

	private IEnumerator HandleForcedMove(Vector2 targetPoint, bool axialX, bool axialY, float initialDelay = 0f, float maximumTime = 1f, List<SpeculativeRigidbody> passThroughRigidbodies = null)
	{
		usingForcedInput = true;
		if (initialDelay > 0f)
		{
			while (initialDelay > 0f)
			{
				forcedInput = Vector2.zero;
				initialDelay -= GameManager.INVARIANT_DELTA_TIME;
				yield return null;
			}
		}
		if (passThroughRigidbodies != null)
		{
			for (int i = 0; i < passThroughRigidbodies.Count; i++)
			{
				base.specRigidbody.RegisterSpecificCollisionException(passThroughRigidbodies[i]);
			}
		}
		Vector2 startPosition = base.CenterPosition;
		float elapsed = 0f;
		while (usingForcedInput)
		{
			Vector2 dirVec = targetPoint - base.CenterPosition;
			if (axialX != axialY)
			{
				if (axialX)
				{
					dirVec = targetPoint - base.CenterPosition.WithY(targetPoint.y);
					if (Vector2.Dot(dirVec, targetPoint - startPosition.WithY(targetPoint.y)) < 0f)
					{
						break;
					}
				}
				else
				{
					dirVec = targetPoint - base.CenterPosition.WithX(targetPoint.x);
					if (Vector2.Dot(dirVec, targetPoint - startPosition.WithX(targetPoint.x)) < 0f)
					{
						break;
					}
				}
			}
			else if (Vector2.Dot(dirVec, targetPoint - startPosition) < 0f)
			{
				break;
			}
			forcedInput = dirVec.normalized;
			float clampedDeltaTime = Mathf.Clamp((!(Time.timeScale <= 0f)) ? BraveTime.DeltaTime : GameManager.INVARIANT_DELTA_TIME, 0f, 0.1f);
			elapsed += clampedDeltaTime;
			if (elapsed > maximumTime)
			{
				break;
			}
			yield return null;
		}
		if (passThroughRigidbodies != null)
		{
			for (int j = 0; j < passThroughRigidbodies.Count; j++)
			{
				base.specRigidbody.DeregisterSpecificCollisionException(passThroughRigidbodies[j]);
			}
		}
		usingForcedInput = false;
		forcedInput = Vector2.zero;
	}

	public bool IsQuickEquipGun(Gun gunToCheck)
	{
		return gunToCheck == m_cachedQuickEquipGun || gunToCheck == CurrentGun;
	}

	public void DoQuickEquip()
	{
		if (GameManager.Options.QuickSelectEnabled)
		{
			if (m_cachedQuickEquipGun != null && inventory.AllGuns.Contains(m_cachedQuickEquipGun) && CurrentGun != m_cachedQuickEquipGun)
			{
				Gun cachedQuickEquipGun = m_cachedQuickEquipGun;
				CacheQuickEquipGun();
				int num = inventory.AllGuns.IndexOf(cachedQuickEquipGun);
				int change = num - inventory.AllGuns.IndexOf(CurrentGun);
				ChangeGun(change);
				m_equippedGunShift = -1;
			}
			else if (CurrentGun == m_cachedQuickEquipGun && m_backupCachedQuickEquipGun != null && inventory.AllGuns.Contains(m_backupCachedQuickEquipGun) && CurrentGun != m_backupCachedQuickEquipGun)
			{
				Gun backupCachedQuickEquipGun = m_backupCachedQuickEquipGun;
				CacheQuickEquipGun();
				int num2 = inventory.AllGuns.IndexOf(backupCachedQuickEquipGun);
				int change2 = num2 - inventory.AllGuns.IndexOf(CurrentGun);
				ChangeGun(change2);
				m_equippedGunShift = -1;
			}
			else
			{
				ChangeGun(-1);
			}
		}
		else
		{
			ChangeGun(-1);
		}
	}

	protected virtual Vector2 HandlePlayerInput()
	{
		exceptionTracker = 0;
		if (m_activeActions == null)
		{
			return Vector2.zero;
		}
		Vector2 vector = Vector2.zero;
		if (CurrentInputState != PlayerInputState.NoMovement)
		{
			vector = AdjustInputVector(m_activeActions.Move.Vector, BraveInput.MagnetAngles.movementCardinal, BraveInput.MagnetAngles.movementOrdinal);
		}
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		HandleStartDodgeRoll(vector);
		CollisionData result = null;
		if (vector.x > 0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Right, out result, true, false))
		{
			vector.x = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (vector.x < -0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Left, out result, true, false))
		{
			vector.x = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (vector.y > 0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Up, out result, true, false))
		{
			vector.y = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (vector.y < -0.01f && PhysicsEngine.Instance.RigidbodyCast(base.specRigidbody, IntVector2.Down, out result, true, false))
		{
			vector.y = 0f;
		}
		CollisionData.Pool.Free(ref result);
		if (IsGhost)
		{
			GameOptions.ControllerBlankControl controllerBlankControl = ((!IsPrimaryPlayer) ? GameManager.Options.additionalBlankControlTwo : GameManager.Options.additionalBlankControl);
			bool flag = controllerBlankControl == GameOptions.ControllerBlankControl.BOTH_STICKS_DOWN && m_activeActions.CheckBothSticksButton();
			if (Time.timeScale > 0f)
			{
				bool flag2 = false;
				if (m_activeActions.Device != null)
				{
					flag2 |= m_activeActions.Device.Action1.WasPressed || m_activeActions.Device.Action2.WasPressed || m_activeActions.Device.Action3.WasPressed || m_activeActions.Device.Action4.WasPressed || m_activeActions.MenuSelectAction.WasPressed;
				}
				if (IsKeyboardAndMouse() && Input.GetMouseButtonDown(0))
				{
					flag2 = true;
				}
				if (m_blankCooldownTimer <= 0f && (flag2 || m_activeActions.ShootAction.WasPressed || m_activeActions.UseItemAction.WasPressed || m_activeActions.BlankAction.WasPressed || flag))
				{
					DoGhostBlank();
				}
			}
			return vector;
		}
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(PlayerIDX);
		if (AcceptingNonMotionInput)
		{
			if (IsKeyboardAndMouse() && !GameManager.Options.DisableQuickGunKeys)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1))
				{
					ChangeToGunSlot(0);
				}
				if (Input.GetKeyDown(KeyCode.Alpha2))
				{
					ChangeToGunSlot(1);
				}
				if (Input.GetKeyDown(KeyCode.Alpha3))
				{
					ChangeToGunSlot(2);
				}
				if (Input.GetKeyDown(KeyCode.Alpha4))
				{
					ChangeToGunSlot(3);
				}
				if (Input.GetKeyDown(KeyCode.Alpha5))
				{
					ChangeToGunSlot(4);
				}
				if (Input.GetKeyDown(KeyCode.Alpha6))
				{
					ChangeToGunSlot(5);
				}
				if (Input.GetKeyDown(KeyCode.Alpha7))
				{
					ChangeToGunSlot(6);
				}
				if (Input.GetKeyDown(KeyCode.Alpha8))
				{
					ChangeToGunSlot(7);
				}
				if (Input.GetKeyDown(KeyCode.Alpha9))
				{
					ChangeToGunSlot(8);
				}
				if (Input.GetKeyDown(KeyCode.Alpha0))
				{
					ChangeToGunSlot(9);
				}
			}
			m_equippedGunShift = 0;
			if (!m_gunWasDropped && !GameUIRoot.Instance.MetalGearActive && !Minimap.Instance.IsFullscreen)
			{
				if (m_activeActions.GunDownAction.WasReleased)
				{
					if (!m_gunChangePressedWhileMetalGeared)
					{
						ChangeGun(1);
					}
					m_gunChangePressedWhileMetalGeared = false;
				}
				if (m_activeActions.GunUpAction.WasReleased)
				{
					if (!m_gunChangePressedWhileMetalGeared)
					{
						ChangeGun(-1);
					}
					m_gunChangePressedWhileMetalGeared = false;
				}
				if (inventory.DualWielding && m_activeActions.SwapDualGunsAction.WasPressed)
				{
					inventory.SwapDualGuns();
				}
				if (m_activeActions.GunQuickEquipAction.WasReleased)
				{
					bool flag3 = true;
					DoQuickEquip();
				}
			}
			if ((m_activeActions.GunQuickEquipAction.IsPressed || ForceMetalGearMenu) && !GameManager.IsBossIntro && !Minimap.Instance.IsFullscreen && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
			{
				m_metalGearFrames++;
				m_metalGearTimer += GameManager.INVARIANT_DELTA_TIME;
				float num = 0.175f;
				if (m_metalGearTimer > num && !m_metalWasGeared)
				{
					m_metalWasGeared = true;
					m_metalGearTimer = 0f;
					m_metalGearFrames = 0;
					GameUIRoot.Instance.TriggerMetalGearGunSelect(this);
				}
			}
			else
			{
				m_metalWasGeared = false;
				m_metalGearTimer = 0f;
				m_metalGearFrames = 0;
			}
			if (m_activeActions.DropGunAction.IsPressed && CurrentGun != null && inventory.AllGuns.Count > 1 && !inventory.GunLocked.Value && CurrentGun.CanActuallyBeDropped(this) && !m_gunWasDropped)
			{
				m_dropGunTimer += BraveTime.DeltaTime;
				if (m_dropGunTimer > 0.5f)
				{
					m_gunWasDropped = true;
					m_dropGunTimer = 0f;
					ForceDropGun(CurrentGun);
					DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
				}
			}
			else if (!m_activeActions.DropGunAction.IsPressed)
			{
				m_gunWasDropped = false;
				m_dropGunTimer = 0f;
			}
			if (!m_itemWasDropped)
			{
				if (m_activeActions.ItemUpAction.WasReleased)
				{
					ChangeItem(1);
				}
				else if (m_activeActions.ItemDownAction.WasReleased)
				{
					ChangeItem(-1);
				}
			}
			if (m_activeActions.DropItemAction.IsPressed && CurrentItem != null && CurrentItem.CanActuallyBeDropped(this) && !m_itemWasDropped && !m_preventItemSwitching)
			{
				m_dropItemTimer += BraveTime.DeltaTime;
				if (m_dropItemTimer > 0.5f)
				{
					m_itemWasDropped = true;
					m_dropItemTimer = 0f;
					DropActiveItem(CurrentItem);
					DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
				}
			}
			else if (!m_activeActions.DropItemAction.IsPressed)
			{
				m_itemWasDropped = false;
				m_dropItemTimer = 0f;
			}
			if (m_activeActions.ReloadAction.WasPressed && CurrentGun != null)
			{
				CurrentGun.Reload();
				if (CurrentGun.OnReloadPressed != null)
				{
					CurrentGun.OnReloadPressed(this, CurrentGun, true);
				}
				if ((bool)CurrentSecondaryGun)
				{
					CurrentSecondaryGun.Reload();
					if (CurrentSecondaryGun.OnReloadPressed != null)
					{
						CurrentSecondaryGun.OnReloadPressed(this, CurrentSecondaryGun, true);
					}
				}
				if (OnReloadPressed != null)
				{
					OnReloadPressed(this, CurrentGun);
				}
			}
			bool buttonDown = instanceForPlayer.GetButtonDown(GungeonActions.GungeonActionType.UseItem);
			bool flag4 = true;
			if (buttonDown && (!IsDodgeRolling || ((bool)CurrentItem && CurrentItem.usableDuringDodgeRoll)))
			{
				UseItem();
				if (flag4)
				{
					instanceForPlayer.ConsumeButtonDown(GungeonActions.GungeonActionType.UseItem);
				}
			}
			GameOptions.ControllerBlankControl controllerBlankControl2 = ((!IsPrimaryPlayer) ? GameManager.Options.additionalBlankControlTwo : GameManager.Options.additionalBlankControl);
			bool flag5 = controllerBlankControl2 == GameOptions.ControllerBlankControl.BOTH_STICKS_DOWN && m_activeActions.CheckBothSticksButton();
			if (Time.timeScale > 0f && m_blankCooldownTimer <= 0f && (m_activeActions.BlankAction.WasPressed || flag5))
			{
				DoConsumableBlank();
			}
			if (Minimap.Instance != null && !GameUIRoot.Instance.MetalGearActive)
			{
				bool wasPressed = m_activeActions.MapAction.WasPressed;
				bool holdOpen = false;
				if (wasPressed)
				{
					Minimap.Instance.ToggleMinimap(true, holdOpen);
				}
			}
		}
		if (CurrentInputState == PlayerInputState.AllInput || CurrentInputState == PlayerInputState.FoyerInputOnly)
		{
			IPlayerInteractable playerInteractable = null;
			if (m_currentRoom != null)
			{
				playerInteractable = m_currentRoom.GetNearestInteractable(base.CenterPosition, 1f, this);
			}
			if (playerInteractable != m_lastInteractionTarget || ForceRefreshInteractable)
			{
				exceptionTracker = 100;
				if (m_lastInteractionTarget is MonoBehaviour && !(m_lastInteractionTarget as MonoBehaviour))
				{
					m_lastInteractionTarget = null;
				}
				exceptionTracker = 101;
				if (m_lastInteractionTarget != null)
				{
					m_lastInteractionTarget.OnExitRange(this);
					exceptionTracker = 102;
					if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && m_lastInteractionTarget != null)
					{
						for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
						{
							exceptionTracker = 103;
							PlayerController playerController = GameManager.Instance.AllPlayers[i];
							if ((bool)playerController && !(playerController == this) && !playerController.healthHaver.IsDead && playerController.CurrentRoom != null)
							{
								exceptionTracker = 104;
								if (m_lastInteractionTarget == playerController.CurrentRoom.GetNearestInteractable(playerController.CenterPosition, 1f, playerController))
								{
									m_lastInteractionTarget.OnEnteredRange(playerController);
								}
								exceptionTracker = 105;
							}
						}
					}
				}
				if (playerInteractable != null)
				{
					playerInteractable.OnEnteredRange(this);
				}
				m_lastInteractionTarget = playerInteractable;
			}
			if (playerInteractable != null && m_activeActions.InteractAction.WasPressed)
			{
				if (IsDodgeRolling)
				{
					ToggleGunRenderers(true, "dodgeroll");
					ToggleHandRenderers(true, "dodgeroll");
				}
				GameUIRoot.Instance.levelNameUI.BanishLevelNameText();
				bool shouldBeFlipped;
				string text = playerInteractable.GetAnimationState(this, out shouldBeFlipped);
				playerInteractable.Interact(this);
				if (IsSlidingOverSurface)
				{
					text = string.Empty;
				}
				if (!(playerInteractable is ShopItemController))
				{
					DidUnstealthyAction();
				}
				if (text != string.Empty)
				{
					HandleFlipping(shouldBeFlipped ? 180 : 0);
					m_handlingQueuedAnimation = true;
					string text2 = ((!(CurrentGun == null) || ForceHandless) ? "_hand" : "_twohands");
					string text3 = ((!UseArmorlessAnim) ? string.Empty : "_armorless");
					if (RenderBodyHand && base.spriteAnimator.GetClipByName(text + text2 + text3) != null)
					{
						base.spriteAnimator.Play(text + text2 + text3);
					}
					else if (base.spriteAnimator.GetClipByName(text + text3) != null)
					{
						base.spriteAnimator.Play(text + text3);
					}
					m_overrideGunAngle = (shouldBeFlipped ? 180 : 0);
				}
			}
			else if (playerInteractable == null && m_activeActions.InteractAction.WasPressed && !IsPetting && !IsInCombat && !IsDodgeRolling && !m_handlingQueuedAnimation)
			{
				List<AIActor> allEnemies = StaticReferenceManager.AllEnemies;
				for (int j = 0; j < allEnemies.Count; j++)
				{
					AIActor aIActor = allEnemies[j];
					if ((bool)aIActor && !aIActor.IsNormalEnemy && (bool)aIActor.CompanionOwner)
					{
						CompanionController component = aIActor.GetComponent<CompanionController>();
						if (component.CanBePet && Vector2.Distance(base.CenterPosition, component.specRigidbody.GetUnitCenter(ColliderType.HitBox)) <= 2.5f)
						{
							component.DoPet(this);
							base.spriteAnimator.Play("pet");
							ToggleGunRenderers(false, "petting");
							ToggleHandRenderers(false, "petting");
							m_petDirection = ((!(aIActor.specRigidbody.UnitCenter.x > base.specRigidbody.UnitCenter.x)) ? 180 : 0);
							m_pettingTarget = component;
							break;
						}
					}
				}
			}
		}
		ForceRefreshInteractable = false;
		if (AcceptingNonMotionInput || CurrentInputState == PlayerInputState.FoyerInputOnly)
		{
			Vector2 vector2 = DetermineAimPointInWorld();
			if (CurrentGun != null)
			{
				m_currentGunAngle = CurrentGun.HandleAimRotation(vector2);
				if ((bool)CurrentSecondaryGun)
				{
					CurrentSecondaryGun.HandleAimRotation(vector2);
				}
			}
			if (m_overrideGunAngle.HasValue)
			{
				m_currentGunAngle = m_overrideGunAngle.Value;
				gunAttachPoint.localRotation = Quaternion.Euler(gunAttachPoint.localRotation.x, gunAttachPoint.localRotation.y, m_currentGunAngle);
			}
			else
			{
				m_currentGunAngle = (vector2 - base.specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
			}
		}
		if (AcceptingNonMotionInput)
		{
			base.sprite.UpdateZDepth();
			if (inventory.DualWielding && (bool)CurrentSecondaryGun)
			{
				HandleGunFiringInternal(CurrentSecondaryGun, instanceForPlayer, true);
			}
			HandleGunFiringInternal(CurrentGun, instanceForPlayer, false);
		}
		else if (CurrentInputState == PlayerInputState.OnlyMovement && CurrentGun != null && CurrentGun.IsCharging && instanceForPlayer.GetButton(GungeonActions.GungeonActionType.Shoot) && m_shouldContinueFiring)
		{
			CurrentGun.ContinueAttack(m_CanAttack);
		}
		return vector;
	}

	private void HandleGunFiringInternal(Gun targetGun, BraveInput currentBraveInput, bool isSecondary)
	{
		if (!(targetGun != null))
		{
			return;
		}
		bool flag = currentBraveInput.GetButtonDown(GungeonActions.GungeonActionType.Shoot) || forceFireDown;
		if (this.OnTriedToInitiateAttack != null && flag)
		{
			this.OnTriedToInitiateAttack(this);
		}
		if (SuppressThisClick)
		{
			exceptionTracker = 200;
			while (currentBraveInput.GetButtonDown(GungeonActions.GungeonActionType.Shoot))
			{
				currentBraveInput.ConsumeButtonDown(GungeonActions.GungeonActionType.Shoot);
				if (currentBraveInput.GetButtonUp(GungeonActions.GungeonActionType.Shoot))
				{
					currentBraveInput.ConsumeButtonUp(GungeonActions.GungeonActionType.Shoot);
				}
			}
			exceptionTracker = 201;
			if (!currentBraveInput.GetButton(GungeonActions.GungeonActionType.Shoot))
			{
				SuppressThisClick = false;
			}
		}
		else if (m_CanAttack && flag)
		{
			exceptionTracker = 202;
			bool flag2 = false;
			Gun.AttackResult attackResult = targetGun.Attack();
			flag2 = flag2 || (attackResult != Gun.AttackResult.Fail && attackResult != Gun.AttackResult.OnCooldown);
			m_newFloorNoInput = false;
			exceptionTracker = 203;
			if (!HasFiredNonStartingGun && attackResult == Gun.AttackResult.Success && !targetGun.StarterGunForAchievement)
			{
				HasFiredNonStartingGun = true;
			}
			m_shouldContinueFiring = true;
			IsFiring = attackResult == Gun.AttackResult.Success && !targetGun.IsCharging;
			exceptionTracker = 204;
			if (attackResult == Gun.AttackResult.Success)
			{
				DidUnstealthyAction();
			}
			if (flag2 && !isSecondary)
			{
				currentBraveInput.ConsumeButtonDown(GungeonActions.GungeonActionType.Shoot);
			}
			m_controllerSemiAutoTimer = 0f;
		}
		else if ((currentBraveInput.GetButtonUp(GungeonActions.GungeonActionType.Shoot) || forceFireUp) && !KeepChargingDuringRoll)
		{
			exceptionTracker = 205;
			IsFiring = targetGun.CeaseAttack(m_CanAttack);
			if (!isSecondary)
			{
				currentBraveInput.ConsumeButtonUp(GungeonActions.GungeonActionType.Shoot);
			}
			m_shouldContinueFiring = false;
		}
		else if ((currentBraveInput.GetButton(GungeonActions.GungeonActionType.Shoot) || forceFire || KeepChargingDuringRoll) && m_shouldContinueFiring)
		{
			exceptionTracker = 206;
			bool flag3 = IsDodgeRolling && !IsSlidingOverSurface;
			if (IsSlidingOverSurface)
			{
				m_hasFiredWhileSliding = true;
			}
			if (UseFakeSemiAutoCooldown && targetGun.DefaultModule.shootStyle == ProjectileModule.ShootStyle.SemiAutomatic && !targetGun.HasShootStyle(ProjectileModule.ShootStyle.Charged) && !flag3 && targetGun.CurrentAmmo > 0)
			{
				m_controllerSemiAutoTimer += BraveTime.DeltaTime;
				if (m_controllerSemiAutoTimer > BraveInput.ControllerFakeSemiAutoCooldown && !targetGun.IsEmpty && m_CanAttack)
				{
					exceptionTracker = 207;
					targetGun.CeaseAttack(false);
					if (targetGun.Attack() == Gun.AttackResult.Success)
					{
						m_controllerSemiAutoTimer = 0f;
						IsFiring = !targetGun.IsCharging;
					}
				}
				else
				{
					exceptionTracker = 208;
					bool flag4 = targetGun.ContinueAttack(m_CanAttack);
					IsFiring = flag4 && !targetGun.IsCharging;
				}
			}
			else
			{
				exceptionTracker = 209;
				bool flag5 = targetGun.ContinueAttack(m_CanAttack);
				IsFiring = flag5 && !targetGun.IsCharging;
			}
			exceptionTracker = 210;
			if (!targetGun.IsReloading)
			{
				DidUnstealthyAction();
			}
		}
		else if (targetGun.IsFiring || targetGun.IsPreppedForThrow)
		{
			exceptionTracker = 211;
			IsFiring = targetGun.CeaseAttack(m_CanAttack);
			m_shouldContinueFiring = false;
		}
		if (IsFiring)
		{
			m_isThreatArrowing = false;
			m_elapsedNonalertTime = 0f;
		}
	}

	public void RemoveBrokenInteractable(IPlayerInteractable ixable)
	{
		if (m_lastInteractionTarget == ixable)
		{
			m_lastInteractionTarget.OnExitRange(this);
			m_lastInteractionTarget = null;
		}
	}

	private void ChangeItem(int change)
	{
		if (!m_preventItemSwitching)
		{
			if (activeItems.Count > 1)
			{
				CurrentItem.OnItemSwitched(this);
				m_selectedItemIndex += change;
				int num = (m_selectedItemIndex = (m_selectedItemIndex + activeItems.Count) % activeItems.Count);
			}
			else
			{
				m_selectedItemIndex = 0;
			}
			if (!EncounterTrackable.SuppressNextNotification)
			{
				GameUIRoot.Instance.TemporarilyShowItemName(IsPrimaryPlayer);
			}
		}
	}

	public void CacheQuickEquipGun()
	{
		m_backupCachedQuickEquipGun = m_cachedQuickEquipGun;
		m_cachedQuickEquipGun = CurrentGun;
	}

	public void ChangeToGunSlot(int slotIndex, bool overrideGunLock = false)
	{
		if (inventory.AllGuns.Count != 0 && (bool)CurrentGun && slotIndex >= 0 && slotIndex < inventory.AllGuns.Count)
		{
			int num = inventory.AllGuns.IndexOf(CurrentGun);
			int change = slotIndex - num;
			ChangeGun(change, true, overrideGunLock);
		}
	}

	public void ChangeGun(int change, bool forceEmptySelect = false, bool overrideGunLock = false)
	{
		if (inventory.AllGuns.Count == 0 || (inventory.DualWielding && inventory.AllGuns.Count <= 2 && CurrentSecondaryGun != null) || change % inventory.AllGuns.Count == 0)
		{
			return;
		}
		if (IsDodgeRolling)
		{
			CurrentGun.ToggleRenderers(true);
		}
		bool flag = GameManager.Options.HideEmptyGuns && IsInCombat && !forceEmptySelect;
		bool dualWielding = inventory.DualWielding;
		if (flag || dualWielding)
		{
			int num = 0;
			while ((flag && inventory.GetTargetGunWithChange(change).CurrentAmmo == 0) || (dualWielding && inventory.GetTargetGunWithChange(change) == CurrentSecondaryGun))
			{
				num++;
				change += Math.Sign(change);
				if (num >= inventory.AllGuns.Count)
				{
					break;
				}
			}
			if (inventory.GetTargetGunWithChange(change) == CurrentSecondaryGun)
			{
				change += Math.Sign(change);
			}
		}
		GameUIRoot.Instance.ForceClearReload(PlayerIDX);
		GunInventory gunInventory = inventory;
		int amt = change;
		bool overrideGunLock2 = overrideGunLock;
		gunInventory.ChangeGun(amt, false, overrideGunLock2);
		if (IsDodgeRolling)
		{
			CurrentGun.ToggleRenderers(false);
		}
		m_equippedGunShift = change;
	}

	public void ClearPerLevelData()
	{
		m_currentRoom = null;
		m_lastInteractionTarget = null;
		stats.ToNextLevel();
		m_bellygeonDepressedTiles.Clear();
		m_bellygeonDepressedTileTimers.Clear();
		for (int i = 0; i < additionalItems.Count; i++)
		{
			UnityEngine.Object.Destroy(additionalItems[i].gameObject);
		}
		additionalItems.Clear();
	}

	public void BraveOnLevelWasLoaded()
	{
		m_newFloorNoInput = true;
		HasGottenKeyThisRun = false;
		LevelToLoadOnPitfall = string.Empty;
		m_cachedLevelToLoadOnPitfall = string.Empty;
		m_interruptingPitRespawn = false;
		m_cachedPosition = Vector2.zero;
		m_currentRoom = null;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.GetOtherPlayer(this).m_currentRoom = null;
		}
		if (GameManager.Instance.InTutorial)
		{
			base.sprite.gameObject.SetLayerRecursively(LayerMask.NameToLayer("ShadowCaster"));
		}
		if (GameUIRoot.Instance != null)
		{
			GameUIRoot.Instance.UpdatePlayerHealthUI(this, base.healthHaver);
			if (passiveItems != null && passiveItems.Count > 0)
			{
				for (int i = 0; i < passiveItems.Count; i++)
				{
					GameUIRoot.Instance.AddPassiveItemToDock(passiveItems[i], this);
				}
			}
			Blanks = Mathf.Max(Blanks, ((GameManager.Instance.CurrentGameType != 0) ? stats.NumBlanksPerFloorCoop : stats.NumBlanksPerFloor) + Mathf.FloorToInt(stats.GetStatValue(PlayerStats.StatType.AdditionalBlanksPerFloor)));
			if (GameManager.Instance.InTutorial)
			{
				Blanks = 0;
			}
			GameUIRoot.Instance.UpdatePlayerBlankUI(this);
			carriedConsumables.ForceUpdateUI();
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				GameUIRoot.Instance.UpdateGunData(GameManager.Instance.GetOtherPlayer(this).inventory, 0, GameManager.Instance.GetOtherPlayer(this));
				GameUIRoot.Instance.UpdateItemData(GameManager.Instance.GetOtherPlayer(this), GameManager.Instance.GetOtherPlayer(this).CurrentItem, GameManager.Instance.GetOtherPlayer(this).activeItems);
			}
			if (IsGhost)
			{
				GameUIRoot.Instance.DisableCoopPlayerUI(this);
				GameUIRoot.Instance.TransitionToGhostUI(this);
			}
			else if (CurrentGun != null)
			{
				CurrentGun.ForceImmediateReload();
			}
			if (OnNewFloorLoaded != null)
			{
				OnNewFloorLoaded(this);
			}
		}
		if ((bool)base.knockbackDoer)
		{
			base.knockbackDoer.ClearContinuousKnockbacks();
		}
		Shader.SetGlobalFloat("_MeduziReflectionsEnabled", 0f);
		if (!m_usesRandomStartingEquipment || m_randomStartingItemsInitialized || (!GameManager.Instance.IsLoadingFirstShortcutFloor && GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON))
		{
			return;
		}
		m_randomStartingItemsInitialized = true;
		for (int j = 0; j < startingPassiveItemIds.Count; j++)
		{
			if (!HasPassiveItem(startingPassiveItemIds[j]))
			{
				AcquirePassiveItemPrefabDirectly(PickupObjectDatabase.GetById(startingPassiveItemIds[j]) as PassiveItem);
			}
		}
		for (int k = 0; k < startingActiveItemIds.Count; k++)
		{
			if (!HasActiveItem(startingActiveItemIds[k]))
			{
				EncounterTrackable.SuppressNextNotification = true;
				PlayerItem playerItem = PickupObjectDatabase.GetById(startingActiveItemIds[k]) as PlayerItem;
				playerItem.Pickup(this);
				EncounterTrackable.SuppressNextNotification = false;
			}
		}
	}

	private void EnteredNewRoom(RoomHandler newRoom)
	{
		RealtimeEnteredCurrentRoom = Time.realtimeSinceStartup;
	}

	public void ForceChangeRoom(RoomHandler newRoom)
	{
		RoomHandler currentRoom = m_currentRoom;
		m_currentRoom = newRoom;
		if (currentRoom != null)
		{
			currentRoom.PlayerExit(this);
			currentRoom.OnBecameInvisible(this);
		}
		m_currentRoom.PlayerEnter(this);
		EnteredNewRoom(m_currentRoom);
		m_inExitLastFrame = false;
		GameManager.Instance.MainCameraController.AssignBoundingPolygon(m_currentRoom.cameraBoundingPolygon);
	}

	private void HandleRoomProcessing()
	{
		if (BraveUtility.isLoadingLevel || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if (m_roomBeforeExit == null)
		{
			m_roomBeforeExit = m_currentRoom;
		}
		Dungeon dungeon = GameManager.Instance.Dungeon;
		DungeonData data = dungeon.data;
		CellData cellSafe = data.GetCellSafe(PhysicsEngine.PixelToUnit(base.specRigidbody.PrimaryPixelCollider.LowerLeft).ToIntVector2(VectorConversions.Floor));
		CellData cellSafe2 = data.GetCellSafe(PhysicsEngine.PixelToUnit(base.specRigidbody.PrimaryPixelCollider.LowerRight).ToIntVector2(VectorConversions.Floor));
		CellData cellSafe3 = data.GetCellSafe(PhysicsEngine.PixelToUnit(base.specRigidbody.PrimaryPixelCollider.UpperLeft).ToIntVector2(VectorConversions.Floor));
		CellData cellSafe4 = data.GetCellSafe(PhysicsEngine.PixelToUnit(base.specRigidbody.PrimaryPixelCollider.UpperRight).ToIntVector2(VectorConversions.Floor));
		if (cellSafe == null || cellSafe2 == null || cellSafe3 == null || cellSafe4 == null)
		{
			return;
		}
		RoomHandler roomHandler = null;
		CellData cellData = null;
		if (cellSafe.isExitCell || cellSafe2.isExitCell || cellSafe3.isExitCell || cellSafe4.isExitCell)
		{
			cellData = (cellSafe.isExitCell ? cellSafe : (cellSafe2.isExitCell ? cellSafe2 : (cellSafe3.isExitCell ? cellSafe3 : ((!cellSafe4.isExitCell) ? null : cellSafe4))));
			if (cellData != null)
			{
				roomHandler = ((cellData.connectedRoom1 == m_currentRoom) ? cellData.connectedRoom2 : cellData.connectedRoom1);
			}
			m_previousExitLinkedRoom = roomHandler;
			m_inExitLastFrame = true;
		}
		InExitCell = cellData != null;
		CurrentExitCell = cellData;
		if (!m_inExitLastFrame)
		{
			m_roomBeforeExit = m_currentRoom;
		}
		if (roomHandler == null)
		{
			roomHandler = ((cellSafe.parentRoom != m_currentRoom) ? cellSafe.parentRoom : ((cellSafe2.parentRoom != m_currentRoom) ? cellSafe2.parentRoom : ((cellSafe3.parentRoom != m_currentRoom) ? cellSafe3.parentRoom : ((cellSafe4.parentRoom == m_currentRoom) ? null : cellSafe4.parentRoom))));
		}
		if (roomHandler != null)
		{
			if (roomHandler.visibility == RoomHandler.VisibilityStatus.OBSCURED || roomHandler.visibility == RoomHandler.VisibilityStatus.REOBSCURED || roomHandler.IsSealed)
			{
				bool flag = cellSafe.isDoorFrameCell || cellSafe2.isDoorFrameCell || cellSafe3.isDoorFrameCell || cellSafe4.isDoorFrameCell;
				if (cellSafe.parentRoom != m_currentRoom && cellSafe2.parentRoom != m_currentRoom && cellSafe3.parentRoom != m_currentRoom && cellSafe4.parentRoom != m_currentRoom && !flag)
				{
					if (m_currentRoom != null)
					{
						m_currentRoom.PlayerExit(this);
					}
					m_currentRoom = roomHandler;
					m_currentRoom.PlayerEnter(this);
					EnteredNewRoom(m_currentRoom);
					m_inExitLastFrame = false;
					GameManager.Instance.MainCameraController.AssignBoundingPolygon(m_currentRoom.cameraBoundingPolygon);
				}
			}
			else
			{
				if (m_currentRoom != null)
				{
					m_currentRoom.OnBecameVisible(this);
				}
				if (cellData != null && cellData.exitDoor != null)
				{
					if (cellData.exitDoor.IsOpenForVisibilityTest && (cellData.exitDoor.subsidiaryBlocker == null || !cellData.exitDoor.subsidiaryBlocker.isSealed) && (cellData.exitDoor.subsidiaryDoor == null || cellData.exitDoor.subsidiaryDoor.IsOpenForVisibilityTest))
					{
						roomHandler.OnBecameVisible(this);
					}
					else
					{
						roomHandler.OnBecameInvisible(this);
					}
				}
				else
				{
					roomHandler.OnBecameVisible(this);
				}
				if (!cellSafe.isExitCell && !cellSafe2.isExitCell && !cellSafe3.isExitCell && !cellSafe4.isExitCell && cellSafe.parentRoom == roomHandler)
				{
					m_inExitLastFrame = false;
					if (m_currentRoom != null)
					{
						m_currentRoom.PlayerExit(this);
					}
					m_currentRoom = roomHandler;
					m_currentRoom.PlayerEnter(this);
					EnteredNewRoom(m_currentRoom);
					GameManager.Instance.MainCameraController.AssignBoundingPolygon(m_currentRoom.cameraBoundingPolygon);
				}
			}
		}
		else if (m_inExitLastFrame)
		{
			m_inExitLastFrame = false;
			if (m_previousExitLinkedRoom != null && m_previousExitLinkedRoom.visibility != 0)
			{
				Pixelator.Instance.ProcessRoomAdditionalExits(IntVector2.Zero, m_previousExitLinkedRoom, false);
			}
		}
		for (int i = 0; i < data.rooms.Count; i++)
		{
			RoomHandler roomHandler2 = data.rooms[i];
			if (roomHandler2.visibility != RoomHandler.VisibilityStatus.CURRENT || roomHandler2 == m_currentRoom || roomHandler2 == roomHandler)
			{
				continue;
			}
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(this);
				if ((bool)otherPlayer && (otherPlayer.CurrentRoom == roomHandler2 || (otherPlayer.InExitCell && otherPlayer.CurrentExitCell != null && (bool)otherPlayer.CurrentExitCell.exitDoor && (otherPlayer.CurrentExitCell.exitDoor.upstreamRoom == roomHandler2 || otherPlayer.CurrentExitCell.exitDoor.downstreamRoom == roomHandler2))))
				{
					continue;
				}
			}
			roomHandler2.PlayerExit(this);
		}
		if (m_currentRoom != null)
		{
			m_currentRoom.PlayerInCell(this, cellSafe.position, base.specRigidbody.PrimaryPixelCollider.UnitBottomLeft);
			m_currentRoom.PlayerInCell(this, cellSafe2.position, base.specRigidbody.PrimaryPixelCollider.UnitBottomRight);
			m_currentRoom.PlayerInCell(this, cellSafe3.position, base.specRigidbody.PrimaryPixelCollider.UnitTopLeft);
			m_currentRoom.PlayerInCell(this, cellSafe4.position, base.specRigidbody.PrimaryPixelCollider.UnitTopRight);
		}
		if (dungeon != null && dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.BELLYGEON)
		{
			IntVector2 intVector = SpriteBottomCenter.IntXY(VectorConversions.Floor);
			if (intVector != m_cachedLastCenterCellBellygeon)
			{
				m_cachedLastCenterCellBellygeon = intVector;
				if (m_bellygeonDepressedTiles.Contains(intVector))
				{
					m_bellygeonDepressedTileTimers[intVector] = 1f;
				}
				else
				{
					m_bellygeonDepressedTiles.Add(intVector);
					m_bellygeonDepressedTileTimers.Add(intVector, 1f);
				}
				data.TriggerFloorAnimationsInCell(intVector);
			}
			for (int j = 0; j < m_bellygeonDepressedTiles.Count; j++)
			{
				if (!(m_bellygeonDepressedTiles[j] == intVector))
				{
					m_bellygeonDepressedTileTimers[m_bellygeonDepressedTiles[j]] = m_bellygeonDepressedTileTimers[m_bellygeonDepressedTiles[j]] - BraveTime.DeltaTime;
					if (m_bellygeonDepressedTileTimers[m_bellygeonDepressedTiles[j]] <= 0f)
					{
						data.UntriggerFloorAnimationsInCell(m_bellygeonDepressedTiles[j]);
						m_bellygeonDepressedTileTimers.Remove(m_bellygeonDepressedTiles[j]);
						m_bellygeonDepressedTiles.RemoveAt(j);
						j--;
					}
				}
			}
		}
		HandleCurrentRoomExtraData();
	}

	private void HandleCurrentRoomExtraData()
	{
		bool flag = false;
		if (IsPrimaryPlayer)
		{
			flag = true;
		}
		else if (GameManager.Instance.PrimaryPlayer.healthHaver.IsDead)
		{
			flag = true;
		}
		if (!flag)
		{
			return;
		}
		if (GameManager.Instance.Dungeon.OverrideAmbientLight)
		{
			RenderSettings.ambientLight = GameManager.Instance.Dungeon.OverrideAmbientColor;
		}
		else if (CurrentRoom != null && !CurrentRoom.area.IsProceduralRoom && CurrentRoom.area.runtimePrototypeData != null && CurrentRoom.area.runtimePrototypeData.usesCustomAmbient)
		{
			Color color = CurrentRoom.area.runtimePrototypeData.customAmbient;
			if (GameManager.Options.LightingQuality == GameOptions.GenericHighMedLowOption.LOW && CurrentRoom.area.runtimePrototypeData.usesDifferentCustomAmbientLowQuality)
			{
				color = CurrentRoom.area.runtimePrototypeData.customAmbientLowQuality;
			}
			Vector3 vector = new Vector3(color.r, color.g, color.b) * RenderSettings.ambientIntensity;
			Vector3 vector2 = new Vector3(RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b);
			if (vector != vector2)
			{
				Vector3 vector3 = Vector3.MoveTowards(vector2, vector, 0.35f * GameManager.INVARIANT_DELTA_TIME);
				Color color3 = (RenderSettings.ambientLight = new Color(vector3.x, vector3.y, vector3.z, RenderSettings.ambientLight.a));
			}
		}
		else
		{
			Color color4 = ((GameManager.Options.LightingQuality != 0) ? GameManager.Instance.Dungeon.decoSettings.ambientLightColor : GameManager.Instance.Dungeon.decoSettings.lowQualityAmbientLightColor);
			Vector3 vector4 = new Vector3(color4.r, color4.g, color4.b) * RenderSettings.ambientIntensity;
			Vector3 vector5 = new Vector3(RenderSettings.ambientLight.r, RenderSettings.ambientLight.g, RenderSettings.ambientLight.b);
			if (vector4 != vector5)
			{
				Vector3 vector6 = Vector3.MoveTowards(vector5, vector4, 0.35f * GameManager.INVARIANT_DELTA_TIME);
				Color color6 = (RenderSettings.ambientLight = new Color(vector6.x, vector6.y, vector6.z, RenderSettings.ambientLight.a));
			}
		}
	}

	private void HandleGunAttachPointInternal(Gun targetGun, bool isSecondary = false)
	{
		if (targetGun == null)
		{
			return;
		}
		Vector3 vector = m_startingAttachPointPosition;
		Vector3 vector2 = downwardAttachPointPosition;
		if (targetGun.IsForwardPosition)
		{
			vector = vector.WithX(m_spriteDimensions.x - vector.x);
			vector2 = vector2.WithX(m_spriteDimensions.x - vector2.x);
		}
		if (SpriteFlipped)
		{
			vector = vector.WithX(m_spriteDimensions.x - vector.x);
			vector2 = vector2.WithX(m_spriteDimensions.x - vector2.x);
		}
		float num = ((!SpriteFlipped) ? 1 : (-1));
		Vector3 a = targetGun.GetCarryPixelOffset(characterIdentity).ToVector3();
		vector += Vector3.Scale(a, new Vector3(num, 1f, 1f)) * 0.0625f;
		vector2 += Vector3.Scale(a, new Vector3(num, 1f, 1f)) * 0.0625f;
		if (targetGun.Handedness == GunHandedness.NoHanded && SpriteFlipped)
		{
			vector += Vector3.Scale(targetGun.leftFacingPixelOffset.ToVector3(), new Vector3(num, 1f, 1f)) * 0.0625f;
			vector2 += Vector3.Scale(targetGun.leftFacingPixelOffset.ToVector3(), new Vector3(num, 1f, 1f)) * 0.0625f;
		}
		if (IsFlying)
		{
			vector += new Vector3(0f, 0.1875f, 0f);
			vector2 += new Vector3(0f, 0.1875f, 0f);
		}
		if (isSecondary)
		{
			if (targetGun.transform.parent != SecondaryGunPivot)
			{
				targetGun.transform.parent = SecondaryGunPivot;
				targetGun.transform.localRotation = Quaternion.identity;
				targetGun.HandleSpriteFlip(SpriteFlipped);
				targetGun.UpdateAttachTransform();
			}
			SecondaryGunPivot.position = gunAttachPoint.position + num * new Vector3(-0.75f, 0f, 0f);
			return;
		}
		if (targetGun.transform.parent != gunAttachPoint)
		{
			targetGun.transform.parent = gunAttachPoint;
			targetGun.transform.localRotation = Quaternion.identity;
			targetGun.HandleSpriteFlip(SpriteFlipped);
			targetGun.UpdateAttachTransform();
		}
		if (targetGun.IsHeroSword)
		{
			float t = 1f - Mathf.Abs(m_currentGunAngle - 90f) / 90f;
			gunAttachPoint.localPosition = BraveUtility.QuantizeVector(Vector3.Slerp(vector, vector2, t), 16f);
		}
		else if (targetGun.Handedness == GunHandedness.TwoHanded)
		{
			float t2 = Mathf.PingPong(Mathf.Abs(1f - Mathf.Abs(m_currentGunAngle + 90f) / 90f), 1f);
			Vector2 zero = Vector2.zero;
			zero = ((!(m_currentGunAngle > 0f)) ? (Vector2.Scale(targetGun.carryPixelDownOffset.ToVector2(), new Vector2(num, 1f)) * 0.0625f) : (Vector2.Scale(targetGun.carryPixelUpOffset.ToVector2(), new Vector2(num, 1f)) * 0.0625f));
			if (targetGun.LockedHorizontalOnCharge)
			{
				zero = Vector3.Slerp(zero, Vector2.zero, targetGun.GetChargeFraction());
			}
			if (m_currentGunAngle < 0f)
			{
				gunAttachPoint.localPosition = BraveUtility.QuantizeVector(Vector3.Slerp(vector, vector2 + zero.ToVector3ZUp(), t2), 16f);
			}
			else
			{
				gunAttachPoint.localPosition = BraveUtility.QuantizeVector(Vector3.Slerp(vector, vector + zero.ToVector3ZUp(), t2), 16f);
			}
		}
		else
		{
			gunAttachPoint.localPosition = BraveUtility.QuantizeVector(vector, 16f);
		}
	}

	private void HandleGunAttachPoint()
	{
		if ((bool)CurrentGun)
		{
			HandleGunAttachPointInternal(CurrentGun);
		}
		if (inventory != null && inventory.DualWielding && (bool)CurrentSecondaryGun)
		{
			HandleGunAttachPointInternal(CurrentSecondaryGun, true);
		}
	}

	private void HandleShellCasingDisplacement()
	{
	}

	protected override void OnDestroy()
	{
		ClearOverrideShader();
		if (PassiveItem.ActiveFlagItems != null)
		{
			PassiveItem.ActiveFlagItems.Remove(this);
		}
		base.OnDestroy();
	}

	public void TriggerHighStress(float duration)
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.NextShotKills = true;
		}
		m_highStressTimer = duration;
	}

	private void DoAutoAimNotification(bool warnOnly)
	{
		dfLabel nameLabel = GameUIRoot.Instance.notificationController.NameLabel;
		if (warnOnly)
		{
			GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.PostprocessString(nameLabel.ForceGetLocalizedValue("#SUPERDUPERAUTOAIM_WARNING_TITLE")), StringTableManager.PostprocessString(nameLabel.ForceGetLocalizedValue("#SUPERDUPERAUTOAIM_WARNING_BODY")), null, -1);
		}
		else
		{
			GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.PostprocessString(nameLabel.ForceGetLocalizedValue("#SUPERDUPERAUTOAIM_POPUP_TITLE")), StringTableManager.PostprocessString(nameLabel.ForceGetLocalizedValue("#SUPERDUPERAUTOAIM_WARNING_BODY_B")), null, -1);
		}
	}

	public void DoVibration(Vibration.Time time, Vibration.Strength strength)
	{
		BraveInput.GetInstanceForPlayer(PlayerIDX).DoVibration(time, strength);
	}

	public void DoVibration(float time, Vibration.Strength strength)
	{
		BraveInput.GetInstanceForPlayer(PlayerIDX).DoVibration(time, strength);
	}

	public void DoVibration(Vibration.Time time, Vibration.Strength largeMotor, Vibration.Strength smallMotor)
	{
		BraveInput.GetInstanceForPlayer(PlayerIDX).DoVibration(time, largeMotor, smallMotor);
	}

	public void DoScreenShakeVibration(float time, float magnitude)
	{
		BraveInput.GetInstanceForPlayer(PlayerIDX).DoScreenShakeVibration(time, magnitude);
	}

	public void DoSustainedVibration(Vibration.Strength strength)
	{
		BraveInput.GetInstanceForPlayer(PlayerIDX).DoSustainedVibration(strength);
	}

	public void DoSustainedVibration(Vibration.Strength largeMotor, Vibration.Strength smallMotor)
	{
		BraveInput.GetInstanceForPlayer(PlayerIDX).DoSustainedVibration(largeMotor, smallMotor);
	}
}
