using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class ShopkeepBlast1 : Script
{
	private class BurstBullet : Bullet
	{
		private float m_newSpeed;

		private int m_term;

		public BurstBullet(string name, float newSpeed, int term, bool suppressVfx)
			: base(name, suppressVfx)
		{
			m_newSpeed = newSpeed;
			m_term = term;
		}

		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(m_newSpeed), m_term);
			return null;
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float num = 22.5f;
				float num2 = Random.Range((0f - num) / 2f, num / 2f);
				for (int i = 0; i < 16; i++)
				{
					Fire(new Direction(num2 + (float)i * num, DirectionType.Relative), new Speed(9f), new Bullet(null, true));
				}
			}
		}
	}

	private const int NumBulletsInBurst = 16;

	protected override IEnumerator Top()
	{
		FireBurst("left barrel");
		FireBurst("right barrel");
		QuadShot(base.AimDirection + Random.Range(-60f, 60f), (!BraveUtility.RandomBool()) ? "right barrel" : "left barrel", Random.Range(9f, 11f));
		return null;
	}

	private void FireBurst(string transform)
	{
		float num = 22.5f;
		float num2 = Random.Range((0f - num) / 2f, num / 2f);
		for (int i = 0; i < 16; i++)
		{
			Offset offset = new Offset(transform);
			Direction direction = new Direction(num2 + (float)i * num, DirectionType.Relative);
			Speed speed = new Speed(9f);
			bool suppressVfx = i > 0;
			Fire(offset, direction, speed, new Bullet(null, suppressVfx));
		}
	}

	private void QuadShot(float direction, string transform, float speed)
	{
		for (int i = 0; i < 4; i++)
		{
			Fire(bullet: (i != 0) ? ((Bullet)new SpeedChangingBullet("bigBullet", speed, 120, -1, true)) : ((Bullet)new BurstBullet("burstBullet", speed, 120, true)), offset: new Offset(transform), direction: new Direction(direction), speed: new Speed(speed - (float)i * 1.5f));
		}
	}
}
