using System;
using System.Collections.Generic;
using FullInspector.Internal;
using UnityEngine;

namespace FullInspector
{
	[AddComponentMenu("")]
	public abstract class fiBaseStorageComponent<T> : MonoBehaviour, fiIEditorOnlyTag, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<UnityEngine.Object> _keys;

		[SerializeField]
		private List<T> _values;

		private IDictionary<UnityEngine.Object, T> _data;

		public IDictionary<UnityEngine.Object, T> Data
		{
			get
			{
				if (_data == null)
				{
					_data = new Dictionary<UnityEngine.Object, T>();
				}
				return _data;
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (_keys == null || _values == null)
			{
				return;
			}
			_data = new Dictionary<UnityEngine.Object, T>();
			for (int i = 0; i < Math.Min(_keys.Count, _values.Count); i++)
			{
				if (!object.ReferenceEquals(_keys[i], null))
				{
					Data[_keys[i]] = _values[i];
				}
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (_data == null)
			{
				_keys = null;
				_values = null;
				return;
			}
			_keys = new List<UnityEngine.Object>(_data.Count);
			_values = new List<T>(_data.Count);
			foreach (KeyValuePair<UnityEngine.Object, T> datum in _data)
			{
				_keys.Add(datum.Key);
				_values.Add(datum.Value);
			}
		}
	}
}
