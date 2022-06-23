using System;

public class ActiveAmmunitionData
{
	public int ShotsRemaining;

	public float DamageModifier = 1f;

	public float SpeedModifier = 1f;

	public float RangeModifier = 1f;

	public Action<Projectile, Gun> OnAmmoModification;

	public void HandleAmmunition(Projectile p, Gun g)
	{
		p.baseData.damage *= DamageModifier;
		p.baseData.speed *= SpeedModifier;
		p.baseData.range *= RangeModifier;
		if (OnAmmoModification != null)
		{
			OnAmmoModification(p, g);
		}
	}
}
