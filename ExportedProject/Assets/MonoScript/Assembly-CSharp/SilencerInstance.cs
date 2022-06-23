using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;
using UnityEngine;

public class SilencerInstance : MonoBehaviour
{
	public static float? s_MaxRadiusLimiter;

	private Camera m_camera;

	private Material m_distortionMaterial;

	private float dIntensity;

	private float dRadius;

	public bool ForceNoDamage;

	public bool UsesCustomProjectileCallback;

	public Action<Projectile> OnCustomBlankedProjectile;

	public void TriggerSilencer(Vector2 centerPoint, float expandSpeed, float maxRadius, GameObject silencerVFX, float distIntensity, float distRadius, float pushForce, float pushRadius, float knockbackForce, float knockbackRadius, float additionalTimeAtMaxRadius, PlayerController user, bool breaksWalls = true, bool skipBreakables = false)
	{
		bool flag = true;
		float num = 10f;
		float num2 = 7f;
		float num3 = 1f;
		if (maxRadius < 5f)
		{
			flag = true;
			num = 10f;
			num2 = maxRadius;
		}
		float? num4 = s_MaxRadiusLimiter;
		if (num4.HasValue)
		{
			maxRadius = s_MaxRadiusLimiter.Value;
		}
		bool shouldReflectInstead = false;
		if (user != null)
		{
			for (int i = 0; i < user.passiveItems.Count; i++)
			{
				BlankModificationItem blankModificationItem = user.passiveItems[i] as BlankModificationItem;
				if (blankModificationItem != null)
				{
					if (blankModificationItem.BlankReflectsEnemyBullets)
					{
						shouldReflectInstead = true;
					}
					if (blankModificationItem.MakeBlankDealDamage)
					{
						flag = true;
						num += blankModificationItem.BlankDamage;
						num2 = Mathf.Max(num2, blankModificationItem.BlankDamageRadius);
					}
					num3 *= blankModificationItem.BlankForceMultiplier;
					ProcessBlankModificationItemAdditionalEffects(blankModificationItem, centerPoint, user);
				}
			}
		}
		if ((bool)user && user.HasActiveBonusSynergy(CustomSynergyType.ELDER_BLANK_BULLETS))
		{
			shouldReflectInstead = true;
		}
		dIntensity = distIntensity;
		dRadius = distRadius;
		m_camera = GameManager.Instance.MainCameraController.GetComponent<Camera>();
		if (silencerVFX != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(silencerVFX, centerPoint.ToVector3ZUp(centerPoint.y), Quaternion.identity);
			UnityEngine.Object.Destroy(obj, 1f);
		}
		Exploder.DoRadialPush(centerPoint.ToVector3ZUp(), pushForce, pushRadius);
		Exploder.DoRadialKnockback(centerPoint.ToVector3ZUp(), knockbackForce * num3, knockbackRadius);
		if (!skipBreakables)
		{
			Exploder.DoRadialMinorBreakableBreak(centerPoint.ToVector3ZUp(), knockbackRadius);
		}
		if (breaksWalls)
		{
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(centerPoint.ToIntVector2(VectorConversions.Floor));
			for (int j = 0; j < StaticReferenceManager.AllMajorBreakables.Count; j++)
			{
				if (StaticReferenceManager.AllMajorBreakables[j].IsSecretDoor)
				{
					RoomHandler absoluteRoomFromPosition2 = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(StaticReferenceManager.AllMajorBreakables[j].transform.position.IntXY(VectorConversions.Floor));
					if (absoluteRoomFromPosition2 == absoluteRoomFromPosition)
					{
						StaticReferenceManager.AllMajorBreakables[j].ApplyDamage(1E+10f, Vector2.zero, false, true, true);
						StaticReferenceManager.AllMajorBreakables[j].ApplyDamage(1E+10f, Vector2.zero, false, true, true);
						StaticReferenceManager.AllMajorBreakables[j].ApplyDamage(1E+10f, Vector2.zero, false, true, true);
					}
				}
			}
		}
		if (flag && !ForceNoDamage)
		{
			Exploder.DoRadialDamage(num, centerPoint.ToVector3ZUp(), num2, false, true);
		}
		if (distIntensity > 0f)
		{
			m_distortionMaterial = new Material(ShaderCache.Acquire("Brave/Internal/DistortionWave"));
			Vector4 centerPointInScreenUV = GetCenterPointInScreenUV(centerPoint);
			m_distortionMaterial.SetVector("_WaveCenter", centerPointInScreenUV);
			Pixelator.Instance.RegisterAdditionalRenderPass(m_distortionMaterial);
		}
		if (maxRadius > 10f)
		{
			List<BulletScriptSource> allBulletScriptSources = StaticReferenceManager.AllBulletScriptSources;
			for (int k = 0; k < allBulletScriptSources.Count; k++)
			{
				BulletScriptSource bulletScriptSource = allBulletScriptSources[k];
				if (!bulletScriptSource.IsEnded && bulletScriptSource.RootBullet != null && bulletScriptSource.RootBullet.EndOnBlank)
				{
					bulletScriptSource.ForceStop();
				}
			}
			ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
			for (int num5 = allProjectiles.Count - 1; num5 >= 0; num5--)
			{
				Projectile projectile = allProjectiles[num5];
				if ((bool)projectile.braveBulletScript && projectile.braveBulletScript.bullet != null && projectile.braveBulletScript.bullet.EndOnBlank)
				{
					if (UsesCustomProjectileCallback && OnCustomBlankedProjectile != null)
					{
						OnCustomBlankedProjectile(projectile);
					}
					projectile.DieInAir(false, true, true, true);
				}
			}
		}
		Pixelator.Instance.StartCoroutine(BackupDistortionCleanup());
		StartCoroutine(HandleSilence(centerPoint, expandSpeed, maxRadius, additionalTimeAtMaxRadius, user, shouldReflectInstead));
	}

