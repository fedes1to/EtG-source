using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Lich/QuickDrawBurstShot1")]
public class LichQuickDrawBurstShot1 : Script
{
	protected override IEnumerator Top()
	{
		float aimDirection = GetAimDirection(Random.Range(0, 3), 12f);
		for (int i = -2; i <= 2; i++)
		{
			Fire(new Direction(aimDirection + (float)(i * 10)), new Speed(12f), new Bullet("quickHoming"));
		}
		return null;
	}
}
