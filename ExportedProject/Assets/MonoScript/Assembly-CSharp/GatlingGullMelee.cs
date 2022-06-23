using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/Melee")]
public class GatlingGullMelee : BasicAttackBehavior
{
	public float TriggerDistance = 4f;

	public float Damage = 1f;

	public float KnockbackForce = 30f;

	public GameObject CenterPoint;

	public float DamageDistance;

	public float StickyFriction = 0.1f;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		if ((m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.specRigidbody.UnitCenter).magnitude < TriggerDistance)
		{
			m_aiAnimator.PlayUntilFinished("melee", true);
			m_aiActor.ClearPath();
			tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
			return BehaviorResult.RunContinuous;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_aiActor.spriteAnimator.IsPlaying("melee"))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		UpdateCooldowns();
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (!(frame.eventInfo == "melee_hit"))
		{
			return;
		}
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		if (!targetRigidbody)
		{
			return;
		}
		Vector2 vector = targetRigidbody.GetUnitCenter(ColliderType.HitBox) - CenterPoint.transform.position.XY();
		if (!(vector.magnitude < DamageDistance))
		{
			return;
		}
		PlayerController playerController = ((!targetRigidbody.gameActor) ? null : (targetRigidbody.gameActor as PlayerController));
		if ((bool)targetRigidbody.healthHaver && targetRigidbody.healthHaver.IsVulnerable && (!playerController || !playerController.IsEthereal))
		{
			targetRigidbody.healthHaver.ApplyDamage(Damage, vector.normalized, m_aiActor.GetActorName());
			if ((bool)targetRigidbody.knockbackDoer)
			{
				targetRigidbody.knockbackDoer.ApplyKnockback(vector.normalized, KnockbackForce);
			}
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(StickyFriction, 0f, false);
		}
		if ((bool)targetRigidbody.majorBreakable)
		{
			targetRigidbody.majorBreakable.ApplyDamage(1000f, vector.normalized, true);
		}
	}
}
