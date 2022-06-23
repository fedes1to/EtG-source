using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalConvict/WalkAndShoot1")]
public class BossFinalConvictWalkAndShoot1 : Script
{
	private const int NumBullets = 100;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 100; i++)
		{
			Fire(new Direction(Random.Range(-15f, 15f), DirectionType.Relative), new Speed(9f));
			yield return Wait(3);
		}
	}
}
