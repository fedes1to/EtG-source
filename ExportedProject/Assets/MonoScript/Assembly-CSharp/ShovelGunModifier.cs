using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ShovelGunModifier : MonoBehaviour, IGunInheritable
{
	public GameObject HoleVFX;

	public GenericLootTable GoodDigLootTable;

	public GenericLootTable SynergyGoodDigLootTable;

	public GenericLootTable BadDigLootTable;

	public GenericLootTable SynergyBadDigLootTable;

	public int NumberOfGoodDigs = 5;

	public int NumberOfGoodDigsAddedBySynergy = 5;

	public bool WeightedByShotsRemaining = true;

	public bool OnlyOnEmptyReload;

	private Gun m_gun;

	private bool m_alreadyRolledReward;

	private int m_goodDigsUsed;

	private bool m_wasReloading;

	private RoomHandler m_lastRoomDugGood;

	public void Start()
	{
		m_gun = GetComponent<Gun>();
		m_gun.LockedHorizontalOnReload = true;
		if (OnlyOnEmptyReload)
		{
			Gun gun = m_gun;
			gun.OnAutoReload = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnAutoReload, new Action<PlayerController, Gun>(HandleReloadPressedSimple));
		}
		else
		{
			Gun gun2 = m_gun;
			gun2.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun2.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
		}
	}

	private void Update()
	{
		if (m_wasReloading && (bool)m_gun && !m_gun.IsReloading)
		{
			m_wasReloading = false;
		}
		if ((bool)m_gun && m_gun.CurrentOwner != null && m_gun.ClipShotsRemaining > 0)
		{
			m_alreadyRolledReward = false;
		}
	}

	private void HandleReloadPressedSimple(PlayerController ownerPlayer, Gun sourceGun)
	{
		HandleReloadPressed(ownerPlayer, sourceGun, false);
	}

	private void HandleReloadPressed(PlayerController ownerPlayer, Gun sourceGun, bool something)
	{
		if (sourceGun.IsReloading)
		{
			if (!m_wasReloading)
			{
				m_wasReloading = true;
				ownerPlayer.StartCoroutine(HandleDig(sourceGun));
			}
		}
		else
		{
			m_wasReloading = false;
		}
	}

	private IEnumerator HandleDig(Gun sourceGun)
	{
		float lootChanceMultiplier = 1f;
		if (WeightedByShotsRemaining)
		{
			float num = (float)sourceGun.ClipShotsRemaining * 1f / (float)sourceGun.ClipCapacity;
			if (UnityEngine.Random.value < num)
			{
				lootChanceMultiplier = 0f;
			}
		}
		float elapsed = 0f;
		while (elapsed < 0.75f)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		Vector2 offset = new Vector3(0f, -1.125f);
		SpawnManager.SpawnVFX(HoleVFX, m_gun.barrelOffset.position.XY() + offset, Quaternion.identity);
		if (lootChanceMultiplier > 0f && !m_alreadyRolledReward)
		{
			GenericLootTable genericLootTable = BadDigLootTable;
			bool flag = (bool)sourceGun && sourceGun.OwnerHasSynergy(CustomSynergyType.TWO_KINDS_OF_PEOPLE);
			bool flag2 = m_goodDigsUsed < ((!flag) ? NumberOfGoodDigs : (NumberOfGoodDigs + NumberOfGoodDigsAddedBySynergy)) && UnityEngine.Random.value > 0.5f;
			if (flag2 && (bool)sourceGun && (bool)sourceGun.CurrentOwner)
			{
				RoomHandler currentRoom = (sourceGun.CurrentOwner as PlayerController).CurrentRoom;
				if (currentRoom == m_lastRoomDugGood)
				{
					flag2 = false;
				}
				else
				{
					m_lastRoomDugGood = currentRoom;
				}
			}
			if (flag)
			{
				if (flag2)
				{
					m_goodDigsUsed++;
					genericLootTable = SynergyGoodDigLootTable;
				}
				else
				{
					genericLootTable = SynergyBadDigLootTable;
				}
			}
			else if (flag2)
			{
				m_goodDigsUsed++;
				genericLootTable = GoodDigLootTable;
			}
			GameObject gameObject = genericLootTable.SelectByWeight();
			if ((bool)gameObject)
			{
				LootEngine.SpawnItem(gameObject, m_gun.barrelOffset.position.XY() + offset, Vector2.zero, 0f);
			}
		}
		m_alreadyRolledReward = true;
	}

	public void InheritData(Gun sourceGun)
	{
		ShovelGunModifier component = sourceGun.GetComponent<ShovelGunModifier>();
		if ((bool)component)
		{
			m_goodDigsUsed = component.m_goodDigsUsed;
		}
	}

	public void MidGameSerialize(List<object> data, int dataIndex)
	{
		data.Add(m_goodDigsUsed);
	}

	public void MidGameDeserialize(List<object> data, ref int dataIndex)
	{
		m_goodDigsUsed = (int)data[dataIndex];
		dataIndex++;
	}
}
