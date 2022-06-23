using System;
using System.Collections.Generic;
using UnityEngine;

public class MaxNumberAliveModifier : MonoBehaviour
{
	private List<Projectile> m_aliveProjectiles;

	public int MaxNumberAlive;

	private void Start()
	{
		Gun component = GetComponent<Gun>();
		m_aliveProjectiles = new List<Projectile>();
		component.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(component.PostProcessProjectile, new Action<Projectile>(HandleProjectileFired));
	}

	private void HandleProjectileFired(Projectile obj)
	{
		m_aliveProjectiles.Add(obj);
		CompactList();
	}

	private void CompactList()
	{
		for (int i = 0; i < m_aliveProjectiles.Count; i++)
		{
			if (!m_aliveProjectiles[i])
			{
				m_aliveProjectiles.RemoveAt(i);
				i--;
			}
		}
		while (m_aliveProjectiles.Count > MaxNumberAlive)
		{
			Projectile projectile = m_aliveProjectiles[0];
			m_aliveProjectiles.RemoveAt(0);
			projectile.DieInAir();
		}
	}
}
