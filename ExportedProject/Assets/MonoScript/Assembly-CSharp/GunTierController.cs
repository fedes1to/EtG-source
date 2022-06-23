using System;
using System.Collections.Generic;
using UnityEngine;

public class GunTierController : MonoBehaviour, IGunInheritable
{
	public int[] TierThresholds;

	public int TiersToDropOnDamage = 100;

	public int MaxTier = 3;

	public GameObject[] TierVFX;

	private Gun m_gun;

	private PlayerController m_playerOwner;

	private int m_kills;

	private int KillsToNextTier
	{
		get
		{
			int currentStrengthTier = m_gun.CurrentStrengthTier;
			if (currentStrengthTier >= TierThresholds.Length)
			{
				return int.MaxValue;
			}
			return TierThresholds[currentStrengthTier];
		}
	}

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnInitializedWithOwner = (Action<GameActor>)Delegate.Combine(gun.OnInitializedWithOwner, new Action<GameActor>(OnGunInitialized));
		Gun gun2 = m_gun;
		gun2.OnDropped = (Action)Delegate.Combine(gun2.OnDropped, new Action(OnGunDroppedOrDestroyed));
		if (m_gun.CurrentOwner != null)
		{
			OnGunInitialized(m_gun.CurrentOwner);
		}
	}

	private void OnGunInitialized(GameActor obj)
	{
		if (m_playerOwner != null)
		{
			OnGunDroppedOrDestroyed();
		}
		if (!(obj == null) && obj is PlayerController)
		{
			m_playerOwner = obj as PlayerController;
			m_playerOwner.OnKilledEnemy += OnEnemyKilled;
			m_playerOwner.healthHaver.OnDamaged += OnPlayerDamaged;
		}
	}

	private void PlayTierVFX(PlayerController p)
	{
		if (m_gun.CurrentStrengthTier >= 0 && m_gun.CurrentStrengthTier < TierVFX.Length)
		{
			p.PlayEffectOnActor(TierVFX[m_gun.CurrentStrengthTier], Vector3.up, true, true);
		}
	}

	private void OnEnemyKilled(PlayerController obj)
	{
		if (obj.CurrentGun == m_gun)
		{
			m_kills++;
			if (m_kills > KillsToNextTier)
			{
				m_gun.CurrentStrengthTier = Mathf.Clamp(m_gun.CurrentStrengthTier + 1, 0, MaxTier - 1);
				PlayTierVFX(obj);
			}
		}
	}

	private void OnDestroy()
	{
		OnGunDroppedOrDestroyed();
	}

	private void OnGunDroppedOrDestroyed()
	{
		if (m_playerOwner != null)
		{
			m_playerOwner.healthHaver.OnDamaged -= OnPlayerDamaged;
			m_playerOwner.OnKilledEnemy -= OnEnemyKilled;
			m_playerOwner = null;
		}
	}

	private void OnPlayerDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		int num = Mathf.Clamp(m_gun.CurrentStrengthTier - TiersToDropOnDamage, 0, MaxTier - 1);
		if (m_gun.CurrentStrengthTier != num)
		{
			m_gun.CurrentStrengthTier = num;
			PlayTierVFX(m_gun.CurrentOwner as PlayerController);
		}
		if (m_gun.CurrentStrengthTier == 0)
		{
			m_kills = 0;
		}
		else
		{
			m_kills = TierThresholds[m_gun.CurrentStrengthTier - 1];
		}
	}

	public void InheritData(Gun sourceGun)
	{
		GunTierController component = sourceGun.GetComponent<GunTierController>();
		if ((bool)component)
		{
			m_kills = component.m_kills;
		}
	}

	public void MidGameSerialize(List<object> data, int dataIndex)
	{
		data.Add(m_kills);
	}

	public void MidGameDeserialize(List<object> data, ref int dataIndex)
	{
		m_kills = (int)data[dataIndex];
		if (m_gun == null)
		{
			m_gun = GetComponent<Gun>();
		}
		while (m_kills > KillsToNextTier)
		{
			m_gun.CurrentStrengthTier = Mathf.Clamp(m_gun.CurrentStrengthTier + 1, 0, MaxTier - 1);
		}
		dataIndex++;
	}
}
