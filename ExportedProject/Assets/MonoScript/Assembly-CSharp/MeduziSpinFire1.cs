using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Meduzi/SpinFire1")]
public class MeduziSpinFire1 : Script
{
	private const int NumWaves = 29;

	private const int NumBulletsPerWave = 6;

	private const float AngleDeltaEachWave = -37f;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 29; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				float num = (float)j * 60f + -37f * (float)i;
				Fire(new Offset(1.66f, 0f, num, string.Empty), new Direction(num), new Speed(9f), new Bullet("bigBullet"));
			}
			yield return Wait(6);
		}
	}
}
