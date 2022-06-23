using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/Megalich/AntiJetpack1")]
public class MegalichAntiJetpack1 : Script
{
	private const int NumBullets = 30;

	private const int NumLines = 4;

	private const float RoomHalfWidth = 20f;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		for (int i = 0; i < 4; i++)
		{
			bool offset = i % 2 == 1;
			for (int j = 0; j < 30; j++)
			{
				if (!offset || j != 29)
				{
					Fire(new Offset(SubdivideRange(-20f, 20f, 30, j, offset), 7.5f, 0f, string.Empty), new Direction(-90f), new Speed(7f), new SpeedChangingBullet(8f, 60, 20 - 6 * i));
				}
			}
			yield return Wait(10);
		}
		yield return Wait(80);
	}
}
