using System.Collections;
using UnityEngine;

public class GaseousFormPlayerItem : PlayerItem
{
	public float Duration = 5f;

	protected override void DoEffect(PlayerController user)
	{
		if ((bool)user)
		{
			user.StartCoroutine(HandleDuration(user));
			AkSoundEngine.PostEvent("Play_OBJ_metalskin_activate_01", base.gameObject);
		}
	}

	private void ChangeRendering(PlayerController user, bool val)
	{
		if (!user)
		{
			return;
		}
		if (val)
		{
			user.ChangeSpecialShaderFlag(0, 1f);
			user.FlatColorOverridden = true;
			user.ChangeFlatColorOverride(new Color(0.4f, 0.31f, 0.49f, 1f));
			user.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
			user.ToggleShadowVisiblity(false);
			SpriteOutlineManager.RemoveOutlineFromSprite(user.sprite, true);
			return;
		}
		user.ChangeSpecialShaderFlag(0, 0f);
		user.FlatColorOverridden = false;
		user.ChangeFlatColorOverride(Color.clear);
		user.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider));
		user.ToggleShadowVisiblity(true);
		if (!SpriteOutlineManager.HasOutline(user.sprite))
		{
			SpriteOutlineManager.AddOutlineToSprite(user.sprite, user.outlineColor, 0.1f);
		}
	}

	private IEnumerator HandleDuration(PlayerController user)
	{
		base.IsCurrentlyActive = true;
		m_activeElapsed = 0f;
		m_activeDuration = Duration;
		if ((bool)user && (bool)user.specRigidbody)
		{
			user.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider, CollisionLayer.Projectile));
		}
		if ((bool)user)
		{
			user.IsEthereal = true;
			if ((bool)user.healthHaver)
			{
				user.healthHaver.IsVulnerable = false;
			}
			user.SetIsFlying(true, "gaseousform");
			user.SetCapableOfStealing(true, "GaseousFormPlayerItem");
			ChangeRendering(user, true);
		}
		float elapsed = 0f;
		while (elapsed < Duration)
		{
			elapsed += BraveTime.DeltaTime;
			if ((bool)user && (bool)user.healthHaver)
			{
				user.healthHaver.IsVulnerable = false;
			}
			yield return null;
		}
		if ((bool)user)
		{
			ChangeRendering(user, false);
			user.SetIsFlying(false, "gaseousform");
			user.IsEthereal = false;
			if ((bool)user.healthHaver)
			{
				user.healthHaver.IsVulnerable = true;
			}
			user.SetCapableOfStealing(false, "GaseousFormPlayerItem");
			if ((bool)user.specRigidbody)
			{
				user.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider, CollisionLayer.Projectile));
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(user.specRigidbody);
			}
		}
		base.IsCurrentlyActive = false;
		if ((bool)this)
		{
			AkSoundEngine.PostEvent("Play_OBJ_metalskin_end_01", base.gameObject);
		}
	}

	protected override void OnPreDrop(PlayerController user)
	{
		if (!base.IsCurrentlyActive)
		{
			return;
		}
		StopAllCoroutines();
		if ((bool)user)
		{
			ChangeRendering(user, false);
			user.SetIsFlying(false, "gaseousform");
			user.IsEthereal = false;
			if ((bool)user.healthHaver)
			{
				user.healthHaver.IsVulnerable = true;
			}
			user.SetCapableOfStealing(false, "GaseousFormPlayerItem");
			if ((bool)user.specRigidbody)
			{
				user.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox, CollisionLayer.EnemyCollider, CollisionLayer.Projectile));
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(user.specRigidbody);
			}
			base.IsCurrentlyActive = false;
		}
		if ((bool)this)
		{
			AkSoundEngine.PostEvent("Play_OBJ_metalskin_end_01", base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
