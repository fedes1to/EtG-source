using System;
using UnityEngine;

public class LockOnHomingModifier : BraveBehaviour
{
	public float HomingRadius = 2f;

	public float AngularVelocity = 180f;

	public GameObject LockOnVFX;

	[NonSerialized]
	public AIActor lockOnTarget;

	protected Projectile m_projectile;

	private void Start()
	{
		if (!m_projectile)
		{
			m_projectile = GetComponent<Projectile>();
			if (!lockOnTarget && (bool)m_projectile.PossibleSourceGun && (bool)m_projectile.PossibleSourceGun.LastLaserSightEnemy)
			{
				lockOnTarget = m_projectile.PossibleSourceGun.LastLaserSightEnemy;
			}
		}
		Projectile obj = m_projectile;
		obj.ModifyVelocity = (Func<Vector2, Vector2>)Delegate.Combine(obj.ModifyVelocity, new Func<Vector2, Vector2>(ModifyVelocity));
	}

	public void AssignTargetManually(AIActor enemy)
	{
		lockOnTarget = enemy;
	}

	public void AssignProjectile(Projectile source)
	{
		m_projectile = source;
	}

	private Vector2 ModifyVelocity(Vector2 inVel)
	{
		Vector2 vector = inVel;
		if (!lockOnTarget)
		{
			return inVel;
		}
		Vector2 vector2 = ((!base.sprite) ? base.transform.position.XY() : base.sprite.WorldCenter);
		Vector2 vector3 = lockOnTarget.CenterPosition - vector2;
		AIActor aIActor = lockOnTarget;
		float sqrMagnitude = vector3.sqrMagnitude;
		sqrMagnitude = Mathf.Sqrt(sqrMagnitude);
		if (sqrMagnitude < HomingRadius && aIActor != null)
		{
			float num = 1f - sqrMagnitude / HomingRadius;
			float target = vector3.ToAngle();
			float num2 = inVel.ToAngle();
			float maxDelta = AngularVelocity * num * m_projectile.LocalDeltaTime;
			float num3 = Mathf.MoveTowardsAngle(num2, target, maxDelta);
			if (m_projectile is HelixProjectile)
			{
				float angleDiff = num3 - num2;
				(m_projectile as HelixProjectile).AdjustRightVector(angleDiff);
			}
			else
			{
				if (m_projectile.shouldRotate)
				{
					base.transform.rotation = Quaternion.Euler(0f, 0f, num3);
				}
				vector = BraveMathCollege.DegreesToVector(num3, inVel.magnitude);
			}
			if (m_projectile.OverrideMotionModule != null)
			{
				m_projectile.OverrideMotionModule.AdjustRightVector(num3 - num2);
			}
		}
		if (vector == Vector2.zero)
		{
			return inVel;
		}
		return vector;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_projectile)
		{
			Projectile obj = m_projectile;
			obj.ModifyVelocity = (Func<Vector2, Vector2>)Delegate.Remove(obj.ModifyVelocity, new Func<Vector2, Vector2>(ModifyVelocity));
		}
		base.OnDestroy();
	}
}
