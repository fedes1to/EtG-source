using UnityEngine;

public class CopFreezeTargetBehavior : BasicAttackBehavior
{
	public float FreezeRadius = 7f;

	public float FreezeDelayTime = 2f;

	public GameActorFreezeEffect FreezeEffect;

	private float m_freezeTimer;

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
		if (m_aiActor.TargetRigidbody == null)
		{
			return BehaviorResult.Continue;
		}
		float num = Vector2.Distance(m_aiActor.CenterPosition, m_aiActor.TargetRigidbody.UnitCenter);
		if (num > FreezeRadius)
		{
			return BehaviorResult.Continue;
		}
		if (num < 2f)
		{
			return BehaviorResult.Continue;
		}
		DoFreeze();
		m_aiAnimator.FacingDirection = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
		m_aiAnimator.LockFacingDirection = true;
		if (!m_aiAnimator.IsPlaying("freeze"))
		{
			m_aiAnimator.PlayUntilCancelled("freeze");
		}
		m_aiActor.ClearPath();
		m_freezeTimer = FreezeDelayTime;
		return BehaviorResult.RunContinuous;
	}

	private void DoFreeze()
	{
		if ((bool)m_aiActor.TargetRigidbody.aiActor && !m_aiActor.TargetRigidbody.aiActor.IsFrozen)
		{
			Debug.Log("DOING COP FREEZE");
			m_aiActor.TargetRigidbody.aiActor.ApplyEffect(FreezeEffect);
		}
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		DecrementTimer(ref m_freezeTimer);
		if (m_freezeTimer <= 0f || m_aiActor.TargetRigidbody == null)
		{
			m_aiAnimator.EndAnimationIf("freeze");
			return ContinuousBehaviorResult.Finished;
		}
		DoFreeze();
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if ((bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunRenderers(true, "ShootBulletScript");
		}
		m_aiAnimator.LockFacingDirection = false;
		m_aiAnimator.EndAnimation();
		UpdateCooldowns();
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
	}
}
