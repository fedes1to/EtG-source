using UnityEngine;

public class TetherBehavior : MovementBehaviorBase
{
	private enum State
	{
		Idle,
		PathingToTarget,
		ReturningToSpawn
	}

	public float KnockbackInvulnerabilityDelay = 0.5f;

	public float PathInterval = 0.25f;

	private float m_repathTimer;

	private float m_preventKnockbackTimer;

	private Vector2 m_tetherPosition;

	private State m_state;

	public override float DesiredCombatDistance
	{
		get
		{
			return 0f;
		}
	}

	public override void Start()
	{
		base.Start();
		m_tetherPosition = m_aiActor.specRigidbody.UnitCenter;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	public override BehaviorResult Update()
	{
		if (m_state == State.Idle)
		{
			if (Vector2.Distance(m_tetherPosition, m_aiActor.specRigidbody.UnitCenter) > 0.1f)
			{
				m_state = State.ReturningToSpawn;
				m_aiActor.PathfindToPosition(m_tetherPosition, m_tetherPosition);
				m_repathTimer = PathInterval;
				m_preventKnockbackTimer = KnockbackInvulnerabilityDelay;
			}
		}
		else if (m_state == State.ReturningToSpawn)
		{
			if (m_preventKnockbackTimer > 0f)
			{
				m_preventKnockbackTimer -= m_deltaTime;
				if (m_preventKnockbackTimer <= 0f)
				{
					m_aiActor.knockbackDoer.SetImmobile(true, "TetherBehavior");
				}
			}
			if (m_aiActor.PathComplete)
			{
				m_state = State.Idle;
				m_aiActor.knockbackDoer.SetImmobile(false, "TetherBehavior");
			}
			else if (m_repathTimer <= 0f)
			{
				m_aiActor.PathfindToPosition(m_tetherPosition, m_tetherPosition);
				m_repathTimer = PathInterval;
			}
		}
		return BehaviorResult.Continue;
	}
}
