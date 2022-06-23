using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BulletKing/Slam1")]
public class BulletKingSlam1 : Script
{
	public class SpinningBullet : Bullet
	{
		private Vector2 centerPoint;

		private float startAngle;

		public SpinningBullet(Vector2 centerPoint, float startAngle, bool isHard)
			: base("slam", false, false, isHard)
		{
			this.centerPoint = centerPoint;
			this.startAngle = startAngle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			float radius = Vector2.Distance(centerPoint, base.Position);
			float speed = Speed;
			float spinAngle = startAngle;
			float spinSpeed = 0f;
			for (int i = 0; i < 180; i++)
			{
				speed += 2f / 15f;
				radius += speed / 60f;
				spinSpeed += 1f / 6f;
				spinAngle += spinSpeed / 60f;
				base.Position = centerPoint + BraveMathCollege.DegreesToVector(spinAngle, radius);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 36;

	private const int NumHardBullets = 12;

	private const float RadiusAcceleration = 8f;

	private const float SpinAccelration = 10f;

	public static float SpinningBulletSpinSpeed = 180f;

	private const int Time = 180;

	protected bool IsHard
	{
		get
		{
			return this is BulletKingSlamHard1;
		}
	}

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float startAngle = RandomAngle();
		float delta = 10f;
		for (int i = 0; i < 36; i++)
		{
			float num = startAngle + (float)i * delta;
			Fire(new Offset(1f, 0f, num, string.Empty), new Direction(num), new Speed((!IsHard) ? 5 : 8), new SpinningBullet(base.Position, num, IsHard));
		}
		if (IsHard)
		{
			for (int j = 0; j < 12; j++)
			{
				float num2 = RandomAngle();
				Fire(new Offset(1f, 0f, num2, string.Empty), new Direction(num2), new Speed(Random.Range(3f, 5f)), new SpinningBullet(base.Position, num2, IsHard));
			}
		}
		yield return Wait(90);
	}
}
