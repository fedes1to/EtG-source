using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class BabyDragunBurst1 : Script
{
	private const int NumBullets = 7;

	private const float HalfArc = 15f;

	protected override IEnumerator Top()
	{
		yield return Wait(15);
		float startDirection = base.AimDirection - 15f;
		for (int i = 0; i < 7; i++)
		{
			Fire(new Direction(SubdivideArc(startDirection, 30f, 7, i)), new Speed(Random.Range(10f, 12f)));
		}
	}
}
