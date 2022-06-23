using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Helicopter/RandomBurstsRapid1")]
public class HelicopterRandomRapid1 : Script
{
	private const int NumBullets = 6;

	private static string[] Transforms = new string[4] { "shoot point 1", "shoot point 2", "shoot point 3", "shoot point 4" };

	protected override IEnumerator Top()
	{
		float startDirection2 = RandomAngle();
		string transform2 = BraveUtility.RandomElement(Transforms);
		for (int i = 0; i < 6; i++)
		{
			Fire(new Offset(transform2), new Direction(SubdivideCircle(startDirection2, 6, i)), new Speed(9f), new HelicopterRandomSimple1.BigBullet());
		}
		if (!BraveUtility.RandomBool())
		{
			yield return Wait(15);
			startDirection2 = RandomAngle();
			transform2 = BraveUtility.RandomElement(Transforms);
			for (int j = 0; j < 6; j++)
			{
				Fire(new Offset(transform2), new Direction(SubdivideCircle(startDirection2, 6, j)), new Speed(9f), new HelicopterRandomSimple1.BigBullet());
			}
		}
	}
}
