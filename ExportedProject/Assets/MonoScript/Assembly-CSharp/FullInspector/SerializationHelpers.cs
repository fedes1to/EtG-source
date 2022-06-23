using System;
using System.Collections.Generic;
using FullInspector.Internal;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector
{
	public static class SerializationHelpers
	{
		public static T DeserializeFromContent<T, TSerializer>(string content) where TSerializer : BaseSerializer
		{
			return (T)DeserializeFromContent<TSerializer>(typeof(T), content);
		}

		public static object DeserializeFromContent<TSerializer>(Type storageType, string content) where TSerializer : BaseSerializer
		{
			TSerializer val = fiSingletons.Get<TSerializer>();
			NotSupportedSerializationOperator serializationOperator = fiSingletons.Get<NotSupportedSerializationOperator>();
			return val.Deserialize(fsPortableReflection.AsMemberInfo(storageType), content, serializationOperator);
		}

		public static string SerializeToContent<T, TSerializer>(T value) where TSerializer : BaseSerializer
		{
			return SerializeToContent<TSerializer>(typeof(T), value);
		}

		public static string SerializeToContent<TSerializer>(Type storageType, object value) where TSerializer : BaseSerializer
		{
			TSerializer val = fiSingletons.Get<TSerializer>();
			NotSupportedSerializationOperator serializationOperator = fiSingletons.Get<NotSupportedSerializationOperator>();
			return val.Serialize(fsPortableReflection.AsMemberInfo(storageType), value, serializationOperator);
		}

		public static T Clone<T, TSerializer>(T obj) where TSerializer : BaseSerializer
		{
			return (T)Clone<TSerializer>(typeof(T), obj);
		}

		public static object Clone<TSerializer>(Type storageType, object obj) where TSerializer : BaseSerializer
		{
			TSerializer val = fiSingletons.Get<TSerializer>();
			ListSerializationOperator listSerializationOperator = fiSingletons.Get<ListSerializationOperator>();
			listSerializationOperator.SerializedObjects = new List<UnityEngine.Object>();
			string serializedState = val.Serialize(fsPortableReflection.AsMemberInfo(storageType), obj, listSerializationOperator);
			object result = val.Deserialize(fsPortableReflection.AsMemberInfo(storageType), serializedState, listSerializationOperator);
			listSerializationOperator.SerializedObjects = null;
			return result;
		}
	}
}
