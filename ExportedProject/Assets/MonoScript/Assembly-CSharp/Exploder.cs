using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;
using UnityEngine;

public class Exploder : MonoBehaviour
{
	public static Action OnExplosionTriggered;

	private static bool ExplosionIsExtant;

	public static bool IsExplosionOccurring()
	{
		return ExplosionIsExtant || ExplosionManager.Instance.QueueCount > 0;
	}

	public static void Explode(Vector3 position, ExplosionData data, Vector2 sourceNormal, Action onExplosionBegin = null, bool ignoreQueues = false, CoreDamageTypes damageTypes = CoreDamageTypes.None, bool ignoreDamageCaps = false)
	{
		if (data.useDefaultExplosion && data != GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData)
		{
			DoDefaultExplosion(position, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps);
			return;
		}
		GameObject gameObject = new GameObject("temp_explosion_processor", typeof(Exploder));
		gameObject.GetComponent<Exploder>().DoExplode(position, data, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps);
	}

	public static void DoDefaultExplosion(Vector3 position, Vector2 sourceNormal, Action onExplosionBegin = null, bool ignoreQueues = false, CoreDamageTypes damageTypes = CoreDamageTypes.None, bool ignoreDamageCaps = false)
	{
		Explode(position, GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes);
	}

	protected void DoExplode(Vector3 position, ExplosionData data, Vector2 sourceNormal, Action onExplosionBegin = null, bool ignoreQueues = false, CoreDamageTypes damageTypes = CoreDamageTypes.None, bool ignoreDamageCaps = false)
	{
		StartCoroutine(HandleExplosion(position, data, sourceNormal, onExplosionBegin, ignoreQueues, damageTypes, ignoreDamageCaps));
	}

	public static void DoRadialMajorBreakableDamage(float damage, Vector3 position, float radius)
	{
		List<MajorBreakable> allMajorBreakables = StaticReferenceManager.AllMajorBreakables;
		float num = radius * radius;
		if (allMajorBreakables == null)
		{
			return;
		}
		for (int i = 0; i < allMajorBreakables.Count; i++)
		{
			MajorBreakable majorBreakable = allMajorBreakables[i];
			if ((bool)majorBreakable && majorBreakable.enabled && !majorBreakable.IgnoreExplosions)
			{
				Vector2 sourceDirection = majorBreakable.CenterPoint - position.XY();
				if (sourceDirection.sqrMagnitude < num)
				{
					majorBreakable.ApplyDamage(damage, sourceDirection, false, true);
				}
			}
		}
	}

	public static void DoRadialIgnite(GameActorFireEffect fire, Vector3 position, float radius, VFXPool hitVFX = null)
	{
		List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
		if (allHealthHavers == null)
		{
			return;
		}
		float num = radius * radius;
		for (int i = 0; i < allHealthHavers.Count; i++)
		{
			HealthHaver healthHaver = allHealthHavers[i];
			if (!healthHaver || !healthHaver.gameObject.activeSelf || !healthHaver.aiActor)
			{
				continue;
			}
			AIActor aiActor = healthHaver.aiActor;
			if (aiActor.IsGone || !aiActor.isActiveAndEnabled || (aiActor.CenterPosition - position.XY()).sqrMagnitude > num)
			{
				continue;
			}
			aiActor.ApplyEffect(fire);
			if (hitVFX != null)
			{
				if (aiActor.specRigidbody.HitboxPixelCollider != null)
				{
					PixelCollider pixelCollider = aiActor.specRigidbody.GetPixelCollider(ColliderType.HitBox);
					Vector2 vector = BraveMathCollege.ClosestPointOnRectangle(position, pixelCollider.UnitBottomLeft, pixelCollider.UnitDimensions);
					hitVFX.SpawnAtPosition(vector);
				}
				else
				{
					hitVFX.SpawnAtPosition(aiActor.CenterPosition);
				}
			}
		}
	}

