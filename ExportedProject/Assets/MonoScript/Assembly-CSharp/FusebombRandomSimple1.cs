using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Fusebomb/RandomBurstsSimple1")]
public class FusebombRandomSimple1 : Script
{
	protected override IEnumerator Top()
	{
		if (Random.value < 0.5f)
		{
			int num = 10;
			float startAngle = RandomAngle();
			for (int i = 0; i < num; i++)
			{
				Fire(new Direction(SubdivideArc(startAngle, 360f, num, i)), new Speed(9f));
			}
		}
		else
		{
			int num2 = 5;
			float aimDirection = base.AimDirection;
			float num3 = 35f;
			bool flag = BraveUtility.RandomBool();
			for (int j = 0; j < num2 + (flag ? (-1) : 0); j++)
			{
				Fire(new Direction(SubdivideArc(aimDirection - num3, num3 * 2f, num2, j, flag)), new Speed(9f));
			}
		}
		return null;
	}
}
