using UnityEngine;

namespace FullInspector.Internal
{
	public class fiGraphMetadataSerializer<TPersistentData> : fiIGraphMetadataStorage, ISerializationCallbackReceiver where TPersistentData : IGraphMetadataItemPersistent
	{
		[SerializeField]
		private string[] _keys;

		[SerializeField]
		private TPersistentData[] _values;

		[SerializeField]
		private Object _target;

		public void RestoreData(Object target)
		{
			_target = target;
			if (_keys != null && _values != null)
			{
				fiPersistentMetadata.GetMetadataFor(_target).Deserialize(_keys, _values);
			}
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (!(_target == null))
			{
				fiGraphMetadata metadataFor = fiPersistentMetadata.GetMetadataFor(_target);
				if (metadataFor.ShouldSerialize())
				{
					metadataFor.Serialize<TPersistentData>(out _keys, out _values);
				}
			}
		}
	}
}
