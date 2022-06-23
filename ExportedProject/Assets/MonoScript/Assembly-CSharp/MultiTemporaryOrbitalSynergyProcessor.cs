using System;
using UnityEngine;

public class MultiTemporaryOrbitalSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public GameObject OrbitalPrefab;

	private MultiTemporaryOrbitalLayer m_layer;

	private bool m_hasBeenInitialized;

	private Gun m_gun;

	private bool m_attached;

	private PlayerController m_lastPlayer;

	private void Start()
	{
		m_gun = GetComponent<Gun>();
		m_layer = new MultiTemporaryOrbitalLayer();
		m_layer.collisionDamage = 3f;
	}

	private void Update()
	{
		if (!m_attached)
		{
			if ((bool)m_gun && (bool)m_gun.CurrentOwner && m_gun.OwnerHasSynergy(RequiredSynergy))
			{
				PlayerController playerController = m_gun.CurrentOwner as PlayerController;
				playerController.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Combine(playerController.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(HandleEnemyDamaged));
				m_lastPlayer = playerController;
				m_attached = true;
			}
		}
		else if (!m_gun || !m_gun.CurrentOwner || !m_gun.OwnerHasSynergy(RequiredSynergy))
		{
			if ((bool)m_lastPlayer)
			{
				PlayerController lastPlayer = m_lastPlayer;
				lastPlayer.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Remove(lastPlayer.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(HandleEnemyDamaged));
			}
			m_lastPlayer = null;
			m_attached = false;
		}
		if (m_hasBeenInitialized)
		{
			m_layer.Update();
		}
	}

	private void OnDestroy()
	{
		if (m_attached)
		{
			if (m_layer != null)
			{
				m_layer.Disconnect();
			}
			if ((bool)m_lastPlayer)
			{
				PlayerController lastPlayer = m_lastPlayer;
				lastPlayer.OnAnyEnemyReceivedDamage = (Action<float, bool, HealthHaver>)Delegate.Remove(lastPlayer.OnAnyEnemyReceivedDamage, new Action<float, bool, HealthHaver>(HandleEnemyDamaged));
			}
			m_lastPlayer = null;
			m_attached = false;
		}
	}

	private void HandleEnemyDamaged(float dmg, bool fatal, HealthHaver target)
	{
		if ((bool)m_gun && m_gun.CurrentOwner is PlayerController && (m_gun.CurrentOwner as PlayerController).CurrentGun == m_gun && fatal)
		{
			m_layer.targetNumberOrbitals = Mathf.Min(20, m_layer.targetNumberOrbitals + 1);
			if (!m_hasBeenInitialized)
			{
				m_layer.Initialize(m_lastPlayer, OrbitalPrefab);
				m_hasBeenInitialized = true;
			}
		}
	}
}
