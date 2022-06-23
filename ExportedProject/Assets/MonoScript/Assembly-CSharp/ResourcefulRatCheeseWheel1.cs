using System.Collections;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/ResourcefulRat/CheeseWheel1")]
public class ResourcefulRatCheeseWheel1 : Script
{
	public class CheeseWedgeBullet : Bullet
	{
		private ResourcefulRatCheeseWheel1 m_parent;

		private Vector2 m_targetPos;

		private float m_endingAngle;

		private bool m_isMisfire;

		private float m_additionalRampHeight;

		public CheeseWedgeBullet(ResourcefulRatCheeseWheel1 parent, string bulletName, float additionalRampHeight, Vector2 targetPos, float endingAngle, bool isMisfire)
			: base(bulletName, true)
		{
			m_parent = parent;
			m_targetPos = targetPos;
			m_endingAngle = endingAngle;
			m_isMisfire = isMisfire;
			m_additionalRampHeight = additionalRampHeight;
		}

		protected override IEnumerator Top()
		{
			int travelTime = Random.RandomRange(90, 136);
			Projectile.IgnoreTileCollisionsFor(90f);
			Projectile.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.LowObstacle));
			Projectile.sprite.HeightOffGround = 10f + m_additionalRampHeight + Random.value / 2f;
			Projectile.sprite.ForceRotationRebuild();
			Projectile.sprite.UpdateZDepth();
			int r = Random.Range(0, 20);
			yield return Wait(15 + r);
			Speed = 2.5f;
			yield return Wait(50 - r);
			Speed = 0f;
			if (m_isMisfire)
			{
				Direction += 180f;
				Speed = 2.5f;
				yield return Wait(180);
				Vanish(true);
				yield break;
			}
			Direction = (m_targetPos - base.Position).ToAngle();
			ChangeSpeed(new Speed((m_targetPos - base.Position).magnitude / ((float)(travelTime - 15) / 60f)), 30);
			yield return Wait(travelTime);
			Speed = 0f;
			base.Position = m_targetPos;
			Direction = m_endingAngle;
			if ((bool)Projectile && (bool)Projectile.sprite)
			{
				Projectile.sprite.HeightOffGround -= 1f;
				Projectile.sprite.UpdateZDepth();
			}
			int totalTime = 350;
			yield return Wait(totalTime - m_parent.Tick);
			Vanish(true);
		}
	}

	public class CheeseWheelBullet : Bullet
	{
		public CheeseWheelBullet()
			: base("cheeseWheel", true)
		{
		}

		protected override IEnumerator Top()
		{
			Projectile.spriteAnimator.Play("cheese_wheel_burst");
			Projectile.ImmuneToSustainedBlanks = true;
			yield return Wait(45);
			Projectile.Ramp(-1.5f, 100f);
			yield return Wait(80);
			for (int i = 0; i < 80; i++)
			{
				Bullet bullet = new Bullet("cheese", true);
				Fire(new Direction(RandomAngle()), new Speed(Random.Range(12f, 33f)), bullet);
				bullet.Projectile.ImmuneToSustainedBlanks = true;
			}
			if ((bool)base.BulletBank)
			{
				ResourcefulRatController component = base.BulletBank.GetComponent<ResourcefulRatController>();
				if ((bool)component)
				{
					GameManager.Instance.MainCameraController.DoScreenShake(component.cheeseSlamScreenShake, null);
				}
			}
			yield return Wait(25);
			Vanish(true);
		}
	}

	private const float WallInset = 1.25f;

	private const int WallInsetTime = 40;

	private const int WallInsetTimeVariation = 20;

	private const int NumVerticalWallBullets = 20;

	private const int NumHorizontalWallBullets = 20;

	private const int MisfireBullets = 5;

	private const int TargetPoints = 8;

	private const float TargetAngleDelta = 45f;

	private const float TargetOffset = 0.875f;

	private const int MinTravelTime = 90;

	private const int MaxTravelTime = 135;

	private const int WaveDelay = 75;

	private const int NumWaves = 3;

	private static string[] TargetNames = new string[8] { "cheeseWedge0", "cheeseWedge1", "cheeseWedge2", "cheeseWedge3", "cheeseWedge4", "cheeseWedge5", "cheeseWedge6", "cheeseWedge7" };

	private static float[] RampHeights = new float[8] { 2f, 1f, 0f, 1f, 2f, 3f, 4f, 2f };

	private static Vector2[] TargetOffsets = new Vector2[8]
	{
		new Vector2(0f, 0.0625f),
		new Vector2(0.0625f, -0.0625f),
		new Vector2(0.0625f, 0f),
		new Vector2(0.0625f, -0.0625f),
		new Vector2(0.0625f, 0.0625f),
		new Vector2(0f, 0f),
		new Vector2(0.0625f, 0f),
		new Vector2(0.125f, -0.125f)
	};

	protected override IEnumerator Top()
	{
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		Vector2 roomLowerLeft = area.UnitBottomLeft;
		Vector2 roomUpperRight = area.UnitTopRight - new Vector2(0f, 3f);
		Vector2 roomCenter = area.UnitCenter - new Vector2(0f, 2.5f);
		PostWwiseEvent("Play_BOSS_Rat_Cheese_Summon_01");
		for (int i = 0; i < 3; i++)
		{
			int misfireIndex = Random.Range(0, 15);
			for (int j = 0; j < 20; j++)
			{
				Vector2 spawnPos = new Vector2(roomLowerLeft.x, SubdivideRange(roomLowerLeft.y, roomUpperRight.y, 21, j, true));
				spawnPos += new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f));
				spawnPos.x -= 1.25f;
				bool isMisfire = j >= misfireIndex && j < misfireIndex + 5;
				FireWallBullet(0f, spawnPos, roomCenter, isMisfire);
			}
			misfireIndex = Random.Range(0, 15);
			for (int k = 0; k < 20; k++)
			{
				Vector2 spawnPos2 = new Vector2(SubdivideRange(roomLowerLeft.x, roomUpperRight.x, 21, k, true), roomUpperRight.y);
				spawnPos2 += new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f));
				spawnPos2.y += 3.25f;
				bool isMisfire2 = k >= misfireIndex && k < misfireIndex + 5;
				FireWallBullet(-90f, spawnPos2, roomCenter, isMisfire2);
			}
			misfireIndex = Random.Range(0, 15);
			for (int l = 0; l < 20; l++)
			{
				Vector2 spawnPos3 = new Vector2(roomUpperRight.x, SubdivideRange(roomLowerLeft.y, roomUpperRight.y, 21, l, true));
				spawnPos3 += new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f));
				spawnPos3.x += 1.25f;
				bool isMisfire3 = l >= misfireIndex && l < misfireIndex + 5;
				FireWallBullet(180f, spawnPos3, roomCenter, isMisfire3);
			}
			misfireIndex = Random.Range(0, 15);
			for (int m = 0; m < 20; m++)
			{
				Vector2 spawnPos4 = new Vector2(SubdivideRange(roomLowerLeft.x, roomUpperRight.x, 21, m, true), roomLowerLeft.y);
				spawnPos4 += new Vector2(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f));
				spawnPos4.y -= 1.25f;
				bool isMisfire4 = m >= misfireIndex && m < misfireIndex + 5;
				FireWallBullet(90f, spawnPos4, roomCenter, isMisfire4);
			}
			if (i == 2)
			{
				base.EndOnBlank = true;
			}
			yield return Wait(75);
		}
		yield return Wait(125);
		AIActor aiActor = base.BulletBank.aiActor;
		aiActor.aiAnimator.PlayUntilFinished("cheese_wheel_out");
		aiActor.IsGone = true;
		aiActor.specRigidbody.CollideWithOthers = false;
		Fire(Offset.OverridePosition(roomCenter), new Speed(), new CheeseWheelBullet());
		yield return Wait(65);
		aiActor.IsGone = false;
		aiActor.specRigidbody.CollideWithOthers = true;
		yield return Wait(105);
	}

	public override void OnForceEnded()
	{
		AIActor aiActor = base.BulletBank.aiActor;
		aiActor.IsGone = false;
		aiActor.specRigidbody.CollideWithOthers = true;
	}

	private void FireWallBullet(float facingDir, Vector2 spawnPos, Vector2 roomCenter, bool isMisfire)
	{
		float angleDeg = (spawnPos - roomCenter).ToAngle();
		int num = Mathf.RoundToInt(BraveMathCollege.ClampAngle360(angleDeg) / 45f) % 8;
		float num2 = (float)num * 45f;
		Vector2 targetPos = (roomCenter + BraveMathCollege.DegreesToVector(num2, 0.875f) + TargetOffsets[num]).Quantize(0.0625f);
		Fire(Offset.OverridePosition(spawnPos), new Direction(facingDir), new Speed(), new CheeseWedgeBullet(this, TargetNames[num], RampHeights[num], targetPos, num2 + 180f, isMisfire));
	}
}
