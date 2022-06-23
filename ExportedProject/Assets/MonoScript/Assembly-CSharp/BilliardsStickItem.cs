using System;
using UnityEngine;

public class BilliardsStickItem : PassiveItem
{
	public float KnockbackForce = 800f;

	public float AngleTolerance = 30f;

	public Color TintColor = Color.white;

	public int TintPriority = 2;

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		player.PostProcessProjectile += HandlePostProcessProjectile;
		player.PostProcessBeam += HandlePostProcessBeam;
		player.PostProcessBeamTick += HandlePostProcessBeamTick;
	}

	private void HandlePostProcessProjectile(Projectile targetProjectile, float effectChanceScalar)
	{
		targetProjectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(targetProjectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		targetProjectile.AdjustPlayerProjectileTint(TintColor, TintPriority);
	}

	private void HandleHitEnemy(Projectile sourceProjectile, SpeculativeRigidbody hitRigidbody, bool fatal)
	{
		if (!fatal || !hitRigidbody)
		{
			return;
		}
		if ((bool)sourceProjectile)
		{
			sourceProjectile.baseData.force = 0f;
		}
		AIActor aIActor = hitRigidbody.aiActor;
		KnockbackDoer knockbackDoer = hitRigidbody.knockbackDoer;
		if ((bool)aIActor)
		{
			if ((bool)aIActor.GetComponent<ExplodeOnDeath>())
			{
				UnityEngine.Object.Destroy(aIActor.GetComponent<ExplodeOnDeath>());
			}
			hitRigidbody.AddCollisionLayerOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox));
			hitRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(hitRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandleHitEnemyHitEnemy));
		}
		if ((bool)knockbackDoer && (bool)sourceProjectile)
		{
			float nearestDistance = -1f;
			AIActor nearestEnemyInDirection = aIActor.ParentRoom.GetNearestEnemyInDirection(aIActor.CenterPosition, sourceProjectile.Direction, AngleTolerance, out nearestDistance);
			Vector2 direction = sourceProjectile.Direction;
			if ((bool)nearestEnemyInDirection)
			{
				direction = nearestEnemyInDirection.CenterPosition - aIActor.CenterPosition;
			}
			knockbackDoer.ApplyKnockback(direction, KnockbackForce, true);
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

	private void HandlePostProcessBeam(BeamController targetBeam)
	{
	}

	private void HandlePostProcessBeamTick(BeamController arg1, SpeculativeRigidbody arg2, float arg3)
	{
	}

	public override DebrisObject Drop(PlayerController player)
	{
		if ((bool)player)
		{
			player.PostProcessProjectile -= HandlePostProcessProjectile;
			player.PostProcessBeam -= HandlePostProcessBeam;
			player.PostProcessBeamTick -= HandlePostProcessBeamTick;
		}
		return base.Drop(player);
	}

	protected override void OnDestroy()
	{
		if ((bool)base.Owner)
		{
			base.Owner.PostProcessProjectile -= HandlePostProcessProjectile;
			base.Owner.PostProcessBeam -= HandlePostProcessBeam;
			base.Owner.PostProcessBeamTick -= HandlePostProcessBeamTick;
		}
		base.OnDestroy();
	}
}
