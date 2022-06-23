using System;
using UnityEngine;

public class TeapotHealingModifier : MonoBehaviour
{
	public int AmmoCost = 24;

	private Projectile m_projectile;

	private void Awake()
	{
		m_projectile = GetComponent<Projectile>();
		m_projectile.allowSelfShooting = true;
		m_projectile.collidesWithEnemies = true;
		m_projectile.collidesWithPlayer = true;
		m_projectile.UpdateCollisionMask();
		SpeculativeRigidbody specRigidbody = m_projectile.specRigidbody;
		specRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreCollision));
	}

	private void HandlePreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!otherRigidbody)
		{
			return;
		}
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if ((bool)component && component != m_projectile.Owner && !component.IsGhost)
		{
			if ((bool)m_projectile.PossibleSourceGun)
			{
				component.healthHaver.ApplyHealing(0.5f);
				AkSoundEngine.PostEvent("Play_OBJ_heart_heal_01", base.gameObject);
				GameObject gameObject = BraveResources.Load<GameObject>("Global VFX/VFX_Healing_Sparkles_001");
				if (gameObject != null)
				{
					component.PlayEffectOnActor(gameObject, Vector3.zero);
				}
				m_projectile.PossibleSourceGun.LoseAmmo(AmmoCost);
				m_projectile.DieInAir();
			}
			PhysicsEngine.SkipCollision = true;
		}
		else if ((bool)component)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}
}
