using System;

namespace FullInspector
{
	public interface fiISerializerMetadata
	{
		Guid SerializerGuid { get; }

		Type SerializerType { get; }

		Type[] SerializationOptInAnnotationTypes { get; }

		Type[] SerializationOptOutAnnotationTypes { get; }
	}
}
