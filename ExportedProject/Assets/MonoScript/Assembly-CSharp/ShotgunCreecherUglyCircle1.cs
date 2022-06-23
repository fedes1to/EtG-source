using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class ShotgunCreecherUglyCircle1 : Script
{
	public class CreecherBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(12f), 60);
			for (int i = 0; i < 60; i++)
			{
				if ((bool)Projectile)
				{
					Projectile.Speed = Speed;
					Projectile.UpdateSpeed();
				}
			}
			return null;
		}
	}

	private const int NumBulletNodes = 7;

	private const int NumBulletsPerNode = 2;

	protected override IEnumerator Top()
	{
		for (int i = 1; i <= 7; i++)
		{
			string transform = string.Format("shoot point {0}", i);
			for (int j = 0; j < 2; j++)
			{
				Fire(new Offset(transform), new Direction(RandomAngle()), new Speed(Random.Range(8, 12)), new CreecherBullet());
			}
		}
		return null;
	}
}
