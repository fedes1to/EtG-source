using System;
using System.Collections.Generic;
using UnityEngine;

public class PunchoutGunController : MonoBehaviour
{
	public string UIStarSpriteName;

	public Projectile BaseProjectile;

	public Projectile OverrideProjectile;

	public float ChargeTimeNormal;

	public float ChargeTimeStar = 0.5f;

	[CheckAnimation(null)]
	public string OverrideFireAnimation;

	[CheckAnimation(null)]
	public string OverrideChargeAnimation;

	private string CachedFireAnimation;

	private string CachedChargeAnimation;

	private Gun m_gun;

	private List<dfSprite> m_extantStars = new List<dfSprite>();

	private PlayerController m_cachedPlayer;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnPreFireProjectileModifier = (Func<Gun, Projectile, ProjectileModule, Projectile>)Delegate.Combine(gun.OnPreFireProjectileModifier, new Func<Gun, Projectile, ProjectileModule, Projectile>(HandlePrefireModifier));
		Gun gun2 = m_gun;
		gun2.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun2.PostProcessProjectile, new Action<Projectile>(HandleProjectileFired));
		Gun gun3 = m_gun;
		gun3.OnDropped = (Action)Delegate.Combine(gun3.OnDropped, new Action(HandleDropped));
		CachedFireAnimation = m_gun.shootAnimation;
		CachedChargeAnimation = m_gun.chargeAnimation;
	}

	private void Update()
	{
		if (!m_cachedPlayer && (bool)m_gun && m_gun.CurrentOwner is PlayerController)
		{
			m_cachedPlayer = m_gun.CurrentOwner as PlayerController;
			m_cachedPlayer.OnReceivedDamage += HandleWasDamaged;
		}
		else if ((bool)m_cachedPlayer && (bool)m_gun && !m_gun.CurrentOwner)
		{
			m_cachedPlayer.OnReceivedDamage -= HandleWasDamaged;
			m_cachedPlayer = null;
		}
	}

	private void HandleWasDamaged(PlayerController obj)
	{
		if (base.enabled)
		{
			RemoveAllStars();
		}
	}

	private void HandleDropped()
	{
		RemoveAllStars();
	}

	public void OnDisable()
	{
		RemoveAllStars();
	}

	public void OnDestroy()
	{
		RemoveAllStars();
		if ((bool)m_cachedPlayer)
		{
			m_cachedPlayer.OnReceivedDamage -= HandleWasDamaged;
			m_cachedPlayer = null;
		}
	}

	private Projectile HandlePrefireModifier(Gun sourceGun, Projectile sourceProjectile, ProjectileModule sourceModule)
	{
		bool flag = sourceProjectile == OverrideProjectile;
		if (m_extantStars.Count >= 3 && flag && (bool)m_cachedPlayer)
		{
			RemoveAllStars();
			if ((bool)m_gun.spriteAnimator)
			{
				m_gun.OverrideAnimations = true;
				m_gun.spriteAnimator.Play(OverrideFireAnimation);
			}
			return sourceProjectile;
		}
		return BaseProjectile;
	}

	private void HandleProjectileFired(Projectile spawnedProjectile)
	{
		spawnedProjectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(spawnedProjectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleEnemyHit));
		m_gun.OverrideAnimations = false;
		if (m_extantStars.Count < 3 && m_gun.chargeAnimation != CachedChargeAnimation)
		{
			m_gun.shootAnimation = CachedFireAnimation;
			m_gun.chargeAnimation = CachedChargeAnimation;
		}
	}

	private void RemoveAllStars()
	{
		if ((bool)m_cachedPlayer && GameUIRoot.HasInstance)
		{
			GameUIAmmoController ammoControllerForPlayerID = GameUIRoot.Instance.GetAmmoControllerForPlayerID(m_cachedPlayer.PlayerIDX);
			for (int num = m_extantStars.Count - 1; num >= 0; num--)
			{
				ammoControllerForPlayerID.DeregisterAdditionalSprite(m_extantStars[num]);
			}
		}
		m_extantStars.Clear();
	}

	private void HandleEnemyHit(Projectile sourceProjectile, SpeculativeRigidbody hitRigidbody, bool fatal)
	{
		if (m_gun.CurrentOwner is PlayerController && fatal && m_extantStars.Count < 3)
		{
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			GameUIAmmoController ammoControllerForPlayerID = GameUIRoot.Instance.GetAmmoControllerForPlayerID(playerController.PlayerIDX);
			dfSprite item = ammoControllerForPlayerID.RegisterNewAdditionalSprite(UIStarSpriteName);
			m_extantStars.Add(item);
			if (m_extantStars.Count >= 3)
			{
				m_gun.chargeAnimation = OverrideChargeAnimation;
			}
		}
	}
}
