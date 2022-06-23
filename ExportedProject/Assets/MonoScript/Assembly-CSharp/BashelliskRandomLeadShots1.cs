using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Bashellisk/RandomLeadShots1")]
public class BashelliskRandomLeadShots1 : Script
{
	public int NumBullets = 10;

	public float BulletSpeed = 14f;

	protected override IEnumerator Top()
	{
		float leadAmount = Random.Range(0f, 2f);
		for (int i = 0; i < NumBullets; i++)
		{
			float dir = GetAimDirection(leadAmount, BulletSpeed);
			Fire(new Direction(dir), new Speed(BulletSpeed));
			yield return Wait(10);
		}
	}
}
