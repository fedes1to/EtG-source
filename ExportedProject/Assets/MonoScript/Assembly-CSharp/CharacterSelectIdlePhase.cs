using System;
using UnityEngine;

[Serializable]
public class CharacterSelectIdlePhase
{
	public enum VFXPhaseTrigger
	{
		NONE,
		IN,
		HOLD,
		OUT
	}

	public float holdMin = 4f;

	public float holdMax = 10f;

	public string inAnimation = string.Empty;

	public string holdAnimation = string.Empty;

	public float optionalHoldChance = 0.5f;

	public string optionalHoldIdleAnimation = string.Empty;

	public string outAnimation = string.Empty;

	[Header("Optional VFX")]
	public VFXPhaseTrigger vfxTrigger;

	public float vfxHoldPeriod = 1f;

	public tk2dSpriteAnimator vfxSpriteAnimator;

	public tk2dSpriteAnimator endVFXSpriteAnimator;
}
