using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Rocket2")]
public class DraGunRocket2 : Script
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
			for (int j = 0; j < 5; j++)
			{
				Fire(new Offset(new Vector2(0f, -1f), 0f, string.Empty), new Direction(180f), new Speed(16 - j * 4), new SpeedChangingBullet("default_novfx", 12f, 60));
				Fire(new Offset(new Vector2(0f, -1f), 0f, string.Empty), new Direction(), new Speed(16 - j * 4), new SpeedChangingBullet("default_novfx", 12f, 60));
			}
			for (int k = 0; k < 12; k++)
			{
				float direction = ((k % 2 != 0) ? Random.Range(0f, 35f) : Random.Range(150f, 182f));
				Fire(new Direction(direction), new Speed(Random.Range(4f, 12f)), new ShrapnelBullet());
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
			ChangeSpeed(new Speed(12f), 60);
			BounceProjModifier bounce = Projectile.GetComponent<BounceProjModifier>();
			bool hasBounced = false;
			base.ManualControl = true;
			yield return Wait(Random.Range(0, 10));
			Vector2 truePosition = base.Position;
			float trueDirection = Direction;
			for (int i = 0; i < 360; i++)
			{
				if (!hasBounced && bounce.numberOfBounces == 0)
				{
					trueDirection = BraveMathCollege.QuantizeFloat(trueDirection, 90f) + 180f;
					Speed = 18f;
					hasBounced = true;
				}
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
		Fire(new Direction(-90f), new Speed(40f), new Rocket());
		return null;
	}
}
