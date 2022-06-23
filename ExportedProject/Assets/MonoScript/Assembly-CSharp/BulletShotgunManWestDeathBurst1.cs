using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("BulletShotgunMan/WestDeathBurst1")]
public class BulletShotgunManWestDeathBurst1 : Script
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
		Fire(new Direction(0f, DirectionType.Aim), new Speed(9f), new Bullet("bigBullet"));
		Fire(new Direction(BraveUtility.RandomSign() * Random.Range(20f, 40f), DirectionType.Aim), new Speed(9f), new Bullet("bigBullet"));
		return null;
	}
}
