using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/NegativePlatformingBehavior")]
public class InfinilichNegativePlatformingBehavior : BasicAttackBehavior
{
	private enum SpinState
	{
		None,
		ArmsIn,
		GoToStartPoint,
		BulletScript,
		ArmsOut
	}

	public float SetupTime = 1f;

	public float FlightSpeed = 6f;

	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	private SpinState m_state;

	private Vector2 m_startPoint;

	private Vector2 m_targetPoint;

	private float m_setupTime;

	private float m_setupTimer;

	private BulletScriptSource m_bulletSource;

	private SpinState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
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
		State = SpinState.ArmsIn;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == SpinState.ArmsIn)
		{
			if (!m_aiAnimator.IsPlaying("arms_in"))
			{
				m_startPoint = m_aiActor.specRigidbody.UnitCenter;
				m_targetPoint = m_aiActor.ParentRoom.area.Center + new Vector2(0f, -1.5f);
				float magnitude = (m_targetPoint - m_startPoint).magnitude;
				m_setupTime = Mathf.Min(SetupTime, 1.5f * magnitude / FlightSpeed);
				State = SpinState.GoToStartPoint;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (State == SpinState.GoToStartPoint)
		{
			Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
			Vector2 vector = Vector2Extensions.SmoothStep(m_startPoint, m_targetPoint, m_setupTimer / m_setupTime);
			if (m_setupTimer > m_setupTime)
			{
				m_aiActor.BehaviorVelocity = Vector2.zero;
				State = SpinState.BulletScript;
				return ContinuousBehaviorResult.Continue;
			}
			m_aiActor.BehaviorVelocity = (vector - unitCenter) / BraveTime.DeltaTime;
			m_setupTimer += m_deltaTime;
		}
		else if (State == SpinState.BulletScript)
		{
			if (m_bulletSource.IsEnded)
			{
				State = SpinState.ArmsOut;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (State == SpinState.ArmsOut && !m_aiAnimator.IsPlaying("arms_out"))
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		State = SpinState.None;
		m_aiActor.BehaviorOverridesVelocity = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	private void Fire()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = BulletScript;
		m_bulletSource.Initialize();
	}

	private void BeginState(SpinState state)
	{
		switch (state)
		{
		case SpinState.ArmsIn:
			m_aiActor.ClearPath();
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = Vector2.zero;
			m_aiAnimator.PlayUntilCancelled("arms_in");
			break;
		case SpinState.GoToStartPoint:
			m_aiAnimator.PlayUntilCancelled("spin");
			m_setupTimer = 0f;
			break;
		case SpinState.BulletScript:
			Fire();
			break;
		case SpinState.ArmsOut:
			m_aiAnimator.PlayUntilFinished("arms_out");
			break;
		}
	}

	private void EndState(SpinState state)
	{
		if (state == SpinState.BulletScript && (bool)m_bulletSource && !m_bulletSource.IsEnded)
		{
			m_bulletSource.ForceStop();
		}
	}
}
