using System;
using UnityEngine;

public class MassiveCriticalModifier : MonoBehaviour
{
	public float ActivationChance = 0.01f;

	public Projectile ReplacementProjectile;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnPreFireProjectileModifier = (Func<Gun, Projectile, ProjectileModule, Projectile>)Delegate.Combine(gun.OnPreFireProjectileModifier, new Func<Gun, Projectile, ProjectileModule, Projectile>(HandleProjectileReplacement));
	}

	private Projectile HandleProjectileReplacement(Gun sourceGun, Projectile sourceProjectile, ProjectileModule sourceModule)
	{
		if (UnityEngine.Random.value > ActivationChance)
		{
			return sourceProjectile;
		}
		ReplacementProjectile.IsCritical = true;
		return ReplacementProjectile;
	}
}
