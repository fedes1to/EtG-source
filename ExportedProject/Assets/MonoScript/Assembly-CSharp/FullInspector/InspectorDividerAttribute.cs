using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InspectorDividerAttribute : Attribute, IInspectorAttributeOrder
	{
		public double Order = 50.0;

		double IInspectorAttributeOrder.Order
		{
			get
			{
				return Order;
			}
		}
	}
}
