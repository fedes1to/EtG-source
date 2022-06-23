using System;
using System.Reflection;
using FullSerializer.Internal;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorOrderAttribute : Attribute
	{
		public double Order;

		public InspectorOrderAttribute(double order)
		{
			Order = order;
		}

		public static double GetInspectorOrder(MemberInfo memberInfo)
		{
			InspectorOrderAttribute attribute = fsPortableReflection.GetAttribute<InspectorOrderAttribute>(memberInfo);
			if (attribute != null)
			{
				return attribute.Order;
			}
			return double.MaxValue;
		}
	}
}
