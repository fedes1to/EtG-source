using System;

namespace FullInspector
{
	public class TypeSpecifier<TBaseType>
	{
		public Type Type;

		public TypeSpecifier()
		{
		}

		public TypeSpecifier(Type type)
		{
			Type = type;
		}

		public static implicit operator Type(TypeSpecifier<TBaseType> specifier)
		{
			return specifier.Type;
		}

		public static implicit operator TypeSpecifier<TBaseType>(Type type)
		{
			TypeSpecifier<TBaseType> typeSpecifier = new TypeSpecifier<TBaseType>();
			typeSpecifier.Type = type;
			return typeSpecifier;
		}

		public override bool Equals(object obj)
		{
			TypeSpecifier<TBaseType> typeSpecifier = obj as TypeSpecifier<TBaseType>;
			return typeSpecifier != null && Type == typeSpecifier.Type;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode();
		}
	}
}
