using System.Collections;
using Brave.BulletScript;
using FullInspector;

[InspectorDropdownName("BulletManMagic/Astral2")]
public class BulletManMagicAstral2 : Script
{
	public class AstralBullet : Bullet
	{
		private const int NumBullets = 18;

		public AstralBullet()
			: base("astral")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(30);
			Projectile.specRigidbody.CollideWithOthers = true;
			Direction = base.AimDirection;
			Speed = 1.5f;
			for (int i = 0; i < 105; i++)
			{
				Direction = base.AimDirection;
				yield return Wait(1);
			}
			float startDirection = RandomAngle();
			float delta = 20f;
			for (int j = 0; j < 18; j++)
			{
				Fire(new Direction(startDirection + (float)j * delta), new Speed(9f));
			}
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		Fire(new AstralBullet());
		return null;
	}
}
