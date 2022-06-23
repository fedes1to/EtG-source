using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Kaliber/ThreeHeads1")]
public class KaliberThreeHeads1 : Script
{
	private const int NumBullets = 28;

	protected override IEnumerator Top()
	{
		float num = RandomAngle();
		float num2 = 12.8571424f;
		for (int i = 0; i < 28; i++)
		{
			Fire(new Direction(num + (float)i * num2), new Speed(9f), new Bullet("burst"));
		}
		return null;
	}
}
