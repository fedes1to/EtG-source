using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class MimicBlackMiniguns1 : Script
{
	private const int NumBursts = 10;

	private const int NumBulletsInBurst = 16;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 10; i++)
		{
			FireBurst((i % 2 != 0) ? "right gun" : "left gun");
			if (i % 3 == 2)
			{
				yield return Wait(6);
				QuadShot(base.AimDirection + Random.Range(-60f, 60f), (!BraveUtility.RandomBool()) ? "right gun" : "left gun", Random.Range(9f, 11f));
				yield return Wait(6);
			}
			yield return Wait(12);
		}
	}

	private void FireBurst(string transform)
	{
		float num = RandomAngle();
		float num2 = 22.5f;
		for (int i = 0; i < 16; i++)
		{
			Fire(new Offset(transform), new Direction(num + (float)i * num2), new Speed(9f));
		}
	}

	private void QuadShot(float direction, string transform, float speed)
	{
		for (int i = 0; i < 4; i++)
		{
			Fire(new Offset(transform), new Direction(direction), new Speed(speed - (float)i * 1.5f), new SpeedChangingBullet("bigBullet", speed, 120));
		}
	}
}
