using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BossFinalRogue/BigGun1")]
public class BossFinalRogueBigGun1 : Script
{
	private const float NumBullets = 26f;

	private const float NumFastBullets = 44f;

	protected override IEnumerator Top()
	{
		float delta = 13.8461542f;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; (float)j < 26f; j++)
			{
				Fire(new Offset("big gun left cannon"), new Direction(-90f + delta * (float)j + (float)(i * 2)), new Speed(10f), new Bullet("bigGunSlow"));
			}
			yield return Wait(22);
			for (int k = 0; (float)k < 26f; k++)
			{
				Fire(new Offset("big gun right cannon"), new Direction(-90f + delta * (float)k + (float)(i * 2)), new Speed(10f), new Bullet("bigGunSlow"));
			}
			yield return Wait(23);
		}
		yield return Wait(56);
		for (int l = 0; (float)l < 44f; l++)
		{
			Fire(new Offset("big gun left cannon"), new Direction(-90f + delta * (float)l), new Speed(18f), new Bullet("bigGunFast"));
			Fire(new Offset("big gun right cannon"), new Direction(-90f + delta * (float)l), new Speed(18f), new Bullet("bigGunFast"));
		}
	}
}
