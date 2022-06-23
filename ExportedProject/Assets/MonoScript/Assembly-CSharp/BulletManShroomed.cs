using System.Collections;
using Brave.BulletScript;

public class BulletManShroomed : Script
{
	protected override IEnumerator Top()
	{
		Fire(new Direction(-20f, DirectionType.Aim), new Speed(9f));
		Fire(new Direction(20f, DirectionType.Aim), new Speed(9f));
		return null;
	}
}
