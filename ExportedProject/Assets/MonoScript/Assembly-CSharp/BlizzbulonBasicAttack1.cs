using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Blizzbulon/BasicAttack1")]
public class BlizzbulonBasicAttack1 : Script
{
	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		float num = 30f;
		for (int i = 0; i < 12; i++)
		{
			Fire(new Direction((float)i * num), new Speed(6f));
		}
		return null;
	}
}
