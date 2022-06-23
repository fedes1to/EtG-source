using System;
using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using PathologicalGames;
using UnityEngine;
using UnityEngine.Serialization;

public class Projectile : BraveBehaviour
{
	public enum ProjectileDestroyMode
	{
		Destroy,
		DestroyComponent,
		BecomeDebris,
		None
	}

	protected enum HandleDamageResult
	{
		NO_HEALTH,
		HEALTH,
		HEALTH_AND_KILLED
	}

	public static bool s_delayPlayerDamage;

	public static float s_maxProjectileScale = 3.5f;

	private static float s_enemyBulletSpeedModfier = 1f;

	private static float s_baseEnemyBulletSpeedMultiplier = 1f;

	[NonSerialized]
	public Gun PossibleSourceGun;

	[NonSerialized]
	public bool SpawnedFromOtherPlayerProjectile;

	[NonSerialized]
	public float PlayerProjectileSourceGameTimeslice = -1f;

	[NonSerialized]
	private GameActor m_owner;

	[FormerlySerializedAs("BulletMLSettings")]
	public BulletScriptSettings BulletScriptSettings;

	[EnumFlags]
	public CoreDamageTypes damageTypes;

	public bool allowSelfShooting;

	public bool collidesWithPlayer = true;

	public bool collidesWithProjectiles;

	[ShowInInspectorIf("collidesWithProjectiles", true)]
	public bool collidesOnlyWithPlayerProjectiles;

	[ShowInInspectorIf("collidesWithProjectiles", true)]
	public int projectileHitHealth;

	public bool collidesWithEnemies = true;

	public bool shouldRotate;

	[FormerlySerializedAs("shouldFlip")]
	[ShowInInspectorIf("shouldRotate", false)]
	public bool shouldFlipVertically;

	public bool shouldFlipHorizontally;

	public bool ignoreDamageCaps;

	[NonSerialized]
	private float m_cachedInitialDamage = -1f;

	public ProjectileData baseData;

	public bool AppliesPoison;

	public float PoisonApplyChance = 1f;

	public GameActorHealthEffect healthEffect;

	public bool AppliesSpeedModifier;

	public float SpeedApplyChance = 1f;

	public GameActorSpeedEffect speedEffect;

	public bool AppliesCharm;

	public float CharmApplyChance = 1f;

	public GameActorCharmEffect charmEffect;

	public bool AppliesFreeze;

	public float FreezeApplyChance = 1f;

	public GameActorFreezeEffect freezeEffect;

	public bool AppliesFire;

	public float FireApplyChance = 1f;

	public GameActorFireEffect fireEffect;

	public bool AppliesStun;

	public float StunApplyChance = 1f;

	public float AppliedStunDuration = 1f;

	public bool AppliesBleed;

	public GameActorBleedEffect bleedEffect;

	public bool AppliesCheese;

	public float CheeseApplyChance = 1f;

	public GameActorCheeseEffect cheeseEffect;

	public float BleedApplyChance = 1f;

	public bool CanTransmogrify;

	[ShowInInspectorIf("CanTransmogrify", false)]
	public float ChanceToTransmogrify;

	[EnemyIdentifier]
	public string[] TransmogrifyTargetGuids;

	[NonSerialized]
	public float BossDamageMultiplier = 1f;

	[NonSerialized]
	public bool SpawnedFromNonChallengeItem;

	[NonSerialized]
	public bool TreatedAsNonProjectileForChallenge;

	public ProjectileImpactVFXPool hitEffects;

	public bool CenterTilemapHitEffectsByProjectileVelocity;

	public VFXPool wallDecals;

	public bool damagesWalls = true;

	public float persistTime = 0.25f;

	public float angularVelocity;

	public float angularVelocityVariance;

	[EnemyIdentifier]
	public string spawnEnemyGuidOnDeath;

	public bool HasFixedKnockbackDirection;

	public float FixedKnockbackDirection;

	public bool pierceMinorBreakables;

	[Header("Audio Flags")]
	public string objectImpactEventName = string.Empty;

	public string enemyImpactEventName = string.Empty;

	public string onDestroyEventName = string.Empty;

	public string additionalStartEventName = string.Empty;

	[Header("Unusual Options")]
	public bool IsRadialBurstLimited;

	public int MaxRadialBurstLimit = -1;

	public SynergyBurstLimit[] AdditionalBurstLimits;

	public bool AppliesKnockbackToPlayer;

	public float PlayerKnockbackForce;

	public bool HasDefaultTint;

	[ShowInInspectorIf("HasDefaultTint", false)]
	public Color DefaultTintColor;

	[NonSerialized]
	public bool IsCritical;

	[NonSerialized]
	public float BlackPhantomDamageMultiplier = 1f;

	[Header("For Brents")]
	public bool PenetratesInternalWalls;

	public bool neverMaskThis;

	public bool isFakeBullet;

	public bool CanBecomeBlackBullet = true;

	public TrailRenderer TrailRenderer;

	public CustomTrailRenderer CustomTrailRenderer;

	public ParticleSystem ParticleTrail;

	public bool DelayedDamageToExploders;

	public Action<Projectile, SpeculativeRigidbody, bool> OnHitEnemy;

	public Action<Projectile, SpeculativeRigidbody> OnWillKillEnemy;

	public Action<DebrisObject> OnBecameDebris;

	public Action<DebrisObject> OnBecameDebrisGrounded;

	[NonSerialized]
	public bool IsBlackBullet;

	private bool m_forceBlackBullet;

	[NonSerialized]
	public List<GameActorEffect> statusEffectsToApply = new List<GameActorEffect>();

	private bool m_initialized;

	private Transform m_transform;

	private bool? m_cachedHasBeamController;

	public float AdditionalScaleMultiplier = 1f;

	private int m_cachedLayer;

	private int m_currentTintPriority = -1;

	public Func<Vector2, Vector2> ModifyVelocity;

	[NonSerialized]
	public bool CurseSparks;

	private Vector2? m_lastSparksPoint;

	public Action<Projectile> PreMoveModifiers;

	[NonSerialized]
	public ProjectileMotionModule OverrideMotionModule;

	[NonSerialized]
	protected bool m_usesNormalMoveRegardless;

	public static Dungeon m_cachedDungeon;

	public static int m_cacheTick;

	protected bool m_isInWall;

	private SpeculativeRigidbody m_shooter;

	protected float m_currentSpeed;

	protected Vector2 m_currentDirection;

	protected MeshRenderer m_renderer;

	protected float m_timeElapsed;

	protected float m_distanceElapsed;

	protected Vector3 m_lastPosition;

	protected bool m_hasImpactedObject;

	protected bool m_hasImpactedEnemy;

	protected bool m_hasDiedInAir;

	protected bool m_hasPierced;

	private int m_healthHaverHitCount;

	private bool m_cachedCollidesWithPlayer;

	private bool m_cachedCollidesWithProjectiles;

	private bool m_cachedCollidesWithEnemies;

	private bool m_cachedDamagesWalls;

	private ProjectileData m_cachedBaseData;

	private BulletScriptSettings m_cachedBulletScriptSettings;

	private bool m_cachedCollideWithTileMap;

	private bool m_cachedCollideWithOthers;

	private int m_cachedSpriteId = -1;

	private PrefabPool m_spawnPool;

	private bool m_isRamping;

	private float m_rampTimer;

	private float m_rampDuration;

	private float m_currentRampHeight;

	private float m_startRampHeight;

	private float m_ignoreTileCollisionsTimer;

	private float m_outOfBoundsCounter;

	private bool m_isExitClippingTiles;

	private float m_exitClippingDistance;

	public static float CurrentProjectileDepth = 0.8f;

	public const float c_DefaultProjectileDepth = 0.8f;

	public static float EnemyBulletSpeedMultiplier
	{
		get
		{
			return s_enemyBulletSpeedModfier;
		}
	}

	public static float BaseEnemyBulletSpeedMultiplier
	{
		get
		{
			return s_baseEnemyBulletSpeedMultiplier;
		}
		set
		{
			s_baseEnemyBulletSpeedMultiplier = value;
			UpdateEnemyBulletSpeedMultiplier();
		}
	}

	public BulletScriptBehavior braveBulletScript { get; set; }

