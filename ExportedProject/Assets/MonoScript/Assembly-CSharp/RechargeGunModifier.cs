using System;
using UnityEngine;

public class RechargeGunModifier : MonoBehaviour
{
	public float MaxDamageMultiplierPerStack = 0.1f;

	public float MinDamageMultiplierPerStack = 0.05f;

	public float MultiplierCap = 4f;

	public int StackFalloffPoint = 5;

	public RechargeGunProjectileTier[] Projectiles;

	private Gun m_gun;

	private bool m_callbackInitialized;

	private PlayerController m_cachedPlayer;

	private int m_counter = 1;

	private bool m_wasReloading;

	public void Start()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnPreFireProjectileModifier = (Func<Gun, Projectile, ProjectileModule, Projectile>)Delegate.Combine(gun.OnPreFireProjectileModifier, new Func<Gun, Projectile, ProjectileModule, Projectile>(HandleReplaceProjectile));
		Gun gun2 = m_gun;
		gun2.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun2.PostProcessProjectile, new Action<Projectile>(PostProcessProjectile));
		Gun gun3 = m_gun;
		gun3.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun3.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
	}

	private Projectile HandleReplaceProjectile(Gun arg1, Projectile arg2, ProjectileModule arg3)
	{
		Projectile result = arg2;
		int num = 0;
		for (int i = 0; i < Projectiles.Length; i++)
		{
			if (m_counter > Projectiles[i].RequiredStacks && num <= Projectiles[i].RequiredStacks)
			{
				num = Projectiles[i].RequiredStacks;
				result = Projectiles[i].ReplacementProjectile;
			}
		}
		return result;
	}

	private void PostProcessProjectile(Projectile obj)
	{
		int num = Mathf.Min(StackFalloffPoint, m_counter);
		int num2 = Mathf.Max(0, m_counter - StackFalloffPoint);
		float b = 1f + MaxDamageMultiplierPerStack * (float)num + MinDamageMultiplierPerStack * (float)num2;
		b = Mathf.Min(MultiplierCap, b);
		obj.baseData.damage *= b;
	}

	private void Update()
	{
		if (!m_callbackInitialized && m_gun.CurrentOwner is PlayerController)
		{
			m_callbackInitialized = true;
			m_cachedPlayer = m_gun.CurrentOwner as PlayerController;
			m_cachedPlayer.OnTriedToInitiateAttack += HandleTriedToInitiateAttack;
		}
		else if (m_callbackInitialized && !(m_gun.CurrentOwner is PlayerController))
		{
			m_callbackInitialized = false;
			if ((bool)m_cachedPlayer)
			{
				m_cachedPlayer.OnTriedToInitiateAttack -= HandleTriedToInitiateAttack;
				m_cachedPlayer = null;
			}
		}
		if (m_wasReloading && (bool)m_gun && !m_gun.IsReloading)
		{
			m_wasReloading = false;
		}
	}

	private void HandleTriedToInitiateAttack(PlayerController sourcePlayer)
	{
	}

	private void HandleReloadPressed(PlayerController ownerPlayer, Gun sourceGun, bool something)
	{
		if (sourceGun.IsReloading)
		{
			if (!m_wasReloading)
			{
				m_counter = 0;
				m_wasReloading = true;
			}
			m_counter++;
			AkSoundEngine.PostEvent("Play_WPN_RechargeGun_Recharge_01", base.gameObject);
		}
		else
		{
			m_wasReloading = false;
		}
	}
}
