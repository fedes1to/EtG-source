using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using FullInspector.Internal;
using FullSerializer;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector
{
	public sealed class InspectedProperty
	{
		public string Name;

		public string DisplayName;

		private bool? _isPublicCache;

		private bool? _isAutoPropertyCache;

		public Type StorageType;

		public MemberInfo MemberInfo { get; private set; }

		public bool IsPublic
		{
			get
			{
				bool? isPublicCache = _isPublicCache;
				if (!isPublicCache.HasValue)
				{
					FieldInfo fieldInfo = MemberInfo as FieldInfo;
					if (fieldInfo != null)
					{
						_isPublicCache = fieldInfo.IsPublic;
					}
					PropertyInfo propertyInfo = MemberInfo as PropertyInfo;
					if (propertyInfo != null)
					{
						_isPublicCache = propertyInfo.GetGetMethod(false) != null || propertyInfo.GetSetMethod(false) != null;
					}
					if (!_isPublicCache.HasValue)
					{
						_isPublicCache = false;
					}
				}
				return _isPublicCache.Value;
			}
		}

		public bool IsAutoProperty
		{
			get
			{
				bool? isAutoPropertyCache = _isAutoPropertyCache;
				if (!isAutoPropertyCache.HasValue)
				{
					if (!(MemberInfo is PropertyInfo))
					{
						_isAutoPropertyCache = false;
					}
					else
					{
						string text = string.Format("<{0}>k__BackingField", MemberInfo.Name);
						bool value = false;
						FieldInfo[] declaredFields = MemberInfo.DeclaringType.GetDeclaredFields();
						foreach (FieldInfo fieldInfo in declaredFields)
						{
							if (fsPortableReflection.HasAttribute<CompilerGeneratedAttribute>(fieldInfo) && fieldInfo.Name == text)
							{
								value = true;
								break;
							}
						}
						_isAutoPropertyCache = value;
					}
				}
				return _isAutoPropertyCache.Value;
			}
		}

		public bool IsStatic { get; private set; }

		public bool CanWrite { get; private set; }

		public object DefaultValue
		{
			get
			{
				if (StorageType.Resolve().IsValueType)
				{
					return InspectedType.Get(StorageType).CreateInstance();
				}
				return null;
			}
		}

		public InspectedProperty(PropertyInfo property)
		{
			MemberInfo = property;
			StorageType = property.PropertyType;
			CanWrite = property.GetSetMethod(true) != null;
			IsStatic = (property.GetGetMethod(true) ?? property.GetSetMethod(true)).IsStatic;
			SetupNames();
		}

		public InspectedProperty(FieldInfo field)
		{
			MemberInfo = field;
			StorageType = field.FieldType;
			CanWrite = !field.IsLiteral;
			IsStatic = field.IsStatic;
			SetupNames();
		}

		public void Write(object context, object value)
		{
			try
			{
				FieldInfo fieldInfo = MemberInfo as FieldInfo;
				PropertyInfo propertyInfo = MemberInfo as PropertyInfo;
				if (fieldInfo != null)
				{
					if (!fieldInfo.IsLiteral)
					{
						fieldInfo.SetValue(context, value);
					}
				}
				else if (propertyInfo != null)
				{
					MethodInfo setMethod = propertyInfo.GetSetMethod(true);
					if (setMethod != null)
					{
						setMethod.Invoke(context, new object[1] { value });
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarning(string.Concat("Caught exception when writing property ", Name, " with context=", context, " and value=", value));
				Debug.LogException(exception);
			}
		}

		public object Read(object context)
		{
			try
			{
				if (MemberInfo is PropertyInfo)
				{
					return ((PropertyInfo)MemberInfo).GetValue(context, new object[0]);
				}
				return ((FieldInfo)MemberInfo).GetValue(context);
			}
			catch (Exception exception)
			{
				Debug.LogWarning(string.Concat("Caught exception when reading property ", Name, " with  context=", context, "; returning default value for ", StorageType.CSharpName()));
				Debug.LogException(exception);
				return DefaultValue;
			}
		}

		private void SetupNames()
		{
			Name = MemberInfo.Name;
			InspectorNameAttribute attribute = fsPortableReflection.GetAttribute<InspectorNameAttribute>(MemberInfo);
			if (attribute != null)
			{
				DisplayName = attribute.DisplayName;
			}
			if (string.IsNullOrEmpty(DisplayName))
			{
				DisplayName = fiDisplayNameMapper.Map(Name);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			InspectedProperty inspectedProperty = obj as InspectedProperty;
			if (inspectedProperty == null)
			{
				return false;
			}
			return StorageType == inspectedProperty.StorageType && Name == inspectedProperty.Name;
		}

		public bool Equals(InspectedProperty p)
		{
			if (p == null)
			{
				return false;
			}
			return StorageType == p.StorageType && Name == p.Name;
		}

		public override int GetHashCode()
		{
			return StorageType.GetHashCode() ^ Name.GetHashCode();
		}
	}
}
