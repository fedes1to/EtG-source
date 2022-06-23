using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("BulletShotgunMan/BlueBasicAttack1")]
public class BulletShotgunManBlueBasicAttack1 : Script
{
	protected override IEnumerator Top()
	{
		float aimDirection = base.AimDirection;
		for (int i = -2; i <= 2; i++)
		{
			Fire(new Direction((float)(i * 20) + aimDirection), new Speed(5f));
		}
		yield return Wait(40);
		if (!base.BulletBank || !base.BulletBank.behaviorSpeculator.IsStunned)
		{
			if (BraveMathCollege.AbsAngleBetween(base.AimDirection, aimDirection) > 30f)
			{
				aimDirection = base.AimDirection;
			}
			for (float num = -1.5f; num <= 1.5f; num += 1f)
			{
				Fire(new Direction(num * 20f + aimDirection), new Speed(5f));
			}
		}
	}
}
