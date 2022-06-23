using System;
using System.Collections.Generic;
using UnityEngine;

public class SunglassesItem : PassiveItem
{
	public static bool SunglassesActive;

	public float timeScaleMultiplier = 0.25f;

	public float Duration = 3f;

	public float InternalCooldown = 5f;

	private float m_remainingSlowTime;

	private float m_internalCooldown;

	public float MIBSynergyScale = 1.33f;

	public float MIBSynergyDamage = 1.8f;

	protected override void Update()
	{
		base.Update();
		if (m_owner != null && m_pickedUp)
		{
			if (m_remainingSlowTime <= 0f)
			{
				m_internalCooldown -= BraveTime.DeltaTime;
				BraveTime.ClearMultiplier(base.gameObject);
				SunglassesActive = false;
			}
			else
			{
				SunglassesActive = true;
				m_remainingSlowTime -= GameManager.INVARIANT_DELTA_TIME;
				BraveTime.SetTimeScaleMultiplier(timeScaleMultiplier, base.gameObject);
			}
		}
		else
		{
			BraveTime.ClearMultiplier(base.gameObject);
		}
	}

	private void OnExplosion()
	{
		if (!(m_internalCooldown > 0f))
		{
			m_internalCooldown = InternalCooldown;
			m_remainingSlowTime = Duration;
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.MEN_IN_BLACK) && (bool)base.Owner.CurrentGun)
			{
				AkSoundEngine.PostEvent("Play_WPN_active_reload_01", base.gameObject);
				base.Owner.CurrentGun.ForceImmediateReload();
				base.Owner.CurrentGun.TriggerTemporaryBoost(MIBSynergyDamage, MIBSynergyScale, Duration, true);
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			Exploder.OnExplosionTriggered = (Action)Delegate.Combine(Exploder.OnExplosionTriggered, new Action(OnExplosion));
			if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
			{
				PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
			}
			if (!PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
			{
				PassiveItem.ActiveFlagItems[player].Add(GetType(), 1);
			}
			else
			{
				PassiveItem.ActiveFlagItems[player][GetType()] = PassiveItem.ActiveFlagItems[player][GetType()] + 1;
			}
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		Exploder.OnExplosionTriggered = (Action)Delegate.Remove(Exploder.OnExplosionTriggered, new Action(OnExplosion));
		BraveTime.ClearMultiplier(base.gameObject);
		if (PassiveItem.ActiveFlagItems.ContainsKey(player) && PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		debrisObject.GetComponent<SunglassesItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		SunglassesActive = false;
		BraveTime.ClearMultiplier(base.gameObject);
		Exploder.OnExplosionTriggered = (Action)Delegate.Remove(Exploder.OnExplosionTriggered, new Action(OnExplosion));
		BraveTime.ClearMultiplier(base.gameObject);
		if (m_pickedUp && PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
		base.OnDestroy();
	}
}
