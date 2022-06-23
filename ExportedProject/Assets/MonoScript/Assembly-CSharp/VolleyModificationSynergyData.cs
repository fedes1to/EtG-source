using System;

[Serializable]
public class VolleyModificationSynergyData
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool AddsChargeProjectile;

	[ShowInInspectorIf("AddsChargeProjectile", false)]
	public ProjectileModule.ChargeProjectile ChargeProjectileToAdd;

	public bool AddsModules;

	public ProjectileModule[] ModulesToAdd;

	public bool AddsDuplicatesOfBaseModule;

	[ShowInInspectorIf("AddsDuplicatesOfBaseModule", false)]
	public int DuplicatesOfBaseModule;

	[ShowInInspectorIf("AddsDuplicatesOfBaseModule", false)]
	public float BaseModuleDuplicateAngle = 10f;

	public bool ReplacesSourceProjectile;

	[ShowInInspectorIf("ReplacesSourceProjectile", false)]
	public float ReplacementChance = 1f;

	[ShowInInspectorIf("ReplacesSourceProjectile", false)]
	public bool OnlyReplacesAdditionalProjectiles;

	[ShowInInspectorIf("ReplacesSourceProjectile", false)]
	public Projectile ReplacementProjectile;

	[ShowInInspectorIf("ReplacesSourceProjectile", false)]
	public bool UsesMultipleReplacementProjectiles;

	[ShowInInspectorIf("UsesMultipleReplacementProjectiles", false)]
	public bool MultipleReplacementsSequential;

	public Projectile[] MultipleReplacementProjectiles;

	[ShowInInspectorIf("ReplacesSourceProjectile", false)]
	public bool ReplacementSkipsChargedShots;

	public bool SetsNumberFinalProjectiles;

	[ShowInInspectorIf("SetsNumberFinalProjectiles", false)]
	public int NumberFinalProjectiles = 1;

	[ShowInInspectorIf("SetsNumberFinalProjectiles", false)]
	public bool AddsNewFinalProjectile;

	[ShowInInspectorIf("AddsNewFinalProjectile", false)]
	public Projectile NewFinalProjectile;

	[ShowInInspectorIf("AddsNewFinalProjectile", false)]
	public string NewFinalProjectileAmmoType;

	public bool SetsBurstCount;

	[ShowInInspectorIf("SetsBurstCount", false)]
	public bool MakesDefaultModuleBurst;

	[ShowInInspectorIf("SetsBurstCount", false)]
	public float BurstMultiplier = 1f;

	[ShowInInspectorIf("SetsBurstCount", false)]
	public int BurstShift;

	public bool AddsPossibleProjectileToPrimaryModule;

	[ShowInInspectorIf("AddsPossibleProjectileToPrimaryModule", false)]
	public Projectile AdditionalModuleProjectile;

	[NonSerialized]
	public int multipleSequentialReplacementIndex;
}
