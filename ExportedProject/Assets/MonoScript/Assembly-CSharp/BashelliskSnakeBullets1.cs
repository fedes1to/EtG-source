using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Bashellisk/SnakeBullets1")]
public class BashelliskSnakeBullets1 : Script
{
	public class SnakeBullet : Bullet
	{
		private int delay;

		public SnakeBullet(int delay)
			: base("snakeBullet")
		{
			this.delay = delay;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			yield return Wait(delay);
			Vector2 truePosition = base.Position;
			for (int i = 0; i < 360; i++)
			{
				float offsetMagnitude = Mathf.SmoothStep(-0.6f, 0.6f, Mathf.PingPong(0.5f + (float)i / 60f * 3f, 1f));
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + BraveMathCollege.DegreesToVector(Direction - 90f, offsetMagnitude);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 8;

	private const int BulletSpeed = 11;

	private const float SnakeMagnitude = 0.6f;

	private const float SnakePeriod = 3f;

	protected override IEnumerator Top()
	{
		float aimDirection = GetAimDirection((!((double)Random.value < 0.5)) ? 1 : 0, 11f);
		for (int i = 0; i < 8; i++)
		{
			Fire(new Direction(aimDirection), new Speed(11f), new SnakeBullet(i * 3));
		}
		return null;
	}
}
