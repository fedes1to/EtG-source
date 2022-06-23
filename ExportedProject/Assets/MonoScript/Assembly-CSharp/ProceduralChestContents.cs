using System.Collections.Generic;
using UnityEngine;

public class ProceduralChestContents : ScriptableObject
{
	[BetterList]
	public List<ProceduralChestItem> items;

	public PickupObject GetItem(float val)
	{
		float num = 0f;
		for (int i = 0; i < items.Count; i++)
		{
			num += items[i].chance;
		}
		float num2 = 0f;
		for (int j = 0; j < items.Count; j++)
		{
			num2 += items[j].chance;
			if (num2 / num > val)
			{
				return items[j].item;
			}
		}
		return items[items.Count - 1].item;
	}
}
