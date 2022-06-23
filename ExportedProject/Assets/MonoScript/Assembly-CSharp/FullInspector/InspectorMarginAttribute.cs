using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InspectorMarginAttribute : Attribute, IInspectorAttributeOrder
	{
		public int Margin;

		public double Order;

		double IInspectorAttributeOrder.Order
		{
			get
			{
				return Order;
			}
		}

		public InspectorMarginAttribute(int margin)
		{
			Margin = margin;
		}
	}
}
