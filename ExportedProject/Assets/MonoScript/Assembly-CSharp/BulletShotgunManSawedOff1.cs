using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("BulletShotgunMan/SawedOff1")]
public class BulletShotgunManSawedOff1 : Script
{
	protected override IEnumerator Top()
	{
		float aimDirection = GetAimDirection(1f, 9f);
		for (int i = -2; i <= 2; i++)
		{
			Fire(new Direction(aimDirection + (float)(i * 6)), new Speed(10f - (float)Mathf.Abs(i) * 0.5f));
		}
		return null;
	}
}
