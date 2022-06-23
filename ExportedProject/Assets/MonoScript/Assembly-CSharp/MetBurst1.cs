using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class MetBurst1 : Script
{
	private const int NumBullets = 3;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 3; i++)
		{
			if ((bool)base.BulletBank && (bool)base.BulletBank.behaviorSpeculator && base.BulletBank.behaviorSpeculator.IsStunned)
			{
				break;
			}
			base.BulletBank.aiAnimator.PlayUntilFinished("fire", true);
			yield return Wait(18);
			Fire(new Direction(0f, DirectionType.Aim), new Speed(9f));
			for (int j = 0; j < 3; j++)
			{
				Fire(new Direction(Random.Range(-50f, 50f), DirectionType.Aim), new Speed(9f));
			}
			yield return Wait(24);
		}
	}
}
