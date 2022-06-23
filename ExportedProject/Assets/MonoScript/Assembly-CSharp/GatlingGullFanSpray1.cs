using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/FanSpray1")]
public class GatlingGullFanSpray1 : Script
{
	private const float SprayAngle = 90f;

	private const float SpraySpeed = 150f;

	private const int SprayIterations = 4;

	protected override IEnumerator Top()
	{
		float angle = base.AimDirection - 45f;
		float totalDuration = 2.4f;
		int numBullets = Mathf.RoundToInt(totalDuration * 10f);
		for (int i = 0; i < numBullets; i++)
		{
			float t = (float)i / (float)numBullets;
			float tInFullPass = t * 4f % 2f;
			float currentAngle = angle + Mathf.PingPong(tInFullPass * 90f, 90f);
			Fire(new Direction(currentAngle), new Speed((i != 12) ? 12 : 30));
			yield return Wait(6);
		}
	}
}
