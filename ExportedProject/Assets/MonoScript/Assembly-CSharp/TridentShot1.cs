using System.Collections;
using Brave.BulletScript;

public class TridentShot1 : Script
{
	protected override IEnumerator Top()
	{
		Fire(new Direction(-12f, DirectionType.Aim), new Speed(10f));
		Fire(new Direction(0f, DirectionType.Aim), new Speed(10f));
		Fire(new Direction(12f, DirectionType.Aim), new Speed(10f));
		return null;
	}
}
