using System.Collections;
using Brave.BulletScript;

public class MimicIntroFire1 : Script
{
	public class BigBullet : Bullet
	{
		private bool m_isBlackPhantom;

		public BigBullet(bool isBlackPhantom)
			: base("bigbullet")
		{
			base.ForceBlackBullet = true;
			m_isBlackPhantom = isBlackPhantom;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(80);
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if (preventSpawningProjectiles)
			{
				return;
			}
			for (int i = 0; i < 8; i++)
			{
				Bullet bullet = new SpeedChangingBullet(10f, 120, 600);
				Fire(new Direction(i * 45), new Speed(8f), bullet);
				if (!m_isBlackPhantom)
				{
					bullet.ForceBlackBullet = false;
					bullet.Projectile.ForceBlackBullet = false;
					bullet.Projectile.ReturnFromBlackBullet();
				}
			}
		}
	}

	protected override IEnumerator Top()
	{
		bool isBlackPhantom = (bool)base.BulletBank && (bool)base.BulletBank.aiActor && base.BulletBank.aiActor.IsBlackPhantom;
		Fire(new Direction(base.AimDirection), new Speed(8f), new BigBullet(isBlackPhantom));
		return null;
	}
}
