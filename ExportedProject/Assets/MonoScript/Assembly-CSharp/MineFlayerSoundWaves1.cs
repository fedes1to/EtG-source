using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MineFlayer/SoundWaves1")]
public class MineFlayerSoundWaves1 : Script
{
	private class ReflectBullet : Bullet
	{
		private int m_ticksLeft = -1;

		public ReflectBullet()
			: base("bounce")
		{
		}

		protected override IEnumerator Top()
		{
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(specRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			Projectile.BulletScriptSettings.surviveTileCollisions = true;
			while (m_ticksLeft < 0)
			{
				if (base.ManualControl)
				{
					Reflect();
					base.ManualControl = false;
				}
				yield return Wait(1);
			}
			yield return Wait(m_ticksLeft);
			Vanish();
		}

		private void OnTileCollision(CollisionData tilecollision)
		{
			Reflect();
		}

		private void Reflect()
		{
			Speed = 8f;
			Direction += 180f + UnityEngine.Random.Range(-10f, 10f);
			Velocity = BraveMathCollege.DegreesToVector(Direction, Speed);
			PhysicsEngine.PostSliceVelocity = Velocity;
			m_ticksLeft = (int)((float)base.Tick * 1.5f);
			if ((bool)Projectile.TrailRendererController)
			{
				Projectile.TrailRendererController.Stop();
			}
			Projectile.BulletScriptSettings.surviveTileCollisions = false;
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Remove(specRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)Projectile)
			{
				SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
				specRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Remove(specRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
			}
		}
	}

	private const int NumWaves = 5;

	private const int NumBullets = 18;

	protected override IEnumerator Top()
	{
		float delta = 20f;
		for (int i = 0; i < 5; i++)
		{
			yield return Wait(33);
			int numBullets = 18;
			float startDirection = RandomAngle();
			if (i == 4)
			{
				numBullets /= 2;
				delta *= 2f;
			}
			for (int j = 0; j < numBullets; j++)
			{
				Fire(new Direction(startDirection + (float)j * delta), new Speed(12f), new ReflectBullet());
			}
			yield return Wait(12);
		}
	}
}
