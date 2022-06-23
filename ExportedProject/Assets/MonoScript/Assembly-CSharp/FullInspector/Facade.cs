using System;
using System.Collections.Generic;
using FullInspector.Internal;
using FullSerializer;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector
{
	public class Facade<T>
	{
		public Type InstanceType;

		public Dictionary<string, string> FacadeMembers = new Dictionary<string, string>();

		public List<UnityEngine.Object> ObjectReferences = new List<UnityEngine.Object>();

		public void PopulateInstance(ref T instance)
		{
			if (instance.GetType() != InstanceType)
			{
				Debug.LogWarning("PopulateInstance: Actual Facade type is different (instance.GetType() = " + instance.GetType().CSharpName() + ", InstanceType = " + InstanceType.CSharpName() + ")");
			}
			Type serializerType = fiInstalledSerializerManager.DefaultMetadata.SerializerType;
			BaseSerializer baseSerializer = (BaseSerializer)fiSingletons.Get(serializerType);
			ListSerializationOperator listSerializationOperator = new ListSerializationOperator();
			listSerializationOperator.SerializedObjects = ObjectReferences;
			ListSerializationOperator serializationOperator = listSerializationOperator;
			InspectedType inspectedType = InspectedType.Get(instance.GetType());
			foreach (KeyValuePair<string, string> facadeMember in FacadeMembers)
			{
				string key = facadeMember.Key;
				InspectedProperty propertyByName = inspectedType.GetPropertyByName(key);
				if (propertyByName != null)
				{
					try
					{
						object value = baseSerializer.Deserialize(propertyByName.StorageType.Resolve(), facadeMember.Value, serializationOperator);
						propertyByName.Write(instance, value);
					}
					catch (Exception ex)
					{
						Debug.LogError("Skipping property " + key + " in facade due to deserialization exception.\n" + ex);
					}
				}
			}
		}

		public T ConstructInstance()
		{
			T instance = (T)Activator.CreateInstance(InstanceType);
			PopulateInstance(ref instance);
			return instance;
		}

		public T ConstructInstance(GameObject context)
		{
			T instance = ((!typeof(Component).IsAssignableFrom(InstanceType)) ? ((T)Activator.CreateInstance(InstanceType)) : ((T)(object)context.AddComponent(InstanceType)));
			PopulateInstance(ref instance);
			return instance;
		}
	}
}
