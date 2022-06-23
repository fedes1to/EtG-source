using System;
using System.Linq;
using System.Reflection;
using DaikonForge.Tween.Interpolation;

namespace DaikonForge.Tween
{
	public class TweenNamedProperty<T>
	{
		public static Tween<T> Obtain(object target, string propertyName)
		{
			return Obtain(target, propertyName, Interpolators.Get<T>());
		}

		public static Tween<T> Obtain(object target, string propertyName, Interpolator<T> interpolator)
		{
			if (target == null)
			{
				throw new ArgumentException("Target object cannot be NULL");
			}
			Type type = target.GetType();
			MemberInfo member = getMember(type, propertyName);
			if (member == null)
			{
				throw new ArgumentException(string.Format("Failed to find property {0}.{1}", type.Name, propertyName));
			}
			bool flag = false;
			if (member is FieldInfo)
			{
				if (((FieldInfo)member).FieldType != typeof(T))
				{
					flag = true;
				}
			}
			else if (((PropertyInfo)member).PropertyType != typeof(T))
			{
				flag = true;
			}
			if (flag)
			{
				throw new InvalidCastException(string.Format("{0}.{1} cannot be cast to type {2}", type.Name, member.Name, typeof(T).Name));
			}
			T val = get(target, type, member);
			return Tween<T>.Obtain().SetStartValue(val).SetEndValue(val)
				.SetInterpolator(interpolator)
				.OnExecute(set(target, type, member));
		}

		public static T GetCurrentValue(object target, string propertyName)
		{
			Type type = target.GetType();
			MemberInfo member = getMember(type, propertyName);
			if (member == null)
			{
				throw new ArgumentException(string.Format("Failed to find property {0}.{1}", type.Name, propertyName));
			}
			return get(target, type, member);
		}

		private static MethodInfo getGetMethod(PropertyInfo property)
		{
			return property.GetGetMethod();
		}

		private static MethodInfo getSetMethod(PropertyInfo property)
		{
			return property.GetSetMethod();
		}

		private static MemberInfo getMember(Type type, string propertyName)
		{
			return type.GetMember(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();
		}

		private static T get(object target, Type type, MemberInfo member)
		{
			if (member is PropertyInfo)
			{
				PropertyInfo property = (PropertyInfo)member;
				MethodInfo getMethod = getGetMethod(property);
				if (getMethod == null)
				{
					throw new ArgumentException(string.Format("Property {0}.{1} cannot be read", type.Name, member.Name));
				}
				return (T)getMethod.Invoke(target, null);
			}
			if (member is FieldInfo)
			{
				FieldInfo fieldInfo = (FieldInfo)member;
				return (T)fieldInfo.GetValue(target);
			}
			throw new ArgumentException(string.Format("Failed to find property {0}.{1}", type.Name, member.Name));
		}

		private static TweenAssignmentCallback<T> set(object target, Type type, MemberInfo member)
		{
			if (member is PropertyInfo)
			{
				return setProperty(target, type, (PropertyInfo)member);
			}
			if (member is FieldInfo)
			{
				return setField(target, type, (FieldInfo)member);
			}
			throw new ArgumentException(string.Format("Failed to find property {0}.{1}", type.Name, member.Name));
		}

		private static TweenAssignmentCallback<T> setField(object target, Type type, FieldInfo field)
		{
			if (field.IsLiteral)
			{
				throw new ArgumentException(string.Format("Property {0}.{1} cannot be assigned", type.Name, field.Name));
			}
			return delegate(T result)
			{
				field.SetValue(target, result);
			};
		}

		private static TweenAssignmentCallback<T> setProperty(object target, Type type, PropertyInfo property)
		{
			MethodInfo setter = getSetMethod(property);
			if (setter == null)
			{
				throw new ArgumentException(string.Format("Property {0}.{1} cannot be assigned", type.Name, property.Name));
			}
			object[] paramArray = new object[1];
			return delegate(T result)
			{
				paramArray[0] = result;
				setter.Invoke(target, paramArray);
			};
		}
	}
}
