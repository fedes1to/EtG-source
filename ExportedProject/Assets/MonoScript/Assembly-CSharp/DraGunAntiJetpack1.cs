using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/DraGun/AntiJetpack1")]
public class DraGunAntiJetpack1 : Script
{
	private const int NumBullets = 30;

	private const int NumLines = 4;

	private const float RoomHalfWidth = 17.5f;

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
					Fire(new Offset(SubdivideRange(-17.5f, 17.5f, 30, j, offset), 18f, 0f, string.Empty), new Direction(-90f), new Speed(8f), new SpeedChangingBullet(9f, 90, 60 - 6 * i));
				}
			}
			yield return Wait(10);
		}
		yield return Wait(80);
	}
}
