using System;
using UnityEngine;

public class OnDamagedGunImprovementModifier : MonoBehaviour
{
	public int AdditionalClipCapacity;

	private Gun m_gun;

	private PlayerController m_playerOwner;

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
			m_playerOwner.healthHaver.OnHealthChanged += OnHealthChanged;
		}
	}

	private void OnHealthChanged(float resultValue, float maxValue)
	{
		m_gun.AdditionalClipCapacity = Mathf.FloorToInt((maxValue - resultValue) * 2f);
		m_playerOwner.stats.RecalculateStats(m_playerOwner);
	}

	private void OnDestroy()
	{
		OnGunDroppedOrDestroyed();
	}

	private void OnGunDroppedOrDestroyed()
	{
		if (m_playerOwner != null)
		{
			m_playerOwner.healthHaver.OnHealthChanged -= OnHealthChanged;
			m_playerOwner = null;
		}
	}
}
