using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Internal
{
	public class fiSerializedObjectSnapshot
	{
		private readonly List<string> _keys;

		private readonly List<string> _values;

		private readonly List<Object> _objectReferences;

		public bool IsEmpty
		{
			get
			{
				return _keys.Count == 0 || _values.Count == 0;
			}
		}

		public fiSerializedObjectSnapshot(ISerializedObject obj)
		{
			_keys = new List<string>(obj.SerializedStateKeys);
			_values = new List<string>(obj.SerializedStateValues);
			_objectReferences = new List<Object>(obj.SerializedObjectReferences);
		}

		public void RestoreSnapshot(ISerializedObject target)
		{
			target.SerializedStateKeys = new List<string>(_keys);
			target.SerializedStateValues = new List<string>(_values);
			target.SerializedObjectReferences = new List<Object>(_objectReferences);
			target.RestoreState();
		}

		public override bool Equals(object obj)
		{
			fiSerializedObjectSnapshot fiSerializedObjectSnapshot2 = obj as fiSerializedObjectSnapshot;
			if (object.ReferenceEquals(fiSerializedObjectSnapshot2, null))
			{
				return false;
			}
			return AreEqual(_keys, fiSerializedObjectSnapshot2._keys) && AreEqual(_values, fiSerializedObjectSnapshot2._values) && AreEqual(_objectReferences, fiSerializedObjectSnapshot2._objectReferences);
		}

		public override int GetHashCode()
		{
			int num = 13;
			num = num * 7 + _keys.GetHashCode();
			num = num * 7 + _values.GetHashCode();
			return num * 7 + _objectReferences.GetHashCode();
		}

		public static bool operator ==(fiSerializedObjectSnapshot a, fiSerializedObjectSnapshot b)
		{
			return object.Equals(a, b);
		}

		public static bool operator !=(fiSerializedObjectSnapshot a, fiSerializedObjectSnapshot b)
		{
			return !object.Equals(a, b);
		}

		private static bool AreEqual<T>(List<T> a, List<T> b)
		{
			if (a.Count != b.Count)
			{
				return false;
			}
			for (int i = 0; i < a.Count; i++)
			{
				if (!EqualityComparer<T>.Default.Equals(a[i], b[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
