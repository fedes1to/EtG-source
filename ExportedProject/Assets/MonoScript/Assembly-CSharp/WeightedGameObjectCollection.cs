using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeightedGameObjectCollection
{
	public List<WeightedGameObject> elements;

	public WeightedGameObjectCollection()
	{
		elements = new List<WeightedGameObject>();
	}

	public void Add(WeightedGameObject w)
	{
		elements.Add(w);
	}

	public float GetTotalWeight()
	{
		float num = 0f;
		for (int i = 0; i < elements.Count; i++)
		{
			WeightedGameObject weightedGameObject = elements[i];
			bool flag = true;
			for (int j = 0; j < weightedGameObject.additionalPrerequisites.Length; j++)
			{
				if (!weightedGameObject.additionalPrerequisites[j].CheckConditionsFulfilled())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				num += weightedGameObject.weight;
			}
		}
		return num;
	}

	public GameObject SelectByWeight()
	{
		int outIndex = -1;
		return SelectByWeight(out outIndex);
	}

	public GameObject SelectByWeight(out int outIndex, bool useSeedRandom = false)
	{
		outIndex = -1;
		List<WeightedGameObject> list = new List<WeightedGameObject>();
		float num = 0f;
		for (int i = 0; i < elements.Count; i++)
		{
			WeightedGameObject weightedGameObject = elements[i];
			bool flag = true;
			if (weightedGameObject.additionalPrerequisites != null)
			{
				for (int j = 0; j < weightedGameObject.additionalPrerequisites.Length; j++)
				{
					if (!weightedGameObject.additionalPrerequisites[j].CheckConditionsFulfilled())
					{
						flag = false;
						break;
					}
				}
			}
			if (weightedGameObject.gameObject != null)
			{
				PickupObject component = weightedGameObject.gameObject.GetComponent<PickupObject>();
				if (component != null && !component.PrerequisitesMet())
				{
					flag = false;
				}
			}
			if (flag)
			{
				list.Add(weightedGameObject);
				num += weightedGameObject.weight;
			}
		}
		float num2 = ((!useSeedRandom) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) * num;
		float num3 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			num3 += list[k].weight;
			if (num3 > num2)
			{
				outIndex = elements.IndexOf(list[k]);
				return list[k].gameObject;
			}
		}
		outIndex = elements.IndexOf(list[list.Count - 1]);
		return list[list.Count - 1].gameObject;
	}

	public GameObject SubshopStyleSelectByWeightWithoutDuplicatesFullPrereqs(List<GameObject> extant, Func<GameObject, float, float> weightModifier, int minElements, bool useSeedRandom = false)
	{
		List<WeightedGameObject> list = new List<WeightedGameObject>();
		float num = 0f;
		bool flag = useSeedRandom;
		int num2 = 0;
		while (num2 < 2 && list.Count < minElements)
		{
			num2++;
			for (int i = 0; i < elements.Count; i++)
			{
				WeightedGameObject weightedGameObject = elements[i];
				if (weightedGameObject.gameObject == null || (extant != null && extant.Contains(weightedGameObject.gameObject) && !weightedGameObject.forceDuplicatesPossible))
				{
					continue;
				}
				PickupObject component = weightedGameObject.gameObject.GetComponent<PickupObject>();
				bool flag2 = true;
				if (component.quality == PickupObject.ItemQuality.SPECIAL)
				{
					flag2 = false;
					if (component is AncientPrimerItem || component is ArcaneGunpowderItem || component is AstralSlugItem || component is ObsidianShellItem)
					{
						flag2 = true;
					}
				}
				if (component != null && (!component.PrerequisitesMet() || !flag2))
				{
					continue;
				}
				EncounterTrackable component2 = weightedGameObject.gameObject.GetComponent<EncounterTrackable>();
				if (component2 != null && !flag && GameStatsManager.Instance.QueryEncounterableDifferentiator(component2) > 0 && !weightedGameObject.forceDuplicatesPossible)
				{
					continue;
				}
				bool flag3 = true;
				for (int j = 0; j < weightedGameObject.additionalPrerequisites.Length; j++)
				{
					if (!weightedGameObject.additionalPrerequisites[j].CheckConditionsFulfilled())
					{
						flag3 = false;
						break;
					}
				}
				if (flag3)
				{
					float num3 = ((weightModifier == null) ? weightedGameObject.weight : weightModifier(weightedGameObject.gameObject, weightedGameObject.weight));
					list.Add(weightedGameObject);
					num += num3;
				}
			}
			flag = true;
		}
		float num4 = ((!useSeedRandom) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) * num;
		float num5 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			float num6 = ((weightModifier == null) ? list[k].weight : weightModifier(list[k].gameObject, list[k].weight));
			num5 += num6;
			if (num5 > num4)
			{
				return list[k].gameObject;
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[list.Count - 1].gameObject;
	}

	public GameObject SelectByWeightWithoutDuplicatesFullPrereqs(List<GameObject> extant, Func<GameObject, float, float> weightModifier, bool useSeedRandom = false)
	{
		List<WeightedGameObject> list = new List<WeightedGameObject>();
		float num = 0f;
		for (int i = 0; i < elements.Count; i++)
		{
			WeightedGameObject weightedGameObject = elements[i];
			if (weightedGameObject.gameObject == null)
			{
				list.Add(weightedGameObject);
				num += weightedGameObject.weight;
			}
			else
			{
				if (extant != null && extant.Contains(weightedGameObject.gameObject) && !weightedGameObject.forceDuplicatesPossible)
				{
					continue;
				}
				PickupObject component = weightedGameObject.gameObject.GetComponent<PickupObject>();
				bool flag = true;
				if (component.quality == PickupObject.ItemQuality.SPECIAL)
				{
					flag = false;
					bool flag2 = component is SpecialKeyItem && (component as SpecialKeyItem).keyType == SpecialKeyItem.SpecialKeyType.RESOURCEFUL_RAT_LAIR;
					if (component is AncientPrimerItem || component is ArcaneGunpowderItem || component is AstralSlugItem || component is ObsidianShellItem || flag2)
					{
						flag = true;
					}
				}
				if (component != null && (!component.PrerequisitesMet() || !flag))
				{
					continue;
				}
				EncounterTrackable component2 = weightedGameObject.gameObject.GetComponent<EncounterTrackable>();
				if (!useSeedRandom && component2 != null && GameStatsManager.Instance.QueryEncounterableDifferentiator(component2) > 0 && !weightedGameObject.forceDuplicatesPossible)
				{
					continue;
				}
				bool flag3 = true;
				for (int j = 0; j < weightedGameObject.additionalPrerequisites.Length; j++)
				{
					if (!weightedGameObject.additionalPrerequisites[j].CheckConditionsFulfilled())
					{
						flag3 = false;
						break;
					}
				}
				if (flag3)
				{
					float num2 = ((weightModifier == null) ? weightedGameObject.weight : weightModifier(weightedGameObject.gameObject, weightedGameObject.weight));
					list.Add(weightedGameObject);
					num += num2;
				}
			}
		}
		float num3 = ((!useSeedRandom) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) * num;
		float num4 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			float num5 = ((weightModifier == null) ? list[k].weight : weightModifier(list[k].gameObject, list[k].weight));
			num4 += num5;
			if (num4 > num3)
			{
				return list[k].gameObject;
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[list.Count - 1].gameObject;
	}

	public GameObject SelectByWeightWithoutDuplicates(List<GameObject> extant, bool useSeedRandom = false)
	{
		if (extant.Count == elements.Count)
		{
			return null;
		}
		List<WeightedGameObject> list = new List<WeightedGameObject>();
		float num = 0f;
		for (int i = 0; i < elements.Count; i++)
		{
			WeightedGameObject weightedGameObject = elements[i];
			if (extant.Contains(weightedGameObject.gameObject))
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < weightedGameObject.additionalPrerequisites.Length; j++)
			{
				if (!weightedGameObject.additionalPrerequisites[j].CheckConditionsFulfilled())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(weightedGameObject);
				num += weightedGameObject.weight;
			}
		}
		float num2 = ((!useSeedRandom) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) * num;
		float num3 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			num3 += list[k].weight;
			if (num3 > num2)
			{
				return list[k].gameObject;
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[list.Count - 1].gameObject;
	}
}
