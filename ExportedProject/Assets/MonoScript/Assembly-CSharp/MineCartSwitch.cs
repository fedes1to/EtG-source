using System;
using System.Collections.Generic;

public class MineCartSwitch : DungeonPlaceableBehaviour
{
	[DwarfConfigurable]
	public float PrimaryPathIndex;

	[DwarfConfigurable]
	public float TogglePathIndex = 1f;

	private void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (rigidbodyCollision.OtherRigidbody.projectile != null)
		{
			List<PathMover> componentsInRoom = GetAbsoluteParentRoom().GetComponentsInRoom<PathMover>();
			for (int i = 0; i < componentsInRoom.Count; i++)
			{
				componentsInRoom[i].IsUsingAlternateTargets = !componentsInRoom[i].IsUsingAlternateTargets;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
