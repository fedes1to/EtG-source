using System;
using System.Collections.Generic;

[Serializable]
public class WeightedRoomCollection
{
	[TrimElementTags]
	public List<WeightedRoom> elements;

	public WeightedRoomCollection()
	{
		elements = new List<WeightedRoom>();
	}

	public void Add(WeightedRoom w)
	{
		elements.Add(w);
	}

	public WeightedRoom SelectByWeight()
	{
		List<WeightedRoom> list = new List<WeightedRoom>();
		float num = 0f;
		for (int i = 0; i < elements.Count; i++)
		{
			WeightedRoom weightedRoom = elements[i];
			bool flag = true;
			for (int j = 0; j < weightedRoom.additionalPrerequisites.Length; j++)
			{
				if (!weightedRoom.additionalPrerequisites[j].CheckConditionsFulfilled())
				{
					flag = false;
					break;
				}
			}
			if (weightedRoom.room != null && !weightedRoom.room.CheckPrerequisites())
			{
				flag = false;
			}
			else if (flag)
			{
				list.Add(weightedRoom);
				num += weightedRoom.weight;
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		float num2 = BraveRandom.GenerationRandomValue() * num;
		float num3 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			num3 += list[k].weight;
			if (num3 > num2)
			{
				return list[k];
			}
		}
		return list[list.Count - 1];
	}

	public WeightedRoom SelectByWeightWithoutDuplicates(List<PrototypeDungeonRoom> extant)
	{
		List<WeightedRoom> list = new List<WeightedRoom>();
		float num = 0f;
		for (int i = 0; i < elements.Count; i++)
		{
			WeightedRoom weightedRoom = elements[i];
			if (extant.Contains(weightedRoom.room))
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < weightedRoom.additionalPrerequisites.Length; j++)
			{
				if (!weightedRoom.additionalPrerequisites[j].CheckConditionsFulfilled())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(weightedRoom);
				num += weightedRoom.weight;
			}
		}
		float num2 = BraveRandom.GenerationRandomValue() * num;
		float num3 = 0f;
		for (int k = 0; k < list.Count; k++)
		{
			num3 += list[k].weight;
			if (num3 > num2)
			{
				return list[k];
			}
		}
		return list[list.Count - 1];
	}
}
