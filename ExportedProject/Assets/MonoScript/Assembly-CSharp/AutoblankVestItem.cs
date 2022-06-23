using System;

public class AutoblankVestItem : PassiveItem
{
	[PickupIdentifier]
	public int ElderBlankID;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			HealthHaver obj = player.healthHaver;
			obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Combine(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(HandleEffect));
		}
	}

	private bool HasElderBlank()
	{
		if (m_owner.HasActiveItem(ElderBlankID))
		{
			return true;
		}
		return false;
	}

	private void HandleEffect(HealthHaver source, HealthHaver.ModifyDamageEventArgs args)
	{
		if (args == EventArgs.Empty || args.ModifiedDamage <= 0f || !source.IsVulnerable)
		{
			return;
		}
		if ((bool)m_owner && HasElderBlank())
		{
			for (int i = 0; i < m_owner.activeItems.Count; i++)
			{
				if (m_owner.activeItems[i].PickupObjectId == ElderBlankID && !m_owner.activeItems[i].IsOnCooldown)
				{
					source.TriggerInvulnerabilityPeriod();
					m_owner.ForceBlank();
					m_owner.activeItems[i].ForceApplyCooldown(m_owner);
					args.ModifiedDamage = 0f;
					return;
				}
			}
		}
		if ((bool)m_owner && m_owner.Blanks > 0 && !m_owner.IsFalling)
		{
			source.TriggerInvulnerabilityPeriod();
			m_owner.ForceConsumableBlank();
			args.ModifiedDamage = 0f;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		AutoblankVestItem component = debrisObject.GetComponent<AutoblankVestItem>();
		HealthHaver obj = player.healthHaver;
		obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Remove(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(HandleEffect));
		component.m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			HealthHaver obj = m_owner.healthHaver;
			obj.ModifyDamage = (Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>)Delegate.Remove(obj.ModifyDamage, new Action<HealthHaver, HealthHaver.ModifyDamageEventArgs>(HandleEffect));
		}
		base.OnDestroy();
	}
}
