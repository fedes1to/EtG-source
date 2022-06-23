using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class WizardPurpleHomingShots1 : Script
{
	public class StoryBullet : Bullet
	{
		public float horizontalOffset;

		public float OffsetAimDirection
		{
			get
			{
				Vector2 vector = BulletManager.PlayerPosition();
				Vector2 position = base.Position;
				Vector2 v = vector - position;
				Vector2 vector2 = ((!(v.magnitude < Mathf.Abs(horizontalOffset) * 2f)) ? (v.Rotate(90f).normalized * horizontalOffset) : Vector2.zero);
				return (vector + vector2 - position).ToAngle();
			}
		}

		public StoryBullet(string name, float horizontalOffset)
			: base(name)
		{
			this.horizontalOffset = horizontalOffset;
		}

		public bool HasLostTarget()
		{
			return WizardPurpleHomingShots1.HasLostTarget((Bullet)this);
		}
	}

	public class KnightBullet : StoryBullet
	{
		public KnightBullet()
			: base("knight", 1.5f)
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(60);
			Speed = 7f;
			Direction = base.OffsetAimDirection;
			for (int i = 0; i < 300; i++)
			{
				if (HasLostTarget())
				{
					Vanish();
					yield break;
				}
				ChangeDirection(new Direction(base.OffsetAimDirection, DirectionType.Absolute, 3f));
				if ((bool)Projectile)
				{
					Projectile.Direction = BraveMathCollege.DegreesToVector(Direction);
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class MageBullet : StoryBullet
	{
		private const int ShotCooldown = 60;

		private const int NumBullets = 3;

		public MageBullet()
			: base("mage", 0.5f)
		{
		}

		protected override IEnumerator Top()
		{
			tk2dSpriteAnimator spriteAnimator = Projectile.spriteAnimator;
			yield return Wait(60);
			Speed = 2f;
			Direction = base.OffsetAimDirection;
			int shotsFired = 0;
			int cooldown = 60;
			for (int i = 0; i < 300; i++)
			{
				if (HasLostTarget())
				{
					Vanish();
					yield break;
				}
				if (!spriteAnimator.Playing)
				{
					spriteAnimator.Play(spriteAnimator.DefaultClip);
				}
				if (shotsFired < 3)
				{
					cooldown--;
					if (cooldown == 12)
					{
						spriteAnimator.Play("enemy_projectile_mage_fire");
					}
					if (cooldown <= 0)
					{
						float num = BraveMathCollege.ClampAngle360(Direction);
						int num2 = ((!(num > 90f) || !(num < 270f)) ? 1 : (-1));
						Fire(new Offset(PhysicsEngine.PixelToUnit(new IntVector2(num2 * 5, 12)), 0f, string.Empty), new Direction(GetAimDirection(1f, 10f)), new Speed(10f), new Bullet("mage_fireball"));
						shotsFired++;
						cooldown = 60;
					}
				}
				else if (shotsFired == 3 && cooldown > 0)
				{
					cooldown--;
					if (cooldown == 0)
					{
						Speed = 7f;
					}
				}
				ChangeDirection(new Direction(base.OffsetAimDirection, DirectionType.Absolute, 3f));
				if ((bool)Projectile)
				{
					Projectile.Direction = BraveMathCollege.DegreesToVector(Direction);
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public class BardBullet : StoryBullet
	{
		private const int NoTurnTime = 60;

		private BounceProjModifier m_bounceMod;

		private int m_noTurnTimer;

		public BardBullet()
			: base("bard", -1f)
		{
		}

		protected override IEnumerator Top()
		{
			m_bounceMod = Projectile.GetComponent<BounceProjModifier>();
			m_bounceMod.OnBounce += OnBounce;
			yield return Wait(60);
			Speed = 7f;
			Direction = base.OffsetAimDirection;
			for (int i = 0; i < 300; i++)
			{
				if (HasLostTarget())
				{
					Vanish();
					yield break;
				}
				int turnSpeed = 3;
				if (m_noTurnTimer > 0)
				{
					turnSpeed = (int)Mathf.Lerp(0f, 3f, 1f - (float)m_noTurnTimer / 60f);
					m_noTurnTimer--;
				}
				BardBullet bardBullet = this;
				float offsetAimDirection = base.OffsetAimDirection;
				float maxFrameDelta = turnSpeed;
				bardBullet.ChangeDirection(new Direction(offsetAimDirection, DirectionType.Absolute, maxFrameDelta));
				if ((bool)Projectile)
				{
					Projectile.Direction = BraveMathCollege.DegreesToVector(Direction);
				}
				yield return Wait(1);
			}
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)m_bounceMod)
			{
				m_bounceMod.OnBounce -= OnBounce;
			}
		}

		private void OnBounce()
		{
			m_noTurnTimer = 60;
		}
	}

	public class RogueBullet : StoryBullet
	{
		private const int ClampedLifetime = 80;

		private TeleportProjModifier m_teleportMod;

		private bool m_clampLifetime;

		public RogueBullet()
			: base("rogue", -2f)
		{
		}

		protected override IEnumerator Top()
		{
			m_teleportMod = Projectile.GetComponent<TeleportProjModifier>();
			m_teleportMod.enabled = false;
			m_teleportMod.OnTeleport += OnTeleport;
			yield return Wait(60);
			Speed = 7f;
			Direction = base.OffsetAimDirection;
			m_teleportMod.enabled = true;
			int lifetime = 300;
			for (int i = 0; i < lifetime; i++)
			{
				if (HasLostTarget())
				{
					Vanish();
					yield break;
				}
				if (m_clampLifetime)
				{
					if (lifetime > i + 80)
					{
						lifetime = i + 80;
					}
					m_clampLifetime = false;
				}
				ChangeDirection(new Direction(base.OffsetAimDirection, DirectionType.Absolute, 3f));
				if ((bool)Projectile)
				{
					Projectile.Direction = BraveMathCollege.DegreesToVector(Direction);
				}
				yield return Wait(1);
			}
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)m_teleportMod)
			{
				m_teleportMod.OnTeleport -= OnTeleport;
			}
		}

		private void OnTeleport()
		{
			m_clampLifetime = true;
		}
	}

	private const int NumBullets = 3;

	private const int Delay = 45;

	private const int AirTime = 300;

	private int[] m_bullets = new int[4] { 0, 1, 2, 3 };

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		BraveUtility.RandomizeArray(m_bullets);
		yield return Wait(15);
		HealthHaver healthHaver = base.BulletBank.healthHaver;
		float startingHealth = healthHaver.GetCurrentHealth();
		for (int i = 0; i < 3; i++)
		{
			healthHaver.aiAnimator.PlayVfx("fire");
			yield return Wait(28);
			Bullet newBullet;
			switch (m_bullets[i])
			{
			case 0:
				newBullet = new BardBullet();
				break;
			case 1:
				newBullet = new KnightBullet();
				break;
			case 2:
				newBullet = new MageBullet();
				break;
			default:
				newBullet = new RogueBullet();
				break;
			}
			Fire(new Direction(90f), new Speed(1f), newBullet);
			for (int j = 0; j < 45; j++)
			{
				if (!healthHaver || healthHaver.IsDead || healthHaver.GetCurrentHealth() < startingHealth || HasLostTarget(this))
				{
					ForceEnd();
					yield break;
				}
				yield return Wait(1);
			}
		}
	}

	protected static bool HasLostTarget(Bullet bullet)
	{
		AIActor aiActor = bullet.BulletBank.aiActor;
		if ((bool)aiActor && !aiActor.TargetRigidbody && aiActor.CanTargetEnemies && !aiActor.CanTargetPlayers)
		{
			return true;
		}
		return false;
	}
}
