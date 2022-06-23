using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/HighPriest/CircleBurst6")]
public class HighPriestCircleBurst6 : Script
{
	private const int NumBullets = 6;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		float num2 = 60f;
		for (int i = 0; i < 6; i++)
		{
			Fire(new Direction(num + (float)i * num2), new Speed(9f), new Bullet("homingPop"));
		}
		return null;
	}
}
