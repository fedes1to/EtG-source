using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Blizzbulon/BasicAttack2")]
public class BlizzbulonBasicAttack2 : Script
{
	protected override IEnumerator Top()
	{
		Fire(new Direction(-28f, DirectionType.Aim), new Speed(9f));
		Fire(new Direction(0f, DirectionType.Aim), new Speed(11f));
		Fire(new Direction(28f, DirectionType.Aim), new Speed(9f));
		yield return Wait(45);
		Fire(new Direction(-28f, DirectionType.Aim), new Speed(9f));
		Fire(new Direction(0f, DirectionType.Aim), new Speed(11f));
		Fire(new Direction(28f, DirectionType.Aim), new Speed(9f));
	}
}
