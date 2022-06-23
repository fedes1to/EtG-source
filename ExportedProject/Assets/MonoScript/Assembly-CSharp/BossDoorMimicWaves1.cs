using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossDoorMimic/Waves1")]
public class BossDoorMimicWaves1 : Script
{
	private const int NumWaves = 7;

	private const int NumBulletsPerWave = 5;

	private const float Arc = 60f;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 7; i++)
		{
			bool offset = false;
			int numBullets = 5;
			if (i % 2 == 1)
			{
				offset = true;
				numBullets--;
			}
			float startDirection = base.AimDirection - 30f;
			for (int j = 0; j < numBullets; j++)
			{
				Fire(new Direction(SubdivideArc(startDirection, 60f, 5, j, offset)), new Speed(12f), new Bullet("teeth_wave"));
			}
			yield return Wait(15);
		}
	}
}
