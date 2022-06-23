using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FullInspector.Internal;
using FullSerializer;
using FullSerializer.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace FullInspector
{
	public sealed class InspectedType
	{
		private static Dictionary<Type, InspectedType> _cachedMetadata;

		private bool? _hasDefaultConstructorCache;

		private List<InspectedMember> _allMembers;

		private Dictionary<IInspectedMemberFilter, List<InspectedMember>> _cachedMembers;

		private Dictionary<IInspectedMemberFilter, List<InspectedProperty>> _cachedProperties;

		private Dictionary<IInspectedMemberFilter, List<InspectedMethod>> _cachedMethods;

		private bool _isArray;

		private Dictionary<IInspectedMemberFilter, Dictionary<string, List<InspectedMember>>> _categoryCache = new Dictionary<IInspectedMemberFilter, Dictionary<string, List<InspectedMember>>>();

		private Dictionary<string, InspectedProperty> _nameToProperty;

		private Dictionary<string, InspectedProperty> _formerlySerializedAsPropertyNames;

		public bool HasDefaultConstructor
		{
			get
			{
				if (!_hasDefaultConstructorCache.HasValue)
				{
					if (_isArray)
					{
						_hasDefaultConstructorCache = true;
					}
					else if (ReflectedType.Resolve().IsValueType)
					{
						_hasDefaultConstructorCache = true;
					}
					else
					{
						ConstructorInfo declaredConstructor = ReflectedType.GetDeclaredConstructor(fsPortableReflection.EmptyTypes);
						_hasDefaultConstructorCache = declaredConstructor != null;
					}
				}
				return _hasDefaultConstructorCache.Value;
			}
		}

		public Type ReflectedType { get; private set; }

		public bool IsCollection { get; private set; }

		static InspectedType()
		{
			_cachedMetadata = new Dictionary<Type, InspectedType>();
			InitializePropertyRemoval();
		}

		internal InspectedType(Type type)
		{
			ReflectedType = type;
			_isArray = type.IsArray;
			IsCollection = _isArray || type.IsImplementationOf(typeof(ICollection<>));
			if (IsCollection)
			{
				return;
			}
			_cachedMembers = new Dictionary<IInspectedMemberFilter, List<InspectedMember>>();
			_cachedProperties = new Dictionary<IInspectedMemberFilter, List<InspectedProperty>>();
			_cachedMethods = new Dictionary<IInspectedMemberFilter, List<InspectedMethod>>();
			_allMembers = new List<InspectedMember>();
			if (ReflectedType.Resolve().BaseType != null)
			{
				InspectedType inspectedType = Get(ReflectedType.Resolve().BaseType);
				_allMembers.AddRange(inspectedType._allMembers);
			}
			List<InspectedMember> list = CollectUnorderedLocalMembers(type).ToList();
			StableSort(list, (InspectedMember a, InspectedMember b) => Math.Sign(InspectorOrderAttribute.GetInspectorOrder(a.MemberInfo) - InspectorOrderAttribute.GetInspectorOrder(b.MemberInfo)));
			_allMembers.AddRange(list);
			_nameToProperty = new Dictionary<string, InspectedProperty>();
			_formerlySerializedAsPropertyNames = new Dictionary<string, InspectedProperty>();
			foreach (InspectedMember allMember in _allMembers)
			{
				if (allMember.IsProperty)
				{
					if (fiSettings.EmitWarnings && _nameToProperty.ContainsKey(allMember.Name))
					{
						Debug.LogWarning("Duplicate property with name=" + allMember.Name + " detected on " + ReflectedType.CSharpName());
					}
					_nameToProperty[allMember.Name] = allMember.Property;
					object[] customAttributes = allMember.MemberInfo.GetCustomAttributes(typeof(FormerlySerializedAsAttribute), true);
					for (int i = 0; i < customAttributes.Length; i++)
					{
						FormerlySerializedAsAttribute formerlySerializedAsAttribute = (FormerlySerializedAsAttribute)customAttributes[i];
						_nameToProperty[formerlySerializedAsAttribute.oldName] = allMember.Property;
					}
				}
			}
		}

		public static InspectedType Get(Type type)
		{
			InspectedType value;
			if (!_cachedMetadata.TryGetValue(type, out value))
			{
				value = new InspectedType(type);
				_cachedMetadata[type] = value;
			}
			return value;
		}

		public object CreateInstance()
		{
			if (typeof(ScriptableObject).IsAssignableFrom(ReflectedType))
			{
				return ScriptableObject.CreateInstance(ReflectedType);
			}
			if (typeof(Component).IsAssignableFrom(ReflectedType))
			{
				GameObject gameObject = fiLateBindings.Selection.activeObject as GameObject;
				if (gameObject != null)
				{
					Component component = gameObject.GetComponent(ReflectedType);
					if (component != null)
					{
						return component;
					}
					return gameObject.AddComponent(ReflectedType);
				}
				Debug.LogWarning("No selected game object; constructing an unformatted instance (which will be null) for " + ReflectedType);
				return FormatterServices.GetSafeUninitializedObject(ReflectedType);
			}
			if (!HasDefaultConstructor)
			{
				return FormatterServices.GetSafeUninitializedObject(ReflectedType);
			}
			if (_isArray)
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

		public List<InspectedMember> GetMembers(IInspectedMemberFilter filter)
		{
			VerifyNotCollection();
			List<InspectedMember> value;
			if (!_cachedMembers.TryGetValue(filter, out value))
			{
				value = new List<InspectedMember>();
				for (int i = 0; i < _allMembers.Count; i++)
				{
					InspectedMember item = _allMembers[i];
					if ((!item.IsProperty) ? filter.IsInterested(item.Method) : filter.IsInterested(item.Property))
					{
						value.Add(item);
					}
				}
				_cachedMembers[filter] = value;
			}
			return value;
		}

		public List<InspectedProperty> GetProperties(IInspectedMemberFilter filter)
		{
			VerifyNotCollection();
			List<InspectedProperty> value;
			if (!_cachedProperties.TryGetValue(filter, out value))
			{
				List<InspectedMember> members = GetMembers(filter);
				value = (from member in members
					where member.IsProperty
					select member.Property).ToList();
				_cachedProperties[filter] = value;
			}
			return value;
		}

		public List<InspectedMethod> GetMethods(IInspectedMemberFilter filter)
		{
			VerifyNotCollection();
			List<InspectedMethod> value;
			if (!_cachedMethods.TryGetValue(filter, out value))
			{
				List<InspectedMember> members = GetMembers(filter);
				value = (from member in members
					where member.IsMethod
					select member.Method).ToList();
				_cachedMethods[filter] = value;
			}
			return value;
		}

		private void VerifyNotCollection()
		{
			if (IsCollection)
			{
				throw new InvalidOperationException(string.Concat("Operation not valid -- ", ReflectedType, " is a collection"));
			}
		}

		public static void StableSort<T>(IList<T> list, Func<T, T, int> comparator)
		{
			for (int i = 1; i < list.Count; i++)
			{
				T val = list[i];
				int num = i - 1;
				while (num >= 0 && comparator(list[num], val) > 0)
				{
					list[num + 1] = list[num];
					num--;
				}
				list[num + 1] = val;
			}
		}

		private static List<InspectedMember> CollectUnorderedLocalMembers(Type reflectedType)
		{
			List<InspectedMember> list = new List<InspectedMember>();
			MemberInfo[] declaredMembers = reflectedType.GetDeclaredMembers();
			foreach (MemberInfo memberInfo in declaredMembers)
			{
				PropertyInfo propertyInfo = memberInfo as PropertyInfo;
				FieldInfo fieldInfo = memberInfo as FieldInfo;
				if (propertyInfo != null)
				{
					MethodInfo getMethod = propertyInfo.GetGetMethod(true);
					MethodInfo setMethod = propertyInfo.GetSetMethod(true);
					if ((getMethod == null || getMethod == getMethod.GetBaseDefinition()) && (setMethod == null || setMethod == setMethod.GetBaseDefinition()))
					{
						list.Add(new InspectedMember(new InspectedProperty(propertyInfo)));
					}
				}
				else if (fieldInfo != null)
				{
					list.Add(new InspectedMember(new InspectedProperty(fieldInfo)));
				}
			}
			MethodInfo[] declaredMethods = reflectedType.GetDeclaredMethods();
			foreach (MethodInfo methodInfo in declaredMethods)
			{
				if (methodInfo == methodInfo.GetBaseDefinition())
				{
					list.Add(new InspectedMember(new InspectedMethod(methodInfo)));
				}
			}
			return list;
		}

		public Dictionary<string, List<InspectedMember>> GetCategories(IInspectedMemberFilter filter)
		{
			VerifyNotCollection();
			Dictionary<string, List<InspectedMember>> value;
			if (!_categoryCache.TryGetValue(filter, out value))
			{
				value = new Dictionary<string, List<InspectedMember>>();
				_categoryCache[filter] = value;
				foreach (InspectedMember member in GetMembers(filter))
				{
					bool flag = false;
					object[] customAttributes = member.MemberInfo.GetCustomAttributes(typeof(InspectorCategoryAttribute), true);
					for (int i = 0; i < customAttributes.Length; i++)
					{
						InspectorCategoryAttribute inspectorCategoryAttribute = (InspectorCategoryAttribute)customAttributes[i];
						if (!value.ContainsKey(inspectorCategoryAttribute.Category))
						{
							value[inspectorCategoryAttribute.Category] = new List<InspectedMember>();
							if (inspectorCategoryAttribute.Category == "Conditions")
							{
								value["Cond. (All)"] = new List<InspectedMember>();
							}
						}
						value[inspectorCategoryAttribute.Category].Add(member);
						if (inspectorCategoryAttribute.Category == "Conditions")
						{
							value["Cond. (All)"].Add(member);
						}
						flag = true;
					}
					if (!flag)
					{
						if (!value.ContainsKey("Default"))
						{
							value["Default"] = new List<InspectedMember>();
						}
						value["Default"].Add(member);
					}
				}
			}
			if (value.Count == 1 && value.ContainsKey("Default"))
			{
				value.Clear();
			}
			return value;
		}

		public InspectedProperty GetPropertyByName(string name)
		{
			VerifyNotCollection();
			InspectedProperty value;
			if (!_nameToProperty.TryGetValue(name, out value))
			{
				return null;
			}
			return value;
		}

		public InspectedProperty GetPropertyByFormerlySerializedName(string name)
		{
			VerifyNotCollection();
			InspectedProperty value;
			if (!_formerlySerializedAsPropertyNames.TryGetValue(name, out value))
			{
				return null;
			}
			return value;
		}

		private static void InitializePropertyRemoval()
		{
			RemoveProperty<IntPtr>("m_value");
			RemoveProperty<UnityEngine.Object>("m_UnityRuntimeReferenceData");
			RemoveProperty<UnityEngine.Object>("m_UnityRuntimeErrorString");
			RemoveProperty<UnityEngine.Object>("name");
			RemoveProperty<UnityEngine.Object>("hideFlags");
			RemoveProperty<Component>("active");
			RemoveProperty<Component>("tag");
			RemoveProperty<Behaviour>("enabled");
			RemoveProperty<MonoBehaviour>("useGUILayout");
		}

		public static void RemoveProperty<T>(string propertyName)
		{
			InspectedType inspectedType = Get(typeof(T));
			inspectedType._nameToProperty.Remove(propertyName);
			inspectedType._cachedMembers = new Dictionary<IInspectedMemberFilter, List<InspectedMember>>();
			inspectedType._cachedMethods = new Dictionary<IInspectedMemberFilter, List<InspectedMethod>>();
			inspectedType._cachedProperties = new Dictionary<IInspectedMemberFilter, List<InspectedProperty>>();
			for (int i = 0; i < inspectedType._allMembers.Count; i++)
			{
				if (propertyName == inspectedType._allMembers[i].Name)
				{
					inspectedType._allMembers.RemoveAt(i);
					break;
				}
			}
		}

		private static bool IsSimpleTypeThatUnityCanSerialize(Type type)
		{
			if (IsPrimitiveSkippedByUnity(type))
			{
				return false;
			}
			if (type.Resolve().IsPrimitive)
			{
				return true;
			}
			if (type == typeof(string))
			{
				return true;
			}
			return false;
		}

		private static bool IsPrimitiveSkippedByUnity(Type type)
		{
			return type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong) || type == typeof(sbyte);
		}

		public static bool IsSerializedByUnity(InspectedProperty property)
		{
			if (property.MemberInfo is PropertyInfo)
			{
				return false;
			}
			if (property.IsStatic)
			{
				return false;
			}
			FieldInfo fieldInfo = property.MemberInfo as FieldInfo;
			if (fieldInfo.IsInitOnly)
			{
				return false;
			}
			if (!property.IsPublic && !property.MemberInfo.IsDefined(typeof(SerializeField), true))
			{
				return false;
			}
			Type storageType = property.StorageType;
			return IsSimpleTypeThatUnityCanSerialize(storageType) || (typeof(UnityEngine.Object).IsAssignableFrom(storageType) && !storageType.Resolve().IsGenericType) || (storageType.IsArray && !storageType.GetElementType().IsArray && IsSimpleTypeThatUnityCanSerialize(storageType.GetElementType())) || (storageType.Resolve().IsGenericType && storageType.GetGenericTypeDefinition() == typeof(List<>) && IsSimpleTypeThatUnityCanSerialize(storageType.GetGenericArguments()[0]));
		}

		public static bool IsSerializedByFullInspector(InspectedProperty property)
		{
			if (property.IsStatic)
			{
				return false;
			}
			if (typeof(BaseObject).Resolve().IsAssignableFrom(property.StorageType.Resolve()))
			{
				return false;
			}
			MemberInfo memberInfo = property.MemberInfo;
			if (fsPortableReflection.HasAttribute<NonSerializedAttribute>(memberInfo) || fsPortableReflection.HasAttribute<NotSerializedAttribute>(memberInfo))
			{
				return false;
			}
			Type[] serializationOptOutAnnotations = fiInstalledSerializerManager.SerializationOptOutAnnotations;
			for (int i = 0; i < serializationOptOutAnnotations.Length; i++)
			{
				if (memberInfo.IsDefined(serializationOptOutAnnotations[i], true))
				{
					return false;
				}
			}
			if (fsPortableReflection.HasAttribute<SerializeField>(memberInfo) || fsPortableReflection.HasAttribute<SerializableAttribute>(memberInfo))
			{
				return true;
			}
			Type[] serializationOptInAnnotations = fiInstalledSerializerManager.SerializationOptInAnnotations;
			for (int j = 0; j < serializationOptInAnnotations.Length; j++)
			{
				if (memberInfo.IsDefined(serializationOptInAnnotations[j], true))
				{
					return true;
				}
			}
			if (property.MemberInfo is PropertyInfo)
			{
				if (!fiSettings.SerializeAutoProperties)
				{
					return false;
				}
				if (!property.IsAutoProperty)
				{
					return false;
				}
			}
			return property.IsPublic;
		}
	}
}
