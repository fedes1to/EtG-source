using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FullSerializer.Internal
{
	public static class fsPortableReflection
	{
		private struct AttributeQuery
		{
			public MemberInfo MemberInfo;

			public Type AttributeType;
		}

		private class AttributeQueryComparator : IEqualityComparer<AttributeQuery>
		{
			public bool Equals(AttributeQuery x, AttributeQuery y)
			{
				return x.MemberInfo == y.MemberInfo && x.AttributeType == y.AttributeType;
			}

			public int GetHashCode(AttributeQuery obj)
			{
				return obj.MemberInfo.GetHashCode() + 17 * obj.AttributeType.GetHashCode();
			}
		}

		public static Type[] EmptyTypes = new Type[0];

		private static IDictionary<AttributeQuery, Attribute> _cachedAttributeQueries = new Dictionary<AttributeQuery, Attribute>(new AttributeQueryComparator());

		private static BindingFlags DeclaredFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public static bool HasAttribute(MemberInfo element, Type attributeType)
		{
			return GetAttribute(element, attributeType) != null;
		}

		public static bool HasAttribute<TAttribute>(MemberInfo element)
		{
			return HasAttribute(element, typeof(TAttribute));
		}

		public static Attribute GetAttribute(MemberInfo element, Type attributeType)
		{
			AttributeQuery attributeQuery = default(AttributeQuery);
			attributeQuery.MemberInfo = element;
			attributeQuery.AttributeType = attributeType;
			AttributeQuery key = attributeQuery;
			Attribute value;
			if (!_cachedAttributeQueries.TryGetValue(key, out value))
			{
				object[] customAttributes = element.GetCustomAttributes(attributeType, true);
				value = (Attribute)customAttributes.FirstOrDefault();
				_cachedAttributeQueries[key] = value;
			}
			return value;
		}

		public static TAttribute GetAttribute<TAttribute>(MemberInfo element) where TAttribute : Attribute
		{
			return (TAttribute)GetAttribute(element, typeof(TAttribute));
		}

		public static PropertyInfo GetDeclaredProperty(this Type type, string propertyName)
		{
			PropertyInfo[] declaredProperties = type.GetDeclaredProperties();
			for (int i = 0; i < declaredProperties.Length; i++)
			{
				if (declaredProperties[i].Name == propertyName)
				{
					return declaredProperties[i];
				}
			}
			return null;
		}

		public static MethodInfo GetDeclaredMethod(this Type type, string methodName)
		{
			MethodInfo[] declaredMethods = type.GetDeclaredMethods();
			for (int i = 0; i < declaredMethods.Length; i++)
			{
				if (declaredMethods[i].Name == methodName)
				{
					return declaredMethods[i];
				}
			}
			return null;
		}

		public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] parameters)
		{
			ConstructorInfo[] declaredConstructors = type.GetDeclaredConstructors();
			foreach (ConstructorInfo constructorInfo in declaredConstructors)
			{
				ParameterInfo[] parameters2 = constructorInfo.GetParameters();
				if (parameters.Length != parameters2.Length)
				{
					continue;
				}
				for (int j = 0; j < parameters2.Length; j++)
				{
					if (parameters2[j].ParameterType != parameters[j])
					{
					}
				}
				return constructorInfo;
			}
			return null;
		}

		public static ConstructorInfo[] GetDeclaredConstructors(this Type type)
		{
			return type.GetConstructors(DeclaredFlags);
		}

		public static MemberInfo[] GetFlattenedMember(this Type type, string memberName)
		{
			List<MemberInfo> list = new List<MemberInfo>();
			while (type != null)
			{
				MemberInfo[] declaredMembers = type.GetDeclaredMembers();
				for (int i = 0; i < declaredMembers.Length; i++)
				{
					if (declaredMembers[i].Name == memberName)
					{
						list.Add(declaredMembers[i]);
					}
				}
				type = type.Resolve().BaseType;
			}
			return list.ToArray();
		}

		public static MethodInfo GetFlattenedMethod(this Type type, string methodName)
		{
			while (type != null)
			{
				MethodInfo[] declaredMethods = type.GetDeclaredMethods();
				for (int i = 0; i < declaredMethods.Length; i++)
				{
					if (declaredMethods[i].Name == methodName)
					{
						return declaredMethods[i];
					}
				}
				type = type.Resolve().BaseType;
			}
			return null;
		}

		public static IEnumerable<MethodInfo> GetFlattenedMethods(this Type type, string methodName)
		{
			while (type != null)
			{
				MethodInfo[] methods = type.GetDeclaredMethods();
				for (int i = 0; i < methods.Length; i++)
				{
					if (methods[i].Name == methodName)
					{
						yield return methods[i];
					}
				}
				type = type.Resolve().BaseType;
			}
		}

		public static PropertyInfo GetFlattenedProperty(this Type type, string propertyName)
		{
			while (type != null)
			{
				PropertyInfo[] declaredProperties = type.GetDeclaredProperties();
				for (int i = 0; i < declaredProperties.Length; i++)
				{
					if (declaredProperties[i].Name == propertyName)
					{
						return declaredProperties[i];
					}
				}
				type = type.Resolve().BaseType;
			}
			return null;
		}

		public static MemberInfo GetDeclaredMember(this Type type, string memberName)
		{
			MemberInfo[] declaredMembers = type.GetDeclaredMembers();
			for (int i = 0; i < declaredMembers.Length; i++)
			{
				if (declaredMembers[i].Name == memberName)
				{
					return declaredMembers[i];
				}
			}
			return null;
		}

		public static MethodInfo[] GetDeclaredMethods(this Type type)
		{
			return type.GetMethods(DeclaredFlags);
		}

		public static PropertyInfo[] GetDeclaredProperties(this Type type)
		{
			return type.GetProperties(DeclaredFlags);
		}

		public static FieldInfo[] GetDeclaredFields(this Type type)
		{
			return type.GetFields(DeclaredFlags);
		}

		public static MemberInfo[] GetDeclaredMembers(this Type type)
		{
			return type.GetMembers(DeclaredFlags);
		}

		public static MemberInfo AsMemberInfo(Type type)
		{
			return type;
		}

		public static bool IsType(MemberInfo member)
		{
			return member is Type;
		}

		public static Type AsType(MemberInfo member)
		{
			return (Type)member;
		}

		public static Type Resolve(this Type type)
		{
			return type;
		}
	}
}
