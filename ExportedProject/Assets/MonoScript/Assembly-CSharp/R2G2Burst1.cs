using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("R2G2/Burst1")]
public class R2G2Burst1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = 0; i < 6; i++)
		{
			Fire(new Direction(Random.Range(-10, 10), DirectionType.Aim), new Speed(9f));
			yield return 10;
		}
	}
}
