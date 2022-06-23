using UnityEngine;

public class BulletStatusEffectItem : PassiveItem
{
	public float chanceOfActivating = 1f;

	public float chanceFromBeamPerSecond = 1f;

	public bool TintBullets;

	public bool TintBeams;

	public Color TintColor = Color.green;

	public int TintPriority = 5;

	public GameObject ParticlesToAdd;

	public bool AddsDamageType;

	[EnumFlags]
	public CoreDamageTypes DamageTypesToAdd;

	[Header("Status Effects")]
	public bool AppliesSpeedModifier;

	public GameActorSpeedEffect SpeedModifierEffect;

	public bool AppliesDamageOverTime;

	public GameActorHealthEffect HealthModifierEffect;

	public bool AppliesCharm;

	public GameActorCharmEffect CharmModifierEffect;

	public bool AppliesFreeze;

	public GameActorFreezeEffect FreezeModifierEffect;

	[ShowInInspectorIf("AppliesFreeze", false)]
	public bool FreezeScalesWithDamage;

	[ShowInInspectorIf("FreezeScalesWithDamage", false)]
	public float FreezeAmountPerDamage = 1f;

	public bool AppliesFire;

	public GameActorFireEffect FireModifierEffect;

	public bool ConfersElectricityImmunity;

	public bool AppliesTransmog;

	[EnemyIdentifier]
	public string TransmogTargetGuid;

	public BulletStatusEffectItemSynergy[] Synergies;

	private PlayerController m_player;

	private DamageTypeModifier m_electricityImmunity;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			if (ConfersElectricityImmunity)
			{
				m_electricityImmunity = new DamageTypeModifier();
				m_electricityImmunity.damageMultiplier = 0f;
				m_electricityImmunity.damageType = CoreDamageTypes.Electric;
				player.healthHaver.damageTypeModifiers.Add(m_electricityImmunity);
			}
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
			player.PostProcessBeamTick += PostProcessBeamTick;
		}
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		float num = chanceOfActivating;
		if (chanceOfActivating < 1f)
		{
			num = chanceOfActivating * effectChanceScalar;
		}
		if (AppliesFreeze || AppliesFire || AppliesDamageOverTime)
		{
			if ((bool)m_player && m_player.HasActiveBonusSynergy(CustomSynergyType.ALPHA_STATUS) && m_player.CurrentGun.LastShotIndex == 0)
			{
				num = 1f;
			}
			if ((bool)m_player && m_player.HasActiveBonusSynergy(CustomSynergyType.OMEGA_STATUS) && m_player.CurrentGun.LastShotIndex == m_player.CurrentGun.ClipCapacity - 1)
			{
				num = 1f;
			}
		}
		if (AppliesCharm && (bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.UNBELIEVABLY_CHARMING))
		{
			num = 1f;
		}
		if (AppliesTransmog && (bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.BE_A_CHICKEN))
		{
			num *= 1.5f;
		}
		if ((bool)m_player)
		{
			for (int i = 0; i < Synergies.Length; i++)
			{
				if (m_player.HasActiveBonusSynergy(Synergies[i].RequiredSynergy))
				{
					num *= Synergies[i].ChanceMultiplier;
				}
			}
		}
		if (!(Random.value < num))
		{
			return;
		}
		if (AddsDamageType)
		{
			obj.damageTypes |= DamageTypesToAdd;
		}
		if (ParticlesToAdd != null)
		{
			GameObject gameObject = SpawnManager.SpawnVFX(ParticlesToAdd, true);
			gameObject.transform.parent = obj.transform;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0.5f);
			ParticleKiller component = gameObject.GetComponent<ParticleKiller>();
			if (component != null)
			{
				component.Awake();
			}
		}
		if (AppliesSpeedModifier)
		{
			obj.statusEffectsToApply.Add(SpeedModifierEffect);
		}
		if (AppliesDamageOverTime)
		{
			obj.statusEffectsToApply.Add(HealthModifierEffect);
		}
		if (AppliesFreeze)
		{
			GameActorFreezeEffect freezeModifierEffect = FreezeModifierEffect;
			if (FreezeScalesWithDamage)
			{
				freezeModifierEffect.FreezeAmount = obj.ModifiedDamage * FreezeAmountPerDamage;
			}
			obj.statusEffectsToApply.Add(freezeModifierEffect);
		}
		if (AppliesCharm)
		{
			obj.statusEffectsToApply.Add(CharmModifierEffect);
		}
		if (AppliesFire)
		{
			obj.statusEffectsToApply.Add(FireModifierEffect);
		}
		if (AppliesTransmog && !obj.CanTransmogrify)
		{
			obj.CanTransmogrify = true;
			obj.ChanceToTransmogrify = 1f;
			obj.TransmogrifyTargetGuids = new string[1];
			obj.TransmogrifyTargetGuids[0] = TransmogTargetGuid;
		}
		if (TintBullets)
		{
			obj.AdjustPlayerProjectileTint(TintColor, TintPriority);
		}
	}

	private void PostProcessBeam(BeamController beam)
	{
		if (TintBeams)
		{
			beam.AdjustPlayerBeamTint(TintColor.WithAlpha(TintColor.a / 2f), TintPriority);
		}
	}

	private void PostProcessBeamTick(BeamController beam, SpeculativeRigidbody hitRigidbody, float tickRate)
	{
		GameActor gameActor = hitRigidbody.gameActor;
		if ((bool)gameActor && Random.value < BraveMathCollege.SliceProbability(chanceFromBeamPerSecond, tickRate))
		{
			if (AppliesSpeedModifier)
			{
				gameActor.ApplyEffect(SpeedModifierEffect);
			}
			if (AppliesDamageOverTime)
			{
				gameActor.ApplyEffect(HealthModifierEffect);
			}
			if (AppliesFreeze)
			{
				gameActor.ApplyEffect(FreezeModifierEffect);
			}
			if (AppliesCharm)
			{
				gameActor.ApplyEffect(CharmModifierEffect);
			}
			if (AppliesFire)
			{
				gameActor.ApplyEffect(FireModifierEffect);
			}
			if (AppliesTransmog && gameActor is AIActor)
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
		debrisObject.GetComponent<BulletStatusEffectItem>().m_pickedUpThisRun = true;
		if (m_electricityImmunity != null)
		{
			player.healthHaver.damageTypeModifiers.Remove(m_electricityImmunity);
		}
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
			if (m_electricityImmunity != null)
			{
				m_player.healthHaver.damageTypeModifiers.Remove(m_electricityImmunity);
			}
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
			m_player.PostProcessBeamTick -= PostProcessBeamTick;
		}
	}
}
