using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/FlameBreath1")]
public class DraGunFlameBreath1 : Script
{
	private const int NumBullets = 80;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 80; i++)
		{
			Fire(new Direction(Random.Range(-20f, 20f), DirectionType.Aim), new Speed(14f), new Bullet("Breath"));
			yield return Wait(3);
		}
	}
}
