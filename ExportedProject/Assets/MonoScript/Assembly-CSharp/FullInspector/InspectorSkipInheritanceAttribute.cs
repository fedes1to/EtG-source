using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorSkipInheritanceAttribute : Attribute, IInspectorAttributeOrder
	{
		double IInspectorAttributeOrder.Order
		{
			get
			{
				return double.MinValue;
			}
		}
	}
}
