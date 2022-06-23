using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KatanaItem : PlayerItem
{
	public float dashDistance = 10f;

	public float dashSpeed = 30f;

	public float collisionDamage = 50f;

	public float stunDuration = 1f;

	public float momentaryPause = 0.25f;

	public float finalDelay = 0.5f;

	public int sequentialValidUses = 3;

	public GameObject trailVFXPrefab;

	public GameObject poofVFX;

	private bool m_isDashing;

	private int m_useCount;

	private List<AIActor> actorsPassed = new List<AIActor>();

	private List<MajorBreakable> breakablesPassed = new List<MajorBreakable>();

	protected override void DoEffect(PlayerController user)
	{
		AkSoundEngine.PostEvent("Play_CHR_ninja_dash_01", base.gameObject);
		if (!m_isDashing)
		{
			m_useCount++;
			StartCoroutine(HandleDash(user));
		}
	}

	protected override void AfterCooldownApplied(PlayerController user)
	{
		if (m_useCount >= sequentialValidUses)
		{
			m_useCount = 0;
		}
		else
		{
			ClearCooldowns();
		}
	}

	private float CalculateAdjustedDashDistance(PlayerController user, Vector2 dashDirection)
	{
		return dashDistance;
	}

	private IEnumerator HandleDash(PlayerController user)
	{
		m_isDashing = true;
		if (poofVFX != null)
		{
			user.PlayEffectOnActor(poofVFX, Vector3.zero, false);
		}
		Vector2 startPosition = user.sprite.WorldCenter;
		actorsPassed.Clear();
		breakablesPassed.Clear();
		user.IsVisible = false;
		user.SetInputOverride("katana");
		user.healthHaver.IsVulnerable = false;
		GameObject trailInstance = UnityEngine.Object.Instantiate(trailVFXPrefab, user.sprite.WorldCenter.ToVector3ZUp(), Quaternion.identity);
		trailInstance.transform.parent = user.transform;
		TrailController trail = trailInstance.GetComponent<TrailController>();
		trail.boneSpawnOffset = user.sprite.WorldCenter - user.specRigidbody.Position.UnitPosition;
		user.FallingProhibited = true;
		PixelCollider playerHitbox = user.specRigidbody.HitboxPixelCollider;
		playerHitbox.CollisionLayerCollidableOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
		SpeculativeRigidbody speculativeRigidbody = user.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(KatanaPreCollision));
		Vector2 dashDirection = BraveInput.GetInstanceForPlayer(user.PlayerIDX).ActiveActions.Move.Vector;
		float adjDashDistance = CalculateAdjustedDashDistance(user, dashDirection);
		float duration = Mathf.Max(0.0001f, adjDashDistance / dashSpeed);
		float elapsed = 0f - BraveTime.DeltaTime;
		while (elapsed < duration)
		{
			user.healthHaver.IsVulnerable = false;
			elapsed += BraveTime.DeltaTime;
			float adjSpeed = Mathf.Min(dashSpeed, adjDashDistance / BraveTime.DeltaTime);
			user.specRigidbody.Velocity = dashDirection.normalized * adjSpeed;
			yield return null;
		}
		user.IsVisible = true;
		user.ToggleGunRenderers(false, "katana");
		base.renderer.enabled = true;
		base.transform.localPosition = new Vector3(-0.125f, 0.125f, 0f);
		base.transform.localRotation = Quaternion.Euler(0f, 0f, 280f);
		if (poofVFX != null)
		{
			user.PlayEffectOnActor(poofVFX, Vector3.zero, false);
		}
		StartCoroutine(EndAndDamage(new List<AIActor>(actorsPassed), new List<MajorBreakable>(breakablesPassed), user, dashDirection, startPosition, user.sprite.WorldCenter));
		if (momentaryPause > 0f)
		{
			yield return new WaitForSeconds(finalDelay);
		}
		base.renderer.enabled = false;
		user.ToggleGunRenderers(true, "katana");
		playerHitbox.CollisionLayerCollidableOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
		SpeculativeRigidbody speculativeRigidbody2 = user.specRigidbody;
		speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(KatanaPreCollision));
		user.FallingProhibited = false;
		user.ClearInputOverride("katana");
		m_isDashing = false;
		trail.DisconnectFromSpecRigidbody();
	}

	private IEnumerator EndAndDamage(List<AIActor> actors, List<MajorBreakable> breakables, PlayerController user, Vector2 dashDirection, Vector2 startPosition, Vector2 endPosition)
	{
		yield return new WaitForSeconds(finalDelay);
		Exploder.DoLinearPush(user.sprite.WorldCenter, startPosition, 13f, 5f);
		user.healthHaver.IsVulnerable = true;
		for (int i = 0; i < actors.Count; i++)
		{
			if ((bool)actors[i])
			{
				actors[i].healthHaver.ApplyDamage(collisionDamage, dashDirection, "Katana");
			}
		}
		for (int j = 0; j < breakables.Count; j++)
		{
			if ((bool)breakables[j])
			{
				breakables[j].ApplyDamage(100f, dashDirection, false);
			}
		}
	}

	private void KatanaPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (otherRigidbody.projectile != null)
		{
			PhysicsEngine.SkipCollision = true;
		}
		if (otherRigidbody.aiActor != null)
		{
			PhysicsEngine.SkipCollision = true;
			if (!actorsPassed.Contains(otherRigidbody.aiActor))
			{
				otherRigidbody.aiActor.DelayActions(1f);
				actorsPassed.Add(otherRigidbody.aiActor);
			}
		}
		if (otherRigidbody.majorBreakable != null)
		{
			PhysicsEngine.SkipCollision = true;
			if (!breakablesPassed.Contains(otherRigidbody.majorBreakable))
			{
				breakablesPassed.Add(otherRigidbody.majorBreakable);
			}
		}
	}

	public override void OnItemSwitched(PlayerController user)
	{
		base.OnItemSwitched(user);
		if (m_useCount > 0)
		{
			m_useCount = 0;
			ApplyCooldown(user);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
