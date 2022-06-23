using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/GiantPowderSkull/EyesSprinkler1")]
public class GiantPowderSkullEyesSprinkler1 : Script
{
	private const int NumBullets = 75;

	private const float DeltaAngle1 = 12f;

	private const float DeltaAngle2 = 16f;

	protected override IEnumerator Top()
	{
		float startAngle1 = RandomAngle();
		float startAngle2 = RandomAngle();
		for (int i = 0; i < 75; i++)
		{
			Fire(new Offset("left eye"), new Direction(startAngle1 + (float)i * 12f), new Speed(12f), new Bullet("default_novfx"));
			yield return Wait(2);
			Fire(new Offset("right eye"), new Direction(startAngle2 + (float)i * 16f), new Speed(12f), new Bullet("default_novfx"));
			yield return Wait(2);
		}
	}
}
