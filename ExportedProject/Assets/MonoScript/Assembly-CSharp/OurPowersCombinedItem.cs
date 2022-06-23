public class OurPowersCombinedItem : PassiveItem
{
	public float PercentOfOtherGunsDamage = 0.02f;

	protected PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
			player.PostProcessBeamTick += PostProcessBeamTick;
		}
	}

	private float GetDamageContribution()
	{
		float num = 0f;
		if (m_player != null)
		{
			for (int i = 0; i < m_player.inventory.AllGuns.Count; i++)
			{
				Gun gun = m_player.inventory.AllGuns[i];
				if (gun == m_player.CurrentGun || gun.DefaultModule == null)
				{
					continue;
				}
				if (gun.DefaultModule.projectiles.Count > 0 && gun.DefaultModule.projectiles[0] != null)
				{
					num += gun.DefaultModule.projectiles[0].baseData.damage * PercentOfOtherGunsDamage;
				}
				else
				{
					if (gun.DefaultModule.chargeProjectiles == null || gun.DefaultModule.chargeProjectiles.Count <= 0)
					{
						continue;
					}
					for (int j = 0; j < gun.DefaultModule.chargeProjectiles.Count; j++)
					{
						if (gun.DefaultModule.chargeProjectiles[j].Projectile != null)
						{
							num += gun.DefaultModule.chargeProjectiles[j].Projectile.baseData.damage * PercentOfOtherGunsDamage;
							break;
						}
					}
				}
			}
		}
		return num;
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		obj.baseData.damage += GetDamageContribution();
	}

	private void PostProcessBeam(BeamController beam)
	{
		beam.DamageModifier += GetDamageContribution();
	}

	private void PostProcessBeamTick(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickRate)
	{
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<OurPowersCombinedItem>().m_pickedUpThisRun = true;
		m_player = null;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		player.PostProcessBeamTick -= PostProcessBeamTick;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
			m_player.PostProcessBeamTick -= PostProcessBeamTick;
		}
	}
}
