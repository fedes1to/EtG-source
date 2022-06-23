using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class PedestalMimicThrowStuff1 : Script
{
	public class AcceleratingBullet : Bullet
	{
		public AcceleratingBullet()
			: base("default")
		{
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(12f), 20);
			Wait(120);
			yield break;
		}
	}

	public class HomingShot : Bullet
	{
		public HomingShot(string bulletName)
			: base(bulletName)
		{
		}

		protected override IEnumerator Top()
		{
			for (int i = 0; i < 180; i++)
			{
				float aim = GetAimDirection(1f, 12f);
				float delta = BraveMathCollege.ClampAngle180(aim - Direction);
				if (Mathf.Abs(delta) > 100f)
				{
					break;
				}
				Direction += Mathf.MoveTowards(0f, delta, 1f);
				yield return Wait(1);
			}
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				for (int i = 0; i < 8; i++)
				{
					Fire(new Direction(i * 45), new Speed(8f), new SpeedChangingBullet(10f, 120, 600));
				}
			}
		}
	}

	private static readonly string[] BulletNames = new string[3] { "boot", "gun", "sponge" };

	private const float HomingSpeed = 12f;

	protected override IEnumerator Top()
	{
		int numBullets = 20;
		float deltaAngle = 360 / (numBullets + 1) * 2;
		int j = Random.RandomRange(0, BulletNames.Length);
		for (int i = 0; i < numBullets; i++)
		{
			float angle = -180f + (float)i * deltaAngle;
			Fire(new Offset(1.5f, 0f, angle, string.Empty), new Direction(angle), new Speed(4f), new AcceleratingBullet());
			yield return Wait(4);
			if (i % 10 == 9)
			{
				Fire(new Direction(0f, DirectionType.Aim), new Speed(12f), new HomingShot(BulletNames[j]));
				j = (j + 1) % BulletNames.Length;
			}
		}
	}
}
