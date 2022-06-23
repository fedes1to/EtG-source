using System;
using System.Collections.Generic;
using FullInspector.Internal;
using UnityEngine;

namespace FullInspector
{
	public class fiGraphMetadata
	{
		public struct MetadataMigration
		{
			public int NewIndex;

			public int OldIndex;
		}

		private Dictionary<string, List<object>> _precomputedData;

		[ShowInInspector]
		private CullableDictionary<int, fiGraphMetadata, IntDictionary<fiGraphMetadata>> _childrenInt;

		[ShowInInspector]
		private CullableDictionary<string, fiGraphMetadata, Dictionary<string, fiGraphMetadata>> _childrenString;

		[ShowInInspector]
		private CullableDictionary<Type, object, Dictionary<Type, object>> _metadata;

		private fiGraphMetadata _parentMetadata;

		private fiUnityObjectReference _targetObject;

		private string _accessPath;

		private UnityEngine.Object TargetObject
		{
			get
			{
				if (_targetObject != null && _targetObject.IsValid)
				{
					return _targetObject.Target;
				}
				if (_parentMetadata != null)
				{
					return _parentMetadata.TargetObject;
				}
				return null;
			}
		}

		public string Path
		{
			get
			{
				return _accessPath;
			}
		}

		public fiGraphMetadata()
			: this(null)
		{
		}

		public fiGraphMetadata(fiUnityObjectReference targetObject)
			: this(null, string.Empty)
		{
			_targetObject = targetObject;
		}

		private fiGraphMetadata(fiGraphMetadata parentMetadata, string accessKey)
		{
			_childrenInt = new CullableDictionary<int, fiGraphMetadata, IntDictionary<fiGraphMetadata>>();
			_childrenString = new CullableDictionary<string, fiGraphMetadata, Dictionary<string, fiGraphMetadata>>();
			_metadata = new CullableDictionary<Type, object, Dictionary<Type, object>>();
			_parentMetadata = parentMetadata;
			if (_parentMetadata == null)
			{
				_precomputedData = new Dictionary<string, List<object>>();
			}
			else
			{
				_precomputedData = _parentMetadata._precomputedData;
			}
			RebuildAccessPath(accessKey);
			if (!_precomputedData.ContainsKey(_accessPath))
			{
				return;
			}
			foreach (object item in _precomputedData[_accessPath])
			{
				_metadata[item.GetType()] = item;
			}
		}

		public bool ShouldSerialize()
		{
			return !_childrenInt.IsEmpty || !_childrenString.IsEmpty;
		}

		public void Serialize<TPersistentData>(out string[] keys_, out TPersistentData[] values_) where TPersistentData : IGraphMetadataItemPersistent
		{
			List<string> list = new List<string>();
			List<TPersistentData> list2 = new List<TPersistentData>();
			AddSerializeData(list, list2);
			keys_ = list.ToArray();
			values_ = list2.ToArray();
		}

		private void AddSerializeData<TPersistentData>(List<string> keys, List<TPersistentData> values) where TPersistentData : IGraphMetadataItemPersistent
		{
			foreach (KeyValuePair<Type, object> item in _metadata.Items)
			{
				if (item.Key == typeof(TPersistentData) && ((IGraphMetadataItemPersistent)item.Value).ShouldSerialize())
				{
					keys.Add(_accessPath);
					values.Add((TPersistentData)item.Value);
				}
			}
			foreach (KeyValuePair<int, fiGraphMetadata> item2 in _childrenInt.Items)
			{
				item2.Value.AddSerializeData(keys, values);
			}
			foreach (KeyValuePair<string, fiGraphMetadata> item3 in _childrenString.Items)
			{
				item3.Value.AddSerializeData(keys, values);
			}
		}

