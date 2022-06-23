using System;
using UnityEngine;

public class ProjectileRandomizerItem : PassiveItem
{
	public float OverallChanceToTakeEffect = 0.5f;

	public float BeamShootDuration = 3f;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Combine(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModification));
		}
	}

	private static float GetQualityModifiedAmmo(Gun cg)
	{
		switch (cg.quality)
		{
		case ItemQuality.A:
			return Mathf.Min((float)cg.AdjustedMaxAmmo * 0.8f, 250f);
		case ItemQuality.S:
			return Mathf.Min((float)cg.AdjustedMaxAmmo * 0.7f, 100f);
		default:
			return cg.AdjustedMaxAmmo;
		}
	}

	public static Projectile GetRandomizerProjectileFromPlayer(PlayerController sourcePlayer, Projectile fallbackProjectile, int fallbackAmmo)
	{
		int num = fallbackAmmo;
		for (int i = 0; i < sourcePlayer.inventory.AllGuns.Count; i++)
		{
			if ((bool)sourcePlayer.inventory.AllGuns[i] && !sourcePlayer.inventory.AllGuns[i].InfiniteAmmo)
			{
				Gun cg = sourcePlayer.inventory.AllGuns[i];
				num += Mathf.CeilToInt(GetQualityModifiedAmmo(cg));
			}
		}
		int num2 = fallbackAmmo;
		float num3 = (float)num * UnityEngine.Random.value;
		if ((float)num2 > num3)
		{
			return fallbackProjectile;
		}
		for (int j = 0; j < sourcePlayer.inventory.AllGuns.Count; j++)
		{
			if (!sourcePlayer.inventory.AllGuns[j] || sourcePlayer.inventory.AllGuns[j].InfiniteAmmo)
			{
				continue;
			}
			Gun cg2 = sourcePlayer.inventory.AllGuns[j];
			num2 += Mathf.CeilToInt(GetQualityModifiedAmmo(cg2));
			if (!((float)num2 > num3))
			{
				continue;
			}
			ProjectileModule defaultModule = sourcePlayer.inventory.AllGuns[j].DefaultModule;
			if (defaultModule.shootStyle == ProjectileModule.ShootStyle.Beam)
			{
				return fallbackProjectile;
			}
			if (defaultModule.shootStyle == ProjectileModule.ShootStyle.Charged)
			{
				Projectile projectile = null;
				for (int k = 0; k < 15; k++)
				{
					ProjectileModule.ChargeProjectile chargeProjectile = defaultModule.chargeProjectiles[UnityEngine.Random.Range(0, defaultModule.chargeProjectiles.Count)];
					if (chargeProjectile != null)
					{
						projectile = chargeProjectile.Projectile;
					}
					if ((bool)projectile)
					{
						break;
					}
				}
				return projectile ?? fallbackProjectile;
			}
			Projectile currentProjectile = defaultModule.GetCurrentProjectile();
			return currentProjectile ?? fallbackProjectile;
		}
		return fallbackProjectile;
	}

	private Projectile HandlePreFireProjectileModification(Gun sourceGun, Projectile sourceProjectile)
	{
		float num = OverallChanceToTakeEffect;
		if ((bool)sourceGun && sourceGun.Volley != null)
		{
			num /= (float)sourceGun.Volley.projectiles.Count;
		}
		if (UnityEngine.Random.value > num)
		{
			return sourceProjectile;
		}
		if ((bool)sourceGun && sourceGun.InfiniteAmmo)
		{
			return sourceProjectile;
		}
		int num2 = 0;
		if ((bool)m_player && m_player.inventory != null)
		{
			for (int i = 0; i < m_player.inventory.AllGuns.Count; i++)
			{
				if ((bool)m_player.inventory.AllGuns[i] && !m_player.inventory.AllGuns[i].InfiniteAmmo)
				{
					Gun cg = m_player.inventory.AllGuns[i];
					num2 += Mathf.CeilToInt(GetQualityModifiedAmmo(cg));
				}
			}
			int num3 = 0;
			float num4 = (float)num2 * UnityEngine.Random.value;
			for (int j = 0; j < m_player.inventory.AllGuns.Count; j++)
			{
				if (!m_player.inventory.AllGuns[j] || m_player.inventory.AllGuns[j].InfiniteAmmo)
				{
					continue;
				}
				Gun cg2 = m_player.inventory.AllGuns[j];
				num3 += Mathf.CeilToInt(GetQualityModifiedAmmo(cg2));
				if (!((float)num3 > num4))
				{
					continue;
				}
				ProjectileModule defaultModule = m_player.inventory.AllGuns[j].DefaultModule;
				if (defaultModule.shootStyle == ProjectileModule.ShootStyle.Beam)
				{
					BeamController.FreeFireBeam(defaultModule.GetCurrentProjectile(), m_player, m_player.CurrentGun.CurrentAngle, BeamShootDuration, true);
					return sourceProjectile;
				}
				if (defaultModule.shootStyle == ProjectileModule.ShootStyle.Charged)
				{
					Projectile projectile = null;
					for (int k = 0; k < 15; k++)
					{
						ProjectileModule.ChargeProjectile chargeProjectile = defaultModule.chargeProjectiles[UnityEngine.Random.Range(0, defaultModule.chargeProjectiles.Count)];
						if (chargeProjectile != null)
						{
							projectile = chargeProjectile.Projectile;
						}
						if ((bool)projectile)
						{
							break;
						}
					}
					return projectile ?? sourceProjectile;
				}
				Projectile currentProjectile = defaultModule.GetCurrentProjectile();
				return currentProjectile ?? sourceProjectile;
			}
		}
		return sourceProjectile;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<ProjectileRandomizerItem>().m_pickedUpThisRun = true;
		player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Remove(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModification));
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			PlayerController player = m_player;
			player.OnPreFireProjectileModifier = (Func<Gun, Projectile, Projectile>)Delegate.Remove(player.OnPreFireProjectileModifier, new Func<Gun, Projectile, Projectile>(HandlePreFireProjectileModification));
		}
	}
}
