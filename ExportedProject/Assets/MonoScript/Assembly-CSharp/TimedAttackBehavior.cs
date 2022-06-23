using UnityEngine;

public class TimedAttackBehavior : BasicAttackBehavior
{
	public float Duration;

	public BasicAttackBehavior AttackBehavior;

	private BehaviorResult m_defaultBehaviorResult;

	private bool m_runChildContinuous;

	private float m_runTimer;

	public override void Start()
	{
		base.Start();
		AttackBehavior.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_runTimer);
		AttackBehavior.Upkeep();
	}

	public override bool OverrideOtherBehaviors()
	{
		return AttackBehavior.OverrideOtherBehaviors();
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
		behaviorResult = AttackBehavior.Update();
		switch (behaviorResult)
		{
		case BehaviorResult.Continue:
			return behaviorResult;
		case BehaviorResult.SkipRemainingClassBehaviors:
		case BehaviorResult.RunContinuousInClass:
			m_defaultBehaviorResult = BehaviorResult.RunContinuousInClass;
			break;
		}
		if (behaviorResult == BehaviorResult.RunContinuous || behaviorResult == BehaviorResult.SkipAllRemainingBehaviors)
		{
			m_defaultBehaviorResult = BehaviorResult.RunContinuous;
		}
		m_runChildContinuous = behaviorResult == BehaviorResult.RunContinuous || behaviorResult == BehaviorResult.RunContinuousInClass;
		m_runTimer = Duration;
		return m_defaultBehaviorResult;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (!m_runChildContinuous)
		{
			if (m_runTimer <= 0f)
			{
				return ContinuousBehaviorResult.Finished;
			}
			BehaviorResult behaviorResult = AttackBehavior.Update();
			m_runChildContinuous = behaviorResult == BehaviorResult.RunContinuous || behaviorResult == BehaviorResult.RunContinuousInClass;
			return ContinuousBehaviorResult.Continue;
		}
		ContinuousBehaviorResult continuousBehaviorResult = AttackBehavior.ContinuousUpdate();
		if (continuousBehaviorResult == ContinuousBehaviorResult.Finished)
		{
			AttackBehavior.EndContinuousUpdate();
			m_runChildContinuous = false;
		}
		return (m_runTimer <= 0f) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (m_runChildContinuous)
		{
			AttackBehavior.EndContinuousUpdate();
			m_runChildContinuous = false;
		}
		UpdateCooldowns();
	}

	public override void Destroy()
	{
		AttackBehavior.Destroy();
		base.Destroy();
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		AttackBehavior.Init(gameObject, aiActor, aiShooter);
	}

	public override void SetDeltaTime(float deltaTime)
	{
		base.SetDeltaTime(deltaTime);
		AttackBehavior.SetDeltaTime(deltaTime);
	}

	public override bool IsReady()
	{
		return base.IsReady() && AttackBehavior.IsReady();
	}

	public override bool UpdateEveryFrame()
	{
		return AttackBehavior.UpdateEveryFrame();
	}

	public override bool IsOverridable()
	{
		return AttackBehavior.IsOverridable();
	}
}
