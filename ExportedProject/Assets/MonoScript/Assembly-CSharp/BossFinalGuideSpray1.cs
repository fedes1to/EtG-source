using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalGuide/Spray1")]
public class BossFinalGuideSpray1 : Script
{
	private const float SprayAngle = 90f;

	private const float SpraySpeed = 110f;

	private const int SprayIterations = 3;

	protected override IEnumerator Top()
	{
		int numBullets = Mathf.RoundToInt(14.727273f);
		for (int i = 0; i < numBullets; i++)
		{
			float t = (float)i / (float)numBullets;
			float tInFullPass = t * 3f % 2f;
			Fire(new Offset("left gun"), new Direction(GetAimDirection("left gun") - 45f + Mathf.PingPong(tInFullPass * 90f, 90f)), new Speed(12f));
			yield return Wait(4);
			Fire(new Offset("right gun"), new Direction(GetAimDirection("right gun") + 45f - Mathf.PingPong(tInFullPass * 90f, 90f)), new Speed(12f));
			yield return Wait(4);
		}
	}
}
