using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class LootData
{
	public GenericLootTable lootTable;

	public List<GenericLootTable> overrideItemLootTables;

	[NonSerialized]
	public List<PickupObject.ItemQuality> overrideItemQualities;

	public float Common_Chance;

	public float D_Chance;

	public float C_Chance;

	public float B_Chance;

	public float A_Chance;

	public float S_Chance;

	public bool CompletesSynergy;

	public bool canDropMultipleItems;

	public bool onlyOneGunCanDrop = true;

	[ShowInInspectorIf("canDropMultipleItems", false)]
	public WeightedIntCollection multipleItemDropChances;

	[NonSerialized]
	public bool ForceNotCommon;

	[NonSerialized]
	public bool PreferGunDrop;

	[NonSerialized]
	public int LastGenerationNumSynergiesCalculated;

	private void ClearPerDropData()
	{
		PreferGunDrop = false;
		ForceNotCommon = false;
	}

	public PickupObject GetSingleItemForPlayer(PlayerController player, int tierShift = 0)
	{
		GameObject itemForPlayer = GetItemForPlayer(player, lootTable, null, tierShift);
		ClearPerDropData();
		if (itemForPlayer != null)
		{
			return itemForPlayer.GetComponent<PickupObject>();
		}
		return null;
	}

	public List<PickupObject> GetItemsForPlayer(PlayerController player, int tierShift = 0, GenericLootTable OverrideDropTable = null, System.Random generatorRandom = null)
	{
		LastGenerationNumSynergiesCalculated = 0;
		List<GameObject> list = new List<GameObject>();
		List<PickupObject> list2 = new List<PickupObject>();
		int num = ((!canDropMultipleItems) ? 1 : multipleItemDropChances.SelectByWeight(generatorRandom));
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = null;
			if (num > 1 && overrideItemLootTables.Count > i && overrideItemLootTables[i] != null)
			{
				PickupObject.ItemQuality? overrideQuality = null;
				if (overrideItemQualities != null && overrideItemQualities.Count > i)
				{
					overrideQuality = overrideItemQualities[i];
				}
				gameObject = GetItemForPlayer(player, overrideItemLootTables[i], list, tierShift, flag, overrideQuality, generatorRandom);
			}
			else
			{
				GenericLootTable tableToUse = lootTable;
				List<GameObject> excludedObjects = list;
				bool excludeGuns = flag;
				gameObject = GetItemForPlayer(player, tableToUse, excludedObjects, tierShift, excludeGuns, null, generatorRandom);
			}
			if (gameObject != null)
			{
				PickupObject component = gameObject.GetComponent<PickupObject>();
				if (component is Gun && onlyOneGunCanDrop)
				{
					flag = true;
				}
				list2.Add(component);
				list.Add(gameObject);
			}
			ClearPerDropData();
		}
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE))
		{
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].PickupObjectId == GlobalItemIds.UnfinishedGun)
				{
					list2[j] = PickupObjectDatabase.GetById(GlobalItemIds.FinishedGun);
				}
			}
		}
		return list2;
	}

	public GameObject GetItemForPlayer(PlayerController player, GenericLootTable tableToUse, List<GameObject> excludedObjects, int tierShift = 0, bool excludeGuns = false, PickupObject.ItemQuality? overrideQuality = null, System.Random generatorRandom = null)
	{
		PickupObject.ItemQuality itemQuality = ((!overrideQuality.HasValue) ? GetTargetItemQuality(player, generatorRandom) : overrideQuality.Value);
		itemQuality = (PickupObject.ItemQuality)Mathf.Min(5, Mathf.Max(0, (int)(itemQuality + tierShift)));
		bool flag = false;
		bool flag2 = GameStatsManager.HasInstance && GameStatsManager.Instance.IsRainbowRun;
		List<int> rainbowRunForceExcludedIDs = GameManager.Instance.RainbowRunForceExcludedIDs;
		List<int> rainbowRunForceIncludedIDs = GameManager.Instance.RainbowRunForceIncludedIDs;
		if (CompletesSynergy)
		{
			SynercacheManager.UseCachedSynergyIDs = true;
		}
		while (itemQuality >= PickupObject.ItemQuality.COMMON)
		{
			if (itemQuality > PickupObject.ItemQuality.COMMON)
			{
				flag = true;
			}
			List<WeightedGameObject> compiledRawItems = tableToUse.GetCompiledRawItems();
			List<KeyValuePair<WeightedGameObject, float>> list = new List<KeyValuePair<WeightedGameObject, float>>();
			float num = 0f;
			List<KeyValuePair<WeightedGameObject, float>> list2 = new List<KeyValuePair<WeightedGameObject, float>>();
			float num2 = 0f;
			for (int i = 0; i < compiledRawItems.Count; i++)
			{
				if (!(compiledRawItems[i].gameObject != null))
				{
					continue;
				}
				PickupObject component = compiledRawItems[i].gameObject.GetComponent<PickupObject>();
				bool flag3 = RewardManager.CheckQualityForItem(component, player, itemQuality, CompletesSynergy, RewardManager.RewardSource.UNSPECIFIED);
				if ((component.ItemSpansBaseQualityTiers || component.ItemRespectsHeartMagnificence) && itemQuality != PickupObject.ItemQuality.D && itemQuality != 0 && itemQuality != PickupObject.ItemQuality.S)
				{
					flag3 = true;
				}
				if (component is SpiceItem && player != null && player.spiceCount > 0)
				{
					flag3 = true;
				}
				if (!(component != null) || !flag3)
				{
					continue;
				}
				bool flag4 = true;
				float num3 = compiledRawItems[i].weight;
				if (excludedObjects != null && excludedObjects.Contains(component.gameObject))
				{
					flag4 = false;
					continue;
				}
				if (flag2)
				{
					if (rainbowRunForceExcludedIDs != null && rainbowRunForceExcludedIDs.Contains(component.PickupObjectId))
					{
						flag4 = false;
						continue;
					}
					if ((itemQuality == PickupObject.ItemQuality.D || itemQuality == PickupObject.ItemQuality.C) && rainbowRunForceIncludedIDs != null && !rainbowRunForceIncludedIDs.Contains(component.PickupObjectId))
					{
						flag4 = false;
						continue;
					}
				}
				if (component is Gun && excludeGuns)
				{
					flag4 = false;
					continue;
				}
				if (!component.PrerequisitesMet())
				{
					flag4 = false;
				}
				if (component is Gun)
				{
					Gun gun = component as Gun;
					if (gun.InfiniteAmmo && !gun.CanBeDropped && gun.quality == PickupObject.ItemQuality.SPECIAL)
					{
						flag4 = false;
						continue;
					}
					GunClass gunClass = gun.gunClass;
					if (gunClass != 0)
					{
						int num4 = ((!(player == null)) ? player.inventory.ContainsGunOfClass(gunClass, true) : 0);
						float modifierForClass = LootDataGlobalSettings.Instance.GetModifierForClass(gunClass);
						num3 *= Mathf.Pow(modifierForClass, num4);
					}
					if (PreferGunDrop)
					{
						num3 *= 1000f;
					}
				}
				float multiplierForItem = RewardManager.GetMultiplierForItem(component, player, CompletesSynergy);
				if (CompletesSynergy && multiplierForItem > 100000f)
				{
					LastGenerationNumSynergiesCalculated++;
				}
				num3 *= multiplierForItem;
				if (RoomHandler.unassignedInteractableObjects != null)
				{
					for (int j = 0; j < RoomHandler.unassignedInteractableObjects.Count; j++)
					{
						IPlayerInteractable playerInteractable = RoomHandler.unassignedInteractableObjects[j];
						if (playerInteractable is PickupObject)
						{
							PickupObject pickupObject = playerInteractable as PickupObject;
							if ((bool)pickupObject && pickupObject.PickupObjectId == component.PickupObjectId)
							{
								flag4 = false;
								num2 += num3;
								KeyValuePair<WeightedGameObject, float> item = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], num3);
								list2.Add(item);
								break;
							}
						}
					}
				}
				if (GameManager.Instance.IsSeeded)
				{
					if (GameManager.Instance.RewardManager.IsItemInSeededManifests(component))
					{
						flag4 = false;
						num2 += num3;
						KeyValuePair<WeightedGameObject, float> item2 = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], num3);
						list2.Add(item2);
					}
				}
				else
				{
					EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
					if (component2 != null)
					{
						int num5 = 0;
						if (Application.isPlaying)
						{
							num5 = GameStatsManager.Instance.QueryEncounterableDifferentiator(component2);
						}
						if (CompletesSynergy)
						{
							num5 = 0;
						}
						if (num5 > 0 || (Application.isPlaying && GameManager.Instance.ExtantShopTrackableGuids.Contains(component2.EncounterGuid)))
						{
							flag4 = false;
							num2 += num3;
							KeyValuePair<WeightedGameObject, float> item3 = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], num3);
							list2.Add(item3);
						}
						else if (Application.isPlaying && GameStatsManager.Instance.QueryEncounterable(component2) == 0 && GameStatsManager.Instance.QueryEncounterableAnnouncement(component2.EncounterGuid))
						{
							num3 *= 10f;
						}
					}
				}
				if (component.ItemSpansBaseQualityTiers || component.ItemRespectsHeartMagnificence)
				{
					if (RewardManager.AdditionalHeartTierMagnificence >= 3f)
					{
						num3 *= GameManager.Instance.RewardManager.ThreeOrMoreHeartMagMultiplier;
					}
					else if (RewardManager.AdditionalHeartTierMagnificence >= 1f)
					{
						num3 *= GameManager.Instance.RewardManager.OneOrTwoHeartMagMultiplier;
					}
				}
				if (flag4)
				{
					num += num3;
					KeyValuePair<WeightedGameObject, float> item4 = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], num3);
					list.Add(item4);
				}
			}
			if (list.Count == 0 && list2.Count > 0)
			{
				list = list2;
				num = num2;
			}
			if (num > 0f && list.Count > 0)
			{
				float num6 = num * ((generatorRandom == null) ? UnityEngine.Random.value : ((float)generatorRandom.NextDouble()));
				for (int k = 0; k < list.Count; k++)
				{
					num6 -= list[k].Value;
					if (num6 <= 0f)
					{
						string text = ((!(list[k].Key.gameObject != null)) ? "noll" : list[k].Key.gameObject.name);
						Debug.Log("returning item " + text + " #" + k.ToString() + " of " + list.Count + "|" + itemQuality.ToString());
						SynercacheManager.UseCachedSynergyIDs = false;
						return list[k].Key.gameObject;
					}
				}
				Debug.Log("returning last possible item");
				SynercacheManager.UseCachedSynergyIDs = false;
				return list[list.Count - 1].Key.gameObject;
			}
			itemQuality--;
			if (itemQuality < PickupObject.ItemQuality.COMMON && !flag)
			{
				itemQuality = PickupObject.ItemQuality.D;
			}
		}
		SynercacheManager.UseCachedSynergyIDs = false;
		Debug.LogError("Failed to get any item at all.");
		return null;
	}

	protected PickupObject.ItemQuality GetTargetItemTier(System.Random generatorRandom)
	{
		float num = ((!ForceNotCommon) ? Common_Chance : 0f);
		float d_Chance = D_Chance;
		float c_Chance = C_Chance;
		float b_Chance = B_Chance;
		float a_Chance = A_Chance;
		float s_Chance = S_Chance;
		float num2 = num + d_Chance + c_Chance + b_Chance + a_Chance + s_Chance;
		if (num2 == 0f)
		{
			return PickupObject.ItemQuality.D;
		}
		float num3 = num2 * ((generatorRandom == null) ? UnityEngine.Random.value : ((float)generatorRandom.NextDouble()));
		float num4 = 0f;
		num4 += num;
		if (num4 > num3)
		{
			return PickupObject.ItemQuality.COMMON;
		}
		num4 += d_Chance;
		if (num4 > num3)
		{
			return PickupObject.ItemQuality.D;
		}
		num4 += c_Chance;
		if (num4 > num3)
		{
			return PickupObject.ItemQuality.C;
		}
		num4 += b_Chance;
		if (num4 > num3)
		{
			return PickupObject.ItemQuality.B;
		}
		num4 += a_Chance;
		if (num4 > num3)
		{
			return PickupObject.ItemQuality.A;
		}
		num4 += s_Chance;
		if (num4 > num3)
		{
			return PickupObject.ItemQuality.S;
		}
		return PickupObject.ItemQuality.S;
	}

	protected PickupObject.ItemQuality GetTargetItemQuality(PlayerController player, System.Random generatorRandom)
	{
		return GetTargetItemTier(generatorRandom);
	}
}
