using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/Mac10Burst1")]
public class DraGunMac10Burst1 : Script
{
	protected override IEnumerator Top()
	{
		while (true)
		{
			Fire(new Direction(Random.Range(-45f, 45f), DirectionType.Relative), new Speed(12f), new Bullet("UziBurst"));
			yield return Wait(Random.Range(2, 4));
		}
	}
}
