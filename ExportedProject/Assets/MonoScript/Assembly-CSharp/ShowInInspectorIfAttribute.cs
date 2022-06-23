using UnityEngine;

public class ShowInInspectorIfAttribute : PropertyAttribute
{
	public ShowInInspectorIfAttribute(string propertyName, bool indent = false)
	{
	}

	public ShowInInspectorIfAttribute(string propertyName, int value, bool indent = false)
	{
	}
}
