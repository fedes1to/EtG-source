using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[RequireComponent(typeof(GenericIntroDoer))]
public class GunonIntroDoer : SpecificIntroDoer
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void PlayerWalkedIn(PlayerController player, List<tk2dSpriteAnimator> animators)
	{
		base.aiAnimator.LockFacingDirection = true;
		base.aiAnimator.FacingDirection = -90f;
		RoomHandler parentRoom = base.aiActor.ParentRoom;
		if (parentRoom == null)
		{
			return;
		}
		List<TorchController> componentsInRoom = parentRoom.GetComponentsInRoom<TorchController>();
		for (int i = 0; i < componentsInRoom.Count; i++)
		{
			TorchController torchController = componentsInRoom[i];
			if ((bool)torchController && (bool)torchController.specRigidbody)
			{
				torchController.specRigidbody.CollideWithOthers = false;
			}
		}
	}

	public override void EndIntro()
	{
		base.aiAnimator.LockFacingDirection = false;
		base.aiAnimator.EndAnimation();
	}
}
