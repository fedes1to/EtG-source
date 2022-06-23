using System;
using System.Collections.Generic;
using FullSerializer.Internal;

namespace FullInspector.Internal
{
	public static class BehaviorTypeToSerializerTypeMap
	{
		private struct SerializationMapping
		{
			public Type BehaviorType;

			public Type SerializerType;
		}

		private static List<SerializationMapping> _mappings = new List<SerializationMapping>();

		public static void Register(Type behaviorType, Type serializerType)
		{
			_mappings.Add(new SerializationMapping
			{
				BehaviorType = behaviorType,
				SerializerType = serializerType
			});
		}

		public static Type GetSerializerType(Type behaviorType)
		{
			for (int i = 0; i < _mappings.Count; i++)
			{
				SerializationMapping serializationMapping = _mappings[i];
				if (serializationMapping.BehaviorType.Resolve().IsAssignableFrom(behaviorType.Resolve()))
				{
					return serializationMapping.SerializerType;
				}
			}
			return fiInstalledSerializerManager.DefaultMetadata.SerializerType;
		}
	}
}
