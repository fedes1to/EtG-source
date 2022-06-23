using UnityEngine;

public class CheckAnimationAttribute : PropertyAttribute
{
	public string animator;

	public CheckAnimationAttribute(string animator = null)
	{
		this.animator = animator;
	}
}
