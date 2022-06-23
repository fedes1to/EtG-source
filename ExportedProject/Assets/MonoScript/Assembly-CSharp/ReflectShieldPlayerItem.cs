using System;
using System.Collections;

public class ReflectShieldPlayerItem : PlayerItem
{
	public float duration = 5f;

	protected SpeculativeRigidbody userSRB;

	private bool m_usedOverrideMaterial;

	protected override void DoEffect(PlayerController user)
	{
		userSRB = user.specRigidbody;
		user.StartCoroutine(HandleShield(user));
		AkSoundEngine.PostEvent("Play_OBJ_metalskin_activate_01", base.gameObject);
	}

	private IEnumerator HandleShield(PlayerController user)
	{
		base.IsCurrentlyActive = true;
		m_activeElapsed = 0f;
		m_activeDuration = duration;
		m_usedOverrideMaterial = user.sprite.usesOverrideMaterial;
		user.sprite.usesOverrideMaterial = true;
		user.SetOverrideShader(ShaderCache.Acquire("Brave/ItemSpecific/MetalSkinShader"));
		SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		user.healthHaver.IsVulnerable = false;
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			user.healthHaver.IsVulnerable = false;
			yield return null;
		}
		if ((bool)user)
		{
			user.healthHaver.IsVulnerable = true;
			user.ClearOverrideShader();
			user.sprite.usesOverrideMaterial = m_usedOverrideMaterial;
			SpeculativeRigidbody speculativeRigidbody2 = user.specRigidbody;
			speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
			base.IsCurrentlyActive = false;
		}
		if ((bool)this)
		{
			AkSoundEngine.PostEvent("Play_OBJ_metalskin_end_01", base.gameObject);
		}
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		Projectile component = otherRigidbody.GetComponent<Projectile>();
		if (component != null && !(component.Owner is PlayerController))
		{
			PassiveReflectItem.ReflectBullet(component, true, userSRB.gameActor, 10f);
			PhysicsEngine.SkipCollision = true;
		}
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (base.IsCurrentlyActive)
		{
			StopAllCoroutines();
			if ((bool)user)
			{
				user.healthHaver.IsVulnerable = true;
				user.ClearOverrideShader();
				user.sprite.usesOverrideMaterial = m_usedOverrideMaterial;
				SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
				speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
				base.IsCurrentlyActive = false;
			}
			if ((bool)this)
			{
				AkSoundEngine.PostEvent("Play_OBJ_metalskin_end_01", base.gameObject);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
