using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Megalich/DonkeyKong1")]
public class MegalichDonkeyKong1 : Script
{
	private const int NumWaves = 3;

	private const int NumLargeWaves = 5;

	private const int NumLargeBulletsPerWave = 5;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 3; i++)
		{
			yield return Wait(6);
			ShootSmallBullets(1f, false);
			yield return Wait(36);
			ShootSmallBullets(-1f, true);
			yield return Wait(30);
		}
	}

	private void ShootSmallBullets(float dir, bool isOffset)
	{
		for (int i = 0; i < 5; i++)
		{
			int num = 5;
			float num2 = 0f;
			if (isOffset)
			{
				num--;
				num2 += 0.5f;
			}
			for (int j = 0; j < num; j++)
			{
				Fire(new Offset(dir * -19.5f, -0.25f + Mathf.Lerp(0f, -10f, ((float)j + num2) / 4f), 0f, string.Empty), new Direction((!(dir > 0f)) ? 180 : 0), new Speed(14f), new DelayedBullet("frogger", 7 * i));
			}
		}
	}
}
