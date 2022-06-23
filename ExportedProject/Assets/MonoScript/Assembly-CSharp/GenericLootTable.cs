using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericLootTable : ScriptableObject
{
	public WeightedGameObjectCollection defaultItemDrops;

	public List<GenericLootTable> includedLootTables;

	public DungeonPrerequisite[] tablePrerequisites;

	private WeightedGameObjectCollection m_compiledCollection;

	public bool RawContains(GameObject g)
	{
		for (int i = 0; i < defaultItemDrops.elements.Count; i++)
		{
			if (defaultItemDrops.elements[i].gameObject == g)
			{
				return true;
			}
		}
		return false;
	}

	public GameObject SelectByWeight(bool useSeedRandom = false)
	{
		return GetCompiledCollection().SelectByWeight();
	}

	public GameObject SelectByWeightWithoutDuplicates(List<GameObject> extant, bool useSeedRandom = false)
	{
		return GetCompiledCollection().SelectByWeightWithoutDuplicates(extant, useSeedRandom);
	}

	public GameObject SelectByWeightWithoutDuplicatesFullPrereqs(List<GameObject> extant, bool allowSpice = true, bool useSeedRandom = false)
	{
		return GetCompiledCollection(allowSpice).SelectByWeightWithoutDuplicatesFullPrereqs(extant, null, useSeedRandom);
	}

	public GameObject SubshopSelectByWeightWithoutDuplicatesFullPrereqs(List<GameObject> extant, Func<GameObject, float, float> weightModifier, int minElements, bool useSeedRandom = false)
	{
		return GetCompiledCollection().SubshopStyleSelectByWeightWithoutDuplicatesFullPrereqs(extant, weightModifier, minElements, useSeedRandom);
	}

	public GameObject SelectByWeightWithoutDuplicatesFullPrereqs(List<GameObject> extant, Func<GameObject, float, float> weightModifier, bool useSeedRandom = false)
	{
		return GetCompiledCollection().SelectByWeightWithoutDuplicatesFullPrereqs(extant, weightModifier, useSeedRandom);
	}

	public List<WeightedGameObject> GetCompiledRawItems()
	{
		WeightedGameObjectCollection compiledCollection = GetCompiledCollection();
		return compiledCollection.elements;
	}

	protected WeightedGameObjectCollection GetCompiledCollection(bool allowSpice = true)
	{
		int num = 0;
		if (allowSpice && Application.isPlaying && GameManager.Instance.PrimaryPlayer != null)
		{
			num = GameManager.Instance.PrimaryPlayer.spiceCount;
			if (GameManager.Instance.SecondaryPlayer != null)
			{
				num += GameManager.Instance.SecondaryPlayer.spiceCount;
			}
		}
		if (includedLootTables.Count == 0 && num == 0)
		{
			return defaultItemDrops;
		}
		WeightedGameObjectCollection weightedGameObjectCollection = new WeightedGameObjectCollection();
		for (int i = 0; i < defaultItemDrops.elements.Count; i++)
		{
			weightedGameObjectCollection.Add(defaultItemDrops.elements[i]);
		}
		for (int j = 0; j < includedLootTables.Count; j++)
		{
			if (includedLootTables[j].tablePrerequisites.Length > 0)
			{
				bool flag = false;
				for (int k = 0; k < includedLootTables[j].tablePrerequisites.Length; k++)
				{
					if (!includedLootTables[j].tablePrerequisites[k].CheckConditionsFulfilled())
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			WeightedGameObjectCollection compiledCollection = includedLootTables[j].GetCompiledCollection();
			for (int l = 0; l < compiledCollection.elements.Count; l++)
			{
				weightedGameObjectCollection.Add(compiledCollection.elements[l]);
			}
		}
		if (allowSpice && num > 0)
		{
			float totalWeight = weightedGameObjectCollection.GetTotalWeight();
			float weight = SpiceItem.GetSpiceWeight(num) * totalWeight;
			GameObject gameObject = PickupObjectDatabase.GetById(GlobalItemIds.Spice).gameObject;
			WeightedGameObject weightedGameObject = new WeightedGameObject();
			weightedGameObject.SetGameObject(gameObject);
			weightedGameObject.weight = weight;
			weightedGameObject.additionalPrerequisites = new DungeonPrerequisite[0];
			weightedGameObjectCollection.Add(weightedGameObject);
		}
		return weightedGameObjectCollection;
	}
}
