using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class GrimReaperSwing1 : Script
{
	public class ReaperBullet : Bullet
	{
		private Vector2 m_phantomBulletPoint;

		private Vector2 m_offset;

		public ReaperBullet(Vector2 phantomBulletPoint, Vector2 offset)
		{
			m_phantomBulletPoint = phantomBulletPoint;
			m_offset = offset;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			yield return Wait(5);
			for (int i = 0; i < 180; i++)
			{
				Projectile.ResetDistance();
				Direction = Mathf.MoveTowardsAngle(Direction, GetAimDirection(m_phantomBulletPoint, 0f, Speed), 2f);
				UpdateVelocity();
				m_phantomBulletPoint += Velocity / 60f;
				base.Position += Velocity / 60f;
				float rotation = Velocity.ToAngle();
				Vector2 goalPos = m_phantomBulletPoint + m_offset.Rotate(rotation);
				if (i < 30)
				{
					Vector2 vector = goalPos - base.Position;
					base.Position += vector / (30 - i);
				}
				else
				{
					base.Position = goalPos;
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		Vector2 zero = Vector2.zero;
		Vector2 p = zero + new Vector2(-1.5f, -1.5f);
		Vector2 vector = new Vector2(2f, -5f);
		Vector2 p2 = vector + new Vector2(-1.5f, 0.4f);
		Vector2 vector2 = new Vector2(-0.5f, -1.5f);
		Vector2 p3 = vector2 + new Vector2(0.75f, 0.75f);
		Vector2 vector3 = new Vector2(-0.5f, 1.5f);
		Vector2 p4 = vector3 + new Vector2(0.75f, -0.75f);
		float num = BraveMathCollege.ClampAngle180((BulletManager.PlayerPosition() - base.BulletBank.specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle());
		bool flag = num > -45f && num < 120f;
		Vector2 phantomBulletPoint = base.Position + BraveMathCollege.CalculateBezierPoint(0.5f, zero, p, p2, vector);
		for (int i = 0; i < 12; i++)
		{
			float num2 = (float)i / 11f;
			Vector2 offset = BraveMathCollege.CalculateBezierPoint(num2, zero, p, p2, vector);
			if (flag)
			{
				num2 = 1f - num2;
			}
			Vector2 offset2 = BraveMathCollege.CalculateBezierPoint(num2, vector2, p3, p4, vector3);
			Fire(new Offset(offset, 0f, string.Empty), new Direction(0f, DirectionType.Aim), new Speed(8f), new ReaperBullet(phantomBulletPoint, offset2));
		}
		return null;
	}
}
