using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bird/EggVomit1")]
public class BirdEggVomit1 : Script
{
	public class EggBullet : Bullet
	{
		private bool spawnedBursts;

		public EggBullet()
			: base("egg")
		{
		}

		protected override IEnumerator Top()
		{
			Projectile.sprite.SetSprite("egg_projectile_001");
			float startRotation = ((!(Direction > 90f) || !(Direction < 270f)) ? 135 : (-135));
			for (int i = 0; i < 45; i++)
			{
				Vector2 velocity = BraveMathCollege.DegreesToVector(Direction, Speed);
				velocity += new Vector2(0f, -7f) / 60f;
				Direction = velocity.ToAngle();
				Speed = velocity.magnitude;
				Projectile.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startRotation, 0f, (float)i / 45f));
				yield return Wait(1);
			}
			Projectile.transform.rotation = Quaternion.identity;
			Speed = 0f;
			Projectile.spriteAnimator.Play();
			int animTime = Mathf.RoundToInt(Projectile.spriteAnimator.DefaultClip.BaseClipLength * 60f);
			yield return Wait(animTime / 2);
			if (!spawnedBursts)
			{
				SpawnBursts();
			}
			yield return Wait(animTime / 2);
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!spawnedBursts && !preventSpawningProjectiles)
			{
				SpawnBursts();
			}
		}

		private void SpawnBursts()
		{
			float num = RandomAngle();
			float num2 = 10f;
			for (int i = 0; i < 36; i++)
			{
				Fire(new Direction(num + (float)i * num2), new Speed(9f), new AcceleratingBullet());
			}
			num += num2 / 2f;
			for (int j = 0; j < 36; j++)
			{
				Fire(new Direction(num + (float)j * num2), new Speed(5f), new AcceleratingBullet());
			}
			num += num2 / 2f;
			for (int k = 0; k < 36; k++)
			{
				Fire(new Direction(num + (float)k * num2), new Speed(1f), new AcceleratingBullet());
			}
			spawnedBursts = true;
		}
	}

	public class AcceleratingBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(9f), 180);
			return null;
		}
	}

	private const int NumBullets = 36;

	protected override IEnumerator Top()
	{
		float num = BraveMathCollege.ClampAngle360(Direction);
		float direction = ((!(num > 90f) || !(num <= 180f)) ? 20 : 160);
		Fire(new Direction(direction), new Speed(2f), new EggBullet());
		return null;
	}
}
