using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Bunker/Arc1")]
public class BunkerArc1 : Script
{
	protected override IEnumerator Top()
	{
		for (int i = 0; i < 21; i++)
		{
			Fire(new Offset(0f, -3.25f + (float)i * 0.325f, 0f, string.Empty, DirectionType.Relative), new Direction(-60 + i * 6, DirectionType.Relative), new Speed(7f));
			Fire(new Offset(0f, 3.25f - (float)i * 0.325f, 0f, string.Empty, DirectionType.Relative), new Direction(60 - i * 6, DirectionType.Relative), new Speed(7f));
			yield return Wait(3);
		}
	}
}
