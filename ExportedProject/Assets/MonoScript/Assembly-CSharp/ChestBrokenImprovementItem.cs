using System;
using System.Collections.Generic;
using UnityEngine;

public class ChestBrokenImprovementItem : PassiveItem
{
	public static float PickupQualChance;

	public static float MinusOneQualChance = 0.5f;

	public static float EqualQualChance = 0.45f;

	public static float PlusQualChance = 0.05f;

	public float ChanceForPickupQuality;

	public float ChanceForMinusOneQuality = 0.5f;

	public float ChanceForEqualQuality = 0.45f;

	public float ChanceForPlusOneQuality = 0.05f;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			float num = ChanceForPickupQuality + ChanceForMinusOneQuality + ChanceForEqualQuality + ChanceForPlusOneQuality;
			PickupQualChance = ChanceForPickupQuality / num;
			MinusOneQualChance = ChanceForMinusOneQuality / num;
			EqualQualChance = ChanceForEqualQuality / num;
			PlusQualChance = ChanceForPlusOneQuality / num;
			if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
			{
				PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
			}
			if (!PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[player].Add(GetType(), 1);
			}
			else
			{
				PassiveItem.ActiveFlagItems[player][GetType()] = PassiveItem.ActiveFlagItems[player][GetType()] + 1;
			}
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if (PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		if ((bool)debrisObject && (bool)debrisObject.GetComponent<ChestBrokenImprovementItem>())
		{
			debrisObject.GetComponent<ChestBrokenImprovementItem>().m_pickedUpThisRun = true;
		}
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		BraveTime.ClearMultiplier(base.gameObject);
		if (m_pickedUp && PassiveItem.ActiveFlagItems != null && PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
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
