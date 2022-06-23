using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class PortableTurretController : MonoBehaviour
{
	[NonSerialized]
	public PlayerController sourcePlayer;

	public float maxDuration = 20f;

	private AIActor actor;

	private GameObject m_fallbackProjectile;

	private void Awake()
	{
		actor = GetComponent<AIActor>();
		actor.PreventFallingInPitsEver = true;
	}

	private void Start()
	{
		actor.CanTargetEnemies = true;
		actor.CanTargetPlayers = false;
		actor.ParentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
		actor.HasBeenEngaged = true;
		RoomHandler parentRoom = actor.ParentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom.OnEnemiesCleared, new Action(HandleRoomCleared));
		AIShooter aiShooter = actor.aiShooter;
		aiShooter.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(aiShooter.PostProcessProjectile, new Action<Projectile>(PostProcessProjectile));
		GetComponent<tk2dSpriteAnimator>().QueueAnimation("portable_turret_fire");
		actor.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.PlayerCollider));
		actor.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox));
		StartCoroutine(HandleTimedDestroy());
	}

	private void Update()
	{
		if ((bool)actor && actor.IsFalling)
		{
			GetComponent<tk2dSpriteAnimator>().Play("portable_turret_undeploy");
			tk2dSpriteAnimator component = GetComponent<tk2dSpriteAnimator>();
			component.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(component.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(Disappear));
		}
	}

	private void PostProcessProjectile(Projectile obj)
	{
		if (!sourcePlayer)
		{
			return;
		}
		sourcePlayer.DoPostProcessProjectile(obj);
		if (sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.TURRET_RANDOMIZER))
		{
			if (m_fallbackProjectile == null)
			{
				m_fallbackProjectile = actor.bulletBank.Bullets[0].BulletObject;
			}
			actor.bulletBank.Bullets[0].BulletObject = ProjectileRandomizerItem.GetRandomizerProjectileFromPlayer(sourcePlayer, m_fallbackProjectile.GetComponent<Projectile>(), 800).gameObject;
		}
		if (!sourcePlayer.HasActiveBonusSynergy(CustomSynergyType.CAPTAINPLANTIT))
		{
		}
	}

	public void NotifyDropped()
	{
		HandleRoomCleared();
	}

	private IEnumerator HandleTimedDestroy()
	{
		yield return new WaitForSeconds(maxDuration);
		GetComponent<tk2dSpriteAnimator>().Play("portable_turret_undeploy");
		tk2dSpriteAnimator component = GetComponent<tk2dSpriteAnimator>();
		component.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(component.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(Disappear));
	}

	private void HandleRoomCleared()
	{
		if ((bool)actor)
		{
			GetComponent<tk2dSpriteAnimator>().Play("portable_turret_undeploy");
			tk2dSpriteAnimator component = GetComponent<tk2dSpriteAnimator>();
			component.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(component.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(Disappear));
		}
	}

	private void Disappear(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2)
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
