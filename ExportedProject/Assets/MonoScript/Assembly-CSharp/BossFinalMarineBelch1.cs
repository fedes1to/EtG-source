using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalMarine/Belch1")]
public class BossFinalMarineBelch1 : Script
{
	public class SnakeBullet : Bullet
	{
		private int delay;

		private Vector2 target;

		public SnakeBullet(int delay, Vector2 target)
		{
			this.delay = delay;
			this.target = target;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			yield return Wait(delay);
			Vector2 truePosition = base.Position;
			for (int i = 0; i < 360; i++)
			{
				float offsetMagnitude = Mathf.SmoothStep(-0.75f, 0.75f, Mathf.PingPong(0.5f + (float)i / 60f * 3f, 1f));
				if (i > 20 && i < 60)
				{
					float num = (target - truePosition).ToAngle();
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

	private const int NumSnakes = 10;

	private const int NumBullets = 5;

	private const int BulletSpeed = 12;

	private const float SnakeMagnitude = 0.75f;

	private const float SnakePeriod = 3f;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 10; i++)
		{
			float startingDirection = Random.Range(-150f, -30f);
			Vector2 targetPos = GetPredictedTargetPosition((!((double)Random.value < 0.5)) ? 1 : 0, 12f);
			for (int j = 0; j < 5; j++)
			{
				Fire(new Direction(startingDirection), new Speed(12f), new SnakeBullet(j * 3, targetPos));
			}
			yield return Wait(6);
		}
	}
}
