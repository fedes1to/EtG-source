using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Bunker/TriangleWave1")]
public class BunkerTriangleWave1 : Script
{
	protected override IEnumerator Top()
	{
		for (int k = 0; k < 6; k++)
		{
			Fire(new Offset(0f, -3.25f + (float)k * 1.3f, 0f, string.Empty, DirectionType.Relative), new Direction(-40 + 16 * k, DirectionType.Relative), new Speed(9f), new Bullet("default2"));
			yield return Wait(3);
		}
		for (int j = 0; j < 5; j++)
		{
			Fire(new Offset(0f, 2.6f - (float)j * 1.3f, 0f, string.Empty, DirectionType.Relative), new Direction(24 - 16 * j, DirectionType.Relative), new Speed(9f), new Bullet("default2"));
			yield return Wait(3);
		}
		for (int i = 0; i < 5; i++)
		{
			Fire(new Offset(0f, -2.6f + (float)i * 1.3f, 0f, string.Empty, DirectionType.Relative), new Direction(-24 + 16 * i, DirectionType.Relative), new Speed(9f), new Bullet("default2"));
			yield return Wait(3);
		}
	}
}
