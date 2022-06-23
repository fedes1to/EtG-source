using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Fusebomb/RandomBurstsRapid1")]
public class FusebombRandomRapid1 : Script
{
	private const int NumBullets = 8;

	private static bool s_offset;

	protected override IEnumerator Top()
	{
		float startAngle = RandomAngle();
		for (int i = 0; i < 8; i++)
		{
			Fire(new Direction(SubdivideArc(startAngle, 360f, 8, i)), new Speed(9f));
		}
		return null;
	}
}
