using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Rocket1")]
public class DraGunRocket1 : Script
{
	public class Rocket : Bullet
	{
		public Rocket()
			: base("rocket")
		{
		}

		protected override IEnumerator Top()
		{
			return null;
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			for (int i = 0; i < 42; i++)
			{
				Fire(new Direction(SubdivideArc(-10f, 200f, 42, i)), new Speed(12f), new Bullet("default_novfx"));
				if (i < 41)
				{
					Fire(new Direction(SubdivideArc(-10f, 200f, 42, i, true)), new Speed(8f), new SpeedChangingBullet("default_novfx", 12f, 60));
				}
				Fire(new Direction(SubdivideArc(-10f, 200f, 42, i)), new Speed(4f), new SpeedChangingBullet("default_novfx", 12f, 60));
			}
			for (int j = 0; j < 12; j++)
			{
				Fire(new Direction(Random.Range(20f, 160f)), new Speed(Random.Range(10f, 16f)), new ShrapnelBullet());
			}
		}
	}

	public class ShrapnelBullet : Bullet
	{
		private const float WiggleMagnitude = 0.75f;

		private const float WigglePeriod = 3f;

		public ShrapnelBullet()
			: base("shrapnel")
		{
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			yield return Wait(Random.Range(0, 10));
			Vector2 truePosition = base.Position;
			float trueDirection = Direction;
			for (int i = 0; i < 360; i++)
			{
				float offsetMagnitude = Mathf.SmoothStep(-0.75f, 0.75f, Mathf.PingPong(0.5f + (float)i / 60f * 3f, 1f));
				Vector2 lastPosition = truePosition;
				truePosition += BraveMathCollege.DegreesToVector(trueDirection, Speed / 60f);
				base.Position = truePosition + BraveMathCollege.DegreesToVector(trueDirection - 90f, offsetMagnitude);
				Direction = (truePosition - lastPosition).ToAngle();
				Projectile.transform.rotation = Quaternion.Euler(0f, 0f, Direction);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 42;

	protected override IEnumerator Top()
	{
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			if (Random.value < 0.5f)
			{
				Fire(new Direction(-60f), new Speed(40f), new Rocket());
				Fire(new Direction(-120f), new Speed(20f), new Rocket());
			}
			else
			{
				Fire(new Direction(-60f), new Speed(20f), new Rocket());
				Fire(new Direction(-120f), new Speed(40f), new Rocket());
			}
		}
		else
		{
			Fire(new Direction(-90f), new Speed(40f), new Rocket());
		}
		return null;
	}
}
