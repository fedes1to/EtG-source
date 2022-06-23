using UnityEngine;

public static class VolleyUtility
{
	public static void FireVolley(ProjectileVolleyData sourceVolley, Vector2 shootPoint, Vector2 aimDirection, GameActor possibleOwner = null, bool treatedAsNonProjectileForChallenge = false)
	{
		for (int i = 0; i < sourceVolley.projectiles.Count; i++)
		{
			ProjectileModule mod = sourceVolley.projectiles[i];
			ShootSingleProjectile(mod, sourceVolley, shootPoint, BraveMathCollege.Atan2Degrees(aimDirection), 0f, possibleOwner, treatedAsNonProjectileForChallenge);
		}
	}

	public static void ShootSingleProjectile(ProjectileModule mod, ProjectileVolleyData volley, Vector2 shootPoint, float fireAngle, float chargeTime, GameActor possibleOwner = null, bool treatedAsNonProjectileForChallenge = false)
	{
		Projectile projectile = null;
		ProjectileModule.ChargeProjectile chargeProjectile = null;
		if (mod.shootStyle == ProjectileModule.ShootStyle.Charged)
		{
			chargeProjectile = mod.GetChargeProjectile(chargeTime);
			if (chargeProjectile != null)
			{
				projectile = chargeProjectile.Projectile;
				projectile.pierceMinorBreakables = true;
			}
		}
		else
		{
			projectile = mod.GetCurrentProjectile();
		}
		if (!projectile)
		{
			if (mod.shootStyle != ProjectileModule.ShootStyle.Charged)
			{
				mod.IncrementShootCount();
			}
			return;
		}
		float angleForShot = mod.GetAngleForShot();
		GameObject gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, shootPoint.ToVector3ZisY() + Quaternion.Euler(0f, 0f, fireAngle) * mod.positionOffset, Quaternion.Euler(0f, 0f, fireAngle + angleForShot));
		Projectile component = gameObject.GetComponent<Projectile>();
		if ((bool)possibleOwner)
		{
			component.Owner = possibleOwner;
			component.Shooter = possibleOwner.specRigidbody;
		}
		if (treatedAsNonProjectileForChallenge)
		{
			component.TreatedAsNonProjectileForChallenge = true;
		}
		component.Inverted = mod.inverted;
		if (volley != null && volley.UsesShotgunStyleVelocityRandomizer)
		{
			component.baseData.speed *= volley.GetVolleySpeedMod();
		}
		component.PlayerProjectileSourceGameTimeslice = Time.time;
		if (possibleOwner is PlayerController)
		{
			(possibleOwner as PlayerController).DoPostProcessProjectile(component);
		}
		if (!mod.mirror)
		{
			return;
		}
		gameObject = SpawnManager.SpawnProjectile(projectile.gameObject, shootPoint.ToVector3ZisY() + Quaternion.Euler(0f, 0f, fireAngle) * mod.InversePositionOffset, Quaternion.Euler(0f, 0f, fireAngle - angleForShot));
		Projectile component2 = gameObject.GetComponent<Projectile>();
		component2.Inverted = true;
		if ((bool)possibleOwner)
		{
			component2.Owner = possibleOwner;
			component2.Shooter = possibleOwner.specRigidbody;
			if ((bool)possibleOwner.aiShooter)
			{
				component2.collidesWithEnemies = possibleOwner.aiShooter.CanShootOtherEnemies;
			}
		}
		if (treatedAsNonProjectileForChallenge)
		{
			component2.TreatedAsNonProjectileForChallenge = true;
		}
		component2.PlayerProjectileSourceGameTimeslice = Time.time;
		if (possibleOwner is PlayerController)
		{
			(possibleOwner as PlayerController).DoPostProcessProjectile(component2);
		}
		component2.baseData.SetAll(component.baseData);
		component2.IsCritical = component.IsCritical;
	}

	public static Projectile ShootSingleProjectile(Projectile currentProjectile, Vector2 shootPoint, float fireAngle, bool inverted, GameActor possibleOwner = null)
	{
		float num = 0f;
		GameObject gameObject = SpawnManager.SpawnProjectile(currentProjectile.gameObject, shootPoint.ToVector3ZisY(), Quaternion.Euler(0f, 0f, fireAngle + num));
		Projectile component = gameObject.GetComponent<Projectile>();
		if ((bool)possibleOwner)
		{
			component.Owner = possibleOwner;
			component.Shooter = possibleOwner.specRigidbody;
		}
		component.Inverted = inverted;
		component.PlayerProjectileSourceGameTimeslice = Time.time;
		if (possibleOwner is PlayerController)
		{
			(possibleOwner as PlayerController).DoPostProcessProjectile(component);
		}
		return component;
	}
}
