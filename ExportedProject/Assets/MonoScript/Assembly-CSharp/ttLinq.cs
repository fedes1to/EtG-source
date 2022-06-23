using System;
using System.Collections.Generic;
using System.Linq;

public static class ttLinq
{
	public static TSource ttAggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
	{
		TSource val = default(TSource);
		for (int i = 0; i < source.Count(); i++)
		{
			val = func(val, source.ElementAt(i));
		}
		return val;
	}

	public static TAccumulate ttAggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
	{
		TAccumulate val = seed;
		for (int i = 0; i < source.Count(); i++)
		{
			val = func(val, source.ElementAt(i));
		}
		return val;
	}

	public static TResult ttAggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
	{
		TAccumulate arg = seed;
		for (int i = 0; i < source.Count(); i++)
		{
			arg = func(arg, source.ElementAt(i));
		}
		return resultSelector(arg);
	}

	public static TSource ttLast<TSource>(this IEnumerable<TSource> source)
	{
		int index = source.Count() - 1;
		return source.ElementAt(index);
	}

	public static List<TSource> ttOrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
	{
		TSource[] array = source.ToArray();
		TKey[] keys = array.Select(keySelector).ToArray();
		Array.Sort(keys, array);
		return array.ToList();
	}

	public static List<TSource> ttThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		IOrderedEnumerable<TSource> source2 = source.CreateOrderedEnumerable(keySelector, Comparer<TKey>.Default, false);
		return source2.ToList();
	}

	public static List<TSource> ttOrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
	{
		TSource[] array = source.ToArray();
		TKey[] keys = array.Select(keySelector).ToArray();
		Array.Sort(keys, array);
		return array.Reverse().ToList();
	}

	public static TResult[] ttSelect<TSource, TResult>(this IList<TSource> source, Func<TSource, TResult> selector)
	{
		TResult[] array = new TResult[source.Count];
		for (int i = 0; i < source.Count; i++)
		{
			array[i] = selector(source[i]);
		}
		return array;
	}

	public static int ttSum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> func)
	{
		int num = 0;
		for (int i = 0; i < source.Count(); i++)
		{
			num += func(source.ElementAt(i));
		}
		return num;
	}

	public static float ttSum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> func)
	{
		float num = 0f;
		for (int i = 0; i < source.Count(); i++)
		{
			num += func(source.ElementAt(i));
		}
		return num;
	}

	public static double ttSum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> func)
	{
		double num = 0.0;
		for (int i = 0; i < source.Count(); i++)
		{
			num += func(source.ElementAt(i));
		}
		return num;
	}

	public static List<TSource> ttWhere<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> func)
	{
		List<TSource> list = new List<TSource>();
		foreach (TSource item in source)
		{
			if (func(item))
			{
				list.Add(item);
			}
		}
		return list;
	}
}
