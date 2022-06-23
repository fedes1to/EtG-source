using System;

[Serializable]
public class PlayerItemProjectileInterface
{
	public bool UseCurrentGunProjectile = true;

	public Projectile SpecifiedProjectile;

	public Projectile GetProjectile(PlayerController targetPlayer)
	{
		if (UseCurrentGunProjectile && (bool)targetPlayer.CurrentGun)
		{
			Projectile currentProjectile = targetPlayer.CurrentGun.DefaultModule.GetCurrentProjectile();
			if ((bool)currentProjectile)
			{
				return currentProjectile;
			}
		}
		return SpecifiedProjectile;
	}
}
