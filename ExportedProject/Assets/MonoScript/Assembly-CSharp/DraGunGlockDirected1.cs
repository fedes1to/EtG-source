using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/DraGun/GlockDirected1")]
public class DraGunGlockDirected1 : Script
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
		if (num > -91f && num < -89f)
		{
			int num2 = 8;
			float startAngle = -170f;
			for (int i = 0; i < num2; i++)
			{
				Fire(new Direction(SubdivideArc(startAngle, 160f, num2, i)), new Speed(9f), new Bullet(BulletName));
			}
			if (IsHard)
			{
				for (int j = 0; j < num2 - 1; j++)
				{
					Fire(new Direction(SubdivideArc(startAngle, 160f, num2, j, true)), new Speed(1f), new SpeedChangingBullet(BulletName, 9f, 60));
				}
			}
			float aimDirection = base.AimDirection;
			if (BraveMathCollege.AbsAngleBetween(aimDirection, -90f) <= 90f)
			{
				Fire(new Direction(base.AimDirection), new Speed(9f), new Bullet(BulletName));
			}
		}
		else
		{
			int num3 = 12;
			float startAngle2 = RandomAngle();
			for (int k = 0; k < num3; k++)
			{
				Fire(new Direction(SubdivideCircle(startAngle2, num3, k)), new Speed(9f), new Bullet(BulletName + "_spin"));
			}
			if (IsHard)
			{
				for (int l = 0; l < num3; l++)
				{
					Fire(new Direction(SubdivideCircle(startAngle2, num3, l, 1f, true)), new Speed(1f), new SpeedChangingBullet(BulletName + "_spin", 9f, 60));
				}
			}
			Fire(new Direction(base.AimDirection), new Speed(9f), new Bullet(BulletName));
		}
		return null;
	}
}
