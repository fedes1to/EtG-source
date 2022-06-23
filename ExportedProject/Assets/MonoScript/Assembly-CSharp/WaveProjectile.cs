using System;
using UnityEngine;

public class WaveProjectile : Projectile
{
	public float amplitude = 1f;

	public float frequency = 2f;

	protected override void Move()
	{
		m_timeElapsed += base.LocalDeltaTime;
		int num = ((!base.Inverted) ? 1 : (-1));
		float num2 = (float)num * amplitude * 2f * (float)Math.PI * frequency * Mathf.Cos(m_timeElapsed * 2f * (float)Math.PI * frequency);
		base.specRigidbody.Velocity = base.transform.right * baseData.speed + base.transform.up * num2;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
