using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BulletKing/Suck1")]
public class BulletKingSuck1 : Script
{
	public class SuckBullet : Bullet
	{
		private Vector2 m_centerPoint;

		private float m_startAngle;

		private int m_index;

		public SuckBullet(Vector2 centerPoint, float startAngle, int i)
			: base("suck")
		{
			m_centerPoint = centerPoint;
			m_startAngle = startAngle;
			m_index = i;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			float radius = 1f;
			float angle = m_startAngle;
			int remainingWait = 110;
			for (int j = 1; j < m_index; j++)
			{
				int steps = Mathf.Max(4, 14 - j * 2);
				float deltaRadius = 1f / (float)steps;
				float deltaAngle = 10f / (float)steps;
				for (int k = 0; k < steps; k++)
				{
					radius += deltaRadius;
					angle += deltaAngle;
					base.Position = m_centerPoint + BraveMathCollege.DegreesToVector(angle, radius);
					yield return Wait(1);
					remainingWait--;
				}
			}
			bool isDoingTell = false;
			while (remainingWait > 0)
			{
				if (!isDoingTell && remainingWait <= 60)
				{
					Projectile.spriteAnimator.Play("enemy_projectile_small_fire_tell");
					isDoingTell = true;
				}
				remainingWait--;
				yield return Wait(1);
			}
			Direction = (m_centerPoint - base.Position).ToAngle();
			float distToTravel = (m_centerPoint - base.Position).magnitude;
			base.ManualControl = false;
			for (int i = 0; i < 240; i++)
			{
				if (i < 40)
				{
					Speed += 0.2f;
				}
				distToTravel -= Speed / 60f;
				if (distToTravel < 2f)
				{
					Vanish();
					break;
				}
				yield return null;
			}
			Vanish();
		}
	}

	private const int NumBulletRings = 20;

	private const int BulletsPerRing = 6;

	private const float AngleDeltaPerRing = 10f;

	private const float StartRadius = 1f;

	private const float RadiusPerRing = 1f;

	public static float SpinningBulletSpinSpeed = 180f;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float startAngle = RandomAngle();
		float radius = 1f;
		for (int j = 0; j < 20; j++)
		{
			for (int k = 0; k < 6; k++)
			{
				float num = SubdivideCircle(startAngle, 6, k);
				Vector2 overridePosition = base.Position + BraveMathCollege.DegreesToVector(num, radius);
				Fire(Offset.OverridePosition(overridePosition), new Direction(num), new SuckBullet(base.Position, num, j));
			}
		}
		yield return Wait(110);
		for (int i = 159; i >= 0; i--)
		{
			if (i % 20 == 0)
			{
				float num2 = RandomAngle();
				Fire(new Offset(1f, 0f, num2, string.Empty), new Direction(num2), new Speed(7f), new Bullet("suck"));
			}
			yield return null;
		}
	}
}
