using System;
using System.Collections;
using UnityEngine;

public class MeduziDeathController : BraveBehaviour
{
	public void Start()
	{
		base.healthHaver.ManualDeathHandling = true;
		base.healthHaver.OnPreDeath += OnBossDeath;
	}

	protected override void OnDestroy()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		base.OnDestroy();
	}

	public void Shatter()
	{
		base.aiAnimator.enabled = false;
		base.spriteAnimator.PlayAndDestroyObject("burst");
		base.specRigidbody.enabled = false;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
	}

	private void OnBossDeath(Vector2 dir)
	{
		base.aiAnimator.PlayUntilCancelled("death", true);
		StartCoroutine(HandlePostDeathExplosionCR());
		base.healthHaver.OnPreDeath -= OnBossDeath;
	}

	private IEnumerator HandlePostDeathExplosionCR()
	{
		while (base.aiAnimator.IsPlaying("death"))
		{
			yield return null;
		}
		base.aiActor.StealthDeath = true;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.DeathAnimationComplete(null, null);
		if ((bool)base.aiActor)
		{
			UnityEngine.Object.Destroy(base.aiActor);
		}
		if ((bool)base.healthHaver)
		{
			UnityEngine.Object.Destroy(base.healthHaver);
		}
		if ((bool)base.behaviorSpeculator)
		{
			UnityEngine.Object.Destroy(base.behaviorSpeculator);
		}
		RegenerateCache();
		base.specRigidbody.CollideWithOthers = true;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
	}

	private void OnRigidbodyCollision(CollisionData collision)
	{
		if ((bool)collision.OtherRigidbody.projectile)
		{
			Shatter();
		}
	}
}
