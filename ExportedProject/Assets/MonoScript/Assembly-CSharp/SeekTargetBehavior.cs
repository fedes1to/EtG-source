using System;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class SeekTargetBehavior : RangedMovementBehavior
{
	private enum State
	{
		Idle,
		PathingToTarget,
		ReturningToSpawn
	}

	public bool StopWhenInRange = true;

	public float CustomRange = -1f;

	[InspectorShowIf("StopWhenInRange")]
	public bool LineOfSight = true;

	public bool ReturnToSpawn = true;

	public float SpawnTetherDistance;

	public float PathInterval = 0.25f;

	[NonSerialized]
	public bool ExternalCooldownSource;

	private float m_repathTimer;

	private State m_state;

	public override float DesiredCombatDistance
	{
		get
		{
			return CustomRange;
		}
	}

	public override bool AllowFearRunState
	{
		get
		{
			return true;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
		if (InRange() && (bool)targetRigidbody)
		{
			bool flag = m_aiActor.HasLineOfSightToTarget;
			float desiredCombatDistance = m_aiActor.DesiredCombatDistance;
			m_state = State.PathingToTarget;
			if ((bool)m_aiActor.TargetRigidbody && (bool)m_aiActor.TargetRigidbody.aiActor && !m_aiActor.TargetRigidbody.CollideWithOthers)
			{
				flag = true;
			}
			if (ExternalCooldownSource)
			{
				m_aiActor.ClearPath();
				return BehaviorResult.Continue;
			}
			if (StopWhenInRange && m_aiActor.DistanceToTarget <= desiredCombatDistance && (!LineOfSight || flag))
			{
				m_aiActor.ClearPath();
				return BehaviorResult.Continue;
			}
			if (m_repathTimer <= 0f)
			{
				CellValidator cellValidator = null;
				if (SpawnTetherDistance > 0f)
				{
					cellValidator = (IntVector2 p) => Vector2.Distance(p.ToCenterVector2(), m_aiActor.SpawnPosition) < SpawnTetherDistance;
				}
				Vector2 unitCenter = targetRigidbody.UnitCenter;
				AIActor aiActor = m_aiActor;
				Vector2 targetPosition = unitCenter;
				CellValidator cellValidator2 = cellValidator;
				aiActor.PathfindToPosition(targetPosition, null, true, cellValidator2);
				m_repathTimer = PathInterval;
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		if (m_state == State.PathingToTarget)
		{
			m_aiActor.ClearPath();
			m_state = State.Idle;
		}
		else if (m_state == State.Idle)
		{
			if (ReturnToSpawn && m_aiActor.GridPosition != m_aiActor.SpawnGridPosition && m_aiActor.PathComplete)
			{
				m_aiActor.PathfindToPosition(m_aiActor.SpawnPosition);
				m_state = State.ReturningToSpawn;
			}
		}
		else if (m_state == State.ReturningToSpawn && m_aiActor.PathComplete)
		{
			m_state = State.Idle;
		}
		return BehaviorResult.Continue;
	}
}
