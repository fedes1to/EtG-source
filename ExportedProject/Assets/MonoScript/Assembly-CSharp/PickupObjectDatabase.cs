using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupObjectDatabase : ObjectDatabase<PickupObject>
{
	private static PickupObjectDatabase m_instance;

	public static PickupObjectDatabase Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = BraveResources.Load<PickupObjectDatabase>("PickupObjectDatabase", ".asset");
			}
			return m_instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance != null;
		}
	}

	public static void Unload()
	{
		m_instance = null;
		Resources.UnloadAsset(m_instance);
	}

	public static int GetId(PickupObject obj)
	{
		return Instance.InternalGetId(obj);
	}

	public static PickupObject GetById(int id)
	{
		return Instance.InternalGetById(id);
	}

	public static PickupObject GetByName(string name)
	{
		return Instance.InternalGetByName(name);
	}

	public static Gun GetRandomGun()
	{
		List<Gun> list = new List<Gun>();
		for (int i = 0; i < Instance.Objects.Count; i++)
		{
			if (Instance.Objects[i] != null && Instance.Objects[i] is Gun && Instance.Objects[i].quality != PickupObject.ItemQuality.EXCLUDED && Instance.Objects[i].quality != PickupObject.ItemQuality.SPECIAL && Instance.Objects[i].contentSource != ContentSource.EXCLUDED && !(Instance.Objects[i] is ContentTeaserGun))
			{
				EncounterTrackable component = Instance.Objects[i].GetComponent<EncounterTrackable>();
				if ((bool)component && component.PrerequisitesMet())
				{
					list.Add(Instance.Objects[i] as Gun);
				}
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static Gun GetRandomStartingGun(System.Random usedRandom)
	{
		List<Gun> list = new List<Gun>();
		for (int i = 0; i < Instance.Objects.Count; i++)
		{
			if (Instance.Objects[i] != null && Instance.Objects[i] is Gun && Instance.Objects[i].quality != PickupObject.ItemQuality.EXCLUDED && !(Instance.Objects[i] is ContentTeaserGun) && (Instance.Objects[i] as Gun).StarterGunForAchievement && (Instance.Objects[i] as Gun).InfiniteAmmo)
			{
				EncounterTrackable component = Instance.Objects[i].GetComponent<EncounterTrackable>();
				if ((bool)component && component.PrerequisitesMet())
				{
					list.Add(Instance.Objects[i] as Gun);
				}
			}
		}
		return list[usedRandom.Next(list.Count)];
	}

	public static Gun GetRandomGunOfQualities(System.Random usedRandom, List<int> excludedIDs, params PickupObject.ItemQuality[] qualities)
	{
		List<Gun> list = new List<Gun>();
		for (int i = 0; i < Instance.Objects.Count; i++)
		{
			if (Instance.Objects[i] != null && Instance.Objects[i] is Gun && Instance.Objects[i].quality != PickupObject.ItemQuality.EXCLUDED && Instance.Objects[i].quality != PickupObject.ItemQuality.SPECIAL && !(Instance.Objects[i] is ContentTeaserGun) && Array.IndexOf(qualities, Instance.Objects[i].quality) != -1 && !excludedIDs.Contains(Instance.Objects[i].PickupObjectId) && (Instance.Objects[i].PickupObjectId != GlobalItemIds.UnfinishedGun || !GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE)))
			{
				EncounterTrackable component = Instance.Objects[i].GetComponent<EncounterTrackable>();
				if ((bool)component && component.PrerequisitesMet())
				{
					list.Add(Instance.Objects[i] as Gun);
				}
			}
		}
		int num = usedRandom.Next(list.Count);
		if (num < 0 || num >= list.Count)
		{
			return null;
		}
		return list[num];
	}

	public static PassiveItem GetRandomPassiveOfQualities(System.Random usedRandom, List<int> excludedIDs, params PickupObject.ItemQuality[] qualities)
	{
		List<PassiveItem> list = new List<PassiveItem>();
		for (int i = 0; i < Instance.Objects.Count; i++)
		{
			if (Instance.Objects[i] != null && Instance.Objects[i] is PassiveItem && Instance.Objects[i].quality != PickupObject.ItemQuality.EXCLUDED && Instance.Objects[i].quality != PickupObject.ItemQuality.SPECIAL && !(Instance.Objects[i] is ContentTeaserItem) && Array.IndexOf(qualities, Instance.Objects[i].quality) != -1 && !excludedIDs.Contains(Instance.Objects[i].PickupObjectId))
			{
				EncounterTrackable component = Instance.Objects[i].GetComponent<EncounterTrackable>();
				if ((bool)component && component.PrerequisitesMet())
				{
					list.Add(Instance.Objects[i] as PassiveItem);
				}
			}
		}
		int num = usedRandom.Next(list.Count);
		if (num < 0 || num >= list.Count)
		{
			return null;
		}
		return list[num];
	}

	public static PickupObject GetByEncounterName(string name)
	{
		for (int i = 0; i < Instance.Objects.Count; i++)
		{
			PickupObject pickupObject = Instance.Objects[i];
			if (!pickupObject)
			{
				continue;
			}
			EncounterTrackable encounterTrackable = pickupObject.encounterTrackable;
			if ((bool)encounterTrackable)
			{
				string primaryDisplayName = encounterTrackable.journalData.GetPrimaryDisplayName();
				if (primaryDisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return pickupObject;
				}
			}
		}
		return null;
	}
}