		public void Deserialize<TPersistentData>(string[] keys, TPersistentData[] values)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				string key = keys[i];
				List<object> value;
				if (!_precomputedData.TryGetValue(key, out value))
				{
					value = new List<object>();
					_precomputedData[key] = value;
				}
				value.Add(values[i]);
			}
		}

		public void BeginCullZone()
		{
			_childrenInt.BeginCullZone();
			_childrenString.BeginCullZone();
			_metadata.BeginCullZone();
		}

		public void EndCullZone()
		{
			_childrenInt.EndCullZone();
			_childrenString.EndCullZone();
			_metadata.EndCullZone();
		}

		private void RebuildAccessPath(string accessKey)
		{
			_accessPath = string.Empty;
			if (_parentMetadata != null && !string.IsNullOrEmpty(_parentMetadata._accessPath))
			{
				_accessPath = _accessPath + _parentMetadata._accessPath + ".";
			}
			_accessPath += accessKey;
		}

		public void SetChild(int identifier, fiGraphMetadata metadata)
		{
			_childrenInt[identifier] = metadata;
			metadata.RebuildAccessPath(identifier.ToString());
		}

		public void SetChild(string identifier, fiGraphMetadata metadata)
		{
			_childrenString[identifier] = metadata;
			metadata.RebuildAccessPath(identifier);
		}

		public static void MigrateMetadata<T>(fiGraphMetadata metadata, T[] previous, T[] updated)
		{
			List<MetadataMigration> list = ComputeNeededMigrations(metadata, previous, updated);
			string[] array = new string[list.Count];
			string[] array2 = new string[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				array[i] = metadata.Enter(list[i].OldIndex).Metadata._accessPath;
				array2[i] = metadata.Enter(list[i].NewIndex).Metadata._accessPath;
			}
			List<fiGraphMetadata> list2 = new List<fiGraphMetadata>(list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				list2.Add(metadata._childrenInt[j]);
			}
			for (int k = 0; k < list.Count; k++)
			{
				metadata._childrenInt[list[k].NewIndex] = list2[k];
			}
		}

		private static List<MetadataMigration> ComputeNeededMigrations<T>(fiGraphMetadata metadata, T[] previous, T[] updated)
		{
			List<MetadataMigration> list = new List<MetadataMigration>();
			for (int i = 0; i < updated.Length; i++)
			{
				int num = Array.IndexOf(previous, updated[i]);
				if (num != -1 && num != i)
				{
					list.Add(new MetadataMigration
					{
						NewIndex = i,
						OldIndex = num
					});
				}
			}
			return list;
		}

		public fiGraphMetadataChild Enter(int childIdentifier)
		{
			fiGraphMetadata value;
			if (!_childrenInt.TryGetValue(childIdentifier, out value))
			{
				value = new fiGraphMetadata(this, childIdentifier.ToString());
				_childrenInt[childIdentifier] = value;
			}
			fiGraphMetadataChild result = default(fiGraphMetadataChild);
			result.Metadata = value;
			return result;
		}

		public fiGraphMetadataChild Enter(string childIdentifier)
		{
			fiGraphMetadata value;
			if (!_childrenString.TryGetValue(childIdentifier, out value))
			{
				value = new fiGraphMetadata(this, childIdentifier);
				_childrenString[childIdentifier] = value;
			}
			fiGraphMetadataChild result = default(fiGraphMetadataChild);
			result.Metadata = value;
			return result;
		}

		public T GetPersistentMetadata<T>() where T : IGraphMetadataItemPersistent, new()
		{
			bool wasCreated;
			return GetPersistentMetadata<T>(out wasCreated);
		}

		public T GetPersistentMetadata<T>(out bool wasCreated) where T : IGraphMetadataItemPersistent, new()
		{
			return GetCommonMetadata<T>(out wasCreated);
		}

		public T GetMetadata<T>() where T : IGraphMetadataItemNotPersistent, new()
		{
			bool wasCreated;
			return GetMetadata<T>(out wasCreated);
		}

		public T GetMetadata<T>(out bool wasCreated) where T : IGraphMetadataItemNotPersistent, new()
		{
			return GetCommonMetadata<T>(out wasCreated);
		}

		private T GetCommonMetadata<T>(out bool wasCreated) where T : new()
		{
			object value;
			if (!_metadata.TryGetValue(typeof(T), out value))
			{
				value = new T();
				_metadata[typeof(T)] = value;
				wasCreated = true;
			}
			else
			{
				wasCreated = false;
			}
			return (T)value;
		}

		public T GetInheritedMetadata<T>() where T : IGraphMetadataItemNotPersistent, new()
		{
			object value;
			if (_metadata.TryGetValue(typeof(T), out value))
			{
				return (T)value;
			}
			if (_parentMetadata == null)
			{
				return GetMetadata<T>();
			}
			return _parentMetadata.GetInheritedMetadata<T>();
		}

		public bool TryGetMetadata<T>(out T metadata) where T : IGraphMetadataItemNotPersistent, new()
		{
			object value;
			bool result = _metadata.TryGetValue(typeof(T), out value);
			metadata = (T)value;
			return result;
		}

		public bool TryGetInheritedMetadata<T>(out T metadata) where T : IGraphMetadataItemNotPersistent, new()
		{
			object value;
			if (_metadata.TryGetValue(typeof(T), out value))
			{
				metadata = (T)value;
				return true;
			}
			if (_parentMetadata == null)
			{
				metadata = default(T);
				return false;
			}
			return _parentMetadata.TryGetInheritedMetadata<T>(out metadata);
		}
	}
}
