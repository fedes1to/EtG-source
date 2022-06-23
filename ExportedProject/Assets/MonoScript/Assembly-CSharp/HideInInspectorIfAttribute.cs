public class HideInInspectorIfAttribute : ShowInInspectorIfAttribute
{
	public HideInInspectorIfAttribute(string propertyName, bool indent = false)
		: base(propertyName, indent)
	{
	}

	public HideInInspectorIfAttribute(string propertyName, int value, bool indent = false)
		: base(propertyName, value, indent)
	{
	}
}
