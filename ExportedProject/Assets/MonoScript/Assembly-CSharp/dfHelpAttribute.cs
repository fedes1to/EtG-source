using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public class dfHelpAttribute : Attribute
{
	public string HelpURL { get; private set; }

	public dfHelpAttribute(string url)
	{
		HelpURL = url;
	}
}
