using System;
using System.Reflection;
using FullSerializer.Internal;

namespace FullInspector
{
	public abstract class BaseSerializer
	{
		public virtual bool SupportsMultithreading
		{
			get
			{
				return false;
			}
		}

		public abstract string Serialize(MemberInfo storageType, object value, ISerializationOperator serializationOperator);

		public abstract object Deserialize(MemberInfo storageType, string serializedState, ISerializationOperator serializationOperator);

		protected static Type GetStorageType(MemberInfo member)
		{
			if (member is FieldInfo)
			{
				return ((FieldInfo)member).FieldType;
			}
			if (member is PropertyInfo)
			{
				return ((PropertyInfo)member).PropertyType;
			}
			if (fsPortableReflection.IsType(member))
			{
				return fsPortableReflection.AsType(member);
			}
			throw new InvalidOperationException("Unknown member type " + member);
		}
	}
}
