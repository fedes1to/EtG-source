using System;
using System.Collections;
using System.Collections.Generic;

public class dfList<T> : IList<T>, IDisposable, IPoolable, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private class PooledEnumerator : IEnumerator<T>, IEnumerable<T>, IEnumerator, IDisposable, IEnumerable
	{
		private static Queue<PooledEnumerator> pool = new Queue<PooledEnumerator>();

		private dfList<T> list;

		private Func<T, bool> predicate;

		private int currentIndex;

		private T currentValue;

		private bool isValid;

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		public T Current
		{
			get
			{
				if (!isValid)
				{
					throw new InvalidOperationException("The enumerator is no longer valid");
				}
				return currentValue;
			}
		}

		public static PooledEnumerator Obtain(dfList<T> list, Func<T, bool> predicate)
		{
			PooledEnumerator pooledEnumerator = ((pool.Count <= 0) ? new PooledEnumerator() : pool.Dequeue());
			pooledEnumerator.ResetInternal(list, predicate);
			return pooledEnumerator;
		}

		public void Release()
		{
			if (isValid)
			{
				isValid = false;
				pool.Enqueue(this);
			}
		}

		private void ResetInternal(dfList<T> list, Func<T, bool> predicate)
		{
			isValid = true;
			this.list = list;
			this.predicate = predicate;
			currentIndex = 0;
			currentValue = default(T);
		}

		public void Dispose()
		{
			Release();
		}

		public bool MoveNext()
		{
			if (!isValid)
			{
				throw new InvalidOperationException("The enumerator is no longer valid");
			}
			while (currentIndex < list.Count)
			{
				T arg = list[currentIndex++];
				if (predicate != null && !predicate(arg))
				{
					continue;
				}
				currentValue = arg;
				return true;
			}
			Release();
			currentValue = default(T);
			return false;
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}
	}

	private class FunctorComparer : IComparer<T>, IDisposable
	{
		private static Queue<FunctorComparer> pool = new Queue<FunctorComparer>();

		private Comparison<T> comparison;

		public static FunctorComparer Obtain(Comparison<T> comparison)
		{
			FunctorComparer functorComparer = ((pool.Count <= 0) ? new FunctorComparer() : pool.Dequeue());
			functorComparer.comparison = comparison;
			return functorComparer;
		}

		public void Release()
		{
			comparison = null;
			if (!pool.Contains(this))
			{
				pool.Enqueue(this);
			}
		}

		public int Compare(T x, T y)
		{
			return comparison(x, y);
		}

		public void Dispose()
		{
			Release();
		}
	}

	private static Queue<object> pool = new Queue<object>(1024);

	private const int DEFAULT_CAPACITY = 128;

	private T[] items = new T[128];

	private int count;

	private bool isElementTypeValueType;

	private bool isElementTypePoolable;

	private bool autoReleaseItems;

	public bool AutoReleaseItems
	{
		get
		{
			return autoReleaseItems;
		}
		set
		{
			autoReleaseItems = value;
		}
	}

	public int Count
	{
		get
		{
			return count;
		}
	}

	internal int Capacity
	{
		get
		{
			return items.Length;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return false;
		}
	}

	public T this[int index]
	{
		get
		{
			if (index < 0 || index > count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			return items[index];
		}
		set
		{
			if (index < 0 || index > count - 1)
			{
				throw new IndexOutOfRangeException();
			}
			items[index] = value;
		}
	}

	internal T[] Items
	{
		get
		{
			return items;
		}
	}

	internal dfList()
	{
		isElementTypeValueType = typeof(T).IsValueType;
		isElementTypePoolable = typeof(IPoolable).IsAssignableFrom(typeof(T));
	}

	internal dfList(IList<T> listToClone)
		: this()
	{
		AddRange(listToClone);
	}

	internal dfList(int capacity)
		: this()
	{
		EnsureCapacity(capacity);
	}

	public static void ClearPool()
	{
		lock (pool)
		{
			pool.Clear();
			pool.TrimExcess();
		}
	}

	public static dfList<T> Obtain()
	{
		lock (pool)
		{
			if (pool.Count == 0)
			{
				return new dfList<T>();
			}
			return (dfList<T>)pool.Dequeue();
		}
	}

	public static dfList<T> Obtain(int capacity)
	{
		dfList<T> dfList2 = Obtain();
		dfList2.EnsureCapacity(capacity);
		return dfList2;
	}

	public void ReleaseItems()
	{
		if (!isElementTypePoolable)
		{
			throw new InvalidOperationException(string.Format("Element type {0} does not implement the {1} interface", typeof(T).Name, typeof(IPoolable).Name));
		}
		for (int i = 0; i < count; i++)
		{
			IPoolable poolable = items[i] as IPoolable;
			poolable.Release();
		}
		Clear();
	}

	public void Release()
	{
		lock (pool)
		{
			if (autoReleaseItems && isElementTypePoolable)
			{
				autoReleaseItems = false;
				ReleaseItems();
			}
			else
			{
				Clear();
			}
			pool.Enqueue(this);
		}
	}

	public void Enqueue(T item)
	{
		lock (items)
		{
			Add(item);
		}
	}

	public T Dequeue()
	{
		lock (items)
		{
			if (count == 0)
			{
				throw new IndexOutOfRangeException();
			}
			T result = items[0];
			RemoveAt(0);
			return result;
		}
	}

	public T Pop()
	{
		lock (items)
		{
			if (count == 0)
			{
				throw new IndexOutOfRangeException();
			}
			T result = items[count - 1];
			items[count - 1] = default(T);
			count--;
			return result;
		}
	}

	public dfList<T> Clone()
	{
		dfList<T> dfList2 = Obtain(count);
		Array.Copy(items, 0, dfList2.items, 0, count);
		dfList2.count = count;
		return dfList2;
	}

	public void Reverse()
	{
		Array.Reverse(items, 0, count);
	}

	public void Sort()
	{
		Array.Sort(items, 0, count, null);
	}

	public void Sort(IComparer<T> comparer)
	{
		Array.Sort(items, 0, count, comparer);
	}

	public void Sort(Comparison<T> comparison)
	{
		if (comparison == null)
		{
			throw new ArgumentNullException("comparison");
		}
		if (count > 0)
		{
			using (FunctorComparer comparer = FunctorComparer.Obtain(comparison))
			{
				Array.Sort(items, 0, count, comparer);
			}
		}
	}

	public void EnsureCapacity(int Size)
	{
		if (items.Length < Size)
		{
			int newSize = Size / 128 * 128 + 128;
			Array.Resize(ref items, newSize);
		}
	}

	public void AddRange(dfList<T> list)
	{
		int num = list.count;
		EnsureCapacity(count + num);
		Array.Copy(list.items, 0, items, count, num);
		count += num;
	}

	public void AddRange(IList<T> list)
	{
		int num = list.Count;
		EnsureCapacity(count + num);
		for (int i = 0; i < num; i++)
		{
			items[count++] = list[i];
		}
	}

	public void AddRange(T[] list)
	{
		int num = list.Length;
		EnsureCapacity(count + num);
		Array.Copy(list, 0, items, count, num);
		count += num;
	}

	public int IndexOf(T item)
	{
		return Array.IndexOf(items, item, 0, count);
	}

	public void Insert(int index, T item)
	{
		EnsureCapacity(count + 1);
		if (index < count)
		{
			Array.Copy(items, index, items, index + 1, count - index);
		}
		items[index] = item;
		count++;
	}

	public void InsertRange(int index, T[] array)
	{
		if (array == null)
		{
			throw new ArgumentNullException("items");
		}
		if (index < 0 || index > count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		EnsureCapacity(count + array.Length);
		if (index < count)
		{
			Array.Copy(items, index, items, index + array.Length, count - index);
		}
		array.CopyTo(items, index);
		count += array.Length;
	}

	public void InsertRange(int index, dfList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("items");
		}
		if (index < 0 || index > count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		EnsureCapacity(count + list.count);
		if (index < count)
		{
			Array.Copy(items, index, items, index + list.count, count - index);
		}
		Array.Copy(list.items, 0, items, index, list.count);
		count += list.count;
	}

	public void RemoveAll(Predicate<T> predicate)
	{
		int num = 0;
		while (num < count)
		{
			if (predicate(items[num]))
			{
				RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public void RemoveAt(int index)
	{
		if (index >= count)
		{
			throw new ArgumentOutOfRangeException();
		}
		count--;
		if (index < count)
		{
			Array.Copy(items, index + 1, items, index, count - index);
		}
		items[count] = default(T);
	}

	public void RemoveRange(int index, int length)
	{
		if (index < 0 || length < 0 || count - index < length)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (count > 0)
		{
			count -= length;
			if (index < count)
			{
				Array.Copy(items, index + length, items, index, count - index);
			}
			Array.Clear(items, count, length);
		}
	}

	public void Add(T item)
	{
		EnsureCapacity(count + 1);
		items[count++] = item;
	}

	public void Add(T item0, T item1)
	{
		EnsureCapacity(count + 2);
		items[count++] = item0;
		items[count++] = item1;
	}

	public void Add(T item0, T item1, T item2)
	{
		EnsureCapacity(count + 3);
		items[count++] = item0;
		items[count++] = item1;
		items[count++] = item2;
	}

	public void Clear()
	{
		if (!isElementTypeValueType)
		{
			Array.Clear(items, 0, items.Length);
		}
		count = 0;
	}

	public void TrimExcess()
	{
		Array.Resize(ref items, count);
	}

	public bool Contains(T item)
	{
		if (item == null)
		{
			for (int i = 0; i < count; i++)
			{
				if (items[i] == null)
				{
					return true;
				}
			}
			return false;
		}
		EqualityComparer<T> @default = EqualityComparer<T>.Default;
		for (int j = 0; j < count; j++)
		{
			if (@default.Equals(items[j], item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array)
	{
		CopyTo(array, 0);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		Array.Copy(items, 0, array, arrayIndex, count);
	}

	public void CopyTo(int sourceIndex, T[] dest, int destIndex, int length)
	{
		if (sourceIndex + length > count)
		{
			throw new IndexOutOfRangeException("sourceIndex");
		}
		if (dest == null)
		{
			throw new ArgumentNullException("dest");
		}
		if (destIndex + length > dest.Length)
		{
			throw new IndexOutOfRangeException("destIndex");
		}
		Array.Copy(items, sourceIndex, dest, destIndex, length);
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num == -1)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public List<T> ToList()
	{
		List<T> list = new List<T>(count);
		list.AddRange(ToArray());
		return list;
	}

	public T[] ToArray()
	{
		T[] array = new T[count];
		Array.Copy(items, 0, array, 0, count);
		return array;
	}

	public T[] ToArray(int index, int length)
	{
		T[] array = new T[count];
		if (count > 0)
		{
			CopyTo(index, array, 0, length);
		}
		return array;
	}

	public dfList<T> GetRange(int index, int length)
	{
		dfList<T> dfList2 = Obtain(length);
		CopyTo(0, dfList2.items, index, length);
		return dfList2;
	}

	public bool Any(Func<T, bool> predicate)
	{
		for (int i = 0; i < count; i++)
		{
			if (predicate(items[i]))
			{
				return true;
			}
		}
		return false;
	}

	public T First()
	{
		if (count == 0)
		{
			throw new IndexOutOfRangeException();
		}
		return items[0];
	}

	public T FirstOrDefault()
	{
		if (count > 0)
		{
			return items[0];
		}
		return default(T);
	}

	public T FirstOrDefault(Func<T, bool> predicate)
	{
		for (int i = 0; i < count; i++)
		{
			if (predicate(items[i]))
			{
				return items[i];
			}
		}
		return default(T);
	}

	public T Last()
	{
		if (count == 0)
		{
			throw new IndexOutOfRangeException();
		}
		return items[count - 1];
	}

	public T LastOrDefault()
	{
		if (count == 0)
		{
			return default(T);
		}
		return items[count - 1];
	}

	public T LastOrDefault(Func<T, bool> predicate)
	{
		T result = default(T);
		for (int i = 0; i < count; i++)
		{
			if (predicate(items[i]))
			{
				result = items[i];
			}
		}
		return result;
	}

	public dfList<T> Where(Func<T, bool> predicate)
	{
		dfList<T> dfList2 = Obtain(count);
		for (int i = 0; i < count; i++)
		{
			if (predicate(items[i]))
			{
				dfList2.Add(items[i]);
			}
		}
		return dfList2;
	}

	public int Matching(Func<T, bool> predicate)
	{
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (predicate(items[i]))
			{
				num++;
			}
		}
		return num;
	}

	public dfList<TResult> Select<TResult>(Func<T, TResult> selector)
	{
		dfList<TResult> dfList2 = dfList<TResult>.Obtain(count);
		for (int i = 0; i < count; i++)
		{
			dfList2.Add(selector(items[i]));
		}
		return dfList2;
	}

	public dfList<T> Concat(dfList<T> list)
	{
		dfList<T> dfList2 = Obtain(count + list.count);
		dfList2.AddRange(this);
		dfList2.AddRange(list);
		return dfList2;
	}

	public dfList<TResult> Convert<TResult>()
	{
		dfList<TResult> dfList2 = dfList<TResult>.Obtain(count);
		for (int i = 0; i < count; i++)
		{
			dfList2.Add((TResult)System.Convert.ChangeType(items[i], typeof(TResult)));
		}
		return dfList2;
	}

	public void ForEach(Action<T> action)
	{
		int num = 0;
		while (num < Count)
		{
			action(items[num++]);
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		return PooledEnumerator.Obtain(this, null);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return PooledEnumerator.Obtain(this, null);
	}

	public void Dispose()
	{
		Release();
	}
}
