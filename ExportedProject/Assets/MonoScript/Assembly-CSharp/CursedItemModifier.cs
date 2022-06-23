using System;
using System.Collections.Generic;
using UnityEngine;

public class CursedItemModifier : MonoBehaviour
{
	private PickupObject m_pickup;

	private StatModifier m_addedModifier;

	private void Start()
	{
		m_pickup = GetComponent<PickupObject>();
		if (m_pickup is PassiveItem)
		{
			PassiveItem passiveItem = m_pickup as PassiveItem;
			StatModifier[] array = passiveItem.passiveStatModifiers;
			StatModifier statModifier = new StatModifier();
			statModifier.amount = 1f;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			statModifier.statToBoost = PlayerStats.StatType.Curse;
			Array.Resize(ref array, array.Length + 1);
			m_addedModifier = statModifier;
			array[array.Length - 1] = statModifier;
			if (passiveItem.Owner != null)
			{
				passiveItem.Owner.stats.RecalculateStats(passiveItem.Owner);
			}
		}
		else if (!(m_pickup is PlayerItem))
		{
		}
	}

	private void OnDestroy()
	{
		if (m_pickup is PassiveItem)
		{
			PassiveItem passiveItem = m_pickup as PassiveItem;
			StatModifier[] passiveStatModifiers = passiveItem.passiveStatModifiers;
			List<StatModifier> list = new List<StatModifier>(passiveStatModifiers);
			bool flag = list.Remove(m_addedModifier);
			passiveItem.passiveStatModifiers = list.ToArray();
			if (passiveItem.Owner != null)
			{
				passiveItem.Owner.stats.RecalculateStats(passiveItem.Owner);
			}
		}
	}
}
