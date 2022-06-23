using System.Collections;
using Brave.BulletScript;

public class GripMasterRing1 : Script
{
	protected override IEnumerator Top()
	{
		float aimDirection = base.AimDirection;
		int num = 16;
		for (int i = 0; i < num; i++)
		{
			Fire(new Direction(SubdivideCircle(aimDirection, num, i)), new Speed(9f));
		}
		float num2 = 135f;
		float startAngle = aimDirection - num2 / 2f;
		num = 7;
		for (int j = 0; j < num - 1; j++)
		{
			Fire(new Direction(SubdivideArc(startAngle, num2, num, j, true)), new Speed(17f), new SpeedChangingBullet(9f, 30));
		}
		for (int k = 0; k < num - 1; k++)
		{
			Fire(new Direction(SubdivideArc(startAngle, num2, num, k, true)), new Speed(1f), new SpeedChangingBullet(9f, 30));
		}
		return null;
	}
}
