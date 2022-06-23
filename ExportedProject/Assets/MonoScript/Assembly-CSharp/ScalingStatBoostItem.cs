using UnityEngine;

public class ScalingStatBoostItem : PassiveItem
{
	public enum ScalingModeTarget
	{
		CURRENCY,
		CURSE
	}

	public PlayerStats.StatType TargetStat = PlayerStats.StatType.Damage;

	public float MinScaling = 1f;

	public float MaxScaling = 2f;

	public float ScalingTargetMin;

	public float ScalingTargetMax = 500f;

	public bool TintBullets;

	public bool TintBeams;

	public Color TintColor = Color.yellow;

	public int TintPriority = 2;

	public AnimationCurve ScalingCurve;

	public ScalingModeTarget ScalingTarget;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
		}
	}

	private void PostProcessBeam(BeamController beam)
	{
		if ((bool)beam)
		{
			Projectile projectile = beam.projectile;
			if ((bool)projectile)
			{
				PostProcessProjectile(projectile, 1f);
			}
			beam.AdjustPlayerBeamTint(TintColor.WithAlpha(TintColor.a / 2f), TintPriority);
		}
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		if ((bool)m_player)
		{
			float t = 0f;
			switch (ScalingTarget)
			{
			case ScalingModeTarget.CURRENCY:
				t = Mathf.Clamp01(Mathf.InverseLerp(ScalingTargetMin, ScalingTargetMax, m_player.carriedConsumables.Currency));
				t = ScalingCurve.Evaluate(t);
				break;
			case ScalingModeTarget.CURSE:
				t = Mathf.Clamp01(Mathf.InverseLerp(ScalingTargetMin, ScalingTargetMax, m_player.stats.GetStatValue(PlayerStats.StatType.Curse)));
				t = ScalingCurve.Evaluate(t);
				break;
			}
			float num = Mathf.Lerp(MinScaling, MaxScaling, t);
			PlayerStats.StatType targetStat = TargetStat;
			if (targetStat == PlayerStats.StatType.Damage)
			{
				obj.baseData.damage *= num;
			}
			if (TintBullets)
			{
				obj.AdjustPlayerProjectileTint(TintColor, TintPriority);
			}
			if (ScalingTarget == ScalingModeTarget.CURSE)
			{
				obj.CurseSparks = true;
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<ScalingStatBoostItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
		}
	}
}
