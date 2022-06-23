using System;
using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class ShotgunExecutionerChain1 : Script
{
	private class ArmBullet : Bullet
	{
		public const int BulletDelay = 60;

		private const float WiggleMagnitude = 0.4f;

		public const int WiggleTime = 30;

		private const int NumBulletsToPreShake = 5;

		private ShotgunExecutionerChain1 m_parentScript;

		private ShotgunExecutionerChain1 shotgunExecutionerChain1;

		private HandBullet m_handBullet;

		private int m_index;

		public ArmBullet(ShotgunExecutionerChain1 parentScript, HandBullet handBullet, int index)
			: base("chain")
		{
			m_parentScript = parentScript;
			m_handBullet = handBullet;
			m_index = index;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			while (!m_parentScript.IsEnded && !m_handBullet.IsEnded && !m_handBullet.HasStopped && (bool)base.BulletBank)
			{
				base.Position = Vector2.Lerp(m_parentScript.Position, m_handBullet.Position, (float)m_index / 20f);
				yield return Wait(1);
			}
			if (m_parentScript.IsEnded)
			{
				Vanish();
				yield break;
			}
			int delay = 20 - m_index - 5;
			if (delay > 0)
			{
				yield return Wait(delay);
			}
			float currentOffset = 0f;
			Vector2 truePosition = base.Position;
			int halfWiggleTime = 10;
			for (int i = 0; i < 30; i++)
			{
				if (i == 0 && delay < 0)
				{
					i = -delay;
				}
				float magnitude4 = 0.4f;
				magnitude4 = Mathf.Min(magnitude4, Mathf.Lerp(0.2f, 0.4f, (float)m_index / 8f));
				magnitude4 = Mathf.Min(magnitude4, Mathf.Lerp(0.2f, 0.4f, (float)(20 - m_index - 1) / 3f));
				magnitude4 = Mathf.Lerp(magnitude4, 0f, (float)i / (float)halfWiggleTime - 2f);
				base.Position = truePosition + BraveMathCollege.DegreesToVector(magnitude: Mathf.SmoothStep(0f - magnitude4, magnitude4, Mathf.PingPong(0.5f + (float)i / (float)halfWiggleTime, 1f)), angle: Direction - 90f);
				yield return Wait(1);
			}
			yield return Wait(m_index + 1 + 60);
			Vanish();
		}
	}

	private class HandBullet : Bullet
	{
		private ShotgunExecutionerChain1 m_parentScript;

		public bool HasStopped { get; set; }

		public HandBullet(ShotgunExecutionerChain1 parentScript)
			: base("chain")
		{
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			Projectile.BulletScriptSettings.surviveRigidbodyCollisions = true;
			Projectile.BulletScriptSettings.surviveTileCollisions = true;
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			while (!m_parentScript.IsEnded && !HasStopped)
			{
				yield return Wait(1);
			}
			if (m_parentScript.IsEnded)
			{
				Vanish();
				yield break;
			}
			yield return Wait(111);
			Vanish();
		}

		private void OnCollision(CollisionData collision)
		{
			bool flag = collision.collisionType == CollisionData.CollisionType.TileMap;
			SpeculativeRigidbody otherRigidbody = collision.OtherRigidbody;
			if ((bool)otherRigidbody)
			{
				flag = (bool)otherRigidbody.majorBreakable || otherRigidbody.PreventPiercing || ((!otherRigidbody.gameActor && !otherRigidbody.minorBreakable) ? true : false);
			}
			if (flag)
			{
				base.Position = collision.MyRigidbody.UnitCenter + PhysicsEngine.PixelToUnit(collision.NewPixelsToMove);
				Speed = 0f;
				HasStopped = true;
				PhysicsEngine.PostSliceVelocity = new Vector2(0f, 0f);
				SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
				specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			}
			else
			{
				PhysicsEngine.PostSliceVelocity = collision.MyRigidbody.Velocity;
			}
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)Projectile)
			{
				SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
				specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			}
			HasStopped = true;
		}
	}

	private const int NumArmBullets = 20;

	private const int NumVolley = 3;

	private const int FramesBetweenVolleys = 30;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		HandBullet handBullet = null;
		for (int i = 0; i < 3; i++)
		{
			handBullet = FireVolley(i, 20 + i * 5);
			if (i < 2)
			{
				yield return Wait(30);
			}
		}
		while (!handBullet.IsEnded && !handBullet.HasStopped)
		{
			yield return Wait(1);
		}
		yield return Wait(120);
	}

	private HandBullet FireVolley(int volleyIndex, float speed)
	{
		HandBullet handBullet = new HandBullet(this);
		Fire(new Direction(base.AimDirection), new Speed(speed), handBullet);
		for (int i = 0; i < 20; i++)
		{
			Fire(new Direction(base.AimDirection), new ArmBullet(this, handBullet, i));
		}
		return handBullet;
	}
}
