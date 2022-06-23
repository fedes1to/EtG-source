using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Blobulord/SplitAttack1")]
public class BlobulordSplitAttack1 : Script
{
	public class BlobulonBullet : Bullet
	{
		private static string[] Projectiles = new string[3] { "blobulon", "blobulon", "blobuloid" };

		private Vector2 m_spawnPoint;

		private int m_spawnDelay;

		private bool m_doSpawn;

		public BlobulonBullet(Vector2 spawnPoint, int spawnDelay = 0, bool doSpawn = false)
		{
			BankName = BraveUtility.RandomElement(Projectiles);
			m_spawnPoint = spawnPoint;
			m_spawnDelay = spawnDelay;
			m_doSpawn = doSpawn;
		}

		protected override IEnumerator Top()
		{
			tk2dSpriteAnimator spriteAnimator = Projectile.spriteAnimator;
			if (m_doSpawn)
			{
				Projectile.specRigidbody.CollideWithOthers = false;
				tk2dSpriteAnimationClip spawnClip = spriteAnimator.GetClipByName(BankName + "_projectile_spawn");
				spriteAnimator.Play(spawnClip);
				while (spriteAnimator.IsPlaying(spawnClip))
				{
					yield return Wait(1);
				}
				Projectile.specRigidbody.CollideWithOthers = true;
			}
			else
			{
				spriteAnimator.Play();
			}
			int timeRemaining = 352 - m_spawnDelay;
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(specRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			Projectile.BulletScriptSettings.surviveTileCollisions = true;
			ChangeSpeed(new Speed(10f), 30);
			while (timeRemaining > 100)
			{
				if (timeRemaining == 145)
				{
					ChangeSpeed(new Speed(), 45);
				}
				timeRemaining--;
				yield return Wait(1);
			}
			if (Vector2.Distance(m_spawnPoint, base.Position) > 20f)
			{
				Speed = 0f;
				spriteAnimator.Play(BankName + "_projectile_impact");
				spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
				yield break;
			}
			Vector2 startVelocity = Velocity;
			base.ManualControl = true;
			while (timeRemaining > 50)
			{
				Vector2 goalVelocity = (m_spawnPoint - base.Position) / ((float)timeRemaining / 60f);
				Velocity = Vector2.Lerp(startVelocity, goalVelocity, (float)(100 - timeRemaining) / 50f);
				base.Position += Velocity / 60f;
				timeRemaining--;
				yield return Wait(1);
			}
			SpeculativeRigidbody specRigidbody2 = Projectile.specRigidbody;
			specRigidbody2.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Remove(specRigidbody2.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			Vector2 goal = m_spawnPoint + (base.Position - m_spawnPoint).normalized * UnityEngine.Random.Range(0.5f, 2f);
			Direction = (goal - base.Position).ToAngle();
			Speed = (goal - base.Position).magnitude / 50f * 60f;
			base.ManualControl = false;
			yield return Wait(50);
			Speed = 0f;
			spriteAnimator.Play(BankName + "_projectile_impact");
			spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
		}

		private void OnTileCollision(CollisionData tilecollision)
		{
			float num = (-tilecollision.MyRigidbody.Velocity).ToAngle();
			float num2 = tilecollision.Normal.ToAngle();
			float num3 = BraveMathCollege.ClampAngle360(num + 2f * (num2 - num));
			num3 = (Direction = num3 + UnityEngine.Random.Range(-30f, 30f));
			Velocity = BraveMathCollege.DegreesToVector(Direction, Speed);
			PhysicsEngine.PostSliceVelocity = Velocity;
		}

		private void OnAnimationCompleted(tk2dSpriteAnimator tk2DSpriteAnimator, tk2dSpriteAnimationClip tk2DSpriteAnimationClip)
		{
			Vanish(true);
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)Projectile)
			{
				SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
				specRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Remove(specRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
				tk2dSpriteAnimator spriteAnimator = Projectile.spriteAnimator;
				spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
			}
		}
	}

	private const int NumBullets = 32;

	private const int TotalTime = 352;

	private const float BulletSpeed = 10f;

	protected override IEnumerator Top()
	{
		for (int j = 0; j < 32; j++)
		{
			float num = (float)j * 11.25f;
			Fire(new Offset(1f, 0f, num, string.Empty), new Direction(num), new Speed(UnityEngine.Random.Range(4f, 11f)), new BlobulonBullet(base.Position));
		}
		for (int i = 0; i < 10; i++)
		{
			float angle = RandomAngle();
			Fire(new Offset(UnityEngine.Random.Range(0f, 1.5f), 0f, angle, string.Empty), new Direction(angle), new Speed(), new BlobulonBullet(base.Position, i * 30, true));
			yield return Wait(30);
		}
	}
}
