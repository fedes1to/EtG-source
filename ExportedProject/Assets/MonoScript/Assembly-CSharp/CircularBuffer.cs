using System;
using System.Collections;
using System.Collections.Generic;

public class CircularBuffer<T> : IEnumerable<T>, IEnumerable
{
	private T[] m_buffer;

	private int m_head;

	private int m_tail;

	public int Count { get; private set; }

	public int Capacity
	{
		get
		{
			return m_buffer.Length;
		}
		set
		{
			if (value != m_buffer.Length)
			{
				T[] array = new T[value];
				int num = 0;
				while (Count > 0 && num < value)
				{
					array[num++] = Dequeue();
				}
				m_buffer = array;
				Count = num;
				m_head = num - 1;
				m_tail = 0;
			}
		}
	}

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_buffer[(m_tail + index) % Capacity];
		}
		set
		{
			if (index < 0 || index >= Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			m_buffer[(m_tail + index) % Capacity] = value;
		}
	}

	public CircularBuffer(int capacity)
	{
		m_buffer = new T[capacity];
		m_head = capacity - 1;
	}

	public T Enqueue(T item)
	{
		m_head = (m_head + 1) % Capacity;
		T result = m_buffer[m_head];
		m_buffer[m_head] = item;
		if (Count == Capacity)
		{
			m_tail = (m_tail + 1) % Capacity;
		}
		else
		{
			Count++;
		}
		return result;
	}

	public T Dequeue()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("queue exhausted");
		}
		T result = m_buffer[m_tail];
		m_buffer[m_tail] = default(T);
		m_tail = (m_tail + 1) % Capacity;
		Count--;
		return result;
	}

	public void Clear()
	{
		m_head = Capacity - 1;
		m_tail = 0;
		Count = 0;
	}

	public int IndexOf(T item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (object.Equals(item, this[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, T item)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (Count == index)
		{
			Enqueue(item);
			return;
		}
		T item2 = this[Count - 1];
		for (int i = index; i < Count - 2; i++)
		{
			this[i + 1] = this[i];
		}
		this[index] = item;
		Enqueue(item2);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		for (int num = index; num > 0; num--)
		{
			this[num] = this[num - 1];
		}
		Dequeue();
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (Count != 0 && Capacity != 0)
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
