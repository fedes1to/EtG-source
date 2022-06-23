using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using FullSerializer.Internal;
using UnityEngine;

namespace FullSerializer
{
	public class fsMetaType
	{
		private static Dictionary<Type, fsMetaType> _metaTypes = new Dictionary<Type, fsMetaType>();

		public Type ReflectedType;

		private bool _hasEmittedAotData;

		private bool? _hasDefaultConstructorCache;

		private bool _isDefaultConstructorPublic;

		public fsMetaProperty[] Properties { get; private set; }

		public bool HasDefaultConstructor
		{
			get
			{
				if (!_hasDefaultConstructorCache.HasValue)
				{
					if (ReflectedType.Resolve().IsArray)
					{
						_hasDefaultConstructorCache = true;
						_isDefaultConstructorPublic = true;
					}
					else if (ReflectedType.Resolve().IsValueType)
					{
						_hasDefaultConstructorCache = true;
						_isDefaultConstructorPublic = true;
					}
					else
					{
						ConstructorInfo declaredConstructor = ReflectedType.GetDeclaredConstructor(fsPortableReflection.EmptyTypes);
						_hasDefaultConstructorCache = declaredConstructor != null;
						if (declaredConstructor != null)
						{
							_isDefaultConstructorPublic = declaredConstructor.IsPublic;
						}
					}
				}
				return _hasDefaultConstructorCache.Value;
			}
		}

		private fsMetaType(Type reflectedType)
		{
			ReflectedType = reflectedType;
			List<fsMetaProperty> list = new List<fsMetaProperty>();
			CollectProperties(list, reflectedType);
			Properties = list.ToArray();
		}

		public static fsMetaType Get(Type type)
		{
			fsMetaType value;
			if (!_metaTypes.TryGetValue(type, out value))
			{
				value = new fsMetaType(type);
				_metaTypes[type] = value;
			}
			return value;
		}

		public static void ClearCache()
		{
			_metaTypes = new Dictionary<Type, fsMetaType>();
		}

		private static void CollectProperties(List<fsMetaProperty> properties, Type reflectedType)
		{
			bool flag = fsConfig.DefaultMemberSerialization == fsMemberSerialization.OptIn;
			bool flag2 = fsConfig.DefaultMemberSerialization == fsMemberSerialization.OptOut;
			fsObjectAttribute attribute = fsPortableReflection.GetAttribute<fsObjectAttribute>(reflectedType);
			if (attribute != null)
			{
				flag = attribute.MemberSerialization == fsMemberSerialization.OptIn;
				flag2 = attribute.MemberSerialization == fsMemberSerialization.OptOut;
			}
			MemberInfo[] declaredMembers = reflectedType.GetDeclaredMembers();
			MemberInfo[] array = declaredMembers;
			foreach (MemberInfo member in array)
			{
				if (fsConfig.IgnoreSerializeAttributes.Any((Type t) => fsPortableReflection.HasAttribute(member, t)))
				{
					continue;
				}
				PropertyInfo propertyInfo = member as PropertyInfo;
				FieldInfo fieldInfo = member as FieldInfo;
				if ((flag && !fsConfig.SerializeAttributes.Any((Type t) => fsPortableReflection.HasAttribute(member, t))) || (flag2 && fsConfig.IgnoreSerializeAttributes.Any((Type t) => fsPortableReflection.HasAttribute(member, t))))
				{
					continue;
				}
				if (propertyInfo != null)
				{
					if (CanSerializeProperty(propertyInfo, declaredMembers, flag2))
					{
						properties.Add(new fsMetaProperty(propertyInfo));
					}
				}
				else if (fieldInfo != null && CanSerializeField(fieldInfo, flag2))
				{
					properties.Add(new fsMetaProperty(fieldInfo));
				}
			}
			if (reflectedType.Resolve().BaseType != null)
			{
				CollectProperties(properties, reflectedType.Resolve().BaseType);
			}
		}

		private static bool IsAutoProperty(PropertyInfo property, MemberInfo[] members)
		{
			if (!property.CanWrite || !property.CanRead)
			{
				return false;
			}
			string text = "<" + property.Name + ">k__BackingField";
			for (int i = 0; i < members.Length; i++)
			{
				if (members[i].Name == text)
				{
					return true;
				}
			}
			return false;
		}

		private static bool CanSerializeProperty(PropertyInfo property, MemberInfo[] members, bool annotationFreeValue)
		{
			if (typeof(Delegate).IsAssignableFrom(property.PropertyType))
			{
				return false;
			}
			MethodInfo getMethod = property.GetGetMethod(false);
			MethodInfo setMethod = property.GetSetMethod(false);
			if ((getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic))
			{
				return false;
			}
			if (fsConfig.SerializeAttributes.Any((Type t) => fsPortableReflection.HasAttribute(property, t)))
			{
				return true;
			}
			if (!property.CanRead || !property.CanWrite)
			{
				return false;
			}
			if ((fsConfig.SerializeNonAutoProperties || IsAutoProperty(property, members)) && getMethod != null && (fsConfig.SerializeNonPublicSetProperties || setMethod != null))
			{
				return true;
			}
			return annotationFreeValue;
		}

		private static bool CanSerializeField(FieldInfo field, bool annotationFreeValue)
		{
			if (typeof(Delegate).IsAssignableFrom(field.FieldType))
			{
				return false;
			}
			if (field.IsDefined(typeof(CompilerGeneratedAttribute), false))
			{
				return false;
			}
			if (field.IsStatic)
			{
				return false;
			}
			if (fsConfig.SerializeAttributes.Any((Type t) => fsPortableReflection.HasAttribute(field, t)))
			{
				return true;
			}
			if (!annotationFreeValue && !field.IsPublic)
			{
				return false;
			}
			return true;
		}

		public bool EmitAotData()
		{
			if (!_hasEmittedAotData)
			{
				_hasEmittedAotData = true;
				for (int i = 0; i < Properties.Length; i++)
				{
					if (!Properties[i].IsPublic)
					{
						return false;
					}
				}
				if (!HasDefaultConstructor)
				{
					return false;
				}
				fsAotCompilationManager.AddAotCompilation(ReflectedType, Properties, _isDefaultConstructorPublic);
				return true;
			}
			return false;
		}

		public object CreateInstance()
		{
			if (ReflectedType.Resolve().IsInterface || ReflectedType.Resolve().IsAbstract)
			{
				throw new Exception("Cannot create an instance of an interface or abstract type for " + ReflectedType);
			}
			if (typeof(ScriptableObject).IsAssignableFrom(ReflectedType))
			{
				return ScriptableObject.CreateInstance(ReflectedType);
			}
			if (typeof(string) == ReflectedType)
			{
				return string.Empty;
			}
			if (!HasDefaultConstructor)
			{
				return FormatterServices.GetSafeUninitializedObject(ReflectedType);
			}
			if (ReflectedType.Resolve().IsArray)
			{
				return Array.CreateInstance(ReflectedType.GetElementType(), 0);
			}
			try
			{
				return Activator.CreateInstance(ReflectedType, true);
			}
			catch (MissingMethodException innerException)
			{
				throw new InvalidOperationException(string.Concat("Unable to create instance of ", ReflectedType, "; there is no default constructor"), innerException);
			}
			catch (TargetInvocationException innerException2)
			{
				throw new InvalidOperationException(string.Concat("Constructor of ", ReflectedType, " threw an exception when creating an instance"), innerException2);
			}
			catch (MemberAccessException innerException3)
			{
				throw new InvalidOperationException("Unable to access constructor of " + ReflectedType, innerException3);
			}
		}
	}
}