	[HideInInspector]
	public GameActor Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
			if (m_owner is AIActor)
			{
				OwnerName = (m_owner as AIActor).GetActorName();
			}
			else if (m_owner is PlayerController)
			{
				if (PossibleSourceGun == null)
				{
					PossibleSourceGun = (m_owner as PlayerController).CurrentGun;
				}
				OwnerName = ((!(m_owner as PlayerController).IsPrimaryPlayer) ? "secondaryplayer" : "primaryplayer");
			}
			CheckBlackPhantomness();
		}
	}

	public ProjectileTrapController TrapOwner { get; set; }

	public string OwnerName { get; set; }

	public float GetCachedBaseDamage
	{
		get
		{
			return m_cachedInitialDamage;
		}
	}

	public float ModifiedDamage
	{
		get
		{
			return baseData.damage;
		}
	}

	public bool SuppressHitEffects { get; set; }

	protected float LocalTimeScale
	{
		get
		{
			if ((bool)Owner && Owner is AIActor)
			{
				return (Owner as AIActor).LocalTimeScale;
			}
			if ((bool)TrapOwner)
			{
				return TrapOwner.LocalTimeScale;
			}
			return Time.timeScale;
		}
	}

	public float LocalDeltaTime
	{
		get
		{
			if ((bool)Owner && Owner is AIActor)
			{
				return (Owner as AIActor).LocalDeltaTime;
			}
			return BraveTime.DeltaTime;
		}
	}

	public SpeculativeRigidbody Shooter
	{
		get
		{
			return m_shooter;
		}
		set
		{
			m_shooter = value;
			if (!allowSelfShooting)
			{
				base.specRigidbody.RegisterSpecificCollisionException(m_shooter);
			}
		}
	}

	public float Speed
	{
		get
		{
			return m_currentSpeed;
		}
		set
		{
			m_currentSpeed = value;
		}
	}

	public Vector2 Direction
	{
		get
		{
			return m_currentDirection;
		}
		set
		{
			m_currentDirection = value;
		}
	}

	public bool CanKillBosses
	{
		get
		{
			if (Owner == null || !(Owner is PlayerController))
			{
				return false;
			}
			return (Owner as PlayerController).BossKillingMode;
		}
	}

	public ProjectileDestroyMode DestroyMode { get; set; }

	public bool Inverted { get; set; }

	public Vector2 LastVelocity { get; set; }

	public bool ManualControl { get; set; }

	public bool ForceBlackBullet
	{
		get
		{
			return m_forceBlackBullet;
		}
		set
		{
			if (m_forceBlackBullet != value)
			{
				m_forceBlackBullet = value;
				CheckBlackPhantomness();
			}
			else
			{
				m_forceBlackBullet = value;
			}
		}
	}

	public bool IsBulletScript { get; set; }

	public bool CanBeKilledByExplosions
	{
		get
		{
			bool? cachedHasBeamController = m_cachedHasBeamController;
			if (!cachedHasBeamController.HasValue)
			{
				m_cachedHasBeamController = GetComponent<BeamController>();
			}
			return !m_cachedHasBeamController.Value && !ImmuneToBlanks && !ImmuneToSustainedBlanks;
		}
	}

	public bool CanBeCaught
	{
		get
		{
			PierceProjModifier component = GetComponent<PierceProjModifier>();
			if (component != null && component.BeastModeLevel != 0)
			{
				return false;
			}
			if (!base.sprite)
			{
				return false;
			}
			return true;
		}
	}

	public float ElapsedTime
	{
		get
		{
			return m_timeElapsed;
		}
	}

	public Vector2? OverrideTrailPoint { get; set; }

	public bool SkipDistanceElapsedCheck { get; set; }

	public bool ImmuneToBlanks { get; set; }

	public bool ImmuneToSustainedBlanks { get; set; }

	public bool ForcePlayerBlankable { get; set; }

	public bool IsReflectedBySword { get; set; }

	public int LastReflectedSlashId { get; set; }

	public ProjectileTrailRendererController TrailRendererController { get; set; }

	private Vector2 SafeCenter
	{
		get
		{
			if ((bool)base.specRigidbody)
			{
				return base.specRigidbody.UnitCenter;
			}
			if ((bool)m_transform)
			{
				return m_transform.position.XY();
			}
			return LastPosition.XY();
		}
	}

	public Vector3 LastPosition
	{
		get
		{
			return m_lastPosition;
		}
		set
		{
			m_lastPosition = value;
		}
	}

	public bool HasImpactedEnemy
	{
		get
		{
			return m_hasImpactedEnemy;
		}
	}

	public int NumberHealthHaversHit
	{
		get
		{
			return m_healthHaverHitCount;
		}
	}

	public bool HasDiedInAir
	{
		get
		{
			return m_hasDiedInAir;
		}
	}

	public event Action<Projectile> OnPostUpdate;

	public event Action<Projectile> OnReflected;

	public event Action<Projectile> OnDestruction;

	public static void UpdateEnemyBulletSpeedMultiplier()
	{
		float num = 1f;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			num = GameManager.Instance.COOP_ENEMY_PROJECTILE_SPEED_MULTIPLIER;
		}
		if (GameManager.Instance.Dungeon != null)
		{
			s_enemyBulletSpeedModfier = s_baseEnemyBulletSpeedMultiplier * GameManager.Instance.Dungeon.GetNewPlayerSpeedMultiplier() * PlayerStats.GetTotalEnemyProjectileSpeedMultiplier() * num;
		}
		else
		{
			s_enemyBulletSpeedModfier = s_baseEnemyBulletSpeedMultiplier;
		}
	}

	public void SetOwnerSafe(GameActor owner, string ownerName)
	{
		m_owner = owner;
		OwnerName = ownerName;
		CheckBlackPhantomness();
	}

	public static void SetGlobalProjectileDepth(float newDepth)
	{
		CurrentProjectileDepth = newDepth;
	}

	public static void ResetGlobalProjectileDepth()
	{
		CurrentProjectileDepth = 0.8f;
	}

	public void Awake()
	{
		if (baseData == null)
		{
			baseData = new ProjectileData();
		}
		if (BulletScriptSettings == null)
		{
			BulletScriptSettings = new BulletScriptSettings();
		}
		m_transform = base.transform;
		m_cachedInitialDamage = baseData.damage;
		if (base.specRigidbody != null)
		{
			if (PenetratesInternalWalls)
			{
				SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
				speculativeRigidbody.OnPreTileCollision = (SpeculativeRigidbody.OnPreTileCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreTileCollision, new SpeculativeRigidbody.OnPreTileCollisionDelegate(OnPreTileCollision));
			}
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
			SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
			speculativeRigidbody3.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(speculativeRigidbody3.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			SpeculativeRigidbody speculativeRigidbody4 = base.specRigidbody;
			speculativeRigidbody4.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody4.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		}
		if (!base.sprite)
		{
			base.sprite = GetComponentInChildren<tk2dSprite>();
		}
		if (!base.spriteAnimator && (bool)base.sprite)
		{
			base.spriteAnimator = base.sprite.spriteAnimator;
		}
		if (m_renderer == null)
		{
			m_renderer = GetComponentInChildren<MeshRenderer>();
		}
	}

	public void Reawaken()
	{
		if (!base.sprite)
		{
			base.sprite = GetComponentInChildren<tk2dSprite>();
		}
		if (!base.spriteAnimator && (bool)base.sprite)
		{
			base.spriteAnimator = base.sprite.spriteAnimator;
		}
		if (m_renderer == null)
		{
			m_renderer = GetComponentInChildren<MeshRenderer>();
		}
	}

	public void RuntimeUpdateScale(float multiplier)
	{
		if (!base.sprite)
		{
			return;
		}
		float x = base.sprite.scale.x;
		float num = Mathf.Clamp(x * multiplier, 0.01f, s_maxProjectileScale);
		AdditionalScaleMultiplier *= multiplier;
		base.sprite.scale = new Vector3(num, num, num);
		if (base.specRigidbody != null)
		{
			base.specRigidbody.UpdateCollidersOnScale = true;
		}
		if (num > 1.5f)
		{
			Vector3 size = base.sprite.GetBounds().size;
			if (size.x > 4f || size.y > 4f)
			{
				base.sprite.HeightOffGround = UnityEngine.Random.Range(0f, -3f);
			}
		}
	}

	public virtual void Start()
	{
		if (m_initialized)
		{
			return;
		}
		m_initialized = true;
		m_transform = base.transform;
		if (!string.IsNullOrEmpty(additionalStartEventName))
		{
			AkSoundEngine.PostEvent(additionalStartEventName, base.gameObject);
		}
		StaticReferenceManager.AddProjectile(this);
		if ((bool)GetComponent<BeamController>())
		{
			base.enabled = false;
			return;
		}
		if ((bool)m_renderer)
		{
			DepthLookupManager.ProcessRenderer(m_renderer);
		}
		if ((bool)base.sprite)
		{
			base.sprite.HeightOffGround = CurrentProjectileDepth;
			m_currentRampHeight = 0f;
			float num = BraveMathCollege.ClampAngle360(m_transform.eulerAngles.z);
			if (Owner is PlayerController)
			{
				float bulletScaleModifier = (Owner as PlayerController).BulletScaleModifier;
				bulletScaleModifier = Mathf.Clamp(bulletScaleModifier * AdditionalScaleMultiplier, 0.01f, s_maxProjectileScale);
				base.sprite.scale = new Vector3(bulletScaleModifier, bulletScaleModifier, bulletScaleModifier);
				if (bulletScaleModifier != 1f)
				{
					if (base.specRigidbody != null)
					{
						base.specRigidbody.UpdateCollidersOnScale = true;
						base.specRigidbody.ForceRegenerate();
					}
					if (base.sprite.transform != m_transform)
					{
						base.sprite.transform.localPosition = Vector3.Scale(base.sprite.transform.localPosition, base.sprite.scale);
					}
					DoWallExitClipping();
				}
				if (HasDefaultTint)
				{
					AdjustPlayerProjectileTint(DefaultTintColor, 0);
				}
			}
			if (shouldRotate && shouldFlipVertically)
			{
				base.sprite.FlipY = num < 270f && num > 90f;
			}
			if (shouldFlipHorizontally)
			{
				base.sprite.FlipX = num > 90f && num < 270f;
			}
		}
		if (base.specRigidbody != null && Owner is PlayerController)
		{
			base.specRigidbody.UpdateCollidersOnRotation = true;
			base.specRigidbody.UpdateCollidersOnScale = true;
		}
		if (isFakeBullet)
		{
			base.enabled = false;
			base.sprite.HeightOffGround = CurrentProjectileDepth;
			base.sprite.UpdateZDepth();
			return;
		}
		if (base.specRigidbody == null)
		{
			Debug.LogError("No speculative rigidbody found on projectile!", this);
		}
		if (GameManager.PVP_ENABLED && !TreatedAsNonProjectileForChallenge)
		{
			collidesWithPlayer = true;
		}
		if (collidesWithPlayer && Owner is AIActor && (Owner as AIActor).CompanionOwner != null)
		{
			collidesWithPlayer = false;
		}
		if (collidesWithProjectiles)
		{
			for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
			{
				base.specRigidbody.PixelColliders[i].CollisionLayerCollidableOverride |= CollisionMask.LayerToMask(CollisionLayer.Projectile);
			}
		}
		if (!collidesWithPlayer)
		{
			for (int j = 0; j < base.specRigidbody.PixelColliders.Count; j++)
			{
				base.specRigidbody.PixelColliders[j].CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox);
			}
		}
		if (!collidesWithEnemies)
		{
			for (int k = 0; k < base.specRigidbody.PixelColliders.Count; k++)
			{
				base.specRigidbody.PixelColliders[k].CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
			}
		}
		if (Owner is PlayerController)
		{
			for (int l = 0; l < base.specRigidbody.PixelColliders.Count; l++)
			{
				base.specRigidbody.PixelColliders[l].CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyBulletBlocker);
			}
		}
		else if (Owner is AIActor && collidesWithEnemies && PassiveItem.IsFlagSetAtAll(typeof(BattleStandardItem)))
		{
			baseData.damage *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
		}
		if (Owner is PlayerController)
		{
			PostprocessPlayerBullet();
		}
		if (base.specRigidbody.UpdateCollidersOnRotation)
		{
			base.specRigidbody.ForceRegenerate();
		}
		m_timeElapsed = 0f;
		LastPosition = m_transform.position;
		m_currentSpeed = baseData.speed;
		m_currentDirection = m_transform.right;
		if (!shouldRotate)
		{
			m_transform.rotation = Quaternion.identity;
		}
		if (CanKillBosses)
		{
			StartCoroutine(CheckIfBossKillShot());
		}
		if (!shouldRotate)
		{
			base.specRigidbody.IgnorePixelGrid = true;
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unpixelated"));
		}
		if (angularVelocity != 0f)
		{
			angularVelocity = BraveUtility.RandomSign() * angularVelocity + UnityEngine.Random.Range(0f - angularVelocityVariance, angularVelocityVariance);
		}
		CheckBlackPhantomness();
	}

	private void CheckBlackPhantomness()
	{
		if (CanBecomeBlackBullet && (ForceBlackBullet || (Owner is AIActor && (Owner as AIActor).IsBlackPhantom)))
		{
			BecomeBlackBullet();
		}
		else if (IsBlackBullet)
		{
			ReturnFromBlackBullet();
		}
	}

	public int GetRadialBurstLimit(PlayerController source)
	{
		int num = int.MaxValue;
		for (int i = 0; i < AdditionalBurstLimits.Length; i++)
		{
			if (source.HasActiveBonusSynergy(AdditionalBurstLimits[i].RequiredSynergy))
			{
				num = Mathf.Min(num, AdditionalBurstLimits[i].limit);
			}
		}
		if (IsRadialBurstLimited && MaxRadialBurstLimit > -1)
		{
			num = Mathf.Min(num, MaxRadialBurstLimit);
		}
		return num;
	}

	public void CacheLayer(int targetLayer)
	{
		if (!(base.sprite == null))
		{
			m_cachedLayer = base.sprite.gameObject.layer;
			base.gameObject.SetLayerRecursively(targetLayer);
		}
	}

	public void DecacheLayer()
	{
		if (!(base.sprite == null))
		{
			base.gameObject.SetLayerRecursively(m_cachedLayer);
		}
	}

	private void PostprocessPlayerBullet()
	{
		PlayerController playerController = Owner as PlayerController;
		int num = Mathf.FloorToInt(playerController.stats.GetStatValue(PlayerStats.StatType.AdditionalShotPiercing));
		if ((bool)PossibleSourceGun && PossibleSourceGun.gunClass == GunClass.SHOTGUN && playerController.HasActiveBonusSynergy(CustomSynergyType.SHOTGUN_SPEED))
		{
			baseData.speed *= 2f;
			baseData.force *= 3f;
			num++;
		}
		if (num > 0)
		{
			PierceProjModifier component = GetComponent<PierceProjModifier>();
			if (component == null)
			{
				component = base.gameObject.AddComponent<PierceProjModifier>();
				component.penetration = num;
				component.penetratesBreakables = true;
				component.BeastModeLevel = PierceProjModifier.BeastModeStatus.NOT_BEAST_MODE;
			}
			else
			{
				component.penetration += num;
			}
		}
		int num2 = Mathf.FloorToInt(playerController.stats.GetStatValue(PlayerStats.StatType.AdditionalShotBounces));
		if (num2 > 0)
		{
			BounceProjModifier component2 = GetComponent<BounceProjModifier>();
			if (component2 == null)
			{
				component2 = base.gameObject.AddComponent<BounceProjModifier>();
				component2.numberOfBounces = num2;
			}
			else
			{
				component2.numberOfBounces += num2;
			}
		}
	}

	public void AdjustPlayerProjectileTint(Color targetTintColor, int priority, float lerpTime = 0f)
	{
		if (priority > m_currentTintPriority || (priority == m_currentTintPriority && UnityEngine.Random.value < 0.5f))
		{
			m_currentTintPriority = priority;
			if (Owner is PlayerController)
			{
				ChangeTintColorShader(lerpTime, targetTintColor);
			}
		}
	}

	public void RemovePlayerOnlyModifiers()
	{
		HomingModifier component = GetComponent<HomingModifier>();
		if ((bool)component)
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	public void MakeLookLikeEnemyBullet(bool applyScaleChanges = true)
	{
		if ((bool)base.specRigidbody && (bool)base.sprite && applyScaleChanges)
		{
			tk2dSpriteDefinition currentSpriteDef = base.sprite.GetCurrentSpriteDef();
			Bounds bounds = currentSpriteDef.GetBounds();
			float num = Mathf.Max(bounds.size.x, bounds.size.y);
			if (num < 0.5f)
			{
				float num2 = 0.5f / num;
				Debug.Log(num + "|" + num2);
				base.sprite.scale = new Vector3(num2, num2, num2);
				if (num2 != 1f && base.specRigidbody != null)
				{
					base.specRigidbody.UpdateCollidersOnScale = true;
					base.specRigidbody.ForceRegenerate();
				}
			}
		}
		if ((bool)base.sprite && (bool)base.sprite.renderer)
		{
			Material sharedMaterial = base.sprite.renderer.sharedMaterial;
			base.sprite.usesOverrideMaterial = true;
			Material material = new Material(ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive"));
			material.SetTexture("_MainTex", sharedMaterial.GetTexture("_MainTex"));
			material.SetColor("_OverrideColor", new Color(1f, 1f, 1f, 1f));
			LerpMaterialGlow(material, 0f, 22f, 0.4f);
			material.SetFloat("_EmissiveColorPower", 8f);
			material.SetColor("_EmissiveColor", Color.red);
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.red);
			base.sprite.renderer.material = material;
		}
	}

	private void HandleSparks(Vector2? overridePoint = null)
	{
		if (damageTypes == (damageTypes | CoreDamageTypes.Electric) && (bool)base.specRigidbody)
		{
			Vector2 vector = ((!overridePoint.HasValue) ? base.specRigidbody.UnitCenter : overridePoint.Value);
			Vector2 vector2 = ((!m_lastSparksPoint.HasValue) ? m_lastPosition.XY() : m_lastSparksPoint.Value);
			m_lastSparksPoint = vector;
			float magnitude = (m_lastPosition.XY() - vector).magnitude;
			int b = (int)(magnitude * 6f);
			GlobalSparksDoer.DoLinearParticleBurst(Mathf.Max(1, b), vector2, vector, 360f, 5f, 0.5f, null, 0.2f, new Color(0.25f, 0.25f, 1f, 1f));
		}
		if (CurseSparks && (bool)base.specRigidbody)
		{
			Vector2 vector3 = ((!overridePoint.HasValue) ? base.specRigidbody.UnitCenter : overridePoint.Value);
			Vector2 vector4 = ((!m_lastSparksPoint.HasValue) ? m_lastPosition.XY() : m_lastSparksPoint.Value);
			m_lastSparksPoint = vector3;
			float magnitude2 = (m_lastPosition.XY() - vector3).magnitude;
			int b2 = (int)(magnitude2 * 3f);
			GlobalSparksDoer.DoLinearParticleBurst(Mathf.Max(1, b2), vector4, vector3, 360f, 5f, 0.5f, null, 0.2f, new Color(0.25f, 0.25f, 1f, 1f), GlobalSparksDoer.SparksType.DARK_MAGICKS);
		}
	}

	public virtual void Update()
	{
		tk2dBaseSprite tk2dBaseSprite2 = base.sprite;
		bool flag = tk2dBaseSprite2;
		if (Time.frameCount != m_cacheTick)
		{
			m_cachedDungeon = ((!GameManager.Instance) ? null : GameManager.Instance.Dungeon);
			m_cacheTick = Time.frameCount;
		}
		if (IsBlackBullet)
		{
			if (!ForceBlackBullet && Owner is AIActor && !(Owner as AIActor).IsBlackPhantom)
			{
				ReturnFromBlackBullet();
			}
			if ((bool)Owner && !(Owner is AIActor))
			{
				ReturnFromBlackBullet();
			}
		}
		if (m_isRamping && flag)
		{
			float currentRampHeight = m_currentRampHeight;
			if (m_rampTimer <= m_rampDuration)
			{
				m_currentRampHeight = Mathf.Lerp(m_startRampHeight, 0f, m_rampTimer / m_rampDuration);
			}
			else
			{
				m_currentRampHeight = 0f;
				m_isRamping = false;
			}
			tk2dBaseSprite2.HeightOffGround -= currentRampHeight - m_currentRampHeight;
			tk2dBaseSprite2.UpdateZDepthLater();
			float num = LocalDeltaTime;
			if (!(Owner is PlayerController))
			{
				num *= EnemyBulletSpeedMultiplier;
			}
			m_rampTimer += num;
		}
		if (m_ignoreTileCollisionsTimer > 0f)
		{
			float num2 = LocalDeltaTime;
			if (!(Owner is PlayerController))
			{
				num2 *= EnemyBulletSpeedMultiplier;
			}
			m_rampTimer += num2;
			m_ignoreTileCollisionsTimer = Mathf.Max(0f, m_ignoreTileCollisionsTimer - num2);
			if (m_ignoreTileCollisionsTimer <= 0f)
			{
				base.specRigidbody.CollideWithTileMap = true;
			}
		}
		HandleSparks();
		if (!IsBulletScript)
		{
			HandleRange();
			if (!ManualControl)
			{
				if (PreMoveModifiers != null)
				{
					PreMoveModifiers(this);
				}
				if (OverrideMotionModule != null && !m_usesNormalMoveRegardless)
				{
					OverrideMotionModule.Move(this, m_transform, base.sprite, base.specRigidbody, ref m_timeElapsed, ref m_currentDirection, Inverted, shouldRotate);
					LastVelocity = base.specRigidbody.Velocity;
				}
				else
				{
					Move();
				}
			}
			base.specRigidbody.Velocity *= LocalTimeScale;
			if (!(Owner is PlayerController))
			{
				base.specRigidbody.Velocity *= EnemyBulletSpeedMultiplier;
			}
			DoModifyVelocity();
		}
		Vector2 vector = m_transform.position;
		if (m_isInWall && m_cachedDungeon.data.CheckInBounds((int)vector.x, (int)vector.y))
		{
			CellData cellData = m_cachedDungeon.data[(int)vector.x, (int)vector.y];
			if (cellData != null && cellData.type != CellType.WALL)
			{
				m_isInWall = false;
			}
		}
		if ((shouldFlipHorizontally || shouldFlipVertically) && flag)
		{
			if (shouldFlipHorizontally && shouldRotate && shouldFlipVertically)
			{
				bool flipY = (tk2dBaseSprite2.FlipX = Direction.x < 0f);
				tk2dBaseSprite2.FlipY = flipY;
			}
			else if (shouldFlipHorizontally)
			{
				tk2dBaseSprite2.FlipX = Direction.x < 0f;
			}
			else if (shouldRotate && shouldFlipVertically)
			{
				tk2dBaseSprite2.FlipY = Direction.x < 0f;
			}
		}
		if (m_cachedDungeon != null && !m_cachedDungeon.data.CheckInBounds((int)vector.x, (int)vector.y))
		{
			m_outOfBoundsCounter += BraveTime.DeltaTime;
			if (m_outOfBoundsCounter > 5f)
			{
				base.gameObject.SetActive(false);
				SpawnManager.Despawn(base.gameObject);
			}
		}
		else
		{
			m_outOfBoundsCounter = 0f;
		}
		if (damageTypes != 0)
		{
			HandleGoopChecks();
		}
		if (m_isExitClippingTiles && m_distanceElapsed > m_exitClippingDistance)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreTileCollision = (SpeculativeRigidbody.OnPreTileCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreTileCollision, new SpeculativeRigidbody.OnPreTileCollisionDelegate(PreTileCollisionExitClipping));
			m_isExitClippingTiles = false;
		}
		if (this.OnPostUpdate != null)
		{
			this.OnPostUpdate(this);
		}
	}

	protected virtual void DoModifyVelocity()
	{
		if (ModifyVelocity != null)
		{
			base.specRigidbody.Velocity = ModifyVelocity(base.specRigidbody.Velocity);
			if (base.specRigidbody.Velocity != Vector2.zero)
			{
				m_currentDirection = base.specRigidbody.Velocity.normalized;
			}
		}
	}

	protected void HandleGoopChecks()
	{
		IntVector2 intVector = base.specRigidbody.UnitCenter.ToIntVector2();
		if (!m_cachedDungeon.data.CheckInBounds(intVector))
		{
			return;
		}
		RoomHandler absoluteRoomFromPosition = m_cachedDungeon.data.GetAbsoluteRoomFromPosition(intVector);
		List<DeadlyDeadlyGoopManager> roomGoops = absoluteRoomFromPosition.RoomGoops;
		if (roomGoops != null)
		{
			for (int i = 0; i < roomGoops.Count; i++)
			{
				roomGoops[i].ProcessProjectile(this);
			}
		}
	}

	public virtual void SetNewShooter(SpeculativeRigidbody newShooter)
	{
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.DeregisterSpecificCollisionException(m_shooter);
			if (!allowSelfShooting)
			{
				base.specRigidbody.RegisterSpecificCollisionException(newShooter);
			}
		}
		m_shooter = newShooter;
	}

	public void UpdateSpeed()
	{
		m_currentSpeed = baseData.speed;
	}

	public void UpdateCollisionMask()
	{
		if (!base.specRigidbody)
		{
			return;
		}
		if (collidesWithProjectiles)
		{
			for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
			{
				base.specRigidbody.PixelColliders[i].CollisionLayerCollidableOverride |= CollisionMask.LayerToMask(CollisionLayer.Projectile);
			}
		}
		else
		{
			for (int j = 0; j < base.specRigidbody.PixelColliders.Count; j++)
			{
				base.specRigidbody.PixelColliders[j].CollisionLayerCollidableOverride &= ~CollisionMask.LayerToMask(CollisionLayer.Projectile);
			}
		}
		if (!collidesWithEnemies)
		{
			for (int k = 0; k < base.specRigidbody.PixelColliders.Count; k++)
			{
				base.specRigidbody.PixelColliders[k].CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
			}
		}
		else
		{
			for (int l = 0; l < base.specRigidbody.PixelColliders.Count; l++)
			{
				base.specRigidbody.PixelColliders[l].CollisionLayerIgnoreOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
			}
		}
		if (!collidesWithPlayer)
		{
			for (int m = 0; m < base.specRigidbody.PixelColliders.Count; m++)
			{
				base.specRigidbody.PixelColliders[m].CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox);
			}
		}
		else
		{
			for (int n = 0; n < base.specRigidbody.PixelColliders.Count; n++)
			{
				base.specRigidbody.PixelColliders[n].CollisionLayerIgnoreOverride &= ~CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox);
			}
		}
		if (Owner is PlayerController)
		{
			for (int num = 0; num < base.specRigidbody.PixelColliders.Count; num++)
			{
				base.specRigidbody.PixelColliders[num].CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyBulletBlocker);
			}
		}
		else
		{
			for (int num2 = 0; num2 < base.specRigidbody.PixelColliders.Count; num2++)
			{
				base.specRigidbody.PixelColliders[num2].CollisionLayerIgnoreOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyBulletBlocker);
			}
		}
	}

	public void SendInDirection(Vector2 dirVec, bool resetDistance, bool updateRotation = true)
	{
		if (shouldRotate && updateRotation)
		{
			m_transform.eulerAngles = new Vector3(0f, 0f, dirVec.ToAngle());
		}
		m_currentDirection = dirVec.normalized;
		base.specRigidbody.Velocity = m_currentDirection * m_currentSpeed * LocalTimeScale;
		if (OverrideMotionModule != null)
		{
			OverrideMotionModule.SentInDirection(baseData, m_transform, base.sprite, base.specRigidbody, ref m_timeElapsed, ref m_currentDirection, shouldRotate, dirVec, resetDistance, updateRotation);
		}
		if (resetDistance)
		{
			ResetDistance();
		}
	}

	public void ResetDistance()
	{
		m_distanceElapsed = 0f;
	}

	public float GetElapsedDistance()
	{
		return m_distanceElapsed;
	}

	public void Reflected()
	{
		if (this.OnReflected != null)
		{
			this.OnReflected(this);
		}
	}

	public IEnumerator CheckIfBossKillShot()
	{
		RoomHandler currentPlayerRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
		List<AIActor> enemiesInRoom = currentPlayerRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (enemiesInRoom == null)
		{
			yield break;
		}
		AIActor currentBoss = null;
		if (enemiesInRoom != null)
		{
			for (int i = 0; i < enemiesInRoom.Count; i++)
			{
				if ((bool)enemiesInRoom[i] && enemiesInRoom[i].healthHaver.IsBoss)
				{
					currentBoss = enemiesInRoom[i];
				}
			}
		}
		while (!m_hasImpactedObject)
		{
			bool shouldBreak = true;
			if (currentBoss != null && (bool)currentBoss && (bool)currentBoss.healthHaver && !currentBoss.healthHaver.IsDead && currentBoss.healthHaver.GetCurrentHealth() <= ModifiedDamage)
			{
				shouldBreak = false;
				int mask = CollisionLayerMatrix.GetMask(CollisionLayer.Projectile);
				RaycastResult result;
				if (PhysicsEngine.Instance.RaycastWithIgnores(base.specRigidbody.UnitCenter, m_currentDirection, baseData.range, out result, true, true, mask, null, false, null, new SpeculativeRigidbody[2] { base.specRigidbody, Owner.specRigidbody }) && result.SpeculativeRigidbody == currentBoss.specRigidbody && result.Distance < baseData.range - m_distanceElapsed)
				{
					GameUIRoot.Instance.TriggerBossKillCam(this, currentBoss.specRigidbody);
				}
			}
			if (shouldBreak)
			{
				break;
			}
			yield return null;
		}
	}

	public void HandlePassthroughHitEffects(Vector3 point)
	{
		if (hitEffects != null)
		{
			hitEffects.HandleEnemyImpact(point, 0f, null, Vector2.zero, base.specRigidbody.Velocity, false);
		}
	}

	public void Ramp(float startHeightOffset, float duration)
	{
		if ((bool)base.sprite)
		{
			m_isRamping = true;
			m_rampDuration = duration;
			m_rampTimer = 0f;
			m_startRampHeight = startHeightOffset;
			float currentRampHeight = m_currentRampHeight;
			m_currentRampHeight = m_startRampHeight;
			base.sprite.HeightOffGround -= currentRampHeight - m_currentRampHeight;
			base.sprite.UpdateZDepthLater();
		}
	}

	public virtual float EstimatedTimeToTarget(Vector2 targetPoint, Vector2? overridePos = null)
	{
		Vector2 a = ((!overridePos.HasValue) ? base.specRigidbody.UnitCenter : overridePos.Value);
		return Vector2.Distance(a, targetPoint) / Speed;
	}

	public virtual Vector2 GetPredictedTargetPosition(Vector2 targetCenter, Vector2 targetVelocity, Vector2? overridePos = null, float? overrideProjectileSpeed = null)
	{
		Vector2 aimOrigin = ((!overridePos.HasValue) ? base.specRigidbody.UnitCenter : overridePos.Value);
		float firingSpeed = ((!overrideProjectileSpeed.HasValue) ? baseData.speed : overrideProjectileSpeed.Value);
		return BraveMathCollege.GetPredictedPosition(targetCenter, targetVelocity, aimOrigin, firingSpeed);
	}

	public void RemoveBulletScriptControl()
	{
		if ((bool)braveBulletScript)
		{
			if (braveBulletScript.bullet != null)
			{
				braveBulletScript.bullet.DontDestroyGameObject = true;
			}
			braveBulletScript.RemoveBullet();
			braveBulletScript.enabled = false;
			BulletScriptSettings.surviveRigidbodyCollisions = false;
			BulletScriptSettings.surviveTileCollisions = false;
			IsBulletScript = false;
		}
	}

	public void IgnoreTileCollisionsFor(float time)
	{
		base.specRigidbody.CollideWithTileMap = false;
		m_ignoreTileCollisionsTimer = time;
	}

	public void DoWallExitClipping(float pixelMultiplier = 1f)
	{
		m_isExitClippingTiles = true;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreTileCollision = (SpeculativeRigidbody.OnPreTileCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreTileCollision, new SpeculativeRigidbody.OnPreTileCollisionDelegate(PreTileCollisionExitClipping));
		PixelCollider primaryPixelCollider = base.specRigidbody.PrimaryPixelCollider;
		m_exitClippingDistance = pixelMultiplier * Mathf.Max(primaryPixelCollider.UnitWidth, primaryPixelCollider.UnitHeight);
	}

	protected virtual void Move()
	{
		m_timeElapsed += LocalDeltaTime;
		if (angularVelocity != 0f)
		{
			m_transform.RotateAround(m_transform.position.XY(), Vector3.forward, angularVelocity * LocalDeltaTime);
		}
		if (baseData.UsesCustomAccelerationCurve)
		{
			float time = Mathf.Clamp01((m_timeElapsed - baseData.IgnoreAccelCurveTime) / baseData.CustomAccelerationCurveDuration);
			m_currentSpeed = baseData.AccelerationCurve.Evaluate(time) * baseData.speed;
		}
		base.specRigidbody.Velocity = m_currentDirection * m_currentSpeed;
		m_currentSpeed *= 1f - baseData.damping * LocalDeltaTime;
		LastVelocity = base.specRigidbody.Velocity;
	}

	protected virtual void HandleRange()
	{
		m_distanceElapsed += Vector3.Distance(m_lastPosition, m_transform.position);
		LastPosition = m_transform.position;
		if (!SkipDistanceElapsedCheck && m_distanceElapsed > baseData.range)
		{
			DieInAir();
		}
	}

	public void ForceDestruction()
	{
		HandleDestruction(null);
	}

	protected virtual void HandleDestruction(CollisionData lcr, bool allowActorSpawns = true, bool allowProjectileSpawns = true)
	{
		HandleSparks((lcr == null) ? null : new Vector2?(lcr.Contact));
		if (hitEffects != null && hitEffects.HasProjectileDeathVFX)
		{
			hitEffects.HandleProjectileDeathVFX((lcr == null || hitEffects.CenterDeathVFXOnProjectile) ? base.specRigidbody.UnitCenter : lcr.Contact, 0f, null, (lcr == null) ? Vector2.zero : lcr.Normal, base.specRigidbody.Velocity);
		}
		if ((bool)braveBulletScript)
		{
			if (lcr == null)
			{
				braveBulletScript.HandleBulletDestruction(Bullet.DestroyType.DieInAir, null, allowProjectileSpawns);
			}
			else if ((bool)lcr.OtherRigidbody)
			{
				braveBulletScript.HandleBulletDestruction(Bullet.DestroyType.HitRigidbody, lcr.OtherRigidbody, allowProjectileSpawns);
			}
			else
			{
				braveBulletScript.HandleBulletDestruction(Bullet.DestroyType.HitTile, null, allowProjectileSpawns);
			}
		}
		if (allowProjectileSpawns && baseData.onDestroyBulletScript != null && !baseData.onDestroyBulletScript.IsNull)
		{
			if (lcr != null)
			{
				Vector2 unitCenter = base.specRigidbody.UnitCenter;
				if (!lcr.IsInverse)
				{
					unitCenter += PhysicsEngine.PixelToUnit(lcr.NewPixelsToMove);
				}
				SpawnManager.SpawnBulletScript(Owner, baseData.onDestroyBulletScript, unitCenter + lcr.Normal.normalized * PhysicsEngine.PixelToUnit(2), lcr.Normal, collidesWithEnemies, OwnerName);
			}
			else
			{
				SpawnManager.SpawnBulletScript(Owner, baseData.onDestroyBulletScript, m_transform.position, Vector2.up, collidesWithEnemies, OwnerName);
			}
		}
		if (!string.IsNullOrEmpty(spawnEnemyGuidOnDeath) && allowActorSpawns)
		{
			Vector2 unitCenter2 = base.specRigidbody.UnitCenter;
			IntVector2 intVector = unitCenter2.ToIntVector2(VectorConversions.Floor);
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(intVector);
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(spawnEnemyGuidOnDeath);
			AIActor aIActor = AIActor.Spawn(orLoadByGuid, intVector, roomFromPosition, true);
			if ((bool)aIActor.specRigidbody)
			{
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(aIActor.specRigidbody);
			}
			if (IsBlackBullet && (bool)aIActor)
			{
				aIActor.ForceBlackPhantom = true;
			}
		}
		if (this.OnDestruction != null)
		{
			this.OnDestruction(this);
		}
		switch (DestroyMode)
		{
		case ProjectileDestroyMode.Destroy:
			if (!SpawnManager.Despawn(base.gameObject, m_spawnPool))
			{
				base.gameObject.SetActive(false);
			}
			break;
		case ProjectileDestroyMode.DestroyComponent:
		{
			base.specRigidbody.Velocity = Vector2.zero;
			base.specRigidbody.DeregisterSpecificCollisionException(Shooter);
			for (int j = 0; j < base.specRigidbody.PixelColliders.Count; j++)
			{
				base.specRigidbody.PixelColliders[j].IsTrigger = true;
			}
			SpeculativeRigidbody speculativeRigidbody5 = base.specRigidbody;
			speculativeRigidbody5.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Remove(speculativeRigidbody5.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			SpeculativeRigidbody speculativeRigidbody6 = base.specRigidbody;
			speculativeRigidbody6.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody6.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
			SpeculativeRigidbody speculativeRigidbody7 = base.specRigidbody;
			speculativeRigidbody7.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody7.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
			if (m_isExitClippingTiles)
			{
				SpeculativeRigidbody speculativeRigidbody8 = base.specRigidbody;
				speculativeRigidbody8.OnPreTileCollision = (SpeculativeRigidbody.OnPreTileCollisionDelegate)Delegate.Remove(speculativeRigidbody8.OnPreTileCollision, new SpeculativeRigidbody.OnPreTileCollisionDelegate(PreTileCollisionExitClipping));
			}
			UnityEngine.Object.Destroy(this);
			break;
		}
		case ProjectileDestroyMode.BecomeDebris:
		{
			base.specRigidbody.Velocity = Vector2.zero;
			base.specRigidbody.DeregisterSpecificCollisionException(Shooter);
			for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
			{
				base.specRigidbody.PixelColliders[i].IsTrigger = true;
			}
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
			SpeculativeRigidbody speculativeRigidbody3 = base.specRigidbody;
			speculativeRigidbody3.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody3.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
			if (m_isExitClippingTiles)
			{
				SpeculativeRigidbody speculativeRigidbody4 = base.specRigidbody;
				speculativeRigidbody4.OnPreTileCollision = (SpeculativeRigidbody.OnPreTileCollisionDelegate)Delegate.Remove(speculativeRigidbody4.OnPreTileCollision, new SpeculativeRigidbody.OnPreTileCollisionDelegate(PreTileCollisionExitClipping));
			}
			UnityEngine.Object.Destroy(GetComponentInChildren<SimpleSpriteRotator>());
			DebrisObject debrisObject = BecomeDebris((lcr != null) ? lcr.Normal.ToVector3ZUp(0.1f) : Vector3.zero, 0.5f);
			if (OnBecameDebris != null)
			{
				OnBecameDebris(debrisObject);
			}
			if (OnBecameDebrisGrounded != null)
			{
				debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, OnBecameDebrisGrounded);
			}
			UnityEngine.Object.Destroy(this);
			break;
		}
		}
		if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(onDestroyEventName))
		{
			AkSoundEngine.PostEvent(onDestroyEventName, base.gameObject);
		}
	}

	public DebrisObject BecomeDebris(Vector3 force, float height)
	{
		DebrisObject orAddComponent = base.gameObject.GetOrAddComponent<DebrisObject>();
		orAddComponent.angularVelocity = (shouldRotate ? 45 : 0);
		orAddComponent.angularVelocityVariance = (shouldRotate ? 20 : 0);
		orAddComponent.decayOnBounce = 0.5f;
		orAddComponent.bounceCount = 1;
		orAddComponent.canRotate = shouldRotate;
		orAddComponent.shouldUseSRBMotion = true;
		orAddComponent.AssignFinalWorldDepth(-0.5f);
		orAddComponent.sprite = base.specRigidbody.sprite;
		orAddComponent.animatePitFall = true;
		orAddComponent.Trigger(force, height);
		return orAddComponent;
	}

	public void DieInAir(bool suppressInAirEffects = false, bool allowActorSpawns = true, bool allowProjectileSpawns = true, bool killedEarly = false)
	{
		if (base.gameObject.activeSelf && !m_hasDiedInAir)
		{
			m_hasDiedInAir = true;
			BeamController component = GetComponent<BeamController>();
			if ((bool)component)
			{
				component.DestroyBeam();
			}
			SpawnProjModifier component2 = GetComponent<SpawnProjModifier>();
			if (component2 != null && allowProjectileSpawns && ((component2.spawnProjectilesOnCollision && component2.spawnOnObjectCollisions) || component2.spawnProjecitlesOnDieInAir))
			{
				component2.SpawnCollisionProjectiles(m_transform.position.XY(), base.specRigidbody.Velocity.normalized, null);
			}
			ExplosiveModifier component3 = GetComponent<ExplosiveModifier>();
			if (component3 != null)
			{
				component3.Explode(Vector2.zero, ignoreDamageCaps);
			}
			if (!suppressInAirEffects)
			{
				HandleHitEffectsMidair(killedEarly);
			}
			HandleDestruction(null, allowActorSpawns, allowProjectileSpawns);
		}
	}

	public void ChangeColor(float time, Color color)
	{
		if ((bool)base.sprite)
		{
			if (Owner is PlayerController && (bool)base.sprite.renderer && base.sprite.renderer.material.HasProperty("_VertexColor"))
			{
				base.sprite.usesOverrideMaterial = true;
				base.sprite.renderer.material.SetFloat("_VertexColor", 1f);
			}
			if (time == 0f)
			{
				base.sprite.color = color;
			}
			else
			{
				StartCoroutine(ChangeColorCR(time, color));
			}
		}
	}

	private IEnumerator ChangeColorCR(float time, Color color)
	{
		float timer = 0f;
		while (timer < time)
		{
			base.sprite.color = Color.Lerp(Color.white, color, timer / time);
			timer += LocalDeltaTime;
			yield return null;
		}
		base.sprite.color = color;
	}

	public void ChangeTintColorShader(float time, Color color)
	{
		if (!base.sprite)
		{
			return;
		}
		base.sprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
		Material material = base.sprite.renderer.material;
		bool flag = material.HasProperty("_EmissivePower");
		float value = 0f;
		float value2 = 0f;
		if (flag)
		{
			value = material.GetFloat("_EmissivePower");
			value2 = material.GetFloat("_EmissiveColorPower");
		}
		Shader shader = null;
		shader = (flag ? ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive") : ShaderCache.Acquire("tk2d/CutoutVertexColorTintableTilted"));
		if (base.sprite.renderer.material.shader != shader)
		{
			base.sprite.renderer.material.shader = shader;
			base.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
			if (flag)
			{
				base.sprite.renderer.material.SetFloat("_EmissivePower", value);
				base.sprite.renderer.material.SetFloat("_EmissiveColorPower", value2);
			}
		}
		if (time == 0f)
		{
			base.sprite.renderer.sharedMaterial.SetColor("_OverrideColor", color);
		}
		else
		{
			StartCoroutine(ChangeTintColorCR(time, color));
		}
	}

	private IEnumerator ChangeTintColorCR(float time, Color color)
	{
		float timer = 0f;
		Material targetMaterial = base.sprite.renderer.sharedMaterial;
		while (timer < time)
		{
			targetMaterial.SetColor("_OverrideColor", Color.Lerp(Color.white, color, timer / time));
			timer += LocalDeltaTime;
			yield return null;
		}
		targetMaterial.SetColor("_OverrideColor", color);
	}

	protected void HandleWallDecals(CollisionData lcr, Transform parent)
	{
		if (lcr.Normal.y >= 0f)
		{
			return;
		}
		VFXPool vFXPool = null;
		if (wallDecals != null && wallDecals.effects.Length > 0)
		{
			for (int i = 0; i < wallDecals.effects.Length; i++)
			{
				for (int j = 0; j < wallDecals.effects[i].effects.Length; j++)
				{
					wallDecals.effects[i].effects[j].orphaned = false;
					wallDecals.effects[i].effects[j].destructible = true;
				}
			}
			vFXPool = wallDecals;
		}
		else
		{
			DamageTypeEffectDefinition definitionForType = GameManager.Instance.Dungeon.damageTypeEffectMatrix.GetDefinitionForType(damageTypes);
			if (definitionForType != null)
			{
				vFXPool = definitionForType.wallDecals;
			}
		}
		if (vFXPool != null)
		{
			float num = UnityEngine.Random.value * 0.5f - 0.25f;
			Vector3 position = lcr.Contact.ToVector3ZUp(-0.5f);
			position.y += num;
			vFXPool.SpawnAtPosition(position, 0f, parent, lcr.Normal, base.specRigidbody.Velocity, 0.75f + num, false, SpawnManager.SpawnDecal);
		}
	}

	protected virtual void OnPreTileCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, PhysicsEngine.Tile tile, PixelCollider otherPixelCollider)
	{
		if (!PenetratesInternalWalls)
		{
			return;
		}
		IntVector2 position = tile.Position;
		CellData cellData = GameManager.Instance.Dungeon.data[position];
		if (cellData == null || cellData.isRoomInternal)
		{
			if (!m_isInWall)
			{
				CollisionData obj = CollisionData.Pool.Allocate();
				obj.Normal = BraveUtility.GetMajorAxis(m_transform.position.XY() - tile.Position.ToCenterVector2()).normalized;
				obj.Contact = tile.Position.ToCenterVector2() + obj.Normal / 2f;
				HandleHitEffectsTileMap(obj, false);
				CollisionData.Pool.Free(ref obj);
			}
			m_isInWall = true;
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void PreTileCollisionExitClipping(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, PhysicsEngine.Tile tile, PixelCollider tilePixelCollider)
	{
		if (GameManager.HasInstance && !(GameManager.Instance.Dungeon == null))
		{
			DungeonData data = GameManager.Instance.Dungeon.data;
			int x = tile.Position.x;
			int y = tile.Position.y;
			Vector2 velocity = myRigidbody.Velocity;
			if ((!(velocity.y > 0f) || !data.isFaceWallHigher(x, y)) && (!(velocity.y < 0f) || !data.hasTopWall(x, y)) && (!(velocity.x < 0f) || !data.isLeftSideWall(x, y)) && (!(velocity.x > 0f) || !data.isRightSideWall(x, y)))
			{
				PhysicsEngine.SkipCollision = true;
			}
		}
	}

	protected virtual void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (otherRigidbody == m_shooter && !allowSelfShooting)
		{
			PhysicsEngine.SkipCollision = true;
			return;
		}
		if (otherRigidbody.gameActor != null && otherRigidbody.gameActor is PlayerController && (!collidesWithPlayer || (otherRigidbody.gameActor as PlayerController).IsGhost || (otherRigidbody.gameActor as PlayerController).IsEthereal))
		{
			PhysicsEngine.SkipCollision = true;
			return;
		}
		if ((bool)otherRigidbody.aiActor)
		{
			if (Owner is PlayerController && !otherRigidbody.aiActor.IsNormalEnemy)
			{
				PhysicsEngine.SkipCollision = true;
				return;
			}
			if (Owner is AIActor && !collidesWithEnemies && otherRigidbody.aiActor.IsNormalEnemy && !otherRigidbody.aiActor.HitByEnemyBullets)
			{
				PhysicsEngine.SkipCollision = true;
				return;
			}
		}
		if (!GameManager.PVP_ENABLED && Owner is PlayerController && otherRigidbody.GetComponent<PlayerController>() != null && !allowSelfShooting)
		{
			PhysicsEngine.SkipCollision = true;
			return;
		}
		if (GameManager.Instance.InTutorial)
		{
			PlayerController component = otherRigidbody.GetComponent<PlayerController>();
			if ((bool)component)
			{
				if (component.spriteAnimator.QueryInvulnerabilityFrame())
				{
					GameManager.BroadcastRoomTalkDoerFsmEvent("playerDodgedBullet");
				}
				else if (component.IsDodgeRolling)
				{
					GameManager.BroadcastRoomTalkDoerFsmEvent("playerAlmostDodgedBullet");
				}
				else
				{
					GameManager.BroadcastRoomTalkDoerFsmEvent("playerDidNotDodgeBullet");
				}
			}
		}
		if (otherRigidbody.healthHaver != null && otherRigidbody.healthHaver.spriteAnimator != null && otherCollider.CollisionLayer == CollisionLayer.PlayerHitBox && otherRigidbody.spriteAnimator.QueryInvulnerabilityFrame())
		{
			PhysicsEngine.SkipCollision = true;
			StartCoroutine(HandlePostInvulnerabilityFrameExceptions(otherRigidbody));
		}
		else if (collidesWithProjectiles && collidesOnlyWithPlayerProjectiles && (bool)otherRigidbody.projectile && !(otherRigidbody.projectile.Owner is PlayerController))
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	public void ForceCollision(SpeculativeRigidbody otherRigidbody, LinearCastResult lcr)
	{
		CollisionData obj = CollisionData.Pool.Allocate();
		obj.SetAll(lcr);
		obj.OtherRigidbody = otherRigidbody;
		obj.OtherPixelCollider = otherRigidbody.PrimaryPixelCollider;
		obj.MyRigidbody = base.specRigidbody;
		obj.MyPixelCollider = base.specRigidbody.PrimaryPixelCollider;
		obj.Normal = (base.specRigidbody.UnitCenter - otherRigidbody.UnitCenter).normalized;
		obj.Contact = (otherRigidbody.UnitCenter + base.specRigidbody.UnitCenter) / 2f;
		OnRigidbodyCollision(obj);
		CollisionData.Pool.Free(ref obj);
	}

	protected virtual void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (base.specRigidbody.IsGhostCollisionException(rigidbodyCollision.OtherRigidbody))
		{
			return;
		}
		GameObject target = rigidbodyCollision.OtherRigidbody.gameObject;
		SpeculativeRigidbody otherRigidbody = rigidbodyCollision.OtherRigidbody;
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		bool killedTarget;
		HandleDamageResult handleDamageResult = HandleDamage(rigidbodyCollision.OtherRigidbody, rigidbodyCollision.OtherPixelCollider, out killedTarget, component);
		bool flag = handleDamageResult != HandleDamageResult.NO_HEALTH;
		if ((bool)braveBulletScript && braveBulletScript.bullet != null && BulletScriptSettings.surviveTileCollisions && !flag && rigidbodyCollision.OtherPixelCollider.CollisionLayer == CollisionLayer.HighObstacle)
		{
			if (!otherRigidbody.minorBreakable)
			{
				braveBulletScript.bullet.ManualControl = true;
				braveBulletScript.bullet.Position = base.specRigidbody.UnitCenter;
				PhysicsEngine.PostSliceVelocity = Vector2.zero;
			}
			return;
		}
		HandleSparks(rigidbodyCollision.Contact);
		if (flag)
		{
			m_hasImpactedEnemy = true;
			if (OnHitEnemy != null)
			{
				OnHitEnemy(this, rigidbodyCollision.OtherRigidbody, killedTarget);
			}
		}
		else if (ChallengeManager.CHALLENGE_MODE_ACTIVE && ((bool)otherRigidbody.GetComponent<BeholsterBounceRocket>() || (bool)otherRigidbody.healthHaver || (bool)otherRigidbody.GetComponent<BashelliskBodyPickupController>() || (bool)otherRigidbody.projectile))
		{
			m_hasImpactedEnemy = true;
		}
		PierceProjModifier pierceProjModifier = GetComponent<PierceProjModifier>();
		BounceProjModifier bounceProjModifier = GetComponent<BounceProjModifier>();
		if (m_hasImpactedEnemy && (bool)pierceProjModifier && (bool)otherRigidbody.healthHaver && otherRigidbody.healthHaver.IsBoss && pierceProjModifier.HandleBossImpact())
		{
			bounceProjModifier = null;
			pierceProjModifier = null;
		}
		if ((bool)GetComponent<KeyProjModifier>())
		{
			Chest component2 = otherRigidbody.GetComponent<Chest>();
			if ((bool)component2 && component2.IsLocked && component2.ChestIdentifier != Chest.SpecialChestIdentifier.RAT)
			{
				component2.ForceUnlock();
			}
		}
		MinorBreakable minorBreakable = otherRigidbody.minorBreakable;
		MajorBreakable majorBreakable = otherRigidbody.majorBreakable;
		if (majorBreakable != null)
		{
			float num = 1f;
			if (((m_shooter != null && m_shooter.aiActor != null) || m_owner is AIActor) && majorBreakable.InvulnerableToEnemyBullets)
			{
				num = 0f;
			}
			if (pierceProjModifier != null && pierceProjModifier.BeastModeLevel == PierceProjModifier.BeastModeStatus.BEAST_MODE_LEVEL_ONE)
			{
				num = ((!majorBreakable.ImmuneToBeastMode) ? 1000f : (num + 1f));
			}
			if (!majorBreakable.IsSecretDoor || !(PossibleSourceGun != null) || !PossibleSourceGun.InfiniteAmmo)
			{
				float num2 = ((!(Owner is AIActor)) ? ModifiedDamage : ProjectileData.FixedEnemyDamageToBreakables);
				if (num2 <= 0f && GameManager.Instance.InTutorial)
				{
					majorBreakable.ApplyDamage(1.5f, base.specRigidbody.Velocity, false);
				}
				else
				{
					majorBreakable.ApplyDamage(num2 * num, base.specRigidbody.Velocity, Owner is AIActor);
				}
			}
		}
		if (rigidbodyCollision.OtherRigidbody.PreventPiercing)
		{
			pierceProjModifier = null;
		}
		if (!flag && (bool)bounceProjModifier && !minorBreakable && (!bounceProjModifier.onlyBounceOffTiles || !majorBreakable) && !pierceProjModifier && (!bounceProjModifier.useLayerLimit || rigidbodyCollision.OtherPixelCollider.CollisionLayer == bounceProjModifier.layerLimit))
		{
			OnTileCollision(rigidbodyCollision);
			return;
		}
		bool flag2 = (bool)majorBreakable && majorBreakable.IsSecretDoor;
		if (!majorBreakable && otherRigidbody.name.StartsWith("secret exit collider"))
		{
			flag2 = true;
		}
		if (flag2)
		{
			OnTileCollision(rigidbodyCollision);
			return;
		}
		if (otherRigidbody.ReflectProjectiles)
		{
			AkSoundEngine.PostEvent("Play_OBJ_metalskin_deflect_01", GameManager.Instance.gameObject);
			if (IsBulletScript && (bool)bounceProjModifier && bounceProjModifier.removeBulletScriptControl)
			{
				RemoveBulletScriptControl();
			}
			Vector2 vector = rigidbodyCollision.Normal;
			if (otherRigidbody.ReflectProjectilesNormalGenerator != null)
			{
				vector = otherRigidbody.ReflectProjectilesNormalGenerator(rigidbodyCollision.Contact, rigidbodyCollision.Normal);
			}
			float num3 = (-rigidbodyCollision.MyRigidbody.Velocity).ToAngle();
			float num4 = vector.ToAngle();
			float num5 = BraveMathCollege.ClampAngle360(num3 + 2f * (num4 - num3));
			if (shouldRotate)
			{
				m_transform.rotation = Quaternion.Euler(0f, 0f, num5);
			}
			m_currentDirection = BraveMathCollege.DegreesToVector(num5);
			if ((bool)braveBulletScript && braveBulletScript.bullet != null)
			{
				braveBulletScript.bullet.Direction = num5;
			}
			if (!bounceProjModifier || !bounceProjModifier.suppressHitEffectsOnBounce)
			{
				HandleHitEffectsEnemy(rigidbodyCollision.OtherRigidbody, rigidbodyCollision, false);
			}
			Vector2 value = m_currentDirection * m_currentSpeed * LocalTimeScale;
			PhysicsEngine.PostSliceVelocity = value;
			if ((bool)rigidbodyCollision.OtherRigidbody)
			{
				base.specRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 0f, 0.5f);
				rigidbodyCollision.OtherRigidbody.RegisterTemporaryCollisionException(base.specRigidbody, 0f, 0.5f);
			}
			if ((bool)otherRigidbody.knockbackDoer && otherRigidbody.knockbackDoer.knockbackWhileReflecting)
			{
				HandleKnockback(otherRigidbody, component);
			}
			return;
		}
		bool flag3 = false;
		bool flag4 = false;
		if (flag)
		{
			if (!killedTarget || !(component != null))
			{
				flag3 = true;
			}
			if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(enemyImpactEventName))
			{
				AkSoundEngine.PostEvent("Play_WPN_" + enemyImpactEventName + "_impact_01", base.gameObject);
			}
		}
		else
		{
			flag4 = true;
			if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(objectImpactEventName))
			{
				AkSoundEngine.PostEvent("Play_WPN_" + objectImpactEventName + "_impact_01", base.gameObject);
			}
		}
		if (!s_delayPlayerDamage || !component)
		{
			if (flag)
			{
				if (!rigidbodyCollision.OtherRigidbody.healthHaver.IsDead || killedTarget)
				{
					HandleKnockback(rigidbodyCollision.OtherRigidbody, component, killedTarget);
				}
			}
			else
			{
				HandleKnockback(rigidbodyCollision.OtherRigidbody, component);
			}
		}
		if (!component)
		{
			AppliedEffectBase[] components = GetComponents<AppliedEffectBase>();
			AppliedEffectBase[] array = components;
			foreach (AppliedEffectBase appliedEffectBase in array)
			{
				appliedEffectBase.AddSelfToTarget(target);
			}
		}
		base.specRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 0.01f, 0.5f);
		PhysicsEngine.CollisionHaltsVelocity = false;
		Projectile projectile = rigidbodyCollision.OtherRigidbody.projectile;
		if (CanTransmogrify && flag && handleDamageResult != HandleDamageResult.HEALTH_AND_KILLED && UnityEngine.Random.value < ChanceToTransmogrify && (bool)otherRigidbody.aiActor && !otherRigidbody.aiActor.IsMimicEnemy && (bool)otherRigidbody.aiActor.healthHaver && !otherRigidbody.aiActor.healthHaver.IsBoss && otherRigidbody.aiActor.healthHaver.IsVulnerable)
		{
			otherRigidbody.aiActor.Transmogrify(EnemyDatabase.GetOrLoadByGuid(TransmogrifyTargetGuids[UnityEngine.Random.Range(0, TransmogrifyTargetGuids.Length)]), (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
		}
		if (pierceProjModifier != null && pierceProjModifier.preventPenetrationOfActors && flag)
		{
			pierceProjModifier = null;
		}
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = (bool)otherRigidbody && (bool)otherRigidbody.GetComponent<PlayerOrbital>();
		if (BulletScriptSettings.surviveRigidbodyCollisions)
		{
			flag5 = true;
			flag6 = true;
		}
		else if (pierceProjModifier != null && pierceProjModifier.BeastModeLevel == PierceProjModifier.BeastModeStatus.BEAST_MODE_LEVEL_ONE)
		{
			flag5 = true;
			flag6 = true;
		}
		else if (pierceProjModifier != null && pierceProjModifier.penetration > 0 && flag)
		{
			pierceProjModifier.penetration--;
			flag5 = true;
			flag6 = true;
		}
		else if (pierceProjModifier != null && pierceProjModifier.penetratesBreakables && pierceProjModifier.penetration > 0)
		{
			pierceProjModifier.penetration--;
			flag5 = true;
			flag6 = true;
		}
		else if ((bool)projectile && projectileHitHealth > 0)
		{
			PierceProjModifier component3 = projectile.GetComponent<PierceProjModifier>();
			if (((bool)component3 && component3.BeastModeLevel == PierceProjModifier.BeastModeStatus.BEAST_MODE_LEVEL_ONE) || projectile is RobotechProjectile)
			{
				projectileHitHealth -= 2;
				projectile.m_hasImpactedEnemy = true;
			}
			else
			{
				projectileHitHealth--;
				projectile.m_hasImpactedEnemy = true;
			}
			flag5 = projectileHitHealth >= 0;
			flag6 = flag5;
		}
		else if ((bool)minorBreakable && pierceMinorBreakables)
		{
			flag5 = true;
			flag6 = true;
		}
		else if (bounceProjModifier != null && !flag && !m_hasImpactedEnemy)
		{
			bounceProjModifier.HandleChanceToDie();
			if (flag && bounceProjModifier.ExplodeOnEnemyBounce)
			{
				ExplosiveModifier component4 = GetComponent<ExplosiveModifier>();
				if ((bool)component4)
				{
					bounceProjModifier.numberOfBounces = 0;
				}
			}
			int num6 = 1;
			PierceProjModifier pierceProjModifier2 = null;
			if ((bool)otherRigidbody && (bool)otherRigidbody.projectile)
			{
				pierceProjModifier2 = otherRigidbody.GetComponent<PierceProjModifier>();
			}
			if ((bool)pierceProjModifier2 && pierceProjModifier2.BeastModeLevel == PierceProjModifier.BeastModeStatus.BEAST_MODE_LEVEL_ONE)
			{
				num6 = 2;
			}
			bool flag8 = bounceProjModifier.numberOfBounces - num6 >= 0;
			flag8 &= !bounceProjModifier.useLayerLimit || rigidbodyCollision.OtherPixelCollider.CollisionLayer == bounceProjModifier.layerLimit;
			if (flag8 && !flag7)
			{
				if (IsBulletScript && bounceProjModifier.removeBulletScriptControl)
				{
					RemoveBulletScriptControl();
				}
				Vector2 normal = rigidbodyCollision.Normal;
				if ((bool)rigidbodyCollision.MyRigidbody)
				{
					Vector2 velocity = rigidbodyCollision.MyRigidbody.Velocity;
					float num7 = (-velocity).ToAngle();
					float num8 = normal.ToAngle();
					float num9 = BraveMathCollege.ClampAngle360(num7 + 2f * (num8 - num7));
					if (shouldRotate)
					{
						m_transform.rotation = Quaternion.Euler(0f, 0f, num9);
					}
					m_currentDirection = BraveMathCollege.DegreesToVector(num9);
					m_currentSpeed *= 1f - bounceProjModifier.percentVelocityToLoseOnBounce;
					if ((bool)braveBulletScript && braveBulletScript.bullet != null)
					{
						braveBulletScript.bullet.Direction = num9;
						braveBulletScript.bullet.Speed *= 1f - bounceProjModifier.percentVelocityToLoseOnBounce;
					}
					Vector2 inVec = m_currentDirection * m_currentSpeed * LocalTimeScale;
					inVec = bounceProjModifier.AdjustBounceVector(this, inVec, otherRigidbody);
					if (shouldRotate && inVec.normalized != m_currentDirection)
					{
						m_transform.rotation = Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(inVec.normalized));
					}
					m_currentDirection = inVec.normalized;
					if (this is HelixProjectile)
					{
						(this as HelixProjectile).AdjustRightVector(Mathf.DeltaAngle(velocity.ToAngle(), num9));
					}
					if (OverrideMotionModule != null)
					{
						OverrideMotionModule.UpdateDataOnBounce(Mathf.DeltaAngle(velocity.ToAngle(), num9));
					}
					bounceProjModifier.Bounce(this, rigidbodyCollision.Contact, otherRigidbody);
					PhysicsEngine.PostSliceVelocity = inVec;
					if ((bool)rigidbodyCollision.OtherRigidbody)
					{
						base.specRigidbody.RegisterTemporaryCollisionException(rigidbodyCollision.OtherRigidbody, 0f, 0.5f);
						rigidbodyCollision.OtherRigidbody.RegisterTemporaryCollisionException(base.specRigidbody, 0f, 0.5f);
					}
					flag5 = true;
				}
			}
		}
		if (flag3)
		{
			HandleHitEffectsEnemy(rigidbodyCollision.OtherRigidbody, rigidbodyCollision, !flag6 && !flag5);
		}
		if (flag4)
		{
			HandleHitEffectsObject(rigidbodyCollision.OtherRigidbody, rigidbodyCollision, !flag6 && !flag5);
		}
		m_hasPierced |= flag6;
		if (!flag6 && !flag5 && !m_hasImpactedObject)
		{
			m_hasImpactedObject = true;
			for (int j = 0; j < base.specRigidbody.PixelColliders.Count; j++)
			{
				base.specRigidbody.PixelColliders[j].IsTrigger = true;
			}
			if (flag && base.gameObject.activeInHierarchy)
			{
				StartCoroutine(HandlePostCollisionPersistence(rigidbodyCollision, component));
				return;
			}
			HandleNormalProjectileDeath(rigidbodyCollision, !flag7);
			PhysicsEngine.HaltRemainingMovement = true;
		}
	}

	protected virtual void OnTileCollision(CollisionData tileCollision)
	{
		if ((!damagesWalls || SuppressHitEffects) && (bool)base.specRigidbody && (base.specRigidbody.UnitWidth > 1f || base.specRigidbody.UnitHeight > 1f))
		{
			damagesWalls = true;
			SuppressHitEffects = false;
		}
		BounceProjModifier component = GetComponent<BounceProjModifier>();
		SpawnProjModifier component2 = GetComponent<SpawnProjModifier>();
		ExplosiveModifier component3 = GetComponent<ExplosiveModifier>();
		GoopModifier component4 = GetComponent<GoopModifier>();
		if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(objectImpactEventName))
		{
			AkSoundEngine.PostEvent("Play_WPN_" + objectImpactEventName + "_impact_01", base.gameObject);
		}
		HandleSparks(tileCollision.Contact);
		if (BulletScriptSettings.surviveTileCollisions)
		{
			PhysicsEngine.PostSliceVelocity = Vector2.zero;
			return;
		}
		if (component != null)
		{
			component.HandleChanceToDie();
		}
		if (damagesWalls)
		{
			HandleWallDecals(tileCollision, null);
		}
		bool flag = (bool)tileCollision.OtherRigidbody && (bool)tileCollision.OtherRigidbody.GetComponent<PlayerOrbital>();
		int num = 1;
		PierceProjModifier pierceProjModifier = null;
		if ((bool)tileCollision.OtherRigidbody && (bool)tileCollision.OtherRigidbody.projectile)
		{
			pierceProjModifier = tileCollision.OtherRigidbody.GetComponent<PierceProjModifier>();
		}
		if ((bool)pierceProjModifier && pierceProjModifier.BeastModeLevel == PierceProjModifier.BeastModeStatus.BEAST_MODE_LEVEL_ONE)
		{
			num = 2;
		}
		bool flag2 = component != null && component.numberOfBounces - num >= 0;
		if (flag2)
		{
			flag2 &= !component.useLayerLimit || tileCollision.OtherPixelCollider.CollisionLayer == component.layerLimit;
			flag2 = flag2 && !flag;
		}
		if (flag2)
		{
			if (IsBulletScript && component.removeBulletScriptControl)
			{
				RemoveBulletScriptControl();
			}
			Vector2 vector = tileCollision.Normal;
			if ((bool)tileCollision.OtherRigidbody && tileCollision.OtherRigidbody.ReflectProjectilesNormalGenerator != null)
			{
				vector = tileCollision.OtherRigidbody.ReflectProjectilesNormalGenerator(tileCollision.Contact, vector);
			}
			if (component2 != null && component2.spawnProjectilesOnCollision && component2.spawnCollisionProjectilesOnBounce)
			{
				component2.SpawnCollisionProjectiles(tileCollision.PostCollisionUnitCenter, tileCollision.Normal, null);
			}
			if ((bool)tileCollision.MyRigidbody)
			{
				Vector2 velocity = tileCollision.MyRigidbody.Velocity;
				float num2 = (-velocity).ToAngle();
				float num3 = vector.ToAngle();
				float num4 = BraveMathCollege.ClampAngle360(num2 + 2f * (num3 - num2));
				if (shouldRotate)
				{
					m_transform.rotation = Quaternion.Euler(0f, 0f, num4);
				}
				m_currentDirection = BraveMathCollege.DegreesToVector(num4);
				m_currentSpeed *= 1f - component.percentVelocityToLoseOnBounce;
				if ((bool)braveBulletScript && braveBulletScript.bullet != null)
				{
					braveBulletScript.bullet.Direction = num4;
					braveBulletScript.bullet.Speed *= 1f - component.percentVelocityToLoseOnBounce;
				}
				if (!component.suppressHitEffectsOnBounce)
				{
					HandleHitEffectsTileMap(tileCollision, false);
				}
				Vector2 inVec = m_currentDirection * m_currentSpeed * LocalTimeScale;
				inVec = component.AdjustBounceVector(this, inVec, null);
				if (shouldRotate && inVec.normalized != m_currentDirection)
				{
					m_transform.rotation = Quaternion.Euler(0f, 0f, BraveMathCollege.Atan2Degrees(inVec.normalized));
				}
				m_currentDirection = inVec.normalized;
				if (this is HelixProjectile)
				{
					(this as HelixProjectile).AdjustRightVector(Mathf.DeltaAngle(velocity.ToAngle(), num4));
				}
				if (OverrideMotionModule != null)
				{
					OverrideMotionModule.UpdateDataOnBounce(Mathf.DeltaAngle(velocity.ToAngle(), num4));
				}
				component.Bounce(this, tileCollision.Contact, tileCollision.OtherRigidbody);
				PhysicsEngine.PostSliceVelocity = inVec;
			}
		}
		else
		{
			if (component2 != null && component2.spawnProjectilesOnCollision)
			{
				component2.SpawnCollisionProjectiles(tileCollision.PostCollisionUnitCenter, tileCollision.Normal, null);
			}
			if (component4 != null)
			{
				component4.SpawnCollisionGoop(tileCollision);
			}
			if (component3 != null)
			{
				component3.Explode(tileCollision.Normal, ignoreDamageCaps);
			}
			if (!SuppressHitEffects)
			{
				HandleHitEffectsTileMap(tileCollision, true);
			}
			if (GlobalDungeonData.GUNGEON_EXPERIMENTAL)
			{
				Vector2 vector2 = tileCollision.Contact + (-1f * tileCollision.Normal).normalized * 0.5f;
				IntVector2 intVector = new IntVector2(Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(vector2.y));
				GameManager.Instance.Dungeon.DestroyWallAtPosition(intVector.x, intVector.y);
			}
			bool allowProjectileSpawns = !flag;
			HandleDestruction(tileCollision, true, allowProjectileSpawns);
			PhysicsEngine.HaltRemainingMovement = true;
		}
	}

	public void BeamCollision(Projectile currentProjectile)
	{
		if (collidesWithProjectiles)
		{
			DieInAir();
		}
	}

	private IEnumerator HandlePostInvulnerabilityFrameExceptions(SpeculativeRigidbody otherRigidbody)
	{
		base.specRigidbody.RegisterSpecificCollisionException(otherRigidbody);
		float rigidbodyWidth = (float)otherRigidbody.PrimaryPixelCollider.Width / (float)PhysicsEngine.Instance.PixelsPerUnit;
		yield return new WaitForSeconds(rigidbodyWidth / m_currentSpeed * 2f);
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.DeregisterSpecificCollisionException(otherRigidbody);
		}
	}

	private IEnumerator HandlePostCollisionPersistence(CollisionData lcr, PlayerController player)
	{
		CollisionData persistentCollisionData = CollisionData.Pool.Allocate();
		persistentCollisionData.SetAll(lcr);
		OverrideTrailPoint = lcr.Contact;
		if (m_currentSpeed < 20f)
		{
			yield return new WaitForSeconds(persistTime);
		}
		if (s_delayPlayerDamage && (bool)player && !player.spriteAnimator.QueryInvulnerabilityFrame())
		{
			bool killedTarget;
			HandleDamage(lcr.OtherRigidbody, lcr.OtherPixelCollider, out killedTarget, player, true);
			HandleKnockback(lcr.OtherRigidbody, player, killedTarget, true);
		}
		HandleNormalProjectileDeath(persistentCollisionData);
		CollisionData.Pool.Free(ref persistentCollisionData);
	}

	private void HandleNormalProjectileDeath(CollisionData lcr, bool allowProjectileSpawns = true)
	{
		SpawnProjModifier component = GetComponent<SpawnProjModifier>();
		if (component != null && allowProjectileSpawns && component.spawnProjectilesOnCollision && component.spawnOnObjectCollisions)
		{
			Vector2 contact = SafeCenter;
			if (lcr != null && (bool)lcr.MyRigidbody)
			{
				contact = lcr.PostCollisionUnitCenter;
			}
			component.SpawnCollisionProjectiles(contact, lcr.Normal, lcr.OtherRigidbody, true);
		}
		GoopModifier component2 = GetComponent<GoopModifier>();
		if (component2 != null)
		{
			if (lcr == null)
			{
				component2.SpawnCollisionGoop(SafeCenter);
			}
			else
			{
				component2.SpawnCollisionGoop(lcr);
			}
		}
		ExplosiveModifier component3 = GetComponent<ExplosiveModifier>();
		if (component3 != null)
		{
			component3.Explode(Vector2.zero, ignoreDamageCaps, lcr);
		}
		bool allowProjectileSpawns2 = allowProjectileSpawns;
		HandleDestruction(lcr, true, allowProjectileSpawns2);
	}

	protected virtual HandleDamageResult HandleDamage(SpeculativeRigidbody rigidbody, PixelCollider hitPixelCollider, out bool killedTarget, PlayerController player, bool alreadyPlayerDelayed = false)
	{
		killedTarget = false;
		if (rigidbody.ReflectProjectiles)
		{
			return HandleDamageResult.NO_HEALTH;
		}
		if (!rigidbody.healthHaver)
		{
			return HandleDamageResult.NO_HEALTH;
		}
		if (!alreadyPlayerDelayed && s_delayPlayerDamage && (bool)player)
		{
			return HandleDamageResult.HEALTH;
		}
		if (rigidbody.spriteAnimator != null && rigidbody.spriteAnimator.QueryInvulnerabilityFrame())
		{
			return HandleDamageResult.HEALTH;
		}
		bool flag = !rigidbody.healthHaver.IsDead;
		float num = ModifiedDamage;
		if (Owner is AIActor && (bool)rigidbody && (bool)rigidbody.aiActor && (Owner as AIActor).IsNormalEnemy)
		{
			num = ProjectileData.FixedFallbackDamageToEnemies;
			if (rigidbody.aiActor.HitByEnemyBullets)
			{
				num /= 4f;
			}
		}
		if (Owner is PlayerController && m_hasPierced && m_healthHaverHitCount >= 1)
		{
			int num2 = Mathf.Clamp(m_healthHaverHitCount - 1, 0, GameManager.Instance.PierceDamageScaling.Length - 1);
			num *= GameManager.Instance.PierceDamageScaling[num2];
		}
		if (OnWillKillEnemy != null && num >= rigidbody.healthHaver.GetCurrentHealth())
		{
			OnWillKillEnemy(this, rigidbody);
		}
		if (rigidbody.healthHaver.IsBoss)
		{
			num *= BossDamageMultiplier;
		}
		if (BlackPhantomDamageMultiplier != 1f && (bool)rigidbody.aiActor && rigidbody.aiActor.IsBlackPhantom)
		{
			num *= BlackPhantomDamageMultiplier;
		}
		bool flag2 = false;
		if (DelayedDamageToExploders)
		{
			flag2 = (bool)rigidbody.GetComponent<ExplodeOnDeath>() && rigidbody.healthHaver.GetCurrentHealth() <= num;
		}
		if (!flag2)
		{
			HealthHaver obj = rigidbody.healthHaver;
			float damage = num;
			Vector2 velocity = base.specRigidbody.Velocity;
			string ownerName = OwnerName;
			CoreDamageTypes coreDamageTypes = damageTypes;
			DamageCategory damageCategory = (IsBlackBullet ? DamageCategory.BlackBullet : DamageCategory.Normal);
			obj.ApplyDamage(damage, velocity, ownerName, coreDamageTypes, damageCategory, false, hitPixelCollider, ignoreDamageCaps);
			if ((bool)player && player.OnHitByProjectile != null)
			{
				player.OnHitByProjectile(this, player);
			}
		}
		else
		{
			rigidbody.StartCoroutine(HandleDelayedDamage(rigidbody, num, base.specRigidbody.Velocity, hitPixelCollider));
		}
		if ((bool)Owner && Owner is AIActor && (bool)player)
		{
			(Owner as AIActor).HasDamagedPlayer = true;
		}
		killedTarget = flag && rigidbody.healthHaver.IsDead;
		if (!killedTarget && rigidbody.gameActor != null)
		{
			if (AppliesPoison && UnityEngine.Random.value < PoisonApplyChance)
			{
				rigidbody.gameActor.ApplyEffect(healthEffect);
			}
			if (AppliesSpeedModifier && UnityEngine.Random.value < SpeedApplyChance)
			{
				rigidbody.gameActor.ApplyEffect(speedEffect);
			}
			if (AppliesCharm && UnityEngine.Random.value < CharmApplyChance)
			{
				rigidbody.gameActor.ApplyEffect(charmEffect);
			}
			if (AppliesFreeze && UnityEngine.Random.value < FreezeApplyChance)
			{
				rigidbody.gameActor.ApplyEffect(freezeEffect);
			}
			if (AppliesCheese && UnityEngine.Random.value < CheeseApplyChance)
			{
				rigidbody.gameActor.ApplyEffect(cheeseEffect);
			}
			if (AppliesBleed && UnityEngine.Random.value < BleedApplyChance)
			{
				rigidbody.gameActor.ApplyEffect(bleedEffect, -1f, this);
			}
			if (AppliesFire && UnityEngine.Random.value < FireApplyChance)
			{
				rigidbody.gameActor.ApplyEffect(fireEffect);
			}
			if (AppliesStun && UnityEngine.Random.value < StunApplyChance && (bool)rigidbody.gameActor.behaviorSpeculator)
			{
				rigidbody.gameActor.behaviorSpeculator.Stun(AppliedStunDuration);
			}
			for (int i = 0; i < statusEffectsToApply.Count; i++)
			{
				rigidbody.gameActor.ApplyEffect(statusEffectsToApply[i]);
			}
		}
		m_healthHaverHitCount++;
		return (!killedTarget) ? HandleDamageResult.HEALTH : HandleDamageResult.HEALTH_AND_KILLED;
	}

	private IEnumerator HandleDelayedDamage(SpeculativeRigidbody targetRigidbody, float damage, Vector2 damageVec, PixelCollider hitPixelCollider)
	{
		yield return new WaitForSeconds(0.5f);
		if ((bool)targetRigidbody && (bool)targetRigidbody.healthHaver)
		{
			HealthHaver obj = targetRigidbody.healthHaver;
			string ownerName = OwnerName;
			CoreDamageTypes coreDamageTypes = damageTypes;
			DamageCategory damageCategory = (IsBlackBullet ? DamageCategory.BlackBullet : DamageCategory.Normal);
			obj.ApplyDamage(damage, damageVec, ownerName, coreDamageTypes, damageCategory, false, hitPixelCollider, ignoreDamageCaps);
		}
	}

	public void HandleKnockback(SpeculativeRigidbody rigidbody, PlayerController player, bool killedTarget = false, bool alreadyPlayerDelayed = false)
	{
		if (!alreadyPlayerDelayed && s_delayPlayerDamage && (bool)player)
		{
			return;
		}
		KnockbackDoer knockbackDoer = rigidbody.knockbackDoer;
		Vector2 direction = LastVelocity;
		if (HasFixedKnockbackDirection)
		{
			direction = BraveMathCollege.DegreesToVector(FixedKnockbackDirection);
		}
		if ((bool)knockbackDoer)
		{
			if (killedTarget)
			{
				knockbackDoer.ApplySourcedKnockback(direction, baseData.force * knockbackDoer.deathMultiplier, base.gameObject);
			}
			else
			{
				knockbackDoer.ApplySourcedKnockback(direction, baseData.force, base.gameObject);
			}
		}
	}

	protected virtual void HandleHitEffectsEnemy(SpeculativeRigidbody rigidbody, CollisionData lcr, bool playProjectileDeathVfx)
	{
		if (hitEffects == null)
		{
			return;
		}
		if (hitEffects.alwaysUseMidair)
		{
			HandleHitEffectsMidair();
			return;
		}
		Vector3 position = lcr.Contact.ToVector3ZUp(-1f);
		float num = 0f;
		bool flag = false;
		if (rigidbody != null)
		{
			HitEffectHandler hitEffectHandler = rigidbody.hitEffectHandler;
			if (hitEffectHandler != null)
			{
				if (hitEffectHandler.SuppressAllHitEffects)
				{
					flag = true;
				}
				else
				{
					if (hitEffectHandler.additionalHitEffects.Length > 0)
					{
						hitEffectHandler.HandleAdditionalHitEffects(base.specRigidbody.Velocity, lcr.OtherPixelCollider);
					}
					if (hitEffectHandler.overrideHitEffectPool != null && hitEffectHandler.overrideHitEffectPool.type != 0)
					{
						hitEffectHandler.overrideHitEffectPool.SpawnAtPosition(position, num, rigidbody.transform, lcr.Normal, base.specRigidbody.Velocity);
						flag = true;
					}
					else if (hitEffectHandler.overrideHitEffect != null && hitEffectHandler.overrideHitEffect.effects.Length > 0)
					{
						hitEffectHandler.overrideHitEffect.SpawnAtPosition(position, num, rigidbody.transform, lcr.Normal, base.specRigidbody.Velocity);
						flag = true;
					}
				}
			}
		}
		if (!flag)
		{
			hitEffects.HandleEnemyImpact(position, num, rigidbody.transform, lcr.Normal, base.specRigidbody.Velocity, playProjectileDeathVfx);
		}
	}

	protected void HandleHitEffectsObject(SpeculativeRigidbody srb, CollisionData lcr, bool playProjectileDeathVfx)
	{
		if (hitEffects == null)
		{
			return;
		}
		if (hitEffects.alwaysUseMidair)
		{
			HandleHitEffectsMidair();
			return;
		}
		Vector3 vector = lcr.Contact.ToVector3ZUp(-1f);
		float rotation = Mathf.Atan2(lcr.Normal.y, lcr.Normal.x) * 57.29578f;
		bool flag = false;
		if (srb != null)
		{
			HitEffectHandler hitEffectHandler = srb.hitEffectHandler;
			if (hitEffectHandler != null)
			{
				if (hitEffectHandler.SuppressAllHitEffects)
				{
					flag = true;
				}
				else if (hitEffectHandler.overrideMaterialDefinition != null)
				{
					VFXComplex.SpawnMethod overrideSpawnMethod = ((!CenterTilemapHitEffectsByProjectileVelocity) ? null : new VFXComplex.SpawnMethod(SpawnVFXProjectileCenter));
					if (Mathf.Abs(lcr.Normal.x) > Mathf.Abs(lcr.Normal.y))
					{
						if (hitEffects.HasTileMapHorizontalEffects)
						{
							hitEffects.HandleTileMapImpactHorizontal(vector, rotation, lcr.Normal, base.specRigidbody.Velocity, playProjectileDeathVfx, srb.transform, overrideSpawnMethod, SpawnVFXPostProcessStickyGrenades);
							flag = true;
						}
						else if (hitEffectHandler.overrideMaterialDefinition.fallbackHorizontalTileMapEffects.Length > 0)
						{
							hitEffectHandler.overrideMaterialDefinition.SpawnRandomHorizontal(vector, rotation, srb.transform, lcr.Normal, base.specRigidbody.Velocity);
							flag = true;
						}
					}
					else if (hitEffects.HasTileMapVerticalEffects)
					{
						if (lcr.Normal.y > 0f)
						{
							hitEffects.HandleTileMapImpactVertical(vector, -0.25f, rotation, lcr.Normal, base.specRigidbody.Velocity, playProjectileDeathVfx, srb.transform, overrideSpawnMethod, SpawnVFXPostProcessStickyGrenades);
						}
						else
						{
							hitEffects.HandleTileMapImpactVertical(vector, 0.25f, rotation, lcr.Normal, base.specRigidbody.Velocity, playProjectileDeathVfx, srb.transform, overrideSpawnMethod, SpawnVFXPostProcessStickyGrenades);
						}
						flag = true;
					}
					else if (hitEffectHandler.overrideMaterialDefinition.fallbackVerticalTileMapEffects.Length > 0)
					{
						hitEffectHandler.overrideMaterialDefinition.SpawnRandomVertical(vector, rotation, srb.transform, lcr.Normal, base.specRigidbody.Velocity);
						flag = true;
					}
					if (damagesWalls)
					{
						Vector3 vector2 = lcr.Normal.normalized.ToVector3ZUp() * 0.1f;
						Vector3 position = vector + vector2;
						float damage = ((!(Owner is AIActor)) ? ModifiedDamage : ProjectileData.FixedEnemyDamageToBreakables);
						hitEffectHandler.overrideMaterialDefinition.SpawnRandomShard(position, lcr.Normal, damage);
					}
					HandleWallDecals(lcr, srb.transform);
				}
				else if (hitEffectHandler.overrideHitEffectPool != null && hitEffectHandler.overrideHitEffectPool.type != 0)
				{
					hitEffectHandler.overrideHitEffectPool.SpawnAtPosition(vector, 0f, srb.transform, lcr.Normal, base.specRigidbody.Velocity);
					flag = true;
				}
				else if (hitEffectHandler.overrideHitEffect != null && hitEffectHandler.overrideHitEffect.effects.Length > 0)
				{
					hitEffectHandler.overrideHitEffect.SpawnAtPosition(vector, 0f, srb.transform, lcr.Normal, base.specRigidbody.Velocity);
					flag = true;
				}
			}
		}
		if (this is SharkProjectile)
		{
			flag = true;
		}
		if (!flag)
		{
			hitEffects.HandleEnemyImpact(vector, 0f, null, lcr.Normal, base.specRigidbody.Velocity, true);
		}
	}

	public void LerpMaterialGlow(Material targetMaterial, float startGlow, float targetGlow, float duration)
	{
		StartCoroutine(LerpMaterialGlowCR(targetMaterial, startGlow, targetGlow, duration));
	}

	private IEnumerator LerpMaterialGlowCR(Material targetMaterial, float startGlow, float targetGlow, float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += LocalDeltaTime;
			float t = elapsed / duration;
			if (targetMaterial != null)
			{
				targetMaterial.SetFloat("_EmissivePower", Mathf.Lerp(startGlow, targetGlow, t));
			}
			yield return null;
		}
	}

	public GameObject SpawnVFXPostProcessStickyGrenades(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(prefab, position, rotation);
		StickyGrenadeBuff component = GetComponent<StickyGrenadeBuff>();
		if ((bool)component)
		{
			StickyGrenadePersistentDebris component2 = gameObject.GetComponent<StickyGrenadePersistentDebris>();
			if ((bool)component2)
			{
				component2.InitializeSelf(component);
			}
		}
		StrafeBleedBuff component3 = GetComponent<StrafeBleedBuff>();
		if ((bool)component3)
		{
			StrafeBleedPersistentDebris component4 = gameObject.GetComponent<StrafeBleedPersistentDebris>();
			if ((bool)component4)
			{
				component4.InitializeSelf(component3);
			}
		}
		return gameObject;
	}

	public GameObject SpawnVFXProjectileCenter(GameObject prefab, Vector3 position, Quaternion rotation, bool ignoresPools)
	{
		Vector3 vector = position;
		if ((bool)base.specRigidbody)
		{
			vector = base.specRigidbody.UnitCenter.ToVector3ZUp(position.z);
			float num = Vector2.Distance(vector, position);
			vector += LastVelocity.normalized.ToVector3ZUp() * num;
		}
		return SpawnManager.SpawnVFX(prefab, vector, rotation);
	}

	protected void HandleHitEffectsTileMap(CollisionData lcr, bool playProjectileDeathVfx)
	{
		if (hitEffects == null)
		{
			return;
		}
		if (hitEffects.alwaysUseMidair)
		{
			HandleHitEffectsMidair();
			return;
		}
		int x = Mathf.RoundToInt(lcr.Contact.x);
		int y = Mathf.RoundToInt(lcr.Contact.y);
		float num = 0f;
		if (GameManager.Instance.Dungeon.data.CheckInBounds(x, y))
		{
			CellData cellData = GameManager.Instance.Dungeon.data[x, y];
			if (cellData != null && cellData.diagonalWallType != 0)
			{
				if (cellData.diagonalWallType == DiagonalWallType.NORTHEAST || cellData.diagonalWallType == DiagonalWallType.NORTHWEST)
				{
					lcr.Normal = Vector2.down;
				}
				else
				{
					lcr.Normal = Vector2.up;
				}
			}
		}
		Vector3 vector = lcr.Contact.ToVector3ZUp(-1f);
		float rotation = Mathf.Atan2(lcr.Normal.y, lcr.Normal.x) * 57.29578f;
		VFXComplex.SpawnMethod overrideSpawnMethod = ((!CenterTilemapHitEffectsByProjectileVelocity) ? null : new VFXComplex.SpawnMethod(SpawnVFXProjectileCenter));
		if (lcr.Normal.y < -0.1f)
		{
			hitEffects.HandleTileMapImpactVertical(vector, 0.5f + num, rotation, lcr.Normal, base.specRigidbody.Velocity, playProjectileDeathVfx, null, overrideSpawnMethod, SpawnVFXPostProcessStickyGrenades);
		}
		else if (lcr.Normal.y > 0.1f)
		{
			hitEffects.HandleTileMapImpactVertical(vector, -0.25f + num, rotation, lcr.Normal, base.specRigidbody.Velocity, playProjectileDeathVfx, null, overrideSpawnMethod, SpawnVFXPostProcessStickyGrenades);
		}
		else
		{
			hitEffects.HandleTileMapImpactHorizontal(vector, rotation, lcr.Normal, base.specRigidbody.Velocity, playProjectileDeathVfx, null, overrideSpawnMethod, SpawnVFXPostProcessStickyGrenades);
		}
		if (damagesWalls)
		{
			Vector3 vector2 = lcr.Normal.normalized.ToVector3ZUp() * 0.1f;
			Vector3 vector3 = vector + vector2;
			if (GameManager.Instance.Dungeon != null)
			{
				int roomVisualTypeAtPosition = GameManager.Instance.Dungeon.data.GetRoomVisualTypeAtPosition(vector3.XY());
				float damage = ((!(Owner is AIActor)) ? ModifiedDamage : ProjectileData.FixedEnemyDamageToBreakables);
				GameManager.Instance.Dungeon.roomMaterialDefinitions[roomVisualTypeAtPosition].SpawnRandomShard(vector3, lcr.Normal, damage);
			}
		}
	}

	protected void HandleHitEffectsMidair(bool killedEarly = false)
	{
		if (hitEffects == null)
		{
			return;
		}
		if (killedEarly && hitEffects.overrideEarlyDeathVfx != null)
		{
			SpawnManager.SpawnVFX(hitEffects.overrideEarlyDeathVfx, m_transform.position, Quaternion.identity);
		}
		else
		{
			if (hitEffects.suppressMidairDeathVfx)
			{
				return;
			}
			if (hitEffects.overrideMidairDeathVFX != null || hitEffects.alwaysUseMidair)
			{
				GameObject gameObject = SpawnManager.SpawnVFX(hitEffects.overrideMidairDeathVFX, m_transform.position, (!hitEffects.midairInheritsRotation) ? Quaternion.identity : m_transform.rotation);
				BraveBehaviour component = gameObject.GetComponent<BraveBehaviour>();
				if (hitEffects.midairInheritsFlip)
				{
					component.sprite.FlipX = base.sprite.FlipX;
					component.sprite.FlipY = base.sprite.FlipY;
				}
				if (hitEffects.overrideMidairZHeight != -1)
				{
					component.sprite.HeightOffGround = hitEffects.overrideMidairZHeight;
				}
				if (hitEffects.midairInheritsVelocity)
				{
					if ((bool)component.debris)
					{
						component.debris.Trigger(base.specRigidbody.Velocity.ToVector3ZUp(0.5f), 0.1f);
					}
					else
					{
						SimpleMover orAddComponent = gameObject.GetOrAddComponent<SimpleMover>();
						orAddComponent.velocity = m_currentDirection * m_currentSpeed * 0.4f;
						if (component.spriteAnimator != null)
						{
							float num = (float)component.spriteAnimator.DefaultClip.frames.Length / component.spriteAnimator.DefaultClip.fps;
							orAddComponent.acceleration = orAddComponent.velocity / num * -1f;
						}
					}
				}
				else if ((bool)component.debris)
				{
					component.debris.Trigger(new Vector3(UnityEngine.Random.value * 2f - 1f, UnityEngine.Random.value * 2f - 1f, 5f), 0.1f);
				}
				if ((bool)component.particleSystem)
				{
					gameObject.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
				}
			}
			else if (hitEffects != null && (bool)base.specRigidbody)
			{
				hitEffects.HandleTileMapImpactVertical(m_transform.position, 0f, 0f, Vector2.zero, base.specRigidbody.Velocity, false);
			}
		}
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.RemoveProjectile(this);
		base.OnDestroy();
	}

	public void OnSpawned()
	{
		if (m_cachedBaseData == null)
		{
			m_cachedCollidesWithPlayer = collidesWithPlayer;
			m_cachedCollidesWithProjectiles = collidesWithProjectiles;
			m_cachedCollidesWithEnemies = collidesWithEnemies;
			m_cachedDamagesWalls = damagesWalls;
			m_cachedBaseData = new ProjectileData(baseData);
			m_cachedBulletScriptSettings = new BulletScriptSettings(BulletScriptSettings);
			if ((bool)base.specRigidbody)
			{
				m_cachedCollideWithTileMap = base.specRigidbody.CollideWithTileMap;
				m_cachedCollideWithOthers = base.specRigidbody.CollideWithOthers;
			}
			if (!base.sprite)
			{
				base.sprite = GetComponentInChildren<tk2dSprite>();
			}
			if (!base.spriteAnimator && (bool)base.sprite)
			{
				base.spriteAnimator = base.sprite.spriteAnimator;
			}
			if ((bool)base.sprite)
			{
				m_cachedSpriteId = base.sprite.spriteId;
			}
		}
		if (base.enabled)
		{
			Start();
			base.specRigidbody.enabled = true;
			for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
			{
				base.specRigidbody.PixelColliders[i].IsTrigger = false;
			}
			base.specRigidbody.Reinitialize();
			if ((bool)TrailRenderer)
			{
				TrailRenderer.Clear();
			}
			if ((bool)CustomTrailRenderer)
			{
				CustomTrailRenderer.Clear();
			}
			if ((bool)ParticleTrail)
			{
				BraveUtility.EnableEmission(ParticleTrail, true);
			}
		}
		m_spawnPool = SpawnManager.LastPrefabPool;
	}

	public virtual void OnDespawned()
	{
		Cleanup();
	}

	public void BecomeBlackBullet()
	{
		if (!IsBlackBullet && (bool)base.sprite)
		{
			ForceBlackBullet = true;
			IsBlackBullet = true;
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.SetFloat("_BlackBullet", 1f);
			base.sprite.renderer.material.SetFloat("_EmissivePower", -40f);
		}
	}

	public void ReturnFromBlackBullet()
	{
		if (IsBlackBullet)
		{
			IsBlackBullet = false;
			base.sprite.renderer.material.SetFloat("_BlackBullet", 0f);
			base.sprite.usesOverrideMaterial = false;
			base.sprite.ForceUpdateMaterial();
		}
	}

	private void Cleanup()
	{
		StaticReferenceManager.RemoveProjectile(this);
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.enabled = false;
		}
		ManualControl = false;
		IsBulletScript = false;
		SuppressHitEffects = false;
		ReturnFromBlackBullet();
		m_forceBlackBullet = false;
		collidesWithPlayer = m_cachedCollidesWithPlayer;
		collidesWithProjectiles = m_cachedCollidesWithProjectiles;
		collidesWithEnemies = m_cachedCollidesWithEnemies;
		damagesWalls = m_cachedDamagesWalls;
		m_timeElapsed = 0f;
		m_distanceElapsed = 0f;
		LastPosition = Vector3.zero;
		m_hasImpactedObject = false;
		m_hasDiedInAir = false;
		m_hasPierced = false;
		m_healthHaverHitCount = 0;
		m_ignoreTileCollisionsTimer = 0f;
		if (m_cachedBaseData != null && baseData != null)
		{
			baseData.SetAll(m_cachedBaseData);
		}
		if ((bool)TrailRenderer)
		{
			TrailRenderer.Clear();
		}
		if ((bool)CustomTrailRenderer)
		{
			CustomTrailRenderer.Clear();
		}
		if ((bool)ParticleTrail)
		{
			BraveUtility.EnableEmission(ParticleTrail, false);
		}
		if (m_cachedBulletScriptSettings != null && BulletScriptSettings != null)
		{
			BulletScriptSettings.SetAll(m_cachedBulletScriptSettings);
		}
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.CollideWithTileMap = m_cachedCollideWithTileMap;
			base.specRigidbody.CollideWithOthers = m_cachedCollideWithOthers;
			base.specRigidbody.ClearSpecificCollisionExceptions();
		}
		if ((bool)base.spriteAnimator && !base.spriteAnimator.playAutomatically)
		{
			base.spriteAnimator.Stop();
		}
		if ((bool)base.sprite && m_cachedSpriteId >= 0)
		{
			base.sprite.SetSprite(m_cachedSpriteId);
		}
		if ((bool)base.sprite && m_isRamping)
		{
			m_isRamping = false;
			m_currentRampHeight = 0f;
			base.sprite.HeightOffGround = CurrentProjectileDepth;
		}
		Owner = null;
		m_shooter = null;
		TrapOwner = null;
		OwnerName = null;
		m_spawnPool = null;
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.Cleanup();
		}
		m_initialized = false;
		if ((bool)base.specRigidbody)
		{
			PhysicsEngine.Instance.DeregisterWhenAvailable(base.specRigidbody);
		}
	}
}
