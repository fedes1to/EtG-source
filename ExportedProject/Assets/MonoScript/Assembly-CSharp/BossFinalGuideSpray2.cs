using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalGuide/Spray2")]
public class BossFinalGuideSpray2 : Script
{
	private const int NumBullets = 40;

	private const float SprayAngle = 110f;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 40; i++)
		{
			Fire(new Offset("left gun"), new Direction(GetAimDirection("left gun") + (Random.value - 0.5f) * 110f), new Speed(12f));
			yield return Wait(6);
			Fire(new Offset("right gun"), new Direction(GetAimDirection("right gun") + (Random.value - 0.5f) * 110f), new Speed(12f));
			yield return Wait(6);
		}
	}
}
