using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("Bosses/TankTreader/HomingShot1")]
public class TankTreaderHomingShot1 : Script
{
	private class HomingBullet : Bullet
	{
		public HomingBullet()
			: base("homingBullet")
		{
		}

		protected override IEnumerator Top()
		{
			for (int i = 0; i < 75; i++)
			{
				ChangeDirection(new Direction(0f, DirectionType.Aim, 3f));
				if (i == 45)
				{
					Projectile.spriteAnimator.Play("enemy_projectile_rocket_impact");
				}
				yield return Wait(1);
			}
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (!preventSpawningProjectiles)
			{
				float num = RandomAngle();
				float num2 = 22.5f;
				for (int i = 0; i < 16; i++)
				{
					Fire(new Direction(num + num2 * (float)i), new Speed(10f));
				}
				AkSoundEngine.PostEvent("Play_WPN_golddoublebarrelshotgun_shot_01", Projectile.gameObject);
			}
		}
	}

	private const int AirTime = 75;

	private const int NumDeathBullets = 16;

	protected override IEnumerator Top()
	{
		Fire(new Direction(0f, DirectionType.Aim), new Speed(7.5f), new HomingBullet());
		return null;
	}
}
