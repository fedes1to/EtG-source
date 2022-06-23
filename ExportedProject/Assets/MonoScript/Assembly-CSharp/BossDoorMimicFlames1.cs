using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossDoorMimic/Flames1")]
public class BossDoorMimicFlames1 : Script
{
	private const int NumBullets = 70;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 70; i++)
		{
			Fire(new Offset((i % 2 != 0) ? "right eye" : "left eye"), new Direction(Random.Range(-30f, 30f), DirectionType.Aim), new Speed(10f), new Bullet("flame"));
			yield return Wait(5);
		}
	}
}
