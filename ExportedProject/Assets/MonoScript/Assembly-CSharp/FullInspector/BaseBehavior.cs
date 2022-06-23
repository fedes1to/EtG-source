using System.Collections.Generic;
using Beebyte.Obfuscator;
using FullInspector.Internal;
using UnityEngine;

namespace FullInspector
{
	[Skip]
	public abstract class BaseBehavior<TSerializer> : CommonBaseBehavior, ISerializedObject, ISerializationCallbackReceiver where TSerializer : BaseSerializer
	{
		[NotSerialized]
		[HideInInspector]
		[SerializeField]
		private List<Object> _objectReferences;

		[HideInInspector]
		[NotSerialized]
		[SerializeField]
		private List<string> _serializedStateKeys;

		[HideInInspector]
		[SerializeField]
		[NotSerialized]
		private List<string> _serializedStateValues;

		List<Object> ISerializedObject.SerializedObjectReferences
		{
			get
			{
				return _objectReferences;
			}
			set
			{
				_objectReferences = value;
			}
		}

		List<string> ISerializedObject.SerializedStateKeys
		{
			get
			{
				return _serializedStateKeys;
			}
			set
			{
				_serializedStateKeys = value;
			}
		}

		List<string> ISerializedObject.SerializedStateValues
		{
			get
			{
				return _serializedStateValues;
			}
			set
			{
				_serializedStateValues = value;
			}
		}

		bool ISerializedObject.IsRestored { get; set; }

		static BaseBehavior()
		{
			BehaviorTypeToSerializerTypeMap.Register(typeof(BaseBehavior<TSerializer>), typeof(TSerializer));
		}

		protected virtual void Awake()
		{
			fiSerializationManager.OnUnityObjectAwake<TSerializer>(this);
		}

		protected virtual void OnValidate()
		{
			if (!Application.isPlaying && !((ISerializedObject)this).IsRestored)
			{
				RestoreState();
			}
		}

		[ContextMenu("Save Current State")]
		public void SaveState()
		{
			fiISerializedObjectUtility.SaveState<TSerializer>(this);
		}

		[ContextMenu("Restore Saved State")]
		public void RestoreState()
		{
			fiISerializedObjectUtility.RestoreState<TSerializer>(this);
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			((ISerializedObject)this).IsRestored = false;
			fiSerializationManager.OnUnityObjectDeserialize<TSerializer>(this);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			fiSerializationManager.OnUnityObjectSerialize<TSerializer>(this);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
	public abstract class BaseBehavior : BaseBehavior<FullSerializerSerializer>
	{
	}
}
