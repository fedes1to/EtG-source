using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Sunburst/Burst1")]
public class SunburstBurst1 : Script
{
	public class BurstBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			ChangeSpeed(new Speed(5f), 40);
			return null;
		}
	}

	private const int NumBullets = 24;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		float num2 = 15f;
		for (int i = 0; i < 24; i++)
		{
			Fire(new Direction(num + (float)i * num2), new Speed(9f), new BurstBullet());
		}
		return null;
	}
}
