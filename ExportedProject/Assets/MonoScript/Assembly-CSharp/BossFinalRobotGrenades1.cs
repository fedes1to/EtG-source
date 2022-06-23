using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalRobot/Grenades1")]
public class BossFinalRobotGrenades1 : Script
{
	private const int NumBullets = 4;

	protected override IEnumerator Top()
	{
		float airTime = base.BulletBank.GetBullet("grenade").BulletObject.GetComponent<ArcProjectile>().GetTimeInFlight();
		Vector2 vector = BulletManager.PlayerPosition();
		Bullet bullet2 = new Bullet("grenade");
		float direction2 = (vector - base.Position).ToAngle();
		Fire(new Direction(direction2), new Speed(1f), bullet2);
		(bullet2.Projectile as ArcProjectile).AdjustSpeedToHit(vector);
		bullet2.Projectile.ImmuneToSustainedBlanks = true;
		for (int i = 0; i < 4; i++)
		{
			yield return Wait(6);
			Vector2 targetVelocity = BulletManager.PlayerVelocity();
			float startAngle;
			float dist;
			if (targetVelocity != Vector2.zero && targetVelocity.magnitude > 0.5f)
			{
				startAngle = targetVelocity.ToAngle();
				dist = targetVelocity.magnitude * airTime;
			}
			else
			{
				startAngle = RandomAngle();
				dist = 5f * airTime;
			}
			float angle = SubdivideCircle(startAngle, 4, i);
			Vector2 targetPoint = BulletManager.PlayerPosition() + BraveMathCollege.DegreesToVector(angle, dist);
			float direction = (targetPoint - base.Position).ToAngle();
			if (i > 0)
			{
				direction += Random.Range(-12.5f, 12.5f);
			}
			Bullet bullet = new Bullet("grenade");
			Fire(new Direction(direction), new Speed(1f), bullet);
			(bullet.Projectile as ArcProjectile).AdjustSpeedToHit(targetPoint);
			bullet.Projectile.ImmuneToSustainedBlanks = true;
		}
	}
}
