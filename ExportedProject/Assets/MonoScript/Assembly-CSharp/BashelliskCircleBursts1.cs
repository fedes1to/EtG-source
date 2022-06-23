using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Bashellisk/CircleBursts1")]
public class BashelliskCircleBursts1 : Script
{
	private const int NumBullets = 17;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		float num2 = 21.17647f;
		for (int i = 0; i < 17; i++)
		{
			Fire(new Direction(num + (float)i * num2), new Speed(9f), new Bullet("CircleBurst"));
		}
		return null;
	}
}
