using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Lich/SpinFire2")]
public class LichSpinFire2 : Script
{
	private const int NumWaves = 6;

	private const int NumBulletsPerWave = 48;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 6; i++)
		{
			for (int k = 0; k < 48; k++)
			{
				float num = (float)k * 7.5f;
				Fire(new Offset(1f, 0f, num, string.Empty), new Direction(num), new Speed(7f), new Bullet("twirl"));
			}
			yield return Wait(6);
			for (int l = 0; l < 48; l++)
			{
				float num2 = ((float)l + 0.5f) * 7.5f;
				Fire(new Offset(1f, 0f, num2, string.Empty), new Direction(num2), new Speed(7f), new Bullet("twirl"));
			}
			for (int j = 0; j < 20; j++)
			{
				float direction = RandomAngle();
				Fire(new Offset(1f, 0f, direction, string.Empty), new Direction(direction), new Speed(7f), new Bullet("twirl"));
				yield return Wait(3);
			}
		}
	}
}
