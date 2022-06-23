using System;
using System.Collections;

namespace FullSerializer.Internal
{
	public class fsReflectedConverter : fsConverter
	{
		public override bool CanProcess(Type type)
		{
			if (type.Resolve().IsArray || typeof(ICollection).IsAssignableFrom(type))
			{
				return false;
			}
			return true;
		}

		public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
		{
			serialized = fsData.CreateDictionary();
			fsResult success = fsResult.Success;
			fsMetaType fsMetaType = fsMetaType.Get(instance.GetType());
			fsMetaType.EmitAotData();
			for (int i = 0; i < fsMetaType.Properties.Length; i++)
			{
				fsMetaProperty fsMetaProperty2 = fsMetaType.Properties[i];
				if (fsMetaProperty2.CanRead && !fsMetaProperty2.JsonDeserializeOnly)
				{
					fsData data;
					fsResult result = Serializer.TrySerialize(fsMetaProperty2.StorageType, fsMetaProperty2.Read(instance), out data);
					success.AddMessages(result);
					if (!result.Failed)
					{
						serialized.AsDictionary[fsMetaProperty2.JsonName] = data;
					}
				}
			}
			return success;
		}

		public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
		{
			fsResult success = fsResult.Success;
			if ((success += CheckType(data, fsDataType.Object)).Failed)
			{
				return success;
			}
			fsMetaType fsMetaType = fsMetaType.Get(storageType);
			fsMetaType.EmitAotData();
			for (int i = 0; i < fsMetaType.Properties.Length; i++)
			{
				fsMetaProperty fsMetaProperty2 = fsMetaType.Properties[i];
				fsData value;
				if (fsMetaProperty2.CanWrite && data.AsDictionary.TryGetValue(fsMetaProperty2.JsonName, out value))
				{
					object result = null;
					if (fsMetaProperty2.CanRead)
					{
						result = fsMetaProperty2.Read(instance);
					}
					fsResult result2 = Serializer.TryDeserialize(value, fsMetaProperty2.StorageType, ref result);
					success.AddMessages(result2);
					if (!result2.Failed)
					{
						fsMetaProperty2.Write(instance, result);
					}
				}
			}
			return success;
		}

		public override object CreateInstance(fsData data, Type storageType)
		{
			fsMetaType fsMetaType = fsMetaType.Get(storageType);
			return fsMetaType.CreateInstance();
		}
	}
}
