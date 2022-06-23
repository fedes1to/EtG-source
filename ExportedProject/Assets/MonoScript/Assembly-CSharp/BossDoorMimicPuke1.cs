using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossDoorMimic/Puke1")]
public class BossDoorMimicPuke1 : Script
{
	public class PulseBullet : Bullet
	{
		private const float SinPeriod = 0.75f;

		private const float SinMagnitude = 0.75f;

		private float m_initialOffest;

		public PulseBullet(float initialOffest)
			: base("puke_burst")
		{
			m_initialOffest = initialOffest;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			Vector2 offsetDir = BraveMathCollege.DegreesToVector(Direction);
			for (int i = 0; i < 600; i++)
			{
				UpdateVelocity();
				truePosition += Velocity / 60f;
				float mag = Mathf.Sin((m_initialOffest + (float)base.Tick / 60f / 0.75f) * (float)Math.PI) * 0.75f;
				if (i < 60)
				{
					mag *= (float)i / 60f;
				}
				base.Position = truePosition + offsetDir * mag;
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class SnakeBullet : Bullet
	{
		private const float SnakeMagnitude = 0.75f;

		private const float SnakePeriod = 3f;

		private int m_delay;

		private Vector2 m_target;

		private bool m_shouldHome;

		public SnakeBullet(int delay, Vector2 target, bool shouldHome)
			: base("puke_snake")
		{
			m_delay = delay;
			m_target = target;
			m_shouldHome = shouldHome;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			yield return Wait(m_delay);
			Vector2 truePosition = base.Position;
			for (int i = 0; i < 360; i++)
			{
				float offsetMagnitude = Mathf.SmoothStep(-0.75f, 0.75f, Mathf.PingPong(0.5f + (float)i / 60f * 3f, 1f));
				if (m_shouldHome && i > 20 && i < 60)
				{
					float num = (m_target - truePosition).ToAngle();
					float value = BraveMathCollege.ClampAngle180(num - Direction);
					Direction += Mathf.Clamp(value, -6f, 6f);
				}
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + BraveMathCollege.DegreesToVector(Direction - 90f, offsetMagnitude);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumPulseBullets = 32;

	private const float PulseBulletSpeed = 4.5f;

	private const int NumInitialSnakes = 8;

	private const int NumLateSnakes = 6;

	private const int NumBulletsInSnake = 5;

	private const int SnakeBulletSpeed = 8;

	protected override IEnumerator Top()
	{
		float pulseStartAngle = RandomAngle();
		for (int j = 0; j < 32; j++)
		{
			float direction2 = SubdivideCircle(pulseStartAngle, 32, j);
			Fire(new Direction(direction2), new Speed(4.5f), new PulseBullet((float)(2 * j) / 32f));
		}
		for (int k = 0; k < 8; k++)
		{
			float num = RandomAngle();
			Vector2 predictedTargetPosition = GetPredictedTargetPosition((!((double)UnityEngine.Random.value < 0.5)) ? 1 : 0, 8f);
			int num2 = UnityEngine.Random.Range(0, 10);
			for (int l = 0; l < 5; l++)
			{
				Fire(new Offset(new Vector2(0f, 1f), num, string.Empty), new Direction(num), new Speed(8f), new SnakeBullet(num2 + l * 3, predictedTargetPosition, false));
			}
		}
		yield return Wait(40);
		for (int m = 0; m < 32; m++)
		{
			float direction3 = SubdivideCircle(pulseStartAngle, 32, m, 1f, true);
			Fire(new Direction(direction3), new Speed(4.5f), new PulseBullet((float)(2 * m) / 32f));
		}
		for (int i = 0; i < 6; i++)
		{
			float direction = RandomAngle();
			Vector2 targetPos = GetPredictedTargetPosition((!((double)UnityEngine.Random.value < 0.5)) ? 1 : 0, 8f);
			bool shouldHome = UnityEngine.Random.value < 0.33f;
			for (int n = 0; n < 5; n++)
			{
				Fire(new Offset(new Vector2(0f, 1f), direction, string.Empty), new Direction(direction), new Speed(8f), new SnakeBullet(n * 3, targetPos, shouldHome));
			}
			yield return Wait(10);
		}
		yield return Wait(20);
	}
}
