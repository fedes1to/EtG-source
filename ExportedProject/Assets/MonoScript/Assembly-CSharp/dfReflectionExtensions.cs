using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class dfReflectionExtensions
{
	public static Type[] EmptyTypes = new Type[0];

	public static MemberTypes GetMemberType(this MemberInfo member)
	{
		return member.MemberType;
	}

	public static Type GetBaseType(this Type type)
	{
		return type.BaseType;
	}

	public static Assembly GetAssembly(this Type type)
	{
		return type.Assembly;
	}

	[HideInInspector]
	internal static bool SignalHierarchy(this GameObject target, string messageName, params object[] args)
	{
		while (target != null)
		{
			if (target.Signal(messageName, args))
			{
				return true;
			}
			if (target.transform.parent == null)
			{
				break;
			}
			target = target.transform.parent.gameObject;
		}
		return false;
	}

	[HideInInspector]
	internal static bool Signal(this GameObject target, string messageName, params object[] args)
	{
		Component[] components = target.GetComponents(typeof(MonoBehaviour));
		Type[] array = new Type[args.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (args[i] == null)
			{
				array[i] = typeof(object);
			}
			else
			{
				array[i] = args[i].GetType();
			}
		}
		bool result = false;
		foreach (Component component in components)
		{
			if (component == null || component.GetType() == null || (component is MonoBehaviour && !((MonoBehaviour)component).enabled))
			{
				continue;
			}
			MethodInfo method = getMethod(component.GetType(), messageName, array);
			IEnumerator enumerator = null;
			if (method != null)
			{
				enumerator = method.Invoke(component, args) as IEnumerator;
				if (enumerator != null)
				{
					((MonoBehaviour)component).StartCoroutine(enumerator);
				}
				result = true;
			}
			else
			{
				if (args.Length == 0)
				{
					continue;
				}
				MethodInfo method2 = getMethod(component.GetType(), messageName, EmptyTypes);
				if (method2 != null)
				{
					enumerator = method2.Invoke(component, null) as IEnumerator;
					if (enumerator != null)
					{
						((MonoBehaviour)component).StartCoroutine(enumerator);
					}
					result = true;
				}
			}
		}
		return result;
	}

	private static MethodInfo getMethod(Type type, string name, Type[] paramTypes)
	{
		return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, paramTypes, null);
	}

	private static bool matchesParameterTypes(MethodInfo method, Type[] types)
	{
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length != types.Length)
		{
			return false;
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (!parameters[i].ParameterType.IsAssignableFrom(types[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal static FieldInfo[] GetAllFields(this Type type)
	{
		if (type == null)
		{
			return new FieldInfo[0];
		}
		BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		return (from f in type.GetFields(bindingAttr).Concat(type.GetBaseType().GetAllFields())
			where !f.IsDefined(typeof(HideInInspector), true)
			select f).ToArray();
	}
}
