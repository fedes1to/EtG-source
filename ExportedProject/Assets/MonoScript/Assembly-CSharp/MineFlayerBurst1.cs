using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MineFlayer/BellBursts1")]
public class MineFlayerBurst1 : Script
{
	private const int NumBullets = 23;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		float num2 = 15.652174f;
		float num3 = Random.Range(-3f, 3f);
		for (int i = 0; i < 23; i++)
		{
			Fire(new Direction(num + (float)i * num2), new Speed(6f + num3), new SpeedChangingBullet(16f + num3, 60));
		}
		return null;
	}
}
