using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MetalGearRat/LaserBullets1")]
public class MetalGearRatLaserBullets1 : Script
{
	public class LaserBullet : Bullet
	{
		protected override IEnumerator Top()
		{
			Projectile.IgnoreTileCollisionsFor(1.25f);
			yield return Wait(60);
			Direction = base.AimDirection;
			ChangeSpeed(new Speed(11f), 30);
		}
	}

	private const int NumBullets = 12;

	protected override IEnumerator Top()
	{
		AIBeamShooter[] beams = base.BulletBank.GetComponents<AIBeamShooter>();
		while (true)
		{
			yield return Wait(25);
			if (beams == null || beams.Length == 0)
			{
				break;
			}
			AIBeamShooter beam = beams[Random.Range(1, beams.Length)];
			if ((bool)beam && (bool)beam.LaserBeam)
			{
				Vector2 overridePosition = beam.LaserBeam.Origin + beam.LaserBeam.Direction.normalized * beam.MaxBeamLength;
				Fire(Offset.OverridePosition(overridePosition), new LaserBullet());
			}
		}
	}
}
