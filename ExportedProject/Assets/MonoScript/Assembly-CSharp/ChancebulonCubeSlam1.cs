using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Chancebulon/CubeSlam1")]
public class ChancebulonCubeSlam1 : Script
{
	public class ReversingBullet : Bullet
	{
		public ReversingBullet()
			: base("reversible")
		{
		}

		protected override IEnumerator Top()
		{
			if ((bool)base.BulletBank && (bool)base.BulletBank.healthHaver)
			{
				base.BulletBank.healthHaver.OnPreDeath += OnPreDeath;
			}
			float speed = Speed;
			yield return Wait(40);
			ChangeSpeed(new Speed(), 20);
			yield return Wait(20);
			Direction += 180f;
			Projectile.spriteAnimator.Play();
			yield return Wait(60);
			ChangeSpeed(new Speed(speed), 40);
			yield return Wait(70);
			Vanish();
		}

		private void OnPreDeath(Vector2 deathDir)
		{
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)base.BulletBank && (bool)base.BulletBank.healthHaver)
			{
				base.BulletBank.healthHaver.OnPreDeath -= OnPreDeath;
			}
		}
	}

	private const int NumBullets = 11;

	protected override IEnumerator Top()
	{
		FireLine(45f);
		FireLine(135f);
		FireLine(225f);
		FireLine(315f);
		yield return Wait(190);
	}

	private void FireLine(float startingAngle)
	{
		float num = 9f;
		for (int i = 0; i < 11; i++)
		{
			float num2 = Mathf.Atan((-45f + (float)i * num) / 45f) * 57.29578f;
			float num3 = Mathf.Cos(num2 * ((float)Math.PI / 180f));
			float num4 = ((!((double)Mathf.Abs(num3) < 0.0001)) ? (1f / num3) : 1f);
			Fire(new Direction(num2 + startingAngle), new Speed(num4 * 9f), new ReversingBullet());
		}
	}
}
