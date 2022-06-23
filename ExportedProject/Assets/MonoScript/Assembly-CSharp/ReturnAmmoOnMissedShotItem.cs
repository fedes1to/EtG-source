using System.Collections.Generic;
using UnityEngine;

public class ReturnAmmoOnMissedShotItem : PassiveItem, ILevelLoadedListener
{
	public float ChanceToRegainAmmoOnMiss = 0.25f;

	public bool UsesZombieBulletsSynergy;

	public float SynergyChance = 0.5f;

	private PlayerController m_player;

	private Dictionary<float, int> m_slicesFired = new Dictionary<float, int>();

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
		}
	}

	public void BraveOnLevelWasLoaded()
	{
		m_slicesFired.Clear();
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		if (obj.PlayerProjectileSourceGameTimeslice != -1f)
		{
			if (m_slicesFired.ContainsKey(obj.PlayerProjectileSourceGameTimeslice))
			{
				m_slicesFired[obj.PlayerProjectileSourceGameTimeslice] = m_slicesFired[obj.PlayerProjectileSourceGameTimeslice] + 1;
			}
			else
			{
				m_slicesFired.Add(obj.PlayerProjectileSourceGameTimeslice, 1);
			}
			obj.OnDestruction += HandleProjectileDestruction;
		}
	}

	private void HandleProjectileDestruction(Projectile source)
	{
		if (source.PlayerProjectileSourceGameTimeslice == -1f || !m_slicesFired.ContainsKey(source.PlayerProjectileSourceGameTimeslice) || !m_player || !source || !source.PossibleSourceGun || source.PossibleSourceGun.InfiniteAmmo || source.HasImpactedEnemy)
		{
			return;
		}
		m_slicesFired[source.PlayerProjectileSourceGameTimeslice] = m_slicesFired[source.PlayerProjectileSourceGameTimeslice] - 1;
		if (m_slicesFired[source.PlayerProjectileSourceGameTimeslice] == 0)
		{
			float num = ChanceToRegainAmmoOnMiss;
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.ZOMBIE_AMMO))
			{
				num = SynergyChance;
			}
			if (Random.value < num)
			{
				source.PossibleSourceGun.GainAmmo(1);
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<ReturnAmmoOnMissedShotItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
		}
	}
}
