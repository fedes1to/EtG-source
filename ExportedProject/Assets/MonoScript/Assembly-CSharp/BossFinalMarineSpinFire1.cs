using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossFinalMarine/SpinFire1")]
public class BossFinalMarineSpinFire1 : Script
{
	private const int NumWaves = 25;

	private const int NumBulletsPerWave = 6;

	private const float AngleDeltaEachWave = 37f;

	private const int NumBulletsFinalWave = 64;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 25; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				float direction = (float)j * 60f + 37f * (float)i;
				Fire(new Direction(direction), new Speed(7f));
			}
			yield return Wait(8);
		}
		for (int k = 0; k < 64; k++)
		{
			Fire(new Direction((float)k * 360f / 64f), new Speed(12f));
		}
	}
}
