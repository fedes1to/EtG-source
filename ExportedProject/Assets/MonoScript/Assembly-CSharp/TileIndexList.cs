using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TileIndexList
{
	[SerializeField]
	public List<int> indices;

	[SerializeField]
	public List<float> indexWeights;

	public TileIndexList()
	{
		indices = new List<int>();
		indexWeights = new List<float>();
	}

	public int GetIndexOfIndex(int index)
	{
		for (int i = 0; i < indices.Count; i++)
		{
			if (indices[i] == index)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetIndexByWeight()
	{
		float num = indexWeights.Sum();
		float num2 = num * UnityEngine.Random.value;
		float num3 = 0f;
		for (int i = 0; i < indices.Count; i++)
		{
			num3 += indexWeights[i];
			if (num3 >= num2)
			{
				return indices[i];
			}
		}
		if (indices.Count == 0)
		{
			return -1;
		}
		return indices[indices.Count - 1];
	}

	public bool ContainsValid()
	{
		for (int i = 0; i < indices.Count; i++)
		{
			if (indices[i] != -1)
			{
				return true;
			}
		}
		return false;
	}
}
