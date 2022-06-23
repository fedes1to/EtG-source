using System;
using UnityEngine;

public class HelixProjectile : Projectile
{
	public float helixWavelength = 3f;

	public float helixAmplitude = 1f;

	private bool m_helixInitialized;

	private Vector2 m_initialRightVector;

	private Vector2 m_initialUpVector;

	private Vector2 m_privateLastPosition;

	private float m_displacement;

	private float m_yDisplacement;

	public void AdjustRightVector(float angleDiff)
	{
		if (!float.IsNaN(angleDiff))
		{
			m_initialUpVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialUpVector;
			m_initialRightVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialRightVector;
		}
	}

	protected override void Move()
	{
		if (!m_helixInitialized)
		{
			m_helixInitialized = true;
			m_initialRightVector = base.transform.right;
			m_initialUpVector = base.transform.up;
			m_privateLastPosition = base.sprite.WorldCenter;
			m_displacement = 0f;
			m_yDisplacement = 0f;
		}
		m_timeElapsed += BraveTime.DeltaTime;
		int num = ((!base.Inverted) ? 1 : (-1));
		float num2 = m_timeElapsed * baseData.speed;
		float num3 = (float)num * helixAmplitude * Mathf.Sin(m_timeElapsed * (float)Math.PI * baseData.speed / helixWavelength);
		float num4 = num2 - m_displacement;
		float num5 = num3 - m_yDisplacement;
		Vector2 vector = ((m_privateLastPosition = m_privateLastPosition + m_initialRightVector * num4 + m_initialUpVector * num5) - base.sprite.WorldCenter) / BraveTime.DeltaTime;
		float num6 = BraveMathCollege.Atan2Degrees(vector);
		if (shouldRotate && !float.IsNaN(num6))
		{
			base.transform.localRotation = Quaternion.Euler(0f, 0f, num6);
		}
		if (!float.IsNaN(num6))
		{
			m_currentDirection = vector.normalized;
		}
		m_displacement = num2;
		m_yDisplacement = num3;
		base.specRigidbody.Velocity = vector;
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
