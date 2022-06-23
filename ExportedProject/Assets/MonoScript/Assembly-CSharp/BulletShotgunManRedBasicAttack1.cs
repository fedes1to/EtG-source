using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("BulletShotgunMan/RedBasicAttack1")]
public class BulletShotgunManRedBasicAttack1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = -2; i <= 2; i++)
		{
			Fire(new Direction(i * 6, DirectionType.Aim), new Speed(5f));
		}
		return null;
	}
}
