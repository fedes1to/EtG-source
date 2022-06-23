using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class dfMarkupTagInfoAttribute : Attribute
{
	public string TagName { get; set; }

	public dfMarkupTagInfoAttribute(string tagName)
	{
		TagName = tagName;
	}
}
