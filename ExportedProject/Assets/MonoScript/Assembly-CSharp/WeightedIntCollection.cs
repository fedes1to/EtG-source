using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeightedIntCollection
{
	public WeightedInt[] elements;

	public int SelectByWeight(System.Random generatorRandom)
	{
		List<WeightedInt> list = new List<WeightedInt>();
		float num = 0f;
		for (int i = 0; i < elements.Length; i++)
		{
			WeightedInt weightedInt = elements[i];
			if (weightedInt.weight <= 0f)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < weightedInt.additionalPrerequisites.Length; j++)
			{
				if (!weightedInt.additionalPrerequisites[j].CheckConditionsFulfilled())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(weightedInt);
				num += weightedInt.weight;
			}
		}
		float num2 = ((generatorRandom == null) ? UnityEngine.Random.value : ((float)generatorRandom.NextDouble())) * num;
		float num3 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			num3 += list[k].weight;
			if (num3 > num2)
			{
				return list[k].value;
			}
		}
		return list[0].value;
	}

	public int SelectByWeight()
	{
		return SelectByWeight(null);
	}
}
