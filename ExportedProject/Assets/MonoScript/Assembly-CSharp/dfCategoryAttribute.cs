using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public class dfCategoryAttribute : Attribute
{
	public string Category { get; private set; }

	public dfCategoryAttribute(string category)
	{
		Category = category;
	}
}
