using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/KnifeSpray1")]
public class DraGunFlameKnifeSpray1 : Script
{
	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		float deltaAngle = 7f;
		float sign = BraveUtility.RandomSign();
		for (int i = 0; i < 12; i++)
		{
			Fire(new Direction((-45f + deltaAngle * (float)i + (float)Random.Range(-5, 5)) * sign, DirectionType.Relative), new Speed(12f));
			yield return Wait(2);
		}
	}
}
