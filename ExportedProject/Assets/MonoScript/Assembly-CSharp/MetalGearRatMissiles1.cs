using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MetalGearRat/Missiles1")]
public class MetalGearRatMissiles1 : Script
{
	private class HomingBullet : Bullet
	{
		private int m_fireDelay;

		public HomingBullet(int fireDelay = 0)
			: base("missile")
		{
			m_fireDelay = fireDelay;
		}

		public override void Initialize()
		{
			Projectile.spriteAnimator.StopAndResetFrameToDefault();
			BraveUtility.EnableEmission(Projectile.ParticleTrail, false);
			base.Initialize();
		}

		protected override IEnumerator Top()
		{
			if (m_fireDelay > 0)
			{
				yield return Wait(m_fireDelay);
			}
			Projectile.spriteAnimator.Play();
			BraveUtility.EnableEmission(Projectile.ParticleTrail, true);
			PostWwiseEvent("Play_BOSS_RatMech_Missile_01");
			PostWwiseEvent("Play_WPN_YariRocketLauncher_Shot_01");
			float t = UnityEngine.Random.value;
			Speed = Mathf.Lerp(8f, 14f, t);
			Vector2 toTarget = base.BulletBank.PlayerPosition() - base.Position;
			float travelTime = toTarget.magnitude / Speed * 60f - 1f;
			float magnitude = BraveUtility.RandomSign() * (1f - t) * 8f;
			Vector2 offset = magnitude * toTarget.Rotate(90f).normalized;
			base.ManualControl = true;
			int startTick = base.Tick;
			Vector2 truePosition = base.Position;
			Vector2 lastPosition = base.Position;
			Vector2 velocity = toTarget.normalized * Speed;
			for (int i = 0; (float)i < travelTime; i++)
			{
				truePosition += velocity / 60f;
				lastPosition = base.Position;
				base.Position = truePosition + offset * Mathf.Sin((float)(base.Tick - startTick) / travelTime * (float)Math.PI);
				Direction = (base.Position - lastPosition).ToAngle();
				yield return Wait(1);
			}
			Vector2 v = (base.Position - lastPosition) * 60f;
			Speed = v.magnitude;
			Direction = v.ToAngle();
			base.ManualControl = false;
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float num = RandomAngle();
				float num2 = 45f;
				for (int i = 0; i < 8; i++)
				{
					Fire(new Direction(num + num2 * (float)i), new Speed(11f));
					PostWwiseEvent("Play_WPN_smallrocket_impact_01");
				}
			}
		}
	}

	private const int NumDeathBullets = 8;

	private static int[] xOffsets = new int[8] { 0, -4, -7, -11, -14, -18, -21, -28 };

	private static int[] yOffsets = new int[8] { 0, -7, 0, -7, 0, -7, 0, 0 };

	protected override IEnumerator Top()
	{
		int leftDelay = 0;
		int rightDelay = 60;
		if (BraveUtility.RandomBool())
		{
			BraveUtility.Swap(ref leftDelay, ref rightDelay);
		}
		Vector2 leftBasePos = base.BulletBank.GetTransform("missile left shoot point").position;
		Vector2 rightBasePos = base.BulletBank.GetTransform("missile right shoot point").position;
		int[] leftDelays = new int[xOffsets.Length];
		int[] rightDelays = new int[xOffsets.Length];
		for (int j = 0; j < xOffsets.Length; j++)
		{
			leftDelays[j] = 30 + j * 30;
			rightDelays[j] = 30 + j * 30 + 15;
		}
		BraveUtility.RandomizeArray(leftDelays);
		BraveUtility.RandomizeArray(rightDelays);
		int delay = 30;
		for (int i = 0; i < xOffsets.Length; i++)
		{
			int dx = xOffsets[xOffsets.Length - 1 - i];
			int dy = yOffsets[xOffsets.Length - 1 - i];
			Vector2 spawnPos2 = leftBasePos + PhysicsEngine.PixelToUnit(new IntVector2(dx + 1, dy - 7));
			Fire(Offset.OverridePosition(spawnPos2), new Direction(-90f), new HomingBullet(leftDelays[i] - i * 4));
			spawnPos2 = rightBasePos + PhysicsEngine.PixelToUnit(new IntVector2(-dx + 1, dy - 7));
			Fire(Offset.OverridePosition(spawnPos2), new Direction(-90f), new HomingBullet(rightDelays[i] - i * 4));
			yield return Wait(4);
		}
		yield return Wait(220);
	}
}
