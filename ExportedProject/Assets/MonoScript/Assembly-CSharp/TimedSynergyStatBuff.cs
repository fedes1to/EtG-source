using System;

[Serializable]
public class TimedSynergyStatBuff
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public PlayerStats.StatType statToBoost;

	public StatModifier.ModifyMethod modifyType;

	public float amount;

	public float duration;
}
