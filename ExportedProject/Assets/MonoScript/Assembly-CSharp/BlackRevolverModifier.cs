using UnityEngine;

public class BlackRevolverModifier : MonoBehaviour
{
	public float WakeRadius = 3f;

	private Projectile m_projectile;

	private Gun m_gun;

	public void Start()
	{
		m_projectile = GetComponent<Projectile>();
		m_gun = m_projectile.PossibleSourceGun;
	}

	public void Update()
	{
		if (!m_gun || !m_gun.CurrentOwner || !m_projectile)
		{
			return;
		}
		Vector2 unitCenter = m_projectile.specRigidbody.UnitCenter;
		Vector2 direction = m_projectile.Direction;
		float num = WakeRadius * WakeRadius;
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.Owner != m_gun.CurrentOwner)
			{
				Vector2 vector = ((!projectile.specRigidbody) ? projectile.transform.position.XY() : projectile.specRigidbody.UnitCenter);
				float sqrMagnitude = (vector - unitCenter).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					Vector2 newDirection = direction;
					RedirectBullet(projectile, m_gun.CurrentOwner, newDirection, 10f);
				}
			}
		}
	}

	public void RedirectBullet(Projectile p, GameActor newOwner, Vector2 newDirection, float minReflectedBulletSpeed, float angleVariance = 0f, float scaleModifier = 1f, float damageModifier = 1f)
	{
		p.RemoveBulletScriptControl();
		p.Direction = newDirection.normalized;
		if (p.Direction == Vector2.zero)
		{
			p.Direction = Random.insideUnitCircle.normalized;
		}
		if (angleVariance != 0f)
		{
			p.Direction = p.Direction.Rotate(Random.Range(0f - angleVariance, angleVariance));
		}
		if ((bool)p.Owner && (bool)p.Owner.specRigidbody)
		{
			p.specRigidbody.DeregisterSpecificCollisionException(p.Owner.specRigidbody);
		}
		p.Owner = newOwner;
		p.SetNewShooter(newOwner.specRigidbody);
		p.allowSelfShooting = false;
		if (newOwner is AIActor)
		{
			p.collidesWithPlayer = true;
			p.collidesWithEnemies = false;
		}
		else
		{
			p.collidesWithPlayer = false;
			p.collidesWithEnemies = true;
		}
		if (scaleModifier != 1f)
		{
			p.RuntimeUpdateScale(scaleModifier);
		}
		if (p.Speed < minReflectedBulletSpeed)
		{
			p.Speed = minReflectedBulletSpeed;
		}
		if (p.baseData.damage < ProjectileData.FixedFallbackDamageToEnemies)
		{
			p.baseData.damage = ProjectileData.FixedFallbackDamageToEnemies;
		}
		p.baseData.damage *= damageModifier;
		p.UpdateCollisionMask();
		p.ResetDistance();
		p.Reflected();
	}
}
