using System;
using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class RevolvenantPunch1 : Script
{
	private class ArmBullet : Bullet
	{
		public const int NumBullets = 3;

		public const int BulletDelay = 60;

		private const float WiggleMagnitude = 0.4f;

		private const int WiggleTime = 30;

		private const int NumBulletsToPreShake = 5;

		private RevolvenantPunch1 m_parentScript;

		private RevolvenantPunch1 revolvenantPunch1;

		private HandBullet m_handBullet;

		private int m_index;

		public ArmBullet(RevolvenantPunch1 parentScript, HandBullet handBullet, int index)
		{
			m_parentScript = parentScript;
			m_handBullet = handBullet;
			m_index = index;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			while (!m_parentScript.IsEnded && !m_parentScript.Spew && (bool)base.BulletBank)
			{
				base.Position = Vector2.Lerp(m_parentScript.ArmPosition, m_handBullet.Position, (float)m_index / 20f);
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
			for (int j = 0; j < 30; j++)
			{
				if (j == 0 && delay < 0)
				{
					j = -delay;
				}
				float magnitude4 = 0.4f;
				magnitude4 = Mathf.Min(magnitude4, Mathf.Lerp(0.2f, 0.4f, (float)m_index / 8f));
				magnitude4 = Mathf.Min(magnitude4, Mathf.Lerp(0.2f, 0.4f, (float)(20 - m_index - 1) / 3f));
				magnitude4 = Mathf.Lerp(magnitude4, 0f, (float)j / (float)halfWiggleTime - 2f);
				base.Position = truePosition + BraveMathCollege.DegreesToVector(magnitude: Mathf.SmoothStep(0f - magnitude4, magnitude4, Mathf.PingPong(0.5f + (float)j / (float)halfWiggleTime, 1f)), angle: Direction - 90f);
				yield return Wait(1);
			}
			yield return Wait(m_index + 1);
			yield return Wait(UnityEngine.Random.Range(0, 60));
			for (int i = 0; i < 3; i++)
			{
				if (m_parentScript.IsEnded || !m_parentScript.BulletBank || m_parentScript.BulletBank.healthHaver.IsDead || m_parentScript.BulletBank.aiActor.IsFrozen)
				{
					base.ManualControl = false;
					Direction += BraveUtility.RandomSign() * UnityEngine.Random.Range(60f, 120f);
					Speed = 8f;
					yield break;
				}
				Fire(new Direction(Direction + BraveUtility.RandomSign() * UnityEngine.Random.Range(60f, 120f)), new Speed(12f));
				yield return Wait(60);
			}
			base.ManualControl = false;
			Direction = RandomAngle();
			Speed = 9f;
		}
	}

	private class HandBullet : Bullet
	{
		private RevolvenantPunch1 m_parentScript;

		public bool HasStopped { get; set; }

		public HandBullet(RevolvenantPunch1 parentScript)
			: base("hand")
		{
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			Projectile.BulletScriptSettings.surviveRigidbodyCollisions = true;
			Projectile.BulletScriptSettings.surviveTileCollisions = true;
			SpeculativeRigidbody specRigidbody = Projectile.specRigidbody;
			specRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(specRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
			while (!m_parentScript.IsEnded && (bool)m_parentScript.BulletBank && !m_parentScript.BulletBank.healthHaver.IsDead && !m_parentScript.BulletBank.aiActor.IsFrozen)
			{
				yield return Wait(1);
			}
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

	public bool Spew { get; set; }

	public Vector2 ArmPosition { get; set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		string transform = ((!(BraveMathCollege.AbsAngleBetween(base.BulletBank.aiAnimator.FacingDirection, 0f) > 90f)) ? "left arm" : "right arm");
		float direction = GetAimDirection(transform);
		base.BulletBank.aiAnimator.FacingDirection = direction;
		ArmPosition = base.BulletBank.TransformOffset(base.Position, transform);
		HandBullet handBullet = new HandBullet(this);
		Fire(Offset.OverridePosition(ArmPosition), new Direction(direction), new Speed(40f), handBullet);
		for (int i = 0; i < 20; i++)
		{
			Fire(Offset.OverridePosition(ArmPosition), new Direction(direction), new ArmBullet(this, handBullet, i));
		}
		while (!handBullet.IsEnded && !handBullet.HasStopped)
		{
			yield return Wait(1);
		}
		Spew = true;
		yield return Wait(240);
	}
}
