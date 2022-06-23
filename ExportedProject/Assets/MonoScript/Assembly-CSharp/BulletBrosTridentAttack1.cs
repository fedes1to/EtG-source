using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BulletBros/TridentAttack1")]
public class BulletBrosTridentAttack1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = 0; i < 3; i++)
		{
			float aim = ((i % 2 != 0) ? GetAimDirection(1f, 10f) : base.AimDirection);
			Fire(new Direction(aim - 10f), new Speed(9.5f));
			Fire(new Direction(aim), new Speed(9f));
			Fire(new Direction(aim + 10f), new Speed(9.5f));
			yield return Wait(35);
		}
	}
}
