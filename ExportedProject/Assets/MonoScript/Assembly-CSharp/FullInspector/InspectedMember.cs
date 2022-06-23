using System;
using System.Reflection;

namespace FullInspector
{
	public struct InspectedMember
	{
		private InspectedProperty _property;

		private InspectedMethod _method;

		public InspectedProperty Property
		{
			get
			{
				if (!IsProperty)
				{
					throw new InvalidOperationException("Member is not a property");
				}
				return _property;
			}
		}

		public InspectedMethod Method
		{
			get
			{
				if (!IsMethod)
				{
					throw new InvalidOperationException("Member is not a method");
				}
				return _method;
			}
		}

		public bool IsMethod
		{
			get
			{
				return _method != null;
			}
		}

		public bool IsProperty
		{
			get
			{
				return _property != null;
			}
		}

		public string Name
		{
			get
			{
				return MemberInfo.Name;
			}
		}

		public MemberInfo MemberInfo
		{
			get
			{
				if (IsMethod)
				{
					return _method.Method;
				}
				return _property.MemberInfo;
			}
		}

		public InspectedMember(InspectedProperty property)
		{
			_property = property;
			_method = null;
		}

		public InspectedMember(InspectedMethod method)
		{
			_property = null;
			_method = method;
		}
	}
}
