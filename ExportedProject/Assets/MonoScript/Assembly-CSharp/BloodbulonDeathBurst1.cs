using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class BloodbulonDeathBurst1 : Script
{
	private const int NumBullets = 20;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 20; i++)
		{
			QuadShot(RandomAngle(), Random.Range(0f, 1.5f), Random.Range(7f, 11f));
			yield return Wait(1);
		}
	}

	private void QuadShot(float direction, float offset, float speed)
	{
		for (int i = 0; i < 4; i++)
		{
			Fire(new Offset(offset, 0f, direction, string.Empty), new Direction(direction), new Speed(speed - (float)i * 1.5f), new SpeedChangingBullet(speed, 120));
		}
	}
}
