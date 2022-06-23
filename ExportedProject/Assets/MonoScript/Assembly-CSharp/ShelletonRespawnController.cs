using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

public class ShelletonRespawnController : BraveBehaviour
{
	private enum State
	{
		Normal,
		SkullRegeneration
	}

	public tk2dBaseSprite headSprite;

	public float skullHealth = 50f;

	public float skullTime = 8f;

	public float minDistFromPlayer = 4f;

	public string deathAnim;

	public string preRegenAnim;

	public string regenAnim;

	public string regenFromNothingAnim;

	[Header("Shell Sucking")]
	public float radius = 15f;

	public float gravityForce = 50f;

	public float destroyRadius = 0.2f;

	private float m_cachedStartingHealth;

	private float m_radiusSquared;

	private bool m_shouldShellSuck;

	private int m_numRegenerations;

	private int m_cachedHeadDefaultSpriteId;

	private State m_state;

	public void Start()
	{
		m_cachedStartingHealth = base.healthHaver.GetMaxHealth();
		m_radiusSquared = radius * radius;
		base.healthHaver.minimumHealth = 1f;
		base.healthHaver.OnDamaged += OnDamaged;
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		base.aiActor.CustomPitDeathHandling += CustomPitDeathHandling;
		m_cachedHeadDefaultSpriteId = headSprite.spriteId;
	}

