using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("GunNut/SpectreCone1")]
public class GunNutSpectreCone1 : Script
{
	public class BurstingBullet : Bullet
	{
		private const int NumBullets = 18;

		private bool m_isBlackPhantom;

		public BurstingBullet(bool isBlackPhantom)
			: base("bigBullet")
		{
			base.ForceBlackBullet = true;
			m_isBlackPhantom = isBlackPhantom;
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (preventSpawningProjectiles)
			{
				return;
			}
			float num = RandomAngle();
			float num2 = 20f;
			for (int i = 0; i < 18; i++)
			{
				Bullet bullet = new Bullet();
				Fire(new Direction(num + (float)i * num2), new Speed(7f), bullet);
				if (!m_isBlackPhantom)
				{
					bullet.ForceBlackBullet = false;
					bullet.Projectile.ForceBlackBullet = false;
					bullet.Projectile.ReturnFromBlackBullet();
				}
			}
		}
	}

	private const int NumBulletsMainWave = 25;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 25; i++)
		{
			float num = -45f + (float)i * 3.75f;
			Fire(new Offset(0.5f, 0f, Direction + num, string.Empty), new Direction(num, DirectionType.Relative), new Speed(10f));
		}
		float angle = Mathf.MoveTowardsAngle(target: GetAimDirection(1f, 12f), current: Direction, maxDelta: 40f);
		bool isBlackPhantom = (bool)base.BulletBank && (bool)base.BulletBank.aiActor && base.BulletBank.aiActor.IsBlackPhantom;
		Fire(bullet: new BurstingBullet(isBlackPhantom), direction: new Direction(angle), speed: new Speed(12f));
		yield return null;
	}
}
