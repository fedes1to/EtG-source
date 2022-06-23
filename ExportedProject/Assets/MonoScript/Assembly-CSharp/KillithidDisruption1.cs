using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class KillithidDisruption1 : Script
{
	public class AstralBullet : Bullet
	{
		public AstralBullet()
			: base("disruption")
		{
		}

		protected override IEnumerator Top()
		{
			yield return Wait(113);
			if (base.BulletBank.aiActor.healthHaver.IsDead)
			{
				Vanish();
			}
			Projectile.specRigidbody.CollideWithOthers = true;
			Direction = base.AimDirection;
			Speed = 0f;
			int numShots = Random.Range(2, 6);
			for (int i = 0; i < numShots; i++)
			{
				yield return Wait(Random.Range(20, 70));
				if (base.BulletBank.aiActor.healthHaver.IsDead)
				{
					Vanish();
				}
				Projectile.spriteAnimator.PlayFromFrame("killithid_disruption_attack", 0);
				yield return Wait(15);
				Fire(new Direction(0f, DirectionType.Aim), new Speed(12f));
			}
			yield return Wait(30);
			Vanish();
		}
	}

	protected override IEnumerator Top()
	{
		AstralBullet astralBullet = new AstralBullet();
		Fire(astralBullet);
		while (!astralBullet.Destroyed)
		{
			yield return null;
		}
	}
}
