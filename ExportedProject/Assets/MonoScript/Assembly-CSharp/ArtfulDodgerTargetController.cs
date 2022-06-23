using System;
using System.Collections;
using UnityEngine;

public class ArtfulDodgerTargetController : DungeonPlaceableBehaviour
{
	public GameObject HitVFX;

	public Renderer ShadowRenderer;

	[NonSerialized]
	public bool IsBroken;

	public GameObject Sparkles;

	private ArtfulDodgerRoomController m_artfulDodger;

	private void Start()
	{
		m_artfulDodger = GetAbsoluteParentRoom().GetComponentsAbsoluteInRoom<ArtfulDodgerRoomController>()[0];
		m_artfulDodger.RegisterTarget(this);
		base.sprite = GetComponentInChildren<tk2dSprite>();
		base.specRigidbody.enabled = false;
		base.sprite.renderer.enabled = false;
		ShadowRenderer.enabled = false;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
	}

	public void Activate()
	{
		StartCoroutine(HandleActivation());
	}

	private IEnumerator HandleActivation()
	{
		base.specRigidbody.enabled = true;
		yield return new WaitForSeconds(0.75f);
		PathMover m_pathMover = GetComponent<PathMover>();
		if ((bool)m_pathMover && m_pathMover.Path != null)
		{
			m_pathMover.Paused = false;
		}
		LootEngine.DoDefaultItemPoof(base.sprite.WorldCenter);
		base.sprite.renderer.enabled = true;
		ShadowRenderer.enabled = true;
		Sparkles.SetActive(true);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
	}

	public void ExplodeJoyously()
	{
		if (!IsBroken)
		{
			if ((bool)HitVFX)
			{
				SpawnManager.SpawnVFX(HitVFX, base.sprite.WorldCenter, Quaternion.identity);
			}
			IsBroken = true;
			base.specRigidbody.enabled = false;
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.renderer.enabled = false;
			ShadowRenderer.enabled = false;
			Sparkles.SetActive(false);
		}
	}

	public void DisappearSadly()
	{
		if (!IsBroken)
		{
			LootEngine.DoDefaultItemPoof(base.sprite.WorldCenter);
			IsBroken = true;
			base.specRigidbody.enabled = false;
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.renderer.enabled = false;
			ShadowRenderer.enabled = false;
			Sparkles.SetActive(false);
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (IsBroken || !(rigidbodyCollision.OtherRigidbody.projectile != null))
		{
			return;
		}
		Projectile projectile = rigidbodyCollision.OtherRigidbody.projectile;
		if (projectile.name.StartsWith("ArtfulDodger") || ((bool)projectile.PossibleSourceGun && projectile.PossibleSourceGun.name.StartsWith("ArtfulDodger")))
		{
			ArtfulDodgerProjectileController component = projectile.GetComponent<ArtfulDodgerProjectileController>();
			if (component != null)
			{
				component.hitTarget = true;
			}
			ExplodeJoyously();
			PierceProjModifier component2 = projectile.GetComponent<PierceProjModifier>();
			if (component2 == null)
			{
				projectile.DieInAir();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
