using System;
using System.Collections;
using System.Collections.Generic;
using FullInspector.Internal;

namespace FullInspector
{
	internal class IntDictionary<TValue> : IDictionary<int, TValue>, ICollection<KeyValuePair<int, TValue>>, IEnumerable<KeyValuePair<int, TValue>>, IEnumerable
	{
		private List<fiOption<TValue>> _positives = new List<fiOption<TValue>>();

		private Dictionary<int, TValue> _negatives = new Dictionary<int, TValue>();

		public ICollection<int> Keys
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public TValue this[int key]
		{
			get
			{
				if (key < 0)
				{
					return _negatives[key];
				}
				if (key >= _positives.Count)
				{
					throw new KeyNotFoundException(string.Empty + key);
				}
				if (_positives[key].IsEmpty)
				{
					throw new KeyNotFoundException(string.Empty + key);
				}
				return _positives[key].Value;
			}
			set
			{
				if (key < 0)
				{
					_negatives[key] = value;
					return;
				}
				while (key >= _positives.Count)
				{
					_positives.Add(fiOption<TValue>.Empty);
				}
				_positives[key] = fiOption.Just(value);
			}
		}

		public int Count
		{
			get
			{
				int num = _negatives.Count;
				for (int i = 0; i < _positives.Count; i++)
				{
					if (_positives[i].HasValue)
					{
						num++;
					}
				}
				return num;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public void Add(int key, TValue value)
		{
			if (key < 0)
			{
				_negatives.Add(key, value);
				return;
			}
			while (key >= _positives.Count)
			{
				_positives.Add(fiOption<TValue>.Empty);
			}
			if (_positives[key].HasValue)
			{
				throw new Exception("Already have a key for " + key);
			}
			_positives[key] = fiOption.Just(value);
		}

		public bool ContainsKey(int key)
		{
			if (key < 0)
			{
				return _negatives.ContainsKey(key);
			}
			return key < _positives.Count && _positives[key].HasValue;
		}

		public bool Remove(int key)
		{
			if (key < 0)
			{
				return _negatives.Remove(key);
			}
			if (key >= _positives.Count)
			{
				return false;
			}
			if (_positives[key].IsEmpty)
			{
				return false;
			}
			_positives[key] = fiOption<TValue>.Empty;
			return true;
		}

		public bool TryGetValue(int key, out TValue value)
		{
			if (key < 0)
			{
				return _negatives.TryGetValue(key, out value);
			}
			value = default(TValue);
			if (key >= _positives.Count)
			{
				return false;
			}
			if (_positives[key].IsEmpty)
			{
				return false;
			}
			value = _positives[key].Value;
			return true;
		}

		public void Add(KeyValuePair<int, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			_negatives.Clear();
			_positives.Clear();
		}

		public bool Contains(KeyValuePair<int, TValue> item)
		{
			throw new NotSupportedException();
		}

		public void CopyTo(KeyValuePair<int, TValue>[] array, int arrayIndex)
		{
			using (IEnumerator<KeyValuePair<int, TValue>> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, TValue> current = enumerator.Current;
					if (arrayIndex >= array.Length)
					{
						break;
					}
					array[arrayIndex++] = current;
				}
			}
		}

		public bool Remove(KeyValuePair<int, TValue> item)
		{
			throw new NotSupportedException();
		}

		public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
		{
			foreach (KeyValuePair<int, TValue> negative in _negatives)
			{
				yield return negative;
			}
			for (int i = 0; i < _positives.Count; i++)
			{
				if (_positives[i].HasValue)
				{
					yield return new KeyValuePair<int, TValue>(i, _positives[i].Value);
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
