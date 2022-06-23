using FullInspector;

[InspectorDropdownName("Bosses/Beholster/ShootBehavior")]
public class BeholsterShootBehavior : BasicAttackBehavior
{
	private enum State
	{
		Ready,
		Windup,
		Firing
	}

	public bool LineOfSight = true;

	public float WindUpTime = 1f;

	public BulletScriptSelector BulletScript;

	public BeholsterTentacleController Tentacle;

	private State m_state;

	private float m_windupTimer;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_windupTimer);
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
		bool flag = LineOfSight && !m_aiActor.HasLineOfSightToTarget;
		if (m_aiActor.TargetRigidbody == null || flag)
		{
			return BehaviorResult.Continue;
		}
		m_state = State.Windup;
		m_windupTimer = WindUpTime;
		m_aiActor.ClearPath();
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.Windup)
		{
			if (m_windupTimer <= 0f)
			{
				if ((bool)m_aiActor.TargetRigidbody)
				{
					m_aiActor.bulletBank.FixedPlayerPosition = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
				}
				m_aiAnimator.LockFacingDirection = true;
				Tentacle.BulletScriptSource.FreezeTopPosition = true;
				Tentacle.ShootBulletScript(BulletScript);
				m_state = State.Firing;
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (m_state == State.Firing && Tentacle.BulletScriptSource.IsEnded)
		{
			m_state = State.Ready;
			m_aiActor.bulletBank.FixedPlayerPosition = null;
			m_aiAnimator.LockFacingDirection = false;
			Tentacle.BulletScriptSource.FreezeTopPosition = false;
			UpdateCooldowns();
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}
}
