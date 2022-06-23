using System;
using UnityEngine;

public class HungryProjectileModifier : MonoBehaviour
{
	public float DamagePercentGainPerSnack = 0.25f;

	public float MaxMultiplier = 3f;

	public float HungryRadius = 3f;

	public int MaximumBulletsEaten = 10;

	private Projectile m_projectile;

	private int m_numberOfBulletsEaten;

	private bool m_sated;

	private void Awake()
	{
		m_projectile = GetComponent<Projectile>();
		m_projectile.AdjustPlayerProjectileTint(new Color(0.45f, 0.3f, 0.87f), 2);
		m_projectile.collidesWithProjectiles = true;
		SpeculativeRigidbody specRigidbody = m_projectile.specRigidbody;
		specRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreCollision));
	}

	private void Update()
	{
		if (m_sated)
		{
			return;
		}
		Vector2 vector = m_projectile.transform.position.XY();
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.Owner is AIActor)
			{
				float sqrMagnitude = (projectile.transform.position.XY() - vector).sqrMagnitude;
				if (sqrMagnitude < HungryRadius)
				{
					EatBullet(projectile);
				}
			}
		}
	}

	private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!m_sated && (bool)otherRigidbody && (bool)otherRigidbody.projectile)
		{
			if (otherRigidbody.projectile.Owner is AIActor)
			{
				EatBullet(otherRigidbody.projectile);
			}
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void EatBullet(Projectile other)
	{
		if (!m_sated)
		{
			other.DieInAir();
			float num = Mathf.Min(MaxMultiplier, 1f + (float)m_numberOfBulletsEaten * DamagePercentGainPerSnack);
			m_numberOfBulletsEaten++;
			float num2 = Mathf.Min(MaxMultiplier, 1f + (float)m_numberOfBulletsEaten * DamagePercentGainPerSnack);
			float b = num2 / num;
			float num3 = Mathf.Max(1f, b);
			if (num3 > 1f)
			{
				m_projectile.RuntimeUpdateScale(num3);
				m_projectile.baseData.damage *= num3;
			}
			if (m_numberOfBulletsEaten >= MaximumBulletsEaten)
			{
				m_sated = true;
				m_projectile.AdjustPlayerProjectileTint(m_projectile.DefaultTintColor, 3);
			}
		}
	}
}
