using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/DraGun/GlockRicochet1")]
public class DraGunGlockRicochet1 : Script
{
	protected override IEnumerator Top()
	{
		float num = BraveMathCollege.ClampAngle180(Direction);
		if (num > -91f && num < -89f)
		{
			int num2 = 8;
			float num3 = -170f;
			float num4 = 160f / (float)(num2 - 1);
			for (int i = 0; i < num2; i++)
			{
				Fire(new Direction(num3 + (float)i * num4), new Speed(9f), new Bullet("ricochet"));
			}
			float aimDirection = base.AimDirection;
			if (BraveMathCollege.AbsAngleBetween(aimDirection, -90f) <= 90f)
			{
				Fire(new Direction(base.AimDirection), new Speed(9f), new Bullet("ricochet"));
			}
		}
		else
		{
			int num5 = 8;
			float num6 = -45f;
			float num7 = 90f / (float)(num5 - 1);
			for (int j = 0; j < num5; j++)
			{
				Fire(new Direction(num6 + (float)j * num7, DirectionType.Relative), new Speed(9f), new Bullet("ricochet"));
			}
		}
		return null;
	}
}
