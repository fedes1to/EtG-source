using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace FullSerializer.Internal
{
	public class fsDictionaryConverter : fsConverter
	{
		public override bool CanProcess(Type type)
		{
			return typeof(IDictionary).IsAssignableFrom(type);
		}

		public override object CreateInstance(fsData data, Type storageType)
		{
			return fsMetaType.Get(storageType).CreateInstance();
		}

		public override fsResult TryDeserialize(fsData data, ref object instance_, Type storageType)
		{
			IDictionary dictionary = (IDictionary)instance_;
			fsResult success = fsResult.Success;
			Type keyStorageType;
			Type valueStorageType;
			GetKeyValueTypes(dictionary.GetType(), out keyStorageType, out valueStorageType);
			if (data.IsList)
			{
				List<fsData> asList = data.AsList;
				for (int i = 0; i < asList.Count; i++)
				{
					fsData data2 = asList[i];
					if ((success += CheckType(data2, fsDataType.Object)).Failed)
					{
						return success;
					}
					fsData subitem;
					if ((success += CheckKey(data2, "Key", out subitem)).Failed)
					{
						return success;
					}
					fsData subitem2;
					if ((success += CheckKey(data2, "Value", out subitem2)).Failed)
					{
						return success;
					}
					object result = null;
					object result2 = null;
					if ((success += Serializer.TryDeserialize(subitem, keyStorageType, ref result)).Failed)
					{
						return success;
					}
					if ((success += Serializer.TryDeserialize(subitem2, valueStorageType, ref result2)).Failed)
					{
						return success;
					}
					AddItemToDictionary(dictionary, result, result2);
				}
				return success;
			}
			if (data.IsDictionary)
			{
				foreach (KeyValuePair<string, fsData> item in data.AsDictionary)
				{
					if (!fsSerializer.IsReservedKeyword(item.Key))
					{
						fsData data3 = new fsData(item.Key);
						fsData value = item.Value;
						object result3 = null;
						object result4 = null;
						if ((success += Serializer.TryDeserialize(data3, keyStorageType, ref result3)).Failed)
						{
							return success;
						}
						if ((success += Serializer.TryDeserialize(value, valueStorageType, ref result4)).Failed)
						{
							return success;
						}
						AddItemToDictionary(dictionary, result3, result4);
					}
				}
				return success;
			}
			return FailExpectedType(data, fsDataType.Array, fsDataType.Object);
		}

		public override fsResult TrySerialize(object instance_, out fsData serialized, Type storageType)
		{
			serialized = fsData.Null;
			fsResult success = fsResult.Success;
			IDictionary dictionary = (IDictionary)instance_;
			Type keyStorageType;
			Type valueStorageType;
			GetKeyValueTypes(dictionary.GetType(), out keyStorageType, out valueStorageType);
			IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
			bool flag = true;
			List<fsData> list = new List<fsData>(dictionary.Count);
			List<fsData> list2 = new List<fsData>(dictionary.Count);
			while (enumerator.MoveNext())
			{
				fsData data;
				if ((success += Serializer.TrySerialize(keyStorageType, enumerator.Key, out data)).Failed)
				{
					return success;
				}
				fsData data2;
				if ((success += Serializer.TrySerialize(valueStorageType, enumerator.Value, out data2)).Failed)
				{
					return success;
				}
				list.Add(data);
				list2.Add(data2);
				flag &= data.IsString;
			}
			if (flag)
			{
				serialized = fsData.CreateDictionary();
				Dictionary<string, fsData> asDictionary = serialized.AsDictionary;
				for (int i = 0; i < list.Count; i++)
				{
					fsData fsData = list[i];
					fsData value = list2[i];
					asDictionary[fsData.AsString] = value;
				}
			}
			else
			{
				serialized = fsData.CreateList(list.Count);
				List<fsData> asList = serialized.AsList;
				for (int j = 0; j < list.Count; j++)
				{
					fsData value2 = list[j];
					fsData value3 = list2[j];
					Dictionary<string, fsData> dictionary2 = new Dictionary<string, fsData>();
					dictionary2["Key"] = value2;
					dictionary2["Value"] = value3;
					asList.Add(new fsData(dictionary2));
				}
			}
			return success;
		}

		private fsResult AddItemToDictionary(IDictionary dictionary, object key, object value)
		{
			if (key == null || value == null)
			{
				Type @interface = fsReflectionUtility.GetInterface(dictionary.GetType(), typeof(ICollection<>));
				if (@interface == null)
				{
					return fsResult.Warn(string.Concat(dictionary.GetType(), " does not extend ICollection"));
				}
				Type type = @interface.GetGenericArguments()[0];
				object obj = Activator.CreateInstance(type, key, value);
				MethodInfo flattenedMethod = @interface.GetFlattenedMethod("Add");
				if (key == null)
				{
					return fsResult.Fail("Null entry in dictionary.");
				}
				flattenedMethod.Invoke(dictionary, new object[1] { obj });
				return fsResult.Success;
			}
			dictionary[key] = value;
			return fsResult.Success;
		}

		private static void GetKeyValueTypes(Type dictionaryType, out Type keyStorageType, out Type valueStorageType)
		{
			Type @interface = fsReflectionUtility.GetInterface(dictionaryType, typeof(IDictionary<, >));
			if (@interface != null)
			{
				Type[] genericArguments = @interface.GetGenericArguments();
				keyStorageType = genericArguments[0];
				valueStorageType = genericArguments[1];
			}
			else
			{
				keyStorageType = typeof(object);
				valueStorageType = typeof(object);
			}
		}
	}
}
