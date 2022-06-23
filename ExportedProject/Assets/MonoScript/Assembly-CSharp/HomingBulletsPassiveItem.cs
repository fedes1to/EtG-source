using UnityEngine;

public class HomingBulletsPassiveItem : PassiveItem
{
	public float ActivationChance = 1f;

	public float homingRadius = 5f;

	public float homingAngularVelocity = 360f;

	public bool SynergyIncreasesDamageIfNotActive;

	[LongNumericEnum]
	public CustomSynergyType SynergyRequired;

	public float SynergyDamageMultiplier = 1.5f;

	protected PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeamChanceTick += PostProcessBeamChanceTick;
		}
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		if (Random.value > ActivationChance * effectChanceScalar)
		{
			if (SynergyIncreasesDamageIfNotActive && (bool)m_player && m_player.HasActiveBonusSynergy(SynergyRequired))
			{
				obj.baseData.damage *= SynergyDamageMultiplier;
				obj.RuntimeUpdateScale(SynergyDamageMultiplier);
			}
			return;
		}
		HomingModifier homingModifier = obj.gameObject.GetComponent<HomingModifier>();
		if (homingModifier == null)
		{
			homingModifier = obj.gameObject.AddComponent<HomingModifier>();
			homingModifier.HomingRadius = 0f;
			homingModifier.AngularVelocity = 0f;
		}
		float num = ((!SynergyIncreasesDamageIfNotActive || !m_player || !m_player.HasActiveBonusSynergy(SynergyRequired)) ? 1f : 2f);
		homingModifier.HomingRadius += homingRadius * num;
		homingModifier.AngularVelocity += homingAngularVelocity * num;
	}

	private void PostProcessBeamChanceTick(BeamController beam)
	{
		if (!(Random.value > ActivationChance))
		{
			beam.ChanceBasedHomingRadius += homingRadius;
			beam.ChanceBasedHomingAngularVelocity += homingAngularVelocity;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<HomingBulletsPassiveItem>().m_pickedUpThisRun = true;
		m_player = null;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeamChanceTick -= PostProcessBeamChanceTick;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeamChanceTick -= PostProcessBeamChanceTick;
		}
	}
}
