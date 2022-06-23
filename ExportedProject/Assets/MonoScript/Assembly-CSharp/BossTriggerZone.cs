using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(SpeculativeRigidbody))]
public class BossTriggerZone : BraveBehaviour
{
	public bool HasTriggered { get; set; }

	public RoomHandler ParentRoom { get; set; }

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		ParentRoom = GameManager.Instance.Dungeon.GetRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		if (ParentRoom != null)
		{
			if (ParentRoom.bossTriggerZones == null)
			{
				ParentRoom.bossTriggerZones = new List<BossTriggerZone>();
			}
			ParentRoom.bossTriggerZones.Add(this);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnTriggerCollision(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody myRigidbody, CollisionData collisionData)
	{
		if (HasTriggered || (collisionData.OtherPixelCollider.CollisionLayer != CollisionLayer.PlayerCollider && collisionData.OtherPixelCollider.CollisionLayer != 0))
		{
			return;
		}
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if (!component)
		{
			return;
		}
		List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
		for (int i = 0; i < allHealthHavers.Count; i++)
		{
			if (allHealthHavers[i].IsBoss)
			{
				GenericIntroDoer component2 = allHealthHavers[i].GetComponent<GenericIntroDoer>();
				if ((bool)component2 && component2.triggerType == GenericIntroDoer.TriggerType.BossTriggerZone)
				{
					ObjectVisibilityManager component3 = component2.GetComponent<ObjectVisibilityManager>();
					component3.ChangeToVisibility(RoomHandler.VisibilityStatus.CURRENT);
					component2.TriggerSequence(component);
					HasTriggered = true;
					break;
				}
			}
		}
	}
}
