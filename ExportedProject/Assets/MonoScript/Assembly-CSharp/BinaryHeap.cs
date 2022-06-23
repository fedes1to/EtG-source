using System;
using System.Collections;
using System.Collections.Generic;

public class BinaryHeap<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : IComparable<T>
{
	private const int c_defaultSize = 4;

	private T[] m_data = new T[4];

	private int m_count;

	private int m_capacity = 4;

	private bool m_sorted;

	public int Count
	{
		get
		{
			return m_count;
		}
	}

	public int Capacity
	{
		get
		{
			return m_capacity;
		}
		set
		{
			int capacity = m_capacity;
			m_capacity = Math.Max(value, m_count);
			if (m_capacity != capacity)
			{
				T[] array = new T[m_capacity];
				Array.Copy(m_data, array, m_count);
				m_data = array;
			}
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return false;
		}
	}

	public BinaryHeap()
	{
	}

	private BinaryHeap(T[] data, int count)
	{
		Capacity = count;
		m_count = count;
		Array.Copy(data, m_data, count);
	}

	public T Peek()
	{
		return m_data[0];
	}

	public void Clear()
	{
		m_count = 0;
		m_data = new T[m_capacity];
	}

	public void Add(T item)
	{
		if (m_count == m_capacity)
		{
			Capacity *= 2;
		}
		m_data[m_count] = item;
		UpHeap();
		m_count++;
	}

	public T Remove()
	{
		if (m_count == 0)
		{
			throw new InvalidOperationException("Cannot remove item, heap is empty.");
		}
		T result = m_data[0];
		m_count--;
		m_data[0] = m_data[m_count];
		m_data[m_count] = default(T);
		DownHeap();
		return result;
	}

	private void UpHeap()
	{
		m_sorted = false;
		int num = m_count;
		T val = m_data[num];
		int num2 = Parent(num);
		while (num2 > -1 && val.CompareTo(m_data[num2]) < 0)
		{
			m_data[num] = m_data[num2];
			num = num2;
			num2 = Parent(num);
		}
		m_data[num] = val;
	}

	private void DownHeap()
	{
		m_sorted = false;
		int num = 0;
		T val = m_data[num];
		while (true)
		{
			int num2 = Child1(num);
			if (num2 >= m_count)
			{
				break;
			}
			int num3 = Child2(num);
			int num4 = ((num3 < m_count) ? ((m_data[num2].CompareTo(m_data[num3]) >= 0) ? num3 : num2) : num2);
			if (val.CompareTo(m_data[num4]) > 0)
			{
				m_data[num] = m_data[num4];
				num = num4;
				continue;
			}
			break;
		}
		m_data[num] = val;
	}

	private void EnsureSort()
	{
		if (!m_sorted)
		{
			Array.Sort(m_data, 0, m_count);
			m_sorted = true;
		}
	}

	private static int Parent(int index)
	{
		return index - 1 >> 1;
	}

	private static int Child1(int index)
	{
		return (index << 1) + 1;
	}

	private static int Child2(int index)
	{
		return (index << 1) + 2;
	}

	public BinaryHeap<T> Copy()
	{
		return new BinaryHeap<T>(m_data, m_count);
	}

	public IEnumerator<T> GetEnumerator()
	{
		EnsureSort();
		for (int i = 0; i < m_count; i++)
		{
			yield return m_data[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(T item)
	{
		EnsureSort();
		return Array.BinarySearch(m_data, 0, m_count, item) >= 0;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		EnsureSort();
		Array.Copy(m_data, array, m_count);
	}

	public bool Remove(T item)
	{
		EnsureSort();
		int num = Array.BinarySearch(m_data, 0, m_count, item);
		if (num < 0)
		{
			return false;
		}
		Array.Copy(m_data, num + 1, m_data, num, m_count - num);
		m_data[m_count] = default(T);
		m_count--;
		return true;
	}
}
