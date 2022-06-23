using System;
using System.Collections;
using System.Collections.Generic;

namespace FullInspector
{
	public static class fiListUtility
	{
		public static void Add<T>(ref IList list)
		{
			if (list.GetType().IsArray)
			{
				T[] array = (T[])list;
				Array.Resize(ref array, array.Length + 1);
				list = array;
			}
			else
			{
				list.Add(default(T));
			}
		}

		public static void InsertAt<T>(ref IList list, int index)
		{
			if (list.GetType().IsArray)
			{
				List<T> list2 = new List<T>((IList<T>)list);
				list2.Insert(index, default(T));
				list = list2.ToArray();
			}
			else
			{
				list.Insert(index, default(T));
			}
		}

		public static void RemoveAt<T>(ref IList list, int index)
		{
			if (list.GetType().IsArray)
			{
				List<T> list2 = new List<T>((IList<T>)list);
				list2.RemoveAt(index);
				list = list2.ToArray();
			}
			else
			{
				list.RemoveAt(index);
			}
		}
	}
}
