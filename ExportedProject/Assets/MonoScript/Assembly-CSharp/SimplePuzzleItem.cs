using System;
using UnityEngine;

public class SimplePuzzleItem : PickupObject
{
	private bool m_pickedUp;

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnPreCollision));
	}

	private void OnPreCollision(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (!m_pickedUp)
		{
			PlayerController component = otherRigidbody.GetComponent<PlayerController>();
			if (component != null)
			{
				Pickup(component);
				AkSoundEngine.PostEvent("Play_OBJ_item_pickup_01", base.gameObject);
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_pickedUp = true;
			base.specRigidbody.enabled = false;
			base.renderer.enabled = false;
			DebrisObject component = GetComponent<DebrisObject>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
				UnityEngine.Object.Destroy(base.specRigidbody);
				player.AcquirePuzzleItem(this);
			}
			else
			{
				UnityEngine.Object.Instantiate(base.gameObject);
				player.AcquirePuzzleItem(this);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
