using System;
using System.Collections;
using System.Collections.Generic;

public class PooledLinkedList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	private LinkedList<T> m_list = new LinkedList<T>();

	private LinkedList<T> m_pool = new LinkedList<T>();

	public LinkedListNode<T> First
	{
		get
		{
			return m_list.First;
		}
	}

	public LinkedListNode<T> Last
	{
		get
		{
			return m_list.Last;
		}
	}

	public int Count
	{
		get
		{
			return m_list.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return ((ICollection<T>)m_list).IsReadOnly;
		}
	}

	public void ClearPool()
	{
		m_pool.Clear();
	}

	public LinkedListNode<T> GetByIndexSlow(int index)
	{
		if (m_list.Count == 0)
		{
			throw new IndexOutOfRangeException();
		}
		LinkedListNode<T> linkedListNode = m_list.First;
		for (int i = 0; i < index; i++)
		{
			linkedListNode = linkedListNode.Next;
			if (linkedListNode == null)
			{
				throw new IndexOutOfRangeException();
			}
		}
		return linkedListNode;
	}

	public void AddAfter(LinkedListNode<T> node, T value)
	{
		m_list.AddAfter(node, value);
	}

	public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
	{
		m_list.AddAfter(node, newNode);
	}

	public void AddBefore(LinkedListNode<T> node, T value)
	{
		m_list.AddBefore(node, value);
	}

	public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
	{
		m_list.AddBefore(node, newNode);
	}

	public void AddFirst(T value)
	{
		if (m_pool.Count > 0)
		{
			LinkedListNode<T> first = m_pool.First;
			m_pool.RemoveFirst();
			first.Value = value;
			m_list.AddFirst(first);
		}
		else
		{
			m_list.AddFirst(value);
		}
	}

	public void AddFirst(LinkedListNode<T> node)
	{
		m_list.AddFirst(node);
	}

	public void AddLast(T value)
	{
		if (m_pool.Count > 0)
		{
			LinkedListNode<T> first = m_pool.First;
			m_pool.RemoveFirst();
			first.Value = value;
			m_list.AddLast(first);
		}
		else
		{
			m_list.AddLast(value);
		}
	}

	public void AddLast(LinkedListNode<T> node)
	{
		m_list.AddLast(node);
	}

	public bool Remove(T value)
	{
		LinkedListNode<T> linkedListNode = m_list.Find(value);
		if (linkedListNode == null)
		{
			return false;
		}
		m_list.Remove(linkedListNode);
		linkedListNode.Value = default(T);
		m_pool.AddLast(linkedListNode);
		return true;
	}

	public void Remove(LinkedListNode<T> node, bool returnToPool)
	{
		m_list.Remove(node);
		if (returnToPool)
		{
			node.Value = default(T);
			m_pool.AddLast(node);
		}
	}

	public void RemoveFirst()
	{
		LinkedListNode<T> first = m_list.First;
		m_list.Remove(first);
		first.Value = default(T);
		m_pool.AddLast(first);
	}

	public void RemoveLast()
	{
		LinkedListNode<T> last = m_list.Last;
		m_list.Remove(last);
		last.Value = default(T);
		m_pool.AddLast(last);
	}

	void ICollection<T>.Add(T item)
	{
		AddLast(item);
	}

	public void Clear()
	{
		while (m_list.Count > 0)
		{
			RemoveLast();
		}
	}

	public bool Contains(T item)
	{
		return m_list.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		m_list.CopyTo(array, arrayIndex);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_list).GetEnumerator();
	}
}
