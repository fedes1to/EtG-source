using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/SweepFlameBreath2")]
public class DraGunSweepFlameBreath2 : Script
{
	protected override IEnumerator Top()
	{
		while (true)
		{
			Fire(new Direction(Random.Range(-45f, 45f), DirectionType.Relative), new Speed(14f), new Bullet("Sweep"));
			if (base.Tick % 2 == 1)
			{
				Fire(new Direction(Random.Range(-15f, 15f), DirectionType.Relative), new Speed(Random.Range(2, 8)), new SpeedChangingBullet("Sweep", 14f, 120));
			}
			yield return Wait(1);
		}
	}
}
