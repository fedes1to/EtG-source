using System.Collections.Generic;
using UnityEngine;

public class WeightedList<T>
{
	public List<WeightedItem<T>> elements;

	public void Add(T item, float weight)
	{
		if (elements == null)
		{
			elements = new List<WeightedItem<T>>();
		}
		elements.Add(new WeightedItem<T>(item, weight));
	}

	public T SelectByWeight()
	{
		if (elements == null || elements.Count == 0)
		{
			return default(T);
		}
		float num = 0f;
		for (int i = 0; i < elements.Count; i++)
		{
			num += elements[i].weight;
		}
		float num2 = Random.value * num;
		float num3 = 0f;
		for (int j = 0; j < elements.Count; j++)
		{
			num3 += elements[j].weight;
			if (num3 > num2)
			{
				return elements[j].value;
			}
		}
		return elements[elements.Count - 1].value;
	}
}
