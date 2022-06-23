using System;
using UnityEngine;

public class ConvergeProjectile : Projectile
{
	public float convergeDistance = 10f;

	public float amplitude = 1f;

	protected override void Move()
	{
		m_timeElapsed += BraveTime.DeltaTime;
		float num = convergeDistance / baseData.speed;
		if (m_timeElapsed < num)
		{
			int num2 = ((!base.Inverted) ? 1 : (-1));
			float num3 = (float)num2 * amplitude * 2f * (float)Math.PI * (baseData.speed / (convergeDistance * 2f)) * Mathf.Cos(m_timeElapsed * 2f * (float)Math.PI * (baseData.speed / (convergeDistance * 2f)));
			base.specRigidbody.Velocity = base.transform.right * baseData.speed + base.transform.up * num3;
		}
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
