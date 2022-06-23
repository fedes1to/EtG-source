using System;
using Dungeonator;
using UnityEngine;

public class ShootProjectileOnGunfireDoer : BraveBehaviour, SingleSpawnableGunPlacedObject
{
	[CheckAnimation(null)]
	public string inAnimation;

	[CheckAnimation(null)]
	public string fireAnimation;

	[CheckAnimation(null)]
	public string idleAnimation;

	[CheckAnimation(null)]
	public string outAnimation;

	public bool HasOverrideSynergy;

	[LongNumericEnum]
	public CustomSynergyType OverrideSynergy;

	public ProjectileVolleyData OverrideSynergyVolley;

	private Gun m_sourceGun;

	private PlayerController m_ownerPlayer;

	private bool m_isActive;

	private RoomHandler m_room;

	private bool m_firedThisFrame;

	private int m_lastFiredFrame = -1;

	public void Initialize(Gun sourceGun)
	{
		if ((bool)sourceGun && (bool)sourceGun.CurrentOwner && sourceGun.CurrentOwner is PlayerController)
		{
			if (!string.IsNullOrEmpty(inAnimation))
			{
				base.spriteAnimator.Play(inAnimation);
			}
			m_isActive = true;
			m_sourceGun = sourceGun;
			Gun sourceGun2 = m_sourceGun;
			sourceGun2.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(sourceGun2.PostProcessProjectile, new Action<Projectile>(HandleProjectileFired));
			m_room = base.transform.position.GetAbsoluteRoom();
			m_ownerPlayer = sourceGun.CurrentOwner as PlayerController;
		}
	}

	private void Update()
	{
		if (m_isActive)
		{
			if (m_ownerPlayer.CurrentRoom != m_room)
			{
				Deactivate();
			}
			m_firedThisFrame = false;
		}
	}

	public void Deactivate()
	{
		m_isActive = false;
		if ((bool)m_sourceGun)
		{
			Gun sourceGun = m_sourceGun;
			sourceGun.PostProcessProjectile = (Action<Projectile>)Delegate.Remove(sourceGun.PostProcessProjectile, new Action<Projectile>(HandleProjectileFired));
		}
		if ((bool)this)
		{
			if (!string.IsNullOrEmpty(inAnimation))
			{
				base.spriteAnimator.PlayAndDestroyObject(outAnimation);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	private void HandleProjectileFired(Projectile obj)
	{
		if (!this || !base.sprite)
		{
			Deactivate();
		}
		else
		{
			if (!m_isActive)
			{
				return;
			}
			if (!m_firedThisFrame)
			{
				m_sourceGun.muzzleFlashEffects.SpawnAtPosition(base.sprite.WorldCenter.ToVector3ZUp());
			}
			m_firedThisFrame = true;
			if (!string.IsNullOrEmpty(inAnimation) && !base.spriteAnimator.IsPlaying(fireAnimation))
			{
				base.spriteAnimator.PlayForDuration(fireAnimation, -1f, idleAnimation);
			}
			if (HasOverrideSynergy && m_ownerPlayer.HasActiveBonusSynergy(OverrideSynergy))
			{
				Vector2 worldCenter = base.sprite.WorldCenter;
				float nearestDistance = -1f;
				AIActor nearestEnemy = base.transform.position.GetAbsoluteRoom().GetNearestEnemy(worldCenter, out nearestDistance);
				if ((bool)nearestEnemy && m_lastFiredFrame != Time.frameCount)
				{
					m_lastFiredFrame = Time.frameCount;
					VolleyUtility.FireVolley(OverrideSynergyVolley, worldCenter, nearestEnemy.CenterPosition - worldCenter, obj.Owner);
				}
			}
			else if ((bool)obj)
			{
				Vector3 vector = obj.transform.position - m_sourceGun.barrelOffset.position;
				GameObject gameObject = SpawnManager.SpawnProjectile(obj.gameObject, base.sprite.WorldCenter + vector.XY(), obj.transform.rotation);
				if ((bool)gameObject)
				{
					Projectile component = gameObject.GetComponent<Projectile>();
					component.Owner = obj.Owner;
					component.Shooter = obj.Shooter;
					component.PossibleSourceGun = obj.PossibleSourceGun;
					component.collidesWithPlayer = false;
					component.collidesWithEnemies = true;
				}
			}
		}
	}
}
