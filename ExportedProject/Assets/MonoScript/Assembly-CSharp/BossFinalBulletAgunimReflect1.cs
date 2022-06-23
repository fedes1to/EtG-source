using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalBullet/AgunimReflect1")]
public class BossFinalBulletAgunimReflect1 : Script
{
	public class RingBullet : Bullet
	{
		private float m_angle;

		public RingBullet(float angle = 0f)
			: base("ring")
		{
			m_angle = angle;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.IgnoreTileCollisionsFor(0.6f);
			Vector2 centerPosition = base.Position;
			for (int i = 0; i < 300; i++)
			{
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				m_angle += 7.5f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(m_angle, 0.55f);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const float FakeChance = 0.33f;

	private static bool WasLastShotFake;

	private const int FakeNumBullets = 5;

	private const float FakeRadius = 0.55f;

	private const float FakeSpinSpeed = 450f;

	protected override IEnumerator Top()
	{
		yield return Wait(48);
		if (!WasLastShotFake && Random.value < 0.33f)
		{
			WasLastShotFake = true;
			for (int i = 0; i < 5; i++)
			{
				RingBullet ringBullet = new RingBullet(SubdivideCircle(0f, 5, i));
				Fire(new Direction(base.AimDirection), new Speed(10f), ringBullet);
				ringBullet.Projectile.IgnoreTileCollisionsFor(0.6f);
			}
			yield return Wait(60);
			yield break;
		}
		WasLastShotFake = false;
		Bullet bullet = new Bullet("reflect");
		Fire(new Direction(0f, DirectionType.Aim), new Speed(12f), bullet);
		bullet.Projectile.IsReflectedBySword = true;
		bullet.Projectile.IgnoreTileCollisionsFor(0.6f);
		do
		{
			yield return Wait(1);
		}
		while ((bool)bullet.Projectile && !bullet.Destroyed);
		yield return Wait(24);
	}
}