	public static void DoRadialDamage(float damage, Vector3 position, float radius, bool damagePlayers, bool damageEnemies, bool ignoreDamageCaps = false, VFXPool hitVFX = null)
	{
		List<HealthHaver> allHealthHavers = StaticReferenceManager.AllHealthHavers;
		if (allHealthHavers == null)
		{
			return;
		}
		for (int i = 0; i < allHealthHavers.Count; i++)
		{
			HealthHaver healthHaver = allHealthHavers[i];
			if (!healthHaver || !healthHaver.gameObject.activeSelf || ((bool)healthHaver.aiActor && healthHaver.aiActor.IsGone) || ((bool)healthHaver.aiActor && !healthHaver.aiActor.isActiveAndEnabled))
			{
				continue;
			}
			for (int j = 0; j < healthHaver.NumBodyRigidbodies; j++)
			{
				SpeculativeRigidbody bodyRigidbody = healthHaver.GetBodyRigidbody(j);
				Vector2 vector = healthHaver.transform.position.XY();
				Vector2 vector2 = vector - position.XY();
				float num = 0f;
				bool flag = false;
				bool flag2 = false;
				if (bodyRigidbody.HitboxPixelCollider != null)
				{
					vector = bodyRigidbody.HitboxPixelCollider.UnitCenter;
					vector2 = vector - position.XY();
					num = BraveMathCollege.DistToRectangle(position.XY(), bodyRigidbody.HitboxPixelCollider.UnitBottomLeft, bodyRigidbody.HitboxPixelCollider.UnitDimensions);
				}
				else
				{
					vector = healthHaver.transform.position.XY();
					vector2 = vector - position.XY();
					num = vector2.magnitude;
				}
				if (num < radius)
				{
					PlayerController component = healthHaver.GetComponent<PlayerController>();
					if (component != null)
					{
						bool flag3 = true;
						if (PassiveItem.ActiveFlagItems.ContainsKey(component) && PassiveItem.ActiveFlagItems[component].ContainsKey(typeof(HelmetItem)) && num > radius * HelmetItem.EXPLOSION_RADIUS_MULTIPLIER)
						{
							flag3 = false;
						}
						if (IsPlayerBlockedByWall(component, position))
						{
							flag3 = false;
						}
						if (damagePlayers && flag3 && !component.IsEthereal)
						{
							float damage2 = 0.5f;
							Vector2 direction = vector2;
							string enemiesString = StringTableManager.GetEnemiesString("#EXPLOSION");
							CoreDamageTypes damageTypes = CoreDamageTypes.None;
							DamageCategory damageCategory = DamageCategory.Normal;
							bool ignoreDamageCaps2 = ignoreDamageCaps;
							healthHaver.ApplyDamage(damage2, direction, enemiesString, damageTypes, damageCategory, false, null, ignoreDamageCaps2);
							flag2 = true;
						}
					}
					else if (damageEnemies)
					{
						AIActor aiActor = healthHaver.aiActor;
						if (damagePlayers || !aiActor || aiActor.IsNormalEnemy)
						{
							float damage2 = damage;
							Vector2 direction = vector2;
							string enemiesString = StringTableManager.GetEnemiesString("#EXPLOSION");
							CoreDamageTypes damageTypes = CoreDamageTypes.None;
							DamageCategory damageCategory = DamageCategory.Normal;
							bool ignoreDamageCaps2 = ignoreDamageCaps;
							healthHaver.ApplyDamage(damage2, direction, enemiesString, damageTypes, damageCategory, false, null, ignoreDamageCaps2);
							flag2 = true;
						}
					}
					flag = true;
				}
				if (flag2 && hitVFX != null)
				{
					if (bodyRigidbody.HitboxPixelCollider != null)
					{
						PixelCollider pixelCollider = bodyRigidbody.GetPixelCollider(ColliderType.HitBox);
						Vector2 vector3 = BraveMathCollege.ClosestPointOnRectangle(position, pixelCollider.UnitBottomLeft, pixelCollider.UnitDimensions);
						hitVFX.SpawnAtPosition(vector3);
					}
					else
					{
						hitVFX.SpawnAtPosition(healthHaver.transform.position.XY());
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
	}

	private static bool IsPlayerBlockedByWall(PlayerController attachedPlayer, Vector2 explosionPos)
	{
		Vector2 centerPosition = attachedPlayer.CenterPosition;
		RaycastResult result;
		bool flag = PhysicsEngine.Instance.Raycast(explosionPos, centerPosition - explosionPos, Vector2.Distance(centerPosition, explosionPos), out result, true, false);
		RaycastResult.Pool.Free(ref result);
		if (!flag)
		{
			return false;
		}
		centerPosition = attachedPlayer.specRigidbody.HitboxPixelCollider.UnitTopCenter;
		flag = PhysicsEngine.Instance.Raycast(explosionPos, centerPosition - explosionPos, Vector2.Distance(centerPosition, explosionPos), out result, true, false);
		RaycastResult.Pool.Free(ref result);
		if (!flag)
		{
			return false;
		}
		centerPosition = attachedPlayer.specRigidbody.PrimaryPixelCollider.UnitBottomCenter;
		flag = PhysicsEngine.Instance.Raycast(explosionPos, centerPosition - explosionPos, Vector2.Distance(centerPosition, explosionPos), out result, true, false);
		RaycastResult.Pool.Free(ref result);
		if (!flag)
		{
			return false;
		}
		return true;
	}

	public static void DoRadialMinorBreakableBreak(Vector3 position, float radius)
	{
		float num = radius * radius;
		List<MinorBreakable> allMinorBreakables = StaticReferenceManager.AllMinorBreakables;
		if (allMinorBreakables == null)
		{
			return;
		}
		for (int i = 0; i < allMinorBreakables.Count; i++)
		{
			MinorBreakable minorBreakable = allMinorBreakables[i];
			if ((bool)minorBreakable && !minorBreakable.resistsExplosions && !minorBreakable.OnlyBrokenByCode)
			{
				Vector2 vector = minorBreakable.CenterPoint - position.XY();
				if (vector.sqrMagnitude < num)
				{
					minorBreakable.Break(vector.normalized);
				}
			}
		}
	}

	public static void DoRadialPush(Vector3 position, float force, float radius)
	{
		float num = radius * radius;
		for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
		{
			Vector2 vector = StaticReferenceManager.AllDebris[i].transform.position.XY();
			Vector2 vector2 = vector - position.XY();
			float sqrMagnitude = vector2.sqrMagnitude;
			if (sqrMagnitude < num)
			{
				float num2 = 1f - vector2.magnitude / radius;
				StaticReferenceManager.AllDebris[i].ApplyVelocity(vector2.normalized * num2 * force * (1f + UnityEngine.Random.value / 5f));
			}
		}
	}

	public static void DoRadialKnockback(Vector3 position, float force, float radius)
	{
		List<AIActor> allEnemies = StaticReferenceManager.AllEnemies;
		if (allEnemies == null)
		{
			return;
		}
		for (int i = 0; i < allEnemies.Count; i++)
		{
			Vector2 centerPosition = allEnemies[i].CenterPosition;
			Vector2 vector = centerPosition - position.XY();
			float magnitude = vector.magnitude;
			if (magnitude < radius)
			{
				KnockbackDoer knockbackDoer = allEnemies[i].knockbackDoer;
				if ((bool)knockbackDoer)
				{
					float num = 1f - magnitude / radius;
					knockbackDoer.ApplyKnockback(vector.normalized, num * force);
				}
			}
		}
	}

	public static void DoDistortionWave(Vector2 center, float distortionIntensity, float distortionRadius, float maxRadius, float duration)
	{
		Exploder component = new GameObject("temp_explosion_processor", typeof(Exploder)).GetComponent<Exploder>();
		component.StartCoroutine(component.DoDistortionWaveLocal(center, distortionIntensity, distortionRadius, maxRadius, duration));
	}

	private Vector4 GetCenterPointInScreenUV(Vector2 centerPoint, float dIntensity, float dRadius)
	{
		Vector3 vector = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(centerPoint.ToVector3ZUp());
		return new Vector4(vector.x, vector.y, dRadius, dIntensity);
	}

	private IEnumerator DoDistortionWaveLocal(Vector2 center, float distortionIntensity, float distortionRadius, float maxRadius, float duration)
	{
		Material distMaterial = new Material(ShaderCache.Acquire("Brave/Internal/DistortionWave"));
		Vector4 distortionSettings2 = GetCenterPointInScreenUV(center, distortionIntensity, distortionRadius);
		distMaterial.SetVector("_WaveCenter", distortionSettings2);
		Pixelator.Instance.RegisterAdditionalRenderPass(distMaterial);
		float elapsed = 0f;
		while (elapsed < duration && (!BraveUtility.isLoadingLevel || !GameManager.Instance.IsLoadingLevel))
		{
			elapsed += BraveTime.DeltaTime;
			float t2 = elapsed / duration;
			t2 = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, t2);
			distortionSettings2 = GetCenterPointInScreenUV(center, distortionIntensity, distortionRadius);
			distortionSettings2.w = Mathf.Lerp(distortionSettings2.w, 0f, t2);
			distMaterial.SetVector("_WaveCenter", distortionSettings2);
			float currentRadius = Mathf.Lerp(0f, maxRadius, t2);
			distMaterial.SetFloat("_DistortProgress", currentRadius / maxRadius * (maxRadius / 33.75f));
			yield return null;
		}
		Pixelator.Instance.DeregisterAdditionalRenderPass(distMaterial);
		UnityEngine.Object.Destroy(distMaterial);
	}

	public static void DoLinearPush(Vector2 p1, Vector2 p2, float force, float radius)
	{
		float num = radius * radius;
		for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
		{
			Vector2 vector = StaticReferenceManager.AllDebris[i].transform.position.XY();
			float num2 = vector.x - p1.x;
			float num3 = vector.y - p1.y;
			float num4 = p2.x - p1.x;
			float num5 = p2.y - p1.y;
			float num6 = num2 * num4 + num3 * num5;
			float num7 = num4 * num4 + num5 * num5;
			float num8 = -1f;
			if (num7 != 0f)
			{
				num8 = num6 / num7;
			}
			float num9;
			float num10;
			if (num8 < 0f)
			{
				num9 = p1.x;
				num10 = p1.y;
			}
			else if (num8 > 1f)
			{
				num9 = p2.x;
				num10 = p2.y;
			}
			else
			{
				num9 = p1.x + num8 * num4;
				num10 = p1.y + num8 * num5;
			}
			float x = vector.x - num9;
			float y = vector.y - num10;
			Vector2 vector2 = new Vector2(x, y);
			float sqrMagnitude = vector2.sqrMagnitude;
			if (sqrMagnitude < num)
			{
				float num11 = 1f - vector2.magnitude / radius;
				StaticReferenceManager.AllDebris[i].ApplyVelocity(vector2.normalized * num11 * force * (1f + UnityEngine.Random.value / 5f));
			}
		}
	}

	private IEnumerator HandleCurrentExplosionNotification(float t)
	{
		float elapsed = 0f;
		while (elapsed < t)
		{
			elapsed += BraveTime.DeltaTime;
			ExplosionIsExtant = true;
			yield return null;
		}
		ExplosionIsExtant = false;
	}

	private IEnumerator HandleBulletDeletionFrames(Vector3 centerPosition, float bulletDeletionSqrRadius, float duration)
	{
		float elapsed = 0f;
		if (GameManager.HasInstance && (bool)GameManager.Instance.Dungeon)
		{
			Dungeon dungeon = GameManager.Instance.Dungeon;
			bulletDeletionSqrRadius *= Mathf.InverseLerp(0.66f, 1f, dungeon.ExplosionBulletDeletionMultiplier);
			if (!dungeon.IsExplosionBulletDeletionRecovering)
			{
				dungeon.ExplosionBulletDeletionMultiplier = Mathf.Clamp01(dungeon.ExplosionBulletDeletionMultiplier - 0.8f);
			}
			if (bulletDeletionSqrRadius <= 0f)
			{
				yield break;
			}
		}
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
			for (int num = allProjectiles.Count - 1; num >= 0; num--)
			{
				Projectile projectile = allProjectiles[num];
				if ((bool)projectile && !(projectile.Owner is PlayerController))
				{
					Vector2 vector = (projectile.transform.position - centerPosition).XY();
					if (projectile.CanBeKilledByExplosions && vector.sqrMagnitude < bulletDeletionSqrRadius)
					{
						projectile.DieInAir();
					}
				}
			}
			List<BasicTrapController> allTraps = StaticReferenceManager.AllTriggeredTraps;
			for (int num2 = allTraps.Count - 1; num2 >= 0; num2--)
			{
				BasicTrapController basicTrapController = allTraps[num2];
				if ((bool)basicTrapController && basicTrapController.triggerOnBlank)
				{
					float sqrMagnitude = (basicTrapController.CenterPoint() - centerPosition.XY()).sqrMagnitude;
					if (sqrMagnitude < bulletDeletionSqrRadius)
					{
						basicTrapController.Trigger();
					}
				}
			}
			yield return null;
		}
	}

	private IEnumerator HandleCirc(tk2dSprite AdditiveCircSprite, float targetScale, float duration)
	{
		AdditiveCircSprite.transform.parent = null;
		AdditiveCircSprite.color = Color.white;
		AdditiveCircSprite.transform.localScale = targetScale * Vector3.one * 0.5f;
		yield return null;
		AdditiveCircSprite.transform.localScale = targetScale * Vector3.one;
		yield return null;
		float ela = 0f;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			AdditiveCircSprite.color = Color.Lerp(t: ela / duration, a: new Color(1f, 1f, 1f, 1f), b: new Color(1f, 1f, 1f, 0f));
			yield return null;
		}
		UnityEngine.Object.Destroy(AdditiveCircSprite.gameObject);
	}

	private IEnumerator HandleExplosion(Vector3 position, ExplosionData data, Vector2 sourceNormal, Action onExplosionBegin, bool ignoreQueues, CoreDamageTypes damageTypes, bool ignoreDamageCaps)
	{
		if (data.usesComprehensiveDelay)
		{
			yield return new WaitForSeconds(data.comprehensiveDelay);
		}
		if (OnExplosionTriggered != null)
		{
			OnExplosionTriggered();
		}
		bool addFireGoop = (damageTypes | CoreDamageTypes.Fire) == damageTypes;
		bool addFreezeGoop = (damageTypes | CoreDamageTypes.Ice) == damageTypes;
		bool addPoisonGoop = (damageTypes | CoreDamageTypes.Poison) == damageTypes;
		if (!GameManager.HasInstance || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			addFireGoop = false;
			addFreezeGoop = false;
			addPoisonGoop = false;
		}
		bool isFreezeExplosion = data.isFreezeExplosion;
		if (!data.isFreezeExplosion && addFreezeGoop)
		{
			isFreezeExplosion = true;
			data.freezeRadius = data.damageRadius;
			data.freezeEffect = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultFreezeExplosionEffect;
		}
		if (!ignoreQueues)
		{
			ExplosionManager.Instance.Queue(this);
			while (!ExplosionManager.Instance.IsExploderReady(this))
			{
				yield return null;
			}
			ExplosionManager.Instance.Dequeue();
			if (ExplosionManager.Instance.QueueCount == 0)
			{
				ExplosionManager.Instance.StartCoroutine(HandleCurrentExplosionNotification(0.5f));
			}
		}
		if (onExplosionBegin != null)
		{
			onExplosionBegin();
		}
		float damageRadius = data.GetDefinedDamageRadius();
		float pushSqrRadius = data.pushRadius * data.pushRadius;
		float bulletDeletionSqrRadius = damageRadius * damageRadius;
		if (addFreezeGoop)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultFreezeGoop).TimedAddGoopCircle(position.XY(), damageRadius);
			DeadlyDeadlyGoopManager.FreezeGoopsCircle(position.XY(), damageRadius);
		}
		if (addFireGoop)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultFireGoop).TimedAddGoopCircle(position.XY(), damageRadius);
		}
		if (addPoisonGoop)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPoisonGoop).TimedAddGoopCircle(position.XY(), damageRadius);
		}
		if (!isFreezeExplosion)
		{
			DeadlyDeadlyGoopManager.IgniteGoopsCircle(position.XY(), damageRadius);
		}
		if ((bool)data.effect)
		{
			GameObject gameObject = ((!(data.effect.GetComponent<ParticleSystem>() != null) && !(data.effect.GetComponentInChildren<ParticleSystem>() != null)) ? SpawnManager.SpawnVFX(data.effect, position, Quaternion.identity) : SpawnManager.SpawnVFX(data.effect, position, Quaternion.identity));
			if (data.rotateEffectToNormal && (bool)gameObject)
			{
				gameObject.transform.rotation = Quaternion.Euler(0f, 0f, sourceNormal.ToAngle());
			}
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			if ((bool)component)
			{
				component.HeightOffGround += UnityEngine.Random.Range(-0.1f, 0.2f);
				component.UpdateZDepth();
			}
			ExplosionDebrisLauncher[] componentsInChildren = gameObject.GetComponentsInChildren<ExplosionDebrisLauncher>();
			Vector3 position2 = gameObject.transform.position.WithZ(gameObject.transform.position.y);
			GameObject gameObject2 = new GameObject("SoundSource");
			gameObject2.transform.position = position2;
			if (data.playDefaultSFX)
			{
				AkSoundEngine.PostEvent("Play_WPN_grenade_blast_01", gameObject2);
			}
			UnityEngine.Object.Destroy(gameObject2, 5f);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if ((bool)componentsInChildren[i])
				{
					if (sourceNormal == Vector2.zero)
					{
						componentsInChildren[i].Launch();
					}
					else
					{
						componentsInChildren[i].Launch(sourceNormal);
					}
				}
			}
			if ((bool)gameObject)
			{
				Transform transform = gameObject.transform.Find("scorch");
				if ((bool)transform)
				{
					transform.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
				}
			}
			if (!data.doExplosionRing)
			{
			}
		}
		yield return new WaitForSeconds(data.explosionDelay);
		List<HealthHaver> allHealth = StaticReferenceManager.AllHealthHavers;
		if (allHealth != null && (data.doDamage || data.doForce))
		{
			for (int j = 0; j < allHealth.Count; j++)
			{
				HealthHaver healthHaver = allHealth[j];
				if (!healthHaver || ((bool)healthHaver && (bool)healthHaver.aiActor && (!healthHaver.aiActor.HasBeenEngaged || ((bool)healthHaver.aiActor.CompanionOwner && data.damageToPlayer == 0f))) || data.ignoreList.Contains(healthHaver.specRigidbody) || position.GetAbsoluteRoom() != allHealth[j].transform.position.GetAbsoluteRoom())
				{
					continue;
				}
				for (int k = 0; k < healthHaver.NumBodyRigidbodies; k++)
				{
					SpeculativeRigidbody bodyRigidbody = healthHaver.GetBodyRigidbody(k);
					PlayerController playerController = ((!bodyRigidbody) ? null : (bodyRigidbody.gameActor as PlayerController));
					Vector2 vector = healthHaver.transform.position.XY();
					Vector2 vector4 = vector - position.XY();
					bool flag = false;
					Vector2 vector2;
					float num;
					if (bodyRigidbody.HitboxPixelCollider != null)
					{
						vector = bodyRigidbody.HitboxPixelCollider.UnitCenter;
						vector2 = vector - position.XY();
						num = BraveMathCollege.DistToRectangle(position.XY(), bodyRigidbody.HitboxPixelCollider.UnitBottomLeft, bodyRigidbody.HitboxPixelCollider.UnitDimensions);
					}
					else
					{
						vector = healthHaver.transform.position.XY();
						vector2 = vector - position.XY();
						num = vector2.magnitude;
					}
					if (((bool)playerController && ((data.doDamage && num < damageRadius) || (isFreezeExplosion && num < data.freezeRadius) || (data.doForce && num < data.pushRadius)) && IsPlayerBlockedByWall(playerController, position)) || ((bool)playerController && (!bodyRigidbody.CollideWithOthers || (playerController.DodgeRollIsBlink && playerController.IsDodgeRolling))))
					{
						continue;
					}
					if (data.doDamage && num < damageRadius)
					{
						if ((bool)playerController)
						{
							bool flag2 = true;
							if (PassiveItem.ActiveFlagItems.ContainsKey(playerController) && PassiveItem.ActiveFlagItems[playerController].ContainsKey(typeof(HelmetItem)) && num > damageRadius * HelmetItem.EXPLOSION_RADIUS_MULTIPLIER)
							{
								flag2 = false;
							}
							if (flag2 && !playerController.IsEthereal)
							{
								float damageToPlayer = data.damageToPlayer;
								Vector2 direction = vector2;
								string enemiesString = StringTableManager.GetEnemiesString("#EXPLOSION");
								CoreDamageTypes damageTypes2 = CoreDamageTypes.None;
								DamageCategory damageCategory = DamageCategory.Normal;
								healthHaver.ApplyDamage(damageToPlayer, direction, enemiesString, damageTypes2, damageCategory, false, null, ignoreDamageCaps);
							}
						}
						else
						{
							float damageToPlayer = data.damage;
							Vector2 direction = vector2;
							string enemiesString = StringTableManager.GetEnemiesString("#EXPLOSION");
							CoreDamageTypes damageTypes2 = CoreDamageTypes.None;
							DamageCategory damageCategory = DamageCategory.Normal;
							healthHaver.ApplyDamage(damageToPlayer, direction, enemiesString, damageTypes2, damageCategory, false, null, ignoreDamageCaps);
							if (data.IsChandelierExplosion && (!healthHaver || healthHaver.healthHaver.IsDead))
							{
								GameStatsManager.Instance.RegisterStatChange(TrackedStats.ENEMIES_KILLED_WITH_CHANDELIERS, 1f);
							}
						}
						flag = true;
					}
					if (isFreezeExplosion && num < data.freezeRadius)
					{
						if ((bool)healthHaver && healthHaver.gameActor != null && !healthHaver.IsDead && (!healthHaver.aiActor || !healthHaver.aiActor.IsGone))
						{
							healthHaver.gameActor.ApplyEffect(data.freezeEffect);
						}
						flag = true;
					}
					if (data.doForce && num < data.pushRadius)
					{
						KnockbackDoer knockbackDoer = healthHaver.knockbackDoer;
						if ((bool)knockbackDoer)
						{
							float num2 = 1f - num / data.pushRadius;
							if (data.preventPlayerForce && (bool)healthHaver.GetComponent<PlayerController>())
							{
								num2 = 0f;
							}
							knockbackDoer.ApplyKnockback(vector2.normalized, num2 * data.force);
						}
						flag = true;
					}
					if (flag)
					{
						break;
					}
				}
			}
		}
		List<MinorBreakable> allBreakables = StaticReferenceManager.AllMinorBreakables;
		if (allBreakables != null)
		{
			for (int l = 0; l < allBreakables.Count; l++)
			{
				MinorBreakable minorBreakable = allBreakables[l];
				if ((bool)minorBreakable && !minorBreakable.resistsExplosions && !minorBreakable.OnlyBrokenByCode)
				{
					Vector2 vector3 = minorBreakable.CenterPoint - position.XY();
					if (vector3.sqrMagnitude < pushSqrRadius)
					{
						minorBreakable.Break(vector3.normalized);
					}
				}
			}
		}
		if (data.doDestroyProjectiles)
		{
			float duration = 0.2f;
			PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
			if ((bool)bestActivePlayer && bestActivePlayer.CurrentRoom != null && bestActivePlayer.CurrentRoom.area != null && bestActivePlayer.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				duration = 0.035f;
			}
			GameManager.Instance.Dungeon.StartCoroutine(HandleBulletDeletionFrames(position, bulletDeletionSqrRadius, duration));
		}
		if (data.doDamage || data.breakSecretWalls)
		{
			List<MajorBreakable> allMajorBreakables = StaticReferenceManager.AllMajorBreakables;
			if (allMajorBreakables != null)
			{
				for (int m = 0; m < allMajorBreakables.Count; m++)
				{
					MajorBreakable majorBreakable = allMajorBreakables[m];
					if (!majorBreakable || !majorBreakable.enabled || majorBreakable.IgnoreExplosions)
					{
						continue;
					}
					Vector2 sourceDirection = majorBreakable.CenterPoint - position.XY();
					if (sourceDirection.sqrMagnitude < pushSqrRadius && (!majorBreakable.IsSecretDoor || !data.forcePreventSecretWallDamage))
					{
						if (data.doDamage)
						{
							majorBreakable.ApplyDamage(data.damage, sourceDirection, false, true);
						}
						if (data.breakSecretWalls && majorBreakable.IsSecretDoor)
						{
							StaticReferenceManager.AllMajorBreakables[m].ApplyDamage(1E+10f, Vector2.zero, false, true, true);
							StaticReferenceManager.AllMajorBreakables[m].ApplyDamage(1E+10f, Vector2.zero, false, true, true);
							StaticReferenceManager.AllMajorBreakables[m].ApplyDamage(1E+10f, Vector2.zero, false, true, true);
						}
					}
				}
			}
		}
		if (data.doForce)
		{
			DoRadialPush(position, data.debrisForce, data.pushRadius);
		}
		if (data.doScreenShake && GameManager.Instance.MainCameraController != null)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(data.ss, position);
		}
		if (data.doStickyFriction && GameManager.Instance.MainCameraController != null)
		{
			StickyFrictionManager.Instance.RegisterExplosionStickyFriction();
		}
		for (int n = 0; n < StaticReferenceManager.AllRatTrapdoors.Count; n++)
		{
			if ((bool)StaticReferenceManager.AllRatTrapdoors[n])
			{
				StaticReferenceManager.AllRatTrapdoors[n].OnNearbyExplosion(position);
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
