using System.Collections;
using Brave.BulletScript;

public class CircleBurst12 : Script
{
	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		float num2 = 30f;
		for (int i = 0; i < 12; i++)
		{
			Fire(new Direction(num + (float)i * num2), new Speed(9f));
		}
		return null;
	}
}
