using System;
using System.Collections;
using UnityEngine;

public class ActiveShieldItem : PlayerItem
{
	public GameObject prefabToAttachToPlayer;

	public float MaxShieldTime = 7f;

	public float DurationPortionToFlicker = 2f;

	private GameObject instanceShield;

	private tk2dSprite instanceShieldSprite;

	protected override void DoEffect(PlayerController user)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			AkSoundEngine.PostEvent("Play_OBJ_item_throw_01", base.gameObject);
			base.IsCurrentlyActive = true;
			instanceShield = user.RegisterAttachedObject(prefabToAttachToPlayer, "jetpack");
			instanceShieldSprite = instanceShield.GetComponentInChildren<tk2dSprite>();
			if (user.HasActiveBonusSynergy(CustomSynergyType.MIRROR_SHIELD))
			{
				instanceShieldSprite.spriteAnimator.Play("shield2_on");
			}
			user.ChangeAttachedSpriteDepth(instanceShieldSprite, -1f);
			SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PreventBulletCollisions));
			user.specRigidbody.BlockBeams = true;
			user.MovementModifiers += NoMotionModifier;
			user.IsStationary = true;
			user.IsGunLocked = true;
			user.OnPreDodgeRoll += HandleDodgeRollStarted;
			user.OnTriedToInitiateAttack += HandleTriedAttack;
			user.StartCoroutine(HandleDuration(user));
		}
	}

	private IEnumerator HandleDuration(PlayerController user)
	{
		if ((bool)user && user.HasActiveBonusSynergy(CustomSynergyType.MIRROR_SHIELD))
		{
			yield return null;
			if ((bool)instanceShieldSprite)
			{
				instanceShieldSprite.spriteAnimator.Play("shield2_on");
			}
		}
		float ela = 0f;
		while (ela < MaxShieldTime && base.IsCurrentlyActive)
		{
			ela = (m_activeElapsed = ela + BraveTime.DeltaTime);
			m_activeDuration = MaxShieldTime;
			if (ela > MaxShieldTime - DurationPortionToFlicker)
			{
				bool flag = ela * 6f % 1f > 0.5f;
				if ((bool)instanceShieldSprite)
				{
					instanceShieldSprite.renderer.enabled = flag;
				}
			}
			yield return null;
		}
		if (base.IsCurrentlyActive)
		{
			if ((bool)instanceShieldSprite)
			{
				instanceShieldSprite.renderer.enabled = true;
				instanceShieldSprite.HeightOffGround = 0.5f;
				instanceShieldSprite.UpdateZDepth();
			}
			DoActiveEffect(user);
		}
	}

	private void HandleTriedAttack(PlayerController obj)
	{
		DoActiveEffect(obj);
	}

	private void HandleDodgeRollStarted(PlayerController obj)
	{
		DoActiveEffect(obj);
	}

	private void PreventBulletCollisions(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if ((bool)otherRigidbody.projectile)
		{
			if ((bool)LastOwner && LastOwner.HasActiveBonusSynergy(CustomSynergyType.MIRROR_SHIELD))
			{
				PassiveReflectItem.ReflectBullet(otherRigidbody.projectile, true, LastOwner, 10f);
			}
			else
			{
				otherRigidbody.projectile.DieInAir();
			}
			PhysicsEngine.SkipCollision = true;
		}
		if ((bool)otherRigidbody.aiActor)
		{
			if ((bool)otherRigidbody.knockbackDoer)
			{
				otherRigidbody.knockbackDoer.ApplyKnockback(otherRigidbody.UnitCenter - myRigidbody.UnitCenter, 25f);
			}
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void LateUpdate()
	{
		if (base.IsCurrentlyActive)
		{
			instanceShieldSprite.HeightOffGround = 0.5f;
			instanceShieldSprite.UpdateZDepth();
		}
	}

	private void NoMotionModifier(ref Vector2 voluntaryVel, ref Vector2 involuntaryVel)
	{
		voluntaryVel = Vector2.zero;
	}

	protected override void DoActiveEffect(PlayerController user)
	{
		base.IsCurrentlyActive = false;
		user.MovementModifiers -= NoMotionModifier;
		SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PreventBulletCollisions));
		user.specRigidbody.BlockBeams = false;
		Transform parent = instanceShield.transform.parent;
		user.DeregisterAttachedObject(instanceShield, false);
		user.IsStationary = false;
		user.IsGunLocked = false;
		user.OnPreDodgeRoll -= HandleDodgeRollStarted;
		user.OnTriedToInitiateAttack -= HandleTriedAttack;
		instanceShield.transform.parent = parent;
		instanceShieldSprite.spriteAnimator.Play((!user || !user.HasActiveBonusSynergy(CustomSynergyType.MIRROR_SHIELD)) ? "shield_off" : "shield2_off");
		tk2dSpriteAnimator obj = instanceShieldSprite.spriteAnimator;
		obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DestroyParentObject));
		instanceShieldSprite = null;
	}

	private void DestroyParentObject(tk2dSpriteAnimator source, tk2dSpriteAnimationClip clip)
	{
		UnityEngine.Object.Destroy(source.transform.parent.gameObject);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			DoActiveEffect(user);
		}
	}

	public override void OnItemSwitched(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			DoActiveEffect(user);
		}
	}

	protected override void OnDestroy()
	{
		if (LastOwner != null)
		{
			LastOwner.OnPreDodgeRoll -= HandleDodgeRollStarted;
			LastOwner.OnTriedToInitiateAttack -= HandleTriedAttack;
		}
		if (base.IsCurrentlyActive)
		{
			DoActiveEffect(LastOwner);
		}
		base.OnDestroy();
	}
}
