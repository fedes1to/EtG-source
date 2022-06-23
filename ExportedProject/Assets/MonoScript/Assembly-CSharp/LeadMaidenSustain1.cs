using System;
using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class LeadMaidenSustain1 : Script
{
	public class SpikeBullet : Bullet
	{
		private int m_fireTick;

		private float m_hitNormal;

		public SpikeBullet(int fireTick)
		{
			m_fireTick = fireTick;
		}

		protected override IEnumerator Top()
		{
			Projectile.BulletScriptSettings.surviveTileCollisions = true;
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			while (Speed > 0f)
			{
				yield return Wait(1);
			}
			float turnSpeed = BraveMathCollege.AbsAngleBetween(m_hitNormal, Direction) / 30f;
			for (int j = 0; j < 30; j++)
			{
				Direction = Mathf.MoveTowardsAngle(Direction, m_hitNormal, turnSpeed);
				yield return Wait(1);
			}
			while (base.Tick < m_fireTick)
			{
				yield return Wait(1);
			}
			base.Position = Projectile.transform.position;
			Projectile.spriteAnimator.Play();
			float startDirection = Direction;
			for (int i = 0; i < 30; i++)
			{
				Direction = Mathf.LerpAngle(startDirection, base.AimDirection, (float)i / 29f);
				yield return Wait(1);
			}
			Projectile.spriteAnimator.StopAndResetFrameToDefault();
			Vector2 target = BulletManager.PlayerPosition() + UnityEngine.Random.insideUnitCircle * 3f;
			Direction = (target - base.Position).ToAngle();
			Projectile.BulletScriptSettings.surviveTileCollisions = false;
			Speed = UnityEngine.Random.Range(6f, 9f);
			yield return Wait(180);
			Vanish();
		}

		private void OnCollision(CollisionData tileCollision)
		{
			Speed = 0f;
			m_hitNormal = tileCollision.Normal.ToAngle();
			PhysicsEngine.PostSliceVelocity = default(Vector2);
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			if ((bool)tileCollision.OtherRigidbody)
			{
				Vanish();
			}
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)Projectile)
			{
				SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
				specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			}
		}
	}

	private const int NumWaves = 3;

	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		float startDirection = RandomAngle();
		float delta = 30f;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 12; j++)
			{
				Fire(new Direction(startDirection + (float)j * delta), new Speed(10f), new SpikeBullet(90 + (3 - i) * 30));
			}
			yield return Wait(30);
			startDirection += 10f;
		}
		yield return Wait(90);
	}
}
