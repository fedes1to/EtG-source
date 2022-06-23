using System;
using UnityEngine;

public class HealingReceivedModificationItem : PassiveItem
{
	public float ChanceToImproveHealing = 0.5f;

	public float HealingImprovedBy = 0.5f;

	public GameObject OnImprovedHealingVFX;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			HealthHaver obj = player.healthHaver;
			obj.ModifyHealing = (Action<HealthHaver, HealthHaver.ModifyHealingEventArgs>)Delegate.Combine(obj.ModifyHealing, new Action<HealthHaver, HealthHaver.ModifyHealingEventArgs>(ModifyIncomingHealing));
			base.Pickup(player);
		}
	}

	private void ModifyIncomingHealing(HealthHaver source, HealthHaver.ModifyHealingEventArgs args)
	{
		if (args != EventArgs.Empty && UnityEngine.Random.value < ChanceToImproveHealing)
		{
			if (OnImprovedHealingVFX != null)
			{
				source.GetComponent<PlayerController>().PlayEffectOnActor(OnImprovedHealingVFX, Vector3.zero);
			}
			args.ModifiedHealing += HealingImprovedBy;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		HealthHaver obj = player.healthHaver;
		obj.ModifyHealing = (Action<HealthHaver, HealthHaver.ModifyHealingEventArgs>)Delegate.Remove(obj.ModifyHealing, new Action<HealthHaver, HealthHaver.ModifyHealingEventArgs>(ModifyIncomingHealing));
		debrisObject.GetComponent<HealingReceivedModificationItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp)
		{
			HealthHaver obj = m_owner.healthHaver;
			obj.ModifyHealing = (Action<HealthHaver, HealthHaver.ModifyHealingEventArgs>)Delegate.Combine(obj.ModifyHealing, new Action<HealthHaver, HealthHaver.ModifyHealingEventArgs>(ModifyIncomingHealing));
		}
		base.OnDestroy();
	}
}
