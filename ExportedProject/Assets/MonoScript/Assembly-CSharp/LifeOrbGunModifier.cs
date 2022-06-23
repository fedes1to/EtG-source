using System;
using UnityEngine;

public class LifeOrbGunModifier : MonoBehaviour
{
	public float damageFraction = 1f;

	public GameObject OverheadVFX;

	public GameObject OnKilledEnemyVFX;

	public GameObject OnBurstGunVFX;

	public GameObject OnBurstDamageVFX;

	private GameObject m_overheadVFXInstance;

	private Gun m_gun;

	private bool m_connected;

	private PlayerController m_lastOwner;

	private HealthHaver m_lastTargetDamaged;

	private float m_totalDamageDealtToLastTarget;

	private float m_storedSoulDamage;

	private bool m_isDealingBurstDamage;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
	}

	private void HandleReloadPressed(PlayerController owner, Gun source, bool reloadSomething)
	{
		if (m_storedSoulDamage > 0f)
		{
			if ((bool)OnBurstGunVFX)
			{
				SpawnManager.SpawnVFX(OnBurstGunVFX, owner.CurrentGun.barrelOffset.position, Quaternion.identity);
			}
			m_isDealingBurstDamage = true;
			owner.CurrentRoom.ApplyActionToNearbyEnemies(owner.transform.position.XY(), 100f, ProcessEnemy);
			m_isDealingBurstDamage = false;
			ClearSoul(false);
		}
	}

	private void OnDisable()
	{
		ClearSoul(true);
	}

	private void ClearSoul(bool disabling)
	{
		m_storedSoulDamage = 0f;
		m_gun.idleAnimation = string.Empty;
		if (!disabling)
		{
			m_gun.PlayIdleAnimation();
		}
		if ((bool)m_overheadVFXInstance)
		{
			SpawnManager.Despawn(m_overheadVFXInstance.gameObject);
			m_overheadVFXInstance = null;
		}
	}

	private void ProcessEnemy(AIActor a, float distance)
	{
		if ((bool)a && a.IsNormalEnemy && (bool)a.healthHaver && !a.IsGone)
		{
			if ((bool)m_lastOwner)
			{
				a.healthHaver.ApplyDamage(m_storedSoulDamage * damageFraction, Vector2.zero, m_lastOwner.ActorName);
			}
			else
			{
				a.healthHaver.ApplyDamage(m_storedSoulDamage * damageFraction, Vector2.zero, "projectile");
			}
			if ((bool)OnBurstDamageVFX)
			{
				a.PlayEffectOnActor(OnBurstDamageVFX, Vector3.zero);
			}
		}
	}

	private void Update()
	{
		if (!m_connected && (bool)m_gun.CurrentOwner)
		{
			m_connected = true;
			m_lastOwner = m_gun.CurrentOwner as PlayerController;
			m_lastOwner.OnDealtDamageContext += HandlePlayerDealtDamage;
		}
		else if (m_connected && !m_gun.CurrentOwner)
		{
			m_connected = false;
			m_lastOwner.OnDealtDamageContext -= HandlePlayerDealtDamage;
		}
	}

	private void HandlePlayerDealtDamage(PlayerController source, float damage, bool fatal, HealthHaver target)
	{
		if (source.CurrentGun != m_gun || m_isDealingBurstDamage)
		{
			return;
		}
		if (m_lastTargetDamaged != target)
		{
			m_lastTargetDamaged = target;
			m_totalDamageDealtToLastTarget = 0f;
		}
		m_totalDamageDealtToLastTarget += damage;
		if (fatal)
		{
			m_storedSoulDamage = m_totalDamageDealtToLastTarget;
			m_lastTargetDamaged = null;
			m_totalDamageDealtToLastTarget = 0f;
			if ((bool)OverheadVFX && !m_overheadVFXInstance)
			{
				m_overheadVFXInstance = source.PlayEffectOnActor(OverheadVFX, Vector3.up);
				m_overheadVFXInstance.transform.localPosition = m_overheadVFXInstance.transform.localPosition.Quantize(0.0625f);
				m_gun.idleAnimation = "life_orb_full_idle";
				m_gun.PlayIdleAnimation();
			}
			if ((bool)OnKilledEnemyVFX && (bool)target && (bool)target.aiActor)
			{
				target.aiActor.PlayEffectOnActor(OnKilledEnemyVFX, new Vector3(0f, 0.5f, 0f), false);
			}
		}
	}
}
