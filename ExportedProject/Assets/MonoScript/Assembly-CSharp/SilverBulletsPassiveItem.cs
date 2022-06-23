using UnityEngine;

public class SilverBulletsPassiveItem : PassiveItem
{
	public float BlackPhantomDamageMultiplier = 2f;

	private PlayerController m_player;

	public bool TintBullets;

	public bool TintBeams;

	public Color TintColor = Color.grey;

	public int TintPriority = 1;

	public GameObject SynergyPowerVFX;

	private StatModifier m_synergyStat;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
			player.OnKilledEnemyContext += HandleKilledEnemy;
		}
	}

	private void HandleKilledEnemy(PlayerController sourcePlayer, HealthHaver killedEnemy)
	{
		if (sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.BLESSED_CURSED_BULLETS) && (bool)killedEnemy && (bool)killedEnemy.aiActor && killedEnemy.aiActor.IsBlackPhantom)
		{
			if (m_synergyStat == null)
			{
				m_synergyStat = StatModifier.Create(PlayerStats.StatType.Damage, StatModifier.ModifyMethod.MULTIPLICATIVE, 1f);
				sourcePlayer.ownerlessStatModifiers.Add(m_synergyStat);
			}
			m_synergyStat.amount += 0.0025f;
			sourcePlayer.PlayEffectOnActor(SynergyPowerVFX, new Vector3(0f, -0.5f, 0f));
			sourcePlayer.stats.RecalculateStats(sourcePlayer);
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
			obj.BlackPhantomDamageMultiplier *= BlackPhantomDamageMultiplier;
			if ((bool)m_player && m_player.HasActiveBonusSynergy(CustomSynergyType.DEMONHUNTER))
			{
				obj.BlackPhantomDamageMultiplier *= 1.5f;
			}
			if (TintBullets)
			{
				obj.AdjustPlayerProjectileTint(TintColor, TintPriority);
			}
		}
	}

	private void RemoveSynergyStat(PlayerController targetPlayer)
	{
		if (m_synergyStat != null && (bool)targetPlayer)
		{
			targetPlayer.ownerlessStatModifiers.Remove(m_synergyStat);
			targetPlayer.stats.RecalculateStats(targetPlayer);
			m_synergyStat = null;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<SilverBulletsPassiveItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		player.OnKilledEnemyContext -= HandleKilledEnemy;
		RemoveSynergyStat(player);
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
			m_player.OnKilledEnemyContext -= HandleKilledEnemy;
			RemoveSynergyStat(m_player);
		}
	}
}
