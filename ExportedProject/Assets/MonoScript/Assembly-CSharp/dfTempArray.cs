using System.Collections.Generic;

internal class dfTempArray<T>
{
	private static List<T[]> cache = new List<T[]>(32);

	public static void Clear()
	{
		cache.Clear();
	}

	public static T[] Obtain(int length)
	{
		return Obtain(length, 128);
	}

	public static T[] Obtain(int length, int maxCacheSize)
	{
		lock (cache)
		{
			for (int i = 0; i < cache.Count; i++)
			{
				T[] array = cache[i];
				if (array.Length == length)
				{
					if (i > 0)
					{
						cache.RemoveAt(i);
						cache.Insert(0, array);
					}
					return array;
				}
			}
			if (cache.Count >= maxCacheSize)
			{
				cache.RemoveAt(cache.Count - 1);
			}
			T[] array2 = new T[length];
			cache.Insert(0, array2);
			return array2;
		}
	}
}