	private void ProcessBlankModificationItemAdditionalEffects(BlankModificationItem bmi, Vector2 centerPoint, PlayerController user)
	{
		List<AIActor> activeEnemies = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(centerPoint.ToIntVector2()).GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (bmi.RegainAmmoFraction > 0f)
		{
			for (int i = 0; i < user.inventory.AllGuns.Count; i++)
			{
				Gun gun = user.inventory.AllGuns[i];
				if (gun.CanGainAmmo)
				{
					gun.GainAmmo(Mathf.CeilToInt((float)gun.AdjustedMaxAmmo * bmi.RegainAmmoFraction));
				}
			}
		}
		if (activeEnemies == null)
		{
			return;
		}
		for (int j = 0; j < activeEnemies.Count; j++)
		{
			AIActor aIActor = activeEnemies[j];
			float num = Vector2.Distance(centerPoint, aIActor.CenterPosition);
			if (num <= bmi.BlankDamageRadius)
			{
				if (bmi.BlankStunTime > 0f && (bool)aIActor.behaviorSpeculator)
				{
					aIActor.behaviorSpeculator.Stun(bmi.BlankStunTime);
				}
				if (bmi.BlankFireChance > 0f && UnityEngine.Random.value < bmi.BlankFireChance)
				{
					Debug.Log("appling fire...");
					aIActor.ApplyEffect(bmi.BlankFireEffect);
				}
				if (bmi.BlankPoisonChance > 0f && UnityEngine.Random.value < bmi.BlankPoisonChance)
				{
					aIActor.ApplyEffect(bmi.BlankPoisonEffect);
				}
				if (bmi.BlankFreezeChance > 0f && UnityEngine.Random.value < bmi.BlankFreezeChance)
				{
					aIActor.ApplyEffect(bmi.BlankFreezeEffect);
				}
			}
		}
	}

	private Vector4 GetCenterPointInScreenUV(Vector2 centerPoint)
	{
		Vector3 vector = m_camera.WorldToViewportPoint(centerPoint.ToVector3ZUp());
		return new Vector4(vector.x, vector.y, dRadius, dIntensity);
	}

