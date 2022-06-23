using System;
using System.Collections.Generic;
using System.Linq;
using FullSerializer.Internal;

namespace FullSerializer
{
	public abstract class fsBaseConverter
	{
		public fsSerializer Serializer;

		public virtual object CreateInstance(fsData data, Type storageType)
		{
			if (RequestCycleSupport(storageType))
			{
				throw new InvalidOperationException(string.Concat("Please override CreateInstance for ", GetType().FullName, "; the object graph for ", storageType, " can contain potentially contain cycles, so separated instance creation is needed"));
			}
			return storageType;
		}

		public virtual bool RequestCycleSupport(Type storageType)
		{
			if (storageType == typeof(string))
			{
				return false;
			}
			return storageType.Resolve().IsClass || storageType.Resolve().IsInterface;
		}

		public virtual bool RequestInheritanceSupport(Type storageType)
		{
			return !storageType.Resolve().IsSealed;
		}

		public abstract fsResult TrySerialize(object instance, out fsData serialized, Type storageType);

		public abstract fsResult TryDeserialize(fsData data, ref object instance, Type storageType);

		protected fsResult FailExpectedType(fsData data, params fsDataType[] types)
		{
			return fsResult.Fail(string.Concat(GetType().Name, " expected one of ", string.Join(", ", types.Select((fsDataType t) => t.ToString()).ToArray()), " but got ", data.Type, " in ", data));
		}

		protected fsResult CheckType(fsData data, fsDataType type)
		{
			if (data.Type != type)
			{
				return fsResult.Fail(string.Concat(GetType().Name, " expected ", type, " but got ", data.Type, " in ", data));
			}
			return fsResult.Success;
		}

		protected fsResult CheckKey(fsData data, string key, out fsData subitem)
		{
			return CheckKey(data.AsDictionary, key, out subitem);
		}

		protected fsResult CheckKey(Dictionary<string, fsData> data, string key, out fsData subitem)
		{
			if (!data.TryGetValue(key, out subitem))
			{
				return fsResult.Fail(GetType().Name + " requires a <" + key + "> key in the data " + data);
			}
			return fsResult.Success;
		}

		protected fsResult SerializeMember<T>(Dictionary<string, fsData> data, string name, T value)
		{
			fsData data2;
			fsResult result = Serializer.TrySerialize(typeof(T), value, out data2);
			if (result.Succeeded)
			{
				data[name] = data2;
			}
			return result;
		}

		protected fsResult DeserializeMember<T>(Dictionary<string, fsData> data, string name, out T value)
		{
			fsData value2;
			if (!data.TryGetValue(name, out value2))
			{
				value = default(T);
				return fsResult.Fail("Unable to find member \"" + name + "\"");
			}
			object result = null;
			fsResult result2 = Serializer.TryDeserialize(value2, typeof(T), ref result);
			value = (T)result;
			return result2;
		}
	}
}
