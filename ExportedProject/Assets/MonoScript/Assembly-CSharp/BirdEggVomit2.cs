using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bird/EggVomit2")]
public class BirdEggVomit2 : Script
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
			float num = float.PositiveInfinity;
			for (int i = 0; i < 0; i++)
			{
				Fire(new Direction(0f, DirectionType.Aim), new Speed(9f), new ClusterBullet((float)i * num));
			}
			for (int j = 0; j < 12; j++)
			{
				Fire(new Direction(0f, DirectionType.Aim), new Speed(9f), new InnerBullet());
			}
			spawnedBursts = true;
		}
	}

	public class ClusterBullet : Bullet
	{
		private float clusterAngle;

		public ClusterBullet(float clusterAngle)
		{
			this.clusterAngle = clusterAngle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 centerPosition = base.Position;
			float radius = 0.5f;
			for (int i = 0; i < 180; i++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				radius += 1f / 30f;
				clusterAngle += 2.5f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(clusterAngle, radius);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class InnerBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 centerPosition = base.Position;
			float radius = 0.5f;
			int bounceDelay = Random.Range(0, 30);
			Vector2 startOffset = BraveMathCollege.DegreesToVector(RandomAngle(), Random.Range(0f, radius));
			float goalAngle = RandomAngle();
			float goalRadiusPercent = Random.value;
			for (int i = 0; i < 180; i++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				radius += 1f / 30f;
				Vector2 goalOffset = BraveMathCollege.DegreesToVector(goalAngle, goalRadiusPercent * radius);
				if (bounceDelay == 0)
				{
					startOffset = goalOffset;
					goalAngle = RandomAngle();
					goalRadiusPercent = Random.value;
					goalOffset = BraveMathCollege.DegreesToVector(goalAngle, goalRadiusPercent * radius);
					bounceDelay = 30;
					if (radius > 1f)
					{
						bounceDelay = Mathf.RoundToInt(radius * (float)bounceDelay);
					}
				}
				else
				{
					bounceDelay--;
				}
				base.Position = centerPosition + Vector2.Lerp(startOffset, goalOffset, 1f - (float)bounceDelay / 30f);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int ClusterBullets = 0;

	private const float ClusterRotation = 150f;

	private const float ClusterRadius = 0.5f;

	private const float ClusterRadiusSpeed = 2f;

	private const int InnerBullets = 12;

	private const int InnerBounceTime = 30;

	protected override IEnumerator Top()
	{
		float num = BraveMathCollege.ClampAngle360(Direction);
		float direction = ((!(num > 90f) || !(num <= 180f)) ? 20 : 160);
		Fire(new Direction(direction), new Speed(2f), new EggBullet());
		return null;
	}
}
