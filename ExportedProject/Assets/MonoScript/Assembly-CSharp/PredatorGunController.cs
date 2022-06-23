using System;
using UnityEngine;

public class PredatorGunController : MonoBehaviour
{
	public float HomingRadius = 5f;

	public float HomingAngularVelocity = 360f;

	public GameObject LockOnVFX;

	private AIActor m_lastLockOnEnemy;

	private GameObject m_extantLockOnSprite;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun.PostProcessProjectile, new Action<Projectile>(PostProcessProjectile));
	}

	private void Update()
	{
		if (!m_gun.CurrentOwner || !(m_gun.CurrentOwner is PlayerController))
		{
			return;
		}
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (playerController.CurrentRoom != null)
		{
			float nearestDistance = -1f;
			AIActor nearestEnemy = playerController.CurrentRoom.GetNearestEnemy(playerController.unadjustedAimPoint.XY(), out nearestDistance, true, true);
			if ((bool)nearestEnemy)
			{
				ProcessNearestEnemy(nearestEnemy);
			}
			else
			{
				ProcessNearestEnemy(null);
			}
		}
	}

	private void ProcessNearestEnemy(AIActor hitEnemy)
	{
		if ((bool)hitEnemy)
		{
			if (m_lastLockOnEnemy != hitEnemy)
			{
				if ((bool)m_extantLockOnSprite)
				{
					SpawnManager.Despawn(m_extantLockOnSprite);
				}
				m_extantLockOnSprite = hitEnemy.PlayEffectOnActor((GameObject)BraveResources.Load("Global VFX/VFX_LockOn_Predator"), Vector3.zero, true, true, true);
				m_lastLockOnEnemy = hitEnemy;
			}
		}
		else if ((bool)m_extantLockOnSprite)
		{
			SpawnManager.Despawn(m_extantLockOnSprite);
		}
	}

	private void PostProcessProjectile(Projectile p)
	{
		if ((bool)m_lastLockOnEnemy)
		{
			LockOnHomingModifier lockOnHomingModifier = p.GetComponent<LockOnHomingModifier>();
			if (!lockOnHomingModifier)
			{
				lockOnHomingModifier = p.gameObject.AddComponent<LockOnHomingModifier>();
				lockOnHomingModifier.HomingRadius = 0f;
				lockOnHomingModifier.AngularVelocity = 0f;
			}
			lockOnHomingModifier.HomingRadius += HomingRadius;
			lockOnHomingModifier.AngularVelocity += HomingAngularVelocity;
			lockOnHomingModifier.LockOnVFX = LockOnVFX;
			lockOnHomingModifier.AssignTargetManually(m_lastLockOnEnemy);
		}
	}
}
