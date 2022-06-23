using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Chancebulon/BlizzbulonAttack1")]
public class ChancebulonBlizzbulonAttack1 : Script
{
	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		float deltaAngle = 30f;
		for (int i = 0; i < 12; i++)
		{
			Fire(new Direction((float)i * deltaAngle), new Speed(6f), new Bullet("icicle"));
		}
		yield return Wait(30);
		Fire(new Direction(-28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
		Fire(new Direction(0f, DirectionType.Aim), new Speed(11f), new Bullet("icicle"));
		Fire(new Direction(28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
		yield return Wait(30);
		Fire(new Direction(-28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
		Fire(new Direction(0f, DirectionType.Aim), new Speed(11f), new Bullet("icicle"));
		Fire(new Direction(28f, DirectionType.Aim), new Speed(9f), new Bullet("icicle"));
	}
}
