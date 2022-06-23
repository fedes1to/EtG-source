using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MetalGearRat/Tailgun1")]
public class MetalGearRatTailgun1 : Script
{
	public class TargetDummy : Bullet
	{
		protected override IEnumerator Top()
		{
			while (true)
			{
				float distToTarget = (BulletManager.PlayerPosition() - base.Position).magnitude;
				if (base.Tick < 30)
				{
					Speed = 0f;
				}
				else
				{
					float a = Mathf.Lerp(12f, 4f, Mathf.InverseLerp(7f, 4f, distToTarget));
					Speed = Mathf.Min(a, (float)(base.Tick - 30) / 60f * 10f);
				}
				ChangeDirection(new Direction(0f, DirectionType.Aim, 3f));
				yield return Wait(1);
			}
		}
	}

	public class TargetBullet : Bullet
	{
		private MetalGearRatTailgun1 m_parent;

		private TargetDummy m_targetDummy;

		public TargetBullet(MetalGearRatTailgun1 parent, TargetDummy targetDummy)
			: base("target")
		{
			m_parent = parent;
			m_targetDummy = targetDummy;
		}

		protected override IEnumerator Top()
		{
			Vector2 toCenter = base.Position - m_targetDummy.Position;
			float angle = toCenter.ToAngle();
			float radius = toCenter.magnitude;
			float deltaRadius = radius / 60f;
			base.ManualControl = true;
			Projectile.specRigidbody.CollideWithTileMap = false;
			Projectile.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.LowObstacle));
			while (!m_parent.Destroyed && !m_parent.IsEnded && !m_parent.Done)
			{
				if (base.Tick < 60)
				{
					radius += deltaRadius * 3f;
				}
				if (m_parent.Center)
				{
					radius -= deltaRadius * 2f;
				}
				angle += 1.33333337f;
				base.Position = m_targetDummy.Position + BraveMathCollege.DegreesToVector(angle, radius);
				yield return Wait(1);
			}
			Vanish();
			PostWwiseEvent("Play_BOSS_RatMech_Bomb_01");
		}
	}

	private class BigBullet : Bullet
	{
		public BigBullet()
			: base("big_one")
		{
		}

		public override void Initialize()
		{
			Projectile.spriteAnimator.StopAndResetFrameToDefault();
			base.Initialize();
		}

		protected override IEnumerator Top()
		{
			Projectile.specRigidbody.CollideWithTileMap = false;
			Projectile.specRigidbody.CollideWithOthers = false;
			yield return Wait(60);
			Speed = 0f;
			Projectile.spriteAnimator.Play();
			float startingAngle = RandomAngle();
			for (int i = 0; i < 4; i++)
			{
				bool flag = i % 2 == 0;
				for (int j = 0; j < 39; j++)
				{
					BigBullet bigBullet = this;
					float startAngle = startingAngle;
					int numBullets = 39;
					int i2 = j;
					bool offset = flag;
					float direction = bigBullet.SubdivideCircle(startAngle, numBullets, i2, 1f, offset);
					Fire(new Direction(direction), new Speed(), new SpeedChangingBullet(10f, 17 * i));
				}
			}
			yield return Wait(30);
			Vanish(true);
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
			}
		}
	}

	private const int NumTargetBullets = 16;

	private const float TargetRadius = 3f;

	private const float TargetLegLength = 2.5f;

	public const int TargetTrackTime = 360;

	private const float TargetRotationSpeed = 80f;

	private const int BigOneHeight = 30;

	private const int NumDeathWaves = 4;

	private const int NumDeathBullets = 39;

	private bool Center { get; set; }

	private bool Done { get; set; }

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		TargetDummy targetDummy = new TargetDummy();
		targetDummy.Position = base.BulletBank.aiActor.ParentRoom.area.UnitCenter + new Vector2(0f, 4.5f);
		targetDummy.Direction = base.AimDirection;
		targetDummy.BulletManager = BulletManager;
		targetDummy.Initialize();
		for (int j = 0; j < 16; j++)
		{
			float angle = SubdivideCircle(0f, 16, j);
			Vector2 overridePosition = targetDummy.Position + BraveMathCollege.DegreesToVector(angle, 0.75f);
			Fire(Offset.OverridePosition(overridePosition), new TargetBullet(this, targetDummy));
		}
		Fire(Offset.OverridePosition(targetDummy.Position), new TargetBullet(this, targetDummy));
		for (int k = 0; k < 4; k++)
		{
			float angle2 = k * 90;
			for (int l = 1; l < 4; l++)
			{
				float magnitude = 0.75f + Mathf.Lerp(0f, 0.625f, (float)l / 3f);
				Vector2 overridePosition2 = targetDummy.Position + BraveMathCollege.DegreesToVector(angle2, magnitude);
				Fire(Offset.OverridePosition(overridePosition2), new TargetBullet(this, targetDummy));
			}
		}
		for (int i = 0; i < 360; i++)
		{
			targetDummy.DoTick();
			yield return Wait(1);
		}
		Fire(Offset.OverridePosition(targetDummy.Position + new Vector2(0f, 30f)), new Direction(-90f), new Speed(30f), new BigBullet());
		PostWwiseEvent("Play_BOSS_RatMech_Whistle_01");
		Center = true;
		yield return Wait(60);
		Done = true;
		yield return Wait(60);
	}
}
