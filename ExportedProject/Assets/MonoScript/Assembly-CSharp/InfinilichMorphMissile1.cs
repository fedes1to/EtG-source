using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/MorphMissile1")]
public class InfinilichMorphMissile1 : Script
{
	public class MissileBullet : Bullet
	{
		private Vector2 m_centerOfMassOffset;

		private float m_sign;

		private bool m_isBooster;

		public MissileBullet(Vector2 centerOfMassOffset, float sign, bool isBooster)
		{
			m_centerOfMassOffset = centerOfMassOffset;
			m_sign = sign;
			m_isBooster = isBooster;
		}

		protected override IEnumerator Top()
		{
			Speed *= 3f;
			for (int i = 0; i < 60; i++)
			{
				if (m_isBooster && UnityEngine.Random.value < 0.12f)
				{
					Fire(new Direction(UnityEngine.Random.Range(150f, 210f), DirectionType.Relative), new Speed(6f));
				}
				yield return Wait(1);
			}
			Speed /= 3f;
			Vector2 centerOfMass = base.Position + m_centerOfMassOffset;
			float directionOffset = ((m_sign < 0f) ? (-180) : 0);
			base.ManualControl = true;
			for (int j = 0; j < 150; j++)
			{
				float desiredDirection = (BulletManager.PlayerPosition() - centerOfMass).ToAngle();
				if (j <= 90 || !(BraveMathCollege.AbsAngleBetween(desiredDirection, Direction) < 3f))
				{
					Direction = Mathf.MoveTowardsAngle(maxDelta: Mathf.SmoothStep(0f, 2.5f, (float)j / 120f), current: Direction, target: desiredDirection);
					UpdateVelocity();
					centerOfMass += Velocity / 60f;
					base.Position = centerOfMass + (Quaternion.Euler(0f, 0f, Direction + directionOffset) * -m_centerOfMassOffset).XY();
					if (m_isBooster && UnityEngine.Random.value < 0.04f)
					{
						Fire(new Direction(UnityEngine.Random.Range(150f, 210f), DirectionType.Relative), new Speed(6f));
					}
					yield return Wait(1);
				}
			}
			base.ManualControl = false;
			for (int k = 0; k < 240; k++)
			{
				Speed += 0.2f;
				if (m_isBooster && UnityEngine.Random.value < 0.12f)
				{
					Fire(new Direction(UnityEngine.Random.Range(130f, 230f), DirectionType.Relative), new Speed(8f));
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const float EnemyBulletSpeedItem = 5f;

	private static int[] RightBoosters = new int[9] { 1, 2, 4, 7, 15, 24, 32, 35, 37 };

	private static int[] LeftBoosters = new int[9] { 1, 3, 6, 14, 23, 31, 34, 36, 37 };

	private float m_sign;

	protected override IEnumerator Top()
	{
		float num = BraveMathCollege.ClampAngle180(base.BulletBank.aiAnimator.FacingDirection);
		m_sign = ((num <= 90f && num >= -90f) ? 1 : (-1));
		Vector2 vector = base.Position + new Vector2(m_sign * 2.5f, 0.5f);
		for (int i = 1; i <= 37; i++)
		{
			string transform = "morph bullet " + i;
			bool isBooster = Array.IndexOf((!(m_sign > 0f)) ? LeftBoosters : RightBoosters, i) >= 0;
			Vector2 vector2 = BulletManager.TransformOffset(Vector2.zero, transform);
			Fire(new Offset(transform), new Direction((!(m_sign > 0f)) ? 180 : 0), new Speed(5f), new MissileBullet(vector - vector2, m_sign, isBooster));
		}
		return null;
	}
}
