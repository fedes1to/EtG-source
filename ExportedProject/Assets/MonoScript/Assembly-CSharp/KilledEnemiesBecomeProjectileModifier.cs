using System;
using UnityEngine;

public class KilledEnemiesBecomeProjectileModifier : BraveBehaviour
{
	public bool CompletelyBecomeProjectile;

	public Projectile BaseProjectile;

	private Projectile m_projectile;

	public void Start()
	{
		m_projectile = base.projectile;
		if ((bool)m_projectile)
		{
			Projectile obj = m_projectile;
			obj.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(obj.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		}
	}

	private void HandleHitEnemy(Projectile sourceProjectile, SpeculativeRigidbody hitRigidbody, bool killedEnemy)
	{
		if (!killedEnemy || !hitRigidbody)
		{
			return;
		}
		AIActor aIActor = hitRigidbody.aiActor;
		if ((bool)aIActor && aIActor.IsNormalEnemy && (bool)aIActor.healthHaver && !aIActor.healthHaver.IsBoss)
		{
			if ((bool)aIActor.GetComponent<ExplodeOnDeath>())
			{
				UnityEngine.Object.Destroy(aIActor.GetComponent<ExplodeOnDeath>());
			}
			if (CompletelyBecomeProjectile && (bool)hitRigidbody.sprite)
			{
				aIActor.specRigidbody.enabled = false;
				aIActor.EraseFromExistence();
				GameObject gameObject = UnityEngine.Object.Instantiate(BaseProjectile.gameObject, aIActor.transform.position, Quaternion.Euler(0f, 0f, sourceProjectile.LastVelocity.ToAngle()));
				Projectile component = gameObject.GetComponent<Projectile>();
				tk2dBaseSprite tk2dBaseSprite2 = component.sprite;
				tk2dBaseSprite2.SetSprite(hitRigidbody.sprite.Collection, hitRigidbody.sprite.spriteId);
				component.shouldRotate = true;
			}
			else
			{
				hitRigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
				hitRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(hitRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandleHitEnemyHitEnemy));
			}
		}
	}

	private void HandleHitEnemyHitEnemy(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if ((bool)otherRigidbody && (bool)otherRigidbody.aiActor && (bool)myRigidbody && (bool)myRigidbody.healthHaver)
		{
			AIActor aIActor = otherRigidbody.aiActor;
			myRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(myRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandleHitEnemyHitEnemy));
			if (aIActor.IsNormalEnemy && (bool)aIActor.healthHaver)
			{
				aIActor.healthHaver.ApplyDamage(myRigidbody.healthHaver.GetMaxHealth() * 2f, myRigidbody.Velocity, "Pinball");
			}
		}
	}
}
