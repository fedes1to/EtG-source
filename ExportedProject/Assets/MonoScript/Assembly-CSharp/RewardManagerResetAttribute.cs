using UnityEngine;

public class RewardManagerResetAttribute : PropertyAttribute
{
	public string header;

	public string content;

	public string callback;

	public int targetElement;

	public RewardManagerResetAttribute(string headerMessage, string contentMessage, string callbackFunc, int targetType)
	{
		header = headerMessage;
		content = contentMessage;
		callback = callbackFunc;
		targetElement = targetType;
	}
}
