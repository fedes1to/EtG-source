using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Internal
{
	public class ListSerializationOperator : ISerializationOperator
	{
		public List<UnityEngine.Object> SerializedObjects;

		public UnityEngine.Object RetrieveObjectReference(int storageId)
		{
			if (SerializedObjects == null)
			{
				throw new InvalidOperationException("SerializedObjects cannot be  null");
			}
			if (storageId < 0 || storageId >= SerializedObjects.Count)
			{
				return null;
			}
			return SerializedObjects[storageId];
		}

		public int StoreObjectReference(UnityEngine.Object obj)
		{
			if (SerializedObjects == null)
			{
				throw new InvalidOperationException("SerializedObjects cannot be null");
			}
			if (object.ReferenceEquals(obj, null))
			{
				return -1;
			}
			int count = SerializedObjects.Count;
			SerializedObjects.Add(obj);
			return count;
		}
	}
}
