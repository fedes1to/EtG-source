using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("GunNut/Cone1")]
public class GunNutCone : Script
{
	private const int NumBulletsMainWave = 25;

	protected override IEnumerator Top()
	{
		FireCluster(Direction);
		yield return Wait(10);
		for (int i = 0; i < 25; i++)
		{
			float num = -45f + (float)i * 3.75f;
			Fire(new Offset(0.5f, 0f, Direction + num, string.Empty), new Direction(num, DirectionType.Relative), new Speed(10f));
		}
		FireCluster(Direction - 45f);
		FireCluster(Direction + 45f);
		yield return Wait(10);
		FireCluster(Direction);
	}

	private void FireCluster(float direction)
	{
		Fire(new Offset(0.5f, 0f, direction, string.Empty), new Direction(direction), new Speed(12f));
		Fire(new Offset(0.275f, 0.25f, direction, string.Empty), new Direction(direction), new Speed(12f));
		Fire(new Offset(0.275f, -0.25f, direction, string.Empty), new Direction(direction), new Speed(12f));
		Fire(new Offset(0f, 0.4f, direction, string.Empty), new Direction(direction), new Speed(12f));
		Fire(new Offset(0f, -0.4f, direction, string.Empty), new Direction(direction), new Speed(12f));
	}
}
