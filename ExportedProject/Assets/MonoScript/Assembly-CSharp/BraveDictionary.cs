using System;
using System.Collections.Generic;

public class BraveDictionary<TKey, TValue>
{
	private List<TKey> m_keys = new List<TKey>();

	private List<TValue> m_values = new List<TValue>();

	public int Count
	{
		get
		{
			return m_keys.Count;
		}
	}

	public List<TKey> Keys
	{
		get
		{
			return m_keys;
		}
	}

	public List<TValue> Values
	{
		get
		{
			return m_values;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException();
			}
			for (int i = 0; i < m_keys.Count; i++)
			{
				if (m_keys[i].Equals(key))
				{
					return m_values[i];
				}
			}
			throw new KeyNotFoundException();
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException();
			}
			for (int i = 0; i < m_keys.Count; i++)
			{
				if (m_keys[i].Equals(key))
				{
					m_values[i] = value;
				}
			}
			m_keys.Add(key);
			m_values.Add(value);
		}
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		value = default(TValue);
		if (key == null)
		{
			return false;
		}
		for (int i = 0; i < m_keys.Count; i++)
		{
			if (m_keys[i].Equals(key))
			{
				value = m_values[i];
				return true;
			}
		}
		return false;
	}
}
