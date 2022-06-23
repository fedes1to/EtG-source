using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/SpinBeamsBehavior")]
public class InfinilichSpinBeamsBehavior : BasicAttackBehavior
{
	private enum SpinState
	{
		None,
		ArmsIn,
		GoToStartPoint,
		BeamMode,
		ArmsOut
	}

	public float SetupTime = 1f;

	public float FlightSpeed = 6f;

	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	private SpinState m_state;

	private Vector2 m_startPoint;

	private Vector2 m_targetPoint;

	private List<Vector2> m_futureTargets = new List<Vector2>();

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
				m_targetPoint = m_aiActor.ParentRoom.area.Center + new Vector2(0f, 11f);
				Vector2 vector = m_targetPoint - m_startPoint;
				float magnitude = vector.magnitude;
				m_setupTime = Mathf.Min(SetupTime, 1.5f * magnitude / FlightSpeed);
				m_aiAnimator.FacingDirection = vector.ToAngle();
				State = SpinState.GoToStartPoint;
				return ContinuousBehaviorResult.Continue;
			}
		}
		else if (State == SpinState.GoToStartPoint)
		{
			Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
			Vector2 vector2 = Vector2Extensions.SmoothStep(m_startPoint, m_targetPoint, m_setupTimer / m_setupTime);
			if (m_setupTimer > m_setupTime)
			{
				m_aiActor.BehaviorVelocity = Vector2.zero;
				m_targetPoint = m_aiActor.ParentRoom.area.Center - new Vector2(0f, 11f);
				m_futureTargets.Clear();
				m_futureTargets.Add(m_aiActor.ParentRoom.area.Center + new Vector2(0f, 11f));
				m_futureTargets.Add(m_aiActor.ParentRoom.area.Center);
				State = SpinState.BeamMode;
				return ContinuousBehaviorResult.Continue;
			}
			m_aiActor.BehaviorVelocity = (vector2 - unitCenter) / BraveTime.DeltaTime;
			m_aiAnimator.FacingDirection = m_aiActor.BehaviorVelocity.ToAngle();
			m_setupTimer += m_deltaTime;
		}
		else if (State == SpinState.BeamMode)
		{
			Vector2 vector3 = m_targetPoint - m_aiActor.specRigidbody.UnitCenter;
			float magnitude2 = vector3.magnitude;
			if (magnitude2 < 0.1f)
			{
				if (m_futureTargets.Count > 0)
				{
					m_targetPoint = m_futureTargets[0];
					m_futureTargets.RemoveAt(0);
					m_aiActor.BehaviorVelocity = Vector2.zero;
					return ContinuousBehaviorResult.Continue;
				}
				m_aiActor.BehaviorVelocity = Vector2.zero;
				State = SpinState.ArmsOut;
				return ContinuousBehaviorResult.Continue;
			}
			float num = FlightSpeed;
			if (magnitude2 < FlightSpeed * m_deltaTime)
			{
				num = magnitude2 / m_deltaTime;
			}
			m_aiActor.BehaviorVelocity = vector3.WithX(0f).normalized * num;
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
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.PlayUntilCancelled("spin");
			m_setupTimer = 0f;
			break;
		case SpinState.BeamMode:
			m_aiAnimator.FacingDirection = -90f;
			Fire();
			break;
		case SpinState.ArmsOut:
			m_aiAnimator.PlayUntilFinished("arms_out");
			break;
		}
	}

	private void EndState(SpinState state)
	{
		if (state == SpinState.BeamMode && (bool)m_bulletSource && !m_bulletSource.IsEnded)
		{
			m_bulletSource.ForceStop();
		}
	}
}
