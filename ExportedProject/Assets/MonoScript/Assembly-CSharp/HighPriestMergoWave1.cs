using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/HighPriest/MergoWave1")]
public class HighPriestMergoWave1 : Script
{
	private const int NumBullets = 15;

	private const float Angle = 120f;

	protected override IEnumerator Top()
	{
		float startAngle = -60f;
		float deltaAngle = 8.571428f;
		AIAnimator aiAnimator = base.BulletBank.aiAnimator;
		string name = "mergo";
		Vector2? position = base.Position;
		aiAnimator.PlayVfx(name, null, null, position);
		yield return Wait(60);
		for (int i = 0; i < 15; i++)
		{
			Fire(new Direction(startAngle + (float)i * deltaAngle, DirectionType.Aim), new Speed(8f), new Bullet("mergoWave"));
		}
	}
}
