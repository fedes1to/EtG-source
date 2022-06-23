using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/SweepFlameBreath1")]
public class DraGunSweepFlameBreath1 : Script
{
	protected override IEnumerator Top()
	{
		while (true)
		{
			Fire(new Direction(Random.Range(-45f, 45f), DirectionType.Relative), new Speed(14f), new Bullet("Sweep"));
			yield return Wait(1);
		}
	}
}
