using System.Collections.Generic;
using FullInspector;
using Pathfinding;
using UnityEngine;

[InspectorDropdownName("Bosses/TankTreader/SeekTargetBehavior")]
public class TankTreaderSeekTargetBehavior : RangedMovementBehavior
{
	private enum State
	{
		Idle,
		PathingToTarget
	}

	public bool StopWhenInRange = true;

	public float CustomRange = -1f;

	[InspectorShowIf("StopWhenInRange")]
	public bool LineOfSight = true;

	public float PathInterval = 0.25f;

	public float turnSpeed = 120f;

	private float m_repathTimer;

	private IntVector2 m_startStep;

	private float m_desiredFacingDirection = -90f;

	private State m_state;

	public override void Start()
	{
		base.Start();
		m_updateEveryFrame = true;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_aiAnimator.LockFacingDirection = true;
	}

	public override BehaviorResult Update()
	{
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		m_aiAnimator.FacingDirection = Mathf.MoveTowardsAngle(m_aiAnimator.FacingDirection, m_desiredFacingDirection, turnSpeed * m_deltaTime);
		if (InRange() && (bool)targetRigidbody)
		{
			bool hasLineOfSightToTarget = m_aiActor.HasLineOfSightToTarget;
			float num = ((!(CustomRange >= 0f)) ? m_aiActor.DesiredCombatDistance : CustomRange);
			m_state = State.PathingToTarget;
			if (StopWhenInRange && m_aiActor.DistanceToTarget <= num && (!LineOfSight || hasLineOfSightToTarget))
			{
				m_aiActor.ClearPath();
				m_aiActor.BehaviorVelocity = Vector2.zero;
				return BehaviorResult.Continue;
			}
			if (m_repathTimer <= 0f)
			{
				m_startStep = ((!(m_aiActor.specRigidbody.Velocity.magnitude > 0.01f)) ? IntVector2.Zero : BraveUtility.GetIntMajorAxis(m_aiActor.specRigidbody.Velocity));
				m_aiActor.PathfindToPosition(targetRigidbody.UnitCenter, null, false, null, WeightDoer);
				m_repathTimer = PathInterval;
				SimplifyPath();
			}
			UpdateVelocity();
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		if (m_state == State.PathingToTarget)
		{
			m_aiActor.ClearPath();
			m_state = State.Idle;
		}
		return BehaviorResult.Continue;
	}

	private void SimplifyPath()
	{
		Path path = m_aiActor.Path;
		if (path == null || path.Count < 2)
		{
			return;
		}
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 firstCenterVector = m_aiActor.Path.GetFirstCenterVector2();
		Vector2 secondCenterVector = m_aiActor.Path.GetSecondCenterVector2();
		float num = (firstCenterVector - unitCenter).ToAngle();
		float num2 = (secondCenterVector - unitCenter).ToAngle();
		float num3 = BraveMathCollege.ClampAngle360(num - num2);
		if (num3 > 179f && num3 < 181f)
		{
			path.Positions.RemoveFirst();
		}
		if (path.Count < 2)
		{
			return;
		}
		LinkedListNode<IntVector2> next = path.Positions.First.Next;
		IntVector2 intVector = next.Value - next.Previous.Value;
		while (next != null && next.Next != null)
		{
			IntVector2 intVector2 = next.Next.Value - next.Value;
			if (intVector == intVector2)
			{
				next = next.Next;
				path.Positions.Remove(next.Previous);
			}
			else
			{
				intVector = intVector2;
				next = next.Next;
			}
		}
	}

	private int WeightDoer(IntVector2 prevStep, IntVector2 nextStep)
	{
		if (prevStep == IntVector2.Zero)
		{
			if (m_startStep == IntVector2.Zero)
			{
				return 0;
			}
			prevStep = m_startStep;
		}
		return (prevStep != nextStep) ? 10 : 0;
	}

	private void UpdateVelocity()
	{
		bool willReachGoal;
		Vector2 totalDistToMove;
		Vector2 vector = GetPathVelocityContribution(out willReachGoal, out totalDistToMove);
		Vector2 vector2 = vector;
		if (Mathf.Abs(totalDistToMove.x) < PhysicsEngine.PixelToUnit(2))
		{
			vector2.x = 0f;
		}
		if (Mathf.Abs(totalDistToMove.y) < PhysicsEngine.PixelToUnit(2))
		{
			vector2.y = 0f;
		}
		if (vector2.magnitude > 0.01f)
		{
			float num = vector.ToAngle();
			float f = BraveMathCollege.ClampAngle180(m_aiAnimator.FacingDirection - num);
			if (Mathf.Abs(f) > 0.5f && Mathf.Abs(f) < 179.5f)
			{
				vector = Vector2.zero;
				if (BraveMathCollege.AbsAngleBetween(m_aiAnimator.FacingDirection, num) <= 100f)
				{
					m_desiredFacingDirection = num;
				}
				else
				{
					m_desiredFacingDirection = BraveMathCollege.ClampAngle360(num + 180f);
				}
			}
		}
		m_aiActor.BehaviorVelocity = vector;
		if (willReachGoal)
		{
			m_aiActor.Path.RemoveFirst();
		}
	}

	private Vector2 GetPathVelocityContribution(out bool willReachGoal, out Vector2 totalDistToMove)
	{
		willReachGoal = false;
		totalDistToMove = Vector2.zero;
		if (m_aiActor.Path == null || m_aiActor.Path.Count == 0)
		{
			return Vector2.zero;
		}
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		Vector2 firstCenterVector = m_aiActor.Path.GetFirstCenterVector2();
		totalDistToMove = firstCenterVector - unitCenter;
		float num = m_aiActor.MovementSpeed * m_aiActor.LocalDeltaTime;
		if (num > totalDistToMove.magnitude)
		{
			willReachGoal = true;
			return totalDistToMove / m_aiActor.LocalDeltaTime;
		}
		return m_aiActor.MovementSpeed * totalDistToMove.normalized;
	}
}
