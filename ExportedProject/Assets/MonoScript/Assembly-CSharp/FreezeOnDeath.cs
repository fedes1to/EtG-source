using System;
using UnityEngine;

public class FreezeOnDeath : BraveBehaviour
{
	[CheckDirectionalAnimation(null)]
	public string deathFreezeAnim;

	[CheckDirectionalAnimation(null)]
	public string deathShatterAnim;

	[CheckDirectionalAnimation(null)]
	public string deathInstantShatterAnim;

	public GameObject shatterVfx;

	public bool IsDisintegrating { get; set; }

	public bool IsDeathFrozen { get; set; }

	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnPreDeath;
	}

	protected override void OnDestroy()
	{
		if ((bool)base.spriteAnimator)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DeathCompleted));
		}
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		}
		StaticReferenceManager.AllCorpses.Add(base.gameObject);
		base.OnDestroy();
	}

	private void OnPreDeath(Vector2 dir)
	{
		if ((bool)base.aiActor && (bool)base.healthHaver && base.aiActor.IsFalling)
		{
			base.healthHaver.ManualDeathHandling = false;
			return;
		}
		base.aiAnimator.PlayUntilCancelled(deathFreezeAnim, true);
		IsDeathFrozen = true;
		base.aiActor.IsFrozen = true;
		base.aiActor.ForceDeath(Vector2.zero, false);
		base.aiActor.ImmuneToAllEffects = true;
		base.aiActor.RemoveAllEffects(true);
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		StaticReferenceManager.AllCorpses.Add(base.gameObject);
	}

	private void OnCollision(CollisionData collisionData)
	{
		if (!collisionData.OtherRigidbody)
		{
			return;
		}
		if ((bool)collisionData.OtherRigidbody.projectile)
		{
			DoFullDeath(deathShatterAnim);
			return;
		}
		PlayerController component = collisionData.OtherRigidbody.GetComponent<PlayerController>();
		if ((bool)component && component.IsDodgeRolling)
		{
			DoFullDeath(deathInstantShatterAnim);
		}
	}

	private void DeathVfxTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (frame.eventInfo == "vfx")
		{
			SpawnManager.SpawnVFX(shatterVfx, base.specRigidbody.HitboxPixelCollider.UnitCenter, Quaternion.identity);
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(DeathVfxTriggered));
		}
	}

	private void DeathCompleted(tk2dSpriteAnimator tk2DSpriteAnimator, tk2dSpriteAnimationClip tk2DSpriteAnimationClip)
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void HandleDisintegration()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		base.specRigidbody.enabled = false;
	}

	private void DoFullDeath(string deathAnim)
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Remove(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		base.specRigidbody.enabled = false;
		base.aiAnimator.PlayUntilCancelled(deathAnim, true);
		if ((bool)shatterVfx)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(DeathVfxTriggered));
		}
		tk2dSpriteAnimator obj2 = base.spriteAnimator;
		obj2.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj2.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DeathCompleted));
		StaticReferenceManager.AllCorpses.Remove(base.gameObject);
	}
}
