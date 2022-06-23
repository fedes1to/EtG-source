using System;
using System.Collections.Generic;
using UnityEngine;

public class MulticompanionItem : PassiveItem
{
	[EnemyIdentifier]
	public string CompanionGuid;

	public bool HasSynergy;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	[EnemyIdentifier]
	public string SynergyCompanionGuid;

	public int SynergyMaxNumberOfCompanions = 3;

	public int MaxNumberOfCompanions = 8;

	public bool TriggersOnRoomClear;

	public bool TriggersOnEnemyKill;

	private List<CompanionController> m_companions = new List<CompanionController>();

	private bool m_synergyActive;

	private void CreateNewCompanion(PlayerController owner)
	{
		int num = ((!HasSynergy || !m_synergyActive) ? MaxNumberOfCompanions : SynergyMaxNumberOfCompanions);
		if (m_companions.Count < num || num < 0)
		{
			string guid = ((!HasSynergy || !m_synergyActive) ? CompanionGuid : SynergyCompanionGuid);
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(guid);
			Vector3 position = owner.transform.position;
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				position += new Vector3(1.125f, -0.3125f, 0f);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(orLoadByGuid.gameObject, position, Quaternion.identity);
			CompanionController orAddComponent = gameObject.GetOrAddComponent<CompanionController>();
			m_companions.Add(orAddComponent);
			orAddComponent.Initialize(owner);
			if ((bool)orAddComponent.specRigidbody)
			{
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(orAddComponent.specRigidbody);
			}
		}
	}

	private void DestroyAllCompanions()
	{
		for (int num = m_companions.Count - 1; num >= 0; num--)
		{
			if ((bool)m_companions[num])
			{
				UnityEngine.Object.Destroy(m_companions[num].gameObject);
			}
			m_companions.RemoveAt(num);
		}
	}

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		if (TriggersOnRoomClear)
		{
			player.OnRoomClearEvent += HandleRoomCleared;
		}
		if (TriggersOnEnemyKill)
		{
			player.OnKilledEnemy += HandleEnemyKilled;
		}
		CreateNewCompanion(player);
	}

	private void HandleEnemyKilled(PlayerController p)
	{
		CreateNewCompanion(p);
	}

	private void HandleRoomCleared(PlayerController p)
	{
		CreateNewCompanion(p);
	}

	protected override void Update()
	{
		base.Update();
		for (int num = m_companions.Count - 1; num >= 0; num--)
		{
			if (!m_companions[num])
			{
				m_companions.RemoveAt(num);
			}
			else if ((bool)m_companions[num].healthHaver && m_companions[num].healthHaver.IsDead)
			{
				UnityEngine.Object.Destroy(m_companions[num].gameObject);
				m_companions.RemoveAt(num);
			}
		}
		if ((bool)m_owner && HasSynergy)
		{
			if (m_synergyActive && !m_owner.HasActiveBonusSynergy(RequiredSynergy))
			{
				DestroyAllCompanions();
				m_synergyActive = false;
			}
			else if (!m_synergyActive && m_owner.HasActiveBonusSynergy(RequiredSynergy))
			{
				DestroyAllCompanions();
				m_synergyActive = true;
			}
		}
	}

	private void HandleNewFloor(PlayerController obj)
	{
		DestroyAllCompanions();
		CreateNewCompanion(obj);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DestroyAllCompanions();
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		player.OnRoomClearEvent -= HandleRoomCleared;
		player.OnKilledEnemy -= HandleEnemyKilled;
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			PlayerController owner = m_owner;
			owner.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(owner.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
			m_owner.OnRoomClearEvent -= HandleRoomCleared;
			m_owner.OnKilledEnemy -= HandleEnemyKilled;
		}
		DestroyAllCompanions();
		base.OnDestroy();
	}
}
