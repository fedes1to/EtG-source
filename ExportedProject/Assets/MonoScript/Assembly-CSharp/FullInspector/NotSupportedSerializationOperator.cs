using System;
using UnityEngine;

namespace FullInspector
{
	public class NotSupportedSerializationOperator : ISerializationOperator
	{
		public UnityEngine.Object RetrieveObjectReference(int storageId)
		{
			throw new NotSupportedException("UnityEngine.Object references are not supported with this serialization operator");
		}

		public int StoreObjectReference(UnityEngine.Object obj)
		{
			throw new NotSupportedException(string.Concat("UnityEngine.Object references are not supported with this serialization operator (obj=", obj, ")"));
		}
	}
}