	private IEnumerator HandleSilence(Vector2 centerPoint, float expandSpeed, float maxRadius, float additionalTimeAtMaxRadius, PlayerController user, bool shouldReflectInstead)
	{
		float currentRadius = 0f;
		float previousRadius = 0f;
		while (currentRadius < maxRadius)
		{
			currentRadius += expandSpeed * BraveTime.DeltaTime;
			DestroyBulletsInRange(previousRadius: Mathf.Max(0f, currentRadius - expandSpeed * 0.05f), centerPoint: centerPoint, radius: currentRadius, destroysEnemyBullets: true, destroysPlayerBullets: GameManager.PVP_ENABLED, user: user, reflectsBullets: shouldReflectInstead, useCallback: UsesCustomProjectileCallback, callback: OnCustomBlankedProjectile);
			if (m_distortionMaterial != null)
			{
				Vector4 centerPointInScreenUV = GetCenterPointInScreenUV(centerPoint);
				m_distortionMaterial.SetVector("_WaveCenter", centerPointInScreenUV);
				m_distortionMaterial.SetFloat("_DistortProgress", currentRadius / maxRadius);
			}
			yield return null;
		}
		CleanupDistortion();
		float elapsed = 0f;
		while (elapsed < additionalTimeAtMaxRadius)
		{
			elapsed += BraveTime.DeltaTime;
			bool destroysEnemyBullets = true;
			bool pVP_ENABLED = GameManager.PVP_ENABLED;
			float? previousRadius3 = maxRadius;
			DestroyBulletsInRange(centerPoint, maxRadius, destroysEnemyBullets, pVP_ENABLED, user, false, previousRadius3, UsesCustomProjectileCallback, OnCustomBlankedProjectile);
			yield return null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void CleanupDistortion()
	{
		if (Pixelator.Instance != null && m_distortionMaterial != null)
		{
			Pixelator.Instance.DeregisterAdditionalRenderPass(m_distortionMaterial);
			UnityEngine.Object.Destroy(m_distortionMaterial);
			m_distortionMaterial = null;
		}
	}

	private void OnDestroy()
	{
		CleanupDistortion();
	}

	private IEnumerator BackupDistortionCleanup()
	{
		yield return new WaitForSeconds(3f);
		CleanupDistortion();
	}

	public static void DestroyBulletsInRange(Vector2 centerPoint, float radius, bool destroysEnemyBullets, bool destroysPlayerBullets, PlayerController user = null, bool reflectsBullets = false, float? previousRadius = null, bool useCallback = false, Action<Projectile> callback = null)
	{
		float num = radius * radius;
		float num2 = ((!previousRadius.HasValue) ? 0f : (previousRadius.Value * previousRadius.Value));
		List<Projectile> list = new List<Projectile>();
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int i = 0; i < allProjectiles.Count; i++)
		{
			Projectile projectile = allProjectiles[i];
			if (!projectile || !projectile.sprite)
			{
				continue;
			}
			float sqrMagnitude = (projectile.sprite.WorldCenter - centerPoint).sqrMagnitude;
			if (sqrMagnitude > num || projectile.ImmuneToBlanks || (previousRadius.HasValue && projectile.ImmuneToSustainedBlanks && sqrMagnitude < num2))
			{
				continue;
			}
			if (projectile.Owner != null)
			{
				if (projectile.isFakeBullet || projectile.Owner is AIActor || (projectile.Shooter != null && projectile.Shooter.aiActor != null) || projectile.ForcePlayerBlankable)
				{
					if (destroysEnemyBullets)
					{
						list.Add(projectile);
					}
				}
				else if (projectile.Owner is PlayerController)
				{
					if (destroysPlayerBullets && projectile.Owner != user)
					{
						list.Add(projectile);
					}
				}
				else
				{
					Debug.LogError("Silencer is trying to process a bullet that is owned by something that is neither man nor beast!");
				}
			}
			else if (destroysEnemyBullets)
			{
				list.Add(projectile);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (!destroysPlayerBullets && reflectsBullets)
			{
				PassiveReflectItem.ReflectBullet(list[j], true, user, 10f);
				continue;
			}
			if ((bool)list[j] && (bool)list[j].GetComponent<ChainLightningModifier>())
			{
				ChainLightningModifier component = list[j].GetComponent<ChainLightningModifier>();
				UnityEngine.Object.Destroy(component);
			}
			if (useCallback && callback != null)
			{
				callback(list[j]);
			}
			list[j].DieInAir(false, true, true, true);
		}
		List<BasicTrapController> allTriggeredTraps = StaticReferenceManager.AllTriggeredTraps;
		for (int num3 = allTriggeredTraps.Count - 1; num3 >= 0; num3--)
		{
			BasicTrapController basicTrapController = allTriggeredTraps[num3];
			if ((bool)basicTrapController && basicTrapController.triggerOnBlank)
			{
				float sqrMagnitude2 = (basicTrapController.CenterPoint() - centerPoint).sqrMagnitude;
				if (sqrMagnitude2 < num)
				{
					basicTrapController.Trigger();
				}
			}
		}
	}
}
