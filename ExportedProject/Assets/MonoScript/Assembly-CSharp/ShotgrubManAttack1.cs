using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class ShotgrubManAttack1 : Script
{
	public class GrossBullet : Bullet
	{
		private float deltaAngle;

		public GrossBullet(float deltaAngle)
			: base("gross")
		{
			this.deltaAngle = deltaAngle;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(20);
			Direction += deltaAngle;
			Speed += Random.Range(-1f, 1f);
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float num = RandomAngle();
				float num2 = 60f;
				for (int i = 0; i < 6; i++)
				{
					Fire(new Direction(num + num2 * (float)i), new Speed(8f), new GrubBullet());
				}
			}
		}
	}

	public class GrubBullet : Bullet
	{
		public GrubBullet()
		{
			base.SuppressVfx = true;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			float startVal = Random.value;
			for (int i = 0; i < 360; i++)
			{
				float offsetMagnitude = Mathf.SmoothStep(-0.75f, 0.75f, Mathf.PingPong(startVal + (float)i / 60f * 3f, 1f));
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + BraveMathCollege.DegreesToVector(Direction - 90f, offsetMagnitude);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 5;

	private const float Spread = 45f;

	private const int NumDeathBullets = 6;

	private const float GrubMagnitude = 0.75f;

	private const float GrubPeriod = 3f;

	protected override IEnumerator Top()
	{
		float num = -22.5f;
		float num2 = 9f;
		for (int i = 0; i < 5; i++)
		{
			Fire(new Direction(0f, DirectionType.Aim), new Speed(9f), new GrossBullet(num + (float)i * num2));
		}
		return null;
	}
}
