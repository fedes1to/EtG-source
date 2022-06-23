using System;
using FullSerializer;
using UnityEngine;

namespace FullInspector.Serializers.FullSerializer
{
	public class FullSerializerMetadata : fiISerializerMetadata
	{
		public Guid SerializerGuid
		{
			get
			{
				return new Guid("bc898177-6ff4-423f-91bb-589bc83d8fde");
			}
		}

		public Type SerializerType
		{
			get
			{
				return typeof(FullSerializerSerializer);
			}
		}

		public Type[] SerializationOptInAnnotationTypes
		{
			get
			{
				return new Type[2]
				{
					typeof(SerializeField),
					typeof(fsPropertyAttribute)
				};
			}
		}

		public Type[] SerializationOptOutAnnotationTypes
		{
			get
			{
				return new Type[1] { typeof(fsIgnoreAttribute) };
			}
		}
	}
}
