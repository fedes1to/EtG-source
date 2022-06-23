using System;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class SmoothSeekTargetBehavior : RangedMovementBehavior
{
	public float turnTime = 1f;

	public float stoppedTurnMultiplier = 1f;

	public float targetTolerance;

	public bool slither;

	[InspectorShowIf("slither")]
	[InspectorIndent]
	public float slitherPeriod;

	[InspectorShowIf("slither")]
	[InspectorIndent]
	public float slitherMagnitude;

	public bool bob;

	[InspectorIndent]
	[InspectorShowIf("bob")]
	public float bobPeriod;

	[InspectorIndent]
	[InspectorShowIf("bob")]
	public float bobPeriodVariance;

	[InspectorIndent]
	[InspectorShowIf("bob")]
	public float bobMagnitude;

	public bool pathfind;

	[InspectorIndent]
	[InspectorShowIf("pathfind")]
	public float pathInterval = 0.25f;

	private Vector2 m_targetCenter;

	private float m_timer;

	private float m_pathTimer;

	private float m_direction = -90f;

	private float m_angularVelocity;

	private float m_slitherDirection;

	private float m_bobPeriod;

	private float m_lastBobOffset;

	private float m_timeSinceLastUpdate;

	public override void Start()
	{
		base.Start();
		m_updateEveryFrame = true;
		m_bobPeriod = bobPeriod + UnityEngine.Random.Range(0f - bobPeriodVariance, bobPeriodVariance);
		m_direction = -90f;
		if ((bool)m_aiAnimator)
		{
			m_aiAnimator.FacingDirection = -90f;
		}
		m_targetCenter = m_aiActor.specRigidbody.GetUnitCenter(ColliderType.Ground);
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_timer += m_deltaTime;
		m_timeSinceLastUpdate += m_deltaTime;
		DecrementTimer(ref m_pathTimer);
	}

	public override BehaviorResult Update()
	{
		if (m_timeSinceLastUpdate > 0.4f)
		{
			m_direction = m_aiAnimator.FacingDirection;
		}
		m_timeSinceLastUpdate = 0f;
		if (pathfind && !m_aiActor.HasLineOfSightToTarget)
		{
			if (m_pathTimer <= 0f)
			{
				UpdateTargetCenter();
				Path path = null;
				if (Pathfinder.Instance.GetPath(m_aiActor.PathTile, m_targetCenter.ToIntVector2(VectorConversions.Floor), out path, m_aiActor.Clearance, m_aiActor.PathableTiles) && path.Count > 0)
				{
					path.Smooth(m_aiActor.specRigidbody.UnitCenter, m_aiActor.specRigidbody.UnitDimensions / 2f, m_aiActor.PathableTiles, false, m_aiActor.Clearance);
					m_targetCenter = path.GetFirstCenterVector2();
				}
				m_pathTimer += pathInterval;
			}
		}
		else
		{
			UpdateTargetCenter();
		}
		float num = turnTime;
		if (stoppedTurnMultiplier != 0f && m_aiActor.specRigidbody.Velocity.magnitude < m_aiActor.MovementSpeed / 2f)
		{
			num *= stoppedTurnMultiplier;
		}
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		float num2 = (m_targetCenter - unitCenter).ToAngle();
		if (targetTolerance > 0f)
		{
			float f = Mathf.DeltaAngle(num2, m_direction);
			num2 = ((!(Mathf.Abs(f) < targetTolerance)) ? (num2 + Mathf.Sign(f) * targetTolerance) : m_direction);
		}
		m_direction = Mathf.SmoothDampAngle(m_direction, num2, ref m_angularVelocity, num);
		if (slither)
		{
			m_slitherDirection = Mathf.Sin(m_timer * (float)Math.PI / slitherPeriod) * slitherMagnitude;
		}
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_direction + m_slitherDirection, m_aiActor.MovementSpeed);
		if (bob)
		{
			float num3 = Mathf.Sin(m_timer * (float)Math.PI / m_bobPeriod) * bobMagnitude;
			if (m_deltaTime > 0f)
			{
				m_aiActor.BehaviorVelocity += new Vector2(0f, num3 - m_lastBobOffset) / m_deltaTime;
			}
			m_lastBobOffset = num3;
		}
		return BehaviorResult.Continue;
	}

	private void UpdateTargetCenter()
	{
		if ((bool)m_aiActor.TargetRigidbody)
		{
			m_targetCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
	}
}
