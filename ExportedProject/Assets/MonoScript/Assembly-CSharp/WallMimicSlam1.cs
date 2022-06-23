using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("MimicWall/Slam1")]
public class WallMimicSlam1 : Script
{
	protected override IEnumerator Top()
	{
		float facingDirection = base.BulletBank.aiAnimator.CurrentArtAngle;
		FireLine(facingDirection - 90f, 5f, 45f, -15f);
		FireLine(facingDirection, 11f, -45f, 45f);
		FireLine(facingDirection + 90f, 5f, -45f, 15f);
		yield return Wait(10);
		FireLine(facingDirection - 90f, 4f, 45f, -15f);
		FireLine(facingDirection, 10f, -45f, 45f);
		FireLine(facingDirection + 90f, 4f, -45f, 15f);
	}

	protected void FireLine(float centralAngle, float numBullets, float minAngle, float maxAngle, bool addBlackBullets = false)
	{
		float num = (maxAngle - minAngle) / (numBullets - 1f);
		for (int i = 0; (float)i < numBullets; i++)
		{
			float num2 = Mathf.Atan((minAngle + (float)i * num) / 45f) * 57.29578f;
			float num3 = Mathf.Cos(num2 * ((float)Math.PI / 180f));
			float num4 = ((!((double)Mathf.Abs(num3) < 0.0001)) ? (1f / num3) : 1f);
			Bullet bullet = new Bullet();
			if (addBlackBullets && i % 2 == 1)
			{
				bullet.ForceBlackBullet = true;
			}
			Fire(new Direction(num2 + centralAngle), new Speed(num4 * 9f), bullet);
		}
	}
}
