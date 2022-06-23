using UnityEngine;

public class TogglesPropertyAttribute : PropertyAttribute
{
	public string PropertyName;

	public string Label;

	public TogglesPropertyAttribute(string propertyName, string label = null)
	{
		PropertyName = propertyName;
		Label = label;
	}
}
