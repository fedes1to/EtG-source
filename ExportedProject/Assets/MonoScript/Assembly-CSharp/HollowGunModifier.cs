using System;
using UnityEngine;

public class HollowGunModifier : MonoBehaviour
{
	public float DamageMultiplier = 1.5f;

	private Gun m_gun;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun.PostProcessProjectile, new Action<Projectile>(HandleProcessProjectile));
	}

	private void HandleProcessProjectile(Projectile obj)
	{
		if (m_gun.CurrentOwner is PlayerController)
		{
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			if (playerController.IsDarkSoulsHollow)
			{
				obj.baseData.damage *= DamageMultiplier;
			}
		}
	}
}
