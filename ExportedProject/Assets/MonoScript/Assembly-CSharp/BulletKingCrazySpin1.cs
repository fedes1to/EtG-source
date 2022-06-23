using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BulletKing/CrazySpin1")]
public class BulletKingCrazySpin1 : Script
{
	private const int NumWaves = 29;

	private const int NumBulletsPerWave = 6;

	private const float AngleDeltaEachWave = 37f;

	private const int NumBulletsFinalWave = 64;

	protected bool IsHard
	{
		get
		{
			return this is BulletKingCrazySpinHard1;
		}
	}

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 29; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				float num = (float)j * 60f + 37f * (float)i;
				Fire(new Offset(1.66f, 0f, num, string.Empty), new Direction(num), new Speed(7f), new Bullet("default_novfx"));
			}
			yield return Wait(6);
		}
		for (int k = 0; k < 64; k++)
		{
			Fire(new Direction((float)k * 360f / 64f), new Speed((!IsHard) ? 8 : 13), new Bullet("default_novfx"));
		}
	}
}
