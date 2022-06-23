using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("BulletSkeleton/DoubleShot1")]
public class BulletSkeletonDoubleShot1 : Script
{
	protected virtual bool IsHard
	{
		get
		{
			return false;
		}
	}

	protected override IEnumerator Top()
	{
		int numBullets = 4;
		float sign = BraveUtility.RandomSign();
		bool skipInFirstWave = true;
		bool skipInSecondWave = true;
		if (IsHard)
		{
			if (BraveUtility.RandomBool())
			{
				skipInFirstWave = false;
			}
			else
			{
				skipInSecondWave = false;
			}
		}
		int skip2 = Random.Range(0, numBullets - 1);
		for (int j = 0; j < numBullets - 1; j++)
		{
			if (j != skip2 || !skipInFirstWave)
			{
				Fire(new Direction(SubdivideArc((0f - sign) * 25f, sign * 50f, numBullets, j, true), DirectionType.Aim), new Speed(9f));
			}
			yield return Wait(3);
		}
		yield return Wait(10);
		skip2 = Random.Range(0, numBullets);
		for (int i = 0; i < numBullets; i++)
		{
			if (i != skip2 || !skipInSecondWave)
			{
				Fire(new Direction(SubdivideArc(sign * 25f, (0f - sign) * 50f, numBullets, i), DirectionType.Aim), new Speed(9f));
			}
			yield return Wait(3);
		}
	}
}
