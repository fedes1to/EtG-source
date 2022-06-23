using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class ShelletonBasicAttack1 : Script
{
	private const int NumBullets = 21;

	private const int NumPlugs = 2;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 21; i++)
		{
			float direction = Mathf.Lerp(-80f, 80f, (float)i / 20f);
			Fire(new Direction(direction, DirectionType.Aim), new Speed((i % 2 != 0) ? 10 : 4), new SpeedChangingBullet(10f, 60, 180));
		}
		for (int j = 0; j < 2; j++)
		{
			int num = Random.Range(0, 21);
			float direction2 = Mathf.Lerp(-80f, 80f, (float)num / 20f);
			Fire(new Direction(direction2, DirectionType.Aim), new Speed((num % 2 != 1) ? 10 : 4), new SpeedChangingBullet(10f, 60, 180));
		}
		return null;
	}
}
