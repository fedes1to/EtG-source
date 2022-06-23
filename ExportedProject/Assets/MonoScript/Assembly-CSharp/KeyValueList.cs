using System;
using System.Collections;
using System.Collections.Generic;

public class KeyValueList<K, V> : IList, ICollection, IEnumerable
{
	private List<K> keyList = new List<K>();

	private List<V> valList = new List<V>();

	object IList.this[int index]
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public V this[K key]
	{
		get
		{
			V value;
			if (TryGetValue(key, out value))
			{
				return value;
			}
			throw new KeyNotFoundException();
		}
		set
		{
			int num = keyList.IndexOf(key);
			if (num == -1)
			{
				keyList.Add(key);
				valList.Add(value);
			}
			else
			{
				valList[num] = value;
			}
		}
	}

	public int Count
	{
		get
		{
			return valList.Count;
		}
	}

	public bool IsFixedSize
	{
		get
		{
			return false;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return false;
		}
	}

	public bool IsSynchronized
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public object SyncRoot
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public KeyValueList()
	{
	}

	public KeyValueList(ref List<K> keyListRef, ref List<V> valListRef)
	{
		keyList = keyListRef;
		valList = valListRef;
	}

	public KeyValueList(KeyValueList<K, V> otherKeyValueList)
	{
		AddRange(otherKeyValueList);
	}

	public bool TryGetValue(K key, out V value)
	{
		int num = keyList.IndexOf(key);
		if (num == -1)
		{
			value = default(V);
			return false;
		}
		value = valList[num];
		return true;
	}

	public int Add(object value)
	{
		throw new NotImplementedException("Use KeyValueList[key] = value or insert");
	}

	public void Clear()
	{
		keyList.Clear();
		valList.Clear();
	}

	public bool Contains(V value)
	{
		return valList.Contains(value);
	}

	public bool ContainsKey(K key)
	{
		return keyList.Contains(key);
	}

	public int IndexOf(K key)
	{
		return keyList.IndexOf(key);
	}

	public void Insert(int index, K key, V value)
	{
		if (keyList.Contains(key))
		{
			throw new Exception("Cannot insert duplicate key.");
		}
		keyList.Insert(index, key);
		valList.Insert(index, value);
	}

	public void Insert(int index, KeyValuePair<K, V> kvp)
	{
		Insert(index, kvp.Key, kvp.Value);
	}

	public void Insert(int index, object value)
	{
		string message = "Use Insert(K key, V value) or Insert(KeyValuePair<K, V>)";
		throw new NotImplementedException(message);
	}

	public void Remove(K key)
	{
		int num = keyList.IndexOf(key);
		if (num == -1)
		{
			throw new KeyNotFoundException();
		}
		keyList.RemoveAt(num);
		valList.RemoveAt(num);
	}

	public void Remove(object value)
	{
		throw new NotImplementedException("Use Remove(K key)");
	}

	public void RemoveAt(int index)
	{
		keyList.RemoveAt(index);
		valList.RemoveAt(index);
	}

	public V GetAt(int index)
	{
		if (index >= valList.Count)
		{
			throw new IndexOutOfRangeException();
		}
		return valList[index];
	}

	public void SetAt(int index, V value)
	{
		if (index >= valList.Count)
		{
			throw new IndexOutOfRangeException();
		}
		valList[index] = value;
	}

	public void CopyTo(V[] array, int index)
	{
		valList.CopyTo(array, index);
	}

	public void CopyTo(KeyValueList<K, V> otherKeyValueList, int index)
	{
		using (IEnumerator<KeyValuePair<K, V>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<K, V> current = enumerator.Current;
				otherKeyValueList[current.Key] = current.Value;
			}
		}
	}

	public void AddRange(KeyValueList<K, V> otherKeyValueList)
	{
		otherKeyValueList.CopyTo(this, 0);
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		foreach (K key in keyList)
		{
			yield return new KeyValuePair<K, V>(key, this[key]);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		foreach (K key in keyList)
		{
			yield return new KeyValuePair<K, V>(key, this[key]);
		}
	}

	public override string ToString()
	{
		string[] array = new string[keyList.Count];
		string format = "{0}:{1}";
		int num = 0;
		using (IEnumerator<KeyValuePair<K, V>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<K, V> current = enumerator.Current;
				array[num] = string.Format(format, current.Key, current.Value);
				num++;
			}
		}
		return string.Format("[{0}]", string.Join(", ", array));
	}

	public bool Contains(object value)
	{
		throw new NotImplementedException();
	}

	public int IndexOf(object value)
	{
		throw new NotImplementedException();
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}
}
