using System;

[Serializable]
public struct PlayerOrbitalSynergyData
{
	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public bool EngagesFiring;

	public float ShootCooldownMultiplier;

	public int AdditionalShots;

	public Projectile OverrideProjectile;

	public bool HasOverrideAnimations;

	public string OverrideIdleAnimation;
}
