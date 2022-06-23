using UnityEngine;

[RequireComponent(typeof(tk2dSpriteAnimator))]
public class InvariantSpriteAnimator : BraveBehaviour
{
	public void Awake()
	{
		base.spriteAnimator.ignoreTimeScale = true;
	}
}
