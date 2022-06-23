using UnityEngine;

public class WaitThenChargeBehavior : MovementBehaviorBase
{
	public float Delay;

	private float m_delayTimer;

	private bool m_isCharging;

	private float m_chargeDirection;

	private Vector2 m_center;

	public override void Start()
	{
		base.Start();
		m_delayTimer = Delay;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_delayTimer);
	}

	public override BehaviorResult Update()
	{
		if (m_isCharging)
		{
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_chargeDirection, m_aiActor.MovementSpeed);
		}
		else if (m_delayTimer <= 0f)
		{
			m_isCharging = true;
			if ((bool)m_aiActor.TargetRigidbody)
			{
				m_chargeDirection = (m_aiActor.behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
			}
			else
			{
				m_chargeDirection = Random.Range(0f, 360f);
			}
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = BraveMathCollege.DegreesToVector(m_chargeDirection, m_aiActor.MovementSpeed);
		}
		return BehaviorResult.SkipRemainingClassBehaviors;
	}
}