	public void Update()
	{
		if (base.aiActor.IsFalling && base.behaviorSpeculator.enabled)
		{
			base.behaviorSpeculator.InterruptAndDisable();
		}
		if (m_shouldShellSuck)
		{
			for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
			{
				AdjustDebrisVelocity(StaticReferenceManager.AllDebris[i]);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (m_state == State.Normal && resultValue == 1f)
		{
			StartCoroutine(RegenerationCR());
		}
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (clip.GetFrame(frame).eventInfo == "shell_suck")
		{
			m_shouldShellSuck = true;
		}
	}

	private void CustomPitDeathHandling(AIActor actor, ref bool suppressDeath)
	{
		if (m_state == State.SkullRegeneration)
		{
			base.healthHaver.minimumHealth = 0f;
			base.healthHaver.IsVulnerable = true;
			return;
		}
		suppressDeath = true;
		Reposition();
		TileSpriteClipper[] componentsInChildren = GetComponentsInChildren<TileSpriteClipper>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		headSprite.SetSprite(m_cachedHeadDefaultSpriteId);
		base.aiActor.RecoverFromFall();
		StartCoroutine(RegenerateFromNothingCR());
	}

	private IEnumerator RegenerationCR()
	{
		m_state = State.SkullRegeneration;
		base.behaviorSpeculator.InterruptAndDisable();
		base.aiActor.ClearPath();
		base.aiActor.CollisionDamage = 0f;
		base.knockbackDoer.SetImmobile(true, "ShelletonRespawnController");
		base.specRigidbody.PixelColliders[1].Enabled = false;
		base.specRigidbody.PixelColliders[2].Enabled = true;
		base.healthHaver.SetHealthMaximum(skullHealth);
		base.healthHaver.ForceSetCurrentHealth(skullHealth);
		base.aiAnimator.PlayUntilCancelled(deathAnim);
		while (base.aiAnimator.IsPlaying(deathAnim))
		{
			yield return null;
		}
		base.healthHaver.minimumHealth = 0f;
		base.knockbackDoer.SetImmobile(false, "ShelletonRespawnController");
		base.aiActor.OverridePitfallAnim = "pitfall_head";
		yield return new WaitForSeconds(skullTime);
		if (base.aiActor.IsFalling || base.healthHaver.IsDead)
		{
			yield break;
		}
		base.aiAnimator.PlayUntilFinished(preRegenAnim);
		while (base.aiAnimator.IsPlaying(preRegenAnim))
		{
			yield return null;
			if (base.aiActor.IsFalling || base.healthHaver.IsDead)
			{
				yield break;
			}
		}
		base.healthHaver.IsVulnerable = false;
		base.healthHaver.SetHealthMaximum(m_cachedStartingHealth);
		base.healthHaver.ForceSetCurrentHealth(m_cachedStartingHealth);
		base.healthHaver.minimumHealth = 1f;
		base.knockbackDoer.SetImmobile(true, "ShelletonRespawnController");
		m_numRegenerations++;
		if (m_numRegenerations >= 2)
		{
			base.healthHaver.PreventCooldownGainFromDamage = true;
		}
		base.aiAnimator.PlayUntilFinished(regenAnim);
		while (base.aiAnimator.IsPlaying(regenAnim))
		{
			yield return null;
		}
		m_shouldShellSuck = false;
		base.aiActor.CollisionDamage = 0.5f;
		base.knockbackDoer.SetImmobile(false, "ShelletonRespawnController");
		base.specRigidbody.PixelColliders[1].Enabled = true;
		base.specRigidbody.PixelColliders[2].Enabled = false;
		base.aiActor.OverridePitfallAnim = null;
		base.healthHaver.IsVulnerable = true;
		base.behaviorSpeculator.enabled = true;
		m_state = State.Normal;
	}

	private IEnumerator RegenerateFromNothingCR()
	{
		m_state = State.SkullRegeneration;
		base.behaviorSpeculator.InterruptAndDisable();
		base.aiActor.ClearPath();
		base.aiActor.CollisionDamage = 0f;
		base.knockbackDoer.SetImmobile(true, "ShelletonRespawnController");
		base.specRigidbody.PixelColliders[1].Enabled = false;
		base.specRigidbody.PixelColliders[2].Enabled = false;
		base.healthHaver.IsVulnerable = false;
		base.aiAnimator.PlayUntilCancelled(regenFromNothingAnim);
		while (base.aiAnimator.IsPlaying(deathAnim))
		{
			yield return null;
		}
		base.healthHaver.minimumHealth = 1f;
		base.aiAnimator.PlayUntilFinished(regenAnim);
		while (base.aiAnimator.IsPlaying(regenAnim))
		{
			yield return null;
		}
		m_shouldShellSuck = false;
		base.aiActor.CollisionDamage = 0.5f;
		base.knockbackDoer.SetImmobile(false, "ShelletonRespawnController");
		base.specRigidbody.PixelColliders[1].Enabled = true;
		base.specRigidbody.PixelColliders[2].Enabled = false;
		base.aiActor.OverridePitfallAnim = null;
		base.healthHaver.IsVulnerable = true;
		base.behaviorSpeculator.enabled = true;
		m_state = State.Normal;
	}

	private bool AdjustDebrisVelocity(DebrisObject debris)
	{
		if (debris.IsPickupObject)
		{
			return false;
		}
		if (debris.GetComponent<BlackHoleDoer>() != null)
		{
			return false;
		}
		if (!debris.name.Contains("shell", true))
		{
			return false;
		}
		Vector2 a = debris.sprite.WorldCenter - base.specRigidbody.UnitCenter;
		float num = Vector2.SqrMagnitude(a);
		if (num > m_radiusSquared)
		{
			return false;
		}
		float num2 = Mathf.Sqrt(num);
		if (num2 < destroyRadius)
		{
			UnityEngine.Object.Destroy(debris.gameObject);
			return true;
		}
		Vector2 frameAccelerationForRigidbody = GetFrameAccelerationForRigidbody(debris.sprite.WorldCenter, num2, gravityForce);
		float num3 = Mathf.Clamp(BraveTime.DeltaTime, 0f, 0.02f);
		if (debris.HasBeenTriggered)
		{
			debris.ApplyVelocity(frameAccelerationForRigidbody * num3);
		}
		else if (num2 < radius / 2f)
		{
			debris.Trigger(frameAccelerationForRigidbody * num3, 0.5f);
		}
		return true;
	}

	private Vector2 GetFrameAccelerationForRigidbody(Vector2 unitCenter, float currentDistance, float g)
	{
		float num = Mathf.Clamp01(1f - currentDistance / radius);
		float num2 = g * num * num;
		return (base.specRigidbody.UnitCenter - unitCenter).normalized * num2;
	}

	private void Reposition()
	{
		Vector2 vector = BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
		Vector2 vector2 = BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
		IntVector2 bottomLeft = vector.ToIntVector2(VectorConversions.Ceil);
		IntVector2 topRight = vector2.ToIntVector2(VectorConversions.Floor) - IntVector2.One;
		PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
		Vector2 playerLowerLeft = bestActivePlayer.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
		Vector2 playerUpperRight = bestActivePlayer.specRigidbody.HitboxPixelCollider.UnitTopRight;
		bool hasOtherPlayer = false;
		Vector2 otherPlayerLowerLeft = Vector2.zero;
		Vector2 otherPlayerUpperRight = Vector2.zero;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(bestActivePlayer);
			if ((bool)otherPlayer && (bool)otherPlayer.healthHaver && otherPlayer.healthHaver.IsAlive)
			{
				hasOtherPlayer = true;
				otherPlayerLowerLeft = otherPlayer.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
				otherPlayerUpperRight = otherPlayer.specRigidbody.HitboxPixelCollider.UnitTopRight;
			}
		}
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < base.aiActor.Clearance.x; i++)
			{
				for (int j = 0; j < base.aiActor.Clearance.y; j++)
				{
					if (GameManager.Instance.Dungeon.data.isTopWall(c.x + i, c.y + j))
					{
						return false;
					}
				}
			}
			PixelCollider hitboxPixelCollider = base.aiActor.specRigidbody.HitboxPixelCollider;
			Vector2 vector4 = new Vector2((float)c.x + 0.5f * ((float)base.aiActor.Clearance.x - hitboxPixelCollider.UnitWidth), c.y);
			Vector2 aMax = vector4 + hitboxPixelCollider.UnitDimensions;
			if (BraveMathCollege.AABBDistanceSquared(vector4, aMax, playerLowerLeft, playerUpperRight) < minDistFromPlayer)
			{
				return false;
			}
			if (hasOtherPlayer && BraveMathCollege.AABBDistanceSquared(vector4, aMax, otherPlayerLowerLeft, otherPlayerUpperRight) < minDistFromPlayer)
			{
				return false;
			}
			return (c.x >= bottomLeft.x && c.y >= bottomLeft.y && c.x + base.aiActor.Clearance.x - 1 <= topRight.x && c.y + base.aiActor.Clearance.y - 1 <= topRight.y) ? true : false;
		};
		Vector2 vector3 = base.aiActor.specRigidbody.UnitCenter - base.aiActor.transform.position.XY();
		IntVector2? randomAvailableCell = base.aiActor.ParentRoom.GetRandomAvailableCell(base.aiActor.Clearance, base.aiActor.PathableTiles, false, cellValidator);
		if (randomAvailableCell.HasValue)
		{
			base.aiActor.transform.position = Pathfinder.GetClearanceOffset(randomAvailableCell.Value, base.aiActor.Clearance) - vector3;
			base.aiActor.specRigidbody.Reinitialize();
		}
		else
		{
			Debug.LogWarning("TELEPORT FAILED!", base.aiActor);
		}
	}
}
