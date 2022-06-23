using UnityEngine;

public class PassiveGooperItem : PassiveItem
{
	public enum Condition
	{
		WhileDodgeRolling,
		Always,
		OnDamaged
	}

	public Condition condition;

	public bool IsDegooperator;

	public bool TranslatesGleepGlorp;

	public GoopDefinition goopType;

	public float goopRadius;

	public DamageTypeModifier[] modifiers;

	public PassiveGooperSynergy[] Synergies;

	public float AirSoftSynergyAmmoGainRate = 0.05f;

	private GoopDefinition m_cachedGoopType;

	private float m_synergyAccumAmmo;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_cachedGoopType = goopType;
			base.Pickup(player);
			if (TranslatesGleepGlorp)
			{
				player.UnderstandsGleepGlorp = true;
			}
			if (condition == Condition.WhileDodgeRolling)
			{
				player.OnIsRolling += OnRollFrame;
			}
			else if (condition == Condition.OnDamaged)
			{
				player.OnReceivedDamage += HandleReceivedDamage;
			}
			m_player = player;
			for (int i = 0; i < modifiers.Length; i++)
			{
				player.healthHaver.damageTypeModifiers.Add(modifiers[i]);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!m_pickedUp || !(m_player != null) || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if (condition == Condition.Always)
		{
			DoGoop();
		}
		for (int i = 0; i < Synergies.Length; i++)
		{
			if (!Synergies[i].m_processed && m_player.HasActiveBonusSynergy(Synergies[i].RequiredSynergy))
			{
				Synergies[i].m_processed = true;
				goopType = Synergies[i].overrideGoopType;
				m_player.healthHaver.damageTypeModifiers.Add(Synergies[i].AdditionalDamageModifier);
			}
			else if (Synergies[i].m_processed && !m_player.HasActiveBonusSynergy(Synergies[i].RequiredSynergy))
			{
				Synergies[i].m_processed = false;
				goopType = m_cachedGoopType;
				m_player.healthHaver.damageTypeModifiers.Remove(Synergies[i].AdditionalDamageModifier);
			}
		}
	}

	private void DoGoop()
	{
		if (IsDegooperator)
		{
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.AIR_SOFT) && (bool)base.Owner.CurrentGun)
			{
				int num = DeadlyDeadlyGoopManager.CountGoopsInRadius(m_player.specRigidbody.UnitCenter, goopRadius);
				if (num > 0)
				{
					m_synergyAccumAmmo += (float)num * AirSoftSynergyAmmoGainRate;
					if (m_synergyAccumAmmo > 1f)
					{
						base.Owner.CurrentGun.GainAmmo(Mathf.FloorToInt(m_synergyAccumAmmo));
						m_synergyAccumAmmo -= Mathf.FloorToInt(m_synergyAccumAmmo);
					}
				}
			}
			DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(m_player.specRigidbody.UnitCenter, goopRadius);
		}
		else if (condition == Condition.OnDamaged)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopType).TimedAddGoopCircle(m_player.specRigidbody.UnitCenter, goopRadius);
		}
		else
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopType).AddGoopCircle(m_player.specRigidbody.UnitCenter, goopRadius);
		}
	}

	private void HandleReceivedDamage(PlayerController obj)
	{
		DoGoop();
	}

	private void OnRollFrame(PlayerController obj)
	{
		DoGoop();
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<PassiveGooperItem>().m_pickedUpThisRun = true;
		for (int i = 0; i < modifiers.Length; i++)
		{
			player.healthHaver.damageTypeModifiers.Remove(modifiers[i]);
		}
		if (condition == Condition.WhileDodgeRolling)
		{
			player.OnIsRolling -= OnRollFrame;
		}
		else if (condition == Condition.OnDamaged)
		{
			player.OnReceivedDamage -= HandleReceivedDamage;
		}
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_player != null)
		{
			if (condition == Condition.WhileDodgeRolling)
			{
				m_player.OnIsRolling -= OnRollFrame;
			}
			else if (condition == Condition.OnDamaged)
			{
				m_player.OnReceivedDamage -= HandleReceivedDamage;
			}
			for (int i = 0; i < modifiers.Length; i++)
			{
				m_player.healthHaver.damageTypeModifiers.Remove(modifiers[i]);
			}
		}
		base.OnDestroy();
	}
}
