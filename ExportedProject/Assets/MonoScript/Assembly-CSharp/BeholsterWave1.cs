using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Beholster/Wave1")]
public class BeholsterWave1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = -3; i <= 3; i++)
		{
			Fire(new Direction(i * 20, DirectionType.Aim), new Speed(8f), new Bullet("donut"));
		}
		yield return Wait(35);
		for (float num = -2.5f; num <= 2.5f; num += 1f)
		{
			Fire(new Direction(num * 20f, DirectionType.Aim), new Speed(8f), new Bullet("donut"));
		}
		yield return Wait(35);
		for (int j = -9; j < 9; j++)
		{
			Fire(new Direction(j * 20, DirectionType.Aim), new Speed(8f), new Bullet("donut"));
		}
		yield return Wait(35);
		for (float num2 = -8.5f; num2 <= 8.5f; num2 += 1f)
		{
			Fire(new Direction(num2 * 20f, DirectionType.Aim), new Speed(8f), new Bullet("donut"));
		}
	}
}
