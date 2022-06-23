using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public class dfComponentMemberInfo
{
	public Component Component;

	public string MemberName;

	public bool IsValid
	{
		get
		{
			if (!(Component != null) || string.IsNullOrEmpty(MemberName))
			{
				return false;
			}
			MemberInfo memberInfo = Component.GetType().GetMember(MemberName).FirstOrDefault();
			if (memberInfo == null)
			{
				return false;
			}
			return true;
		}
	}

	public Type GetMemberType()
	{
		Type type = Component.GetType();
		MemberInfo memberInfo = type.GetMember(MemberName).FirstOrDefault();
		if (memberInfo == null)
		{
			throw new MissingMemberException("Member not found: " + type.Name + "." + MemberName);
		}
		if (memberInfo is FieldInfo)
		{
			return ((FieldInfo)memberInfo).FieldType;
		}
		if (memberInfo is PropertyInfo)
		{
			return ((PropertyInfo)memberInfo).PropertyType;
		}
		if (memberInfo is MethodInfo)
		{
			return ((MethodInfo)memberInfo).ReturnType;
		}
		if (memberInfo is EventInfo)
		{
			return ((EventInfo)memberInfo).EventHandlerType;
		}
		throw new InvalidCastException("Invalid member type: " + memberInfo.GetMemberType());
	}

	public MethodInfo GetMethod()
	{
		return Component.GetType().GetMember(MemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault() as MethodInfo;
	}

	public dfObservableProperty GetProperty()
	{
		Type type = Component.GetType();
		MemberInfo memberInfo = Component.GetType().GetMember(MemberName).FirstOrDefault();
		if (memberInfo == null)
		{
			throw new MissingMemberException("Member not found: " + type.Name + "." + MemberName);
		}
		if (!(memberInfo is FieldInfo) && !(memberInfo is PropertyInfo))
		{
			throw new InvalidCastException("Member " + MemberName + " is not an observable field or property");
		}
		return new dfObservableProperty(Component, memberInfo);
	}

	public override string ToString()
	{
		string arg = ((!(Component != null)) ? "[Missing ComponentType]" : Component.GetType().Name);
		string arg2 = (string.IsNullOrEmpty(MemberName) ? "[Missing MemberName]" : MemberName);
		return string.Format("{0}.{1}", arg, arg2);
	}
}
