using System;
using FullSerializer.Internal;

namespace FullInspector.Internal
{
	public static class TypeExtensions
	{
		public static bool IsNullableType(this Type type)
		{
			return type.Resolve().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		private static bool CompareTypes(Type a, Type b)
		{
			if (a.Resolve().IsGenericType && b.Resolve().IsGenericType && (a.Resolve().IsGenericTypeDefinition || b.Resolve().IsGenericTypeDefinition))
			{
				a = a.GetGenericTypeDefinition();
				b = b.GetGenericTypeDefinition();
			}
			return a == b;
		}

		public static bool HasParent(this Type type, Type parentType)
		{
			if (CompareTypes(type, parentType))
			{
				return false;
			}
			if (parentType.IsAssignableFrom(type))
			{
				return true;
			}
			while (type != null)
			{
				if (CompareTypes(type, parentType))
				{
					return true;
				}
				Type[] interfaces = type.GetInterfaces();
				foreach (Type a in interfaces)
				{
					if (CompareTypes(a, parentType))
					{
						return true;
					}
				}
				type = type.Resolve().BaseType;
			}
			return false;
		}

		public static Type GetInterface(this Type type, Type interfaceType)
		{
			if (interfaceType.Resolve().IsGenericType && !interfaceType.Resolve().IsGenericTypeDefinition)
			{
				throw new ArgumentException("GetInterface requires that if the interface type is generic, then it must be the generic type definition, not a specific generic type instantiation");
			}
			while (type != null)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					if (type2.Resolve().IsGenericType)
					{
						if (interfaceType == type2.GetGenericTypeDefinition())
						{
							return type2;
						}
					}
					else if (interfaceType == type2)
					{
						return type2;
					}
				}
				type = type.Resolve().BaseType;
			}
			return null;
		}

		public static bool IsImplementationOf(this Type type, Type interfaceType)
		{
			if (interfaceType.Resolve().IsGenericType && !interfaceType.Resolve().IsGenericTypeDefinition)
			{
				throw new ArgumentException("IsImplementationOf requires that if the interface type is generic, then it must be the generic type definition, not a specific generic type instantiation");
			}
			if (type.Resolve().IsGenericType)
			{
				type = type.GetGenericTypeDefinition();
			}
			while (type != null)
			{
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					if (type2.Resolve().IsGenericType)
					{
						if (interfaceType == type2.GetGenericTypeDefinition())
						{
							return true;
						}
					}
					else if (interfaceType == type2)
					{
						return true;
					}
				}
				type = type.Resolve().BaseType;
			}
			return false;
		}
	}
}
