using UnityEngine;

public class LongEnumShowInInspectorIfAttribute : PropertyAttribute
{
	public LongEnumShowInInspectorIfAttribute(string propertyName, bool value = true, bool indent = false)
	{
	}

	public LongEnumShowInInspectorIfAttribute(string propertyName, int value, bool indent = false)
	{
	}

	public LongEnumShowInInspectorIfAttribute(string propertyName, Object value, bool indent = false)
	{
	}
}
