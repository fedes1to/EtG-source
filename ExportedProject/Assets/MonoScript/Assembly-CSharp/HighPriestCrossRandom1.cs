using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/HighPriest/CrossRandom1")]
public class HighPriestCrossRandom1 : Script
{
	private const int NumBullets = 120;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 120; i++)
		{
			Fire(new Offset("left hand"), new Direction(-65f - Random.value * 230f), new Speed(12f), new Bullet("cross"));
			yield return Wait(1);
			Fire(new Offset("right hand"), new Direction(-115f + Random.value * 230f), new Speed(12f), new Bullet("cross"));
			yield return Wait(1);
		}
	}
}
