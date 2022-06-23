using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InspectorHeaderAttribute : Attribute, IInspectorAttributeOrder
	{
		public double Order = 75.0;

		public string Header;

		double IInspectorAttributeOrder.Order
		{
			get
			{
				return Order;
			}
		}

		public InspectorHeaderAttribute(string header)
		{
			Header = header;
		}
	}
}
