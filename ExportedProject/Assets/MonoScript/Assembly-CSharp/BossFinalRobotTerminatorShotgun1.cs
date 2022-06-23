using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRobot/TerminatorShotgun1")]
public class BossFinalRobotTerminatorShotgun1 : Script
{
	protected override IEnumerator Top()
	{
		switch (Random.Range(0, 4))
		{
		case 0:
		{
			for (int l = -2; l <= 2; l++)
			{
				Fire(new Direction(l * 6, DirectionType.Aim), new Speed(5f));
			}
			break;
		}
		case 1:
		{
			for (int j = -2; j <= 2; j++)
			{
				Fire(new Direction(j * 6, DirectionType.Aim), new Speed(9f));
			}
			break;
		}
		case 2:
		{
			float aimDirection = GetAimDirection(1f, 9f);
			for (int k = -2; k <= 2; k++)
			{
				Fire(new Direction(aimDirection + (float)(k * 6)), new Speed(10f - (float)Mathf.Abs(k) * 0.5f));
			}
			break;
		}
		case 3:
		{
			for (int i = -2; i <= 2; i++)
			{
				Fire(new Direction(i * 6, DirectionType.Aim), new Speed(9f));
			}
			break;
		}
		}
		return null;
	}
}
