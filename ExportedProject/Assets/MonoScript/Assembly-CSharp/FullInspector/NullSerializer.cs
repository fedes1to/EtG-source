using System;
using System.Reflection;

namespace FullInspector
{
	[Obsolete("Please use [fiInspectorOnly]")]
	public class NullSerializer : BaseSerializer
	{
		public override string Serialize(MemberInfo storageType, object value, ISerializationOperator serializationOperator)
		{
			return null;
		}

		public override object Deserialize(MemberInfo storageType, string serializedState, ISerializationOperator serializationOperator)
		{
			return null;
		}
	}
}
