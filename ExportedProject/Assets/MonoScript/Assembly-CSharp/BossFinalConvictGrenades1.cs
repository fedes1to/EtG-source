using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalConvict/Grenades1")]
public abstract class BossFinalConvictGrenades1 : Script
{
	private const int NumBullets = 5;

	private float? m_playerDist;

	private readonly float m_minAngle;

	private readonly float m_maxAngle;

	public BossFinalConvictGrenades1(float minAngle, float maxAngle)
	{
		m_minAngle = minAngle;
		m_maxAngle = maxAngle;
	}

	protected override IEnumerator Top()
	{
		int num = 2;
		FireGrenade(num);
		for (int i = 1; i <= 2; i++)
		{
			FireGrenade(num + i);
			FireGrenade(num - i);
		}
		return null;
	}

	private void FireGrenade(int i)
	{
		float num = Mathf.Lerp(m_minAngle, m_maxAngle, (float)i / 4f);
		Bullet bullet = new Bullet("grenade");
		Fire(new Direction(num), new Speed(1f), bullet);
		ArcProjectile arcProjectile = bullet.Projectile as ArcProjectile;
		float? playerDist = m_playerDist;
		if (!playerDist.HasValue)
		{
			float timeInFlight = arcProjectile.GetTimeInFlight();
			Vector2 b = BulletManager.PlayerPosition() + BulletManager.PlayerVelocity() * timeInFlight;
			m_playerDist = Vector2.Distance(base.Position, b);
		}
		arcProjectile.AdjustSpeedToHit(base.Position + BraveMathCollege.DegreesToVector(num, m_playerDist.Value));
	}
}
