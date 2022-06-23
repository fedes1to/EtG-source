using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Lich/NormalShoot1")]
public class LichNormalShoot1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				Fire(new Direction(Random.Range(-15f, 15f), DirectionType.Aim, 15f), new Speed(12f), new Bullet("default"));
				yield return Wait(8);
			}
			float dirToTarget = BraveMathCollege.ClampAngle360(base.AimDirection);
			if (dirToTarget > 10f && dirToTarget < 170f)
			{
				break;
			}
			yield return 20;
		}
	}
}
