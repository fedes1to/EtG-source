using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class BulletCardinalHat1 : Script
{
	public class SpinningBullet : Bullet
	{
		private const float RotationSpeed = 6f;

		private Vector2 m_origin;

		private Vector2 m_startPos;

		public SpinningBullet(Vector2 origin, Vector2 startPos)
			: base("hat")
		{
			m_origin = origin;
			m_startPos = startPos;
			base.SuppressVfx = true;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 delta = (m_startPos - base.Position) / 45f;
			for (int j = 0; j < 45; j++)
			{
				base.Position += delta;
				yield return Wait(1);
			}
			Speed = 9f;
			float angle = 0f;
			Vector2 centerOfMass = m_origin;
			Vector2 centerOfMassOffset = m_origin - base.Position;
			for (int i = 0; i < 120; i++)
			{
				Direction = Mathf.MoveTowardsAngle(Direction, (BulletManager.PlayerPosition() - centerOfMass).ToAngle(), 1.5f);
				UpdateVelocity();
				centerOfMass += Velocity / 60f;
				angle += 6f;
				base.Position = centerOfMass + (Quaternion.Euler(0f, 0f, angle) * centerOfMassOffset).XY();
				yield return Wait(1);
			}
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		FireSpinningLine(new Vector2(-0.75f, 0f), new Vector2(0.75f, 0f), 3);
		FireSpinningLine(new Vector2(0f, -0.75f), new Vector2(0f, 0.75f), 3);
		yield return Wait(60);
	}

	private void FireSpinningLine(Vector2 start, Vector2 end, int numBullets)
	{
		start *= 0.5f;
		end *= 0.5f;
		float direction = (BulletManager.PlayerPosition() - base.Position).ToAngle();
		for (int i = 0; i < numBullets; i++)
		{
			Vector2 vector = Vector2.Lerp(start, end, (float)i / ((float)numBullets - 1f));
			Fire(new Direction(direction), new SpinningBullet(base.Position, base.Position + vector));
		}
	}
}
