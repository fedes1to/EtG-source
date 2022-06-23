using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("BulletShotgunMan/DeathBurst1")]
public class BulletShotgunManDeathBurst1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = 0; i <= 6; i++)
		{
			Fire(new Direction(i * 60), new Speed(6.5f), new Bullet("flashybullet"));
		}
		return null;
	}
}
