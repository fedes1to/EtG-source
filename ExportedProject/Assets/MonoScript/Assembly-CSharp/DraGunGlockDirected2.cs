using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/GlockDirected2")]
public class DraGunGlockDirected2 : Script
{
	protected virtual string BulletName
	{
		get
		{
			return "glock";
		}
	}

	protected virtual bool IsHard
	{
		get
		{
			return false;
		}
	}

	protected override IEnumerator Top()
	{
		float num = BraveMathCollege.ClampAngle180(Direction);
		bool flag = num > -91f && num < -89f;
		float aimDirection = base.AimDirection;
		int num4;
		if (flag || BraveMathCollege.AbsAngleBetween(aimDirection, -90f) < 45f)
		{
			float num2 = (aimDirection + -90f) / 2f;
			float num3 = 45f;
			num4 = 13;
			for (int i = 0; i < num4; i++)
			{
				Fire(new Direction(SubdivideArc(num2 - num3, num3 * 2f, num4, i)), new Speed(Random.Range(6, 11)), new SpeedChangingBullet(BulletName, 11f, 60));
			}
			Fire(new Direction(aimDirection), new Speed(11f), new SpeedChangingBullet(BulletName, 11f, 60));
			if (IsHard)
			{
				for (int j = 0; j < num4; j++)
				{
					Fire(new Direction(SubdivideArc(num2 - num3, num3 * 2f, num4, j)), new Speed(1f), new SpeedChangingBullet(BulletName, Random.Range(9, 12), 60));
				}
				Fire(new Direction(aimDirection), new Speed(1f), new SpeedChangingBullet(BulletName, 11f, 60));
			}
			yield break;
		}
		num4 = 12;
		float startAngle = RandomAngle();
		for (int k = 0; k < num4; k++)
		{
			Fire(new Direction(SubdivideCircle(startAngle, num4, k)), new Speed(9f), new Bullet(BulletName + "_spin"));
		}
		if (IsHard)
		{
			for (int l = 0; l < num4; l++)
			{
				Fire(new Direction(SubdivideCircle(startAngle, num4, l, 1f, true)), new Speed(1f), new SpeedChangingBullet(BulletName + "_spin", 9f, 60));
			}
		}
		Fire(new Direction(base.AimDirection), new Speed(9f), new Bullet(BulletName));
	}
}
