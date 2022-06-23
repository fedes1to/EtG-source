using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class Gun : PickupObject, IPlayerInteractable
{
	public enum AttackResult
	{
		Success,
		OnCooldown,
		Reload,
		Empty,
		Fail
	}

	public static bool ActiveReloadActivated;

	public static bool ActiveReloadActivatedPlayerTwo;

	public static float s_DualWieldFactor = 0.75f;

	public string gunName = "gun";

	[FormerlySerializedAs("overrideAudioGunName")]
	public string gunSwitchGroup = string.Empty;

	public bool isAudioLoop;

	public bool lowersAudioWhileFiring;

	public GunClass gunClass;

	[SerializeField]
	public StatModifier[] currentGunStatModifiers;

	[SerializeField]
	public StatModifier[] passiveStatModifiers;

	[SerializeField]
	public DamageTypeModifier[] currentGunDamageTypeModifiers;

	[SerializeField]
	public int ArmorToGainOnPickup;

	public Transform barrelOffset;

	public Transform muzzleOffset;

	public Transform chargeOffset;

	public Transform reloadOffset;

	public IntVector2 carryPixelOffset;

	public IntVector2 carryPixelUpOffset;

	public IntVector2 carryPixelDownOffset;

	public bool UsesPerCharacterCarryPixelOffsets;

	public CharacterCarryPixelOffset[] PerCharacterPixelOffsets;

	public IntVector2 leftFacingPixelOffset;

	public GunHandedness gunHandedness;

	public GunHandedness overrideOutOfAmmoHandedness;

	public AdditionalHandState additionalHandState;

	public GunPositionOverride gunPosition;

	public bool forceFlat;

	[FormerlySerializedAs("volley")]
	[SerializeField]
	private ProjectileVolleyData rawVolley;

	public ProjectileModule singleModule;

	[SerializeField]
	public ProjectileVolleyData rawOptionalReloadVolley;

	[NonSerialized]
	public bool OverrideFinaleAudio;

	[NonSerialized]
	public bool HasFiredHolsterShot;

	[NonSerialized]
	public bool HasFiredReloadSynergy;

	[NonSerialized]
	public ProjectileVolleyData modifiedVolley;

	[NonSerialized]
	public ProjectileVolleyData modifiedFinalVolley;

	[NonSerialized]
	public ProjectileVolleyData modifiedOptionalReloadVolley;

	[NonSerialized]
	public List<int> DuctTapeMergedGunIDs;

	[NonSerialized]
	public bool PreventNormalFireAudio;

	[NonSerialized]
	public string OverrideNormalFireAudioEvent;

	public int ammo = 25;

	public bool CanGainAmmo = true;

	[FormerlySerializedAs("InfiniteAmmo")]
	public bool LocalInfiniteAmmo;

	public const float c_FallbackBossDamageModifier = 0.8f;

	public const float c_LuteCompanionDamageMultiplier = 2f;

	public const float c_LuteCompanionScaleMultiplier = 1.75f;

	public const float c_LuteCompanionFireRateMultiplier = 1.5f;

	public bool UsesBossDamageModifier;

	public float CustomBossDamageModifier = -1f;

	[SerializeField]
	private int maxAmmo = -1;

	public float reloadTime;

	[NonSerialized]
	public bool CanReloadNoMatterAmmo;

	public bool blankDuringReload;

	[ShowInInspectorIf("blankDuringReload", false)]
	public float blankReloadRadius = 1f;

	[ShowInInspectorIf("blankDuringReload", false)]
	public bool reflectDuringReload;

	[ShowInInspectorIf("blankDuringReload", false)]
	public float blankKnockbackPower = 20f;

	[ShowInInspectorIf("blankDuringReload", false)]
	public float blankDamageToEnemies;

	[ShowInInspectorIf("blankDuringReload", false)]
	public float blankDamageScalingOnEmptyClip = 1f;

	[NonSerialized]
	private float AdditionalReloadMultiplier = 1f;

	[NonSerialized]
	private int SequentialActiveReloads;

	public bool doesScreenShake = true;

	public ScreenShakeSettings gunScreenShake = new ScreenShakeSettings(1f, 1f, 0.5f, 0.5f);

	public bool directionlessScreenShake;

	public int damageModifier;

	public GameObject thrownObject;

	public ProceduralGunData procGunData;

	public ActiveReloadData activeReloadData;

	public bool ClearsCooldownsLikeAWP;

	public bool AppliesHoming;

	public float AppliedHomingAngularVelocity = 180f;

	public float AppliedHomingDetectRadius = 4f;

	[SerializeField]
	private bool m_unswitchableGun;

	[CheckAnimation(null)]
	public string shootAnimation = string.Empty;

	[ShowInInspectorIf("shootAnimation", false)]
	public bool usesContinuousFireAnimation;

	[CheckAnimation(null)]
	public string reloadAnimation = string.Empty;

	[CheckAnimation(null)]
	public string emptyReloadAnimation = string.Empty;

	[CheckAnimation(null)]
	public string idleAnimation = string.Empty;

	[CheckAnimation(null)]
	public string chargeAnimation = string.Empty;

	[CheckAnimation(null)]
	public string dischargeAnimation = string.Empty;

	[CheckAnimation(null)]
	public string emptyAnimation = string.Empty;

	[CheckAnimation(null)]
	public string introAnimation = string.Empty;

	[CheckAnimation(null)]
	public string finalShootAnimation = string.Empty;

	[CheckAnimation(null)]
	public string enemyPreFireAnimation = string.Empty;

	[CheckAnimation(null)]
	public string outOfAmmoAnimation = string.Empty;

	[CheckAnimation(null)]
	public string criticalFireAnimation = string.Empty;

	[CheckAnimation(null)]
	public string dodgeAnimation = string.Empty;

	public bool usesDirectionalIdleAnimations;

	public bool usesDirectionalAnimator;

	public bool preventRotation;

	public VFXPool muzzleFlashEffects;

	[ShowInInspectorIf("muzzleFlashEffects", false)]
	public bool usesContinuousMuzzleFlash;

	public VFXPool finalMuzzleFlashEffects;

	public VFXPool reloadEffects;

	public VFXPool emptyReloadEffects;

	public VFXPool activeReloadSuccessEffects;

	public VFXPool activeReloadFailedEffects;

	public Light light;

	public float baseLightIntensity;

	public GameObject shellCasing;

	public int shellsToLaunchOnFire = 1;

	public int shellCasingOnFireFrameDelay;

	public int shellsToLaunchOnReload;

	public int reloadShellLaunchFrame;

	public GameObject clipObject;

	public int clipsToLaunchOnReload;

	public int reloadClipLaunchFrame;

	[HideInInspector]
	public string prefabName = string.Empty;

	public bool rampBullets;

	public float rampStartHeight = 2f;

	public float rampTime = 1f;

	public bool IgnoresAngleQuantization;

	public bool IsTrickGun;

	public bool TrickGunAlternatesHandedness;

	public bool PreventOutlines;

	public ProjectileVolleyData alternateVolley;

	[CheckAnimation(null)]
	public string alternateShootAnimation;

	[CheckAnimation(null)]
	public string alternateReloadAnimation;

	[CheckAnimation(null)]
	public string alternateIdleAnimation;

	public string alternateSwitchGroup;

	public bool IsHeroSword;

	public bool HeroSwordDoesntBlank;

	public bool StarterGunForAchievement;

	private float HeroSwordCooldown;

	public bool CanSneakAttack;

	public float SneakAttackDamageMultiplier = 3f;

	public bool SuppressLaserSight;

	public bool RequiresFundsToShoot;

	public int CurrencyCostPerShot = 1;

	public GunWeaponPanelSpriteOverride weaponPanelSpriteOverride;

	public bool IsLuteCompanionBuff;

	public bool MovesPlayerForwardOnChargeFire;

	public bool LockedHorizontalOnCharge;

	public float LockedHorizontalCenterFireOffset = -1f;

	[NonSerialized]
	public bool LockedHorizontalOnReload;

	private float LockedHorizontalCachedAngle = -1f;

	public bool GoopReloadsFree;

	public bool IsUndertaleGun;

	public bool LocalActiveReload;

	public bool UsesRechargeLikeActiveItem;

	public float ActiveItemStyleRechargeAmount = 100f;

	public bool CanAttackThroughObjects;

	private float m_remainingActiveCooldownAmount;

	public bool CanCriticalFire;

	public float CriticalChance = 0.1f;

	public float CriticalDamageMultiplier = 3f;

	public VFXPool CriticalMuzzleFlashEffects;

	public Projectile CriticalReplacementProjectile;

	public bool GainsRateOfFireAsContinueAttack;

	public float RateOfFireMultiplierAdditionPerSecond;

	public bool OnlyUsesIdleInWeaponBox;

	public bool DisablesRendererOnCooldown;

	[FormerlySerializedAs("ObjectToInstantiatedOnClipDepleted")]
	[SerializeField]
	public GameObject ObjectToInstantiateOnReload;

	[NonSerialized]
	public int AdditionalClipCapacity;

	private RoomHandler m_minimapIconRoom;

	private GameObject m_instanceMinimapIcon;

	private GunHandedness? m_cachedGunHandedness;

	public Action<GameActor> OnInitializedWithOwner;

	public Action<Projectile> PostProcessProjectile;

	public Action<ProjectileVolleyData> PostProcessVolley;

	public Action OnDropped;

	public Action<PlayerController, Gun> OnAutoReload;

	public Action<PlayerController, Gun, bool> OnReloadPressed;

	public Action<PlayerController, Gun> OnFinishAttack;

	public Action<PlayerController, Gun> OnPostFired;

	public Action<PlayerController, Gun> OnAmmoChanged;

	public Action<PlayerController, Gun> OnBurstContinued;

	public Func<float, float> OnReflectedBulletDamageModifier;

	public Func<float, float> OnReflectedBulletScaleModifier;

	[NonSerialized]
	private tk2dTiledSprite m_extantLaserSight;

	[NonSerialized]
	public int LastShotIndex = -1;

	[NonSerialized]
	public bool DidTransformGunThisFrame;

	[NonSerialized]
	public float CustomLaserSightDistance = 30f;

	[NonSerialized]
	public float CustomLaserSightHeight = 0.25f;

	[NonSerialized]
	public AIActor LastLaserSightEnemy;

	private GameObject m_extantLockOnSprite;

	private bool m_hasReinitializedAudioSwitch;

	[NonSerialized]
	public bool HasEverBeenAcquiredByPlayer;

	private SingleSpawnableGunPlacedObject m_extantAmp;

	[NonSerialized]
	public bool ForceNextShotCritical;

	private bool m_isCritting;

	public Func<Gun, Projectile, ProjectileModule, Projectile> OnPreFireProjectileModifier;

	public Func<float, float> ModifyActiveCooldownDamage;

	private const bool c_clickingCanActiveReload = true;

	private const bool c_DUAL_WIELD_PARALLEL_RELOAD = false;

	private bool m_hasSwappedTrickGunsThisCycle;

	protected List<ModuleShootData> m_activeBeams = new List<ModuleShootData>();

	private string[] m_directionalIdleNames;

	private bool m_preventIdleLoop;

	private bool m_hasDecrementedFunds;

	private Transform m_throwPrepTransform;

	private tk2dBaseSprite m_sprite;

	private tk2dSpriteAnimator m_anim;

	private GameActor m_owner;

	private int m_defaultSpriteID;

	private Transform m_transform;

	private Transform m_attachTransform;

	private List<Transform> m_childTransformsToFlip;

	private Vector3 m_defaultLocalPosition;

	private MeshRenderer m_meshRenderer;

	private Transform m_clipLaunchAttachPoint;

	private Transform m_localAttachPoint;

	private Transform m_offhandAttachPoint;

	private Transform m_casingLaunchAttachPoint;

	private float gunAngle;

	private float prevGunAngleUnmodified;

	private float gunCooldownModifier;

	private Vector2 m_localAimPoint;

	private Vector3 m_unroundedBarrelPosition;

	private Vector3 m_originalBarrelOffsetPosition;

	private Vector3 m_originalMuzzleOffsetPosition;

	private Vector3 m_originalChargeOffsetPosition;

	private float m_fractionalAmmoUsage;

	public bool HasBeenPickedUp;

	private bool m_reloadWhenDoneFiring;

	private bool m_isReloading;

	private bool m_isThrown;

	private bool m_thrownOnGround = true;

	private bool m_canAttemptActiveReload;

	private bool m_isCurrentlyFiring;

	private bool m_isAudioLooping;

	private float m_continuousAttackTime;

	private float m_reloadElapsed;

	private bool m_hasDoneSingleReloadBlankEffect;

	private bool m_cachedIsGunBlocked;

	private bool m_playedEmptyClipSound;

	private VFXPool m_currentlyPlayingChargeVFX;

	private bool m_midBurstFire;

	private bool m_continueBurstInUpdate;

	private bool m_isContinuousMuzzleFlashOut;

	private Dictionary<ProjectileModule, ModuleShootData> m_moduleData;

	[NonSerialized]
	private List<ActiveAmmunitionData> m_customAmmunitions = new List<ActiveAmmunitionData>();

	private int m_currentStrengthTier;

	[NonSerialized]
	public Dictionary<string, string> AdditionalShootSoundsByModule = new Dictionary<string, string>();

	[NonSerialized]
	public float? OverrideAngleSnap;

	private bool m_isPreppedForThrow;

	private float m_prepThrowTime = -0.3f;

	private const float c_prepTime = 1.2f;

	private const bool c_attackingCanReload = true;

	private const bool c_throwGunOnFire = true;

	public GameActor CurrentOwner
	{
		get
		{
			return m_owner;
		}
	}

	public ProjectileVolleyData RawSourceVolley
	{
		get
		{
			return rawVolley;
		}
		set
		{
			rawVolley = value;
		}
	}

	public ProjectileVolleyData Volley
	{
		get
		{
			return modifiedVolley ?? rawVolley;
		}
		set
		{
			rawVolley = value;
		}
	}

	public ProjectileVolleyData OptionalReloadVolley
	{
		get
		{
			return modifiedOptionalReloadVolley ?? rawOptionalReloadVolley;
		}
	}

	public int CurrentAmmo
	{
		get
		{
			if (RequiresFundsToShoot && m_owner is PlayerController)
			{
				return ClipShotsRemaining;
			}
			return ammo;
		}
		set
		{
			ammo = value;
		}
	}

	public bool InfiniteAmmo
	{
		get
		{
			if ((bool)m_owner && m_owner is PlayerController)
			{
				return LocalInfiniteAmmo || (m_owner as PlayerController).InfiniteAmmo.Value;
			}
			return LocalInfiniteAmmo;
		}
		set
		{
			LocalInfiniteAmmo = value;
		}
	}

	public int AdjustedMaxAmmo
	{
		get
		{
			if (InfiniteAmmo)
			{
				return int.MaxValue;
			}
			if (m_owner == null)
			{
				return maxAmmo;
			}
			if (m_owner is PlayerController)
			{
				if (RequiresFundsToShoot)
				{
					return ClipShotsRemaining;
				}
				if ((m_owner as PlayerController).stats != null)
				{
					float statValue = (m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.AmmoCapacityMultiplier);
					return Mathf.RoundToInt(statValue * (float)maxAmmo);
				}
				return maxAmmo;
			}
			return maxAmmo;
		}
	}

	public float AdjustedReloadTime
	{
		get
		{
			float num = 1f;
			if (m_owner is PlayerController)
			{
				PlayerController playerController = m_owner as PlayerController;
				if ((bool)playerController.CurrentGun && (bool)playerController.CurrentSecondaryGun && playerController.CurrentSecondaryGun == this && playerController.CurrentGun != this)
				{
					return playerController.CurrentGun.AdjustedReloadTime;
				}
				num = playerController.stats.GetStatValue(PlayerStats.StatType.ReloadSpeed);
			}
			return reloadTime * num * AdditionalReloadMultiplier;
		}
	}

	public bool UnswitchableGun
	{
		get
		{
			return m_unswitchableGun;
		}
	}

	public bool LuteCompanionBuffActive
	{
		get
		{
			return IsLuteCompanionBuff && IsFiring;
		}
	}

	public float RemainingActiveCooldownAmount
	{
		get
		{
			return m_remainingActiveCooldownAmount;
		}
		set
		{
			if (m_remainingActiveCooldownAmount > 0f && value <= 0f && (bool)m_owner)
			{
				AkSoundEngine.PostEvent("Play_UI_cooldown_ready_01", m_owner.gameObject);
			}
			m_remainingActiveCooldownAmount = value;
		}
	}

	public float CurrentActiveItemChargeAmount
	{
		get
		{
			return Mathf.Clamp01(1f - m_remainingActiveCooldownAmount / ActiveItemStyleRechargeAmount);
		}
	}

	public float CurrentAngle
	{
		get
		{
			return gunAngle;
		}
	}

	public ProjectileModule DefaultModule
	{
		get
		{
			if ((bool)Volley)
			{
				if (Volley.ModulesAreTiers)
				{
					for (int i = 0; i < Volley.projectiles.Count; i++)
					{
						ProjectileModule projectileModule = Volley.projectiles[i];
						if (projectileModule != null)
						{
							int num = ((projectileModule.CloneSourceIndex < 0) ? i : projectileModule.CloneSourceIndex);
							if (num == CurrentStrengthTier)
							{
								return projectileModule;
							}
						}
					}
				}
				return Volley.projectiles[0];
			}
			return singleModule;
		}
	}

	public bool IsAutomatic
	{
		get
		{
			return DefaultModule.shootStyle == ProjectileModule.ShootStyle.Automatic;
		}
	}

	public bool HasChargedProjectileReady
	{
		get
		{
			if (!m_isCurrentlyFiring)
			{
				return false;
			}
			if (Volley == null)
			{
				if (singleModule.shootStyle == ProjectileModule.ShootStyle.Charged)
				{
					ModuleShootData moduleShootData = m_moduleData[singleModule];
					ProjectileModule.ChargeProjectile chargeProjectile = singleModule.GetChargeProjectile(moduleShootData.chargeTime);
					if (chargeProjectile != null && (bool)chargeProjectile.Projectile)
					{
						return true;
					}
				}
				return false;
			}
			ProjectileVolleyData volley = Volley;
			for (int i = 0; i < volley.projectiles.Count; i++)
			{
				ProjectileModule projectileModule = volley.projectiles[i];
				if (projectileModule.shootStyle == ProjectileModule.ShootStyle.Charged)
				{
					ModuleShootData moduleShootData2 = m_moduleData[projectileModule];
					ProjectileModule.ChargeProjectile chargeProjectile2 = projectileModule.GetChargeProjectile(moduleShootData2.chargeTime);
					if (chargeProjectile2 != null && (bool)chargeProjectile2.Projectile)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public GameUIAmmoType.AmmoType AmmoType
	{
		get
		{
			return DefaultModule.ammoType;
		}
	}

	public string CustomAmmoType
	{
		get
		{
			return DefaultModule.customAmmoType;
		}
	}

	public override string DisplayName
	{
		get
		{
			EncounterTrackable component = GetComponent<EncounterTrackable>();
			if ((bool)component)
			{
				return component.GetModifiedDisplayName();
			}
			return gunName;
		}
	}

	public int ClipShotsRemaining
	{
		get
		{
			if (RequiresFundsToShoot && m_owner is PlayerController)
			{
				return Mathf.FloorToInt((float)(m_owner as PlayerController).carriedConsumables.Currency / (float)CurrencyCostPerShot);
			}
			int num = ammo;
			num = ((m_moduleData != null && m_moduleData.ContainsKey(DefaultModule)) ? ((DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) > 0) ? (DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) - m_moduleData[DefaultModule].numberShotsFired) : ammo) : ((DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) > 0) ? DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) : ammo));
			if (num > ammo)
			{
				ClipShotsRemaining = ammo;
			}
			return Mathf.Min(num, ammo);
		}
		set
		{
			if (m_moduleData.ContainsKey(DefaultModule))
			{
				m_moduleData[DefaultModule].numberShotsFired = DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) - value;
			}
		}
	}

	public bool IsEmpty
	{
		get
		{
			return (!(Volley != null)) ? (!CheckHasLoadedModule(singleModule)) : (!CheckHasLoadedModule(Volley));
		}
	}

	public int ClipCapacity
	{
		get
		{
			return (DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) > 0) ? DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) : AdjustedMaxAmmo;
		}
	}

	private Vector3 ClipLaunchPoint
	{
		get
		{
			return (!(m_clipLaunchAttachPoint == null)) ? m_clipLaunchAttachPoint.position : Vector3.zero;
		}
	}

	private Vector3 CasingLaunchPoint
	{
		get
		{
			return (!(m_casingLaunchAttachPoint == null)) ? m_casingLaunchAttachPoint.position : barrelOffset.position;
		}
	}

	public GunHandedness Handedness
	{
		get
		{
			bool flag = m_owner is PlayerController && (m_owner as PlayerController).inventory != null && (m_owner as PlayerController).inventory.DualWielding;
			if (ammo == 0 && overrideOutOfAmmoHandedness != 0)
			{
				return overrideOutOfAmmoHandedness;
			}
			if (IsPreppedForThrow)
			{
				return GunHandedness.OneHanded;
			}
			if (!m_cachedGunHandedness.HasValue)
			{
				if (gunHandedness == GunHandedness.AutoDetect)
				{
					Transform transform = base.transform.Find("SecondaryHand");
					bool flag2 = transform != null;
					if (IsTrickGun && TrickGunAlternatesHandedness)
					{
						flag2 = transform != null && transform.gameObject.activeSelf;
					}
					m_cachedGunHandedness = (flag2 ? GunHandedness.TwoHanded : GunHandedness.OneHanded);
				}
				else
				{
					m_cachedGunHandedness = gunHandedness;
				}
			}
			GunHandedness? cachedGunHandedness = m_cachedGunHandedness;
			if (cachedGunHandedness.GetValueOrDefault() == GunHandedness.TwoHanded && cachedGunHandedness.HasValue && flag)
			{
				return GunHandedness.OneHanded;
			}
			return m_cachedGunHandedness.Value;
		}
	}

	public bool IsForwardPosition
	{
		get
		{
			switch (gunPosition)
			{
			case GunPositionOverride.AutoDetect:
				return Handedness == GunHandedness.OneHanded || Handedness == GunHandedness.HiddenOneHanded;
			case GunPositionOverride.Forward:
				return true;
			case GunPositionOverride.Back:
				return false;
			default:
				Debug.LogWarning("Unhandled GunPositionOverride type: " + gunPosition);
				return true;
			}
		}
	}

	public Transform PrimaryHandAttachPoint
	{
		get
		{
			return m_localAttachPoint;
		}
	}

	public Transform SecondaryHandAttachPoint
	{
		get
		{
			if (IsTrickGun && TrickGunAlternatesHandedness && m_offhandAttachPoint == null)
			{
				m_offhandAttachPoint = base.transform.Find("SecondaryHand");
			}
			return m_offhandAttachPoint;
		}
	}

	public bool IsFiring
	{
		get
		{
			return m_isCurrentlyFiring;
		}
	}

	public bool IsReloading
	{
		get
		{
			return m_isReloading;
		}
	}

	public bool IsCharging
	{
		get
		{
			if (!m_isCurrentlyFiring)
			{
				return false;
			}
			if (Volley != null)
			{
				for (int i = 0; i < Volley.projectiles.Count; i++)
				{
					ProjectileModule projectileModule = Volley.projectiles[i];
					if (projectileModule.shootStyle == ProjectileModule.ShootStyle.Charged && !m_moduleData[projectileModule].chargeFired)
					{
						return true;
					}
				}
			}
			else if (singleModule.shootStyle == ProjectileModule.ShootStyle.Charged && !m_moduleData[singleModule].chargeFired)
			{
				return true;
			}
			return false;
		}
	}

	public bool NoOwnerOverride { get; set; }

	public Projectile LastProjectile { get; set; }

	public int DefaultSpriteID
	{
		get
		{
			return m_defaultSpriteID;
		}
		set
		{
			m_defaultSpriteID = value;
		}
	}

	public bool IsInWorld
	{
		get
		{
			return m_isThrown;
		}
	}

	public bool LaserSightIsGreen { get; set; }

	public bool DoubleWideLaserSight { get; set; }

	public bool ForceLaserSight { get; set; }

	public tk2dBaseSprite LaserSight
	{
		get
		{
			return m_extantLaserSight;
		}
	}

	public bool IsMinusOneGun { get; set; }

	public List<ModuleShootData> ActiveBeams
	{
		get
		{
			return m_activeBeams;
		}
	}

	public bool OverrideAnimations { get; set; }

	public Transform ThrowPrepTransform
	{
		get
		{
			if (m_throwPrepTransform == null)
			{
				m_throwPrepTransform = base.transform.Find("throw point");
				if (m_throwPrepTransform == null)
				{
					m_throwPrepTransform = new GameObject("throw point").transform;
					m_throwPrepTransform.parent = base.transform;
				}
			}
			m_throwPrepTransform.localPosition = ThrowPrepPosition * -1f;
			return m_throwPrepTransform;
		}
	}

	public Vector3 ThrowPrepPosition
	{
		get
		{
			Vector3 position = base.sprite.WorldTopRight;
			Vector3 vector = Vector3Extensions.WithX(x: barrelOffset.transform.parent.InverseTransformPoint(position).x, vector: barrelOffset.localPosition) * -1f;
			if (m_throwPrepTransform != null)
			{
				m_throwPrepTransform.localPosition = vector * -1f;
			}
			return vector;
		}
	}

	public bool MidBurstFire
	{
		get
		{
			return m_midBurstFire;
		}
	}

	public Dictionary<ProjectileModule, ModuleShootData> RuntimeModuleData
	{
		get
		{
			return m_moduleData;
		}
	}

	public int CurrentStrengthTier
	{
		get
		{
			return m_currentStrengthTier;
		}
		set
		{
			m_currentStrengthTier = value;
			if ((bool)CurrentOwner && CurrentOwner is PlayerController)
			{
				PlayerController playerController = CurrentOwner as PlayerController;
				if (playerController.stats != null)
				{
					playerController.stats.RecalculateStats(playerController);
				}
			}
		}
	}

	public bool IsPreppedForThrow
	{
		get
		{
			return m_isPreppedForThrow && m_prepThrowTime > 0f;
		}
	}

	public IntVector2 GetCarryPixelOffset(PlayableCharacters id)
	{
		IntVector2 result = carryPixelOffset;
		if (UsesPerCharacterCarryPixelOffsets)
		{
			for (int i = 0; i < PerCharacterPixelOffsets.Length; i++)
			{
				if (PerCharacterPixelOffsets[i].character == id)
				{
					result += PerCharacterPixelOffsets[i].carryPixelOffset;
				}
			}
		}
		return result;
	}

	public bool OwnerHasSynergy(CustomSynergyType synergyToCheck)
	{
		return (bool)m_owner && m_owner is PlayerController && (m_owner as PlayerController).HasActiveBonusSynergy(synergyToCheck);
	}

	public int GetBaseMaxAmmo()
	{
		return maxAmmo;
	}

	public void SetBaseMaxAmmo(int a)
	{
		maxAmmo = a;
	}

	public void TransformToTargetGun(Gun targetGun)
	{
		int clipShotsRemaining = ClipShotsRemaining;
		if (m_currentlyPlayingChargeVFX != null)
		{
			m_currentlyPlayingChargeVFX.DestroyAll();
			m_currentlyPlayingChargeVFX = null;
		}
		ProjectileVolleyData volley = Volley;
		rawVolley = targetGun.rawVolley;
		singleModule = targetGun.singleModule;
		modifiedVolley = null;
		if ((bool)targetGun.sprite)
		{
			m_defaultSpriteID = targetGun.sprite.spriteId;
			m_sprite.SetSprite(targetGun.sprite.Collection, m_defaultSpriteID);
			if ((bool)base.spriteAnimator && (bool)targetGun.spriteAnimator)
			{
				base.spriteAnimator.Library = targetGun.spriteAnimator.Library;
			}
			tk2dSpriteDefinition.AttachPoint[] attachPoints = m_sprite.Collection.GetAttachPoints(m_defaultSpriteID);
			tk2dSpriteDefinition.AttachPoint attachPoint = ((attachPoints == null) ? null : Array.Find(attachPoints, (tk2dSpriteDefinition.AttachPoint a) => a.name == "PrimaryHand"));
			if (attachPoint != null)
			{
				m_defaultLocalPosition = -attachPoint.position;
			}
		}
		if (targetGun.maxAmmo != maxAmmo && targetGun.maxAmmo > 0)
		{
			int num = ((!InfiniteAmmo) ? AdjustedMaxAmmo : maxAmmo);
			maxAmmo = targetGun.maxAmmo;
			if (AdjustedMaxAmmo > 0 && num > 0 && ammo > 0 && !InfiniteAmmo)
			{
				ammo = Mathf.FloorToInt((float)ammo / (float)num * (float)AdjustedMaxAmmo);
				ammo = Mathf.Min(ammo, AdjustedMaxAmmo);
			}
			else
			{
				ammo = Mathf.Min(ammo, maxAmmo);
			}
		}
		gunSwitchGroup = targetGun.gunSwitchGroup;
		isAudioLoop = targetGun.isAudioLoop;
		gunClass = targetGun.gunClass;
		if (!string.IsNullOrEmpty(gunSwitchGroup))
		{
			AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
		}
		currentGunDamageTypeModifiers = targetGun.currentGunDamageTypeModifiers;
		carryPixelOffset = targetGun.carryPixelOffset;
		carryPixelUpOffset = targetGun.carryPixelUpOffset;
		carryPixelDownOffset = targetGun.carryPixelDownOffset;
		leftFacingPixelOffset = targetGun.leftFacingPixelOffset;
		UsesPerCharacterCarryPixelOffsets = targetGun.UsesPerCharacterCarryPixelOffsets;
		PerCharacterPixelOffsets = targetGun.PerCharacterPixelOffsets;
		gunPosition = targetGun.gunPosition;
		forceFlat = targetGun.forceFlat;
		if (targetGun.GainsRateOfFireAsContinueAttack != GainsRateOfFireAsContinueAttack)
		{
			GainsRateOfFireAsContinueAttack = targetGun.GainsRateOfFireAsContinueAttack;
			RateOfFireMultiplierAdditionPerSecond = targetGun.RateOfFireMultiplierAdditionPerSecond;
		}
		if ((bool)barrelOffset && (bool)targetGun.barrelOffset)
		{
			barrelOffset.localPosition = targetGun.barrelOffset.localPosition;
			m_originalBarrelOffsetPosition = targetGun.barrelOffset.localPosition;
		}
		if ((bool)muzzleOffset && (bool)targetGun.muzzleOffset)
		{
			muzzleOffset.localPosition = targetGun.muzzleOffset.localPosition;
			m_originalMuzzleOffsetPosition = targetGun.muzzleOffset.localPosition;
		}
		if ((bool)chargeOffset && (bool)targetGun.chargeOffset)
		{
			chargeOffset.localPosition = targetGun.chargeOffset.localPosition;
			m_originalChargeOffsetPosition = targetGun.chargeOffset.localPosition;
		}
		reloadTime = targetGun.reloadTime;
		blankDuringReload = targetGun.blankDuringReload;
		blankReloadRadius = targetGun.blankReloadRadius;
		reflectDuringReload = targetGun.reflectDuringReload;
		blankKnockbackPower = targetGun.blankKnockbackPower;
		blankDamageToEnemies = targetGun.blankDamageToEnemies;
		blankDamageScalingOnEmptyClip = targetGun.blankDamageScalingOnEmptyClip;
		doesScreenShake = targetGun.doesScreenShake;
		gunScreenShake = targetGun.gunScreenShake;
		directionlessScreenShake = targetGun.directionlessScreenShake;
		AppliesHoming = targetGun.AppliesHoming;
		AppliedHomingAngularVelocity = targetGun.AppliedHomingAngularVelocity;
		AppliedHomingDetectRadius = targetGun.AppliedHomingDetectRadius;
		GoopReloadsFree = targetGun.GoopReloadsFree;
		gunHandedness = targetGun.gunHandedness;
		m_cachedGunHandedness = null;
		shootAnimation = targetGun.shootAnimation;
		usesContinuousFireAnimation = targetGun.usesContinuousFireAnimation;
		reloadAnimation = targetGun.reloadAnimation;
		emptyReloadAnimation = targetGun.emptyReloadAnimation;
		idleAnimation = targetGun.idleAnimation;
		chargeAnimation = targetGun.chargeAnimation;
		dischargeAnimation = targetGun.dischargeAnimation;
		emptyAnimation = targetGun.emptyAnimation;
		introAnimation = targetGun.introAnimation;
		finalShootAnimation = targetGun.finalShootAnimation;
		enemyPreFireAnimation = targetGun.enemyPreFireAnimation;
		dodgeAnimation = targetGun.dodgeAnimation;
		muzzleFlashEffects = targetGun.muzzleFlashEffects;
		usesContinuousMuzzleFlash = targetGun.usesContinuousMuzzleFlash;
		finalMuzzleFlashEffects = targetGun.finalMuzzleFlashEffects;
		reloadEffects = targetGun.reloadEffects;
		emptyReloadEffects = targetGun.emptyReloadEffects;
		activeReloadSuccessEffects = targetGun.activeReloadSuccessEffects;
		activeReloadFailedEffects = targetGun.activeReloadFailedEffects;
		shellCasing = targetGun.shellCasing;
		shellsToLaunchOnFire = targetGun.shellsToLaunchOnFire;
		shellCasingOnFireFrameDelay = targetGun.shellCasingOnFireFrameDelay;
		shellsToLaunchOnReload = targetGun.shellsToLaunchOnReload;
		reloadShellLaunchFrame = targetGun.reloadShellLaunchFrame;
		clipObject = targetGun.clipObject;
		clipsToLaunchOnReload = targetGun.clipsToLaunchOnReload;
		reloadClipLaunchFrame = targetGun.reloadClipLaunchFrame;
		IsTrickGun = targetGun.IsTrickGun;
		TrickGunAlternatesHandedness = targetGun.TrickGunAlternatesHandedness;
		alternateVolley = targetGun.alternateVolley;
		alternateShootAnimation = targetGun.alternateShootAnimation;
		alternateReloadAnimation = targetGun.alternateReloadAnimation;
		alternateIdleAnimation = targetGun.alternateIdleAnimation;
		alternateSwitchGroup = targetGun.alternateSwitchGroup;
		rampBullets = targetGun.rampBullets;
		rampStartHeight = targetGun.rampStartHeight;
		rampTime = targetGun.rampTime;
		usesDirectionalAnimator = targetGun.usesDirectionalAnimator;
		usesDirectionalIdleAnimations = targetGun.usesDirectionalIdleAnimations;
		if ((bool)base.aiAnimator)
		{
			UnityEngine.Object.Destroy(base.aiAnimator);
			base.aiAnimator = null;
		}
		if ((bool)targetGun.aiAnimator)
		{
			AIAnimator aIAnimator = base.gameObject.AddComponent<AIAnimator>();
			AIAnimator aIAnimator2 = targetGun.aiAnimator;
			aIAnimator.facingType = aIAnimator2.facingType;
			aIAnimator.DirectionParent = aIAnimator2.DirectionParent;
			aIAnimator.faceSouthWhenStopped = aIAnimator2.faceSouthWhenStopped;
			aIAnimator.faceTargetWhenStopped = aIAnimator2.faceTargetWhenStopped;
			aIAnimator.directionalType = aIAnimator2.directionalType;
			aIAnimator.RotationQuantizeTo = aIAnimator2.RotationQuantizeTo;
			aIAnimator.RotationOffset = aIAnimator2.RotationOffset;
			aIAnimator.ForceKillVfxOnPreDeath = aIAnimator2.ForceKillVfxOnPreDeath;
			aIAnimator.SuppressAnimatorFallback = aIAnimator2.SuppressAnimatorFallback;
			aIAnimator.IsBodySprite = aIAnimator2.IsBodySprite;
			aIAnimator.IdleAnimation = aIAnimator2.IdleAnimation;
			aIAnimator.MoveAnimation = aIAnimator2.MoveAnimation;
			aIAnimator.FlightAnimation = aIAnimator2.FlightAnimation;
			aIAnimator.HitAnimation = aIAnimator2.HitAnimation;
			aIAnimator.OtherAnimations = aIAnimator2.OtherAnimations;
			aIAnimator.OtherVFX = aIAnimator2.OtherVFX;
			aIAnimator.OtherScreenShake = aIAnimator2.OtherScreenShake;
			aIAnimator.IdleFidgetAnimations = aIAnimator2.IdleFidgetAnimations;
			base.aiAnimator = aIAnimator;
		}
		MultiTemporaryOrbitalSynergyProcessor component = targetGun.GetComponent<MultiTemporaryOrbitalSynergyProcessor>();
		MultiTemporaryOrbitalSynergyProcessor component2 = GetComponent<MultiTemporaryOrbitalSynergyProcessor>();
		if (!component && (bool)component2)
		{
			UnityEngine.Object.Destroy(component2);
		}
		else if ((bool)component && !component2)
		{
			MultiTemporaryOrbitalSynergyProcessor multiTemporaryOrbitalSynergyProcessor = base.gameObject.AddComponent<MultiTemporaryOrbitalSynergyProcessor>();
			multiTemporaryOrbitalSynergyProcessor.RequiredSynergy = component.RequiredSynergy;
			multiTemporaryOrbitalSynergyProcessor.OrbitalPrefab = component.OrbitalPrefab;
		}
		if (rawVolley != null)
		{
			for (int i = 0; i < rawVolley.projectiles.Count; i++)
			{
				rawVolley.projectiles[i].ResetRuntimeData();
			}
		}
		else
		{
			singleModule.ResetRuntimeData();
		}
		if (volley != null)
		{
			RawSourceVolley = DuctTapeItem.TransferDuctTapeModules(volley, RawSourceVolley, this);
		}
		if (m_owner is PlayerController)
		{
			PlayerController playerController = m_owner as PlayerController;
			if (playerController.stats != null)
			{
				playerController.stats.RecalculateStats(playerController);
			}
		}
		if (base.gameObject.activeSelf)
		{
			StartCoroutine(HandleFrameDelayedTransformation());
		}
		DidTransformGunThisFrame = true;
	}

	private IEnumerator HandleFrameDelayedTransformation()
	{
		yield return null;
		if (!string.IsNullOrEmpty(gunSwitchGroup))
		{
			AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
		}
		if ((bool)base.spriteAnimator && !string.IsNullOrEmpty(introAnimation))
		{
			base.spriteAnimator.Play(introAnimation);
		}
		else
		{
			PlayIdleAnimation();
		}
		if ((bool)this && base.enabled && (bool)CurrentOwner)
		{
			HandleSpriteFlip(false);
			HandleSpriteFlip(true);
			HandleSpriteFlip(CurrentOwner.SpriteFlipped);
		}
	}

	public void Initialize(GameActor owner)
	{
		if (!m_sprite)
		{
			Awake();
		}
		m_owner = owner;
		m_transform = base.transform;
		m_attachTransform = base.transform.parent;
		m_anim = GetComponent<tk2dSpriteAnimator>();
		m_anim.AnimationCompleted = AnimationCompleteDelegate;
		m_sprite.automaticallyManagesDepth = false;
		m_sprite.IsPerpendicular = !forceFlat;
		m_sprite.independentOrientation = true;
		if (forceFlat)
		{
			owner.sprite.AttachRenderer(m_sprite);
			m_sprite.HeightOffGround = 0.25f;
			m_sprite.UpdateZDepth();
		}
		m_moduleData = new Dictionary<ProjectileModule, ModuleShootData>();
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				ModuleShootData moduleShootData = new ModuleShootData();
				if (ammo < Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner))
				{
					moduleShootData.numberShotsFired = Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner) - ammo;
				}
				m_moduleData.Add(Volley.projectiles[i], moduleShootData);
			}
		}
		else
		{
			ModuleShootData moduleShootData2 = new ModuleShootData();
			if (ammo < singleModule.GetModNumberOfShotsInClip(CurrentOwner))
			{
				moduleShootData2.numberShotsFired = singleModule.GetModNumberOfShotsInClip(CurrentOwner) - ammo;
			}
			m_moduleData.Add(singleModule, moduleShootData2);
		}
		if ((bool)modifiedFinalVolley)
		{
			for (int j = 0; j < modifiedFinalVolley.projectiles.Count; j++)
			{
				ModuleShootData moduleShootData3 = new ModuleShootData();
				if (ammo < modifiedFinalVolley.projectiles[j].GetModNumberOfShotsInClip(CurrentOwner))
				{
					moduleShootData3.numberShotsFired = modifiedFinalVolley.projectiles[j].GetModNumberOfShotsInClip(CurrentOwner) - ammo;
				}
				m_moduleData.Add(modifiedFinalVolley.projectiles[j], moduleShootData3);
			}
		}
		if ((bool)modifiedOptionalReloadVolley)
		{
			for (int k = 0; k < modifiedOptionalReloadVolley.projectiles.Count; k++)
			{
				ModuleShootData moduleShootData4 = new ModuleShootData();
				if (ammo < modifiedOptionalReloadVolley.projectiles[k].GetModNumberOfShotsInClip(CurrentOwner))
				{
					moduleShootData4.numberShotsFired = modifiedOptionalReloadVolley.projectiles[k].GetModNumberOfShotsInClip(CurrentOwner) - ammo;
				}
				m_moduleData.Add(modifiedOptionalReloadVolley.projectiles[k], moduleShootData4);
			}
		}
		if (procGunData != null)
		{
			ApplyProcGunData(procGunData);
		}
		if (m_childTransformsToFlip == null)
		{
			m_childTransformsToFlip = new List<Transform>();
		}
		for (int l = 0; l < m_transform.childCount; l++)
		{
			Transform child = m_transform.GetChild(l);
			if (child.GetComponent<Light>() != null)
			{
				m_childTransformsToFlip.Add(child);
			}
		}
		tk2dSpriteDefinition.AttachPoint[] attachPoints = m_sprite.Collection.GetAttachPoints(m_defaultSpriteID);
		tk2dSpriteDefinition.AttachPoint attachPoint = ((attachPoints == null) ? null : Array.Find(attachPoints, (tk2dSpriteDefinition.AttachPoint a) => a.name == "PrimaryHand"));
		m_defaultLocalPosition = -attachPoint.position;
		if (AppliesHoming)
		{
			PostProcessProjectile = (Action<Projectile>)Delegate.Combine(PostProcessProjectile, new Action<Projectile>(ApplyHomingToProjectile));
		}
		if (m_owner != null)
		{
			SpeculativeRigidbody speculativeRigidbody = m_owner.specRigidbody;
			speculativeRigidbody.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(speculativeRigidbody.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(PostRigidbodyMovement));
		}
		if (OnInitializedWithOwner != null)
		{
			OnInitializedWithOwner(m_owner);
		}
	}

	public void UpdateAttachTransform()
	{
		m_attachTransform = base.transform.parent;
	}

	private void ApplyHomingToProjectile(Projectile obj)
	{
		if (!(obj is HomingProjectile))
		{
			HomingModifier component = obj.GetComponent<HomingModifier>();
			if ((bool)component)
			{
				component.AngularVelocity = Mathf.Max(component.AngularVelocity, AppliedHomingAngularVelocity);
				component.HomingRadius = Mathf.Max(component.HomingRadius, AppliedHomingDetectRadius);
			}
			else
			{
				component = obj.gameObject.AddComponent<HomingModifier>();
				component.AngularVelocity = AppliedHomingAngularVelocity;
				component.HomingRadius = AppliedHomingDetectRadius;
			}
		}
	}

	private void InitializeDefaultFrame()
	{
		if (m_defaultSpriteID == 0)
		{
			PickupObject pickupObject = PickupObjectDatabase.Instance.InternalGetById(PickupObjectId);
			if (pickupObject != null)
			{
				m_defaultSpriteID = pickupObject.sprite.spriteId;
			}
			else
			{
				m_defaultSpriteID = base.sprite.spriteId;
			}
		}
	}

	public void ReinitializeModuleData(ProjectileVolleyData originalSourceVolley)
	{
		Dictionary<ProjectileModule, ModuleShootData> moduleData = m_moduleData;
		if (m_moduleData == null)
		{
			m_moduleData = new Dictionary<ProjectileModule, ModuleShootData>();
		}
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				ProjectileModule projectileModule = Volley.projectiles[i];
				if (!m_moduleData.ContainsKey(projectileModule))
				{
					ModuleShootData moduleShootData = new ModuleShootData();
					if (ammo < projectileModule.GetModNumberOfShotsInClip(CurrentOwner))
					{
						moduleShootData.numberShotsFired = projectileModule.GetModNumberOfShotsInClip(CurrentOwner) - ammo;
					}
					ModuleShootData value;
					if (moduleData != null && originalSourceVolley != null && originalSourceVolley.projectiles != null && i < originalSourceVolley.projectiles.Count && moduleData.TryGetValue(originalSourceVolley.projectiles[i], out value) && (bool)value.beam)
					{
						moduleShootData.alternateAngleSign = value.alternateAngleSign;
						moduleShootData.beam = value.beam;
						moduleShootData.beamKnockbackID = value.beamKnockbackID;
						moduleShootData.angleForShot = value.angleForShot;
						m_activeBeams.Remove(value);
						m_activeBeams.Add(moduleShootData);
					}
					m_moduleData.Add(projectileModule, moduleShootData);
				}
			}
		}
		else
		{
			ModuleShootData moduleShootData2 = new ModuleShootData();
			if (ammo < singleModule.GetModNumberOfShotsInClip(CurrentOwner))
			{
				moduleShootData2.numberShotsFired = singleModule.GetModNumberOfShotsInClip(CurrentOwner) - ammo;
			}
			m_moduleData.Add(singleModule, moduleShootData2);
		}
		if ((bool)modifiedFinalVolley)
		{
			for (int j = 0; j < modifiedFinalVolley.projectiles.Count; j++)
			{
				if (!m_moduleData.ContainsKey(modifiedFinalVolley.projectiles[j]))
				{
					ModuleShootData moduleShootData3 = new ModuleShootData();
					if (ammo < modifiedFinalVolley.projectiles[j].GetModNumberOfShotsInClip(CurrentOwner))
					{
						moduleShootData3.numberShotsFired = modifiedFinalVolley.projectiles[j].GetModNumberOfShotsInClip(CurrentOwner) - ammo;
					}
					m_moduleData.Add(modifiedFinalVolley.projectiles[j], moduleShootData3);
				}
			}
		}
		if ((bool)modifiedOptionalReloadVolley)
		{
			for (int k = 0; k < modifiedOptionalReloadVolley.projectiles.Count; k++)
			{
				if (!m_moduleData.ContainsKey(modifiedOptionalReloadVolley.projectiles[k]))
				{
					ModuleShootData moduleShootData4 = new ModuleShootData();
					if (ammo < modifiedOptionalReloadVolley.projectiles[k].GetModNumberOfShotsInClip(CurrentOwner))
					{
						moduleShootData4.numberShotsFired = modifiedOptionalReloadVolley.projectiles[k].GetModNumberOfShotsInClip(CurrentOwner) - ammo;
					}
					m_moduleData.Add(modifiedOptionalReloadVolley.projectiles[k], moduleShootData4);
				}
			}
		}
		if (!(originalSourceVolley != null))
		{
			return;
		}
		for (int l = 0; l < originalSourceVolley.projectiles.Count; l++)
		{
			if (string.IsNullOrEmpty(originalSourceVolley.projectiles[l].runtimeGuid) || !moduleData.ContainsKey(originalSourceVolley.projectiles[l]))
			{
				continue;
			}
			for (int m = 0; m < Volley.projectiles.Count; m++)
			{
				if (originalSourceVolley.projectiles[l].runtimeGuid == Volley.projectiles[m].runtimeGuid)
				{
					m_activeBeams.Remove(m_moduleData[Volley.projectiles[m]]);
					m_activeBeams.Add(moduleData[originalSourceVolley.projectiles[l]]);
					m_moduleData[Volley.projectiles[m]] = moduleData[originalSourceVolley.projectiles[l]];
				}
			}
		}
	}

	public void Awake()
	{
		m_sprite = GetComponent<tk2dSprite>();
		AwakeAudio();
		m_clipLaunchAttachPoint = base.transform.Find("Clip");
		m_casingLaunchAttachPoint = base.transform.Find("Casing");
		m_localAttachPoint = base.transform.Find("PrimaryHand");
		m_offhandAttachPoint = base.transform.Find("SecondaryHand");
		m_meshRenderer = base.transform.GetComponent<MeshRenderer>();
		if (!string.IsNullOrEmpty(gunSwitchGroup))
		{
			AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
		}
		InitializeDefaultFrame();
		if (rawVolley != null)
		{
			for (int i = 0; i < rawVolley.projectiles.Count; i++)
			{
				rawVolley.projectiles[i].ResetRuntimeData();
			}
		}
		else
		{
			singleModule.ResetRuntimeData();
		}
		if (alternateVolley != null)
		{
			for (int j = 0; j < alternateVolley.projectiles.Count; j++)
			{
				alternateVolley.projectiles[j].ResetRuntimeData();
			}
		}
		if ((bool)barrelOffset)
		{
			m_originalBarrelOffsetPosition = barrelOffset.localPosition;
		}
		if ((bool)muzzleOffset)
		{
			m_originalMuzzleOffsetPosition = muzzleOffset.localPosition;
		}
		if ((bool)chargeOffset)
		{
			m_originalChargeOffsetPosition = chargeOffset.localPosition;
		}
		weaponPanelSpriteOverride = GetComponent<GunWeaponPanelSpriteOverride>();
	}

	private void AwakeAudio()
	{
		AkGameObj akGameObj = GetComponent<AkGameObj>();
		if (!akGameObj)
		{
			akGameObj = base.gameObject.AddComponent<AkGameObj>();
		}
		akGameObj.Register();
	}

	public void OnEnable()
	{
		if (m_isThrown)
		{
			return;
		}
		if (!NoOwnerOverride && !m_isThrown && (m_owner == null || m_owner.CurrentGun != this) && (!(m_owner is PlayerController) || (m_owner as PlayerController).inventory == null || !((m_owner as PlayerController).CurrentSecondaryGun == this)))
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (!NoOwnerOverride)
		{
			HandleSpriteFlip(m_owner.SpriteFlipped);
		}
		if (!m_owner)
		{
			return;
		}
		base.gameObject.GetOrAddComponent<AkGameObj>();
		m_transform.localPosition = BraveUtility.QuantizeVector(m_transform.localPosition, 16f);
		m_transform.localRotation = Quaternion.identity;
		if (ClearsCooldownsLikeAWP)
		{
			ClearCooldowns();
		}
		m_isReloading = false;
		m_reloadWhenDoneFiring = false;
		if (!m_isThrown)
		{
			if (!string.IsNullOrEmpty(introAnimation) && m_anim.GetClipByName(introAnimation) != null)
			{
				Play(introAnimation);
			}
			else
			{
				PlayIdleAnimation();
			}
		}
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Reflection"));
	}

	public void Update()
	{
		m_isCritting = false;
		if (HeroSwordCooldown > 0f)
		{
			HeroSwordCooldown -= BraveTime.DeltaTime;
		}
		if (m_owner == null)
		{
			HandlePickupCurseParticles();
			if (!m_isBeingEyedByRat && Time.frameCount % 50 == 0 && ShouldBeTakenByRat(base.sprite.WorldCenter))
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandleRatTheft());
			}
		}
		else if (UsesRechargeLikeActiveItem && m_owner is PlayerController && (m_owner as PlayerController).CharacterUsesRandomGuns)
		{
			RemainingActiveCooldownAmount = Mathf.Max(0f, m_remainingActiveCooldownAmount - 25f * BraveTime.DeltaTime);
		}
		if (m_reloadWhenDoneFiring && (string.IsNullOrEmpty(shootAnimation) || !base.spriteAnimator.IsPlaying(shootAnimation)) && (string.IsNullOrEmpty(finalShootAnimation) || !base.spriteAnimator.IsPlaying(finalShootAnimation)) && (string.IsNullOrEmpty(criticalFireAnimation) || !base.spriteAnimator.IsPlaying(criticalFireAnimation)))
		{
			Reload();
			if (OnReloadPressed != null)
			{
				OnReloadPressed(CurrentOwner as PlayerController, this, false);
			}
		}
		if (m_continueBurstInUpdate)
		{
			ContinueAttack();
			if (!m_midBurstFire)
			{
				CeaseAttack(false);
			}
		}
		if (m_owner is PlayerController && (bool)m_sprite && m_sprite.FlipX)
		{
			tk2dSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(m_sprite);
			if (outlineSprites != null)
			{
				for (int i = 0; i < outlineSprites.Length; i++)
				{
					if ((bool)outlineSprites[i])
					{
						outlineSprites[i].scale = m_sprite.scale;
					}
				}
			}
		}
		if (m_owner != null && m_instanceMinimapIcon != null)
		{
			GetRidOfMinimapIcon();
		}
		if (IsReloading && blankDuringReload)
		{
			m_reloadElapsed += BraveTime.DeltaTime;
			if (base.spriteAnimator == null || base.spriteAnimator.IsPlaying(reloadAnimation))
			{
				Vector2 unitCenter = m_owner.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				if (reflectDuringReload)
				{
					float arg = 1f;
					if (OnReflectedBulletDamageModifier != null)
					{
						arg = OnReflectedBulletDamageModifier(arg);
					}
					float num = 1f;
					if (OnReflectedBulletScaleModifier != null)
					{
						num = OnReflectedBulletScaleModifier(num);
					}
					PassiveReflectItem.ReflectBulletsInRange(unitCenter, blankReloadRadius, true, m_owner, 10f, num, arg);
				}
				else
				{
					SilencerInstance.DestroyBulletsInRange(unitCenter, blankReloadRadius, true, false);
				}
				float num2 = blankDamageToEnemies;
				if (blankDamageScalingOnEmptyClip > 1f)
				{
					float num3 = (float)ClipShotsRemaining / (float)ClipCapacity;
					float t = Mathf.Clamp01(1f - num3);
					num2 = Mathf.Lerp(num2, num2 * blankDamageScalingOnEmptyClip, t);
				}
				if (num2 > 0f)
				{
					if (m_reloadElapsed > 0.125f && !m_hasDoneSingleReloadBlankEffect)
					{
						m_hasDoneSingleReloadBlankEffect = true;
						Vector2 arcOrigin = PrimaryHandAttachPoint.position.XY();
						float arcRadius = blankReloadRadius * 2f;
						float arcAngle = 45f;
						DealDamageToEnemiesInArc(arcOrigin, arcAngle, arcRadius, num2, blankKnockbackPower);
					}
				}
				else
				{
					Exploder.DoRadialKnockback(unitCenter, blankKnockbackPower, blankReloadRadius + 1.25f);
				}
			}
		}
		if (m_isPreppedForThrow)
		{
			bool flag = m_prepThrowTime < 1.2f;
			bool flag2 = m_prepThrowTime < 0f;
			m_prepThrowTime += BraveTime.DeltaTime;
			PlayerController playerController = CurrentOwner as PlayerController;
			if (m_prepThrowTime < 1.2f)
			{
				HandleSpriteFlip(m_sprite.FlipY);
				if ((bool)playerController)
				{
					playerController.DoSustainedVibration(Vibration.Strength.UltraLight);
				}
			}
			else
			{
				if (flag)
				{
					DoChargeCompletePoof();
				}
				if ((bool)playerController)
				{
					playerController.DoSustainedVibration(Vibration.Strength.Light);
				}
			}
			if (flag2 && m_prepThrowTime >= 0f)
			{
				playerController.ProcessHandAttachment();
			}
		}
		if (m_isThrown && m_sprite.FlipY)
		{
			m_sprite.FlipY = false;
		}
	}

	public void OnWillRenderObject()
	{
		if (!Pixelator.IsRenderingReflectionMap)
		{
			return;
		}
		Bounds bounds = base.sprite.GetBounds();
		float num = bounds.min.y * 2f;
		if (m_owner != null && m_owner.CurrentGun == this)
		{
			bool flipY = base.sprite.FlipY;
			int num2 = ((!flipY) ? 1 : (-1));
			if (flipY)
			{
				num += 2f * bounds.size.y;
			}
			float a = 0f;
			float num3 = 1f - Mathf.Abs(90f - (gunAngle + 540f) % 180f) / 90f;
			if (CurrentOwner != null)
			{
				a = (float)(-1 * num2) * (base.transform.position.y - CurrentOwner.transform.position.y);
			}
			a = Mathf.Lerp(a, (float)num2 * bounds.size.y, num3);
			a += -0.1875f * (float)num2 * (1f - num3);
			num += a;
		}
		base.sprite.renderer.material.SetFloat("_ReflectionYOffset", num);
	}

	public void OnDisable()
	{
		if (m_activeBeams.Count > 0 && doesScreenShake && GameManager.Instance.MainCameraController != null)
		{
			GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		}
		if ((bool)m_extantLockOnSprite)
		{
			SpawnManager.Despawn(m_extantLockOnSprite);
		}
		DespawnVFX();
		base.sprite.SetSprite(m_defaultSpriteID);
		m_reloadWhenDoneFiring = false;
	}

	protected override void OnDestroy()
	{
		if (Minimap.HasInstance)
		{
			GetRidOfMinimapIcon();
		}
		base.OnDestroy();
	}

	public void ToggleRenderers(bool value)
	{
		m_meshRenderer.enabled = value;
		SpriteOutlineManager.ToggleOutlineRenderers(m_sprite, value);
		if ((bool)m_extantLaserSight)
		{
			m_extantLaserSight.renderer.enabled = value;
		}
		if (m_currentlyPlayingChargeVFX == null)
		{
			return;
		}
		m_currentlyPlayingChargeVFX.ToggleRenderers(value);
		if (DefaultModule != null && m_moduleData.ContainsKey(DefaultModule))
		{
			ModuleShootData moduleShootData = m_moduleData[DefaultModule];
			if (moduleShootData != null && moduleShootData.lastChargeProjectile != null)
			{
				TogglePreviousChargeEffectsIfNecessary(moduleShootData.lastChargeProjectile, value);
			}
		}
	}

	public tk2dBaseSprite GetSprite()
	{
		if (m_sprite == null)
		{
			m_sprite = GetComponent<tk2dSprite>();
		}
		return m_sprite;
	}

	public void DespawnVFX()
	{
		if (m_extantLaserSight != null)
		{
			UnityEngine.Object.Destroy(m_extantLaserSight.gameObject);
			m_extantLaserSight = null;
		}
		muzzleFlashEffects.DestroyAll();
		m_isContinuousMuzzleFlashOut = false;
		finalMuzzleFlashEffects.DestroyAll();
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				if (Volley.projectiles[i].chargeProjectiles == null)
				{
					continue;
				}
				for (int j = 0; j < Volley.projectiles[i].chargeProjectiles.Count; j++)
				{
					if (Volley.projectiles[i].chargeProjectiles[j].UsesOverrideMuzzleFlashVfxPool)
					{
						Volley.projectiles[i].chargeProjectiles[j].OverrideMuzzleFlashVfxPool.DestroyAll();
					}
				}
			}
		}
		else if (singleModule.chargeProjectiles != null)
		{
			for (int k = 0; k < singleModule.chargeProjectiles.Count; k++)
			{
				if (singleModule.chargeProjectiles[k].UsesOverrideMuzzleFlashVfxPool)
				{
					singleModule.chargeProjectiles[k].OverrideMuzzleFlashVfxPool.DestroyAll();
				}
			}
		}
		reloadEffects.DestroyAll();
		emptyReloadEffects.DestroyAll();
		activeReloadSuccessEffects.DestroyAll();
		activeReloadFailedEffects.DestroyAll();
	}

	public void ApplyProcGunData(ProceduralGunData data)
	{
		maxAmmo = (ammo = data.ammoData.GetRandomIntValue());
		damageModifier = data.damageData.GetRandomIntValue();
		gunCooldownModifier = data.cooldownData.GetRandomValue();
	}

	public void HandleSpriteFlip(bool flipped)
	{
		if (m_isThrown)
		{
			flipped = false;
		}
		if (usesDirectionalIdleAnimations || preventRotation)
		{
			flipped = false;
		}
		if (flipped && !forceFlat)
		{
			if (!m_sprite.FlipY)
			{
				barrelOffset.localPosition = m_originalBarrelOffsetPosition.WithY(0f - m_originalBarrelOffsetPosition.y);
				if ((bool)muzzleOffset)
				{
					muzzleOffset.localPosition = m_originalMuzzleOffsetPosition.WithY(0f - m_originalMuzzleOffsetPosition.y);
				}
				if ((bool)chargeOffset)
				{
					chargeOffset.localPosition = m_originalChargeOffsetPosition.WithY(0f - m_originalChargeOffsetPosition.y);
				}
				if ((bool)reloadOffset)
				{
					reloadOffset.localPosition = reloadOffset.localPosition.WithY(0f - reloadOffset.localPosition.y);
				}
				for (int i = 0; i < m_childTransformsToFlip.Count; i++)
				{
					m_childTransformsToFlip[i].localPosition = m_childTransformsToFlip[i].localPosition.WithY(0f - m_childTransformsToFlip[i].localPosition.y);
				}
				m_sprite.FlipY = true;
			}
		}
		else if (m_sprite.FlipY)
		{
			barrelOffset.localPosition = m_originalBarrelOffsetPosition;
			if ((bool)muzzleOffset)
			{
				muzzleOffset.localPosition = m_originalMuzzleOffsetPosition;
			}
			if ((bool)chargeOffset)
			{
				chargeOffset.localPosition = m_originalChargeOffsetPosition;
			}
			if ((bool)reloadOffset)
			{
				reloadOffset.localPosition = reloadOffset.localPosition.WithY(0f - reloadOffset.localPosition.y);
			}
			for (int j = 0; j < m_childTransformsToFlip.Count; j++)
			{
				m_childTransformsToFlip[j].localPosition = m_childTransformsToFlip[j].localPosition.WithY(0f - m_childTransformsToFlip[j].localPosition.y);
			}
			m_sprite.FlipY = false;
		}
		if (m_isPreppedForThrow)
		{
			Vector3 vector = m_defaultLocalPosition.WithZ(0f);
			if (flipped)
			{
				vector = Vector3.Scale(vector, new Vector3(1f, -1f, 1f));
			}
			base.transform.localPosition = Vector3.Lerp(vector.WithZ(0f), ThrowPrepPosition, Mathf.Clamp01(m_prepThrowTime / 1.2f));
		}
		else
		{
			m_transform.localPosition = m_defaultLocalPosition.WithZ(0f);
			if (flipped)
			{
				m_transform.localPosition = Vector3.Scale(m_transform.localPosition, new Vector3(1f, -1f, 1f));
			}
		}
		m_transform.localPosition = BraveUtility.QuantizeVector(m_transform.localPosition, 16f);
	}

	private bool ShouldDoLaserSight()
	{
		if (m_isPreppedForThrow)
		{
			return false;
		}
		if (m_isReloading)
		{
			return false;
		}
		if (SuppressLaserSight)
		{
			return false;
		}
		if (ForceLaserSight)
		{
			return true;
		}
		if (PickupObjectId == GlobalItemIds.ArtfulDodgerChallengeGun)
		{
			return true;
		}
		if (CurrentOwner is PlayerController && PassiveItem.ActiveFlagItems.ContainsKey(CurrentOwner as PlayerController) && PassiveItem.ActiveFlagItems[CurrentOwner as PlayerController].ContainsKey(typeof(LaserSightItem)))
		{
			return true;
		}
		return false;
	}

	public float GetChargeFraction()
	{
		bool flag = false;
		float num = 1f;
		if (IsFiring)
		{
			if (Volley != null)
			{
				for (int i = 0; i < Volley.projectiles.Count; i++)
				{
					ProjectileModule projectileModule = Volley.projectiles[i];
					if (projectileModule.shootStyle == ProjectileModule.ShootStyle.Charged)
					{
						ModuleShootData moduleShootData = m_moduleData[projectileModule];
						if (projectileModule.LongestChargeTime > 0f)
						{
							num = Mathf.Min(num, Mathf.Clamp01(moduleShootData.chargeTime / projectileModule.LongestChargeTime));
							flag = true;
						}
					}
				}
			}
			else
			{
				ProjectileModule projectileModule2 = singleModule;
				if (projectileModule2.shootStyle == ProjectileModule.ShootStyle.Charged)
				{
					ModuleShootData moduleShootData2 = m_moduleData[projectileModule2];
					if (projectileModule2.LongestChargeTime > 0f)
					{
						num = Mathf.Min(num, Mathf.Clamp01(moduleShootData2.chargeTime / projectileModule2.LongestChargeTime));
						flag = true;
					}
				}
			}
		}
		if (!flag)
		{
			num = 0f;
		}
		return num;
	}

	public float HandleAimRotation(Vector3 ownerAimPoint, bool limitAimSpeed = false, float aimTimeScale = 1f)
	{
		if (m_isThrown)
		{
			return prevGunAngleUnmodified;
		}
		Vector2 b;
		if (!usesDirectionalIdleAnimations)
		{
			b = ((!LockedHorizontalOnCharge) ? (m_transform.position + Quaternion.Euler(0f, 0f, gunAngle) * Quaternion.Euler(0f, 0f, 0f - m_attachTransform.localRotation.z) * barrelOffset.localPosition).XY() : m_owner.specRigidbody.HitboxPixelCollider.UnitCenter);
		}
		else
		{
			b = (m_transform.position + Quaternion.Euler(0f, 0f, 0f - m_attachTransform.localRotation.z) * barrelOffset.localPosition).XY();
			b = m_owner.specRigidbody.HitboxPixelCollider.UnitCenter;
		}
		float num = Vector2.Distance(ownerAimPoint.XY(), b);
		float t = Mathf.Clamp01((num - 2f) / 3f);
		b = Vector2.Lerp(m_owner.specRigidbody.HitboxPixelCollider.UnitCenter, b, t);
		m_localAimPoint = ownerAimPoint.XY();
		Vector2 vector = m_localAimPoint - b;
		float num2 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		if (OverrideAngleSnap.HasValue)
		{
			num2 = BraveMathCollege.QuantizeFloat(num2, OverrideAngleSnap.Value);
		}
		if ((limitAimSpeed && aimTimeScale != 1f) || m_activeBeams.Count > 0)
		{
			float num3 = float.MaxValue * BraveTime.DeltaTime * aimTimeScale;
			if (m_activeBeams.Count > 0 && (bool)Volley && Volley.UsesBeamRotationLimiter)
			{
				num3 = Volley.BeamRotationDegreesPerSecond * BraveTime.DeltaTime * aimTimeScale;
			}
			float value = BraveMathCollege.ClampAngle180(num2 - prevGunAngleUnmodified);
			num2 = BraveMathCollege.ClampAngle180(prevGunAngleUnmodified + Mathf.Clamp(value, 0f - num3, num3));
			m_localAimPoint = (base.transform.position + (Quaternion.Euler(0f, 0f, num2) * Vector3.right).normalized * vector.magnitude).XY();
		}
		prevGunAngleUnmodified = num2;
		gunAngle = num2;
		m_attachTransform.localRotation = Quaternion.Euler(m_attachTransform.localRotation.x, m_attachTransform.localRotation.y, num2);
		m_unroundedBarrelPosition = barrelOffset.position;
		float num4 = ((!forceFlat) ? (Mathf.RoundToInt(num2 / 10f) * 10) : (Mathf.RoundToInt(num2 / 3f) * 3));
		if (IgnoresAngleQuantization)
		{
			num4 = num2;
		}
		bool flag = base.sprite.FlipY;
		float num5 = 75f;
		float num6 = 105f;
		if (num4 <= 155f && num4 >= 25f)
		{
			num5 = 75f;
			num6 = 105f;
		}
		if (!base.sprite.FlipY && Mathf.Abs(num4) > num6)
		{
			flag = true;
		}
		else if (base.sprite.FlipY && Mathf.Abs(num4) < num5)
		{
			flag = false;
		}
		if (LockedHorizontalOnCharge)
		{
			float chargeFraction = GetChargeFraction();
			LockedHorizontalCachedAngle = num2;
			num4 = Mathf.LerpAngle(num4, flag ? 180 : 0, chargeFraction);
		}
		if (LockedHorizontalOnReload && IsReloading)
		{
			num4 = (flag ? 180 : 0);
		}
		if (m_isPreppedForThrow)
		{
			num4 = ((!(m_prepThrowTime < 1.2f)) ? ((float)Mathf.FloorToInt(Mathf.PingPong(m_prepThrowTime * 15f, 10f) + -95f)) : ((float)Mathf.FloorToInt(Mathf.LerpAngle(num4, -90f, Mathf.Clamp01(m_prepThrowTime / 1.2f)))));
		}
		if (preventRotation)
		{
			num4 = 0f;
		}
		if (usesDirectionalIdleAnimations)
		{
			int num7 = BraveMathCollege.AngleToOctant(num4 + 90f);
			float num8 = num7 * -45;
			Debug.Log(num8);
			float z = (num4 + 360f) % 360f - num8;
			m_attachTransform.localRotation = Quaternion.Euler(m_attachTransform.localRotation.x, m_attachTransform.localRotation.y, z);
		}
		else
		{
			m_attachTransform.localRotation = Quaternion.Euler(m_attachTransform.localRotation.x, m_attachTransform.localRotation.y, num4);
		}
		if (m_currentlyPlayingChargeVFX != null)
		{
			UpdateChargeEffectZDepth(vector);
		}
		if (m_sprite != null)
		{
			m_sprite.ForceRotationRebuild();
		}
		if (ShouldDoLaserSight())
		{
			if (m_extantLaserSight == null)
			{
				string path = "Global VFX/VFX_LaserSight";
				if (!(m_owner is PlayerController))
				{
					path = ((!LaserSightIsGreen) ? "Global VFX/VFX_LaserSight_Enemy" : "Global VFX/VFX_LaserSight_Enemy_Green");
				}
				m_extantLaserSight = SpawnManager.SpawnVFX((GameObject)BraveResources.Load(path)).GetComponent<tk2dTiledSprite>();
				m_extantLaserSight.IsPerpendicular = false;
				m_extantLaserSight.HeightOffGround = CustomLaserSightHeight;
				m_extantLaserSight.renderer.enabled = m_meshRenderer.enabled;
				m_extantLaserSight.transform.parent = barrelOffset;
				if (m_owner is AIActor)
				{
					m_extantLaserSight.renderer.enabled = false;
				}
			}
			m_extantLaserSight.transform.localPosition = Vector3.zero;
			m_extantLaserSight.transform.rotation = Quaternion.Euler(0f, 0f, num2);
			if (m_extantLaserSight.renderer.enabled)
			{
				Func<SpeculativeRigidbody, bool> rigidbodyExcluder = (SpeculativeRigidbody otherRigidbody) => (bool)otherRigidbody.minorBreakable && !otherRigidbody.minorBreakable.stopsBullets;
				bool flag2 = false;
				float num9 = float.MaxValue;
				if (DoubleWideLaserSight)
				{
					CollisionLayer layer = ((m_owner is PlayerController) ? CollisionLayer.EnemyHitBox : CollisionLayer.PlayerHitBox);
					int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, layer, CollisionLayer.BulletBreakable);
					Vector2 vector2 = BraveMathCollege.DegreesToVector(vector.ToAngle() + 90f, 0.0625f);
					RaycastResult result;
					if (PhysicsEngine.Instance.Raycast(barrelOffset.position.XY() + vector2, vector, CustomLaserSightDistance, out result, true, true, rayMask, null, false, rigidbodyExcluder))
					{
						flag2 = true;
						num9 = Mathf.Min(num9, result.Distance);
					}
					RaycastResult.Pool.Free(ref result);
					if (PhysicsEngine.Instance.Raycast(barrelOffset.position.XY() - vector2, vector, CustomLaserSightDistance, out result, true, true, rayMask, null, false, rigidbodyExcluder))
					{
						flag2 = true;
						num9 = Mathf.Min(num9, result.Distance);
					}
					RaycastResult.Pool.Free(ref result);
				}
				else
				{
					CollisionLayer layer2 = ((m_owner is PlayerController) ? CollisionLayer.EnemyHitBox : CollisionLayer.PlayerHitBox);
					int rayMask2 = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, layer2, CollisionLayer.BulletBreakable);
					RaycastResult result2;
					if (PhysicsEngine.Instance.Raycast(barrelOffset.position.XY(), vector, CustomLaserSightDistance, out result2, true, true, rayMask2, null, false, rigidbodyExcluder))
					{
						flag2 = true;
						num9 = result2.Distance;
						if ((bool)result2.SpeculativeRigidbody && (bool)result2.SpeculativeRigidbody.aiActor)
						{
							HandleEnemyHitByLaserSight(result2.SpeculativeRigidbody.aiActor);
						}
					}
					RaycastResult.Pool.Free(ref result2);
				}
				m_extantLaserSight.dimensions = new Vector2((!flag2) ? 480f : (num9 / 0.0625f), 1f);
				m_extantLaserSight.ForceRotationRebuild();
				m_extantLaserSight.UpdateZDepth();
			}
		}
		else if (m_extantLaserSight != null)
		{
			SpawnManager.Despawn(m_extantLaserSight.gameObject);
			m_extantLaserSight = null;
		}
		if (!OwnerHasSynergy(CustomSynergyType.PLASMA_LASER) && (bool)m_extantLockOnSprite)
		{
			SpawnManager.Despawn(m_extantLockOnSprite);
		}
		if (usesDirectionalAnimator)
		{
			base.aiAnimator.LockFacingDirection = true;
			base.aiAnimator.FacingDirection = num2;
		}
		return num2;
	}

	protected void HandleEnemyHitByLaserSight(AIActor hitEnemy)
	{
		if ((bool)hitEnemy && LastLaserSightEnemy != hitEnemy && OwnerHasSynergy(CustomSynergyType.PLASMA_LASER))
		{
			if ((bool)m_extantLockOnSprite)
			{
				SpawnManager.Despawn(m_extantLockOnSprite);
			}
			m_extantLockOnSprite = hitEnemy.PlayEffectOnActor((GameObject)BraveResources.Load("Global VFX/VFX_LockOn"), Vector3.zero, true, true, true);
			LastLaserSightEnemy = hitEnemy;
		}
	}

	protected void UpdateChargeEffectZDepth(Vector2 currentAimDirection)
	{
		float t = (currentAimDirection.normalized.y + 1f) / 2f;
		float heightOffGround = Mathf.Lerp(1.6f, 0.9f, t);
		m_currentlyPlayingChargeVFX.SetHeightOffGround(heightOffGround);
	}

	protected void UpdatePerpendicularity(Vector2 gunToAim)
	{
		if (!forceFlat)
		{
			int num = BraveMathCollege.VectorToQuadrant(gunToAim);
			if (num == 2)
			{
				m_sprite.IsPerpendicular = false;
			}
			else
			{
				m_sprite.IsPerpendicular = true;
			}
		}
	}

	protected float DealSwordDamageToEnemy(AIActor targetEnemy, Vector2 arcOrigin, Vector2 contact, float angle, float overrideDamage = -1f, float overrideForce = -1f)
	{
		Projectile currentProjectile = DefaultModule.GetCurrentProjectile();
		float num = ((!(overrideDamage > 0f)) ? currentProjectile.baseData.damage : overrideDamage);
		float force = ((!(overrideForce > 0f)) ? currentProjectile.baseData.force : overrideForce);
		if ((bool)targetEnemy.healthHaver)
		{
			targetEnemy.healthHaver.ApplyDamage(num, contact - arcOrigin, m_owner.ActorName);
		}
		if ((bool)targetEnemy.knockbackDoer)
		{
			targetEnemy.knockbackDoer.ApplyKnockback(contact - arcOrigin, force);
		}
		currentProjectile.hitEffects.HandleEnemyImpact(contact, angle, targetEnemy.transform, Vector2.zero, Vector2.zero, true);
		return num;
	}

	protected void DealDamageToEnemiesInArc(Vector2 arcOrigin, float arcAngle, float arcRadius, float overrideDamage = -1f, float overrideForce = -1f, List<SpeculativeRigidbody> alreadyHit = null)
	{
		RoomHandler roomHandler = null;
		if (m_owner is PlayerController)
		{
			roomHandler = ((PlayerController)m_owner).CurrentRoom;
		}
		else if (m_owner is AIActor)
		{
			roomHandler = ((AIActor)m_owner).ParentRoom;
		}
		if (roomHandler == null)
		{
			return;
		}
		List<AIActor> activeEnemies = roomHandler.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aIActor = activeEnemies[i];
			if (!aIActor || !aIActor.specRigidbody || !aIActor.IsNormalEnemy || aIActor.IsGone || !aIActor.healthHaver || (alreadyHit != null && alreadyHit.Contains(aIActor.specRigidbody)))
			{
				continue;
			}
			for (int j = 0; j < aIActor.healthHaver.NumBodyRigidbodies; j++)
			{
				SpeculativeRigidbody bodyRigidbody = aIActor.healthHaver.GetBodyRigidbody(j);
				PixelCollider hitboxPixelCollider = bodyRigidbody.HitboxPixelCollider;
				if (hitboxPixelCollider == null)
				{
					continue;
				}
				Vector2 vector = BraveMathCollege.ClosestPointOnRectangle(arcOrigin, hitboxPixelCollider.UnitBottomLeft, hitboxPixelCollider.UnitDimensions);
				float num = Vector2.Distance(vector, arcOrigin);
				float target = BraveMathCollege.Atan2Degrees(vector - arcOrigin);
				if (!(num < arcRadius) || !(Mathf.DeltaAngle(CurrentAngle, target) < arcAngle))
				{
					continue;
				}
				bool flag = true;
				int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, CollisionLayer.BulletBreakable);
				RaycastResult result;
				if (PhysicsEngine.Instance.Raycast(arcOrigin, vector - arcOrigin, num, out result, true, true, rayMask) && result.SpeculativeRigidbody != bodyRigidbody)
				{
					flag = false;
				}
				RaycastResult.Pool.Free(ref result);
				if (!flag)
				{
					continue;
				}
				float damage = DealSwordDamageToEnemy(aIActor, arcOrigin, vector, arcAngle, overrideDamage, overrideForce);
				if (alreadyHit != null)
				{
					if (alreadyHit.Count == 0)
					{
						StickyFrictionManager.Instance.RegisterSwordDamageStickyFriction(damage);
					}
					alreadyHit.Add(aIActor.specRigidbody);
				}
				break;
			}
		}
	}

	protected void HandleHeroSwordSlash(List<SpeculativeRigidbody> alreadyHit, Vector2 arcOrigin, int slashId)
	{
		float num = (m_casingLaunchAttachPoint.position.XY() - PrimaryHandAttachPoint.position.XY()).magnitude * 1.85f;
		float num2 = 45f;
		float num3 = num * num;
		if (HeroSwordDoesntBlank)
		{
			ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
			for (int num4 = allProjectiles.Count - 1; num4 >= 0; num4--)
			{
				Projectile projectile = allProjectiles[num4];
				if ((bool)projectile && !(projectile.Owner is PlayerController) && projectile.IsReflectedBySword && projectile.LastReflectedSlashId != slashId && (!(projectile.Owner is AIActor) || (projectile.Owner as AIActor).IsNormalEnemy))
				{
					Vector2 worldCenter = projectile.sprite.WorldCenter;
					float num5 = Vector2.SqrMagnitude(worldCenter - arcOrigin);
					if (num5 < num3)
					{
						float target = BraveMathCollege.Atan2Degrees(worldCenter - arcOrigin);
						if (Mathf.DeltaAngle(CurrentAngle, target) < num2)
						{
							PassiveReflectItem.ReflectBullet(projectile, true, m_owner, 2f);
							projectile.LastReflectedSlashId = slashId;
						}
					}
				}
			}
		}
		else
		{
			ReadOnlyCollection<Projectile> allProjectiles2 = StaticReferenceManager.AllProjectiles;
			for (int num6 = allProjectiles2.Count - 1; num6 >= 0; num6--)
			{
				Projectile projectile2 = allProjectiles2[num6];
				if ((bool)projectile2 && (!(projectile2.Owner is PlayerController) || projectile2.ForcePlayerBlankable) && (!(projectile2.Owner is AIActor) || (projectile2.Owner as AIActor).IsNormalEnemy))
				{
					Vector2 worldCenter2 = projectile2.sprite.WorldCenter;
					float num7 = Vector2.SqrMagnitude(worldCenter2 - arcOrigin);
					if (num7 < num3)
					{
						float target2 = BraveMathCollege.Atan2Degrees(worldCenter2 - arcOrigin);
						if (Mathf.DeltaAngle(CurrentAngle, target2) < num2)
						{
							projectile2.DieInAir(false, true, true, true);
						}
					}
				}
			}
		}
		DealDamageToEnemiesInArc(arcOrigin, num2, num, -1f, -1f, alreadyHit);
		Projectile currentProjectile = DefaultModule.GetCurrentProjectile();
		float damage = currentProjectile.baseData.damage;
		float num8 = num * num;
		List<MinorBreakable> allMinorBreakables = StaticReferenceManager.AllMinorBreakables;
		for (int num9 = allMinorBreakables.Count - 1; num9 >= 0; num9--)
		{
			MinorBreakable minorBreakable = allMinorBreakables[num9];
			if ((bool)minorBreakable && (bool)minorBreakable.specRigidbody && !minorBreakable.IsBroken && (bool)minorBreakable.sprite && (minorBreakable.sprite.WorldCenter - arcOrigin).sqrMagnitude < num8)
			{
				minorBreakable.Break();
			}
		}
		List<MajorBreakable> allMajorBreakables = StaticReferenceManager.AllMajorBreakables;
		for (int num10 = allMajorBreakables.Count - 1; num10 >= 0; num10--)
		{
			MajorBreakable majorBreakable = allMajorBreakables[num10];
			if ((bool)majorBreakable && (bool)majorBreakable.specRigidbody && !alreadyHit.Contains(majorBreakable.specRigidbody) && !majorBreakable.IsSecretDoor && !majorBreakable.IsDestroyed && !((majorBreakable.specRigidbody.UnitCenter - arcOrigin).sqrMagnitude > num8))
			{
				float num11 = damage;
				if ((bool)majorBreakable.healthHaver)
				{
					num11 *= 0.2f;
				}
				majorBreakable.ApplyDamage(num11, majorBreakable.specRigidbody.UnitCenter - arcOrigin, false);
				alreadyHit.Add(majorBreakable.specRigidbody);
			}
		}
	}

	private IEnumerator HandleSlash()
	{
		int slashId = Time.frameCount;
		List<SpeculativeRigidbody> alreadyHit = new List<SpeculativeRigidbody>();
		m_owner.knockbackDoer.ApplyKnockback(BraveMathCollege.DegreesToVector(CurrentAngle), 40f, 0.25f);
		DoScreenShake();
		HandleShootEffects(DefaultModule);
		if (!HeroSwordDoesntBlank && (m_owner.healthHaver.GetCurrentHealthPercentage() >= 1f || (m_owner as PlayerController).HasActiveBonusSynergy(CustomSynergyType.HERO_OF_CHICKEN)))
		{
			if ((bool)Volley)
			{
				for (int i = 0; i < Volley.projectiles.Count; i++)
				{
					ShootSingleProjectile(Volley.projectiles[i]);
				}
			}
			else
			{
				ShootSingleProjectile(DefaultModule);
			}
		}
		Vector2 cachedSlashOffset = PrimaryHandAttachPoint.position.XY() - m_owner.CenterPosition;
		float ela = 0f;
		while (ela < 0.2f)
		{
			ela += BraveTime.DeltaTime;
			HandleHeroSwordSlash(alreadyHit, m_owner.CenterPosition + cachedSlashOffset, slashId);
			yield return null;
			if (!this)
			{
				break;
			}
		}
	}

	public bool IsGunBlocked()
	{
		if (RequiresFundsToShoot && m_owner is PlayerController && (m_owner as PlayerController).carriedConsumables.Currency < CurrencyCostPerShot)
		{
			return true;
		}
		bool result = false;
		Vector2 unitCenter = m_owner.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		Vector2 vector = barrelOffset.transform.position.XY();
		Vector2 vector2 = vector - unitCenter;
		float magnitude = vector2.magnitude;
		int num = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, CollisionLayer.BulletBreakable);
		if ((bool)m_owner && !(m_owner is PlayerController))
		{
			num |= CollisionMask.LayerToMask(CollisionLayer.EnemyBulletBlocker);
		}
		SpeculativeRigidbody result2;
		if (PhysicsEngine.Instance.Pointcast(vector, out result2, false, true, CollisionMask.LayerToMask(CollisionLayer.BeamBlocker), null, false))
		{
			UltraFortunesFavor ultraFortunesFavor = result2.ultraFortunesFavor;
			if ((bool)ultraFortunesFavor)
			{
				result2.ultraFortunesFavor.HitFromPoint(vector);
				return true;
			}
		}
		if (CanAttackThroughObjects)
		{
			return false;
		}
		PhysicsEngine.Instance.Pointcast(unitCenter, out result2, false, true, num, null, false, m_owner.specRigidbody);
		bool flag = false;
		if (Volley == null && singleModule.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			flag = true;
		}
		if (Volley != null && Volley.projectiles.Count == 1 && Volley.projectiles[0].shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			flag = true;
		}
		int num2 = 100;
		RaycastResult result3;
		while (PhysicsEngine.Instance.Raycast(unitCenter, vector2, magnitude, out result3, true, true, num, null, false, null, result2))
		{
			num2--;
			if (num2 <= 0)
			{
				result = true;
				break;
			}
			SpeculativeRigidbody speculativeRigidbody = result3.SpeculativeRigidbody;
			RaycastResult.Pool.Free(ref result3);
			if (speculativeRigidbody != null)
			{
				MinorBreakable component = speculativeRigidbody.GetComponent<MinorBreakable>();
				if (component != null && (!flag || m_currentlyPlayingChargeVFX != null) && !component.OnlyBrokenByCode)
				{
					component.Break(vector2.normalized * 3f);
					continue;
				}
			}
			if (GameManager.Instance.InTutorial && speculativeRigidbody != null && (bool)speculativeRigidbody.GetComponent<Chest>() && (bool)speculativeRigidbody.majorBreakable)
			{
				speculativeRigidbody.majorBreakable.Break(vector2);
				continue;
			}
			result = true;
			break;
		}
		return result;
	}

	public void ForceThrowGun()
	{
		ThrowGun();
	}

	public DebrisObject DropGun(float dropHeight = 0.5f)
	{
		m_isThrown = true;
		m_thrownOnGround = true;
		if (m_sprite == null)
		{
			m_sprite = base.sprite;
		}
		base.gameObject.SetActive(true);
		m_owner = null;
		Vector3 position = base.transform.position;
		if ((bool)PrimaryHandAttachPoint)
		{
			position = PrimaryHandAttachPoint.position;
		}
		GameObject gameObject = SpawnManager.SpawnProjectile("ThrownGunProjectile", position, Quaternion.identity);
		Projectile projectile = (LastProjectile = gameObject.GetComponent<Projectile>());
		projectile.Shooter = ((!(m_owner != null)) ? null : m_owner.specRigidbody);
		projectile.DestroyMode = Projectile.ProjectileDestroyMode.BecomeDebris;
		projectile.shouldRotate = false;
		if ((bool)projectile)
		{
			TrailRenderer componentInChildren = projectile.GetComponentInChildren<TrailRenderer>();
			if ((bool)componentInChildren)
			{
				UnityEngine.Object.Destroy(componentInChildren);
			}
		}
		SpeculativeRigidbody component2 = gameObject.GetComponent<SpeculativeRigidbody>();
		component2.sprite = base.sprite;
		base.transform.parent = gameObject.transform;
		if (m_sprite.FlipY)
		{
			HandleSpriteFlip(false);
		}
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		if ((bool)PrimaryHandAttachPoint)
		{
			base.transform.localPosition -= PrimaryHandAttachPoint.localPosition;
		}
		if (m_defaultSpriteID >= 0)
		{
			base.spriteAnimator.StopAndResetFrame();
			m_sprite.SetSprite(m_defaultSpriteID);
		}
		if (!RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			RoomHandler.unassignedInteractableObjects.Add(this);
		}
		DebrisObject debrisObject = projectile.BecomeDebris(Vector3.zero, dropHeight);
		debrisObject.Priority = EphemeralObject.EphemeralPriority.Critical;
		debrisObject.FlagAsPickup();
		debrisObject.inertialMass = 10f;
		debrisObject.canRotate = false;
		UnityEngine.Object.Destroy(projectile.GetComponentInChildren<SimpleSpriteRotator>());
		UnityEngine.Object.Destroy(projectile);
		projectile.ForceDestruction();
		SpriteOutlineManager.AddOutlineToSprite(m_sprite, Color.black, 0.1f, 0.05f);
		m_sprite.ForceRotationRebuild();
		if (m_anim != null)
		{
			PlayIdleAnimation();
		}
		if (OnDropped != null)
		{
			OnDropped();
		}
		RegisterMinimapIcon();
		return debrisObject;
	}

	public void PrepGunForThrow()
	{
		if (!m_isPreppedForThrow && CurrentOwner is PlayerController)
		{
			m_isPreppedForThrow = true;
			m_prepThrowTime = -0.3f;
			HandleSpriteFlip(m_sprite.FlipY);
			(CurrentOwner as PlayerController).ProcessHandAttachment();
		}
		AkSoundEngine.PostEvent("Play_BOSS_doormimic_turn_01", base.gameObject);
	}

	public void UnprepGunForThrow()
	{
		if (m_isPreppedForThrow)
		{
			m_isPreppedForThrow = false;
			m_prepThrowTime = -0.3f;
			HandleSpriteFlip(m_sprite.FlipY);
			(CurrentOwner as PlayerController).ProcessHandAttachment();
		}
		AkSoundEngine.PostEvent("Stop_BOSS_doormimic_turn_01", base.gameObject);
	}

	private void ThrowGun()
	{
		m_isThrown = true;
		m_thrownOnGround = false;
		base.gameObject.SetActive(true);
		AkSoundEngine.PostEvent("Play_OBJ_item_throw_01", base.gameObject);
		Vector3 vector = ThrowPrepTransform.parent.TransformPoint((ThrowPrepPosition * -1f).WithX(0f));
		Vector2 vector2 = m_localAimPoint - vector.XY();
		float z = BraveMathCollege.Atan2Degrees(vector2);
		GameObject gameObject = SpawnManager.SpawnProjectile("ThrownGunProjectile", vector, Quaternion.Euler(0f, 0f, z));
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Shooter = m_owner.specRigidbody;
		component.DestroyMode = Projectile.ProjectileDestroyMode.BecomeDebris;
		component.baseData.damage *= (m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.ThrownGunDamage);
		SpeculativeRigidbody component2 = gameObject.GetComponent<SpeculativeRigidbody>();
		component2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTrigger));
		component2.sprite = base.sprite;
		m_sprite.scale = Vector3.one;
		base.transform.parent = gameObject.transform;
		base.transform.localRotation = Quaternion.identity;
		m_sprite.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.MiddleCenter);
		if (m_sprite.FlipY)
		{
			base.transform.localPosition = Vector3.Scale(new Vector3(-1f, 1f, 1f), base.transform.localPosition);
		}
		Bounds bounds = base.sprite.GetBounds();
		component2.PrimaryPixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
		component2.PrimaryPixelCollider.ManualOffsetX = -Mathf.RoundToInt(bounds.extents.x / 0.0625f);
		component2.PrimaryPixelCollider.ManualOffsetY = -Mathf.RoundToInt(bounds.extents.y / 0.0625f);
		component2.PrimaryPixelCollider.ManualWidth = Mathf.RoundToInt(bounds.size.x / 0.0625f);
		component2.PrimaryPixelCollider.ManualHeight = Mathf.RoundToInt(bounds.size.y / 0.0625f);
		component2.UpdateCollidersOnRotation = true;
		component2.UpdateCollidersOnScale = true;
		component.Reawaken();
		component.Owner = CurrentOwner;
		component.Start();
		component.SendInDirection(vector2, true, false);
		component.OnBecameDebris = (Action<DebrisObject>)Delegate.Combine(component.OnBecameDebris, (Action<DebrisObject>)delegate(DebrisObject a)
		{
			if ((bool)barrelOffset)
			{
				barrelOffset.localPosition = m_originalBarrelOffsetPosition;
			}
			if ((bool)muzzleOffset)
			{
				muzzleOffset.localPosition = m_originalMuzzleOffsetPosition;
			}
			if ((bool)chargeOffset)
			{
				chargeOffset.localPosition = m_originalChargeOffsetPosition;
			}
			if ((bool)a)
			{
				a.FlagAsPickup();
				a.Priority = EphemeralObject.EphemeralPriority.Critical;
				TrailRenderer componentInChildren = a.gameObject.GetComponentInChildren<TrailRenderer>();
				if ((bool)componentInChildren)
				{
					UnityEngine.Object.Destroy(componentInChildren);
				}
				SpeculativeRigidbody component3 = a.GetComponent<SpeculativeRigidbody>();
				if ((bool)component3)
				{
					component3.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.Projectile, CollisionLayer.EnemyHitBox));
				}
			}
		});
		component.OnBecameDebrisGrounded = (Action<DebrisObject>)Delegate.Combine(component.OnBecameDebrisGrounded, new Action<DebrisObject>(HandleThrownGunGrounded));
		component.angularVelocity = ((!(vector2.x > 0f)) ? 1080 : (-1080));
		if (!RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			RoomHandler.unassignedInteractableObjects.Add(this);
		}
		component2.ForceRegenerate();
		if ((bool)m_owner)
		{
			(m_owner as PlayerController).DoPostProcessThrownGun(component);
		}
		m_owner = null;
	}

	private void HandleThrownGunGrounded(DebrisObject obj)
	{
		obj.OnGrounded = (Action<DebrisObject>)Delegate.Remove(obj.OnGrounded, new Action<DebrisObject>(HandleThrownGunGrounded));
		obj.inertialMass = 10f;
		if ((bool)barrelOffset)
		{
			barrelOffset.localPosition = m_originalBarrelOffsetPosition;
		}
		if ((bool)muzzleOffset)
		{
			muzzleOffset.localPosition = m_originalMuzzleOffsetPosition;
		}
		if ((bool)chargeOffset)
		{
			chargeOffset.localPosition = m_originalChargeOffsetPosition;
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(m_sprite, true);
		SpriteOutlineManager.AddOutlineToSprite(m_sprite, Color.black, 0.1f, 0.05f);
		m_sprite.UpdateZDepth();
		m_thrownOnGround = true;
	}

	public void RegisterNewCustomAmmunition(ActiveAmmunitionData ammodata)
	{
		if (ammodata != null && ammodata.ShotsRemaining > 0 && !m_customAmmunitions.Contains(ammodata))
		{
			m_customAmmunitions.Add(ammodata);
		}
	}

	public void RegisterMinimapIcon()
	{
		if (!(base.transform.position.y < -300f))
		{
			GameObject gameObject = (GameObject)BraveResources.Load("Global Prefabs/Minimap_Gun_Icon");
			if (gameObject != null && m_owner == null)
			{
				m_minimapIconRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
				m_instanceMinimapIcon = Minimap.Instance.RegisterRoomIcon(m_minimapIconRoom, gameObject);
			}
		}
	}

	public void GetRidOfMinimapIcon()
	{
		if (m_instanceMinimapIcon != null)
		{
			Minimap.Instance.DeregisterRoomIcon(m_minimapIconRoom, m_instanceMinimapIcon);
			m_instanceMinimapIcon = null;
		}
	}

	public void GainAmmo(int amt)
	{
		if (CanGainAmmo && !InfiniteAmmo)
		{
			if (amt > 0)
			{
				UnprepGunForThrow();
			}
			ammo += amt;
			if (AdjustedMaxAmmo > 0)
			{
				ammo = Math.Min(AdjustedMaxAmmo, ammo);
			}
			ammo = Mathf.Clamp(ammo, 0, 100000000);
			if (OnAmmoChanged != null)
			{
				OnAmmoChanged(m_owner as PlayerController, this);
			}
		}
	}

	public void LoseAmmo(int amt)
	{
		ammo -= amt;
		if (ammo < 0)
		{
			ammo = 0;
		}
		if (ClipShotsRemaining > ammo)
		{
			ClipShotsRemaining = ammo;
		}
		if (OnAmmoChanged != null)
		{
			OnAmmoChanged(m_owner as PlayerController, this);
		}
	}

	public void GainAmmo(Gun g)
	{
		if (CanGainAmmo)
		{
			ammo += g.ammo;
			if (AdjustedMaxAmmo > 0)
			{
				ammo = Math.Min(AdjustedMaxAmmo, ammo);
			}
			if (OnAmmoChanged != null)
			{
				OnAmmoChanged(m_owner as PlayerController, this);
			}
		}
	}

	public float GetPrimaryCooldown()
	{
		if (Volley != null)
		{
			return Volley.projectiles[0].cooldownTime;
		}
		return singleModule.cooldownTime;
	}

	public void ClearOptionalReloadVolleyCooldownAndReloadData()
	{
		if (!(OptionalReloadVolley != null))
		{
			return;
		}
		for (int i = 0; i < OptionalReloadVolley.projectiles.Count; i++)
		{
			if (m_moduleData.ContainsKey(OptionalReloadVolley.projectiles[i]))
			{
				m_moduleData[OptionalReloadVolley.projectiles[i]].onCooldown = false;
				m_moduleData[OptionalReloadVolley.projectiles[i]].needsReload = false;
			}
		}
	}

	public void ClearCooldowns()
	{
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				m_moduleData[Volley.projectiles[i]].onCooldown = false;
			}
		}
		else
		{
			m_moduleData[singleModule].onCooldown = false;
		}
		if (UsesRechargeLikeActiveItem)
		{
			RemainingActiveCooldownAmount = 0f;
		}
	}

	public void ClearReloadData()
	{
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				m_moduleData[Volley.projectiles[i]].needsReload = false;
			}
			m_isReloading = false;
		}
		else
		{
			m_moduleData[singleModule].needsReload = false;
			m_isReloading = false;
		}
	}

	public AttackResult Attack(ProjectileData overrideProjectileData = null, GameObject overrideBulletObject = null)
	{
		if (m_isCurrentlyFiring && m_midBurstFire)
		{
			return AttackResult.Fail;
		}
		if (!m_hasReinitializedAudioSwitch)
		{
			m_hasReinitializedAudioSwitch = true;
			if (!string.IsNullOrEmpty(gunSwitchGroup))
			{
				AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
			}
		}
		m_playedEmptyClipSound = false;
		m_continuousAttackTime = 0f;
		if (m_isReloading)
		{
			Reload();
			return AttackResult.Reload;
		}
		if (CurrentAmmo < 0)
		{
			CurrentAmmo = 0;
		}
		if (CurrentAmmo == 0)
		{
			if (!InfiniteAmmo)
			{
				HandleOutOfAmmo();
				return AttackResult.Empty;
			}
			GainAmmo(maxAmmo);
		}
		m_cachedIsGunBlocked = IsGunBlocked();
		m_isCurrentlyFiring = true;
		bool flag = false;
		if (CanCriticalFire)
		{
			float num = (float)PlayerStats.GetTotalCoolness() / 100f;
			if (m_owner.IsStealthed)
			{
				num = 10f;
			}
			if (UnityEngine.Random.value < CriticalChance + num)
			{
				m_isCritting = true;
			}
			if (ForceNextShotCritical)
			{
				ForceNextShotCritical = false;
				m_isCritting = true;
			}
		}
		if (IsHeroSword)
		{
			flag = true;
			if (!m_anim.IsPlaying(shootAnimation) && !m_anim.IsPlaying(reloadAnimation) && HeroSwordCooldown <= 0f)
			{
				HeroSwordCooldown = 0.5f;
				StartCoroutine(HandleSlash());
				HandleShootAnimation(null);
			}
		}
		else if (Volley != null)
		{
			bool flag2 = CheckHasLoadedModule(Volley);
			if (!flag2)
			{
				AttemptedFireNeedReload();
			}
			if (flag2 || reloadTime <= 0f)
			{
				ProjectileVolleyData volley = Volley;
				if (modifiedFinalVolley != null && DefaultModule.HasFinalVolleyOverride() && DefaultModule.IsFinalShot(m_moduleData[DefaultModule], CurrentOwner))
				{
					volley = modifiedFinalVolley;
				}
				flag = HandleInitialGunShoot(volley, overrideProjectileData, overrideBulletObject);
				m_midBurstFire = false;
				for (int i = 0; i < Volley.projectiles.Count; i++)
				{
					ProjectileModule projectileModule = Volley.projectiles[i];
					if (projectileModule.shootStyle == ProjectileModule.ShootStyle.Burst && m_moduleData[projectileModule].numberShotsFiredThisBurst < projectileModule.burstShotCount)
					{
						m_midBurstFire = true;
						break;
					}
				}
			}
		}
		else
		{
			bool flag3 = CheckHasLoadedModule(singleModule);
			if (!flag3)
			{
				AttemptedFireNeedReload();
			}
			if (flag3 || reloadTime <= 0f)
			{
				flag = HandleInitialGunShoot(singleModule, overrideProjectileData, overrideBulletObject);
				m_midBurstFire = false;
				if (singleModule.shootStyle == ProjectileModule.ShootStyle.Burst && m_moduleData[singleModule].numberShotsFiredThisBurst < singleModule.burstShotCount)
				{
					m_midBurstFire = true;
				}
			}
		}
		m_isCurrentlyFiring = flag;
		if (m_isCurrentlyFiring && lowersAudioWhileFiring)
		{
			AkSoundEngine.PostEvent("play_state_volume_lower_01", GameManager.Instance.gameObject);
		}
		if (flag && OnPostFired != null && m_owner is PlayerController)
		{
			OnPostFired(m_owner as PlayerController, this);
		}
		return (!flag) ? AttackResult.OnCooldown : AttackResult.Success;
	}

	public bool ContinueAttack(bool canAttack = true, ProjectileData overrideProjectileData = null)
	{
		if (!m_isCurrentlyFiring)
		{
			if (HasShootStyle(ProjectileModule.ShootStyle.Charged) || HasShootStyle(ProjectileModule.ShootStyle.Automatic) || HasShootStyle(ProjectileModule.ShootStyle.Burst))
			{
				if (IsEmpty)
				{
					return false;
				}
				if (m_isReloading)
				{
					return false;
				}
				if (CurrentAmmo < 0)
				{
					CurrentAmmo = 0;
				}
				if (CurrentAmmo == 0)
				{
					return false;
				}
				if (!canAttack)
				{
					return false;
				}
				return Attack(overrideProjectileData) == AttackResult.Success;
			}
			return false;
		}
		if (m_isReloading)
		{
			return false;
		}
		if (CurrentAmmo < 0)
		{
			CurrentAmmo = 0;
		}
		if (CurrentAmmo == 0)
		{
			CeaseAttack(false);
			return false;
		}
		if (!m_playedEmptyClipSound && ClipShotsRemaining == 0)
		{
			if (GameManager.AUDIO_ENABLED)
			{
				AkSoundEngine.PostEvent("Play_WPN_gun_empty_01", base.gameObject);
			}
			m_playedEmptyClipSound = true;
		}
		m_cachedIsGunBlocked = IsGunBlocked();
		m_isCurrentlyFiring = true;
		m_continuousAttackTime += BraveTime.DeltaTime;
		bool flag = false;
		if (!canAttack || m_cachedIsGunBlocked)
		{
			if (m_activeBeams.Count > 0)
			{
				ClearBeams();
			}
			else if (isAudioLoop && m_isAudioLooping)
			{
				if (GameManager.AUDIO_ENABLED)
				{
					AkSoundEngine.PostEvent("Stop_WPN_gun_loop_01", base.gameObject);
				}
				m_isAudioLooping = false;
			}
			ClearBurstState();
			if (usesContinuousMuzzleFlash)
			{
				muzzleFlashEffects.DestroyAll();
				m_isContinuousMuzzleFlashOut = false;
			}
			m_continuousAttackTime = 0f;
		}
		if (m_activeBeams.Count > 0 && m_owner is PlayerController)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.BEAM_WEAPON_FIRE_TIME, BraveTime.DeltaTime);
		}
		if (CanCriticalFire)
		{
			float num = (float)PlayerStats.GetTotalCoolness() / 100f;
			if (m_owner.IsStealthed)
			{
				num = 10f;
			}
			if (UnityEngine.Random.value < CriticalChance + num)
			{
				m_isCritting = true;
			}
			if (ForceNextShotCritical)
			{
				ForceNextShotCritical = false;
				m_isCritting = true;
			}
		}
		if (Volley != null)
		{
			if (CheckHasLoadedModule(Volley))
			{
				ProjectileVolleyData volley = Volley;
				if (modifiedFinalVolley != null && DefaultModule.HasFinalVolleyOverride() && DefaultModule.IsFinalShot(m_moduleData[DefaultModule], CurrentOwner))
				{
					volley = modifiedFinalVolley;
				}
				flag = HandleContinueGunShoot(volley, canAttack, overrideProjectileData);
				m_midBurstFire = false;
				for (int i = 0; i < Volley.projectiles.Count; i++)
				{
					ProjectileModule projectileModule = Volley.projectiles[i];
					if (projectileModule.shootStyle == ProjectileModule.ShootStyle.Burst && m_moduleData[projectileModule].numberShotsFiredThisBurst < projectileModule.burstShotCount)
					{
						m_midBurstFire = true;
						break;
					}
				}
			}
			else
			{
				CeaseAttack(false);
			}
		}
		else if (CheckHasLoadedModule(singleModule))
		{
			flag = HandleContinueGunShoot(singleModule, canAttack, overrideProjectileData);
		}
		else
		{
			CeaseAttack(false);
			m_midBurstFire = false;
			if (singleModule.shootStyle == ProjectileModule.ShootStyle.Burst && m_moduleData[singleModule].numberShotsFiredThisBurst < singleModule.burstShotCount)
			{
				m_midBurstFire = true;
			}
		}
		if (flag && OnPostFired != null && m_owner is PlayerController)
		{
			OnPostFired(m_owner as PlayerController, this);
		}
		return flag;
	}

	public void OnPrePlayerChange()
	{
		if (m_isPreppedForThrow)
		{
			UnprepGunForThrow();
		}
	}

	public bool CeaseAttack(bool canAttack = true, ProjectileData overrideProjectileData = null)
	{
		if (m_isPreppedForThrow && m_prepThrowTime < 1.2f)
		{
			UnprepGunForThrow();
		}
		else if (m_isPreppedForThrow)
		{
			(m_owner as PlayerController).inventory.RemoveGunFromInventory(this);
			ThrowGun();
			return true;
		}
		if (!m_isCurrentlyFiring)
		{
			return false;
		}
		if (m_midBurstFire && canAttack)
		{
			m_continueBurstInUpdate = true;
			return true;
		}
		m_continueBurstInUpdate = false;
		if (m_isCurrentlyFiring && lowersAudioWhileFiring)
		{
			AkSoundEngine.PostEvent("stop_state_volume_lower_01", GameManager.Instance.gameObject);
		}
		m_isCurrentlyFiring = false;
		m_hasDecrementedFunds = false;
		m_continuousAttackTime = 0f;
		m_cachedIsGunBlocked = IsGunBlocked();
		if (CanCriticalFire)
		{
			float num = (float)PlayerStats.GetTotalCoolness() / 100f;
			if (m_owner.IsStealthed)
			{
				num = 10f;
			}
			if (UnityEngine.Random.value < CriticalChance + num)
			{
				m_isCritting = true;
			}
			if (ForceNextShotCritical)
			{
				ForceNextShotCritical = false;
				m_isCritting = true;
			}
		}
		if (LockedHorizontalOnCharge)
		{
			m_attachTransform.localRotation = Quaternion.Euler(m_attachTransform.localRotation.x, m_attachTransform.localRotation.y, LockedHorizontalCachedAngle);
			gunAngle = LockedHorizontalCachedAngle;
		}
		bool flag = ((!(Volley != null)) ? HandleEndGunShoot(singleModule, canAttack, overrideProjectileData) : HandleEndGunShoot(Volley, canAttack, overrideProjectileData));
		if (MovesPlayerForwardOnChargeFire && flag && (bool)m_owner && m_owner is PlayerController)
		{
			m_owner.knockbackDoer.ApplyKnockback(BraveMathCollege.DegreesToVector(CurrentAngle), 40f, 0.25f);
		}
		if (GameManager.AUDIO_ENABLED)
		{
			AkSoundEngine.PostEvent("Stop_WPN_gun_loop_01", base.gameObject);
		}
		m_isAudioLooping = false;
		ClearBeams();
		if (usesContinuousFireAnimation)
		{
			m_anim.StopAndResetFrame();
			AnimationCompleteDelegate(m_anim, null);
		}
		if (usesContinuousMuzzleFlash)
		{
			muzzleFlashEffects.DestroyAll();
			m_isContinuousMuzzleFlashOut = false;
		}
		if (!m_isReloading && DefaultModule.GetModNumberOfShotsInClip(CurrentOwner) == 1)
		{
			m_reloadWhenDoneFiring = true;
		}
		if ((bool)Volley)
		{
			ProjectileVolleyData volley = Volley;
			if ((bool)volley)
			{
				for (int i = 0; i < volley.projectiles.Count; i++)
				{
					m_moduleData[volley.projectiles[i]].numberShotsFiredThisBurst = 0;
				}
			}
		}
		else
		{
			m_moduleData[singleModule].numberShotsFiredThisBurst = 0;
		}
		if (CurrentOwner is PlayerController && OnFinishAttack != null)
		{
			OnFinishAttack(CurrentOwner as PlayerController, this);
		}
		return flag;
	}

	public void AttemptedFireNeedReload()
	{
		PlayerController playerController = m_owner as PlayerController;
		Reload();
		if (OnReloadPressed != null && (bool)playerController)
		{
			OnReloadPressed(playerController, this, false);
		}
	}

	protected void OnActiveReloadSuccess()
	{
		FinishReload(true);
		float num = 1f;
		if (ActiveReloadActivated && m_owner is PlayerController && (m_owner as PlayerController).IsPrimaryPlayer)
		{
			num *= CogOfBattleItem.ACTIVE_RELOAD_DAMAGE_MULTIPLIER;
		}
		if (ActiveReloadActivatedPlayerTwo && m_owner is PlayerController && !(m_owner as PlayerController).IsPrimaryPlayer)
		{
			num *= CogOfBattleItem.ACTIVE_RELOAD_DAMAGE_MULTIPLIER;
		}
		if (LocalActiveReload)
		{
			num *= Mathf.Pow(activeReloadData.damageMultiply, SequentialActiveReloads + 1);
		}
		Debug.LogError("total damage multiplier: " + num);
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				m_moduleData[Volley.projectiles[i]].activeReloadDamageModifier = num;
			}
		}
		else
		{
			m_moduleData[singleModule].activeReloadDamageModifier = num;
		}
	}

	private void HandleOutOfAmmo()
	{
		if (m_owner is PlayerController)
		{
			PrepGunForThrow();
		}
		else
		{
			m_owner.aiShooter.Inventory.RemoveGunFromInventory(this);
		}
	}

	public void HandleShootAnimation(ProjectileModule module)
	{
		if (!(m_anim != null))
		{
			return;
		}
		string overrideShootAnimation = shootAnimation;
		if (module != null && !string.IsNullOrEmpty(finalShootAnimation) && module.IsFinalShot(m_moduleData[module], CurrentOwner))
		{
			overrideShootAnimation = finalShootAnimation;
		}
		if (m_isCritting && !string.IsNullOrEmpty(criticalFireAnimation))
		{
			overrideShootAnimation = criticalFireAnimation;
		}
		if (module != null && module.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			ProjectileModule.ChargeProjectile chargeProjectile = module.GetChargeProjectile(m_moduleData[module].chargeTime);
			if (chargeProjectile != null && chargeProjectile.UsesOverrideShootAnimation)
			{
				overrideShootAnimation = chargeProjectile.OverrideShootAnimation;
			}
		}
		PlayIfExists(overrideShootAnimation, true);
	}

	public void HandleShootEffects(ProjectileModule module)
	{
		Transform transform = ((!muzzleOffset) ? barrelOffset : muzzleOffset);
		Vector3 position = transform.position - new Vector3(0f, 0f, 0.1f);
		VFXPool vFXPool = muzzleFlashEffects;
		if (module != null && finalMuzzleFlashEffects.type != 0 && module.IsFinalShot(m_moduleData[module], CurrentOwner))
		{
			vFXPool = finalMuzzleFlashEffects;
		}
		if (m_isCritting && CriticalMuzzleFlashEffects.type != 0)
		{
			vFXPool = CriticalMuzzleFlashEffects;
		}
		if (module != null && module.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			ProjectileModule.ChargeProjectile chargeProjectile = module.GetChargeProjectile(m_moduleData[module].chargeTime);
			if (chargeProjectile != null && chargeProjectile.UsesOverrideMuzzleFlashVfxPool)
			{
				vFXPool = chargeProjectile.OverrideMuzzleFlashVfxPool;
			}
		}
		if (!usesContinuousMuzzleFlash || !m_isContinuousMuzzleFlashOut)
		{
			vFXPool.SpawnAtPosition(position, (!preventRotation) ? gunAngle : 0f, transform, Vector2.zero, Vector2.zero, -0.05f, true);
		}
		if (usesContinuousMuzzleFlash)
		{
			m_isContinuousMuzzleFlashOut = true;
		}
		if (shellsToLaunchOnFire <= 0)
		{
			return;
		}
		if (shellCasingOnFireFrameDelay <= 0)
		{
			for (int i = 0; i < shellsToLaunchOnFire; i++)
			{
				SpawnShellCasingAtPosition(CasingLaunchPoint);
			}
		}
		else
		{
			StartCoroutine(HandleShellCasingFireDelay());
		}
	}

	private void TogglePreviousChargeEffectsIfNecessary(ProjectileModule.ChargeProjectile cp, bool visible)
	{
		if (cp != null)
		{
			if (cp.previousChargeProjectile != null && cp.previousChargeProjectile.DelayedVFXDestruction)
			{
				TogglePreviousChargeEffectsIfNecessary(cp.previousChargeProjectile, visible);
			}
			if (cp.UsesVfx && cp.VfxPool != null)
			{
				cp.VfxPool.ToggleRenderers(visible);
			}
		}
	}

	private void DestroyPreviousChargeEffectsIfNecessary(ProjectileModule.ChargeProjectile cp)
	{
		if (cp.previousChargeProjectile != null && cp.previousChargeProjectile.DelayedVFXDestruction)
		{
			DestroyPreviousChargeEffectsIfNecessary(cp.previousChargeProjectile);
		}
		if (cp.UsesVfx)
		{
			cp.VfxPool.DestroyAll();
		}
	}

	private void HandleChargeEffects(ProjectileModule.ChargeProjectile oldChargeProjectile, ProjectileModule.ChargeProjectile newChargeProjectile)
	{
		Transform transform = (chargeOffset ? chargeOffset : ((!muzzleOffset) ? barrelOffset : muzzleOffset));
		Vector3 position = transform.position - new Vector3(0f, 0f, 0.1f);
		if (oldChargeProjectile != null)
		{
			if (!oldChargeProjectile.DelayedVFXDestruction || newChargeProjectile == null)
			{
				DestroyPreviousChargeEffectsIfNecessary(oldChargeProjectile);
			}
			if (oldChargeProjectile.UsesVfx && oldChargeProjectile.VfxPool == m_currentlyPlayingChargeVFX)
			{
				m_currentlyPlayingChargeVFX = null;
			}
			if (oldChargeProjectile.UsesScreenShake)
			{
				GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
			}
		}
		if (newChargeProjectile == null)
		{
			return;
		}
		newChargeProjectile.previousChargeProjectile = oldChargeProjectile;
		if (newChargeProjectile.UsesVfx)
		{
			newChargeProjectile.VfxPool.SpawnAtPosition(position, gunAngle, transform, Vector2.zero, Vector2.zero, 2f, true);
			m_currentlyPlayingChargeVFX = newChargeProjectile.VfxPool;
			if (!m_meshRenderer.enabled)
			{
				m_currentlyPlayingChargeVFX.ToggleRenderers(false);
			}
			else
			{
				m_currentlyPlayingChargeVFX.ToggleRenderers(true);
			}
		}
		if (newChargeProjectile.ShouldDoChargePoof && m_owner is PlayerController)
		{
			DoChargeCompletePoof();
		}
		if (newChargeProjectile.UsesScreenShake)
		{
			GameManager.Instance.MainCameraController.DoContinuousScreenShake(newChargeProjectile.ScreenShake, this, m_owner is PlayerController);
		}
	}

	private void DoChargeCompletePoof()
	{
		GameObject gameObject = SpawnManager.SpawnVFX(BraveResources.Load<GameObject>("Global VFX/VFX_DBZ_Charge"));
		gameObject.transform.parent = m_owner.transform;
		gameObject.transform.position = m_owner.specRigidbody.UnitCenter;
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		component.HeightOffGround = -1f;
		component.UpdateZDepth();
		(CurrentOwner as PlayerController).DoVibration(Vibration.Time.Quick, Vibration.Strength.Medium);
	}

	private void HandleChargeIntensity(ProjectileModule module, ModuleShootData shootData)
	{
		if (!light)
		{
			return;
		}
		ProjectileModule.ChargeProjectile chargeProjectile = module.GetChargeProjectile(shootData.chargeTime);
		if (chargeProjectile != null)
		{
			float num = ((!chargeProjectile.UsesLightIntensity) ? baseLightIntensity : chargeProjectile.LightIntensity);
			float chargeTime = chargeProjectile.ChargeTime;
			float b = num;
			float b2 = chargeTime;
			int num2 = module.chargeProjectiles.IndexOf(chargeProjectile);
			if (num2 < module.chargeProjectiles.Count - 1)
			{
				b = ((!module.chargeProjectiles[num2 + 1].UsesLightIntensity) ? baseLightIntensity : module.chargeProjectiles[num2 + 1].LightIntensity);
				b2 = module.chargeProjectiles[num2 + 1].ChargeTime;
			}
			light.intensity = Mathf.Lerp(num, b, Mathf.InverseLerp(chargeTime, b2, shootData.chargeTime));
		}
	}

	private void EndChargeIntensity()
	{
		if ((bool)light)
		{
			light.intensity = baseLightIntensity;
		}
	}

	private IEnumerator HandleShellCasingFireDelay()
	{
		if (m_anim != null && m_anim.CurrentClip != null)
		{
			float frameLength = 1f / m_anim.CurrentClip.fps;
			yield return new WaitForSeconds(frameLength * (float)shellCasingOnFireFrameDelay);
		}
		if ((bool)this && (bool)m_owner)
		{
			for (int i = 0; i < shellsToLaunchOnFire; i++)
			{
				SpawnShellCasingAtPosition(CasingLaunchPoint);
			}
		}
	}

	private void SpawnShellCasingAtPosition(Vector3 position)
	{
		if (!(shellCasing != null) || !m_transform)
		{
			return;
		}
		GameObject gameObject = SpawnManager.SpawnDebris(shellCasing, position.WithZ(m_transform.position.z), Quaternion.Euler(0f, 0f, gunAngle));
		ShellCasing component = gameObject.GetComponent<ShellCasing>();
		if (component != null)
		{
			component.Trigger();
		}
		DebrisObject component2 = gameObject.GetComponent<DebrisObject>();
		if (!(component2 != null))
		{
			return;
		}
		int num = ((component2.transform.right.x > 0f) ? 1 : (-1));
		Vector3 vector = Vector3.up * (UnityEngine.Random.value * 1.5f + 1f) + -1.5f * Vector3.right * num * (UnityEngine.Random.value + 1.5f);
		Vector3 startingForce = new Vector3(vector.x, 0f, vector.y);
		if (m_owner is PlayerController)
		{
			PlayerController playerController = m_owner as PlayerController;
			if (playerController.CurrentRoom != null && playerController.CurrentRoom.area.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.CATACOMBS_BRIDGE_ROOM)
			{
				startingForce = (vector.x * (float)num * -1f * (barrelOffset.position.XY() - m_localAimPoint).normalized).ToVector3ZUp(vector.y);
			}
		}
		float y = m_owner.transform.position.y;
		float num2 = position.y - m_owner.transform.position.y + 0.2f;
		float num3 = component2.transform.position.y - y + UnityEngine.Random.value * 0.5f;
		component2.additionalHeightBoost = num2 - num3;
		if (gunAngle > 25f && gunAngle < 155f)
		{
			component2.additionalHeightBoost += -0.25f;
		}
		else
		{
			component2.additionalHeightBoost += 0.25f;
		}
		component2.Trigger(startingForce, num3);
	}

	private void SpawnClipAtPosition(Vector3 position)
	{
		if (!(clipObject != null))
		{
			return;
		}
		GameObject gameObject = SpawnManager.SpawnDebris(clipObject, position.WithZ(-0.05f), Quaternion.Euler(0f, 0f, gunAngle));
		DebrisObject component = gameObject.GetComponent<DebrisObject>();
		if (!component)
		{
			return;
		}
		float startingHeight = 0.25f;
		int num = ((component.transform.right.x > 0f) ? 1 : (-1));
		Vector3 startingForce = new Vector3(0f, -1f, 0f);
		if (m_owner is PlayerController)
		{
			PlayerController playerController = m_owner as PlayerController;
			if (playerController.CurrentRoom != null && playerController.CurrentRoom.area.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.CATACOMBS_BRIDGE_ROOM)
			{
				startingForce = new Vector3((float)num * 0.5f * (barrelOffset.position.XY() - m_localAimPoint).x, 0f, 1f);
				startingHeight = 0.5f;
			}
		}
		component.Trigger(startingForce, startingHeight);
	}

	private void DoScreenShake()
	{
		Vector2 dir = Quaternion.Euler(0f, 0f, gunAngle) * Vector3.right;
		if (!(GameManager.Instance.MainCameraController == null))
		{
			if (directionlessScreenShake)
			{
				dir = Vector2.zero;
			}
			GameManager.Instance.MainCameraController.DoGunScreenShake(gunScreenShake, dir, null, m_owner as PlayerController);
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			RoomHandler.unassignedInteractableObjects.Remove(this);
		}
		if (GameManager.Instance.InTutorial)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerAcquiredGun");
		}
		m_isThrown = false;
		m_isBeingEyedByRat = false;
		OnSharedPickup();
		if (!HasEverBeenAcquiredByPlayer)
		{
			player.HasReceivedNewGunThisFloor = true;
		}
		HasEverBeenAcquiredByPlayer = true;
		if (!ShouldBeDestroyedOnExistence())
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.GUNS_PICKED_UP, 1f);
			if (!PileOfDarkSoulsPickup.IsPileOfDarkSoulsPickup)
			{
				player.PlayEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_Ammo_Sparkles_001") as GameObject, Vector3.zero);
			}
			HandleEncounterable(player);
			GetRidOfMinimapIcon();
			if (GameManager.AUDIO_ENABLED)
			{
				AkSoundEngine.PostEvent("Play_OBJ_weapon_pickup_01", base.gameObject);
			}
			if (player.CharacterUsesRandomGuns)
			{
				player.ChangeToRandomGun();
			}
			else
			{
				Gun gun = player.inventory.AddGunToInventory(this, true);
				if (gun.AdjustedMaxAmmo > 0)
				{
					gun.ammo = Math.Min(gun.AdjustedMaxAmmo, gun.ammo);
				}
			}
			PlatformInterface.SetAlienFXColor(m_alienPickupColor, 1f);
		}
		if (base.transform.parent != null)
		{
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public bool HasShootStyle(ProjectileModule.ShootStyle shootStyle)
	{
		ProjectileVolleyData volley = Volley;
		if (Volley == null)
		{
			return singleModule.shootStyle == shootStyle;
		}
		for (int i = 0; i < volley.projectiles.Count; i++)
		{
			if (volley.projectiles[i] != null && volley.projectiles[i].shootStyle == shootStyle)
			{
				return true;
			}
		}
		return false;
	}

	protected void AnimationCompleteDelegate(tk2dSpriteAnimator anima, tk2dSpriteAnimationClip clippy)
	{
		if (clippy != null && (!DisablesRendererOnCooldown || !m_reloadWhenDoneFiring))
		{
			PlayIdleAnimation();
		}
	}

	public void OnTrigger(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody source, CollisionData collisionData)
	{
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if (component != null && BraveInput.WasSelectPressed())
		{
			Pickup(component);
		}
	}

	private void PostRigidbodyMovement(SpeculativeRigidbody specRigidbody, Vector2 unitDelta, IntVector2 pixelDelta)
	{
		if (!this || !base.enabled)
		{
			return;
		}
		for (int num = m_activeBeams.Count - 1; num >= 0; num--)
		{
			if (num >= 0 && num < m_activeBeams.Count)
			{
				ModuleShootData moduleShootData = m_activeBeams[num];
				if (!moduleShootData.beam)
				{
					if (moduleShootData.beamKnockbackID >= 0)
					{
						if ((bool)m_owner && (bool)m_owner.knockbackDoer)
						{
							m_owner.knockbackDoer.EndContinuousKnockback(moduleShootData.beamKnockbackID);
						}
						moduleShootData.beamKnockbackID = -1;
					}
					m_activeBeams.RemoveAt(num);
				}
				else
				{
					moduleShootData.beam.LateUpdatePosition(m_unroundedBarrelPosition + (Vector3)unitDelta);
				}
			}
		}
	}

	private bool CheckHasLoadedModule(ProjectileModule module)
	{
		if (RequiresFundsToShoot && m_owner is PlayerController)
		{
			m_moduleData[module].numberShotsFired = 0;
			m_moduleData[module].needsReload = false;
			return (m_owner as PlayerController).carriedConsumables.Currency > 0;
		}
		if (module.ignoredForReloadPurposes)
		{
			return false;
		}
		if (m_moduleData[module].needsReload)
		{
			return false;
		}
		return true;
	}

	private bool CheckHasLoadedModule(ProjectileVolleyData Volley)
	{
		if (RequiresFundsToShoot && m_owner is PlayerController)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				m_moduleData[Volley.projectiles[i]].numberShotsFired = 0;
				m_moduleData[Volley.projectiles[i]].needsReload = false;
			}
			return (m_owner as PlayerController).carriedConsumables.Currency > 0;
		}
		for (int j = 0; j < Volley.projectiles.Count; j++)
		{
			ProjectileModule projectileModule = Volley.projectiles[j];
			if (!projectileModule.ignoredForReloadPurposes && !m_moduleData[projectileModule].needsReload)
			{
				return true;
			}
		}
		return false;
	}

	private void CreateAmp()
	{
		if (!ObjectToInstantiateOnReload || !m_owner || !(m_owner is PlayerController))
		{
			return;
		}
		if (m_extantAmp != null)
		{
			if ((bool)(m_extantAmp as ShootProjectileOnGunfireDoer))
			{
				m_extantAmp.Deactivate();
				m_extantAmp = null;
			}
			else if ((bool)(m_extantAmp as BreakableShieldController) && !(m_extantAmp as BreakableShieldController).majorBreakable.IsDestroyed)
			{
				return;
			}
		}
		PlayerController playerController = m_owner as PlayerController;
		GameObject gameObject = UnityEngine.Object.Instantiate(position: playerController.CurrentRoom.GetBestRewardLocation(IntVector2.One, RoomHandler.RewardLocationStyle.PlayerCenter, false).ToVector3(), original: ObjectToInstantiateOnReload, rotation: Quaternion.identity);
		if ((bool)gameObject)
		{
			m_extantAmp = gameObject.GetInterface<SingleSpawnableGunPlacedObject>();
			if (m_extantAmp != null)
			{
				m_extantAmp.Initialize(this);
			}
		}
	}

	private bool HandleInitialGunShoot(ProjectileModule module, ProjectileData overrideProjectileData = null, GameObject overrideBulletObject = null)
	{
		if (m_moduleData[module].needsReload)
		{
			Debug.LogError("Trying to shoot a gun without being loaded, should never happen.");
			return false;
		}
		if (m_moduleData[module].onCooldown)
		{
			return false;
		}
		if (UsesRechargeLikeActiveItem && m_remainingActiveCooldownAmount > 0f)
		{
			return false;
		}
		return HandleSpecificInitialGunShoot(module, overrideProjectileData, overrideBulletObject);
	}

	private void IncrementModuleFireCountAndMarkReload(ProjectileModule mod, ProjectileModule.ChargeProjectile currentChargeProjectile)
	{
		m_moduleData[mod].numberShotsFired++;
		m_moduleData[mod].numberShotsFiredThisBurst++;
		if (m_moduleData[mod].numberShotsActiveReload > 0)
		{
			m_moduleData[mod].numberShotsActiveReload--;
		}
		if (currentChargeProjectile != null && currentChargeProjectile.DepleteAmmo)
		{
			foreach (ProjectileModule key in m_moduleData.Keys)
			{
				if (!key.IsDuctTapeModule)
				{
					m_moduleData[key].numberShotsFired = key.GetModNumberOfShotsInClip(CurrentOwner);
					m_moduleData[key].needsReload = true;
				}
			}
		}
		if (mod.GetModNumberOfShotsInClip(CurrentOwner) > 0 && m_moduleData[mod].numberShotsFired >= mod.GetModNumberOfShotsInClip(CurrentOwner))
		{
			m_moduleData[mod].needsReload = true;
		}
		if (mod.shootStyle != ProjectileModule.ShootStyle.Charged)
		{
			mod.IncrementShootCount();
		}
	}

	private bool RawFireVolley(ProjectileVolleyData Volley)
	{
		bool flag = false;
		bool flag2 = true;
		for (int i = 0; i < Volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = Volley.projectiles[i];
			if (!m_moduleData[projectileModule].needsReload && !m_moduleData[projectileModule].onCooldown && (!UsesRechargeLikeActiveItem || !(m_remainingActiveCooldownAmount > 0f)))
			{
				if (Volley.ModulesAreTiers)
				{
					int num = ((projectileModule.CloneSourceIndex < 0) ? i : projectileModule.CloneSourceIndex);
					flag2 = ((num == m_currentStrengthTier) ? true : false);
				}
				if (flag2)
				{
					flag |= HandleSpecificInitialGunShoot(projectileModule, null, null, false);
				}
				else if (!m_cachedIsGunBlocked)
				{
					IncrementModuleFireCountAndMarkReload(projectileModule, null);
				}
			}
		}
		return flag;
	}

	private bool HandleInitialGunShoot(ProjectileVolleyData Volley, ProjectileData overrideProjectileData = null, GameObject overrideBulletObject = null)
	{
		bool playEffects = true;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		bool flag4 = false;
		for (int i = 0; i < Volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = Volley.projectiles[i];
			if (m_moduleData[projectileModule].needsReload)
			{
				continue;
			}
			flag = true;
			if (m_moduleData[projectileModule].onCooldown || (UsesRechargeLikeActiveItem && m_remainingActiveCooldownAmount > 0f))
			{
				continue;
			}
			if (Volley.ModulesAreTiers)
			{
				if (projectileModule.IsDuctTapeModule)
				{
					flag3 = true;
				}
				else
				{
					int num = ((projectileModule.CloneSourceIndex < 0) ? i : projectileModule.CloneSourceIndex);
					if (num == m_currentStrengthTier)
					{
						playEffects = !flag4;
						flag3 = true;
						flag4 = true;
					}
					else
					{
						playEffects = false;
						flag3 = false;
					}
				}
			}
			if (flag3)
			{
				flag2 |= HandleSpecificInitialGunShoot(projectileModule, overrideProjectileData, overrideBulletObject, playEffects);
			}
			else if (!m_cachedIsGunBlocked)
			{
				IncrementModuleFireCountAndMarkReload(projectileModule, null);
			}
			playEffects = false;
		}
		if (!flag)
		{
			Debug.LogError("Trying to shoot a gun without being loaded, should never happen.");
		}
		return flag2;
	}

	private bool HandleSpecificInitialGunShoot(ProjectileModule module, ProjectileData overrideProjectileData = null, GameObject overrideBulletObject = null, bool playEffects = true)
	{
		if (module.shootStyle == ProjectileModule.ShootStyle.SemiAutomatic || module.shootStyle == ProjectileModule.ShootStyle.Burst || module.shootStyle == ProjectileModule.ShootStyle.Automatic)
		{
			if (m_cachedIsGunBlocked)
			{
				return false;
			}
			if (playEffects)
			{
				HandleShootAnimation(module);
				HandleShootEffects(module);
				if (doesScreenShake)
				{
					DoScreenShake();
				}
			}
			if (playEffects || (module.runtimeGuid != null && AdditionalShootSoundsByModule.ContainsKey(module.runtimeGuid)))
			{
				if (module.runtimeGuid != null && AdditionalShootSoundsByModule.ContainsKey(module.runtimeGuid))
				{
					AkSoundEngine.SetSwitch("WPN_Guns", AdditionalShootSoundsByModule[module.runtimeGuid], base.gameObject);
				}
				if (GameManager.AUDIO_ENABLED && (!isAudioLoop || !m_isAudioLooping))
				{
					string in_pszEventName = ((!module.IsFinalShot(m_moduleData[module], m_owner) || OverrideFinaleAudio) ? "Play_WPN_gun_shot_01" : "Play_WPN_gun_finale_01");
					if (!PreventNormalFireAudio)
					{
						AkSoundEngine.PostEvent(in_pszEventName, base.gameObject);
					}
					else
					{
						AkSoundEngine.PostEvent(OverrideNormalFireAudioEvent, base.gameObject);
					}
					m_isAudioLooping = true;
				}
				if (!string.IsNullOrEmpty(gunSwitchGroup))
				{
					AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
				}
			}
			ShootSingleProjectile(module, overrideProjectileData, overrideBulletObject);
			DecrementAmmoCost(module);
			TriggerModuleCooldown(module);
			return true;
		}
		if (module.shootStyle == ProjectileModule.ShootStyle.Beam)
		{
			if (m_cachedIsGunBlocked)
			{
				return false;
			}
			if (playEffects)
			{
				if (m_anim != null)
				{
					PlayIfExists(shootAnimation);
				}
				HandleShootEffects(module);
			}
			if (playEffects || (module.runtimeGuid != null && AdditionalShootSoundsByModule.ContainsKey(module.runtimeGuid)))
			{
				if (module.runtimeGuid != null && AdditionalShootSoundsByModule.ContainsKey(module.runtimeGuid))
				{
					AkSoundEngine.SetSwitch("WPN_Guns", AdditionalShootSoundsByModule[module.runtimeGuid], base.gameObject);
				}
				if (GameManager.AUDIO_ENABLED && (!isAudioLoop || !m_isAudioLooping))
				{
					string in_pszEventName2 = ((!module.IsFinalShot(m_moduleData[module], m_owner) || OverrideFinaleAudio) ? "Play_WPN_gun_shot_01" : "Play_WPN_gun_finale_01");
					if (!PreventNormalFireAudio)
					{
						AkSoundEngine.PostEvent(in_pszEventName2, base.gameObject);
					}
					m_isAudioLooping = true;
				}
				if (!string.IsNullOrEmpty(gunSwitchGroup))
				{
					AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
				}
			}
			BeginFiringBeam(module);
			return true;
		}
		if (module.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			ModuleShootData moduleShootData = m_moduleData[module];
			moduleShootData.chargeTime = 0f;
			moduleShootData.chargeFired = false;
			if (playEffects)
			{
				PlayIfExists(chargeAnimation);
				ProjectileModule.ChargeProjectile chargeProjectile = module.GetChargeProjectile(moduleShootData.chargeTime);
				HandleChargeEffects(null, chargeProjectile);
				HandleChargeIntensity(module, moduleShootData);
				moduleShootData.lastChargeProjectile = chargeProjectile;
				if (GameManager.AUDIO_ENABLED)
				{
					AkSoundEngine.PostEvent("Play_WPN_gun_charge_01", base.gameObject);
				}
			}
			return true;
		}
		return false;
	}

	private bool HandleContinueGunShoot(ProjectileModule module, bool canAttack = true, ProjectileData overrideProjectileData = null)
	{
		if (m_moduleData[module].needsReload)
		{
			Debug.LogError("Attempting to continue fire on an unloaded gun. This should never happen.");
			return false;
		}
		if (m_moduleData[module].onCooldown)
		{
			return false;
		}
		if (UsesRechargeLikeActiveItem && m_remainingActiveCooldownAmount > 0f)
		{
			return false;
		}
		return HandleSpecificContinueGunShoot(module, canAttack, overrideProjectileData);
	}

	private bool HandleContinueGunShoot(ProjectileVolleyData Volley, bool canAttack = true, ProjectileData overrideProjectileData = null)
	{
		bool playEffects = true;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = true;
		for (int i = 0; i < Volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = Volley.projectiles[i];
			if (m_moduleData[projectileModule].needsReload)
			{
				continue;
			}
			flag = true;
			if (m_moduleData[projectileModule].onCooldown || (UsesRechargeLikeActiveItem && m_remainingActiveCooldownAmount > 0f))
			{
				continue;
			}
			if (Volley.ModulesAreTiers)
			{
				if (projectileModule.IsDuctTapeModule)
				{
					flag3 = true;
				}
				else
				{
					int num = ((projectileModule.CloneSourceIndex < 0) ? i : projectileModule.CloneSourceIndex);
					if (num == m_currentStrengthTier)
					{
						playEffects = true;
						flag3 = true;
					}
					else
					{
						playEffects = false;
						flag3 = false;
					}
				}
			}
			if (projectileModule.isExternalAddedModule)
			{
				playEffects = false;
			}
			if (flag3)
			{
				flag2 |= HandleSpecificContinueGunShoot(projectileModule, canAttack, overrideProjectileData, playEffects);
			}
			else if ((projectileModule.shootStyle == ProjectileModule.ShootStyle.Automatic || projectileModule.shootStyle == ProjectileModule.ShootStyle.Burst) && !m_cachedIsGunBlocked && canAttack)
			{
				IncrementModuleFireCountAndMarkReload(projectileModule, null);
			}
			if (flag2)
			{
				playEffects = false;
			}
		}
		if (!flag)
		{
			Debug.LogError("Attempting to continue fire without being loaded. This should never happen.");
		}
		return flag2;
	}

	private bool HandleSpecificContinueGunShoot(ProjectileModule module, bool canAttack = true, ProjectileData overrideProjectileData = null, bool playEffects = true)
	{
		if (module.shootStyle == ProjectileModule.ShootStyle.Automatic || module.shootStyle == ProjectileModule.ShootStyle.Burst)
		{
			if (m_cachedIsGunBlocked)
			{
				return false;
			}
			if (!canAttack)
			{
				return false;
			}
			if (module.shootStyle == ProjectileModule.ShootStyle.Burst && m_moduleData[module].numberShotsFiredThisBurst >= module.burstShotCount)
			{
				m_moduleData[module].numberShotsFiredThisBurst = 0;
				if (OnBurstContinued != null)
				{
					OnBurstContinued(CurrentOwner as PlayerController, this);
				}
			}
			if (playEffects)
			{
				if (!usesContinuousFireAnimation)
				{
					string animName = ((string.IsNullOrEmpty(finalShootAnimation) || !module.IsFinalShot(m_moduleData[module], CurrentOwner)) ? shootAnimation : finalShootAnimation);
					Play(animName);
				}
				HandleShootEffects(module);
				if (doesScreenShake)
				{
					DoScreenShake();
				}
			}
			if (playEffects || (module.runtimeGuid != null && AdditionalShootSoundsByModule.ContainsKey(module.runtimeGuid)))
			{
				if (module.runtimeGuid != null && AdditionalShootSoundsByModule.ContainsKey(module.runtimeGuid))
				{
					AkSoundEngine.SetSwitch("WPN_Guns", AdditionalShootSoundsByModule[module.runtimeGuid], base.gameObject);
				}
				if (GameManager.AUDIO_ENABLED && (!isAudioLoop || !m_isAudioLooping))
				{
					string in_pszEventName = ((!module.IsFinalShot(m_moduleData[module], m_owner) || OverrideFinaleAudio) ? "Play_WPN_gun_shot_01" : "Play_WPN_gun_finale_01");
					if (!PreventNormalFireAudio)
					{
						AkSoundEngine.PostEvent(in_pszEventName, base.gameObject);
					}
					m_isAudioLooping = true;
				}
				if (!string.IsNullOrEmpty(gunSwitchGroup))
				{
					AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
				}
			}
			ShootSingleProjectile(module, overrideProjectileData);
			DecrementAmmoCost(module);
			TriggerModuleCooldown(module);
			return true;
		}
		if (module.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			ModuleShootData moduleShootData = m_moduleData[module];
			if (!moduleShootData.chargeFired)
			{
				float num = 1f;
				if (m_owner is PlayerController)
				{
					num = (m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.ChargeAmountMultiplier);
				}
				moduleShootData.chargeTime += BraveTime.DeltaTime * num;
				if (module.maxChargeTime > 0f && moduleShootData.chargeTime >= module.maxChargeTime && canAttack && !m_cachedIsGunBlocked)
				{
					if (playEffects)
					{
						if (!usesContinuousFireAnimation)
						{
							Play(shootAnimation);
						}
						HandleShootEffects(module);
						if (moduleShootData.lastChargeProjectile != null)
						{
							if (GameManager.AUDIO_ENABLED)
							{
								int num2 = module.chargeProjectiles.IndexOf(moduleShootData.lastChargeProjectile);
								string arg = ((!module.IsFinalShot(m_moduleData[module], m_owner) || OverrideFinaleAudio) ? "Play_WPN_gun_shot_" : "Play_WPN_gun_finale_");
								if (GameManager.AUDIO_ENABLED && (!isAudioLoop || !m_isAudioLooping))
								{
									AkSoundEngine.PostEvent(string.Format("{0}{1:D2}", arg, num2 + 1), base.gameObject);
									m_isAudioLooping = true;
								}
								if (moduleShootData.lastChargeProjectile.UsesAdditionalWwiseEvent)
								{
									AkSoundEngine.PostEvent(moduleShootData.lastChargeProjectile.AdditionalWwiseEvent, base.gameObject);
								}
							}
							HandleChargeEffects(moduleShootData.lastChargeProjectile, null);
							EndChargeIntensity();
							moduleShootData.lastChargeProjectile = null;
						}
						if (doesScreenShake)
						{
							DoScreenShake();
						}
					}
					ShootSingleProjectile(module, overrideProjectileData);
					DecrementAmmoCost(module);
					TriggerModuleCooldown(module);
					moduleShootData.chargeFired = true;
					return true;
				}
				if (playEffects)
				{
					ProjectileModule.ChargeProjectile chargeProjectile = module.GetChargeProjectile(moduleShootData.chargeTime);
					PlayIfExistsAndNotPlaying(chargeAnimation);
					if (chargeProjectile != moduleShootData.lastChargeProjectile)
					{
						if (GameManager.AUDIO_ENABLED)
						{
							int num3 = module.chargeProjectiles.IndexOf(chargeProjectile);
							if (GameManager.AUDIO_ENABLED)
							{
								AkSoundEngine.PostEvent(string.Format("Play_WPN_gun_charge_{0:D2}", num3 + 2), base.gameObject);
							}
						}
						HandleChargeEffects(moduleShootData.lastChargeProjectile, chargeProjectile);
						moduleShootData.lastChargeProjectile = chargeProjectile;
					}
					HandleChargeIntensity(module, moduleShootData);
					if (CurrentOwner is PlayerController)
					{
						bool flag = chargeProjectile != null && (bool)chargeProjectile.Projectile;
						(CurrentOwner as PlayerController).DoSustainedVibration((!flag) ? Vibration.Strength.UltraLight : Vibration.Strength.Light);
					}
				}
				return false;
			}
			return true;
		}
		if (module.shootStyle == ProjectileModule.ShootStyle.Beam)
		{
			if (m_cachedIsGunBlocked)
			{
				return false;
			}
			ModuleShootData moduleShootData2 = m_moduleData[module];
			if (canAttack && !m_activeBeams.Contains(moduleShootData2))
			{
				bool playEffects2 = playEffects;
				HandleSpecificInitialGunShoot(module, overrideProjectileData, null, playEffects2);
			}
			else if (moduleShootData2 != null && (bool)moduleShootData2.beam)
			{
				BeamController beam = moduleShootData2.beam;
				beam.Direction = GetBeamAimDirection(moduleShootData2.angleForShot);
				beam.Origin = m_unroundedBarrelPosition;
				if (beam.knocksShooterBack && moduleShootData2.beamKnockbackID >= 0)
				{
					m_owner.knockbackDoer.UpdateContinuousKnockback(-beam.Direction, beam.knockbackStrength, moduleShootData2.beamKnockbackID);
				}
				if (beam.ShouldUseAmmo)
				{
					float num4 = ((!(m_owner is PlayerController)) ? 1f : (m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.RateOfFire));
					m_fractionalAmmoUsage += BraveTime.DeltaTime * (float)module.ammoCost * num4;
					if (m_fractionalAmmoUsage > 1f)
					{
						ammo = Math.Max(0, ammo - (int)(m_fractionalAmmoUsage / 1f));
						if (module.numberOfShotsInClip > 0)
						{
							moduleShootData2.numberShotsFired += (int)(m_fractionalAmmoUsage / 1f);
							if (module.GetModNumberOfShotsInClip(CurrentOwner) > 0 && moduleShootData2.numberShotsFired >= module.GetModNumberOfShotsInClip(CurrentOwner))
							{
								moduleShootData2.needsReload = true;
							}
						}
						DecrementCustomAmmunitions((int)(m_fractionalAmmoUsage / 1f));
						m_fractionalAmmoUsage %= 1f;
					}
				}
			}
			return true;
		}
		return false;
	}

	private bool HandleEndGunShoot(ProjectileModule module, bool canAttack = true, ProjectileData overrideProjectileData = null)
	{
		if (m_moduleData[module].needsReload)
		{
			return false;
		}
		if (m_moduleData[module].onCooldown)
		{
			return false;
		}
		if (UsesRechargeLikeActiveItem && m_remainingActiveCooldownAmount > 0f)
		{
			return false;
		}
		return HandleSpecificEndGunShoot(module, canAttack, overrideProjectileData);
	}

	private bool HandleEndGunShoot(ProjectileVolleyData Volley, bool canAttack = true, ProjectileData overrideProjectileData = null)
	{
		bool playEffects = true;
		bool flag = false;
		foreach (ProjectileModule projectile in Volley.projectiles)
		{
			if (!m_moduleData[projectile].needsReload && !m_moduleData[projectile].onCooldown && (!UsesRechargeLikeActiveItem || !(m_remainingActiveCooldownAmount > 0f)))
			{
				flag |= HandleSpecificEndGunShoot(projectile, canAttack, overrideProjectileData, playEffects);
				if (flag)
				{
					playEffects = false;
				}
			}
		}
		return flag;
	}

	private bool HandleSpecificEndGunShoot(ProjectileModule module, bool canAttack = true, ProjectileData overrideProjectileData = null, bool playEffects = true)
	{
		if (module.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			ModuleShootData moduleShootData = m_moduleData[module];
			if (!moduleShootData.chargeFired)
			{
				float num = 1f;
				if (m_owner is PlayerController)
				{
					num = (m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.ChargeAmountMultiplier);
				}
				moduleShootData.chargeTime += BraveTime.DeltaTime * num;
				ProjectileModule.ChargeProjectile chargeProjectile = module.GetChargeProjectile(moduleShootData.chargeTime);
				if (chargeProjectile != null && chargeProjectile.Projectile != null && canAttack && !m_cachedIsGunBlocked)
				{
					if (playEffects)
					{
						if (!usesContinuousFireAnimation)
						{
							HandleShootAnimation(module);
						}
						if (GameManager.AUDIO_ENABLED)
						{
							int num2 = module.chargeProjectiles.IndexOf(moduleShootData.lastChargeProjectile);
							string text = ((!module.IsFinalShot(m_moduleData[module], m_owner) || OverrideFinaleAudio) ? "Play_WPN_gun_shot_" : "Play_WPN_gun_finale_");
							if (GameManager.AUDIO_ENABLED && (!isAudioLoop || !m_isAudioLooping) && !PreventNormalFireAudio)
							{
								if (PickupObjectId == GlobalItemIds.Starpew && text == "Play_WPN_gun_shot_" && moduleShootData.chargeTime >= 2f)
								{
									AkSoundEngine.PostEvent("Play_WPN_Starpew_Blast_01", base.gameObject);
								}
								else
								{
									AkSoundEngine.PostEvent(string.Format("{0}{1:D2}", text, num2 + 1), base.gameObject);
								}
								m_isAudioLooping = true;
							}
							if (moduleShootData.lastChargeProjectile != null && moduleShootData.lastChargeProjectile.UsesAdditionalWwiseEvent)
							{
								AkSoundEngine.PostEvent(moduleShootData.lastChargeProjectile.AdditionalWwiseEvent, base.gameObject);
							}
						}
						HandleShootEffects(module);
						if (moduleShootData.lastChargeProjectile != null)
						{
							HandleChargeEffects(moduleShootData.lastChargeProjectile, null);
							EndChargeIntensity();
							moduleShootData.lastChargeProjectile = null;
						}
						if (doesScreenShake)
						{
							DoScreenShake();
						}
					}
					else if (moduleShootData.lastChargeProjectile != null)
					{
						HandleChargeEffects(moduleShootData.lastChargeProjectile, null);
						EndChargeIntensity();
						moduleShootData.lastChargeProjectile = null;
					}
					ShootSingleProjectile(module, overrideProjectileData);
					DecrementAmmoCost(module);
					TriggerModuleCooldown(module);
					moduleShootData.chargeFired = true;
					return true;
				}
				if (playEffects)
				{
					if (!string.IsNullOrEmpty(dischargeAnimation))
					{
						Play(dischargeAnimation);
					}
					else
					{
						PlayIdleAnimation();
					}
					if (moduleShootData.lastChargeProjectile != null)
					{
						HandleChargeEffects(moduleShootData.lastChargeProjectile, null);
						EndChargeIntensity();
						moduleShootData.lastChargeProjectile = null;
					}
				}
				if (module.triggerCooldownForAnyChargeAmount)
				{
					TriggerModuleCooldown(module);
				}
				return false;
			}
		}
		else if (module.shootStyle == ProjectileModule.ShootStyle.Beam)
		{
			if (playEffects)
			{
				PlayIdleAnimation();
			}
			ModuleShootData moduleShootData2 = m_moduleData[module];
			if ((bool)moduleShootData2.beam)
			{
				if (moduleShootData2.beam.knocksShooterBack && moduleShootData2.beamKnockbackID >= 0)
				{
					m_owner.knockbackDoer.EndContinuousKnockback(moduleShootData2.beamKnockbackID);
					moduleShootData2.beamKnockbackID = -1;
				}
				if (doesScreenShake)
				{
					GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
				}
				moduleShootData2.beam.CeaseAttack();
				moduleShootData2.beam = null;
				m_activeBeams.Remove(moduleShootData2);
			}
			return true;
		}
		return false;
	}

	public void ForceFireProjectile(Projectile targetProjectile)
	{
		ProjectileModule projectileModule = null;
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				for (int j = 0; j < Volley.projectiles[j].projectiles.Count; j++)
				{
					if (targetProjectile.name.Contains(Volley.projectiles[j].projectiles[j].name))
					{
						projectileModule = Volley.projectiles[j];
						break;
					}
				}
				if (projectileModule != null)
				{
					break;
				}
			}
		}
		else
		{
			for (int k = 0; k < singleModule.projectiles.Count; k++)
			{
				if (targetProjectile.name.Contains(singleModule.projectiles[k].name))
				{
					projectileModule = singleModule;
					break;
				}
			}
		}
		if (projectileModule != null)
		{
			ShootSingleProjectile(projectileModule);
		}
	}

	private void DecrementCustomAmmunitions(int ammoCost)
	{
		for (int i = 0; i < m_customAmmunitions.Count; i++)
		{
			m_customAmmunitions[i].ShotsRemaining -= ammoCost;
			if (m_customAmmunitions[i].ShotsRemaining <= 0)
			{
				m_customAmmunitions.RemoveAt(i);
				i--;
			}
		}
	}

	private void ApplyCustomAmmunitionsToProjectile(Projectile target)
	{
		for (int i = 0; i < m_customAmmunitions.Count; i++)
		{
			ActiveAmmunitionData activeAmmunitionData = m_customAmmunitions[i];
			activeAmmunitionData.HandleAmmunition(target, this);
		}
	}

	private void ShootSingleProjectile(ProjectileModule mod, ProjectileData overrideProjectileData = null, GameObject overrideBulletObject = null)
	{
		PlayerController playerController = m_owner as PlayerController;
		AIActor aIActor = m_owner as AIActor;
		Projectile projectile = null;
		ProjectileModule.ChargeProjectile chargeProjectile = null;
		if ((bool)overrideBulletObject)
		{
			projectile = overrideBulletObject.GetComponent<Projectile>();
		}
		else if (mod.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			chargeProjectile = mod.GetChargeProjectile(m_moduleData[mod].chargeTime);
			if (chargeProjectile != null)
			{
				projectile = chargeProjectile.Projectile;
				projectile.pierceMinorBreakables = true;
			}
		}
		else
		{
			projectile = mod.GetCurrentProjectile(m_moduleData[mod], CurrentOwner);
		}
		if (!projectile)
		{
			m_moduleData[mod].numberShotsFired++;
			m_moduleData[mod].numberShotsFiredThisBurst++;
			if (m_moduleData[mod].numberShotsActiveReload > 0)
			{
				m_moduleData[mod].numberShotsActiveReload--;
			}
			if (mod.GetModNumberOfShotsInClip(CurrentOwner) > 0 && m_moduleData[mod].numberShotsFired >= mod.GetModNumberOfShotsInClip(CurrentOwner))
			{
				m_moduleData[mod].needsReload = true;
			}
			if (mod.shootStyle != ProjectileModule.ShootStyle.Charged)
			{
				mod.IncrementShootCount();
			}
			return;
		}
		if ((bool)playerController && playerController.OnPreFireProjectileModifier != null)
		{
			projectile = playerController.OnPreFireProjectileModifier(this, projectile);
		}
		if (m_isCritting && (bool)CriticalReplacementProjectile)
		{
			projectile = CriticalReplacementProjectile;
		}
		if (OnPreFireProjectileModifier != null)
		{
			projectile = OnPreFireProjectileModifier(this, projectile, mod);
		}
		if (GameManager.Instance.InTutorial && playerController != null)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("playerFiredGun");
		}
		Vector3 position = barrelOffset.position;
		position = new Vector3(position.x, position.y, -1f);
		float num = ((!(playerController != null)) ? 1f : playerController.stats.GetStatValue(PlayerStats.StatType.Accuracy));
		num = ((!(m_owner is DumbGunShooter) || !(m_owner as DumbGunShooter).overridesInaccuracy) ? num : (m_owner as DumbGunShooter).inaccuracyFraction);
		float angleForShot = mod.GetAngleForShot(m_moduleData[mod].alternateAngleSign, num);
		if (m_moduleData[mod].numberShotsActiveReload > 0 && activeReloadData.usesOverrideAngleVariance)
		{
			ProjectileModule projectileModule = mod;
			float varianceMultiplier = num;
			angleForShot = projectileModule.GetAngleForShot(1f, varianceMultiplier, activeReloadData.overrideAngleVariance);
		}
		if (mod.alternateAngle)
		{
			m_moduleData[mod].alternateAngleSign *= -1f;
		}
		if (LockedHorizontalOnCharge && LockedHorizontalCenterFireOffset >= 0f)
		{
			position = m_owner.specRigidbody.HitboxPixelCollider.UnitCenter + BraveMathCollege.DegreesToVector(gunAngle, LockedHorizontalCenterFireOffset);
		}
		GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, position + Quaternion.Euler(0f, 0f, gunAngle) * mod.positionOffset, Quaternion.Euler(0f, 0f, gunAngle + angleForShot));
		Projectile projectile2 = (LastProjectile = gameObject.GetComponent<Projectile>());
		projectile2.Owner = m_owner;
		projectile2.Shooter = m_owner.specRigidbody;
		projectile2.baseData.damage += damageModifier;
		projectile2.Inverted = mod.inverted;
		if (m_owner is PlayerController && (LocalActiveReload || (playerController.IsPrimaryPlayer && ActiveReloadActivated) || (!playerController.IsPrimaryPlayer && ActiveReloadActivatedPlayerTwo)))
		{
			projectile2.baseData.damage *= m_moduleData[mod].activeReloadDamageModifier;
		}
		if ((bool)m_owner.aiShooter)
		{
			projectile2.collidesWithEnemies = m_owner.aiShooter.CanShootOtherEnemies;
		}
		if (rampBullets)
		{
			projectile2.Ramp(rampStartHeight, rampTime);
			TrailController componentInChildren = gameObject.GetComponentInChildren<TrailController>();
			if ((bool)componentInChildren)
			{
				componentInChildren.rampHeight = true;
				componentInChildren.rampStartHeight = rampStartHeight;
				componentInChildren.rampTime = rampTime;
			}
		}
		if (m_owner is PlayerController)
		{
			PlayerStats stats = playerController.stats;
			projectile2.baseData.damage *= stats.GetStatValue(PlayerStats.StatType.Damage);
			projectile2.baseData.speed *= stats.GetStatValue(PlayerStats.StatType.ProjectileSpeed);
			projectile2.baseData.force *= stats.GetStatValue(PlayerStats.StatType.KnockbackMultiplier);
			projectile2.baseData.range *= stats.GetStatValue(PlayerStats.StatType.RangeMultiplier);
			if (playerController.inventory.DualWielding)
			{
				projectile2.baseData.damage *= s_DualWieldFactor;
			}
			if (CanSneakAttack && playerController.IsStealthed)
			{
				projectile2.baseData.damage *= SneakAttackDamageMultiplier;
			}
			if (m_isCritting)
			{
				projectile2.baseData.damage *= CriticalDamageMultiplier;
				projectile2.IsCritical = true;
			}
			if (UsesBossDamageModifier)
			{
				if (CustomBossDamageModifier >= 0f)
				{
					projectile2.BossDamageMultiplier = CustomBossDamageModifier;
				}
				else
				{
					projectile2.BossDamageMultiplier = 0.8f;
				}
			}
		}
		if (Volley != null && Volley.UsesShotgunStyleVelocityRandomizer)
		{
			projectile2.baseData.speed *= Volley.GetVolleySpeedMod();
		}
		if (aIActor != null && aIActor.IsBlackPhantom)
		{
			projectile2.baseData.speed *= aIActor.BlackPhantomProperties.BulletSpeedMultiplier;
		}
		if (m_moduleData[mod].numberShotsActiveReload > 0)
		{
			if (!activeReloadData.ActiveReloadStacks)
			{
				projectile2.baseData.damage *= activeReloadData.damageMultiply;
			}
			projectile2.baseData.force *= activeReloadData.knockbackMultiply;
		}
		if (overrideProjectileData != null)
		{
			projectile2.baseData.SetAll(overrideProjectileData);
		}
		LastShotIndex = m_moduleData[mod].numberShotsFired;
		projectile2.PlayerProjectileSourceGameTimeslice = Time.time;
		if (!IsMinusOneGun)
		{
			ApplyCustomAmmunitionsToProjectile(projectile2);
			if (m_owner is PlayerController)
			{
				playerController.DoPostProcessProjectile(projectile2);
			}
			if (PostProcessProjectile != null)
			{
				PostProcessProjectile(projectile2);
			}
		}
		if (mod.mirror)
		{
			gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, position + Quaternion.Euler(0f, 0f, gunAngle) * mod.InversePositionOffset, Quaternion.Euler(0f, 0f, gunAngle - angleForShot));
			Projectile projectile3 = (LastProjectile = gameObject.GetComponent<Projectile>());
			projectile3.Inverted = true;
			projectile3.Owner = m_owner;
			projectile3.Shooter = m_owner.specRigidbody;
			if ((bool)m_owner.aiShooter)
			{
				projectile3.collidesWithEnemies = m_owner.aiShooter.CanShootOtherEnemies;
			}
			if (rampBullets)
			{
				projectile3.Ramp(rampStartHeight, rampTime);
				TrailController componentInChildren2 = gameObject.GetComponentInChildren<TrailController>();
				if ((bool)componentInChildren2)
				{
					componentInChildren2.rampHeight = true;
					componentInChildren2.rampStartHeight = rampStartHeight;
					componentInChildren2.rampTime = rampTime;
				}
			}
			projectile3.PlayerProjectileSourceGameTimeslice = Time.time;
			if (!IsMinusOneGun)
			{
				ApplyCustomAmmunitionsToProjectile(projectile3);
				if (m_owner is PlayerController)
				{
					playerController.DoPostProcessProjectile(projectile3);
				}
				if (PostProcessProjectile != null)
				{
					PostProcessProjectile(projectile3);
				}
			}
			projectile3.baseData.SetAll(projectile2.baseData);
			projectile3.IsCritical = projectile2.IsCritical;
		}
		if (modifiedFinalVolley != null && mod == modifiedFinalVolley.projectiles[0])
		{
			mod = DefaultModule;
		}
		if (chargeProjectile != null && chargeProjectile.ReflectsIncomingBullets && (bool)barrelOffset)
		{
			if (chargeProjectile.MegaReflection)
			{
				int num2 = PassiveReflectItem.ReflectBulletsInRange(barrelOffset.position.XY(), 2.66f, true, m_owner, 30f, 1.25f, 1.5f, true);
				if (num2 > 0)
				{
					AkSoundEngine.PostEvent("Play_WPN_duelingpistol_impact_01", base.gameObject);
					AkSoundEngine.PostEvent("Play_PET_junk_punch_01", base.gameObject);
				}
			}
			else
			{
				int num3 = PassiveReflectItem.ReflectBulletsInRange(barrelOffset.position.XY(), 2.66f, true, m_owner, 30f, 1f, 1f, true);
				if (num3 > 0)
				{
					AkSoundEngine.PostEvent("Play_WPN_duelingpistol_impact_01", base.gameObject);
					AkSoundEngine.PostEvent("Play_PET_junk_punch_01", base.gameObject);
				}
			}
		}
		IncrementModuleFireCountAndMarkReload(mod, chargeProjectile);
		if (m_owner is PlayerController)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.BULLETS_FIRED, 1f);
			if (projectile != null && projectile.AppliesKnockbackToPlayer)
			{
				playerController.knockbackDoer.ApplyKnockback(-1f * BraveMathCollege.DegreesToVector(gunAngle), projectile.PlayerKnockbackForce);
			}
		}
	}

	public void TriggerActiveCooldown()
	{
		if (UsesRechargeLikeActiveItem)
		{
			RemainingActiveCooldownAmount = ActiveItemStyleRechargeAmount;
		}
	}

	public void ApplyActiveCooldownDamage(PlayerController Owner, float damageDone)
	{
		if (UsesRechargeLikeActiveItem && (!(Owner.CurrentGun == this) || PlayerItem.AllowDamageCooldownOnActive))
		{
			float num = 1f;
			GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
			if (lastLoadedLevelDefinition != null)
			{
				num /= lastLoadedLevelDefinition.enemyHealthMultiplier;
			}
			damageDone *= num;
			if (ModifyActiveCooldownDamage != null)
			{
				damageDone = ModifyActiveCooldownDamage(damageDone);
			}
			RemainingActiveCooldownAmount = Mathf.Max(0f, m_remainingActiveCooldownAmount - damageDone);
		}
	}

	private void TriggerModuleCooldown(ProjectileModule mod)
	{
		if (UsesRechargeLikeActiveItem)
		{
			TriggerActiveCooldown();
		}
		GameManager.Instance.StartCoroutine(HandleModuleCooldown(mod));
	}

	private IEnumerator HandleModuleCooldown(ProjectileModule mod)
	{
		m_moduleData[mod].onCooldown = true;
		float elapsed = 0f;
		float fireMultiplier = ((!(m_owner is PlayerController)) ? 1f : (m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.RateOfFire));
		if (GainsRateOfFireAsContinueAttack)
		{
			float num = RateOfFireMultiplierAdditionPerSecond * m_continuousAttackTime;
			fireMultiplier += num;
		}
		float cooldownTime2 = ((mod.shootStyle != ProjectileModule.ShootStyle.Burst || m_moduleData[mod].numberShotsFiredThisBurst >= mod.burstShotCount) ? (mod.cooldownTime + gunCooldownModifier) : mod.burstCooldownTime);
		cooldownTime2 *= 1f / fireMultiplier;
		while (elapsed < cooldownTime2)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if (m_moduleData != null && m_moduleData.ContainsKey(mod))
		{
			m_moduleData[mod].onCooldown = false;
			m_moduleData[mod].chargeTime = 0f;
			m_moduleData[mod].chargeFired = false;
		}
	}

	private void BeginFiringBeam(ProjectileModule mod)
	{
		GameObject gameObject = SpawnManager.SpawnProjectile(mod.GetCurrentProjectile(m_moduleData[mod], CurrentOwner).gameObject, m_unroundedBarrelPosition, Quaternion.identity);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = CurrentOwner;
		LastProjectile = component;
		BeamController component2 = gameObject.GetComponent<BeamController>();
		component2.Owner = m_owner;
		component2.Gun = this;
		component2.HitsPlayers = m_owner is AIActor;
		component2.HitsEnemies = m_owner is PlayerController;
		if (m_owner is PlayerController)
		{
			PlayerStats stats = (m_owner as PlayerController).stats;
			component.baseData.damage *= stats.GetStatValue(PlayerStats.StatType.Damage);
			component.baseData.speed *= stats.GetStatValue(PlayerStats.StatType.ProjectileSpeed);
			component.baseData.force *= stats.GetStatValue(PlayerStats.StatType.KnockbackMultiplier);
			component.baseData.range *= stats.GetStatValue(PlayerStats.StatType.RangeMultiplier);
			if ((m_owner as PlayerController).inventory.DualWielding)
			{
				component.baseData.damage *= s_DualWieldFactor;
			}
			if (UsesBossDamageModifier)
			{
				if (CustomBossDamageModifier >= 0f)
				{
					component.BossDamageMultiplier = CustomBossDamageModifier;
				}
				else
				{
					component.BossDamageMultiplier = 0.8f;
				}
			}
		}
		if (doesScreenShake && GameManager.Instance.MainCameraController != null)
		{
			GameManager.Instance.MainCameraController.DoContinuousScreenShake(gunScreenShake, this);
		}
		float varianceMultiplier = ((!(m_owner is PlayerController)) ? 1f : (m_owner as PlayerController).stats.GetStatValue(PlayerStats.StatType.Accuracy));
		float angleForShot = mod.GetAngleForShot(m_moduleData[mod].alternateAngleSign, varianceMultiplier);
		Vector3 beamAimDirection = GetBeamAimDirection(angleForShot);
		component2.Direction = beamAimDirection;
		component2.Origin = m_unroundedBarrelPosition;
		ModuleShootData moduleShootData = m_moduleData[mod];
		moduleShootData.beam = component2;
		moduleShootData.angleForShot = angleForShot;
		KnockbackDoer knockbackDoer = m_owner.knockbackDoer;
		moduleShootData.beamKnockbackID = -1;
		if (component2.knocksShooterBack)
		{
			moduleShootData.beamKnockbackID = knockbackDoer.ApplyContinuousKnockback(-beamAimDirection, component2.knockbackStrength);
		}
		m_activeBeams.Add(moduleShootData);
	}

	private void ClearBeams()
	{
		if (m_activeBeams.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_activeBeams.Count; i++)
		{
			BeamController beam = m_activeBeams[i].beam;
			if ((bool)beam && beam.knocksShooterBack)
			{
				m_owner.knockbackDoer.EndContinuousKnockback(m_activeBeams[i].beamKnockbackID);
				m_activeBeams[i].beamKnockbackID = -1;
			}
			if (doesScreenShake && GameManager.Instance.MainCameraController != null)
			{
				GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
			}
			if ((bool)beam)
			{
				beam.CeaseAttack();
			}
		}
		m_activeBeams.Clear();
		if (GameManager.AUDIO_ENABLED)
		{
			AkSoundEngine.PostEvent("Stop_WPN_gun_loop_01", base.gameObject);
		}
		m_isAudioLooping = false;
	}

	public void ForceImmediateReload(bool forceImmediate = false)
	{
		if (base.gameObject.activeSelf)
		{
			ClearBeams();
		}
		if (IsReloading)
		{
			FinishReload(false, false, forceImmediate);
		}
		else if (HaveAmmoToReloadWith())
		{
			FinishReload(false, true, forceImmediate);
		}
	}

	private void OnActiveReloadPressed(PlayerController p, Gun g, bool actualPress)
	{
		if (!m_isReloading && !(reloadTime < 0f))
		{
			return;
		}
		PlayerController playerController = m_owner as PlayerController;
		if (!playerController || (!actualPress && 1 == 0) || (!LocalActiveReload && (!playerController.IsPrimaryPlayer || !ActiveReloadActivated) && (playerController.IsPrimaryPlayer || !ActiveReloadActivatedPlayerTwo)) || !m_canAttemptActiveReload || GameUIRoot.Instance.GetReloadBarForPlayer(m_owner as PlayerController).IsActiveReloadGracePeriod())
		{
			return;
		}
		if (GameUIRoot.Instance.AttemptActiveReload(m_owner as PlayerController))
		{
			OnActiveReloadSuccess();
			GunFormeSynergyProcessor component = GetComponent<GunFormeSynergyProcessor>();
			if ((bool)component)
			{
				component.JustActiveReloaded = true;
			}
			ChamberGunProcessor component2 = GetComponent<ChamberGunProcessor>();
			if ((bool)component2)
			{
				component2.JustActiveReloaded = true;
			}
		}
		m_canAttemptActiveReload = false;
		OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Remove(OnReloadPressed, new Action<PlayerController, Gun, bool>(OnActiveReloadPressed));
	}

	private bool ReloadIsFree()
	{
		if (GoopReloadsFree && m_owner.CurrentGoop != null)
		{
			return true;
		}
		return false;
	}

	public bool Reload()
	{
		if (IsHeroSword && !HeroSwordDoesntBlank && !m_isCurrentlyFiring && !m_anim.IsPlaying(reloadAnimation))
		{
			Vector2 unitCenter = m_owner.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			SilencerInstance.DestroyBulletsInRange(unitCenter, blankReloadRadius, true, false);
			Play(reloadAnimation);
			return false;
		}
		m_continuousAttackTime = 0f;
		ClearBurstState();
		if (m_isReloading || reloadTime < 0f)
		{
			if (m_canAttemptActiveReload)
			{
				OnActiveReloadPressed(m_owner as PlayerController, this, true);
			}
			return false;
		}
		bool flag = false;
		bool flag2 = ReloadIsFree();
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				if (m_moduleData[Volley.projectiles[i]].numberShotsFired != 0)
				{
					flag = true;
					break;
				}
			}
			if (ammo == 0 && !flag2)
			{
				flag = false;
			}
		}
		else
		{
			if (m_moduleData[singleModule].numberShotsFired != 0)
			{
				flag = true;
			}
			if (ClipShotsRemaining == ammo && !flag2)
			{
				flag = false;
			}
		}
		if (flag)
		{
			flag = flag2 || HaveAmmoToReloadWith();
		}
		if (flag2)
		{
			GainAmmo(Mathf.Max(0, ClipCapacity - ClipShotsRemaining));
			DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(m_owner.CenterPosition, 2f);
		}
		if (flag)
		{
			if (!m_isReloading && IsCharging)
			{
				CeaseAttack(false);
			}
			m_isReloading = true;
			m_canAttemptActiveReload = true;
			m_reloadElapsed = 0f;
			m_hasDoneSingleReloadBlankEffect = false;
			OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(OnReloadPressed, new Action<PlayerController, Gun, bool>(OnActiveReloadPressed));
			if (ClipShotsRemaining == 0 && OnAutoReload != null)
			{
				OnAutoReload(CurrentOwner as PlayerController, this);
			}
			if (GameManager.AUDIO_ENABLED)
			{
				AkSoundEngine.PostEvent("Play_WPN_gun_reload_01", base.gameObject);
			}
			if ((bool)reloadOffset)
			{
				float zRotation = ((!m_owner.SpriteFlipped) ? (gunAngle + reloadOffset.transform.localEulerAngles.z) : (gunAngle - 180f - reloadOffset.transform.localEulerAngles.z));
				reloadOffset.localScale = Vector3.one;
				VFXPool vFXPool = ((!IsEmpty || emptyReloadEffects.type == VFXPoolType.None) ? reloadEffects : emptyReloadEffects);
				vFXPool.SpawnAtPosition(reloadOffset.position, zRotation, reloadOffset, Vector2.zero, Vector2.zero, 0.0375f, true);
				if (m_owner.SpriteFlipped)
				{
					reloadOffset.localScale = new Vector3(-1f, 1f, 1f);
				}
			}
			if (m_owner is PlayerController)
			{
				PlayerController playerController = m_owner as PlayerController;
				if (OptionalReloadVolley != null)
				{
					RawFireVolley(OptionalReloadVolley);
					ClearOptionalReloadVolleyCooldownAndReloadData();
				}
				if (playerController.OnReloadedGun != null)
				{
					playerController.OnReloadedGun(playerController, this);
				}
				if ((bool)ObjectToInstantiateOnReload)
				{
					CreateAmp();
				}
				if (AdjustedReloadTime > 0.1f)
				{
					Vector3 offset = new Vector3(0.1f, m_owner.SpriteDimensions.y / 2f + 0.25f, 0f);
					GameUIRoot.Instance.StartPlayerReloadBar(playerController, offset, AdjustedReloadTime);
				}
			}
			if (m_isReloading)
			{
				if (AdjustedReloadTime > 0f)
				{
					StartCoroutine(HandleReload());
				}
				else
				{
					FinishReload(false, false, true);
				}
			}
			m_reloadWhenDoneFiring = false;
			return true;
		}
		return false;
	}

	public void HandleDodgeroll(float fullDodgeTime)
	{
		if (string.IsNullOrEmpty(dodgeAnimation))
		{
			return;
		}
		if (usesDirectionalAnimator)
		{
			AIAnimator aIAnimator = base.aiAnimator;
			string text = dodgeAnimation;
			aIAnimator.PlayUntilFinished(text, false, null, fullDodgeTime);
		}
		else if (m_anim != null)
		{
			tk2dSpriteAnimationClip clipByName = m_anim.GetClipByName(dodgeAnimation);
			if (clipByName != null)
			{
				float overrideFps = (float)clipByName.frames.Length / fullDodgeTime;
				m_anim.Play(clipByName, 0f, overrideFps);
			}
		}
	}

	private void ClearBurstState()
	{
		m_midBurstFire = false;
		m_continueBurstInUpdate = false;
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				if (Volley.projectiles[i].shootStyle == ProjectileModule.ShootStyle.Burst)
				{
					m_moduleData[Volley.projectiles[i]].numberShotsFiredThisBurst = 0;
				}
			}
		}
		else if (singleModule.shootStyle == ProjectileModule.ShootStyle.Burst)
		{
			m_moduleData[singleModule].numberShotsFiredThisBurst = 0;
		}
	}

	private IEnumerator HandleReload()
	{
		m_isReloading = true;
		string currentReloadAnim = ((string.IsNullOrEmpty(emptyReloadAnimation) || !IsEmpty) ? reloadAnimation : emptyReloadAnimation);
		if (IsTrickGun && !m_hasSwappedTrickGunsThisCycle)
		{
			m_hasSwappedTrickGunsThisCycle = true;
			if (!string.IsNullOrEmpty(gunSwitchGroup) && !string.IsNullOrEmpty(alternateSwitchGroup))
			{
				BraveUtility.Swap(ref gunSwitchGroup, ref alternateSwitchGroup);
				AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
			}
			tk2dSpriteAnimationClip clipByName = m_anim.GetClipByName(currentReloadAnim);
			m_defaultSpriteID = clipByName.frames[clipByName.frames.Length - 1].spriteId;
		}
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				m_moduleData[Volley.projectiles[i]].needsReload = true;
			}
		}
		float elapsed = 0f;
		if (m_anim != null)
		{
			PlayIfExists(currentReloadAnim);
		}
		bool hasLaunchedShellCasings = false;
		bool hasLaunchedClip = false;
		while (elapsed < AdjustedReloadTime)
		{
			elapsed += BraveTime.DeltaTime;
			if (shellsToLaunchOnReload > 0 && !hasLaunchedShellCasings && m_anim.IsPlaying(currentReloadAnim) && m_anim.CurrentFrame == reloadShellLaunchFrame)
			{
				for (int j = 0; j < shellsToLaunchOnReload; j++)
				{
					SpawnShellCasingAtPosition(CasingLaunchPoint);
				}
				hasLaunchedShellCasings = true;
			}
			bool animGoForClip = (m_anim.IsPlaying(currentReloadAnim) && m_anim.CurrentFrame == reloadClipLaunchFrame) || m_anim.GetClipByName(currentReloadAnim) == null;
			if (clipsToLaunchOnReload > 0 && !hasLaunchedClip && animGoForClip)
			{
				for (int k = 0; k < clipsToLaunchOnReload; k++)
				{
					SpawnClipAtPosition(ClipLaunchPoint);
				}
				hasLaunchedClip = true;
			}
			if (m_owner is PlayerController)
			{
				HandleSpriteFlip(m_owner.SpriteFlipped);
			}
			if (!m_isReloading)
			{
				break;
			}
			yield return null;
		}
		if (m_isReloading)
		{
			FinishReload();
		}
	}

	public void MoveBulletsIntoClip(int numBullets)
	{
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				int a = Mathf.Min(numBullets, m_moduleData[Volley.projectiles[i]].numberShotsFired);
				int num = Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner) - m_moduleData[Volley.projectiles[i]].numberShotsFired;
				a = Mathf.Min(a, ammo - num);
				if (a > 0)
				{
					m_moduleData[Volley.projectiles[i]].numberShotsFired -= a;
					m_moduleData[Volley.projectiles[i]].needsReload = false;
				}
			}
		}
		else
		{
			int a2 = Mathf.Min(numBullets, m_moduleData[singleModule].numberShotsFired);
			int num2 = singleModule.GetModNumberOfShotsInClip(CurrentOwner) - m_moduleData[singleModule].numberShotsFired;
			a2 = Mathf.Min(a2, ammo - num2);
			if (a2 > 0)
			{
				m_moduleData[singleModule].numberShotsFired -= a2;
				m_moduleData[singleModule].needsReload = false;
			}
		}
	}

	private void FinishReload(bool activeReload = false, bool silent = false, bool isImmediate = false)
	{
		if (isImmediate)
		{
			string text = ((string.IsNullOrEmpty(emptyReloadAnimation) || !IsEmpty) ? reloadAnimation : emptyReloadAnimation);
			if (IsTrickGun && !m_hasSwappedTrickGunsThisCycle)
			{
				m_hasSwappedTrickGunsThisCycle = true;
				if (!string.IsNullOrEmpty(gunSwitchGroup) && !string.IsNullOrEmpty(alternateSwitchGroup))
				{
					BraveUtility.Swap(ref gunSwitchGroup, ref alternateSwitchGroup);
					AkSoundEngine.SetSwitch("WPN_Guns", gunSwitchGroup, base.gameObject);
				}
				tk2dSpriteAnimationClip clipByName = m_anim.GetClipByName(text);
				m_defaultSpriteID = clipByName.frames[clipByName.frames.Length - 1].spriteId;
			}
		}
		if (!silent)
		{
			if (IsTrickGun)
			{
				BraveUtility.Swap(ref reloadAnimation, ref alternateReloadAnimation);
				BraveUtility.Swap(ref shootAnimation, ref alternateShootAnimation);
				if (!string.IsNullOrEmpty(alternateIdleAnimation))
				{
					BraveUtility.Swap(ref idleAnimation, ref alternateIdleAnimation);
				}
				BraveUtility.Swap(ref rawVolley, ref alternateVolley);
				(CurrentOwner as PlayerController).stats.RecalculateStats(CurrentOwner as PlayerController);
			}
			if (IsTrickGun && TrickGunAlternatesHandedness)
			{
				if (Handedness == GunHandedness.OneHanded)
				{
					m_cachedGunHandedness = GunHandedness.TwoHanded;
					carryPixelOffset = new IntVector2(10, 0);
				}
				else if (Handedness == GunHandedness.TwoHanded)
				{
					m_cachedGunHandedness = GunHandedness.OneHanded;
					carryPixelOffset = new IntVector2(0, 0);
				}
				(m_owner as PlayerController).ProcessHandAttachment();
			}
		}
		m_hasSwappedTrickGunsThisCycle = false;
		HasFiredHolsterShot = false;
		HasFiredReloadSynergy = false;
		OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Remove(OnReloadPressed, new Action<PlayerController, Gun, bool>(OnActiveReloadPressed));
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				int num = Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner) - m_moduleData[Volley.projectiles[i]].numberShotsFired;
				int numberShotsFired = Math.Max(Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner) - ammo, 0);
				m_moduleData[Volley.projectiles[i]].numberShotsFired = numberShotsFired;
				m_moduleData[Volley.projectiles[i]].needsReload = false;
				m_moduleData[Volley.projectiles[i]].activeReloadDamageModifier = 1f;
				int numberShotsActiveReload = Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner) - num;
				if (activeReload)
				{
					m_moduleData[Volley.projectiles[i]].numberShotsActiveReload = numberShotsActiveReload;
				}
			}
		}
		else
		{
			int num2 = singleModule.GetModNumberOfShotsInClip(CurrentOwner) - m_moduleData[singleModule].numberShotsFired;
			int numberShotsFired2 = Math.Max(singleModule.GetModNumberOfShotsInClip(CurrentOwner) - ammo, 0);
			m_moduleData[singleModule].numberShotsFired = numberShotsFired2;
			m_moduleData[singleModule].needsReload = false;
			m_moduleData[singleModule].activeReloadDamageModifier = 1f;
			int numberShotsActiveReload2 = singleModule.GetModNumberOfShotsInClip(CurrentOwner) - num2;
			if (activeReload)
			{
				m_moduleData[singleModule].numberShotsActiveReload = numberShotsActiveReload2;
			}
		}
		if (!silent)
		{
			PlayIdleAnimation();
			SequentialActiveReloads = (activeReload ? (SequentialActiveReloads + 1) : 0);
			if (LocalActiveReload && activeReloadData.ActiveReloadStacks)
			{
				if (activeReload)
				{
					if (activeReloadData.ActiveReloadIncrementsTier)
					{
						CurrentStrengthTier = Mathf.Min(CurrentStrengthTier + 1, activeReloadData.MaxTier - 1);
					}
					AdditionalReloadMultiplier /= activeReloadData.reloadSpeedMultiplier;
				}
				else
				{
					if (activeReloadData.ActiveReloadIncrementsTier)
					{
						CurrentStrengthTier = 0;
					}
					AdditionalReloadMultiplier = 1f;
				}
			}
			HandleActiveReloadEffects(activeReload);
		}
		m_isReloading = false;
	}

	private void HandleActiveReloadEffects(bool activeReload)
	{
		if (!CurrentOwner || !(CurrentOwner.CurrentGun == this))
		{
			return;
		}
		VFXPool vFXPool = null;
		if (activeReload)
		{
			if (activeReloadSuccessEffects.type != 0)
			{
				vFXPool = activeReloadSuccessEffects;
			}
		}
		else if (activeReloadFailedEffects.type != 0)
		{
			vFXPool = activeReloadFailedEffects;
		}
		if ((bool)CurrentOwner && vFXPool != null)
		{
			vFXPool.SpawnAtPosition(CurrentOwner.CenterPosition + new Vector2(0f, 2f), 0f, CurrentOwner.transform, Vector2.zero, Vector2.zero, 5f, true);
		}
	}

	private void PotentialShuffleAmmoForLargeClipGuns()
	{
		bool flag = false;
		int num = 0;
		if (Volley != null && Volley.projectiles.Count > 1)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				if (!Volley.projectiles[i].ignoredForReloadPurposes && Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner) > 100)
				{
					num++;
					flag = true;
				}
			}
		}
		if (num < 2)
		{
			flag = false;
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < Volley.projectiles.Count; j++)
		{
			if (Volley.projectiles[j].GetModNumberOfShotsInClip(CurrentOwner) > 100)
			{
				int num2 = Volley.projectiles[j].GetModNumberOfShotsInClip(CurrentOwner) - 100;
				if (m_moduleData.ContainsKey(Volley.projectiles[j]) && num2 > m_moduleData[Volley.projectiles[j]].numberShotsFired)
				{
					m_moduleData[Volley.projectiles[j]].numberShotsFired = num2;
				}
			}
		}
	}

	private bool HaveAmmoToReloadWith()
	{
		PotentialShuffleAmmoForLargeClipGuns();
		if (CanReloadNoMatterAmmo)
		{
			return true;
		}
		if (Volley != null)
		{
			for (int i = 0; i < Volley.projectiles.Count; i++)
			{
				if (!Volley.projectiles[i].ignoredForReloadPurposes && !Volley.projectiles[i].IsDuctTapeModule && Volley.projectiles[i].GetModNumberOfShotsInClip(CurrentOwner) - m_moduleData[Volley.projectiles[i]].numberShotsFired >= ammo)
				{
					return false;
				}
			}
		}
		else if (singleModule.GetModifiedNumberOfFinalProjectiles(CurrentOwner) - m_moduleData[singleModule].numberShotsFired >= ammo)
		{
			return false;
		}
		return true;
	}

	private Vector3 GetBeamAimDirection(float angleForShot)
	{
		Vector3 vector = Quaternion.Euler(0f, 0f, gunAngle) * Vector3.right;
		return Quaternion.Euler(0f, 0f, angleForShot) * vector;
	}

	public void PlayIdleAnimation()
	{
		if (m_preventIdleLoop)
		{
			return;
		}
		m_preventIdleLoop = true;
		if (!string.IsNullOrEmpty(outOfAmmoAnimation) && ammo == 0)
		{
			Play(outOfAmmoAnimation);
		}
		else if (!string.IsNullOrEmpty(emptyAnimation) && ClipShotsRemaining <= 0)
		{
			Play(emptyAnimation);
		}
		else
		{
			if (m_anim == null)
			{
				m_anim = GetComponent<tk2dSpriteAnimator>();
			}
			if (usesDirectionalIdleAnimations)
			{
				if (m_directionalIdleNames == null)
				{
					m_directionalIdleNames = new string[8]
					{
						idleAnimation + "_E",
						idleAnimation + "_SE",
						idleAnimation + "_S",
						idleAnimation + "_SW",
						idleAnimation + "_W",
						idleAnimation + "_NW",
						idleAnimation + "_N",
						idleAnimation + "_NE"
					};
				}
				float num = gunAngle;
				if (CurrentOwner is PlayerController)
				{
					PlayerController playerController = CurrentOwner as PlayerController;
					num = BraveMathCollege.Atan2Degrees(playerController.unadjustedAimPoint.XY() - m_attachTransform.position.XY());
				}
				int num2 = BraveMathCollege.AngleToOctant(num + 90f);
				if (!m_anim.IsPlaying(m_directionalIdleNames[num2]))
				{
					Play(m_directionalIdleNames[num2]);
				}
			}
			else if (!string.IsNullOrEmpty(idleAnimation) && m_anim.GetClipByName(idleAnimation) != null)
			{
				Play(idleAnimation);
			}
			else
			{
				m_anim.Stop();
				m_sprite.spriteId = m_defaultSpriteID;
			}
		}
		m_preventIdleLoop = false;
	}

	private void DecrementAmmoCost(ProjectileModule module)
	{
		if (modifiedFinalVolley != null && module == modifiedFinalVolley.projectiles[0])
		{
			module = DefaultModule;
		}
		int num = module.ammoCost;
		if (module.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			ProjectileModule.ChargeProjectile chargeProjectile = module.GetChargeProjectile(m_moduleData[module].chargeTime);
			if (chargeProjectile.UsesAmmo)
			{
				num = chargeProjectile.AmmoCost;
			}
		}
		if (InfiniteAmmo)
		{
			num = 0;
		}
		if (RequiresFundsToShoot && !m_hasDecrementedFunds)
		{
			m_hasDecrementedFunds = true;
			(m_owner as PlayerController).carriedConsumables.Currency -= CurrencyCostPerShot;
		}
		ammo = Math.Max(0, ammo - num);
		DecrementCustomAmmunitions(num);
	}

	private void Play(string animName)
	{
		if (!OverrideAnimations)
		{
			if (usesDirectionalAnimator)
			{
				base.aiAnimator.PlayUntilFinished(animName);
			}
			else
			{
				m_anim.Play(animName);
			}
		}
	}

	private void PlayIfExists(string name, bool restartIfPlaying = false)
	{
		if (OverrideAnimations)
		{
			return;
		}
		if (usesDirectionalAnimator && base.aiAnimator.HasDirectionalAnimation(name))
		{
			base.aiAnimator.PlayUntilFinished(name);
			return;
		}
		tk2dSpriteAnimationClip clipByName = m_anim.GetClipByName(name);
		if (clipByName != null)
		{
			if (restartIfPlaying && m_anim.IsPlaying(name))
			{
				m_anim.PlayFromFrame(0);
			}
			else
			{
				m_anim.Play(clipByName);
			}
		}
	}

	private void PlayIfExistsAndNotPlaying(string name)
	{
		if (OverrideAnimations)
		{
			return;
		}
		if (usesDirectionalAnimator && base.aiAnimator.HasDirectionalAnimation(name) && !base.aiAnimator.IsPlaying(name))
		{
			base.aiAnimator.PlayUntilFinished(name);
			return;
		}
		tk2dSpriteAnimationClip clipByName = m_anim.GetClipByName(name);
		if (clipByName != null && !m_anim.IsPlaying(clipByName))
		{
			m_anim.Play(clipByName);
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		if (!base.gameObject.activeSelf)
		{
			return 10000f;
		}
		if (CurrentOwner != null)
		{
			return 10000f;
		}
		if (IsBeingSold)
		{
			return 1000f;
		}
		if (!m_sprite)
		{
			return 1000f;
		}
		if (m_isThrown)
		{
			if (!m_thrownOnGround)
			{
				return 1000f;
			}
			if (m_transform != null && m_transform.parent != null && m_transform.parent.GetComponent<Projectile>() != null)
			{
				return 1000f;
			}
		}
		Bounds bounds = m_sprite.GetBounds();
		Vector2 vector = base.transform.position.XY() + (base.transform.rotation * bounds.min).XY();
		Vector2 vector2 = vector + (base.transform.rotation * bounds.size).XY();
		return BraveMathCollege.DistToRectangle(point, vector, vector2 - vector);
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this && RoomHandler.unassignedInteractableObjects.Contains(this))
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(m_sprite);
			SpriteOutlineManager.AddOutlineToSprite(m_sprite, Color.white, 0.1f);
			m_sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(m_sprite, true);
			SpriteOutlineManager.AddOutlineToSprite(m_sprite, Color.black, 0.1f, 0.05f);
			m_sprite.UpdateZDepth();
		}
	}

	public void Interact(PlayerController interactor)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (GameStatsManager.HasInstance && GameStatsManager.Instance.IsRainbowRun)
		{
			if ((bool)interactor && interactor.CurrentRoom != null && interactor.CurrentRoom == GameManager.Instance.Dungeon.data.Entrance && Time.frameCount == PickupObject.s_lastRainbowPickupFrame)
			{
				return;
			}
			PickupObject.s_lastRainbowPickupFrame = Time.frameCount;
		}
		SpriteOutlineManager.RemoveOutlineFromSprite(m_sprite, true);
		Pickup(interactor);
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	public void TriggerTemporaryBoost(float damageMultiplier, float scaleMultiplier, float duration, bool oneShot)
	{
		StartCoroutine(HandleTemporaryBoost(damageMultiplier, scaleMultiplier, duration, oneShot));
	}

	private IEnumerator HandleTemporaryBoost(float damageMultiplier, float scaleMultiplier, float duration, bool oneShot)
	{
		float startTime = Time.time;
		int numberFired = 0;
		Action<Projectile> processTemporaryBoost = delegate(Projectile p)
		{
			p.AdditionalScaleMultiplier *= scaleMultiplier;
			p.baseData.damage *= damageMultiplier;
			numberFired++;
		};
		Gun gun = this;
		gun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun.PostProcessProjectile, processTemporaryBoost);
		while (Time.time - startTime < duration && (!oneShot || numberFired <= 0))
		{
			yield return null;
		}
		Gun gun2 = this;
		gun2.PostProcessProjectile = (Action<Projectile>)Delegate.Remove(gun2.PostProcessProjectile, processTemporaryBoost);
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		int num = 0;
		if (UsesRechargeLikeActiveItem)
		{
			num++;
			data.Add(m_remainingActiveCooldownAmount);
		}
		IGunInheritable[] interfaces = base.gameObject.GetInterfaces<IGunInheritable>();
		for (int i = 0; i < interfaces.Length; i++)
		{
			interfaces[i].MidGameSerialize(data, i + num);
		}
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		int dataIndex = 0;
		if (UsesRechargeLikeActiveItem)
		{
			m_remainingActiveCooldownAmount = (float)data[dataIndex];
			dataIndex++;
		}
		IGunInheritable[] interfaces = base.gameObject.GetInterfaces<IGunInheritable>();
		for (int i = 0; i < interfaces.Length; i++)
		{
			interfaces[i].MidGameDeserialize(data, ref dataIndex);
		}
	}

	public void CopyStateFrom(Gun other)
	{
		if ((bool)other && other.UsesRechargeLikeActiveItem)
		{
			m_remainingActiveCooldownAmount = other.m_remainingActiveCooldownAmount;
		}
	}

	public void AddAdditionalFlipTransform(Transform t)
	{
		if (m_childTransformsToFlip == null)
		{
			m_childTransformsToFlip = new List<Transform>();
		}
		m_childTransformsToFlip.Add(t);
	}
}
