using System;
using System.Collections.Generic;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector.Internal
{
	public static class fiISerializedObjectUtility
	{
		private static bool SaveStateForProperty(ISerializedObject obj, InspectedProperty property, BaseSerializer serializer, ISerializationOperator serializationOperator, out string serializedValue, ref bool success)
		{
			object obj2 = property.Read(obj);
			try
			{
				if (obj2 == null)
				{
					serializedValue = null;
				}
				else
				{
					serializedValue = serializer.Serialize(property.MemberInfo, obj2, serializationOperator);
				}
				return true;
			}
			catch (Exception ex)
			{
				success = false;
				serializedValue = null;
				Debug.LogError(string.Concat("Exception caught when serializing property <", property.Name, "> in <", obj, "> with value ", obj2, "\n", ex));
				return false;
			}
		}

		public static bool SaveState<TSerializer>(ISerializedObject obj) where TSerializer : BaseSerializer
		{
			bool success = true;
			ISerializationCallbacks serializationCallbacks = obj as ISerializationCallbacks;
			if (serializationCallbacks != null)
			{
				serializationCallbacks.OnBeforeSerialize();
			}
			TSerializer serializer = fiSingletons.Get<TSerializer>();
			ListSerializationOperator listSerializationOperator = fiSingletons.Get<ListSerializationOperator>();
			listSerializationOperator.SerializedObjects = new List<UnityEngine.Object>();
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			if (fiUtility.IsEditor || obj.SerializedStateKeys == null || obj.SerializedStateKeys.Count == 0)
			{
				List<InspectedProperty> properties = InspectedType.Get(obj.GetType()).GetProperties(InspectedMemberFilters.FullInspectorSerializedProperties);
				for (int i = 0; i < properties.Count; i++)
				{
					InspectedProperty inspectedProperty = properties[i];
					string serializedValue;
					if (SaveStateForProperty(obj, inspectedProperty, serializer, listSerializationOperator, out serializedValue, ref success))
					{
						list.Add(inspectedProperty.Name);
						list2.Add(serializedValue);
					}
				}
			}
			else
			{
				InspectedType inspectedType = InspectedType.Get(obj.GetType());
				for (int j = 0; j < obj.SerializedStateKeys.Count; j++)
				{
					InspectedProperty inspectedProperty2 = inspectedType.GetPropertyByName(obj.SerializedStateKeys[j]) ?? inspectedType.GetPropertyByFormerlySerializedName(obj.SerializedStateKeys[j]);
					string serializedValue2;
					if (inspectedProperty2 != null && SaveStateForProperty(obj, inspectedProperty2, serializer, listSerializationOperator, out serializedValue2, ref success))
					{
						list.Add(inspectedProperty2.Name);
						list2.Add(serializedValue2);
					}
				}
			}
			if (AreListsDifferent(obj.SerializedStateKeys, list))
			{
				obj.SerializedStateKeys = list;
			}
			if (AreListsDifferent(obj.SerializedStateValues, list2))
			{
				obj.SerializedStateValues = list2;
			}
			if (AreListsDifferent(obj.SerializedObjectReferences, listSerializationOperator.SerializedObjects))
			{
				obj.SerializedObjectReferences = listSerializationOperator.SerializedObjects;
			}
			if (obj is ScriptableObject)
			{
				fiLateBindings.EditorUtility.SetDirty((ScriptableObject)obj);
			}
			if (serializationCallbacks != null)
			{
				serializationCallbacks.OnAfterSerialize();
			}
			return success;
		}

		private static bool AreListsDifferent(IList<string> a, IList<string> b)
		{
			if (a == null)
			{
				return true;
			}
			if (a.Count != b.Count)
			{
				return true;
			}
			int count = a.Count;
			for (int i = 0; i < count; i++)
			{
				if (a[i] != b[i])
				{
					return true;
				}
			}
			return false;
		}

		private static bool AreListsDifferent(IList<UnityEngine.Object> a, IList<UnityEngine.Object> b)
		{
			if (a == null)
			{
				return true;
			}
			if (a.Count != b.Count)
			{
				return true;
			}
			int count = a.Count;
			for (int i = 0; i < count; i++)
			{
				if (!object.ReferenceEquals(a[i], b[i]))
				{
					return true;
				}
			}
			return false;
		}

		public static bool RestoreState<TSerializer>(ISerializedObject obj) where TSerializer : BaseSerializer
		{
			bool result = true;
			ISerializationCallbacks serializationCallbacks = obj as ISerializationCallbacks;
			if (serializationCallbacks != null)
			{
				serializationCallbacks.OnBeforeDeserialize();
			}
			if (obj.SerializedStateKeys == null)
			{
				obj.SerializedStateKeys = new List<string>();
			}
			if (obj.SerializedStateValues == null)
			{
				obj.SerializedStateValues = new List<string>();
			}
			if (obj.SerializedObjectReferences == null)
			{
				obj.SerializedObjectReferences = new List<UnityEngine.Object>();
			}
			if (obj.SerializedStateKeys.Count != obj.SerializedStateValues.Count && fiSettings.EmitWarnings)
			{
				Debug.LogWarning("Serialized key count does not equal value count; possible data corruption / bad manual edit?", obj as UnityEngine.Object);
			}
			if (obj.SerializedStateKeys.Count == 0)
			{
				if (fiSettings.AutomaticReferenceInstantation)
				{
					InstantiateReferences(obj, null);
				}
				return result;
			}
			TSerializer val = fiSingletons.Get<TSerializer>();
			ListSerializationOperator listSerializationOperator = fiSingletons.Get<ListSerializationOperator>();
			listSerializationOperator.SerializedObjects = obj.SerializedObjectReferences;
			InspectedType inspectedType = InspectedType.Get(obj.GetType());
			for (int i = 0; i < obj.SerializedStateKeys.Count; i++)
			{
				string text = obj.SerializedStateKeys[i];
				string text2 = obj.SerializedStateValues[i];
				InspectedProperty inspectedProperty = inspectedType.GetPropertyByName(text) ?? inspectedType.GetPropertyByFormerlySerializedName(text);
				if (inspectedProperty == null)
				{
					if (fiSettings.EmitWarnings)
					{
						Debug.LogWarning("Unable to find serialized property with name=" + text + " on type " + obj.GetType(), obj as UnityEngine.Object);
					}
					continue;
				}
				object value = null;
				if (!string.IsNullOrEmpty(text2))
				{
					try
					{
						value = val.Deserialize(inspectedProperty.MemberInfo, text2, listSerializationOperator);
					}
					catch (Exception ex)
					{
						result = false;
						Debug.LogError(string.Concat("Exception caught when deserializing property <", text, "> in <", obj, ">\n", ex), obj as UnityEngine.Object);
					}
				}
				try
				{
					inspectedProperty.Write(obj, value);
				}
				catch (Exception message)
				{
					result = false;
					if (fiSettings.EmitWarnings)
					{
						Debug.LogWarning("Caught exception when updating property value; see next message for the exception", obj as UnityEngine.Object);
						Debug.LogError(message);
					}
				}
			}
			if (serializationCallbacks != null)
			{
				serializationCallbacks.OnAfterDeserialize();
			}
			obj.IsRestored = true;
			return result;
		}

		private static void InstantiateReferences(object obj, InspectedType metadata)
		{
			if (metadata == null)
			{
				metadata = InspectedType.Get(obj.GetType());
			}
			if (metadata.IsCollection)
			{
				return;
			}
			List<InspectedProperty> properties = metadata.GetProperties(InspectedMemberFilters.InspectableMembers);
			for (int i = 0; i < properties.Count; i++)
			{
				InspectedProperty inspectedProperty = properties[i];
				if (!inspectedProperty.StorageType.Resolve().IsClass || inspectedProperty.StorageType.Resolve().IsAbstract)
				{
					continue;
				}
				object obj2 = inspectedProperty.Read(obj);
				if (obj2 == null)
				{
					InspectedType inspectedType = InspectedType.Get(inspectedProperty.StorageType);
					if (inspectedType.HasDefaultConstructor)
					{
						object obj3 = inspectedType.CreateInstance();
						inspectedProperty.Write(obj, obj3);
						InstantiateReferences(obj3, inspectedType);
					}
				}
			}
		}
	}
}
