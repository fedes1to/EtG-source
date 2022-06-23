using System;
using UnityEngine;

[Serializable]
public class DodgeRollStats
{
	[HideInInspector]
	public bool hasPreDodgeDelay;

	[TogglableProperty("hasPreDodgeDelay", "Pre-Dodge Delay")]
	public float preDodgeDelay;

	public float time;

	public float distance;

	[NonSerialized]
	public int additionalInvulnerabilityFrames;

	[NonSerialized]
	public float blinkDistanceMultiplier = 1f;

	[NonSerialized]
	public float rollTimeMultiplier = 1f;

	[NonSerialized]
	public float rollDistanceMultiplier = 1f;

	[CurveRange(0f, 0f, 1f, 1f)]
	public AnimationCurve speed;

	private const float c_moveSpeedToRollDistanceConversion = 0.5f;

	public float GetModifiedTime(PlayerController owner)
	{
		float num = 1f;
		if ((bool)GameManager.Instance.Dungeon && GameManager.Instance.Dungeon.IsEndTimes)
		{
			return time;
		}
		if (PassiveItem.IsFlagSetForCharacter(owner, typeof(SunglassesItem)) && SunglassesItem.SunglassesActive)
		{
			num *= 0.75f;
		}
		float statModifier = owner.stats.GetStatModifier(PlayerStats.StatType.DodgeRollSpeedMultiplier);
		float num2 = ((statModifier == 0f) ? 1f : (1f / statModifier));
		return time * rollTimeMultiplier * num * num2;
	}

	public float GetModifiedDistance(PlayerController owner)
	{
		float num = 1f;
		if (PassiveItem.IsFlagSetForCharacter(owner, typeof(SunglassesItem)) && SunglassesItem.SunglassesActive)
		{
			num *= 1.25f;
		}
		float statModifier = owner.stats.GetStatModifier(PlayerStats.StatType.DodgeRollDistanceMultiplier);
		float statModifier2 = owner.stats.GetStatModifier(PlayerStats.StatType.MovementSpeed);
		float num2 = (statModifier2 - 1f) * 0.5f + 1f;
		return distance * rollDistanceMultiplier * num2 * num * blinkDistanceMultiplier * statModifier;
	}
}
