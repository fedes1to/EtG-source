using System;

namespace FullSerializer.Internal
{
	public struct fsVersionedType
	{
		public fsVersionedType[] Ancestors;

		public string VersionString;

		public Type ModelType;

		public object Migrate(object ancestorInstance)
		{
			return Activator.CreateInstance(ModelType, ancestorInstance);
		}

		public override string ToString()
		{
			return string.Concat("fsVersionedType [ModelType=", ModelType, ", VersionString=", VersionString, ", Ancestors.Length=", Ancestors.Length, "]");
		}

		public static bool operator ==(fsVersionedType a, fsVersionedType b)
		{
			return a.ModelType == b.ModelType;
		}

		public static bool operator !=(fsVersionedType a, fsVersionedType b)
		{
			return a.ModelType != b.ModelType;
		}

		public override bool Equals(object obj)
		{
			return obj is fsVersionedType && ModelType == ((fsVersionedType)obj).ModelType;
		}

		public override int GetHashCode()
		{
			return ModelType.GetHashCode();
		}
	}
}
