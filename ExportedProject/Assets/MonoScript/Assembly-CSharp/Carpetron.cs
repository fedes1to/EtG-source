using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class Carpetron
{
	public static HashSet<IntVector2> PostprocessFullRoom(HashSet<IntVector2> set)
	{
		HashSet<IntVector2> hashSet = new HashSet<IntVector2>();
		IntVector2[] cardinalsAndOrdinals = IntVector2.CardinalsAndOrdinals;
		foreach (IntVector2 item in set)
		{
			bool flag = false;
			for (int i = 0; i < cardinalsAndOrdinals.Length; i++)
			{
				if (!set.Contains(item + cardinalsAndOrdinals[i]))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				hashSet.Add(item);
				for (int j = 0; j < cardinalsAndOrdinals.Length; j++)
				{
					hashSet.Add(item + cardinalsAndOrdinals[j]);
				}
			}
		}
		return hashSet;
	}

	public static Tuple<IntVector2, IntVector2> PostprocessSubmatrix(Tuple<IntVector2, IntVector2> rect, out Tuple<IntVector2, IntVector2> bonusRect)
	{
		bonusRect = null;
		IntVector2 intVector = rect.Second - rect.First;
		IntVector2 first = rect.First;
		IntVector2 second = rect.Second;
		if (intVector.x > 12 && intVector.y > 12)
		{
			if (UnityEngine.Random.value < 1.4f)
			{
				int num = intVector.x / 3;
				int num2 = intVector.y / 3;
				first.x += num;
				second.x -= num;
				IntVector2 first2 = rect.First;
				IntVector2 second2 = rect.Second;
				first2.y += num2;
				second2.y -= num2;
				bonusRect = new Tuple<IntVector2, IntVector2>(first2, second2);
			}
			else if (intVector.x > intVector.y)
			{
				while (intVector.y > 4 && UnityEngine.Random.value > 0.3f)
				{
					intVector.y -= 2;
					first.y++;
					second.y--;
				}
			}
			else
			{
				while (intVector.x > 4 && UnityEngine.Random.value > 0.3f)
				{
					intVector.x -= 2;
					first.x++;
					second.x--;
				}
			}
		}
		else if (intVector.x > intVector.y && intVector.x > 12)
		{
			while (intVector.x > 12 && UnityEngine.Random.value > 0.3f)
			{
				intVector.x -= 2;
				first.x++;
				second.x--;
			}
		}
		else if (intVector.y > intVector.x && intVector.y > 12)
		{
			while (intVector.y > 12 && UnityEngine.Random.value > 0.3f)
			{
				intVector.y -= 2;
				first.y++;
				second.y--;
			}
		}
		return new Tuple<IntVector2, IntVector2>(first, second);
	}

	public static Tuple<IntVector2, IntVector2> RawMaxSubmatrix(CellData[][] matrix, IntVector2 basePosition, IntVector2 dimensions, Func<CellData, bool> isInvalidFunction)
	{
		List<IntRect> list = new List<IntRect>();
		int y = dimensions.y;
		int x = dimensions.x;
		int num = -1;
		int num2 = -1;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int[] array = new int[x];
		for (int i = 0; i < x; i++)
		{
			array[i] = -1;
		}
		int[] array2 = new int[x];
		int[] array3 = new int[x];
		Stack<int> stack = new Stack<int>();
		for (int j = 0; j < y; j++)
		{
			for (int k = 0; k < x; k++)
			{
				CellData arg = matrix[basePosition.x + k][basePosition.y + j];
				if (isInvalidFunction(arg))
				{
					array[k] = j;
				}
			}
			stack.Clear();
			for (int l = 0; l < x; l++)
			{
				while (stack.Count > 0 && array[stack.Peek()] <= array[l])
				{
					stack.Pop();
				}
				array2[l] = ((stack.Count != 0) ? stack.Peek() : (-1));
				stack.Push(l);
			}
			stack.Clear();
			for (int num7 = x - 1; num7 >= 0; num7--)
			{
				while (stack.Count > 0 && array[stack.Peek()] <= array[num7])
				{
					stack.Pop();
				}
				array3[num7] = ((stack.Count != 0) ? stack.Peek() : x);
				stack.Push(num7);
			}
			for (int m = 0; m < x; m++)
			{
				num2 = (j - array[m]) * (array3[m] - array2[m] - 1);
				if (num2 > num)
				{
					num = num2;
					num3 = array2[m] + 1;
					num4 = array[m] + 1;
					num5 = array3[m] - 1;
					num6 = j;
					list.Add(new IntRect(num3, num4, num5 - num3, num6 - num4));
				}
			}
		}
		IntVector2 dimensions2 = list[list.Count - 1].Dimensions;
		for (int num8 = list.Count - 2; num8 >= 0; num8--)
		{
			if (list[num8].Dimensions != dimensions2)
			{
				list.RemoveAt(num8);
				num8++;
			}
		}
		int index = Mathf.FloorToInt((float)list.Count / 2f);
		return new Tuple<IntVector2, IntVector2>(new IntVector2(list[index].Left, list[index].Bottom), new IntVector2(list[index].Right, list[index].Top));
	}

	public static Tuple<IntVector2, IntVector2> MaxSubmatrix(CellData[][] matrix, IntVector2 basePosition, IntVector2 dimensions, bool includePits = false, bool includeOverrideFloors = false, bool includeWallNeighbors = false, int visualSubtype = -1)
	{
		DungeonData data = GameManager.Instance.Dungeon.data;
		List<IntRect> list = new List<IntRect>();
		int y = dimensions.y;
		int x = dimensions.x;
		int num = -1;
		int num2 = -1;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int[] array = new int[x];
		for (int i = 0; i < x; i++)
		{
			array[i] = -1;
		}
		int[] array2 = new int[x];
		int[] array3 = new int[x];
		Stack<int> stack = new Stack<int>();
		for (int j = 0; j < y; j++)
		{
			for (int k = 0; k < x; k++)
			{
				CellData cellData = matrix[basePosition.x + k][basePosition.y + j];
				if (cellData == null)
				{
					array[k] = j;
					continue;
				}
				bool flag = (!includeWallNeighbors && cellData.HasWallNeighbor(true, false)) || (!includePits && cellData.HasPitNeighbor(data));
				if (cellData.type == CellType.WALL || cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Ice || cellData.cellVisualData.pathTilesetGridIndex > -1 || (!includeOverrideFloors && cellData.doesDamage) || (!includePits && cellData.type == CellType.PIT) || flag || (!includeOverrideFloors && cellData.cellVisualData.floorTileOverridden) || (!includeOverrideFloors && cellData.HasPhantomCarpetNeighbor()) || (visualSubtype > -1 && cellData.cellVisualData.roomVisualTypeIndex != visualSubtype))
				{
					array[k] = j;
				}
			}
			stack.Clear();
			for (int l = 0; l < x; l++)
			{
				while (stack.Count > 0 && array[stack.Peek()] <= array[l])
				{
					stack.Pop();
				}
				array2[l] = ((stack.Count != 0) ? stack.Peek() : (-1));
				stack.Push(l);
			}
			stack.Clear();
			for (int num7 = x - 1; num7 >= 0; num7--)
			{
				while (stack.Count > 0 && array[stack.Peek()] <= array[num7])
				{
					stack.Pop();
				}
				array3[num7] = ((stack.Count != 0) ? stack.Peek() : x);
				stack.Push(num7);
			}
			for (int m = 0; m < x; m++)
			{
				num2 = (j - array[m]) * (array3[m] - array2[m] - 1);
				if (num2 > num)
				{
					num = num2;
					num3 = array2[m] + 1;
					num4 = array[m] + 1;
					num5 = array3[m] - 1;
					num6 = j;
					list.Add(new IntRect(num3, num4, num5 - num3, num6 - num4));
				}
			}
		}
		IntVector2 dimensions2 = list[list.Count - 1].Dimensions;
		for (int num8 = list.Count - 2; num8 >= 0; num8--)
		{
			if (list[num8].Dimensions != dimensions2)
			{
				list.RemoveAt(num8);
				num8++;
			}
		}
		int index = Mathf.FloorToInt((float)list.Count / 2f);
		return new Tuple<IntVector2, IntVector2>(new IntVector2(list[index].Left, list[index].Bottom), new IntVector2(list[index].Right, list[index].Top));
	}
}
