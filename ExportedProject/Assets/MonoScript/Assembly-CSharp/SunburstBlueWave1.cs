using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Sunburst/BlueWave1")]
public class SunburstBlueWave1 : Script
{
	protected override IEnumerator Top()
	{
		float aimDirection = base.AimDirection;
		Fire(new Offset(0f, 0.66f, aimDirection, string.Empty), new Direction(aimDirection), new Speed(9f));
		Fire(new Offset(0.66f, 0f, aimDirection, string.Empty), new Direction(aimDirection), new Speed(9f));
		Fire(new Offset(0f, -0.66f, aimDirection, string.Empty), new Direction(aimDirection), new Speed(9f));
		return null;
	}
}
