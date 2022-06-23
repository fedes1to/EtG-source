using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Chancebulon/BlobProjectileAttack1")]
public class ChancebulonBlobProjectileAttack1 : Script
{
	public enum BlobType
	{
		Normal,
		Poison,
		Lead
	}

	public class BlobulonBullet : Bullet
	{
		private static string[] Projectiles = new string[3] { "blobulon", "blobulon", "blobuloid" };

		private BlobType m_blobType;

		public BlobulonBullet(BlobType blobType)
		{
			BankName = BraveUtility.RandomElement(Projectiles);
			m_blobType = blobType;
		}

		protected override IEnumerator Top()
		{
			tk2dSpriteAnimator spriteAnimator = Projectile.spriteAnimator;
			spriteAnimator.Play();
			int timeRemaining = 200;
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(specRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			Projectile.BulletScriptSettings.surviveTileCollisions = true;
			ChangeSpeed(new Speed(10f), 30);
			if (m_blobType == BlobType.Poison)
			{
				Projectile.GetComponents<GoopDoer>()[1].enabled = true;
			}
			else if (m_blobType == BlobType.Lead)
			{
				Projectile.GetComponents<GoopDoer>()[0].enabled = true;
			}
			while (timeRemaining > 0)
			{
				if (timeRemaining == 45)
				{
					ChangeSpeed(new Speed(), 45);
				}
				timeRemaining--;
				yield return Wait(1);
			}
			SpeculativeRigidbody specRigidbody2 = Projectile.specRigidbody;
			specRigidbody2.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Remove(specRigidbody2.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
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

	private const int NumBullets = 5;

	private const int TotalTime = 200;

	public const float BulletSpeed = 10f;

	protected override IEnumerator Top()
	{
		BlobType blobType = (BlobType)UnityEngine.Random.Range(0, 3);
		for (int i = 0; i < 5; i++)
		{
			float num = RandomAngle();
			Fire(new Offset(1f, 0f, num, string.Empty), new Direction(num), new Speed(UnityEngine.Random.Range(4f, 11f)), new BlobulonBullet(blobType));
		}
		return null;
	}
}
