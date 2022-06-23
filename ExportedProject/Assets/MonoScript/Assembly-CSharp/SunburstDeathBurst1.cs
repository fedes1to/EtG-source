using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Sunburst/DeathBurst1")]
public class SunburstDeathBurst1 : Script
{
	public class BurstBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(7f), 40);
			yield return Wait(600);
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		yield return Wait(20);
		for (int i = 0; i < 36; i++)
		{
			Fire(new Direction(i * 10), new Speed(9f), new BurstBullet());
		}
	}
}
