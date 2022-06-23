using System;
using System.Collections.Generic;
using FullInspector.Internal;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector
{
	public abstract class fiValue<T> : fiValueProxyEditor, fiIValueProxyAPI, ISerializationCallbackReceiver
	{
		public T Value;

		[HideInInspector]
		[SerializeField]
		private string SerializedState;

		[HideInInspector]
		[SerializeField]
		private List<UnityEngine.Object> SerializedObjectReferences;

		object fiIValueProxyAPI.Value
		{
			get
			{
				return Value;
			}
			set
			{
				Value = (T)value;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			Serialize();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			Deserialize();
		}

		void fiIValueProxyAPI.SaveState()
		{
			Serialize();
		}

		void fiIValueProxyAPI.LoadState()
		{
			Deserialize();
		}

		private void Serialize()
		{
			FullSerializerSerializer fullSerializerSerializer = fiSingletons.Get<FullSerializerSerializer>();
			ListSerializationOperator listSerializationOperator = fiSingletons.Get<ListSerializationOperator>();
			listSerializationOperator.SerializedObjects = new List<UnityEngine.Object>();
			try
			{
				SerializedState = fullSerializerSerializer.Serialize(typeof(T).Resolve(), Value, listSerializationOperator);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat("Exception caught when serializing ", this, " (with type ", GetType(), ")\n", ex));
			}
			SerializedObjectReferences = listSerializationOperator.SerializedObjects;
		}

		private void Deserialize()
		{
			if (SerializedObjectReferences == null)
			{
				SerializedObjectReferences = new List<UnityEngine.Object>();
			}
			FullSerializerSerializer fullSerializerSerializer = fiSingletons.Get<FullSerializerSerializer>();
			ListSerializationOperator listSerializationOperator = fiSingletons.Get<ListSerializationOperator>();
			listSerializationOperator.SerializedObjects = SerializedObjectReferences;
			if (!string.IsNullOrEmpty(SerializedState))
			{
				try
				{
					Value = (T)fullSerializerSerializer.Deserialize(typeof(T).Resolve(), SerializedState, listSerializationOperator);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Concat("Exception caught when deserializing ", this, " (with type ", GetType(), ");\n", ex));
				}
			}
		}
	}
}
