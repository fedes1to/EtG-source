using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProjectileModule
{
	public enum ShootStyle
	{
		SemiAutomatic,
		Automatic,
		Beam,
		Charged,
		Burst
	}

	public enum ProjectileSequenceStyle
	{
		Random,
		Ordered,
		OrderedGroups
	}

	[Serializable]
	public class ChargeProjectile
	{
		public float ChargeTime;

		public Projectile Projectile;

		public ChargeProjectileProperties UsedProperties;

		public int AmmoCost;

		public VFXPool VfxPool;

		public float LightIntensity;

		public ScreenShakeSettings ScreenShake;

		public string OverrideShootAnimation;

		public VFXPool OverrideMuzzleFlashVfxPool;

		public bool MegaReflection;

		public string AdditionalWwiseEvent;

		[NonSerialized]
		public ChargeProjectile previousChargeProjectile;

		public bool UsesOverrideShootAnimation
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.shootAnim) == ChargeProjectileProperties.shootAnim;
			}
		}

		public bool UsesOverrideMuzzleFlashVfxPool
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.muzzleFlash) == ChargeProjectileProperties.muzzleFlash;
			}
		}

		public bool DepleteAmmo
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.depleteAmmo) == ChargeProjectileProperties.depleteAmmo;
			}
		}

		public bool UsesAmmo
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.ammo) == ChargeProjectileProperties.ammo;
			}
		}

		public bool UsesVfx
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.vfx) == ChargeProjectileProperties.vfx;
			}
		}

		public bool UsesLightIntensity
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.lightIntensity) == ChargeProjectileProperties.lightIntensity;
			}
		}

		public bool UsesScreenShake
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.screenShake) == ChargeProjectileProperties.screenShake;
			}
		}

		public bool ReflectsIncomingBullets
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.reflectBullets) == ChargeProjectileProperties.reflectBullets;
			}
		}

		public bool DelayedVFXDestruction
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.delayedVFXClear) == ChargeProjectileProperties.delayedVFXClear;
			}
		}

		public bool ShouldDoChargePoof
		{
			get
			{
				return (bool)Projectile && ChargeTime > 0f && (UsedProperties & ChargeProjectileProperties.disableChargePoof) != ChargeProjectileProperties.disableChargePoof;
			}
		}

		public bool UsesAdditionalWwiseEvent
		{
			get
			{
				return (UsedProperties & ChargeProjectileProperties.additionalWwiseEvent) == ChargeProjectileProperties.additionalWwiseEvent;
			}
		}
	}

	[Flags]
	public enum ChargeProjectileProperties
	{
		ammo = 1,
		vfx = 2,
		lightIntensity = 4,
		screenShake = 8,
		shootAnim = 0x10,
		muzzleFlash = 0x20,
		depleteAmmo = 0x40,
		delayedVFXClear = 0x80,
		disableChargePoof = 0x100,
		reflectBullets = 0x200,
		additionalWwiseEvent = 0x400
	}

	public ShootStyle shootStyle;

	public GameUIAmmoType.AmmoType ammoType;

	public string customAmmoType;

	public List<Projectile> projectiles = new List<Projectile>();

	public ProjectileSequenceStyle sequenceStyle;

	public List<int> orderedGroupCounts;

	public List<ChargeProjectile> chargeProjectiles = new List<ChargeProjectile>();

	public float maxChargeTime;

	public bool triggerCooldownForAnyChargeAmount;

	public bool isFinalVolley;

	public bool usesOptionalFinalProjectile;

	public Projectile finalProjectile;

	public ProjectileVolleyData finalVolley;

	public int numberOfFinalProjectiles = 1;

	public GameUIAmmoType.AmmoType finalAmmoType;

	public string finalCustomAmmoType;

	public float angleFromAim;

	public bool alternateAngle;

	public float angleVariance;

	public Vector3 positionOffset;

	public bool mirror;

	public bool inverted;

	public int ammoCost = 1;

	public int burstShotCount = 3;

	public float burstCooldownTime = 0.2f;

	public float cooldownTime = 1f;

	public int numberOfShotsInClip = -1;

	public bool ignoredForReloadPurposes;

	public bool preventFiringDuringCharge;

	[NonSerialized]
	public bool isExternalAddedModule;

	private int m_cloneSourceIndex = -1;

	[NonSerialized]
	public string runtimeGuid;

	[NonSerialized]
	public bool IsDuctTapeModule;

	private int currentOrderedProjNumber;

	private int currentOrderedGroupNumber;

	private static int m_angleVarianceIterator;

	public int CloneSourceIndex
	{
		get
		{
			return m_cloneSourceIndex;
		}
		set
		{
			m_cloneSourceIndex = value;
		}
	}

	public Vector3 InversePositionOffset
	{
		get
		{
			return new Vector3(positionOffset.x, -1f * positionOffset.y, positionOffset.z);
		}
	}

	public float LongestChargeTime
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < chargeProjectiles.Count; i++)
			{
				ChargeProjectile chargeProjectile = chargeProjectiles[i];
				num = Mathf.Max(num, chargeProjectile.ChargeTime);
			}
			return num;
		}
	}

	public int GetModifiedNumberOfFinalProjectiles(GameActor owner)
	{
		if ((bool)owner && owner is PlayerController && numberOfFinalProjectiles > 0 && (owner as PlayerController).OnlyFinalProjectiles.Value)
		{
			return GetModNumberOfShotsInClip(owner);
		}
		return numberOfFinalProjectiles;
	}

	public int GetModNumberOfShotsInClip(GameActor owner)
	{
		if (numberOfShotsInClip == 1)
		{
			return numberOfShotsInClip;
		}
		if (owner != null && owner is PlayerController)
		{
			PlayerController playerController = owner as PlayerController;
			float statValue = playerController.stats.GetStatValue(PlayerStats.StatType.AdditionalClipCapacityMultiplier);
			float statValue2 = playerController.stats.GetStatValue(PlayerStats.StatType.TarnisherClipCapacityMultiplier);
			int num = Mathf.FloorToInt((float)numberOfShotsInClip * statValue * statValue2);
			if (num < 0)
			{
				return num;
			}
			return Mathf.Max(num, 1);
		}
		return numberOfShotsInClip;
	}

	public static ProjectileModule CreateClone(ProjectileModule source, bool inheritGuid = true, int sourceIndex = -1)
	{
		ProjectileModule projectileModule = new ProjectileModule();
		projectileModule.shootStyle = source.shootStyle;
		projectileModule.ammoType = source.ammoType;
		projectileModule.customAmmoType = source.customAmmoType;
		projectileModule.sequenceStyle = source.sequenceStyle;
		projectileModule.maxChargeTime = source.maxChargeTime;
		projectileModule.triggerCooldownForAnyChargeAmount = source.triggerCooldownForAnyChargeAmount;
		projectileModule.angleFromAim = source.angleFromAim;
		projectileModule.alternateAngle = source.alternateAngle;
		projectileModule.angleVariance = source.angleVariance;
		projectileModule.mirror = source.mirror;
		projectileModule.inverted = source.inverted;
		projectileModule.positionOffset = source.positionOffset;
		projectileModule.ammoCost = source.ammoCost;
		projectileModule.cooldownTime = source.cooldownTime;
		projectileModule.numberOfShotsInClip = source.numberOfShotsInClip;
		projectileModule.usesOptionalFinalProjectile = source.usesOptionalFinalProjectile;
		projectileModule.finalAmmoType = source.finalAmmoType;
		projectileModule.finalCustomAmmoType = source.finalCustomAmmoType;
		projectileModule.numberOfFinalProjectiles = source.numberOfFinalProjectiles;
		projectileModule.isFinalVolley = source.isFinalVolley;
		projectileModule.burstCooldownTime = source.burstCooldownTime;
		projectileModule.burstShotCount = source.burstShotCount;
		projectileModule.ignoredForReloadPurposes = source.ignoredForReloadPurposes;
		projectileModule.preventFiringDuringCharge = source.preventFiringDuringCharge;
		projectileModule.isExternalAddedModule = source.isExternalAddedModule;
		projectileModule.IsDuctTapeModule = source.IsDuctTapeModule;
		projectileModule.projectiles = new List<Projectile>();
		for (int i = 0; i < source.projectiles.Count; i++)
		{
			projectileModule.projectiles.Add(source.projectiles[i]);
		}
		projectileModule.chargeProjectiles = source.chargeProjectiles;
		projectileModule.finalProjectile = source.finalProjectile;
		projectileModule.finalVolley = source.finalVolley;
		projectileModule.orderedGroupCounts = source.orderedGroupCounts;
		if (sourceIndex >= 0)
		{
			projectileModule.CloneSourceIndex = sourceIndex;
		}
		if (inheritGuid && source.runtimeGuid != null)
		{
			projectileModule.runtimeGuid = source.runtimeGuid;
		}
		else
		{
			projectileModule.runtimeGuid = Guid.NewGuid().ToString();
		}
		return projectileModule;
	}

	public void ClearOrderedProjectileData()
	{
		currentOrderedGroupNumber = 0;
		currentOrderedProjNumber = 0;
	}

	public void ResetRuntimeData()
	{
		currentOrderedProjNumber = 0;
		currentOrderedGroupNumber = 0;
		if (string.IsNullOrEmpty(runtimeGuid))
		{
			runtimeGuid = Guid.NewGuid().ToString();
		}
	}

	public bool IsFinalShot(ModuleShootData runtimeData, GameActor owner)
	{
		if (runtimeData.needsReload)
		{
			return false;
		}
		if (isFinalVolley)
		{
			return true;
		}
		return usesOptionalFinalProjectile && GetModNumberOfShotsInClip(owner) - GetModifiedNumberOfFinalProjectiles(owner) <= runtimeData.numberShotsFired;
	}

	public bool HasFinalVolleyOverride()
	{
		return usesOptionalFinalProjectile && finalVolley != null;
	}

	public Projectile GetCurrentProjectile(ModuleShootData runtimeData, GameActor owner)
	{
		if (usesOptionalFinalProjectile && GetModNumberOfShotsInClip(owner) - GetModifiedNumberOfFinalProjectiles(owner) <= runtimeData.numberShotsFired)
		{
			return finalProjectile;
		}
		if (sequenceStyle == ProjectileSequenceStyle.Ordered)
		{
			return projectiles[currentOrderedProjNumber];
		}
		if (sequenceStyle == ProjectileSequenceStyle.OrderedGroups)
		{
			int num = 0;
			for (int i = 0; i < currentOrderedGroupNumber; i++)
			{
				num += orderedGroupCounts[i];
			}
			int index = UnityEngine.Random.Range(num, num + orderedGroupCounts[currentOrderedGroupNumber]);
			currentOrderedGroupNumber = (currentOrderedGroupNumber + 1) % orderedGroupCounts.Count;
			return projectiles[index];
		}
		return projectiles[UnityEngine.Random.Range(0, projectiles.Count)];
	}

	public Projectile GetCurrentProjectile()
	{
		if (shootStyle == ShootStyle.Charged)
		{
			for (int i = 0; i < chargeProjectiles.Count; i++)
			{
				if ((bool)chargeProjectiles[i].Projectile)
				{
					Projectile projectile = chargeProjectiles[i].Projectile;
					projectile.pierceMinorBreakables = true;
					return projectile;
				}
			}
			return null;
		}
		if (sequenceStyle == ProjectileSequenceStyle.Ordered)
		{
			return projectiles[currentOrderedProjNumber];
		}
		if (sequenceStyle == ProjectileSequenceStyle.OrderedGroups)
		{
			int num = 0;
			for (int j = 0; j < currentOrderedGroupNumber; j++)
			{
				num += orderedGroupCounts[j];
			}
			int index = UnityEngine.Random.Range(num, orderedGroupCounts[currentOrderedGroupNumber]);
			currentOrderedGroupNumber = (currentOrderedGroupNumber + 1) % orderedGroupCounts.Count;
			return projectiles[index];
		}
		return projectiles[UnityEngine.Random.Range(0, projectiles.Count)];
	}

	public float GetEstimatedShotsPerSecond(float reloadTime)
	{
		if (cooldownTime <= 0f)
		{
			return 0f;
		}
		float num = cooldownTime;
		if (shootStyle == ShootStyle.Burst && burstShotCount > 1 && burstCooldownTime > 0f)
		{
			num = ((float)(burstShotCount - 1) * burstCooldownTime + cooldownTime) / (float)burstShotCount;
		}
		if (numberOfShotsInClip > 0)
		{
			num += reloadTime / (float)numberOfShotsInClip;
		}
		return 1f / num;
	}

	public void IncrementShootCount()
	{
		currentOrderedProjNumber = (currentOrderedProjNumber + 1) % projectiles.Count;
	}

	public float GetAngleVariance(float varianceMultiplier = 1f)
	{
		float num = BraveMathCollege.GetLowDiscrepancyRandom(m_angleVarianceIterator) * (2f * angleVariance) - angleVariance;
		m_angleVarianceIterator++;
		return num * varianceMultiplier;
	}

	public float GetAngleVariance(float customVariance, float varianceMultiplier)
	{
		float num = BraveMathCollege.GetLowDiscrepancyRandom(m_angleVarianceIterator) * (2f * customVariance) - customVariance;
		m_angleVarianceIterator++;
		return num * varianceMultiplier;
	}

	public float GetAngleForShot(float alternateAngleSign = 1f, float varianceMultiplier = 1f, float? overrideAngleVariance = null)
	{
		float num = alternateAngleSign * angleFromAim;
		float num2 = ((!overrideAngleVariance.HasValue) ? GetAngleVariance(varianceMultiplier) : overrideAngleVariance.Value);
		return num + num2;
	}

	public int ContainsFinalProjectile(Projectile testProj)
	{
		if (usesOptionalFinalProjectile)
		{
			if (finalVolley != null)
			{
				for (int i = 0; i < finalVolley.projectiles.Count; i++)
				{
					if (finalVolley.projectiles[i].projectiles.Contains(testProj))
					{
						return numberOfFinalProjectiles;
					}
				}
			}
			else if (finalProjectile == testProj)
			{
				return numberOfFinalProjectiles;
			}
		}
		return 0;
	}

	public ChargeProjectile GetChargeProjectile(float chargeTime)
	{
		ChargeProjectile chargeProjectile = null;
		for (int i = 0; i < chargeProjectiles.Count; i++)
		{
			ChargeProjectile chargeProjectile2 = chargeProjectiles[i];
			if (chargeProjectile2.ChargeTime <= chargeTime && (chargeProjectile == null || chargeTime - chargeProjectile2.ChargeTime < chargeTime - chargeProjectile.ChargeTime))
			{
				chargeProjectile = chargeProjectile2;
			}
		}
		return chargeProjectile;
	}
}
