using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/MineFlayer/Bong1")]
public class MineFlayerBong1 : Script
{
	private const int NumBullets = 90;

	protected override IEnumerator Top()
	{
		float startDirection = RandomAngle();
		float delta = 9.75f;
		for (int i = 0; i < 90; i++)
		{
			Bullet bullet1 = new Bullet();
			Bullet bullet2 = new Bullet();
			Fire(new Direction(startDirection + (float)i * delta), new Speed(12f), bullet1);
			Fire(new Direction(startDirection + (float)i * delta + 180f), new Speed(12f), bullet2);
			bullet1.Projectile.IgnoreTileCollisionsFor(0.4f);
			bullet2.Projectile.IgnoreTileCollisionsFor(0.4f);
			yield return Wait(1);
		}
	}
}
