using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Bunker/DirectFire1")]
public class BunkerDirectFire1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = 0; i < 15; i++)
		{
			Fire(new Offset(0f, -3.25f + (float)i * 0.43f, 0f, string.Empty, DirectionType.Relative), new Direction(0f, DirectionType.Aim), new Speed(12f), new Bullet("default3"));
			yield return Wait(8);
		}
	}
}
