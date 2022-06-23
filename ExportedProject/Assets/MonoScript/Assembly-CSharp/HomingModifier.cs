using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class HomingModifier : BraveBehaviour
{
	public float HomingRadius = 2f;

	public float AngularVelocity = 180f;

	protected Projectile m_projectile;

	private void Start()
	{
		if (!m_projectile)
		{
			m_projectile = GetComponent<Projectile>();
		}
		Projectile obj = m_projectile;
		obj.ModifyVelocity = (Func<Vector2, Vector2>)Delegate.Combine(obj.ModifyVelocity, new Func<Vector2, Vector2>(ModifyVelocity));
	}

	public void AssignProjectile(Projectile source)
	{
		m_projectile = source;
	}

	private Vector2 ModifyVelocity(Vector2 inVel)
	{
		Vector2 vector = inVel;
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(m_projectile.LastPosition.IntXY(VectorConversions.Floor));
		List<AIActor> activeEnemies = absoluteRoomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null || activeEnemies.Count == 0)
		{
			return inVel;
		}
		float num = float.MaxValue;
		Vector2 vector2 = Vector2.zero;
		AIActor aIActor = null;
		Vector2 vector3 = ((!base.sprite) ? base.transform.position.XY() : base.sprite.WorldCenter);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aIActor2 = activeEnemies[i];
			if ((bool)aIActor2 && aIActor2.IsWorthShootingAt && !aIActor2.IsGone)
			{
				Vector2 vector4 = aIActor2.CenterPosition - vector3;
				float sqrMagnitude = vector4.sqrMagnitude;
				if (sqrMagnitude < num)
				{
					vector2 = vector4;
					num = sqrMagnitude;
					aIActor = aIActor2;
				}
			}
		}
		num = Mathf.Sqrt(num);
		if (num < HomingRadius && aIActor != null)
		{
			float num2 = 1f - num / HomingRadius;
			float target = vector2.ToAngle();
			float num3 = inVel.ToAngle();
			float maxDelta = AngularVelocity * num2 * m_projectile.LocalDeltaTime;
			float num4 = Mathf.MoveTowardsAngle(num3, target, maxDelta);
			if (m_projectile is HelixProjectile)
			{
				float angleDiff = num4 - num3;
				(m_projectile as HelixProjectile).AdjustRightVector(angleDiff);
			}
			else
			{
				if (m_projectile.shouldRotate)
				{
					base.transform.rotation = Quaternion.Euler(0f, 0f, num4);
				}
				vector = BraveMathCollege.DegreesToVector(num4, inVel.magnitude);
			}
			if (m_projectile.OverrideMotionModule != null)
			{
				m_projectile.OverrideMotionModule.AdjustRightVector(num4 - num3);
			}
		}
		if (vector == Vector2.zero || float.IsNaN(vector.x) || float.IsNaN(vector.y))
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
