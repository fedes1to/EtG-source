using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameActorCheeseEffect : GameActorEffect
{
	public float CheeseAmount = 50f;

	public GoopDefinition CheeseGoop;

	public float CheeseGoopRadius = 1.5f;

	public List<GameObject> CheeseCrystals;

	[NonSerialized]
	public int crystalNum = 4;

	public int crystalRot = 8;

	public Vector2 crystalVariation = new Vector2(0.05f, 0.05f);

	public int debrisMinForce = 5;

	public int debrisMaxForce = 5;

	public float debrisAngleVariance = 15f;

	public GameObject vfxExplosion;

	public bool ShouldVanishOnDeath(GameActor actor)
	{
		if ((bool)actor.healthHaver && actor.healthHaver.IsBoss)
		{
			return false;
		}
		if (actor is AIActor && (actor as AIActor).IsSignatureEnemy)
		{
			return false;
		}
		return true;
	}

	public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
		if (!actor || actor.healthHaver.GetCurrentHealth() <= 0f || actor.healthHaver.IsDead)
		{
			return;
		}
		effectData.OnActorPreDeath = delegate(Vector2 dir)
		{
			if (actor.IsCheezen)
			{
				DestroyCrystals(effectData, !actor.IsFalling);
				AkSoundEngine.PostEvent("Play_BOSS_blobulord_burst_01", actor.gameObject);
				actor.CheeseAmount = 0f;
				DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(CheeseGoop).TimedAddGoopCircle(actor.CenterPosition, CheeseGoopRadius, 1f);
				if (ShouldVanishOnDeath(actor))
				{
					if (actor is AIActor)
					{
						(actor as AIActor).ForceDeath(dir, false);
					}
					UnityEngine.Object.Destroy(actor.gameObject);
				}
			}
		};
		actor.healthHaver.OnPreDeath += effectData.OnActorPreDeath;
		actor.CheeseAmount += CheeseAmount * partialAmount;
	}

	public override void OnDarkSoulsAccumulate(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f, Projectile sourceProjectile = null)
	{
		if (!effectData.actor.IsCheezen)
		{
			actor.CheeseAmount += CheeseAmount * partialAmount;
			if (actor.healthHaver.IsBoss)
			{
				actor.CheeseAmount = Mathf.Min(actor.CheeseAmount, 70f);
			}
		}
	}

	public override bool IsFinished(GameActor actor, RuntimeGameActorEffectData effectData, float elapsedTime)
	{
		return actor.CheeseAmount <= 0f;
	}

	public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		if (actor is AIActor && actor.IsCheezen)
		{
			actor.CheeseAmount = 0f;
			if (AppliesTint)
			{
				actor.DeregisterOverrideColor(effectIdentifier);
			}
			DestroyCrystals(effectData, !actor.healthHaver.IsDead);
			if ((bool)actor.behaviorSpeculator)
			{
				actor.behaviorSpeculator.enabled = true;
			}
			actor.IsCheezen = false;
		}
		if ((bool)actor.aiAnimator)
		{
			actor.aiAnimator.FpsScale = 1f;
		}
		actor.healthHaver.OnPreDeath -= effectData.OnActorPreDeath;
		tk2dSpriteAnimator spriteAnimator = actor.spriteAnimator;
		if ((bool)spriteAnimator && (bool)actor.aiAnimator && spriteAnimator.CurrentClip != null && !spriteAnimator.IsPlaying(spriteAnimator.CurrentClip))
		{
			actor.aiAnimator.PlayUntilFinished(actor.spriteAnimator.CurrentClip.name, false, null, -1f, true);
		}
	}

	public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		if ((bool)actor && actor.CheeseAmount > 0f)
		{
			actor.CheeseAmount = Mathf.Max(0f, actor.CheeseAmount - BraveTime.DeltaTime * actor.FreezeDispelFactor);
			if (!actor.IsCheezen)
			{
				if (actor.CheeseAmount > 100f && actor.healthHaver.IsAlive)
				{
					actor.CheeseAmount = 100f;
					if (CheeseCrystals.Count > 0)
					{
						if (effectData.vfxObjects == null)
						{
							effectData.vfxObjects = new List<Tuple<GameObject, float>>();
						}
						int num = crystalNum;
						if ((bool)effectData.actor && (bool)effectData.actor.specRigidbody && effectData.actor.specRigidbody.HitboxPixelCollider != null)
						{
							float num2 = effectData.actor.specRigidbody.HitboxPixelCollider.UnitDimensions.x * effectData.actor.specRigidbody.HitboxPixelCollider.UnitDimensions.y;
							num = Mathf.Max(crystalNum, (int)((float)crystalNum * (0.5f + num2 / 4f)));
						}
						for (int i = 0; i < num; i++)
						{
							GameObject prefab = BraveUtility.RandomElement(CheeseCrystals);
							Vector2 unitCenter = actor.specRigidbody.HitboxPixelCollider.UnitCenter;
							Vector2 vector = BraveUtility.RandomVector2(-crystalVariation, crystalVariation);
							unitCenter += vector;
							float num3 = BraveMathCollege.QuantizeFloat(vector.ToAngle(), 360f / (float)crystalRot);
							Quaternion rotation = Quaternion.Euler(0f, 0f, num3);
							GameObject gameObject = SpawnManager.SpawnVFX(prefab, unitCenter, rotation, true);
							gameObject.transform.parent = actor.transform;
							tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
							if ((bool)component)
							{
								actor.sprite.AttachRenderer(component);
								component.HeightOffGround = 0.1f;
							}
							if ((bool)effectData.actor && (bool)effectData.actor.specRigidbody && effectData.actor.specRigidbody.HitboxPixelCollider != null)
							{
								Vector2 unitCenter2 = effectData.actor.specRigidbody.HitboxPixelCollider.UnitCenter;
								float num4 = (float)i * (360f / (float)num);
								Vector2 normalized = BraveMathCollege.DegreesToVector(num4).normalized;
								normalized.x *= effectData.actor.specRigidbody.HitboxPixelCollider.UnitDimensions.x / 2f;
								normalized.y *= effectData.actor.specRigidbody.HitboxPixelCollider.UnitDimensions.y / 2f;
								float magnitude = normalized.magnitude;
								Vector2 vector2 = unitCenter2 + normalized;
								vector2 += (unitCenter2 - vector2).normalized * (magnitude * UnityEngine.Random.Range(0.15f, 0.85f));
								gameObject.transform.position = vector2.ToVector3ZUp();
								gameObject.transform.rotation = Quaternion.Euler(0f, 0f, num4);
							}
							effectData.vfxObjects.Add(Tuple.Create(gameObject, num3));
						}
					}
					if ((bool)actor.behaviorSpeculator)
					{
						if (actor.behaviorSpeculator.IsInterruptable)
						{
							actor.behaviorSpeculator.InterruptAndDisable();
						}
						else
						{
							actor.behaviorSpeculator.enabled = false;
						}
					}
					if (actor is AIActor)
					{
						AIActor aIActor = actor as AIActor;
						aIActor.ClearPath();
						aIActor.BehaviorOverridesVelocity = false;
					}
					actor.IsCheezen = true;
				}
			}
			else if (actor.IsCheezen)
			{
				if (actor.CheeseAmount <= 0f)
				{
					return;
				}
				if (actor.IsFalling)
				{
					if (effectData.vfxObjects != null && effectData.vfxObjects.Count > 0)
					{
						DestroyCrystals(effectData, false);
					}
					if ((bool)actor.aiAnimator)
					{
						actor.aiAnimator.FpsScale = 1f;
					}
				}
			}
		}
		if (actor.healthHaver.IsDead)
		{
			return;
		}
		float num5 = ((!actor.healthHaver.IsBoss) ? 100 : 70);
		float num6 = ((!actor.IsCheezen) ? Mathf.Clamp01((100f - actor.CheeseAmount) / 100f) : 0f);
		float num7 = ((!actor.IsCheezen) ? Mathf.Clamp01(actor.CheeseAmount / num5) : 1f);
		if ((bool)actor.aiAnimator)
		{
			actor.aiAnimator.FpsScale = ((!actor.IsFalling) ? num6 : 1f);
		}
		if (AppliesTint)
		{
			float num8 = actor.CheeseAmount / actor.FreezeDispelFactor;
			Color overrideColor = TintColor;
			if (num8 < 0.1f)
			{
				overrideColor = Color.black;
			}
			else if (num8 < 0.2f)
			{
				overrideColor = Color.white;
			}
			overrideColor.a *= num7;
			actor.RegisterOverrideColor(overrideColor, effectIdentifier);
		}
	}

	private void DestroyCrystals(RuntimeGameActorEffectData effectData, bool playVfxExplosion = true)
	{
		if (effectData.vfxObjects == null || effectData.vfxObjects.Count == 0)
		{
			return;
		}
		Vector2 vector = Vector2.zero;
		GameActor actor = effectData.actor;
		if ((bool)actor)
		{
			vector = ((!actor.specRigidbody) ? actor.sprite.WorldCenter : actor.specRigidbody.HitboxPixelCollider.UnitCenter);
		}
		else
		{
			int num = 0;
			for (int i = 0; i < effectData.vfxObjects.Count; i++)
			{
				if ((bool)effectData.vfxObjects[i].First)
				{
					vector += effectData.vfxObjects[i].First.transform.position.XY();
					num++;
				}
			}
			if (num == 0)
			{
				return;
			}
			vector /= (float)num;
		}
		if (playVfxExplosion && (bool)vfxExplosion)
		{
			GameObject gameObject = SpawnManager.SpawnVFX(vfxExplosion, vector, Quaternion.identity);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			if ((bool)actor && (bool)component)
			{
				actor.sprite.AttachRenderer(component);
				component.HeightOffGround = 0.1f;
				component.UpdateZDepth();
			}
		}
		for (int j = 0; j < effectData.vfxObjects.Count; j++)
		{
			GameObject first = effectData.vfxObjects[j].First;
			if ((bool)first)
			{
				first.transform.parent = SpawnManager.Instance.VFX;
				DebrisObject orAddComponent = first.GetOrAddComponent<DebrisObject>();
				if ((bool)actor)
				{
					actor.sprite.AttachRenderer(orAddComponent.sprite);
				}
				orAddComponent.sprite.IsPerpendicular = true;
				orAddComponent.DontSetLayer = true;
				orAddComponent.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Critical"));
				orAddComponent.angularVelocity = Mathf.Sign(UnityEngine.Random.value - 0.5f) * 125f;
				orAddComponent.angularVelocityVariance = 60f;
				orAddComponent.decayOnBounce = 0.5f;
				orAddComponent.bounceCount = 1;
				orAddComponent.canRotate = true;
				float num2 = effectData.vfxObjects[j].Second + UnityEngine.Random.Range(0f - debrisAngleVariance, debrisAngleVariance);
				if (orAddComponent.name.Contains("tilt", true))
				{
					num2 += 45f;
				}
				Vector2 vector2 = BraveMathCollege.DegreesToVector(num2) * UnityEngine.Random.Range(debrisMinForce, debrisMaxForce);
				Vector3 startingForce = new Vector3(vector2.x, (!(vector2.y < 0f)) ? 0f : vector2.y, (!(vector2.y > 0f)) ? 0f : vector2.y);
				float startingHeight = ((!actor) ? 0.75f : (first.transform.position.y - actor.specRigidbody.HitboxPixelCollider.UnitBottom));
				if ((bool)orAddComponent.minorBreakable)
				{
					orAddComponent.minorBreakable.enabled = true;
				}
				orAddComponent.Trigger(startingForce, startingHeight);
			}
		}
		effectData.vfxObjects.Clear();
	}
}
