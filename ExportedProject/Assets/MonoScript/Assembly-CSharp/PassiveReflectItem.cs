using System;
using System.Collections.ObjectModel;
using UnityEngine;

public class PassiveReflectItem : PassiveItem
{
	public enum Condition
	{
		WhileDodgeRolling
	}

	public Condition condition;

	public float minReflectedBulletSpeed = 10f;

	public bool retargetReflectedBullet = true;

	public int AmmoGainedOnReflection;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			m_player = player;
			SpeculativeRigidbody speculativeRigidbody = player.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<PassiveReflectItem>().m_pickedUpThisRun = true;
		SpeculativeRigidbody speculativeRigidbody = player.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		return debrisObject;
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (condition == Condition.WhileDodgeRolling && !m_player.spriteAnimator.QueryInvulnerabilityFrame())
		{
			return;
		}
		Projectile component = otherRigidbody.GetComponent<Projectile>();
		if (!(component != null))
		{
			return;
		}
		ReflectBullet(component, retargetReflectedBullet, m_owner, minReflectedBulletSpeed);
		if (AmmoGainedOnReflection > 0)
		{
			Gun currentGun = m_owner.CurrentGun;
			if ((bool)currentGun && currentGun.CanGainAmmo)
			{
				currentGun.GainAmmo(AmmoGainedOnReflection);
			}
		}
		AkSoundEngine.PostEvent("Play_OBJ_metalskin_deflect_01", component.gameObject);
		otherRigidbody.transform.position += component.Direction.ToVector3ZUp() * 0.5f;
		otherRigidbody.Reinitialize();
		PhysicsEngine.SkipCollision = true;
	}

	public static int ReflectBulletsInRange(Vector2 centerPoint, float radius, bool retargetReflectedBulet, GameActor newOwner, float minReflectedBulletSpeed, float scaleModifier = 1f, float damageModifier = 1f, bool applyPostprocess = false)
	{
		int num = 0;
		float num2 = radius * radius;
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int i = 0; i < allProjectiles.Count; i++)
		{
			Projectile projectile = allProjectiles[i];
			if (((bool)projectile.Owner && projectile.Owner is PlayerController) || !projectile.specRigidbody || !projectile || !projectile.sprite)
			{
				continue;
			}
			float sqrMagnitude = (projectile.sprite.WorldCenter - centerPoint).sqrMagnitude;
			if (!(sqrMagnitude > num2))
			{
				ReflectBullet(projectile, retargetReflectedBulet, newOwner, minReflectedBulletSpeed, scaleModifier, damageModifier);
				num++;
				if (applyPostprocess && newOwner is PlayerController)
				{
					SpawnManager.PoolManager.Remove(projectile.transform);
					(newOwner as PlayerController).CustomPostProcessProjectile(projectile, 1f);
				}
			}
		}
		return num;
	}

	public static void ReflectBullet(Projectile p, bool retargetReflectedBullet, GameActor newOwner, float minReflectedBulletSpeed, float scaleModifier = 1f, float damageModifier = 1f, float spread = 0f)
	{
		p.RemoveBulletScriptControl();
		AkSoundEngine.PostEvent("Play_OBJ_metalskin_deflect_01", GameManager.Instance.gameObject);
		if (retargetReflectedBullet && (bool)p.Owner && (bool)p.Owner.specRigidbody)
		{
			p.Direction = (p.Owner.specRigidbody.GetUnitCenter(ColliderType.HitBox) - p.specRigidbody.UnitCenter).normalized;
		}
		else
		{
			Vector2 vector = p.LastVelocity;
			if (p.IsBulletScript && (bool)p.braveBulletScript && p.braveBulletScript.bullet != null)
			{
				vector = p.braveBulletScript.bullet.Velocity;
			}
			p.Direction = -vector.normalized;
			if (p.Direction == Vector2.zero)
			{
				p.Direction = UnityEngine.Random.insideUnitCircle.normalized;
			}
		}
		if (spread != 0f)
		{
			p.Direction = p.Direction.Rotate(UnityEngine.Random.Range(0f - spread, spread));
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
			SpawnManager.PoolManager.Remove(p.transform);
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
		if (p.baseData.damage < 10f)
		{
			p.baseData.damage = 15f;
		}
		p.UpdateCollisionMask();
		p.ResetDistance();
		p.Reflected();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
