using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Kaliber/TwoHeads1")]
public class KaliberTwoHeads1 : Script
{
	private const int NumShots = 6;

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 6; i++)
		{
			yield return Wait(24);
			bool offset = i % 4 > 1;
			if (i % 2 == 0)
			{
				FireArc("2 top left", 70f, 150f, 8, 4, offset);
				FireArc("2 bottom left", 220f, 80f, 5, 2, offset);
			}
			else
			{
				FireArc("2 top right", 110f, -150f, 8, 4, offset);
				FireArc("2 bottom right", 320f, -80f, 5, 2, offset);
			}
		}
		yield return Wait(18);
	}

	private void FireArc(string transform, float startAngle, float sweepAngle, int numBullets, int muzzleIndex, bool offset)
	{
		float num = ((!offset) ? numBullets : (numBullets - 1));
		for (int i = 0; (float)i < num; i++)
		{
			Offset offset2 = new Offset(transform);
			Direction direction = new Direction(SubdivideArc(startAngle, sweepAngle, numBullets, i, offset));
			Speed speed = new Speed(9f);
			bool suppressVfx = i != muzzleIndex;
			Fire(offset2, direction, speed, new Bullet(null, suppressVfx));
		}
	}
}
