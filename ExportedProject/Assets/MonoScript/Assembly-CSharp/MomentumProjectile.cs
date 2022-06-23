using UnityEngine;

public class MomentumProjectile : Projectile
{
	public float momentumFraction = 0.35f;

	public override void Start()
	{
		base.Start();
		if ((bool)base.Owner && (bool)base.Owner.specRigidbody)
		{
			m_currentDirection = m_currentDirection.normalized * (1f - momentumFraction) + base.Owner.specRigidbody.Velocity.normalized * momentumFraction;
			m_currentDirection = m_currentDirection.normalized;
		}
	}

	protected override void Move()
	{
		m_timeElapsed += base.LocalDeltaTime;
		if (angularVelocity != 0f)
		{
			base.transform.RotateAround(base.transform.position.XY(), Vector3.forward, angularVelocity * base.LocalDeltaTime);
		}
		if (baseData.UsesCustomAccelerationCurve)
		{
			float time = Mathf.Clamp01((m_timeElapsed - baseData.IgnoreAccelCurveTime) / baseData.CustomAccelerationCurveDuration);
			m_currentSpeed = baseData.AccelerationCurve.Evaluate(time) * baseData.speed;
		}
		base.specRigidbody.Velocity = m_currentDirection * m_currentSpeed;
		m_currentSpeed *= 1f - baseData.damping * base.LocalDeltaTime;
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
