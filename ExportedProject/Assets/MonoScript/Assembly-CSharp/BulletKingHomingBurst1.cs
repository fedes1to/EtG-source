using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/BulletKing/HomingBurst1")]
public class BulletKingHomingBurst1 : Script
{
	public class HomingBullet : Bullet
	{
		public HomingBullet()
			: base("homing")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(10);
			Direction = base.AimDirection;
			yield return Wait(90);
			Direction = base.AimDirection;
			Fire(new Direction(60f, DirectionType.Aim), new Speed(7f), new Bullet("homingBurst"));
			Fire(new Direction(-60f, DirectionType.Aim), new Speed(7f), new Bullet("homingBurst"));
			yield return Wait(600);
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		yield return Wait(10);
		HomingShot(-1.25f, -0.75f, 0f);
		HomingShot(-1.3125f, -0.4375f, -15f);
		HomingShot(-1.5f, -0.1875f, -30f);
		HomingShot(-1.75f, 0.25f, -45f);
		HomingShot(-2.125f, 1.3125f, -67.5f);
		HomingShot(-2.125f, 1.3125f, -90f);
		HomingShot(-2.125f, 1.3125f, -112.5f);
		HomingShot(-2.0625f, 2.375f, -135f);
		HomingShot(-0.8125f, 3.1875f, -157.5f);
		HomingShot(0.0625f, 3.5625f, 180f);
		HomingShot(0.9375f, 3.1875f, 157.5f);
		HomingShot(2.125f, 2.375f, 135f);
		HomingShot(2.1875f, 1.3125f, 112.5f);
		HomingShot(2.1875f, 1.3125f, 90f);
		HomingShot(2.1875f, 1.3125f, 67.5f);
		HomingShot(1.875f, 0.25f, 45f);
		HomingShot(1.625f, -0.1875f, 30f);
		HomingShot(1.4275f, -0.4375f, 15f);
		HomingShot(1.375f, -0.75f, 0f);
	}

	private void HomingShot(float x, float y, float direction)
	{
		Fire(new Offset(x, y, 0f, string.Empty), new Direction(direction - 90f), new Speed(9f), new HomingBullet());
	}
}
