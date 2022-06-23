using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

[Serializable]
public class GrappleModule
{
	public GameObject GrapplePrefab;

	public float GrappleSpeed = 10f;

	public float GrappleRetractSpeed = 10f;

	public float DamageToEnemies = 10f;

	public float EnemyKnockbackForce = 10f;

	public GameObject sourceGameObject;

	public Action FinishedCallback;

	private GameObject m_extantGrapple;

	private bool m_hasImpactedTile;

	private bool m_hasImpactedEnemy;

	private bool m_hasImpactedShopItem;

	private bool m_hasImpactedItem;

	private bool m_tileImpactFake;

	private AIActor m_impactedEnemy;

	private PickupObject m_impactedItem;

	private ShopItemController m_impactedShopItem;

	private bool m_isDone;

	private PlayerController m_lastUser;

	private Coroutine m_lastCoroutine;

	public void Trigger(PlayerController user)
	{
		m_lastUser = user;
		m_extantGrapple = UnityEngine.Object.Instantiate(GrapplePrefab);
		m_hasImpactedEnemy = false;
		m_hasImpactedTile = false;
		m_hasImpactedItem = false;
		m_hasImpactedShopItem = false;
		m_tileImpactFake = false;
		m_isDone = false;
		tk2dTiledSprite componentInChildren = m_extantGrapple.GetComponentInChildren<tk2dTiledSprite>();
		componentInChildren.dimensions = new Vector2(3f, componentInChildren.dimensions.y);
		m_extantGrapple.transform.position = user.CenterPosition.ToVector3ZUp();
		m_lastCoroutine = user.StartCoroutine(HandleGrappleEffect(user));
	}

	public void MarkDone()
	{
		m_isDone = true;
	}

	public void ForceEndGrapple()
	{
		if (!m_isDone)
		{
			if (m_lastUser != null)
			{
				m_lastUser.healthHaver.IsVulnerable = true;
				m_lastUser.SetIsFlying(false, "grapple", false);
				m_lastUser.CurrentInputState = PlayerInputState.AllInput;
			}
			m_isDone = true;
			m_lastUser = null;
			PhysicsEngine.Instance.OnPostRigidbodyMovement -= PostMovementUpdate;
		}
	}

	public void ForceEndGrappleImmediate()
	{
		if (m_isDone)
		{
			return;
		}
		if (m_lastUser != null)
		{
			if (m_lastCoroutine != null)
			{
				m_lastUser.StopCoroutine(m_lastCoroutine);
			}
			m_lastUser.healthHaver.IsVulnerable = true;
			m_lastUser.SetIsFlying(false, "grapple", false);
			m_lastUser.CurrentInputState = PlayerInputState.AllInput;
		}
		m_isDone = true;
		m_lastUser = null;
		PhysicsEngine.Instance.OnPostRigidbodyMovement -= PostMovementUpdate;
		UnityEngine.Object.Destroy(m_extantGrapple);
		m_extantGrapple = null;
		if (FinishedCallback != null)
		{
			FinishedCallback();
		}
	}

	public void ClearExtantGrapple()
	{
		if (m_extantGrapple != null)
		{
			UnityEngine.Object.Destroy(m_extantGrapple);
			m_extantGrapple = null;
		}
	}

