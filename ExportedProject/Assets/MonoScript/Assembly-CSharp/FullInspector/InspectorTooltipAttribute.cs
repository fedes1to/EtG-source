using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class InspectorTooltipAttribute : Attribute
	{
		public string Tooltip;

		public InspectorTooltipAttribute(string tooltip)
		{
			Tooltip = tooltip;
		}
	}
}
