using System;
using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class CubulonSlam1 : Script
{
	private const int NumBullets = 11;

	protected override IEnumerator Top()
	{
		FireLine(45f);
		FireLine(135f);
		FireLine(225f);
		FireLine(315f);
		return null;
	}

	private void FireLine(float startingAngle)
	{
		float num = 9f;
		for (int i = 0; i < 11; i++)
		{
			float num2 = Mathf.Atan((-45f + (float)i * num) / 45f) * 57.29578f;
			float num3 = Mathf.Cos(num2 * ((float)Math.PI / 180f));
			float num4 = ((!((double)Mathf.Abs(num3) < 0.0001)) ? (1f / num3) : 1f);
			Fire(new Direction(num2 + startingAngle), new Speed(num4 * 9f));
		}
	}
}
