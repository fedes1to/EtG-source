using System;

[Serializable]
public class DamageTypeModifier
{
	public CoreDamageTypes damageType;

	public float damageMultiplier = 1f;

	public DamageTypeModifier()
	{
	}

	public DamageTypeModifier(DamageTypeModifier other)
	{
		damageType = other.damageType;
		damageMultiplier = other.damageMultiplier;
	}
}
