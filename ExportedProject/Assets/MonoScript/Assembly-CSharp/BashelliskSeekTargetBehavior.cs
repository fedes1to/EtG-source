using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Bashellisk/SeekTargetBehavior")]
public class BashelliskSeekTargetBehavior : RangedMovementBehavior
{
	private enum SeekState
	{
		SeekPlayer,
		ConsideringPickup,
		SeekPickup
	}

	public float turnTime = 1f;

	public bool slither;

	public float slitherPeriod;

	public float slitherMagnitude;

	public float minPickupDelay;

	public float maxPickupDelay;

	public float snapDist;

	public float snapTurnTime;

	public bool snapSlither;

	private SeekState m_state;

	private BashelliskHeadController m_head;

	private Vector2 m_targetCenter;

	private BashelliskBodyPickupController m_desiredPickup;

	private bool m_snapToTarget;

	private float m_slitherCounter;

	private float m_direction = -90f;

	private float m_slitherDirection;

	private float m_angularVelocity;

	private float m_pickupConsiderationTimer;

	private SeekState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	public override void Start()
	{
		base.Start();
		m_head = m_aiActor.GetComponent<BashelliskHeadController>();
		m_updateEveryFrame = true;
		if (TurboModeController.IsActive)
		{
			turnTime /= TurboModeController.sEnemyMovementSpeedMultiplier;
			snapTurnTime /= TurboModeController.sEnemyMovementSpeedMultiplier;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_aiActor.BehaviorOverridesVelocity = true;
		if (!m_head.IsMidPickup)
		{
			m_aiActor.BehaviorVelocity = Vector2.zero;
		}
		m_aiAnimator.LockFacingDirection = true;
		DecrementTimer(ref m_pickupConsiderationTimer);
		m_slitherCounter += m_deltaTime * m_aiActor.behaviorSpeculator.CooldownScale;
	}

	public override BehaviorResult Update()
	{
		UpdateState();
		if (m_head.IsMidPickup)
		{
			return BehaviorResult.Continue;
		}
		if (m_head.ReinitMovementDirection)
		{
			m_direction = m_head.aiAnimator.FacingDirection;
			m_head.ReinitMovementDirection = false;
		}
		Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
		float target = (m_targetCenter - unitCenter).ToAngle();
		m_direction = Mathf.SmoothDampAngle(m_direction, target, ref m_angularVelocity, (!m_snapToTarget) ? turnTime : snapTurnTime);
		if ((!m_snapToTarget) ? slither : snapSlither)
		{
			m_slitherDirection = Mathf.Sin(m_slitherCounter * (float)Math.PI / slitherPeriod) * slitherMagnitude;
		}
		m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_direction + m_slitherDirection, m_aiActor.MovementSpeed);
		return BehaviorResult.Continue;
	}

	private void BeginState(SeekState state)
	{
		switch (state)
		{
		case SeekState.ConsideringPickup:
			m_pickupConsiderationTimer = UnityEngine.Random.Range(minPickupDelay, maxPickupDelay);
			break;
		case SeekState.SeekPickup:
			m_head.CanPickup = true;
			m_desiredPickup = m_head.AvailablePickups.GetByIndexSlow(UnityEngine.Random.Range(0, m_head.AvailablePickups.Count)).Value;
			if ((bool)m_desiredPickup)
			{
				m_targetCenter = m_desiredPickup.specRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			break;
		}
	}

	private void UpdateState()
	{
		m_snapToTarget = false;
		if (State == SeekState.SeekPlayer)
		{
			if ((bool)m_aiActor.TargetRigidbody)
			{
				m_targetCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			if (m_head.AvailablePickups.Count > 0)
			{
				State = SeekState.ConsideringPickup;
				UpdateState();
			}
		}
		else if (State == SeekState.ConsideringPickup)
		{
			if (m_head.AvailablePickups.Count == 0)
			{
				State = SeekState.SeekPlayer;
				UpdateState();
			}
			else if (m_pickupConsiderationTimer <= 0f)
			{
				State = SeekState.SeekPickup;
				UpdateState();
			}
		}
		else
		{
			if (State != SeekState.SeekPickup)
			{
				return;
			}
			if ((bool)m_desiredPickup && m_desiredPickup.aiActor.CanTargetPlayers)
			{
				if (Vector2.Distance(m_aiActor.specRigidbody.UnitCenter, m_targetCenter) < snapDist)
				{
					m_snapToTarget = true;
				}
			}
			else
			{
				State = SeekState.SeekPlayer;
				UpdateState();
			}
		}
	}

	private void EndState(SeekState state)
	{
		if (state == SeekState.SeekPickup)
		{
			m_head.CanPickup = false;
		}
	}
}