	protected IEnumerator HandleGrappleEffect(PlayerController user)
	{
		SpeculativeRigidbody grappleRigidbody = m_extantGrapple.GetComponent<SpeculativeRigidbody>();
		grappleRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyBulletBlocker));
		PhysicsEngine.Instance.OnPostRigidbodyMovement += PostMovementUpdate;
		Vector2 startPoint = user.CenterPosition;
		Vector2 aimDirection = user.unadjustedAimPoint.XY() - startPoint;
		grappleRigidbody.RegisterSpecificCollisionException(user.specRigidbody);
		grappleRigidbody.transform.position = startPoint.ToVector3ZUp();
		grappleRigidbody.Velocity = aimDirection.normalized * GrappleSpeed;
		grappleRigidbody.Reinitialize();
		grappleRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(grappleRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(ImpactedTile));
		grappleRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(grappleRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(ImpactedRigidbody));
		grappleRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(grappleRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		tk2dTiledSprite chainSprite = grappleRigidbody.GetComponentInChildren<tk2dTiledSprite>();
		chainSprite.dimensions = new Vector2(3f, chainSprite.dimensions.y);
		m_isDone = false;
		float totalDistanceToGrapple = -1f;
		float grappledDistance = 0f;
		while (!m_isDone)
		{
			if (m_extantGrapple == null)
			{
				yield break;
			}
			if ((bool)user && (bool)user.healthHaver && user.healthHaver.IsDead)
			{
				break;
			}
			Vector2 currentDirVec = grappleRigidbody.UnitCenter - user.CenterPosition;
			int pixelsWide = Mathf.RoundToInt(currentDirVec.magnitude / 0.0625f);
			chainSprite.dimensions = new Vector2(pixelsWide, chainSprite.dimensions.y);
			float currentChainSpriteAngle = BraveMathCollege.Atan2Degrees(currentDirVec);
			grappleRigidbody.transform.rotation = Quaternion.Euler(0f, 0f, currentChainSpriteAngle);
			IPlayerInteractable nearestIxable = user.CurrentRoom.GetNearestInteractable(grappleRigidbody.UnitCenter, 1f, user);
			if (nearestIxable is PickupObject && !(nearestIxable as PickupObject).IsBeingEyedByRat)
			{
				AkSoundEngine.PostEvent("Play_WPN_metalbullet_impact_01", sourceGameObject);
				grappleRigidbody.CollideWithOthers = false;
				m_hasImpactedItem = true;
				m_impactedItem = nearestIxable as PickupObject;
			}
			if (m_hasImpactedEnemy)
			{
				m_impactedEnemy.healthHaver.ApplyDamage(DamageToEnemies, currentDirVec.normalized, "Grapple");
				if ((bool)m_impactedEnemy.knockbackDoer)
				{
					m_impactedEnemy.knockbackDoer.ApplyKnockback(currentDirVec.normalized, EnemyKnockbackForce);
				}
				if ((bool)m_impactedEnemy.behaviorSpeculator)
				{
					m_impactedEnemy.behaviorSpeculator.Stun(3f);
				}
				m_isDone = true;
			}
			else if (m_hasImpactedItem)
			{
				AkSoundEngine.PostEvent("Play_WPN_metalbullet_impact_01", sourceGameObject);
				if (totalDistanceToGrapple == -1f)
				{
					totalDistanceToGrapple = currentDirVec.magnitude;
				}
				grappledDistance += GrappleSpeed * BraveTime.DeltaTime;
				grappleRigidbody.Velocity = (user.CenterPosition - grappleRigidbody.UnitCenter).normalized * GrappleSpeed;
				if (m_impactedItem.specRigidbody != null)
				{
					m_impactedItem.specRigidbody.Velocity = grappleRigidbody.Velocity;
				}
				else
				{
					m_impactedItem.sprite.PlaceAtPositionByAnchor(grappleRigidbody.UnitCenter, tk2dBaseSprite.Anchor.MiddleCenter);
				}
				if (grappledDistance >= totalDistanceToGrapple || Vector2.Distance(user.specRigidbody.UnitCenter, grappleRigidbody.UnitCenter) < 0.5f)
				{
					if ((bool)m_impactedItem && (bool)m_impactedItem.specRigidbody)
					{
						m_impactedItem.specRigidbody.Velocity = Vector2.zero;
					}
					m_isDone = true;
				}
			}
			else if (m_hasImpactedShopItem)
			{
				if (totalDistanceToGrapple == -1f)
				{
					totalDistanceToGrapple = currentDirVec.magnitude;
				}
				grappledDistance += GrappleSpeed * BraveTime.DeltaTime;
				grappleRigidbody.Velocity = (user.CenterPosition - grappleRigidbody.UnitCenter).normalized * GrappleSpeed;
				if ((bool)m_impactedShopItem)
				{
					m_impactedShopItem.sprite.PlaceAtPositionByAnchor(grappleRigidbody.UnitCenter, tk2dBaseSprite.Anchor.MiddleCenter);
				}
				if (grappledDistance >= totalDistanceToGrapple || Vector2.Distance(user.specRigidbody.UnitCenter, grappleRigidbody.UnitCenter) < 0.5f)
				{
					if ((bool)m_impactedItem && (bool)m_impactedItem.specRigidbody)
					{
						m_impactedItem.specRigidbody.Velocity = Vector2.zero;
					}
					m_isDone = true;
					if ((bool)m_impactedShopItem)
					{
						m_impactedShopItem.ForceSteal(m_lastUser);
					}
				}
			}
			else if (m_hasImpactedTile)
			{
				AkSoundEngine.PostEvent("Play_OBJ_hook_pull_01", sourceGameObject);
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.HELLGEON && (bool)user && user.CurrentRoom != null && user.CurrentRoom.AdditionalRoomState == RoomHandler.CustomRoomState.LICH_PHASE_THREE)
				{
					m_tileImpactFake = true;
				}
				if (m_tileImpactFake)
				{
					m_isDone = true;
				}
				else
				{
					if (totalDistanceToGrapple == -1f)
					{
						if (user.HasActiveBonusSynergy(CustomSynergyType.NINJA_TOOLS))
						{
							user.PostProcessProjectile += HandleNinjaToolsSynergy;
						}
						totalDistanceToGrapple = currentDirVec.magnitude;
					}
					user.healthHaver.IsVulnerable = false;
					user.CurrentInputState = PlayerInputState.NoMovement;
					user.SetIsFlying(true, "grapple", false);
					user.specRigidbody.Velocity = currentDirVec.normalized * GrappleRetractSpeed;
					grappledDistance += GrappleRetractSpeed * BraveTime.DeltaTime;
					if (grappledDistance >= totalDistanceToGrapple || Vector2.Distance(user.specRigidbody.UnitCenter, grappleRigidbody.UnitCenter) < 1.5f)
					{
						m_isDone = true;
						user.PostProcessProjectile -= HandleNinjaToolsSynergy;
					}
				}
			}
			yield return null;
		}
		if ((bool)user)
		{
			user.PostProcessProjectile -= HandleNinjaToolsSynergy;
		}
		m_lastUser = null;
		PhysicsEngine.Instance.OnPostRigidbodyMovement -= PostMovementUpdate;
		UnityEngine.Object.Destroy(m_extantGrapple);
		m_extantGrapple = null;
		if (FinishedCallback != null)
		{
			FinishedCallback();
		}
		user.healthHaver.IsVulnerable = true;
		user.SetIsFlying(false, "grapple", false);
		user.CurrentInputState = PlayerInputState.AllInput;
	}

	private void HandleNinjaToolsSynergy(Projectile sourceProjectile, float beamEffectPercentage)
	{
		HomingModifier homingModifier = sourceProjectile.GetComponent<HomingModifier>();
		if (homingModifier == null)
		{
			homingModifier = sourceProjectile.gameObject.AddComponent<HomingModifier>();
			homingModifier.HomingRadius = 0f;
			homingModifier.AngularVelocity = 0f;
		}
		homingModifier.HomingRadius = Mathf.Max(12f, homingModifier.HomingRadius);
		homingModifier.AngularVelocity = Mathf.Max(720f, homingModifier.HomingRadius);
	}

	protected virtual void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (m_hasImpactedItem || m_hasImpactedEnemy)
		{
			PhysicsEngine.SkipCollision = true;
		}
		else if (otherRigidbody.GetComponent<PlayerController>() != null)
		{
			PhysicsEngine.SkipCollision = true;
		}
		else if (otherRigidbody.GetComponent<MinorBreakable>() != null)
		{
			if (!otherRigidbody.GetComponent<MinorBreakable>().IsBroken)
			{
				otherRigidbody.GetComponent<MinorBreakable>().Break();
			}
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void ImpactedRigidbody(CollisionData rigidbodyCollision)
	{
		if ((bool)rigidbodyCollision.OtherRigidbody.aiActor)
		{
			m_impactedEnemy = rigidbodyCollision.OtherRigidbody.aiActor;
			m_hasImpactedEnemy = true;
			rigidbodyCollision.MyRigidbody.Velocity = Vector2.zero;
			return;
		}
		ShopItemController component = rigidbodyCollision.OtherRigidbody.GetComponent<ShopItemController>();
		if ((bool)component)
		{
			AkSoundEngine.PostEvent("Play_WPN_metalbullet_impact_01", sourceGameObject);
			m_impactedShopItem = (component.Locked ? null : component);
			m_hasImpactedShopItem = true;
			component.specRigidbody.enabled = false;
			rigidbodyCollision.MyRigidbody.Velocity = Vector2.zero;
		}
		else
		{
			m_hasImpactedTile = true;
			rigidbodyCollision.MyRigidbody.Velocity = Vector2.zero;
		}
	}

	private void ImpactedTile(CollisionData tileCollision)
	{
		m_hasImpactedTile = true;
		tileCollision.MyRigidbody.Velocity = Vector2.zero;
	}

	private void PostMovementUpdate()
	{
		if ((bool)m_lastUser && (bool)m_extantGrapple)
		{
			SpeculativeRigidbody component = m_extantGrapple.GetComponent<SpeculativeRigidbody>();
			tk2dTiledSprite componentInChildren = component.GetComponentInChildren<tk2dTiledSprite>();
			Vector2 v = component.UnitCenter - m_lastUser.CenterPosition;
			int num = Mathf.RoundToInt(v.magnitude / 0.0625f);
			componentInChildren.dimensions = new Vector2(num, componentInChildren.dimensions.y);
			float z = BraveMathCollege.Atan2Degrees(v);
			component.transform.rotation = Quaternion.Euler(0f, 0f, z);
		}
	}
}
