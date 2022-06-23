using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Beholster/LaserBehavior")]
public class BeholsterLaserBehavior : BasicAttackBehavior
{
	public enum State
	{
		PreCharging,
		Charging,
		Firing
	}

	public enum TrackingType
	{
		Follow,
		ConstantTurn
	}

	public TrackingType trackingType;

	public float initialAimOffset;

	public float chargeTime;

	public float firingTime;

	public float maxTurnRate;

	public float turnRateAcceleration;

	public bool useDegreeCatchUp;

	[InspectorShowIf("useDegreeCatchUp")]
	[InspectorIndent]
	public float minDegreesForCatchUp;

	[InspectorIndent]
	[InspectorShowIf("useDegreeCatchUp")]
	public float degreeCatchUpSpeed;

	public bool useUnitCatchUp;

	[InspectorIndent]
	[InspectorShowIf("useUnitCatchUp")]
	public float minUnitForCatchUp;

	[InspectorIndent]
	[InspectorShowIf("useUnitCatchUp")]
	public float maxUnitForCatchUp;

	[InspectorShowIf("useUnitCatchUp")]
	[InspectorIndent]
	public float unitCatchUpSpeed;

	public bool useUnitOvershoot;

	[InspectorIndent]
	[InspectorShowIf("useUnitOvershoot")]
	public float minUnitForOvershoot;

	[InspectorShowIf("useUnitOvershoot")]
	[InspectorIndent]
	public float unitOvershootTime;

	[InspectorShowIf("useUnitOvershoot")]
	[InspectorIndent]
	public float unitOvershootSpeed;

	private BeholsterController m_beholsterController;

	private State m_state;

	private float m_timer;

	private Vector2 m_targetPosition;

	private float m_currentUnitTurnRate;

	private float m_unitOvershootFixedDirection;

	private float m_unitOvershootTimer;

	private SpeculativeRigidbody m_backupTarget;

	public override void Start()
	{
		base.Start();
		m_beholsterController = m_aiActor.GetComponent<BeholsterController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if ((bool)m_aiActor.TargetRigidbody)
		{
			m_targetPosition = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			m_backupTarget = m_aiActor.TargetRigidbody;
		}
		else if ((bool)m_backupTarget)
		{
			m_targetPosition = m_backupTarget.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_aiActor.ClearPath();
		m_beholsterController.StopFiringTentacles();
		m_beholsterController.PrechargeFiringLaser();
		m_state = State.PreCharging;
		m_aiActor.SuppressTargetSwitch = true;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		Vector2 vector = m_aiActor.transform.position.XY() + m_beholsterController.firingEllipseCenter;
		if (m_state == State.PreCharging)
		{
			if (!m_aiActor.spriteAnimator.Playing)
			{
				m_beholsterController.ChargeFiringLaser(chargeTime);
				m_timer = chargeTime;
				m_state = State.Charging;
			}
		}
		else
		{
			if (m_state == State.Charging)
			{
				m_timer -= m_deltaTime;
				if (m_timer <= 0f)
				{
					float facingDirection = m_aiActor.aiAnimator.FacingDirection;
					m_beholsterController.StartFiringLaser(facingDirection);
					m_timer = firingTime;
					m_state = State.Firing;
				}
				return ContinuousBehaviorResult.Continue;
			}
			if (m_state == State.Firing)
			{
				m_timer -= m_deltaTime;
				if (m_timer <= 0f || !m_beholsterController.FiringLaser)
				{
					return ContinuousBehaviorResult.Finished;
				}
				float laserAngle;
				if (trackingType == TrackingType.Follow)
				{
					float num = Vector2.Distance(m_targetPosition, vector);
					float num2 = (m_targetPosition - vector).ToAngle();
					float num3 = BraveMathCollege.ClampAngle180(num2 - m_beholsterController.LaserAngle);
					float f = num3 * num * ((float)Math.PI / 180f);
					float num4 = maxTurnRate;
					float num5 = Mathf.Sign(num3);
					if (m_unitOvershootTimer > 0f)
					{
						num5 = m_unitOvershootFixedDirection;
						m_unitOvershootTimer -= m_deltaTime;
						num4 = unitOvershootSpeed;
					}
					m_currentUnitTurnRate = Mathf.Clamp(m_currentUnitTurnRate + num5 * turnRateAcceleration * m_deltaTime, 0f - num4, num4);
					float num6 = m_currentUnitTurnRate / num * 57.29578f;
					float num7 = 0f;
					if (useDegreeCatchUp && Mathf.Abs(num3) > minDegreesForCatchUp)
					{
						float b = Mathf.InverseLerp(minDegreesForCatchUp, 180f, Mathf.Abs(num3)) * degreeCatchUpSpeed;
						num7 = Mathf.Max(num7, b);
					}
					if (useUnitCatchUp && Mathf.Abs(f) > minUnitForCatchUp)
					{
						float num8 = Mathf.InverseLerp(minUnitForCatchUp, maxUnitForCatchUp, Mathf.Abs(f)) * unitCatchUpSpeed;
						float b2 = num8 / num * 57.29578f;
						num7 = Mathf.Max(num7, b2);
					}
					if (useUnitOvershoot && Mathf.Abs(f) < minUnitForOvershoot)
					{
						m_unitOvershootFixedDirection = ((m_currentUnitTurnRate > 0f) ? 1 : (-1));
						m_unitOvershootTimer = unitOvershootTime;
					}
					num7 *= Mathf.Sign(num3);
					laserAngle = BraveMathCollege.ClampAngle360(m_beholsterController.LaserAngle + (num6 + num7) * m_deltaTime);
				}
				else
				{
					laserAngle = BraveMathCollege.ClampAngle360(m_beholsterController.LaserAngle + maxTurnRate * m_deltaTime);
				}
				if ((bool)m_beholsterController.LaserBeam && m_beholsterController.LaserBeam.State != 0)
				{
					m_beholsterController.LaserAngle = laserAngle;
				}
				return ContinuousBehaviorResult.Continue;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_beholsterController.StopFiringLaser();
		m_aiAnimator.LockFacingDirection = false;
		m_aiActor.SuppressTargetSwitch = false;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}
}
