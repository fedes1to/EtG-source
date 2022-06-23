using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/HighPriest/CrossSprinkler1")]
public class HighPriestCrossSprinkler1 : Script
{
	private const int NumBullets = 105;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 105; i++)
		{
			float d = (float)i / 105f;
			Fire(new Offset("left hand"), new Direction(-65f - d * 230f * 3.5f), new Speed(12f), new Bullet("cross"));
			Fire(new Offset("right hand"), new Direction(-115f + d * 230f * 3.5f), new Speed(12f), new Bullet("cross"));
			yield return Wait(2);
		}
	}
}
