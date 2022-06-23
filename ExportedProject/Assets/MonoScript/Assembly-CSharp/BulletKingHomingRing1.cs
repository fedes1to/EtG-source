using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BulletKing/HomingRing1")]
public class BulletKingHomingRing1 : Script
{
	public class SmokeBullet : Bullet
	{
		private const float ExpandSpeed = 2f;

		private const float SpinSpeed = 120f;

		private const float Lifetime = 600f;

		private float m_angle;

		public SmokeBullet(float angle)
			: base("homingRing", false, false, true)
		{
			m_angle = angle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 centerPosition = base.Position;
			float radius = 0f;
			for (int i = 0; (float)i < 600f; i++)
			{
				Direction = Mathf.MoveTowardsAngle(target: (BulletManager.PlayerPosition() - centerPosition).ToAngle(), current: Direction, maxDelta: 1f);
				float speedScale = 1f;
				if (i < 60)
				{
					speedScale = Mathf.SmoothStep(0f, 1f, (float)i / 60f);
				}
				UpdateVelocity();
				centerPosition += Velocity / 60f * speedScale;
				if (i < 60)
				{
					radius += 1f / 30f;
				}
				m_angle += 2f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(m_angle, radius);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 24;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float startDirection = RandomAngle();
		float delta = 15f;
		for (int i = 0; i < 24; i++)
		{
			Fire(new Offset(0.0625f, 3.5625f, 0f, string.Empty), new Direction(0f, DirectionType.Aim), new Speed(6f), new SmokeBullet(startDirection + (float)i * delta));
		}
		yield return Wait(45);
	}
}
