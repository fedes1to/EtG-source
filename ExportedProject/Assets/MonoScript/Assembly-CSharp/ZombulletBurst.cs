using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class ZombulletBurst : Script
{
	private class OscillatingBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			float randomOffset = Random.value;
			float startSpeed = Speed;
			for (int i = 0; i < 300; i++)
			{
				Speed = startSpeed + Mathf.SmoothStep(-2f, 2f, Mathf.PingPong((float)base.Tick / 60f + randomOffset, 1f));
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumBullets = 18;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		float num2 = 20f;
		for (int i = 0; i < 18; i++)
		{
			Fire(new Direction(num + (float)i * num2), new Speed(7f), new OscillatingBullet());
		}
		return null;
	}
}
