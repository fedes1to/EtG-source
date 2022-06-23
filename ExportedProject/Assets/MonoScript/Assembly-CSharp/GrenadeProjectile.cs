using UnityEngine;

public class GrenadeProjectile : Projectile
{
	public float startingHeight = 1f;

	private float m_currentHeight;

	private Vector3 m_current3DVelocity;

	public override void Start()
	{
		base.Start();
		m_currentHeight = startingHeight;
		m_current3DVelocity = (m_currentDirection * m_currentSpeed).ToVector3ZUp();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void Move()
	{
		m_current3DVelocity.x = m_currentDirection.x;
		m_current3DVelocity.y = m_currentDirection.y;
		m_current3DVelocity.z += base.LocalDeltaTime * -10f;
		float num = m_currentHeight + m_current3DVelocity.z * base.LocalDeltaTime;
		if (num < 0f)
		{
			m_current3DVelocity.z = 0f - m_current3DVelocity.z;
			num *= -1f;
		}
		m_currentHeight = num;
		m_currentDirection = m_current3DVelocity.XY();
		Vector2 vector = m_current3DVelocity.XY().normalized * m_currentSpeed;
		base.specRigidbody.Velocity = new Vector2(vector.x, vector.y + m_current3DVelocity.z);
		base.LastVelocity = m_current3DVelocity.XY();
	}

	protected override void DoModifyVelocity()
	{
		if (ModifyVelocity != null)
		{
			Vector2 arg = m_current3DVelocity.XY().normalized * m_currentSpeed;
			arg = ModifyVelocity(arg);
			base.specRigidbody.Velocity = new Vector2(arg.x, arg.y + m_current3DVelocity.z);
			if (arg.sqrMagnitude > 0f)
			{
				m_currentDirection = arg.normalized;
			}
		}
	}
}
