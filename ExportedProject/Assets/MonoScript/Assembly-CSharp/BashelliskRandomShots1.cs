using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Bashellisk/RandomShots1")]
public class BashelliskRandomShots1 : Script
{
	public int NumBullets = 5;

	public float BulletSpeed = 10f;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < NumBullets; i++)
		{
			Fire(new Direction(GetAimDirection(1f, BulletSpeed) + (float)Random.Range(-45, 45)), new Speed(BulletSpeed), new Bullet("randomBullet"));
			yield return Wait(12);
		}
	}
}
