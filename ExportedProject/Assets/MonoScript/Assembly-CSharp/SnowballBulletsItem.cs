public class SnowballBulletsItem : PassiveItem
{
	public float PercentScaleGainPerUnit = 10f;

	public float PercentDamageGainPerUnit = 2.5f;

	public float DamageMultiplierCap = 2.5f;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += HandleProjectile;
			player.PostProcessBeamChanceTick += HandleBeamFrame;
		}
	}

	private void HandleBeamFrame(BeamController sourceBeam)
	{
		if (sourceBeam is BasicBeamController)
		{
			BasicBeamController basicBeamController = sourceBeam as BasicBeamController;
			basicBeamController.ProjectileScale = (basicBeamController.Owner as PlayerController).BulletScaleModifier + basicBeamController.ApproximateDistance * (PercentScaleGainPerUnit / 100f);
		}
	}

	private void HandleProjectile(Projectile targetProjectile, float arg2)
	{
		ScalingProjectileModifier scalingProjectileModifier = targetProjectile.gameObject.AddComponent<ScalingProjectileModifier>();
		scalingProjectileModifier.ScaleToDamageRatio = PercentDamageGainPerUnit / PercentScaleGainPerUnit;
		scalingProjectileModifier.MaximumDamageMultiplier = DamageMultiplierCap;
		scalingProjectileModifier.IsSynergyContingent = false;
		if (base.Owner.HasActiveBonusSynergy(CustomSynergyType.SNOWBREAKERS))
		{
			scalingProjectileModifier.PercentGainPerUnit = PercentScaleGainPerUnit * 1.5f;
			scalingProjectileModifier.ScaleMultiplier = 2f;
		}
		else
		{
			scalingProjectileModifier.PercentGainPerUnit = PercentScaleGainPerUnit;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<SnowballBulletsItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= HandleProjectile;
		player.PostProcessBeamChanceTick -= HandleBeamFrame;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= HandleProjectile;
			m_player.PostProcessBeamChanceTick -= HandleBeamFrame;
		}
	}
}
