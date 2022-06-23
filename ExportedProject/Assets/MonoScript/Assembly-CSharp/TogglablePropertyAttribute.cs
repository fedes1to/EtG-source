using UnityEngine;

public class TogglablePropertyAttribute : PropertyAttribute
{
	public string TogglePropertyName;

	public string Label;

	public TogglablePropertyAttribute(string togglePropertyName, string label = null)
	{
		TogglePropertyName = togglePropertyName;
		Label = label;
	}
}
