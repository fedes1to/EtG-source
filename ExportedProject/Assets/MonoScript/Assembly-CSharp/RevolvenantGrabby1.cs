using System;
using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class RevolvenantGrabby1 : Script
{
	private class ArmBullet : Bullet
	{
		private RevolvenantGrabby1 m_parentScript;

		private string m_armTransform;

		private int m_index;

		private float m_offsetAngle;

		public ArmBullet(RevolvenantGrabby1 parentScript, string armTransform, int i, float offsetAngle)
		{
			m_parentScript = parentScript;
			m_armTransform = armTransform;
			m_index = i;
			m_offsetAngle = offsetAngle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			float t3 = (float)m_index / 7f;
			Projectile.sprite.HeightOffGround = (1f - t3) * 10f;
			Projectile.sprite.UpdateZDepth();
			Projectile.specRigidbody.CollideWithTileMap = false;
			while (!m_parentScript.DoShrink)
			{
				if (!base.BulletBank || m_parentScript.Aborting || m_parentScript.Destroyed)
				{
					Vanish();
					yield break;
				}
				t3 = (float)m_index / 7f;
				base.Position = Vector2.MoveTowards(target: GetArmPosition(), current: base.Position, maxDistanceDelta: (2f + t3 * 12f) / 60f);
				yield return Wait(1);
			}
			for (int i = 0; i < 90; i++)
			{
				if (!base.BulletBank || m_parentScript.Aborting)
				{
					Vanish();
					yield break;
				}
				float radius = Mathf.Lerp(3f, 0f, (float)i / 90f);
				t3 = (float)m_index / 7f;
				base.Position = Vector2.MoveTowards(target: GetArmPosition(radius), current: base.Position, maxDistanceDelta: (2f + t3 * 12f) / 60f);
				yield return Wait(1);
			}
			int destroyOrder = 8 - m_index - 1;
			yield return Wait(destroyOrder * 4);
			Vanish(m_index < 4);
		}

		public Vector2 GetArmPosition(float circleRadius = 3f)
		{
			Vector2 vector = BulletManager.TransformOffset(m_parentScript.Position, m_armTransform);
			Vector2 playerPos = m_parentScript.PlayerPos;
			playerPos += (playerPos - m_parentScript.Position).Rotate(m_offsetAngle).normalized * circleRadius;
			float num = (float)m_index / 7f;
			Vector2 normalized = (playerPos - vector).Rotate(m_offsetAngle).normalized;
			normalized *= Mathf.Sin(num * (float)Math.PI) * 0.5f * Mathf.PingPong((float)(base.Tick + m_index * 3) / 75f, 1f);
			if ((bool)Projectile)
			{
				float num2 = BraveMathCollege.ClampAngle360((playerPos - vector).ToAngle());
				if ((m_offsetAngle < 0f && num2 > 90f && num2 < 210f) || (m_offsetAngle > 0f && num2 < 90f && num2 > -30f))
				{
					Projectile.sprite.HeightOffGround = 0f;
				}
				else
				{
					Projectile.sprite.HeightOffGround = (1f - num) * 10f;
				}
			}
			return Vector2.Lerp(vector, playerPos, num) + normalized;
		}
	}

	private class CircleBullet : Bullet
	{
		private RevolvenantGrabby1 m_parentScript;

		private float m_angle;

		private float m_desiredAngle;

		public CircleBullet(RevolvenantGrabby1 parentScript, float angle, float desiredAngle)
		{
			m_parentScript = parentScript;
			m_angle = angle;
			m_desiredAngle = desiredAngle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.specRigidbody.CollideWithTileMap = false;
			Projectile.BulletScriptSettings.surviveRigidbodyCollisions = true;
			while (!m_parentScript.DoShrink)
			{
				if (!base.BulletBank || m_parentScript.Aborting || m_parentScript.Destroyed)
				{
					Vanish();
					yield break;
				}
				Projectile.ResetDistance();
				Projectile.sprite.HeightOffGround = 4f;
				m_angle += 2f;
				m_desiredAngle += 2f;
				m_angle = Mathf.MoveTowardsAngle(m_angle, m_desiredAngle, 1f);
				base.Position = m_parentScript.PlayerPos + BraveMathCollege.DegreesToVector(m_angle, 3f);
				yield return Wait(1);
			}
			Vector2 origin = m_parentScript.PlayerPos;
			for (int i = 0; i < 90; i++)
			{
				if (m_parentScript.Aborting)
				{
					Vanish();
					yield break;
				}
				base.Position = origin + BraveMathCollege.DegreesToVector(magnitude: Mathf.Lerp(3f, 0f, (float)i / 90f), angle: m_angle);
				yield return Wait(1);
			}
			Vanish(true);
		}
	}

	private class HandBullet : Bullet
	{
		private enum State
		{
			Spin,
			Attack,
			Return
		}

		private const int AttackTime = 75;

		private const int ResetTime = 30;

		public float Angle;

		private State m_state;

		private bool m_hasDoneTell;

		private RevolvenantGrabby1 m_parentScript;

		private int m_stateChangeTimer;

		public HandBullet(RevolvenantGrabby1 parentScript, int initialAttackDelay)
			: base("hand")
		{
			m_parentScript = parentScript;
			m_stateChangeTimer = initialAttackDelay;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.specRigidbody.CollideWithTileMap = false;
			Projectile.BulletScriptSettings.surviveRigidbodyCollisions = true;
			yield return Wait(24);
			Vector2 returnStart = base.Position;
			Vector2 returnTarget = base.Position;
			while (!m_parentScript.Destroyed && !m_parentScript.DoShrink)
			{
				if (!base.BulletBank || m_parentScript.Aborting)
				{
					Vanish();
					yield break;
				}
				Projectile.ResetDistance();
				Projectile.sprite.HeightOffGround = 5f;
				m_stateChangeTimer--;
				if (m_state == State.Spin)
				{
					if (!m_hasDoneTell && m_stateChangeTimer < 30)
					{
						Projectile.spriteAnimator.Play();
						m_hasDoneTell = true;
					}
					if (m_stateChangeTimer <= 0)
					{
						m_state = State.Attack;
						m_stateChangeTimer = 75;
						Speed = 6f;
						Direction = (m_parentScript.PlayerPos - base.Position).ToAngle();
						base.ManualControl = false;
					}
					else
					{
						Angle += 2f;
						base.Position = m_parentScript.PlayerPos + BraveMathCollege.DegreesToVector(Angle, 3f);
					}
				}
				else if (m_state == State.Attack)
				{
					if (m_stateChangeTimer <= 0)
					{
						Projectile.spriteAnimator.StopAndResetFrameToDefault();
						returnStart = base.Position;
						returnTarget = (base.Position - m_parentScript.PlayerPos).normalized * 3f;
						m_state = State.Return;
						m_stateChangeTimer = 30;
						base.ManualControl = true;
					}
				}
				else if (m_state == State.Return)
				{
					if (m_stateChangeTimer <= 0)
					{
						base.Position = m_parentScript.PlayerPos + returnTarget;
						Angle = returnTarget.ToAngle();
						m_state = State.Spin;
						m_stateChangeTimer = 135;
					}
					else
					{
						base.Position = Vector2.Lerp(m_parentScript.PlayerPos + returnTarget, returnStart, (float)m_stateChangeTimer / 30f);
					}
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumArmBullets = 8;

	private const int ArmSpawnDelay = 3;

	private const int ArmDestroyTime = 4;

	private const int NumCircleBullets = 6;

	private const float CircleRadius = 3f;

	private const float CircleSpeed = 120f;

	private const int InitialHandAttackDelay = 30;

	private const int HandAttackTime = 120;

	private const int NumHandAttacks = 2;

	private ArmBullet firstLeftBullet;

	private ArmBullet firstRightBullet;

	private HandBullet leftHandBullet;

	private HandBullet rightHandBullet;

	public bool Aborting { get; set; }

	public bool NearDone { get; set; }

	public bool DoShrink { get; set; }

	public Vector2 PlayerPos { get; set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		ArmBullet lastLeftBullet = null;
		ArmBullet lastRightBullet = null;
		PlayerPos = BulletManager.PlayerPosition();
		for (int m = 0; m < 8; m++)
		{
			ArmBullet leftBullet = new ArmBullet(this, "left arm", m, -90f)
			{
				BulletManager = BulletManager
			};
			Fire(Offset.OverridePosition(leftBullet.GetArmPosition()), leftBullet);
			ArmBullet rightBullet = new ArmBullet(this, "right arm", m, 90f)
			{
				BulletManager = BulletManager
			};
			Fire(Offset.OverridePosition(rightBullet.GetArmPosition()), rightBullet);
			if (m == 0)
			{
				firstLeftBullet = leftBullet;
				firstRightBullet = rightBullet;
				leftHandBullet = new HandBullet(this, 120);
				Fire(Offset.OverridePosition(leftBullet.Position), leftHandBullet);
				rightHandBullet = new HandBullet(this, 240);
				Fire(Offset.OverridePosition(leftBullet.Position), rightHandBullet);
			}
			else
			{
				leftHandBullet.Position = leftBullet.Position;
				rightHandBullet.Position = rightBullet.Position;
				if (m == 7)
				{
					lastLeftBullet = leftBullet;
					lastRightBullet = rightBullet;
				}
			}
			for (int n = 0; n < 3; n++)
			{
				PlayerPos = BulletManager.PlayerPosition();
				yield return Wait(1);
			}
		}
		Vector2 pos3 = lastLeftBullet.GetArmPosition();
		float startAngle = (pos3 - PlayerPos).ToAngle();
		for (int l = 0; l < 6; l++)
		{
			if (ShouldAbort(false))
			{
				yield break;
			}
			pos3 = lastLeftBullet.GetArmPosition();
			float angle2 = (pos3 - PlayerPos).ToAngle();
			Fire(Offset.OverridePosition(pos3), new CircleBullet(this, angle2, startAngle));
			if (l == 0)
			{
				leftHandBullet.Position = pos3;
				leftHandBullet.Angle = angle2;
			}
			pos3 = lastRightBullet.GetArmPosition();
			angle2 = (pos3 - PlayerPos).ToAngle();
			Fire(Offset.OverridePosition(pos3), new CircleBullet(this, angle2, BraveMathCollege.ClampAngle360(startAngle + 180f)));
			if (l == 0)
			{
				rightHandBullet.Position = pos3;
				rightHandBullet.Angle = angle2;
			}
			if (l == 5)
			{
				lastLeftBullet.Vanish(true);
				lastRightBullet.Vanish(true);
			}
			for (int j2 = 0; j2 < 15; j2++)
			{
				PlayerPos = BulletManager.PlayerPosition();
				yield return Wait(1);
			}
		}
		int waitTime2 = 270;
		for (int k = 0; k < waitTime2; k++)
		{
			if (ShouldAbort())
			{
				yield break;
			}
			PlayerPos = BulletManager.PlayerPosition();
			yield return Wait(1);
		}
		leftHandBullet.Vanish();
		rightHandBullet.Vanish();
		PlayerPos = BulletManager.PlayerPosition();
		for (int j = 60; j > 0; j--)
		{
			if (ShouldAbort(false))
			{
				yield break;
			}
			PlayerPos = Vector2.MoveTowards(target: BulletManager.PlayerPosition(), current: PlayerPos, maxDistanceDelta: 7f / 60f * ((float)j / 60f));
			yield return Wait(1);
		}
		DoShrink = true;
		base.BulletBank.aiAnimator.LockFacingDirection = true;
		waitTime2 = 122;
		for (int i = 0; i < waitTime2; i++)
		{
			if (ShouldAbort(false))
			{
				base.BulletBank.aiAnimator.LockFacingDirection = false;
				yield break;
			}
			yield return Wait(1);
		}
		base.BulletBank.aiAnimator.LockFacingDirection = false;
	}

	private bool ShouldAbort(bool checkHands = true)
	{
		Aborting = firstLeftBullet.Destroyed || firstRightBullet.Destroyed;
		if (checkHands)
		{
			Aborting |= leftHandBullet.Destroyed && rightHandBullet.Destroyed;
		}
		return Aborting;
	}
}
