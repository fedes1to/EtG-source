using System;

namespace FullSerializer.Internal
{
	public class fsSerializationCallbackProcessor : fsObjectProcessor
	{
		public override bool CanProcess(Type type)
		{
			return typeof(fsISerializationCallbacks).IsAssignableFrom(type);
		}

		public override void OnBeforeSerialize(Type storageType, object instance)
		{
			((fsISerializationCallbacks)instance).OnBeforeSerialize(storageType);
		}

		public override void OnAfterSerialize(Type storageType, object instance, ref fsData data)
		{
			((fsISerializationCallbacks)instance).OnAfterSerialize(storageType, ref data);
		}

		public override void OnBeforeDeserializeAfterInstanceCreation(Type storageType, object instance, ref fsData data)
		{
			if (!(instance is fsISerializationCallbacks))
			{
				throw new InvalidCastException(string.Concat("Please ensure the converter for ", storageType, " actually returns an instance of it, not an instance of ", instance.GetType()));
			}
			((fsISerializationCallbacks)instance).OnBeforeDeserialize(storageType, ref data);
		}

		public override void OnAfterDeserialize(Type storageType, object instance)
		{
			((fsISerializationCallbacks)instance).OnAfterDeserialize(storageType);
		}
	}
}
