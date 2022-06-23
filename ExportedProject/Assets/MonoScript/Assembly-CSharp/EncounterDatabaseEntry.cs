using System;
using FullInspector;
using UnityEngine;

[Serializable]
public class EncounterDatabaseEntry : AssetBundleDatabaseEntry
{
	[InspectorDisabled]
	public string ProxyEncounterGuid;

	[InspectorDisabled]
	public bool doNotificationOnEncounter;

	[InspectorDisabled]
	public bool IgnoreDifferentiator;

	[InspectorDisabled]
	public DungeonPrerequisite[] prerequisites;

	[InspectorDisabled]
	public JournalEntry journalData;

	[InspectorDisabled]
	public bool usesPurpleNotifications;

	[InspectorDisabled]
	public int pickupObjectId = -1;

	[InspectorDisabled]
	public int shootStyleInt = -1;

	[InspectorDisabled]
	public bool isPlayerItem;

	[InspectorDisabled]
	public bool isPassiveItem;

	[InspectorDisabled]
	public bool isInfiniteAmmoGun;

	[InspectorDisabled]
	public bool doesntDamageSecretWalls;

	[InspectorDisabled]
	public bool ForceEncounterState { get; set; }

	public override AssetBundle assetBundle
	{
		get
		{
			return EncounterDatabase.AssetBundle;
		}
	}

	public EncounterDatabaseEntry()
	{
	}

	public EncounterDatabaseEntry(EncounterTrackable encounterTrackable)
	{
		myGuid = encounterTrackable.EncounterGuid;
		SetAll(encounterTrackable);
	}

	public bool PrerequisitesMet()
	{
		if (prerequisites == null || prerequisites.Length == 0)
		{
			return true;
		}
		if (GameStatsManager.Instance.IsForceUnlocked(myGuid))
		{
			return true;
		}
		for (int i = 0; i < prerequisites.Length; i++)
		{
			if (!prerequisites[i].CheckConditionsFulfilled())
			{
				return false;
			}
		}
		return true;
	}

	public string GetSecondTapeDescriptor()
	{
		string result = string.Empty;
		if (shootStyleInt >= 0)
		{
			result = GetShootStyleString((ProjectileModule.ShootStyle)shootStyleInt);
		}
		else if (isPlayerItem)
		{
			result = StringTableManager.GetItemsString("#ITEMSTYLE_ACTIVE");
		}
		else if (isPassiveItem)
		{
			result = StringTableManager.GetItemsString("#ITEMSTYLE_PASSIVE");
		}
		return result;
	}

	public string GetModifiedLongDescription()
	{
		return journalData.GetAmmonomiconFullEntry(isInfiniteAmmoGun, doesntDamageSecretWalls);
	}

	private string GetShootStyleString(ProjectileModule.ShootStyle shootStyle)
	{
		switch (shootStyle)
		{
		case ProjectileModule.ShootStyle.Automatic:
			return StringTableManager.GetItemsString("#SHOOTSTYLE_AUTOMATIC");
		case ProjectileModule.ShootStyle.Beam:
			return StringTableManager.GetItemsString("#SHOOTSTYLE_BEAM");
		case ProjectileModule.ShootStyle.Burst:
			return StringTableManager.GetItemsString("#SHOOTSTYLE_BURST");
		case ProjectileModule.ShootStyle.Charged:
			return StringTableManager.GetItemsString("#SHOOTSTYLE_CHARGE");
		case ProjectileModule.ShootStyle.SemiAutomatic:
			return StringTableManager.GetItemsString("#SHOOTSTYLE_SEMIAUTOMATIC");
		default:
			return string.Empty;
		}
	}

	public void SetAll(EncounterTrackable encounterTrackable)
	{
		ProxyEncounterGuid = encounterTrackable.ProxyEncounterGuid;
		doNotificationOnEncounter = encounterTrackable.DoNotificationOnEncounter;
		IgnoreDifferentiator = encounterTrackable.IgnoreDifferentiator;
		prerequisites = encounterTrackable.prerequisites;
		journalData = encounterTrackable.journalData.Clone();
		usesPurpleNotifications = encounterTrackable.UsesPurpleNotifications;
		PickupObject component = encounterTrackable.GetComponent<PickupObject>();
		pickupObjectId = ((!component) ? (-1) : component.PickupObjectId);
		Gun gun = component as Gun;
		shootStyleInt = (int)((!gun) ? ((ProjectileModule.ShootStyle)(-1)) : gun.DefaultModule.shootStyle);
		isPlayerItem = encounterTrackable.GetComponent<PlayerItem>();
		isPassiveItem = encounterTrackable.GetComponent<PassiveItem>();
		isInfiniteAmmoGun = (bool)gun && gun.InfiniteAmmo;
		doesntDamageSecretWalls = (bool)gun && gun.InfiniteAmmo;
	}

	public bool Equals(EncounterTrackable other)
	{
		if (other == null)
		{
			return false;
		}
		if (ProxyEncounterGuid != other.ProxyEncounterGuid || doNotificationOnEncounter != other.DoNotificationOnEncounter || IgnoreDifferentiator != other.IgnoreDifferentiator || !PrereqArraysEqual(prerequisites, other.prerequisites) || !journalData.Equals(other.journalData) || usesPurpleNotifications != other.UsesPurpleNotifications)
		{
			return false;
		}
		PickupObject component = other.GetComponent<PickupObject>();
		int num = ((!component) ? (-1) : component.PickupObjectId);
		if (pickupObjectId != num)
		{
			return false;
		}
		Gun gun = component as Gun;
		int num2 = (int)((!gun) ? ((ProjectileModule.ShootStyle)(-1)) : gun.DefaultModule.shootStyle);
		if (shootStyleInt != num2)
		{
			return false;
		}
		if (isPlayerItem != (bool)other.GetComponent<PlayerItem>())
		{
			return false;
		}
		if (isPassiveItem != (bool)other.GetComponent<PassiveItem>())
		{
			return false;
		}
		if (isInfiniteAmmoGun != ((bool)gun && gun.InfiniteAmmo))
		{
			return false;
		}
		if (doesntDamageSecretWalls != ((bool)gun && gun.InfiniteAmmo))
		{
			return false;
		}
		return true;
	}

	private static bool PrereqArraysEqual(DungeonPrerequisite[] a, DungeonPrerequisite[] b)
	{
		if (a == null && b != null)
		{
			return false;
		}
		if (b == null && a != null)
		{
			return false;
		}
		if (a.Length != b.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (!a[i].Equals(b[i]))
			{
				return false;
			}
		}
		return true;
	}
}
