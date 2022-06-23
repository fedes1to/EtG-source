using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Internal
{
	public class CullableDictionary<TKey, TValue, TDictionary> : ICullableDictionary<TKey, TValue> where TDictionary : IDictionary<TKey, TValue>, new()
	{
		[SerializeField]
		private TDictionary _primary;

		[SerializeField]
		private TDictionary _culled;

		[SerializeField]
		private bool _isCulling;

		public TValue this[TKey key]
		{
			get
			{
				TValue value;
				if (!TryGetValue(key, out value))
				{
					throw new KeyNotFoundException(string.Empty + key);
				}
				return value;
			}
			set
			{
				_culled.Remove(key);
				_primary[key] = value;
			}
		}

		public IEnumerable<KeyValuePair<TKey, TValue>> Items
		{
			get
			{
				foreach (KeyValuePair<TKey, TValue> item in _primary)
				{
					yield return item;
				}
				foreach (KeyValuePair<TKey, TValue> item2 in _culled)
				{
					yield return item2;
				}
			}
		}

		public bool IsEmpty
		{
			get
			{
				return _primary.Count == 0 && _culled.Count == 0;
			}
		}

		public CullableDictionary()
		{
			_primary = new TDictionary();
			_culled = new TDictionary();
		}

		public void Add(TKey key, TValue value)
		{
			_primary.Add(key, value);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (_culled.TryGetValue(key, out value))
			{
				_culled.Remove(key);
				_primary.Add(key, value);
				return true;
			}
			return _primary.TryGetValue(key, out value);
		}

		public void BeginCullZone()
		{
			if (!_isCulling)
			{
				fiUtility.Swap(ref _primary, ref _culled);
				_isCulling = true;
			}
		}

		public void EndCullZone()
		{
			if (_isCulling)
			{
				_isCulling = false;
			}
			if (fiSettings.EmitGraphMetadataCulls && _culled.Count > 0)
			{
				foreach (KeyValuePair<TKey, TValue> item in _culled)
				{
					Debug.Log(string.Concat("fiGraphMetadata culling \"", item.Key, "\""));
				}
			}
			_culled.Clear();
		}
	}
}
