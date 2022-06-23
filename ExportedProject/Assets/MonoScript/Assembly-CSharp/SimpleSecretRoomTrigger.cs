using Dungeonator;
using UnityEngine;

public class SimpleSecretRoomTrigger : BraveBehaviour, IPlayerInteractable
{
	public SecretRoomManager referencedSecretRoom;

	public RoomHandler parentRoom;

	public void Initialize()
	{
		parentRoom.RegisterInteractable(this);
	}

	private void HandleTrigger(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody)
	{
		if (!referencedSecretRoom.IsOpen && specRigidbody.projectile != null)
		{
			parentRoom.DeregisterInteractable(this);
			if (base.spriteAnimator != null)
			{
				base.spriteAnimator.Play();
			}
			referencedSecretRoom.OpenDoor();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		return Vector2.Distance(point, base.sprite.WorldCenter);
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if (!referencedSecretRoom.IsOpen && (bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
		}
	}

	public void Disable()
	{
		parentRoom.DeregisterInteractable(this);
	}

	public void Interact(PlayerController interactor)
	{
		parentRoom.DeregisterInteractable(this);
		if (!referencedSecretRoom.IsOpen)
		{
			if (base.spriteAnimator != null)
			{
				base.spriteAnimator.Play();
			}
			referencedSecretRoom.OpenDoor();
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
