using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Megalich/Spew1")]
public class MegalichSpew1 : Script
{
	public class SnakeBullet : Bullet
	{
		private int m_delay;

		private Vector2 m_target;

		private bool m_shouldHome;

		public SnakeBullet(int delay, Vector2 target, bool shouldHome)
			: base("spew")
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

	private const int NumInitialSnakes = 20;

	private const int NumLateSnakes = 20;

	private const int NumBullets = 5;

	private const int BulletSpeed = 12;

	private const float SnakeMagnitude = 0.75f;

	private const float SnakePeriod = 3f;

	protected override IEnumerator Top()
	{
		for (int j = 0; j < 20; j++)
		{
			float direction = SubdivideArc(-165f, 150f, 20, j) + Random.Range(-3f, 3f);
			Vector2 predictedTargetPosition = GetPredictedTargetPosition((!((double)Random.value < 0.5)) ? 1 : 0, 12f);
			int num = Random.Range(0, 10);
			for (int k = 0; k < 5; k++)
			{
				Fire(new Direction(direction), new Speed(12f), new SnakeBullet(num + k * 3, predictedTargetPosition, false));
			}
		}
		yield return Wait(40);
		for (int i = 0; i < 20; i++)
		{
			float startingDirection = Random.Range(-165f, -15f);
			Vector2 offset = Random.insideUnitCircle;
			Vector2 targetPos = GetPredictedTargetPosition((!((double)Random.value < 0.5)) ? 1 : 0, 12f);
			bool shouldHome = Random.value < 0.33f;
			for (int l = 0; l < 5; l++)
			{
				Fire(new Offset(offset, 0f, string.Empty), new Direction(startingDirection), new Speed(12f), new SnakeBullet(l * 3, targetPos, shouldHome));
			}
			yield return Wait(10);
		}
		yield return Wait(20);
	}
}
