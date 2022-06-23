using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("BulletShotgunMan/MutantBasicAttack1")]
public class BulletShotgunManMutantBasicAttack1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = -2; i <= 2; i++)
		{
			Fire(new Direction(i * 6, DirectionType.Aim), new Speed(9f));
		}
		return null;
	}
}
