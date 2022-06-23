using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/MorphBoomerang1")]
public class InfinilichMorphBoomerang1 : Script
{
	public class BoomerangBullet : Bullet
	{
		private Vector2 m_centerOfMassOffset;

		private float m_sign;

		public BoomerangBullet(Vector2 centerOfMassOffset, float sign)
		{
			m_centerOfMassOffset = centerOfMassOffset;
			m_sign = sign;
		}

		protected override IEnumerator Top()
		{
			Vector2 centerOfMass = base.Position + m_centerOfMassOffset;
			base.ManualControl = true;
			float angle = 0f;
			for (int j = 0; j < 120; j++)
			{
				Direction = Mathf.MoveTowardsAngle(Direction, (BulletManager.PlayerPosition() - centerOfMass).ToAngle(), 1.5f);
				UpdateVelocity();
				centerOfMass += Velocity / 60f;
				angle += m_sign * -5f;
				base.Position = centerOfMass + (Quaternion.Euler(0f, 0f, angle) * -m_centerOfMassOffset).XY();
				yield return Wait(1);
			}
			for (int i = 0; i < 120; i++)
			{
				Vector2 target = Projectile.Owner.specRigidbody.GetUnitCenter(ColliderType.HitBox);
				Vector2 toTarget = target - base.Position;
				if (toTarget.magnitude < 1f)
				{
					Vanish();
					yield break;
				}
				if ((target - centerOfMass).magnitude < 5f)
				{
					Direction = Mathf.MoveTowardsAngle(Direction, toTarget.ToAngle(), 5f);
				}
				else
				{
					Direction = Mathf.MoveTowardsAngle(Direction, (target - centerOfMass).ToAngle(), 2.5f);
					Speed += 0.1f;
				}
				UpdateVelocity();
				centerOfMass += Velocity / 60f;
				angle += m_sign * -5f;
				base.Position = centerOfMass + (Quaternion.Euler(0f, 0f, angle) * -m_centerOfMassOffset).XY();
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const float EnemyBulletSpeedItem = 12f;

	private const float RotationSpeed = -5f;

	private float m_sign;

	protected override IEnumerator Top()
	{
		float num = BraveMathCollege.ClampAngle180(base.BulletBank.aiAnimator.FacingDirection);
		m_sign = ((num <= 90f && num >= -90f) ? 1 : (-1));
		Vector2 vector = base.Position + new Vector2(m_sign * 2.5f, 1f);
		float direction = (BulletManager.PlayerPosition() - vector).ToAngle();
		for (int i = 1; i <= 43; i++)
		{
			string transform = "morph bullet " + i;
			Vector2 vector2 = BulletManager.TransformOffset(Vector2.zero, transform);
			Fire(new Offset(transform), new Direction(direction), new Speed(12f), new BoomerangBullet(vector - vector2, m_sign));
		}
		return null;
	}
}
