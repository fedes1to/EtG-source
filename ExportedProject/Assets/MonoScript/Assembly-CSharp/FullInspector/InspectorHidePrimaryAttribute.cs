using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InspectorHidePrimaryAttribute : Attribute, IInspectorAttributeOrder
	{
		double IInspectorAttributeOrder.Order
		{
			get
			{
				return double.MaxValue;
			}
		}
	}
}
