using System.Collections.Generic;
using UnityEngine;

public class TripleTapEffect : MonoBehaviour
{
	public int RequiredSequentialShots = 3;

	public int AmmoToGain = 1;

	private int m_shotCounter;

	private AIActor m_companion;

	private PlayerController m_player;

	private Dictionary<float, int> m_slicesFired = new Dictionary<float, int>();

	private void Start()
	{
		m_companion = GetComponent<AIActor>();
		PlayerController companionOwner = m_companion.CompanionOwner;
		if ((bool)companionOwner)
		{
			m_player = companionOwner;
			m_player.PostProcessProjectile += PostProcessProjectile;
		}
	}

	private void PostProcessProjectile(Projectile sourceProjectile, float effectChanceScalar)
	{
		if (sourceProjectile.PlayerProjectileSourceGameTimeslice != -1f)
		{
			if (m_slicesFired.ContainsKey(sourceProjectile.PlayerProjectileSourceGameTimeslice))
			{
				m_slicesFired[sourceProjectile.PlayerProjectileSourceGameTimeslice] = m_slicesFired[sourceProjectile.PlayerProjectileSourceGameTimeslice] + 1;
			}
			else
			{
				m_slicesFired.Add(sourceProjectile.PlayerProjectileSourceGameTimeslice, 1);
			}
			sourceProjectile.OnDestruction += HandleProjectileDestruction;
		}
	}

	private void HandleProjectileDestruction(Projectile source)
	{
		if (source.PlayerProjectileSourceGameTimeslice == -1f || !m_slicesFired.ContainsKey(source.PlayerProjectileSourceGameTimeslice) || !m_player || !source)
		{
			return;
		}
		if (source.HasImpactedEnemy)
		{
			m_slicesFired.Remove(source.PlayerProjectileSourceGameTimeslice);
			if (m_player.HasActiveBonusSynergy(CustomSynergyType.GET_IT_ITS_BOWLING))
			{
				m_shotCounter = Mathf.Min(RequiredSequentialShots, m_shotCounter + source.NumberHealthHaversHit);
			}
			else
			{
				m_shotCounter++;
			}
			if (m_shotCounter >= RequiredSequentialShots)
			{
				m_shotCounter -= RequiredSequentialShots;
				if ((bool)source.PossibleSourceGun && !source.PossibleSourceGun.InfiniteAmmo && source.PossibleSourceGun.CanGainAmmo)
				{
					source.PossibleSourceGun.GainAmmo(AmmoToGain);
				}
			}
		}
		else
		{
			m_slicesFired[source.PlayerProjectileSourceGameTimeslice] = m_slicesFired[source.PlayerProjectileSourceGameTimeslice] - 1;
			if (m_slicesFired[source.PlayerProjectileSourceGameTimeslice] == 0)
			{
				m_shotCounter = 0;
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
		}
	}
}
