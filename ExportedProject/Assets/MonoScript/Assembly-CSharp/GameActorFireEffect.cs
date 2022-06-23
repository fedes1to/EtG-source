using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameActorFireEffect : GameActorHealthEffect
{
	public const float BossMinResistance = 0.25f;

	public const float BossMaxResistance = 0.75f;

	public const float BossResistanceDelta = 0.025f;

	public List<GameObject> FlameVfx;

	public int flameNumPerSquareUnit = 10;

	public Vector2 flameBuffer = new Vector2(0.0625f, 0.0625f);

	public float flameFpsVariation = 0.5f;

	public float flameMoveChance = 0.2f;

	public bool IsGreenFire;

	private float m_particleTimer;

	private float m_emberCounter;

	public override bool ResistanceAffectsDuration
	{
		get
		{
			return true;
		}
	}

	public static RuntimeGameActorEffectData ApplyFlamesToTarget(GameActor actor, GameActorFireEffect sourceEffect)
	{
		RuntimeGameActorEffectData runtimeGameActorEffectData = new RuntimeGameActorEffectData();
		runtimeGameActorEffectData.actor = actor;
		return runtimeGameActorEffectData;
	}

	public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
		base.OnEffectApplied(actor, effectData, partialAmount);
		effectData.OnActorPreDeath = delegate
		{
			DestroyFlames(effectData);
		};
		actor.healthHaver.OnPreDeath += effectData.OnActorPreDeath;
		if (FlameVfx == null || FlameVfx.Count <= 0)
		{
			return;
		}
		if (effectData.vfxObjects == null)
		{
			effectData.vfxObjects = new List<Tuple<GameObject, float>>();
		}
		effectData.OnFlameAnimationCompleted = delegate(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip)
		{
			if (effectData.destroyVfx || !actor)
			{
				spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Remove(spriteAnimator.AnimationCompleted, effectData.OnFlameAnimationCompleted);
				UnityEngine.Object.Destroy(spriteAnimator.gameObject);
			}
			else
			{
				if (UnityEngine.Random.value < flameMoveChance)
				{
					Vector2 vector = actor.specRigidbody.HitboxPixelCollider.UnitDimensions / 2f;
					Vector2 vector2 = BraveUtility.RandomVector2(-vector + flameBuffer, vector - flameBuffer);
					Vector2 vector3 = actor.specRigidbody.HitboxPixelCollider.UnitCenter + vector2;
					spriteAnimator.transform.position = vector3;
				}
				spriteAnimator.Play(clip, 0f, clip.fps * UnityEngine.Random.Range(1f - flameFpsVariation, 1f + flameFpsVariation));
			}
		};
	}

	public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		base.EffectTick(actor, effectData);
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH && (bool)effectData.actor && effectData.actor.specRigidbody.HitboxPixelCollider != null)
		{
			Vector2 unitBottomLeft = effectData.actor.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
			Vector2 unitTopRight = effectData.actor.specRigidbody.HitboxPixelCollider.UnitTopRight;
			m_emberCounter += 30f * BraveTime.DeltaTime;
			if (m_emberCounter > 1f)
			{
				int num = Mathf.FloorToInt(m_emberCounter);
				m_emberCounter -= num;
				GlobalSparksDoer.DoRandomParticleBurst(num, unitBottomLeft, unitTopRight, new Vector3(1f, 1f, 0f), 120f, 0.75f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
			}
		}
		if ((bool)actor && (bool)actor.specRigidbody)
		{
			Vector2 unitDimensions = actor.specRigidbody.HitboxPixelCollider.UnitDimensions;
			Vector2 vector = unitDimensions / 2f;
			int num2 = Mathf.RoundToInt((float)flameNumPerSquareUnit * 0.5f * Mathf.Min(30f, Mathf.Min(unitDimensions.x * unitDimensions.y)));
			m_particleTimer += BraveTime.DeltaTime * (float)num2;
			if (m_particleTimer > 1f)
			{
				int num3 = Mathf.FloorToInt(m_particleTimer);
				Vector2 vector2 = actor.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
				Vector2 vector3 = actor.specRigidbody.HitboxPixelCollider.UnitTopRight;
				PixelCollider pixelCollider = actor.specRigidbody.GetPixelCollider(ColliderType.Ground);
				if (pixelCollider != null && pixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.Manual)
				{
					vector2 = Vector2.Min(vector2, pixelCollider.UnitBottomLeft);
					vector3 = Vector2.Max(vector3, pixelCollider.UnitTopRight);
				}
				vector2 += Vector2.Min(vector * 0.15f, new Vector2(0.25f, 0.25f));
				vector3 -= Vector2.Min(vector * 0.15f, new Vector2(0.25f, 0.25f));
				vector3.y -= Mathf.Min(vector.y * 0.1f, 0.1f);
				GlobalSparksDoer.DoRandomParticleBurst(num3, vector2, vector3, Vector3.zero, 0f, 0f, null, null, null, (!IsGreenFire) ? GlobalSparksDoer.SparksType.STRAIGHT_UP_FIRE : GlobalSparksDoer.SparksType.STRAIGHT_UP_GREEN_FIRE);
				m_particleTimer -= Mathf.Floor(m_particleTimer);
			}
		}
		if (actor.IsGone)
		{
			effectData.elapsed = 10000f;
		}
		if ((actor.IsFalling || actor.IsGone) && effectData.vfxObjects != null && effectData.vfxObjects.Count > 0)
		{
			DestroyFlames(effectData);
		}
	}

	public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		base.OnEffectRemoved(actor, effectData);
		actor.healthHaver.OnPreDeath -= effectData.OnActorPreDeath;
		DestroyFlames(effectData);
	}

	public static void DestroyFlames(RuntimeGameActorEffectData effectData)
	{
		if (effectData.vfxObjects == null)
		{
			return;
		}
		if (!effectData.actor.IsFrozen)
		{
			for (int i = 0; i < effectData.vfxObjects.Count; i++)
			{
				GameObject first = effectData.vfxObjects[i].First;
				if ((bool)first)
				{
					first.transform.parent = SpawnManager.Instance.VFX;
				}
			}
		}
		effectData.vfxObjects.Clear();
		effectData.destroyVfx = true;
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH && (bool)effectData.actor && (bool)effectData.actor.healthHaver && effectData.actor.healthHaver.GetCurrentHealth() <= 0f && effectData.actor.specRigidbody.HitboxPixelCollider != null)
		{
			Vector2 unitBottomLeft = effectData.actor.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
			Vector2 unitTopRight = effectData.actor.specRigidbody.HitboxPixelCollider.UnitTopRight;
			float num = (unitTopRight.x - unitBottomLeft.x) * (unitTopRight.y - unitBottomLeft.y);
			GlobalSparksDoer.DoRandomParticleBurst(Mathf.Max(1, (int)(75f * num)), unitBottomLeft, unitTopRight, new Vector3(1f, 1f, 0f), 120f, 0.75f, null, null, null, GlobalSparksDoer.SparksType.EMBERS_SWIRLING);
		}
	}
}
