using System.Collections.Generic;
using UnityEngine;

public class EvolverGunController : MonoBehaviour, IGunInheritable
{
	[PickupIdentifier]
	public int Form01ID;

	[PickupIdentifier]
	public int Form02ID;

	[PickupIdentifier]
	public int Form03ID;

	[PickupIdentifier]
	public int Form04ID;

	[PickupIdentifier]
	public int Form05ID;

	[PickupIdentifier]
	public int Form06ID;

	public int TypesPerForm = 5;

	private Gun m_gun;

	private bool m_initialized;

	private PlayerController m_player;

	private int m_currentForm;

	private HashSet<string> m_enemiesKilled = new HashSet<string>();

	private int m_savedEnemiesKilled;

	private bool m_synergyActive;

	private bool m_wasDeserialized;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void KilledEnemyContext(PlayerController sourcePlayer, HealthHaver killedEnemy)
	{
		if ((bool)killedEnemy)
		{
			AIActor component = killedEnemy.GetComponent<AIActor>();
			if ((bool)component)
			{
				m_enemiesKilled.Add(component.EnemyGuid);
				UpdateTier(sourcePlayer);
			}
		}
	}

	private void UpdateTier(PlayerController sourcePlayer)
	{
		int num = m_enemiesKilled.Count + m_savedEnemiesKilled;
		int num2 = TypesPerForm;
		if ((bool)sourcePlayer && sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.NATURAL_SELECTION))
		{
			num2 = Mathf.Max(1, TypesPerForm - 2);
		}
		if ((bool)sourcePlayer && sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.POWERHOUSE_OF_THE_CELL))
		{
			num += num2;
		}
		int a = Mathf.FloorToInt((float)num / (float)num2);
		a = Mathf.Min(a, 5);
		if (a != m_currentForm)
		{
			m_currentForm = a;
			TransformToForm(m_currentForm);
		}
	}

	private void Update()
	{
		if (m_initialized && !m_gun.CurrentOwner)
		{
			Disengage();
		}
		else if (!m_initialized && (bool)m_gun.CurrentOwner)
		{
			Engage();
		}
		if ((bool)m_gun.CurrentOwner)
		{
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			if (m_synergyActive && !playerController.HasActiveBonusSynergy(CustomSynergyType.POWERHOUSE_OF_THE_CELL))
			{
				m_synergyActive = false;
				UpdateTier(playerController);
			}
			else if (!m_synergyActive && playerController.HasActiveBonusSynergy(CustomSynergyType.POWERHOUSE_OF_THE_CELL))
			{
				m_synergyActive = true;
				UpdateTier(playerController);
			}
		}
		if (m_wasDeserialized && (bool)m_gun && (bool)m_gun.CurrentOwner && m_gun.CurrentOwner.CurrentGun == m_gun)
		{
			m_wasDeserialized = false;
			UpdateTier(m_gun.CurrentOwner as PlayerController);
		}
	}

	private void OnDestroy()
	{
		m_enemiesKilled.Clear();
		m_savedEnemiesKilled = 0;
		Disengage();
	}

	private void Engage()
	{
		m_initialized = true;
		m_player = m_gun.CurrentOwner as PlayerController;
		m_player.OnKilledEnemyContext += KilledEnemyContext;
	}

	private void Disengage()
	{
		if ((bool)m_player)
		{
			m_player.OnKilledEnemyContext -= KilledEnemyContext;
		}
		m_player = null;
		m_initialized = false;
	}

	private void TransformToForm(int targetForm)
	{
		switch (targetForm)
		{
		case 0:
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(Form01ID) as Gun);
			break;
		case 1:
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(Form02ID) as Gun);
			break;
		case 2:
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(Form03ID) as Gun);
			break;
		case 3:
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(Form04ID) as Gun);
			break;
		case 4:
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(Form05ID) as Gun);
			break;
		case 5:
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(Form06ID) as Gun);
			break;
		}
		m_gun.GainAmmo(m_gun.AdjustedMaxAmmo);
	}

	public void InheritData(Gun sourceGun)
	{
		EvolverGunController component = sourceGun.GetComponent<EvolverGunController>();
		if ((bool)component)
		{
			m_savedEnemiesKilled = component.m_savedEnemiesKilled;
			m_enemiesKilled = component.m_enemiesKilled;
		}
	}

	public void MidGameSerialize(List<object> data, int dataIndex)
	{
		data.Add(m_savedEnemiesKilled + m_enemiesKilled.Count);
	}

	public void MidGameDeserialize(List<object> data, ref int dataIndex)
	{
		m_savedEnemiesKilled = (int)data[dataIndex];
		dataIndex++;
		m_wasDeserialized = true;
	}
}
