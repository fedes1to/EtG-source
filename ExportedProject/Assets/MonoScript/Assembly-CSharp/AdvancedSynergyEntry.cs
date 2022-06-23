using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AdvancedSynergyEntry
{
	public string NameKey;

	public SynergyEntry.SynergyActivation ActivationStatus;

	[PickupIdentifier]
	public List<int> MandatoryGunIDs = new List<int>();

	[PickupIdentifier]
	public List<int> MandatoryItemIDs = new List<int>();

	[PickupIdentifier]
	public List<int> OptionalGunIDs = new List<int>();

	[PickupIdentifier]
	public List<int> OptionalItemIDs = new List<int>();

	public int NumberObjectsRequired = 2;

	public bool ActiveWhenGunUnequipped;

	public bool SuppressVFX;

	public bool RequiresAtLeastOneGunAndOneItem;

	public bool IgnoreLichEyeBullets;

	public List<StatModifier> statModifiers;

	[LongNumericEnum]
	public List<CustomSynergyType> bonusSynergies;

	public bool SynergyIsActive(PlayerController p, PlayerController p2)
	{
		if ((MandatoryGunIDs.Count > 0 || (RequiresAtLeastOneGunAndOneItem && OptionalGunIDs.Count > 0)) && !ActiveWhenGunUnequipped)
		{
			if ((bool)p && (bool)p.CurrentGun && (MandatoryGunIDs.Contains(p.CurrentGun.PickupObjectId) || OptionalGunIDs.Contains(p.CurrentGun.PickupObjectId)))
			{
				return true;
			}
			if ((bool)p2 && (bool)p2.CurrentGun && (MandatoryGunIDs.Contains(p2.CurrentGun.PickupObjectId) || OptionalGunIDs.Contains(p2.CurrentGun.PickupObjectId)))
			{
				return true;
			}
			if ((bool)p && (bool)p.CurrentSecondaryGun && (MandatoryGunIDs.Contains(p.CurrentSecondaryGun.PickupObjectId) || OptionalGunIDs.Contains(p.CurrentSecondaryGun.PickupObjectId)))
			{
				return true;
			}
			if ((bool)p2 && (bool)p2.CurrentSecondaryGun && (MandatoryGunIDs.Contains(p2.CurrentSecondaryGun.PickupObjectId) || OptionalGunIDs.Contains(p2.CurrentSecondaryGun.PickupObjectId)))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public bool ContainsPickup(int id)
	{
		return MandatoryGunIDs.Contains(id) || MandatoryItemIDs.Contains(id) || OptionalGunIDs.Contains(id) || OptionalItemIDs.Contains(id);
	}

	private bool PlayerHasSynergyCompletionItem(PlayerController p)
	{
		if ((bool)p)
		{
			for (int i = 0; i < p.passiveItems.Count; i++)
			{
				if (p.passiveItems[i] is SynergyCompletionItem)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool PlayerHasPickup(PlayerController p, int pickupID)
	{
		if ((bool)p && p.inventory != null && p.inventory.AllGuns != null)
		{
			for (int i = 0; i < p.inventory.AllGuns.Count; i++)
			{
				if (p.inventory.AllGuns[i].PickupObjectId == pickupID)
				{
					return true;
				}
			}
		}
		if ((bool)p)
		{
			for (int j = 0; j < p.activeItems.Count; j++)
			{
				if (p.activeItems[j].PickupObjectId == pickupID)
				{
					return true;
				}
			}
			for (int k = 0; k < p.passiveItems.Count; k++)
			{
				if (p.passiveItems[k].PickupObjectId == pickupID)
				{
					return true;
				}
			}
			if (pickupID == GlobalItemIds.Map && p.EverHadMap)
			{
				return true;
			}
		}
		return false;
	}

	public bool SynergyIsAvailable(PlayerController p, PlayerController p2, int additionalID = -1)
	{
		if (ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE)
		{
			return false;
		}
		if (ActivationStatus == SynergyEntry.SynergyActivation.DEMO)
		{
			return false;
		}
		bool flag = PlayerHasSynergyCompletionItem(p) || PlayerHasSynergyCompletionItem(p2);
		if (IgnoreLichEyeBullets)
		{
			flag = false;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < MandatoryGunIDs.Count; i++)
		{
			if (PlayerHasPickup(p, MandatoryGunIDs[i]) || PlayerHasPickup(p2, MandatoryGunIDs[i]) || MandatoryGunIDs[i] == additionalID)
			{
				num++;
			}
		}
		for (int j = 0; j < MandatoryItemIDs.Count; j++)
		{
			if (PlayerHasPickup(p, MandatoryItemIDs[j]) || PlayerHasPickup(p2, MandatoryItemIDs[j]) || MandatoryItemIDs[j] == additionalID)
			{
				num2++;
			}
		}
		int num3 = 0;
		int num4 = 0;
		for (int k = 0; k < OptionalGunIDs.Count; k++)
		{
			if (PlayerHasPickup(p, OptionalGunIDs[k]) || PlayerHasPickup(p2, OptionalGunIDs[k]) || OptionalGunIDs[k] == additionalID)
			{
				num3++;
			}
		}
		for (int l = 0; l < OptionalItemIDs.Count; l++)
		{
			if (PlayerHasPickup(p, OptionalItemIDs[l]) || PlayerHasPickup(p2, OptionalItemIDs[l]) || OptionalItemIDs[l] == additionalID)
			{
				num4++;
			}
		}
		bool flag2 = MandatoryItemIDs.Count > 0 && MandatoryGunIDs.Count == 0 && OptionalGunIDs.Count > 0 && OptionalItemIDs.Count == 0;
		if (((MandatoryGunIDs.Count > 0 && num > 0) || (flag2 && num3 > 0)) && flag)
		{
			num++;
			num2++;
		}
		if (num < MandatoryGunIDs.Count || num2 < MandatoryItemIDs.Count)
		{
			return false;
		}
		int num5 = MandatoryItemIDs.Count + MandatoryGunIDs.Count + num3 + num4;
		int num6 = MandatoryGunIDs.Count + num3;
		int num7 = MandatoryItemIDs.Count + num4;
		if (num6 > 0 && (MandatoryGunIDs.Count > 0 || flag2 || (RequiresAtLeastOneGunAndOneItem && num6 > 0)) && flag)
		{
			num7++;
			num6++;
			num5 += 2;
		}
		if (RequiresAtLeastOneGunAndOneItem && OptionalGunIDs.Count + MandatoryGunIDs.Count > 0 && OptionalItemIDs.Count + MandatoryItemIDs.Count > 0 && (num6 < 1 || num7 < 1))
		{
			return false;
		}
		int num8 = Mathf.Max(2, NumberObjectsRequired);
		return num5 >= num8;
	}
}
