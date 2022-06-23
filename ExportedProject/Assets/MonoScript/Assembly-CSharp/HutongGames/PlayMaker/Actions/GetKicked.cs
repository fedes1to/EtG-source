using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Kickable corpse.")]
	[ActionCategory(".Brave")]
	public class GetKicked : FsmStateAction
	{
		public FsmOwnerDefault GameObject;

		private int kickCount = 1;

		private bool m_hasInitializedSRB;

		private bool m_isFalling;

		public override void Reset()
		{
			base.Reset();
		}

		public override void Awake()
		{
			base.Awake();
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(GameObject);
			if ((bool)ownerDefaultTarget)
			{
				SpeculativeRigidbody component = ownerDefaultTarget.GetComponent<SpeculativeRigidbody>();
				if ((bool)component && !m_hasInitializedSRB)
				{
					m_hasInitializedSRB = true;
					component.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(component.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(HandlePostRigidbodyMotion));
					component.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(component.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
					component.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(component.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(HandleTileCollision));
				}
			}
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(GameObject);
			TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
			tk2dSpriteAnimator component2 = ownerDefaultTarget.GetComponent<tk2dSpriteAnimator>();
			AIAnimator component3 = ownerDefaultTarget.GetComponent<AIAnimator>();
			PlayerController playerController = ((!component.TalkingPlayer) ? GameManager.Instance.PrimaryPlayer : component.TalkingPlayer);
			if ((bool)component3)
			{
				component3.PlayUntilCancelled("kick" + kickCount, true);
				kickCount = kickCount % 8 + 1;
				if ((bool)component3.specRigidbody && (bool)playerController)
				{
					SpeculativeRigidbody specRigidbody = component3.specRigidbody;
					if (!m_hasInitializedSRB)
					{
						m_hasInitializedSRB = true;
						specRigidbody.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(specRigidbody.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(HandlePostRigidbodyMotion));
						specRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
						specRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(specRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(HandleTileCollision));
					}
					specRigidbody.Velocity += (specRigidbody.UnitCenter - playerController.CenterPosition).normalized * 3f;
					SpawnManager.SpawnVFX((GameObject)BraveResources.Load("Global VFX/VFX_DodgeRollHit"), specRigidbody.UnitCenter, Quaternion.identity, true);
				}
			}
			if ((bool)playerController)
			{
				SetAnimationState(playerController, component);
			}
			Finish();
		}

		private void HandleTileCollision(CollisionData tileCollision)
		{
			Vector2 velocity = tileCollision.MyRigidbody.Velocity;
			float num = (-velocity).ToAngle();
			float num2 = tileCollision.Normal.ToAngle();
			float angle = BraveMathCollege.ClampAngle360(num + 2f * (num2 - num));
			PhysicsEngine.PostSliceVelocity = BraveMathCollege.DegreesToVector(angle).normalized * velocity.magnitude;
		}

		private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
		{
			if ((bool)rigidbodyCollision.OtherRigidbody && (bool)rigidbodyCollision.OtherRigidbody.projectile)
			{
				Vector2 normalized = rigidbodyCollision.OtherRigidbody.projectile.LastVelocity.normalized;
				rigidbodyCollision.MyRigidbody.Velocity += normalized * 1.5f;
				AIAnimator aiAnimator = rigidbodyCollision.MyRigidbody.aiAnimator;
				if ((bool)aiAnimator && aiAnimator.CurrentClipProgress >= 1f)
				{
					aiAnimator.PlayUntilCancelled("kick" + kickCount, true);
					kickCount = kickCount % 8 + 1;
				}
			}
			else
			{
				Vector2 velocity = rigidbodyCollision.MyRigidbody.Velocity;
				float num = (-velocity).ToAngle();
				float num2 = rigidbodyCollision.Normal.ToAngle();
				float angle = BraveMathCollege.ClampAngle360(num + 2f * (num2 - num));
				PhysicsEngine.PostSliceVelocity = BraveMathCollege.DegreesToVector(angle).normalized * velocity.magnitude;
			}
		}

		private void HandlePostRigidbodyMotion(SpeculativeRigidbody arg1, Vector2 arg2, IntVector2 arg3)
		{
			arg1.Velocity = Vector2.MoveTowards(arg1.Velocity, Vector2.zero, 5f * BraveTime.DeltaTime);
			if (!m_isFalling && GameManager.Instance.Dungeon.ShouldReallyFall(arg1.UnitTopLeft) && GameManager.Instance.Dungeon.ShouldReallyFall(arg1.UnitTopRight) && GameManager.Instance.Dungeon.ShouldReallyFall(arg1.UnitBottomLeft) && GameManager.Instance.Dungeon.ShouldReallyFall(arg1.UnitBottomRight))
			{
				GameManager.Instance.Dungeon.StartCoroutine(HandlePitfall(arg1));
			}
		}

		private IEnumerator HandlePitfall(SpeculativeRigidbody srb)
		{
			m_isFalling = true;
			RoomHandler firstRoom = srb.UnitCenter.GetAbsoluteRoom();
			TalkDoerLite talkdoer = srb.GetComponent<TalkDoerLite>();
			firstRoom.DeregisterInteractable(talkdoer);
			srb.Velocity = srb.Velocity.normalized * 0.125f;
			AIAnimator anim = srb.GetComponent<AIAnimator>();
			anim.PlayUntilFinished("pitfall");
			while (anim.IsPlaying("pitfall"))
			{
				yield return null;
			}
			anim.PlayUntilCancelled("kick1");
			srb.Velocity = Vector2.zero;
			RoomHandler targetRoom = firstRoom.TargetPitfallRoom;
			Transform[] childTransforms = targetRoom.hierarchyParent.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < childTransforms.Length; i++)
			{
				if (childTransforms[i].name == "Arrival")
				{
					srb.transform.position = childTransforms[i].position + new Vector3(1f, 1f, 0f);
					srb.Reinitialize();
					RoomHandler.unassignedInteractableObjects.Add(talkdoer);
					break;
				}
			}
		}

		public void SetAnimationState(PlayerController interactor, TalkDoerLite owner)
		{
			bool flag = false;
			string animationName = "tablekick_up";
			Vector2 inVec = interactor.CenterPosition - owner.specRigidbody.UnitCenter;
			switch (BraveMathCollege.VectorToQuadrant(inVec))
			{
			case 0:
				animationName = "tablekick_down";
				break;
			case 1:
				flag = true;
				animationName = "tablekick_right";
				break;
			case 2:
				animationName = "tablekick_up";
				break;
			case 3:
				animationName = "tablekick_right";
				break;
			}
			interactor.QueueSpecificAnimation(animationName);
		}
	}
}
