using System;
using FullInspector.Serializers.FullSerializer;

namespace FullInspector.Internal
{
	public class fiLoadedSerializers : fiILoadedSerializers
	{
		public Type DefaultSerializerProvider
		{
			get
			{
				return typeof(FullSerializerMetadata);
			}
		}

		public Type[] AllLoadedSerializerProviders
		{
			get
			{
				return new Type[1] { typeof(FullSerializerMetadata) };
			}
		}
	}
}
