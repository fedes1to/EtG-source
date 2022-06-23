using UnityEngine;

public class ChaosBulletsItem : PassiveItem
{
	[Header("Nonstatus Effects")]
	public float ChanceToAddBounce;

	public float ChanceToAddPierce;

	public float ChanceToFat = 0.1f;

	public float MinFatScale = 1.25f;

	public float MaxFatScale = 1.75f;

	public bool UsesVelocityModificationCurve;

	public AnimationCurve VelocityModificationCurve;

	[Header("Status Effects")]
	public float ChanceOfActivatingStatusEffect = 1f;

	public float ChanceOfStatusEffectFromBeamPerSecond = 1f;

	public float SpeedModifierWeight;

	public GameActorSpeedEffect SpeedModifierEffect;

	public Color SpeedTintColor;

	public float PoisonModifierWeight;

	public GameActorHealthEffect HealthModifierEffect;

	public Color PoisonTintColor;

	public float CharmModifierWeight;

	public GameActorCharmEffect CharmModifierEffect;

	public Color CharmTintColor;

	public float FreezeModifierWeight;

	public GameActorFreezeEffect FreezeModifierEffect;

	public bool FreezeScalesWithDamage;

	public float FreezeAmountPerDamage = 1f;

	public Color FreezeTintColor;

	public float BurnModifierWeight;

	public GameActorFireEffect FireModifierEffect;

	public Color FireTintColor;

	public float TransmogrifyModifierWeight;

	[EnemyIdentifier]
	public string TransmogTargetGuid;

	public Color TransmogrifyTintColor;

	public bool TintBullets;

	public bool TintBeams;

	public int TintPriority = 6;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
			player.PostProcessBeamTick += PostProcessBeamTick;
		}
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		if (UsesVelocityModificationCurve)
		{
			obj.baseData.speed *= VelocityModificationCurve.Evaluate(Random.value);
		}
		int num = 0;
		while (Random.value < ChanceToAddBounce && num < 10)
		{
			num++;
			BounceProjModifier component = obj.GetComponent<BounceProjModifier>();
			if (component == null)
			{
				component = obj.gameObject.AddComponent<BounceProjModifier>();
				component.numberOfBounces = 1;
			}
			else
			{
				component.numberOfBounces++;
			}
		}
		num = 0;
		while (Random.value < ChanceToAddPierce && num < 10)
		{
			num++;
			PierceProjModifier component2 = obj.GetComponent<PierceProjModifier>();
			if (component2 == null)
			{
				component2 = obj.gameObject.AddComponent<PierceProjModifier>();
				component2.penetration = 2;
				component2.penetratesBreakables = true;
				component2.BeastModeLevel = PierceProjModifier.BeastModeStatus.NOT_BEAST_MODE;
			}
			else
			{
				component2.penetration += 2;
			}
		}
		if (Random.value < ChanceToFat)
		{
			float num2 = Random.Range(MinFatScale, MaxFatScale);
			obj.AdditionalScaleMultiplier *= num2;
		}
		float num3 = ChanceOfActivatingStatusEffect;
		if (ChanceOfActivatingStatusEffect < 1f)
		{
			num3 = ChanceOfActivatingStatusEffect * effectChanceScalar;
		}
		if (!(Random.value < num3))
		{
			return;
		}
		Color targetTintColor = Color.white;
		float num4 = SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight + BurnModifierWeight + TransmogrifyModifierWeight;
		float num5 = num4 * Random.value;
		if (num5 < SpeedModifierWeight)
		{
			targetTintColor = SpeedTintColor;
			obj.statusEffectsToApply.Add(SpeedModifierEffect);
		}
		else if (num5 < SpeedModifierWeight + PoisonModifierWeight)
		{
			targetTintColor = PoisonTintColor;
			obj.statusEffectsToApply.Add(HealthModifierEffect);
		}
		else if (num5 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight)
		{
			targetTintColor = FreezeTintColor;
			GameActorFreezeEffect freezeModifierEffect = FreezeModifierEffect;
			if (FreezeScalesWithDamage)
			{
				freezeModifierEffect.FreezeAmount = obj.ModifiedDamage * FreezeAmountPerDamage;
			}
			obj.statusEffectsToApply.Add(freezeModifierEffect);
		}
		else if (num5 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight)
		{
			targetTintColor = CharmTintColor;
			obj.statusEffectsToApply.Add(CharmModifierEffect);
		}
		else if (num5 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight + BurnModifierWeight)
		{
			targetTintColor = FireTintColor;
			obj.statusEffectsToApply.Add(FireModifierEffect);
		}
		else if (num5 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight + BurnModifierWeight + TransmogrifyModifierWeight)
		{
			targetTintColor = TransmogrifyTintColor;
			obj.CanTransmogrify = true;
			obj.ChanceToTransmogrify = 1f;
			obj.TransmogrifyTargetGuids = new string[1];
			obj.TransmogrifyTargetGuids[0] = TransmogTargetGuid;
		}
		if (TintBullets)
		{
			obj.AdjustPlayerProjectileTint(targetTintColor, TintPriority);
		}
	}

	private void PostProcessBeam(BeamController beam)
	{
		if (!TintBeams)
		{
		}
	}

	private void PostProcessBeamTick(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickRate)
	{
		GameActor gameActor = hitRigidbody.gameActor;
		if ((bool)gameActor && Random.value < BraveMathCollege.SliceProbability(ChanceOfStatusEffectFromBeamPerSecond, tickRate))
		{
			float num = SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight + BurnModifierWeight + TransmogrifyModifierWeight;
			float num2 = num * Random.value;
			if (num2 < SpeedModifierWeight)
			{
				gameActor.ApplyEffect(SpeedModifierEffect);
			}
			else if (num2 < SpeedModifierWeight + PoisonModifierWeight)
			{
				gameActor.ApplyEffect(HealthModifierEffect);
			}
			else if (num2 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight)
			{
				gameActor.ApplyEffect(FreezeModifierEffect);
			}
			else if (num2 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight)
			{
				gameActor.ApplyEffect(CharmModifierEffect);
			}
			else if (num2 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight + BurnModifierWeight)
			{
				gameActor.ApplyEffect(FireModifierEffect);
			}
			else if (num2 < SpeedModifierWeight + PoisonModifierWeight + FreezeModifierWeight + CharmModifierWeight + BurnModifierWeight + TransmogrifyModifierWeight && gameActor is AIActor)
			{
				AIActor aIActor = gameActor as AIActor;
				aIActor.Transmogrify(EnemyDatabase.GetOrLoadByGuid(TransmogTargetGuid), (GameObject)ResourceCache.Acquire("Global VFX/VFX_Item_Spawn_Poof"));
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<ChaosBulletsItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		player.PostProcessBeamTick -= PostProcessBeamTick;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
			m_player.PostProcessBeamTick -= PostProcessBeamTick;
		}
	}
}
