using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("BulletShotgunMan/VeteranDeathBurst1")]
public class BulletShotgunManVeteranDeathBurst1 : Script
{
	protected override IEnumerator Top()
	{
		int num = 5;
		for (int i = 0; i < num; i++)
		{
			Fire(new Direction(SubdivideCircle(0f, num, i)), new Speed(6.5f), new Bullet("flashybullet"));
		}
		num = 5;
		for (int j = 0; j < num; j++)
		{
			Fire(new Direction(SubdivideCircle(0f, num, j, 1f, true)), new Speed(10f), new Bullet("flashybullet"));
		}
		num = 3;
		for (int k = 0; k < num; k++)
		{
			Fire(new Direction(RandomAngle()), new Speed(12f), new Bullet("flashybullet"));
		}
		return null;
	}
}
