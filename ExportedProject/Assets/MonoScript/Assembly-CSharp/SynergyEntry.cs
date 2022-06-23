using System;
using System.Collections.Generic;

[Serializable]
public class SynergyEntry
{
	public enum SynergyActivation
	{
		ACTIVE,
		DEMO,
		INACTIVE,
		ACTIVE_UNBOOSTED
	}

	public string NameKey;

	public SynergyActivation ActivationStatus;

	[PickupIdentifier]
	public List<int> gunIDs;

	[PickupIdentifier]
	public List<int> itemIDs;

	public bool GunsOR;

	public bool ItemsOR;

	public bool ActiveWhenGunUnequipped;

	public bool SuppressVFX;

	public int ExtraItemsOrForBrents;

	public List<StatModifier> statModifiers;

	[LongNumericEnum]
	public List<CustomSynergyType> bonusSynergies;

	public bool SynergyIsActive(PlayerController p, PlayerController p2)
	{
		if (gunIDs.Count > 0 && !ActiveWhenGunUnequipped)
		{
			if ((bool)p && (bool)p.CurrentGun && gunIDs.Contains(p.CurrentGun.PickupObjectId))
			{
				return true;
			}
			if ((bool)p2 && (bool)p2.CurrentGun && gunIDs.Contains(p2.CurrentGun.PickupObjectId))
			{
				return true;
			}
			if ((bool)p && (bool)p.CurrentSecondaryGun && gunIDs.Contains(p.CurrentSecondaryGun.PickupObjectId))
			{
				return true;
			}
			if ((bool)p2 && (bool)p2.CurrentSecondaryGun && gunIDs.Contains(p.CurrentSecondaryGun.PickupObjectId))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public bool SynergyIsAvailable(PlayerController p, PlayerController p2)
	{
		if (ActivationStatus == SynergyActivation.INACTIVE)
		{
			return false;
		}
		if (ActivationStatus == SynergyActivation.DEMO)
		{
			return false;
		}
		bool flag = true;
		bool flag2 = true;
		if (gunIDs.Count > 0)
		{
			if (GunsOR)
			{
				flag = false;
			}
			for (int i = 0; i < gunIDs.Count; i++)
			{
				bool flag3 = false;
				if ((bool)p && p.inventory != null && p.inventory.AllGuns != null)
				{
					for (int j = 0; j < p.inventory.AllGuns.Count; j++)
					{
						if (p.inventory.AllGuns[j].PickupObjectId == gunIDs[i])
						{
							flag3 = true;
							break;
						}
					}
				}
				if ((bool)p2 && p2.inventory != null && p2.inventory.AllGuns != null)
				{
					for (int k = 0; k < p2.inventory.AllGuns.Count; k++)
					{
						if (p2.inventory.AllGuns[k].PickupObjectId == gunIDs[i])
						{
							flag3 = true;
							break;
						}
					}
				}
				if (flag3 && GunsOR)
				{
					flag = true;
					break;
				}
				if (!flag3 && !GunsOR)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		if (itemIDs.Count > 0)
		{
			if (ItemsOR)
			{
				flag2 = false;
			}
			int num = 0;
			for (int l = 0; l < itemIDs.Count; l++)
			{
				bool flag4 = false;
				if ((bool)p)
				{
					for (int m = 0; m < p.activeItems.Count; m++)
					{
						if (p.activeItems[m].PickupObjectId == itemIDs[l])
						{
							flag4 = true;
							break;
						}
					}
					for (int n = 0; n < p.passiveItems.Count; n++)
					{
						if (p.passiveItems[n].PickupObjectId == itemIDs[l])
						{
							flag4 = true;
							break;
						}
					}
					if (itemIDs[l] == GlobalItemIds.Map && p.EverHadMap)
					{
						flag4 = true;
						break;
					}
				}
				if ((bool)p2)
				{
					for (int num2 = 0; num2 < p2.activeItems.Count; num2++)
					{
						if (p2.activeItems[num2].PickupObjectId == itemIDs[l])
						{
							flag4 = true;
							break;
						}
					}
					for (int num3 = 0; num3 < p2.passiveItems.Count; num3++)
					{
						if (p2.passiveItems[num3].PickupObjectId == itemIDs[l])
						{
							flag4 = true;
							break;
						}
					}
					if (itemIDs[l] == GlobalItemIds.Map && p2.EverHadMap)
					{
						flag4 = true;
						break;
					}
				}
				if (flag4 && ItemsOR)
				{
					num++;
				}
				if (!flag4 && !ItemsOR)
				{
					flag2 = false;
					break;
				}
			}
			if (ItemsOR && num > ExtraItemsOrForBrents)
			{
				flag2 = true;
			}
		}
		return flag && flag2;
	}
}
