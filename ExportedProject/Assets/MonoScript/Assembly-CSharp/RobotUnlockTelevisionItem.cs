using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RobotUnlockTelevisionItem : PlayerItem
{
	public DodgeRollStats RollStats;

	protected PlayerController m_owner;

	protected PlayerController.DodgeRollState m_dodgeRollState;

	public override void Pickup(PlayerController player)
	{
		m_owner = player;
		player.OnRollStarted += HandleRoll;
		base.Pickup(player);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		user.OnRollStarted -= HandleRoll;
		m_owner = null;
		base.OnPreDrop(user);
	}

	private void HandleRoll(PlayerController arg1, Vector2 arg2)
	{
		DebrisObject debrisObject = m_owner.DropActiveItem(this, 0f);
		debrisObject.inertialMass = 1E+07f;
		AkSoundEngine.PostEvent("Play_OBJ_item_throw_01", GameManager.Instance.gameObject);
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			m_owner.OnRollStarted -= HandleRoll;
		}
		base.OnDestroy();
	}

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_OBJ_item_throw_01", GameManager.Instance.gameObject);
		DebrisObject debrisObject = m_owner.DropActiveItem(this, 7f);
		GameObject gameObject = debrisObject.gameObject;
		UnityEngine.Object.Destroy(debrisObject);
		SpeculativeRigidbody speculativeRigidbody = gameObject.AddComponent<SpeculativeRigidbody>();
		speculativeRigidbody.transform.position = user.specRigidbody.UnitBottomLeft.ToVector3ZisY();
		speculativeRigidbody.PixelColliders = new List<PixelCollider>();
		PixelCollider pixelCollider = new PixelCollider();
		pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
		pixelCollider.ManualOffsetX = 2;
		pixelCollider.ManualOffsetY = 3;
		pixelCollider.ManualWidth = 11;
		pixelCollider.ManualHeight = 10;
		pixelCollider.CollisionLayer = CollisionLayer.LowObstacle;
		pixelCollider.Enabled = true;
		pixelCollider.IsTrigger = false;
		speculativeRigidbody.PixelColliders.Add(pixelCollider);
		speculativeRigidbody.Reinitialize();
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(speculativeRigidbody);
		speculativeRigidbody.RegisterSpecificCollisionException(user.specRigidbody);
		user.StartCoroutine(HandleDodgeRoll(speculativeRigidbody, user.unadjustedAimPoint.XY() - user.sprite.WorldCenter));
	}

	private bool HandlePitfall(SpeculativeRigidbody targetRigidbody)
	{
		if (GameManager.Instance.Dungeon.ShouldReallyFall(targetRigidbody.UnitCenter))
		{
			DebrisObject debrisObject = targetRigidbody.gameObject.AddComponent<DebrisObject>();
			debrisObject.canRotate = false;
			debrisObject.Trigger(Vector3.zero, 0.01f);
			UnityEngine.Object.Destroy(targetRigidbody);
			return true;
		}
		return false;
	}

	private IEnumerator HandleDodgeRoll(SpeculativeRigidbody targetRigidbody, Vector2 direction)
	{
		float elapsed = 0f;
		targetRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Combine(targetRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(RollPitMovementRestrictor));
		bool hasGrounded = false;
		targetRigidbody.spriteAnimator.PlayAndForceTime(targetRigidbody.spriteAnimator.DefaultClip, RollStats.time);
		while (elapsed < RollStats.time)
		{
			elapsed += BraveTime.DeltaTime;
			float drSpeed = GetDodgeRollSpeed(elapsed, RollStats.speed, RollStats.time, RollStats.distance);
			targetRigidbody.Velocity = direction.normalized * drSpeed;
			m_dodgeRollState = ((!(elapsed > 0.39f)) ? PlayerController.DodgeRollState.InAir : PlayerController.DodgeRollState.OnGround);
			if (!hasGrounded && m_dodgeRollState == PlayerController.DodgeRollState.OnGround)
			{
				hasGrounded = true;
				GameManager.Instance.Dungeon.dungeonDustups.InstantiateLandDustup(targetRigidbody.UnitBottomCenter.ToVector3ZisY());
				OnGrounded(targetRigidbody);
			}
			if (m_dodgeRollState == PlayerController.DodgeRollState.OnGround && HandlePitfall(targetRigidbody))
			{
				yield break;
			}
			yield return null;
		}
		targetRigidbody.spriteAnimator.SetFrame(0);
		targetRigidbody.MovementRestrictor = (SpeculativeRigidbody.MovementRestrictorDelegate)Delegate.Remove(targetRigidbody.MovementRestrictor, new SpeculativeRigidbody.MovementRestrictorDelegate(RollPitMovementRestrictor));
		m_dodgeRollState = PlayerController.DodgeRollState.None;
		targetRigidbody.Velocity = Vector2.zero;
		UnityEngine.Object.Destroy(targetRigidbody);
	}

	private void OnGrounded(SpeculativeRigidbody targetRigidbody)
	{
		PlayerItem component = targetRigidbody.GetComponent<PlayerItem>();
		if (component != null)
		{
			component.ForceAsExtant = true;
			if (!RoomHandler.unassignedInteractableObjects.Contains(component))
			{
				RoomHandler.unassignedInteractableObjects.Add(component);
			}
		}
	}

	private float GetDodgeRollSpeed(float dodgeRollTimer, AnimationCurve speedCurve, float rollTime, float rollDistance)
	{
		float time = Mathf.Clamp01((dodgeRollTimer - BraveTime.DeltaTime) / rollTime);
		float time2 = Mathf.Clamp01(dodgeRollTimer / rollTime);
		float num = (Mathf.Clamp01(speedCurve.Evaluate(time2)) - Mathf.Clamp01(speedCurve.Evaluate(time))) * rollDistance;
		return num / BraveTime.DeltaTime;
	}

	private void RollPitMovementRestrictor(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation)
	{
		if (!validLocation || m_dodgeRollState != PlayerController.DodgeRollState.OnGround)
		{
			return;
		}
		Func<IntVector2, bool> func = delegate(IntVector2 pixel)
		{
			Vector2 vector = PhysicsEngine.PixelToUnitMidpoint(pixel);
			if (!GameManager.Instance.Dungeon.CellSupportsFalling(vector))
			{
				return false;
			}
			List<SpeculativeRigidbody> platformsAt = GameManager.Instance.Dungeon.GetPlatformsAt(vector);
			if (platformsAt != null)
			{
				for (int i = 0; i < platformsAt.Count; i++)
				{
					if (platformsAt[i].PrimaryPixelCollider.ContainsPixel(pixel))
					{
						return false;
					}
				}
			}
			IntVector2 intVector2 = vector.ToIntVector2(VectorConversions.Floor);
			return GameManager.Instance.Dungeon.data.isTopWall(intVector2.x, intVector2.y) || true;
		};
		PixelCollider primaryPixelCollider = specRigidbody.PrimaryPixelCollider;
		if (primaryPixelCollider != null)
		{
			IntVector2 intVector = pixelOffset - prevPixelOffset;
			if (intVector == IntVector2.Down && func(primaryPixelCollider.LowerLeft + pixelOffset) && func(primaryPixelCollider.LowerRight + pixelOffset) && (!func(primaryPixelCollider.UpperRight + prevPixelOffset) || !func(primaryPixelCollider.UpperLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Right && func(primaryPixelCollider.LowerRight + pixelOffset) && func(primaryPixelCollider.UpperRight + pixelOffset) && (!func(primaryPixelCollider.UpperLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerLeft + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Up && func(primaryPixelCollider.UpperRight + pixelOffset) && func(primaryPixelCollider.UpperLeft + pixelOffset) && (!func(primaryPixelCollider.LowerLeft + prevPixelOffset) || !func(primaryPixelCollider.LowerRight + prevPixelOffset)))
			{
				validLocation = false;
			}
			else if (intVector == IntVector2.Left && func(primaryPixelCollider.UpperLeft + pixelOffset) && func(primaryPixelCollider.LowerLeft + pixelOffset) && (!func(primaryPixelCollider.LowerRight + prevPixelOffset) || !func(primaryPixelCollider.UpperRight + prevPixelOffset)))
			{
				validLocation = false;
			}
		}
	}
}
