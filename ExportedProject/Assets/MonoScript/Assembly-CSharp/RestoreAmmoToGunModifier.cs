using System;
using UnityEngine;

public class RestoreAmmoToGunModifier : MonoBehaviour
{
	public float ChanceToWork = 1f;

	public int AmmoToGain = 1;

	public bool RequiresSynergy;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public string RegainAmmoAnimation;

	private Projectile m_projectile;

	private void Start()
	{
		m_projectile = GetComponent<Projectile>();
		Projectile projectile = m_projectile;
		projectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(projectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
	}

	private void HandleHitEnemy(Projectile arg1, SpeculativeRigidbody arg2, bool arg3)
	{
		if ((bool)m_projectile.PossibleSourceGun && (!RequiresSynergy || arg1.PossibleSourceGun.OwnerHasSynergy(RequiredSynergy)) && UnityEngine.Random.value < ChanceToWork)
		{
			m_projectile.PossibleSourceGun.GainAmmo(AmmoToGain);
			if (!string.IsNullOrEmpty(RegainAmmoAnimation))
			{
				m_projectile.PossibleSourceGun.spriteAnimator.PlayForDuration(RegainAmmoAnimation, -1f, m_projectile.PossibleSourceGun.idleAnimation);
			}
		}
	}
}
