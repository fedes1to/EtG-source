using System;
using UnityEngine;

public class FragileGunItemPiece : PickupObject
{
	[NonSerialized]
	public int AssignedGunId = -1;

	private bool m_pickedUp;

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(TriggerWasEntered));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTrigger));
		IgnoredByRat = true;
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
	}

	public void AssignGun(Gun sourceGun)
	{
		AssignedGunId = sourceGun.PickupObjectId;
		if ((bool)sourceGun.sprite)
		{
			base.sprite.SetSprite(sourceGun.sprite.Collection, sourceGun.sprite.spriteId);
		}
	}

	private void TriggerWasEntered(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody, CollisionData collisionData)
	{
		if (!m_pickedUp)
		{
			if (otherRigidbody.GetComponent<PlayerController>() != null)
			{
				PrePickupLogic(otherRigidbody, selfRigidbody);
			}
			else if (otherRigidbody.GetComponent<PickupObject>() != null && (bool)base.debris)
			{
				base.debris.ApplyVelocity((selfRigidbody.UnitCenter - otherRigidbody.UnitCenter).normalized);
				selfRigidbody.RegisterGhostCollisionException(otherRigidbody);
			}
		}
	}

	public void OnTrigger(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody, CollisionData collisionData)
	{
		if (!m_pickedUp && otherRigidbody.GetComponent<PlayerController>() != null)
		{
			PrePickupLogic(otherRigidbody, selfRigidbody);
		}
	}

	private void PrePickupLogic(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody selfRigidbody)
	{
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if (!component.IsGhost && CheckPlayerForItem(component))
		{
			Pickup(component);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private bool CheckPlayerForItem(PlayerController player)
	{
		if ((bool)player)
		{
			for (int i = 0; i < player.passiveItems.Count; i++)
			{
				if (player.passiveItems[i] is FragileGunItem)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Pickup(PlayerController player)
	{
		if (player.IsGhost)
		{
			return;
		}
		m_pickedUp = true;
		FragileGunItem fragileGunItem = null;
		for (int i = 0; i < player.passiveItems.Count; i++)
		{
			if (player.passiveItems[i] is FragileGunItem)
			{
				fragileGunItem = player.passiveItems[i] as FragileGunItem;
				break;
			}
		}
		if ((bool)fragileGunItem)
		{
			fragileGunItem.AcquirePiece(this);
		}
	}
}
