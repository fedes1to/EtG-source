using System;
using UnityEngine;

public class RatPackItem : PlayerItem
{
	public int MaxContainedBullets = 30;

	public float ScoopRadius = 1f;

	public DirectedBurstInterface Burst;

	private int m_containedBullets;

	private Action<Projectile> EatBulletAction;

	public int ContainedBullets
	{
		get
		{
			return m_containedBullets;
		}
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnIsRolling += HandleRollingFrame;
	}

	private void HandleRollingFrame(PlayerController src)
	{
		if (EatBulletAction == null)
		{
			EatBulletAction = (Action<Projectile>)Delegate.Combine(EatBulletAction, new Action<Projectile>(EatBullet));
		}
		if (src.CurrentRollState == PlayerController.DodgeRollState.InAir)
		{
			AffectNearbyProjectiles(src.CenterPosition, ScoopRadius, EatBulletAction);
		}
	}

	private static void AffectNearbyProjectiles(Vector2 center, float radius, Action<Projectile> DoEffect)
	{
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.Owner is AIActor)
			{
				float sqrMagnitude = (projectile.transform.position.XY() - center).sqrMagnitude;
				if (sqrMagnitude < radius)
				{
					DoEffect(projectile);
				}
			}
		}
	}

	private void EatBullet(Projectile p)
	{
		if (p.Owner is AIActor)
		{
			p.DieInAir();
			m_containedBullets++;
			m_containedBullets = Mathf.Clamp(m_containedBullets, 0, MaxContainedBullets);
		}
	}

	private void HandleDodgedProjectile(Projectile p)
	{
		EatBullet(p);
	}

	public override bool CanBeUsed(PlayerController user)
	{
		return m_containedBullets > 0 && base.CanBeUsed(user);
	}

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		Burst.NumberWaves = 1;
		Burst.MinToSpawnPerWave = ContainedBullets;
		Burst.MaxToSpawnPerWave = ContainedBullets;
		Burst.UseShotgunStyleVelocityModifier = true;
		if ((bool)user && (bool)user.CurrentGun)
		{
			Burst.DoBurst(user, user.CurrentGun.CurrentAngle);
		}
		m_containedBullets = 0;
	}

	protected override void OnPreDrop(PlayerController user)
	{
		user.OnIsRolling -= HandleRollingFrame;
		user.OnDodgedProjectile -= HandleDodgedProjectile;
		base.OnPreDrop(user);
	}

	protected override void OnDestroy()
	{
		if ((bool)LastOwner)
		{
			LastOwner.OnIsRolling -= HandleRollingFrame;
			LastOwner.OnDodgedProjectile -= HandleDodgedProjectile;
		}
		base.OnDestroy();
	}
}
