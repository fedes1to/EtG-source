using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public class dfTooltipAttribute : Attribute
{
	public string Tooltip { get; private set; }

	public dfTooltipAttribute(string tooltip)
	{
		Tooltip = tooltip;
	}
}
