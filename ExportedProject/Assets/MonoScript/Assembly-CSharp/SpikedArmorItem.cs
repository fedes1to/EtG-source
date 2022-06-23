using System;
using System.Collections.Generic;
using UnityEngine;

public class SpikedArmorItem : BasicStatPickup
{
	public bool HasIgniteSynergy;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public GameActorFireEffect IgniteEffect;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
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
			if (HasIgniteSynergy)
			{
				SpeculativeRigidbody speculativeRigidbody = player.specRigidbody;
				speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
			}
			base.Pickup(player);
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (HasIgniteSynergy && (bool)m_owner && m_owner.HasActiveBonusSynergy(RequiredSynergy) && (bool)rigidbodyCollision.OtherRigidbody && (bool)rigidbodyCollision.OtherRigidbody.aiActor)
		{
			AIActor aIActor = rigidbodyCollision.OtherRigidbody.aiActor;
			if (aIActor.IsNormalEnemy && !aIActor.IsHarmlessEnemy)
			{
				aIActor.ApplyEffect(IgniteEffect);
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if (PassiveItem.ActiveFlagItems[player].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[player][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[player][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[player].Remove(GetType());
			}
		}
		if ((bool)player && (bool)player.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = player.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		}
		debrisObject.GetComponent<SpikedArmorItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		BraveTime.ClearMultiplier(base.gameObject);
		if (m_pickedUp && PassiveItem.ActiveFlagItems.ContainsKey(m_owner) && PassiveItem.ActiveFlagItems[m_owner].ContainsKey(GetType()))
		{
			PassiveItem.ActiveFlagItems[m_owner][GetType()] = Mathf.Max(0, PassiveItem.ActiveFlagItems[m_owner][GetType()] - 1);
			if (PassiveItem.ActiveFlagItems[m_owner][GetType()] == 0)
			{
				PassiveItem.ActiveFlagItems[m_owner].Remove(GetType());
			}
		}
		if ((bool)m_owner && (bool)m_owner.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = m_owner.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		}
		base.OnDestroy();
	}
}
