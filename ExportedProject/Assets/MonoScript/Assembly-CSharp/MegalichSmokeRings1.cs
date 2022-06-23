using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Megalich/SmokeRings1")]
public class MegalichSmokeRings1 : Script
{
	public class SmokeBullet : Bullet
	{
		private const float ExpandSpeed = 4.5f;

		private const float SpinSpeed = 40f;

		private MegalichSmokeRings1 m_parent;

		private float m_angle;

		private float m_spinSpeed;

		public SmokeBullet(MegalichSmokeRings1 parent, float angle = 0f)
			: base("ring")
		{
			m_parent = parent;
			m_angle = angle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 centerPosition = base.Position;
			float radius = 0f;
			m_spinSpeed = 40f;
			for (int i = 0; i < 300; i++)
			{
				if (i == 40)
				{
					ChangeSpeed(new Speed(18f), 120);
					ChangeDirection(new Direction(m_parent.GetAimDirection(1f, 10f)), 20);
					StartTask(ChangeSpinSpeedTask(180f, 240));
				}
				if (i > 50 && i < 120 && Random.value < 0.002f)
				{
					Direction = base.AimDirection;
					Speed = 12f;
					base.ManualControl = false;
					yield break;
				}
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				if (i < 40)
				{
					radius += 0.075f;
				}
				m_angle += m_spinSpeed / 60f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(m_angle, radius);
				yield return Wait(1);
			}
			Vanish();
		}

		private IEnumerator ChangeSpinSpeedTask(float newSpinSpeed, int term)
		{
			float delta = (newSpinSpeed - m_spinSpeed) / (float)term;
			for (int i = 0; i < term; i++)
			{
				m_spinSpeed += delta;
				yield return Wait(1);
			}
		}
	}

	private const int NumRings = 4;

	private const int NumBullets = 24;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float startDirection = RandomAngle();
		float delta = 15f;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 24; j++)
			{
				Fire(new Direction(-90f), new Speed(), new SmokeBullet(this, startDirection + (float)j * delta));
			}
			if (i < 3)
			{
				yield return Wait(45);
			}
		}
	}
}
