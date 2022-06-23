using System;

namespace FullInspector
{
	[Obsolete("Please use [InspectorMargin] instead of [Margin]")]
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class MarginAttribute : Attribute, IInspectorAttributeOrder
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

		public MarginAttribute(int margin)
		{
			Margin = margin;
		}
	}
}
