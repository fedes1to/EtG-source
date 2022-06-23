using UnityEngine;

public class CheckDirectionalAnimationAttribute : PropertyAttribute
{
	public string aiAnimator;

	public CheckDirectionalAnimationAttribute(string aiAnimator = null)
	{
		this.aiAnimator = aiAnimator;
	}
}
