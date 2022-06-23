public class ModuleShootData
{
	public bool onCooldown;

	public bool needsReload;

	public int numberShotsFired;

	public int numberShotsFiredThisBurst;

	public int numberShotsActiveReload;

	public float chargeTime;

	public bool chargeFired;

	public ProjectileModule.ChargeProjectile lastChargeProjectile;

	public float activeReloadDamageModifier = 1f;

	public float alternateAngleSign = 1f;

	public BeamController beam;

	public int beamKnockbackID;

	public float angleForShot;
}
