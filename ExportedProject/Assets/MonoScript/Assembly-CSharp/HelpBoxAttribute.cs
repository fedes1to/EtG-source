using UnityEngine;

public class HelpBoxAttribute : PropertyAttribute
{
	public string Message;

	public HelpBoxAttribute(string message)
	{
		Message = message;
	}
}
