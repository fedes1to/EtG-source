using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/BasicSwingRight1")]
public class InfinilichBasicSwingRight1 : Script
{
	private const float EnemyBulletSpeedItem = 12f;

	private static int[] ShootPoints = new int[11]
	{
		4, 9, 13, 18, 20, 21, 22, 23, 24, 25,
		26
	};

	protected override IEnumerator Top()
	{
		for (int i = 0; i < ShootPoints.Length; i++)
		{
			string transform = "bullet limb " + ShootPoints[i];
			float leadAmount = Mathf.Lerp(0f, 2f, (float)i / ((float)ShootPoints.Length - 1f));
			float aimDirection = GetAimDirection(transform, leadAmount, 12f);
			Fire(new Offset(transform), new Direction(aimDirection), new Speed(12f));
		}
		return null;
	}
}
