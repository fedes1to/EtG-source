using System;
using UnityEngine;

public class ModifyProjectileSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public bool TintsBullets;

	public Color BulletTint;

	[Header("Spawn Proj Modifiers")]
	public bool IncreaseSpawnedProjectileCount;

	[ShowInInspectorIf("IncreaseSpawnedProjectileCount", false)]
	public float SpawnedProjectileCountMultiplier = 2f;

	public bool IncreasesSpawnProjectileRate;

	[ShowInInspectorIf("IncreasesSpawnProjectileRate", false)]
	public float SpawnProjectileRateMultiplier = 1f;

	public bool AddsSpawnedProjectileInFlight;

	[ShowInInspectorIf("AddsSpawnedProjectileInFlight", false)]
	public Projectile AddFlightSpawnedProjectile;

	[ShowInInspectorIf("AddsSpawnedProjectileInFlight", false)]
	public float InFlightSpawnCooldown = 1f;

	[ShowInInspectorIf("AddsSpawnedProjectileInFlight", false)]
	public string InFlightAudioEvent;

	public bool AddsSpawnedProjectileOnDeath;

	[ShowInInspectorIf("AddsSpawnedProjectileOnDeath", false)]
	public Projectile AddDeathSpawnedProjectile;

	[ShowInInspectorIf("AddsSpawnedProjectileOnDeath", false)]
	public int NumDeathSpawnProjectiles;

	[ShowInInspectorIf("AddsSpawnedProjectileOnDeath", false)]
	public bool OnlySpawnDeathProjectilesInAir;

	[Header("Other Settings")]
	public int AddsBounces;

	public int AddsPierces;

	public bool AddsHoming;

	[ShowInInspectorIf("AddsHoming", false)]
	public float HomingRadius = 5f;

	[ShowInInspectorIf("AddsHoming", false)]
	public float HomingAngularVelocity = 360f;

	[ShowInInspectorIf("AddsHoming", false)]
	public bool HomingIsLockOn;

	[ShowInInspectorIf("HomingIsLockOn", false)]
	public GameObject LockOnVFX;

	public bool OverridesPreviousEffects;

	public bool AddsFire;

	public GameActorFireEffect FireEffect;

	public bool AddsPoison;

	public GameActorHealthEffect PoisonEffect;

	public bool AddsFreeze;

	public GameActorFreezeEffect FreezeEffect;

	public bool AddsSlow;

	public GameActorSpeedEffect SpeedEffect;

	public bool CopiesDevolverModifier;

	[ShowInInspectorIf("CopiesDevolverModifier", false)]
	public DevolverModifier DevolverSourceModifier;

	public bool AddsExplosion;

	public ExplosionData Explosion;

	public float BossDamageMultiplier = 1f;

	public float DamageMultiplier = 1f;

	public float RangeMultiplier = 1f;

	public float ScaleMultiplier = 1f;

	public float SpeedMultiplier = 1f;

	public bool AddsAccelCurve;

	public AnimationCurve AccelCurve;

	public float AccelCurveTime = 1f;

	public bool AddsChainLightning;

	[ShowInInspectorIf("AddsChainLightning", false)]
	public GameObject ChainLinkVFX;

	public bool AddsTransmogrifyChance;

	public float TransmogrifyChance;

	[EnemyIdentifier]
	public string[] TransmogrifyTargetGuids;

	public bool AddsStun;

	public float StunChance;

	public float StunDuration = 2f;

	public bool Dejams;

	public bool Blanks;

	private Projectile m_projectile;

	private void Awake()
	{
		m_projectile = GetComponent<Projectile>();
		if (Dejams)
		{
			Projectile projectile = m_projectile;
			projectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(projectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(DejamEnemy));
		}
		if (Blanks)
		{
			m_projectile.OnDestruction += DoBlank;
		}
	}

	private void DoBlank(Projectile obj)
	{
		if ((bool)m_projectile && m_projectile.Owner is PlayerController)
		{
			PlayerController playerController = m_projectile.Owner as PlayerController;
			Vector2? overrideCenter = m_projectile.specRigidbody.UnitCenter;
			playerController.ForceBlank(25f, 0.5f, false, true, overrideCenter);
		}
	}

	private void DejamEnemy(Projectile source, SpeculativeRigidbody target, bool kill)
	{
		if ((bool)target && (bool)target.aiActor && target.aiActor.IsBlackPhantom)
		{
			target.aiActor.UnbecomeBlackPhantom();
		}
	}

	private void Start()
	{
		PlayerController playerController = m_projectile.Owner as PlayerController;
		if (!playerController || !playerController.HasActiveBonusSynergy(SynergyToCheck))
		{
			return;
		}
		if (TintsBullets)
		{
			m_projectile.AdjustPlayerProjectileTint(BulletTint, 0);
		}
		if (IncreaseSpawnedProjectileCount)
		{
			SpawnProjModifier component = m_projectile.GetComponent<SpawnProjModifier>();
			component.numToSpawnInFlight = (int)((float)component.numToSpawnInFlight * SpawnedProjectileCountMultiplier);
			component.numberToSpawnOnCollison = (int)((float)component.numberToSpawnOnCollison * SpawnedProjectileCountMultiplier);
		}
		if (IncreasesSpawnProjectileRate)
		{
			SpawnProjModifier component2 = m_projectile.GetComponent<SpawnProjModifier>();
			component2.inFlightSpawnCooldown *= SpawnProjectileRateMultiplier;
		}
		if (AddsSpawnedProjectileInFlight)
		{
			SpawnProjModifier component3 = m_projectile.GetComponent<SpawnProjModifier>();
			if (!component3)
			{
				component3 = m_projectile.gameObject.AddComponent<SpawnProjModifier>();
				component3.spawnProjectilesInFlight = true;
				component3.projectileToSpawnInFlight = AddFlightSpawnedProjectile;
				component3.numToSpawnInFlight = 1;
				component3.inFlightSpawnCooldown = InFlightSpawnCooldown;
				component3.inFlightAimAtEnemies = true;
				component3.spawnAudioEvent = InFlightAudioEvent;
			}
		}
		if (AddsSpawnedProjectileOnDeath)
		{
			SpawnProjModifier component4 = m_projectile.GetComponent<SpawnProjModifier>();
			if (!component4)
			{
				component4 = m_projectile.gameObject.AddComponent<SpawnProjModifier>();
				component4.spawnProjectilesOnCollision = !OnlySpawnDeathProjectilesInAir;
				component4.spawnProjecitlesOnDieInAir = true;
				component4.projectileToSpawnOnCollision = AddDeathSpawnedProjectile;
				if (OnlySpawnDeathProjectilesInAir)
				{
					component4.collisionSpawnStyle = SpawnProjModifier.CollisionSpawnStyle.FLAK_BURST;
				}
				if (NumDeathSpawnProjectiles == 1)
				{
					component4.alignToSurfaceNormal = true;
				}
				component4.numberToSpawnOnCollison = NumDeathSpawnProjectiles;
			}
		}
		if (AddsFire && (!m_projectile.AppliesFire || OverridesPreviousEffects))
		{
			m_projectile.AppliesFire = true;
			m_projectile.fireEffect = FireEffect;
		}
		if (AddsPoison && (!m_projectile.AppliesPoison || OverridesPreviousEffects))
		{
			m_projectile.AppliesPoison = true;
			m_projectile.healthEffect = PoisonEffect;
		}
		if (AddsFreeze && (!m_projectile.AppliesFreeze || OverridesPreviousEffects))
		{
			m_projectile.AppliesFreeze = true;
			m_projectile.freezeEffect = FreezeEffect;
		}
		if (AddsSlow && (!m_projectile.AppliesSpeedModifier || OverridesPreviousEffects))
		{
			m_projectile.AppliesSpeedModifier = true;
			m_projectile.speedEffect = SpeedEffect;
		}
		if (AddsExplosion)
		{
			ExplosiveModifier component5 = m_projectile.GetComponent<ExplosiveModifier>();
			if (!component5)
			{
				component5 = m_projectile.gameObject.AddComponent<ExplosiveModifier>();
				component5.explosionData = Explosion;
			}
		}
		if (AddsHoming)
		{
			if (HomingIsLockOn)
			{
				LockOnHomingModifier lockOnHomingModifier = m_projectile.GetComponent<LockOnHomingModifier>();
				if (!lockOnHomingModifier)
				{
					lockOnHomingModifier = m_projectile.gameObject.AddComponent<LockOnHomingModifier>();
					lockOnHomingModifier.HomingRadius = 0f;
					lockOnHomingModifier.AngularVelocity = 0f;
				}
				lockOnHomingModifier.HomingRadius += HomingRadius;
				lockOnHomingModifier.AngularVelocity += HomingAngularVelocity;
				lockOnHomingModifier.LockOnVFX = LockOnVFX;
			}
			else
			{
				HomingModifier homingModifier = m_projectile.GetComponent<HomingModifier>();
				if (!homingModifier)
				{
					homingModifier = m_projectile.gameObject.AddComponent<HomingModifier>();
					homingModifier.HomingRadius = 0f;
					homingModifier.AngularVelocity = 0f;
				}
				homingModifier.HomingRadius += HomingRadius;
				homingModifier.AngularVelocity += HomingAngularVelocity;
			}
		}
		if (AddsBounces > 0)
		{
			BounceProjModifier orAddComponent = m_projectile.gameObject.GetOrAddComponent<BounceProjModifier>();
			orAddComponent.numberOfBounces += AddsBounces;
		}
		if (AddsPierces > 0)
		{
			PierceProjModifier orAddComponent2 = m_projectile.gameObject.GetOrAddComponent<PierceProjModifier>();
			orAddComponent2.penetration += AddsPierces;
		}
		if (CopiesDevolverModifier)
		{
			DevolverModifier devolverModifier = m_projectile.gameObject.AddComponent<DevolverModifier>();
			devolverModifier.chanceToDevolve = DevolverSourceModifier.chanceToDevolve;
			devolverModifier.DevolverHierarchy = DevolverSourceModifier.DevolverHierarchy;
			devolverModifier.EnemyGuidsToIgnore = DevolverSourceModifier.EnemyGuidsToIgnore;
		}
		if (AddsChainLightning)
		{
			ChainLightningModifier component6 = m_projectile.GetComponent<ChainLightningModifier>();
			if (!component6)
			{
				component6 = m_projectile.gameObject.AddComponent<ChainLightningModifier>();
				component6.LinkVFXPrefab = ChainLinkVFX;
				component6.maximumLinkDistance = 7f;
				component6.damagePerHit = 5f;
				component6.damageCooldown = 1f;
			}
		}
		if (BossDamageMultiplier != 1f)
		{
			m_projectile.BossDamageMultiplier *= BossDamageMultiplier;
		}
		if (DamageMultiplier != 1f)
		{
			m_projectile.baseData.damage *= DamageMultiplier;
		}
		if (RangeMultiplier != 1f)
		{
			m_projectile.baseData.range *= RangeMultiplier;
		}
		if (ScaleMultiplier != 1f)
		{
			m_projectile.RuntimeUpdateScale(ScaleMultiplier);
		}
		if (SpeedMultiplier != 1f)
		{
			m_projectile.baseData.speed *= SpeedMultiplier;
			m_projectile.UpdateSpeed();
		}
		if (AddsAccelCurve)
		{
			m_projectile.baseData.AccelerationCurve = AccelCurve;
			m_projectile.baseData.UsesCustomAccelerationCurve = true;
			m_projectile.baseData.CustomAccelerationCurveDuration = AccelCurveTime;
		}
		if (AddsTransmogrifyChance && !m_projectile.CanTransmogrify)
		{
			m_projectile.CanTransmogrify = true;
			m_projectile.ChanceToTransmogrify = TransmogrifyChance;
			m_projectile.TransmogrifyTargetGuids = TransmogrifyTargetGuids;
		}
		if (AddsStun && !m_projectile.AppliesStun)
		{
			m_projectile.AppliesStun = true;
			m_projectile.StunApplyChance = StunChance;
			m_projectile.AppliedStunDuration = StunDuration;
		}
	}
}
