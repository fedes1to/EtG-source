using System;
using System.Collections.Generic;
using UnityEngine;

public class RingOfResourcefulRatItem : PassiveItem, ILevelLoadedListener
{
	private bool m_initializedEver;

	public override void Pickup(PlayerController player)
	{
		if (m_pickedUp)
		{
			return;
		}
		if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
		{
			PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
		}
		if (!m_initializedEver)
		{
			if (!PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[player].Add(GetType(), 1);
			}
			else
			{
				PassiveItem.ActiveFlagItems[player][GetType()] = PassiveItem.ActiveFlagItems[player][GetType()] + 1;
			}
			m_initializedEver = true;
		}
		base.Pickup(player);
	}

	protected override void Update()
	{
		base.Update();
	}

	public void BraveOnLevelWasLoaded()
	{
		if ((bool)m_owner)
		{
			if (!PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[m_owner].Add(GetType(), 1);
			}
			else
			{
				PassiveItem.ActiveFlagItems[m_owner][GetType()] = PassiveItem.ActiveFlagItems[m_owner][GetType()] + 1;
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if (PassiveItem.ActiveFlagItems.ContainsKey(player) && PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		debrisObject.GetComponent<RingOfResourcefulRatItem>().m_pickedUpThisRun = true;
		debrisObject.GetComponent<RingOfResourcefulRatItem>().m_initializedEver = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		BraveTime.ClearMultiplier(base.gameObject);
		if (m_pickedUp && PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
		base.OnDestroy();
	}
}
