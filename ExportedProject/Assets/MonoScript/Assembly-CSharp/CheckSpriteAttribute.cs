using UnityEngine;

public class CheckSpriteAttribute : PropertyAttribute
{
	public string sprite;

	public CheckSpriteAttribute(string sprite = null)
	{
		this.sprite = sprite;
	}
}
