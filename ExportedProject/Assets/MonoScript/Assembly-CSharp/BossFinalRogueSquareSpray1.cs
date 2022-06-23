using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRogue/SquareSpray1")]
public class BossFinalRogueSquareSpray1 : Script
{
	private const float SprayAngle = 145f;

	private const float SpraySpeed = 120f;

	private const int SprayIterations = 4;

	protected override IEnumerator Top()
	{
		float angle = -162.5f;
		float totalDuration = 4.83333349f;
		int numBullets = Mathf.RoundToInt(totalDuration * 10f);
		for (int i = 0; i < numBullets; i++)
		{
			float t = (float)i / (float)numBullets;
			float tInFullPass = t * 4f % 2f;
			float currentAngle = angle + Mathf.PingPong(tInFullPass * 145f, 145f);
			Fire(new Direction(currentAngle), new Speed(12f), new Bullet("square"));
			yield return Wait(7);
		}
	}
}
