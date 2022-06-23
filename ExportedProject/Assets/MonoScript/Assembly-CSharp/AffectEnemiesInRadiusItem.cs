using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public abstract class AffectEnemiesInRadiusItem : PlayerItem
{
	public float EffectRadius = 10f;

	public float EffectTime;

	public Vector2 OnUserEffectOffset = Vector3.zero;

	public bool OnUserEffectAttached;

	public GameObject OnUserEffectVFX;

	public GameObject OnTargetEffectVFX;

	public string AudioEvent;

	public float AmbientVFXTime;

	public GameObject AmbientVFX;

	public float minTimeBetweenAmbientVFX = 0.1f;

	public bool FlashScreen;

	public bool ShakeScreen;

	[ShowInInspectorIf("ShakeScreen", false)]
	public ScreenShakeSettings ScreenShakeData;

	public bool DoEffectDistortionWave;

	private float m_ambientTimer;

	protected override void DoEffect(PlayerController user)
	{
		List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (OnUserEffectVFX != null)
		{
			if (OnUserEffectAttached)
			{
				user.PlayEffectOnActor(OnUserEffectVFX, OnUserEffectOffset);
			}
			else
			{
				SpawnManager.SpawnVFX(OnUserEffectVFX, user.CenterPosition + OnUserEffectOffset, Quaternion.identity, false);
			}
		}
		if (ShakeScreen)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(ScreenShakeData, null);
		}
		if (FlashScreen)
		{
			Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.1f);
		}
		if (DoEffectDistortionWave)
		{
			Exploder.DoDistortionWave(user.CenterPosition, 0.4f, 0.15f, EffectRadius, 0.4f);
		}
		if (!string.IsNullOrEmpty(AudioEvent))
		{
			AkSoundEngine.PostEvent(AudioEvent, base.gameObject);
		}
		if (EffectTime <= 0f)
		{
			if (activeEnemies != null)
			{
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					AIActor aIActor = activeEnemies[i];
					if (!aIActor.IsNormalEnemy)
					{
						continue;
					}
					float num = Vector2.Distance(user.CenterPosition, aIActor.CenterPosition);
					if (num <= EffectRadius)
					{
						AffectEnemy(aIActor);
						if (OnTargetEffectVFX != null)
						{
							SpawnManager.SpawnVFX(OnTargetEffectVFX, aIActor.CenterPosition, Quaternion.identity, false);
						}
					}
				}
				if (AmbientVFXTime > 0f && AmbientVFX != null)
				{
					user.StartCoroutine(HandleAmbientSpawnTime(user.CenterPosition, AmbientVFXTime));
				}
			}
			List<ProjectileTrapController> allProjectileTraps = StaticReferenceManager.AllProjectileTraps;
			for (int j = 0; j < allProjectileTraps.Count; j++)
			{
				ProjectileTrapController projectileTrapController = allProjectileTraps[j];
				if (!projectileTrapController || !projectileTrapController.isActiveAndEnabled)
				{
					continue;
				}
				float num2 = Vector2.Distance(user.CenterPosition, projectileTrapController.shootPoint.position);
				if (num2 <= EffectRadius)
				{
					AffectProjectileTrap(projectileTrapController);
					if (OnTargetEffectVFX != null)
					{
						SpawnManager.SpawnVFX(OnTargetEffectVFX, projectileTrapController.shootPoint.position, Quaternion.identity, false);
					}
				}
			}
			List<ForgeHammerController> allForgeHammers = StaticReferenceManager.AllForgeHammers;
			for (int k = 0; k < allForgeHammers.Count; k++)
			{
				ForgeHammerController forgeHammerController = allForgeHammers[k];
				if ((bool)forgeHammerController && forgeHammerController.isActiveAndEnabled)
				{
					float num3 = Vector2.Distance(user.CenterPosition, forgeHammerController.sprite.WorldCenter);
					if (num3 <= EffectRadius)
					{
						AffectForgeHammer(forgeHammerController);
					}
				}
			}
			List<BaseShopController> allShops = StaticReferenceManager.AllShops;
			for (int l = 0; l < allShops.Count; l++)
			{
				BaseShopController baseShopController = allShops[l];
				float num4 = Vector2.Distance(user.CenterPosition, baseShopController.CenterPosition);
				if (num4 <= EffectRadius)
				{
					AffectShop(baseShopController);
					if (OnTargetEffectVFX != null)
					{
						SpawnManager.SpawnVFX(OnTargetEffectVFX, baseShopController.CenterPosition, Quaternion.identity, false);
					}
				}
			}
			List<MajorBreakable> allMajorBreakables = StaticReferenceManager.AllMajorBreakables;
			for (int m = 0; m < allMajorBreakables.Count; m++)
			{
				MajorBreakable majorBreakable = allMajorBreakables[m];
				if (!majorBreakable.specRigidbody || majorBreakable.specRigidbody.PrimaryPixelCollider == null)
				{
					continue;
				}
				float num5 = Vector2.Distance(user.CenterPosition, majorBreakable.specRigidbody.UnitCenter);
				if (num5 <= EffectRadius)
				{
					AffectMajorBreakable(majorBreakable);
					if (OnTargetEffectVFX != null)
					{
						SpawnManager.SpawnVFX(OnTargetEffectVFX, majorBreakable.specRigidbody.UnitCenter, Quaternion.identity, false);
					}
				}
			}
		}
		else
		{
			user.StartCoroutine(ProcessEffectOverTime(user.CenterPosition, activeEnemies));
		}
	}

	protected void HandleAmbientVFXSpawn(Vector2 centerPoint, float radius)
	{
		if (!(AmbientVFX == null))
		{
			bool flag = false;
			m_ambientTimer -= BraveTime.DeltaTime;
			if (m_ambientTimer <= 0f)
			{
				flag = true;
				m_ambientTimer = minTimeBetweenAmbientVFX;
			}
			if (flag)
			{
				Vector2 vector = centerPoint + Random.insideUnitCircle * radius;
				SpawnManager.SpawnVFX(AmbientVFX, vector, Quaternion.identity);
			}
		}
	}

	protected IEnumerator HandleAmbientSpawnTime(Vector2 centerPoint, float remainingTime)
	{
		float elapsed = 0f;
		while (elapsed < remainingTime)
		{
			elapsed += BraveTime.DeltaTime;
			HandleAmbientVFXSpawn(centerPoint, EffectRadius);
			yield return null;
		}
	}

	protected IEnumerator ProcessEffectOverTime(Vector2 centerPoint, List<AIActor> enemiesInRoom)
	{
		float elapsed = 0f;
		List<AIActor> processedEnemies = new List<AIActor>();
		List<BaseShopController> processedShops = new List<BaseShopController>();
		List<ForgeHammerController> processedHammers = new List<ForgeHammerController>();
		List<ProjectileTrapController> processedTraps = new List<ProjectileTrapController>();
		while (elapsed < EffectTime)
		{
			elapsed += BraveTime.DeltaTime;
			float CurrentRadius = Mathf.Lerp(t: elapsed / EffectTime, a: 0f, b: EffectRadius);
			for (int i = 0; i < enemiesInRoom.Count; i++)
			{
				AIActor aIActor = enemiesInRoom[i];
				if (processedEnemies.Contains(aIActor))
				{
					continue;
				}
				float num = Vector2.Distance(centerPoint, aIActor.CenterPosition);
				if (num <= CurrentRadius)
				{
					AffectEnemy(aIActor);
					if (OnTargetEffectVFX != null)
					{
						SpawnManager.SpawnVFX(OnTargetEffectVFX, aIActor.CenterPosition, Quaternion.identity, false);
					}
					processedEnemies.Add(aIActor);
				}
			}
			List<ProjectileTrapController> allTraps = StaticReferenceManager.AllProjectileTraps;
			for (int j = 0; j < allTraps.Count; j++)
			{
				ProjectileTrapController projectileTrapController = allTraps[j];
				if (processedTraps.Contains(projectileTrapController) || !projectileTrapController || !projectileTrapController.isActiveAndEnabled)
				{
					continue;
				}
				float num2 = Vector2.Distance(centerPoint, projectileTrapController.shootPoint.position);
				if (num2 <= CurrentRadius)
				{
					AffectProjectileTrap(projectileTrapController);
					if (OnTargetEffectVFX != null)
					{
						SpawnManager.SpawnVFX(OnTargetEffectVFX, projectileTrapController.shootPoint.position, Quaternion.identity, false);
					}
					processedTraps.Add(projectileTrapController);
				}
			}
			List<ForgeHammerController> allHammers = StaticReferenceManager.AllForgeHammers;
			for (int k = 0; k < allHammers.Count; k++)
			{
				ForgeHammerController forgeHammerController = allHammers[k];
				if (!processedHammers.Contains(forgeHammerController) && (bool)forgeHammerController && forgeHammerController.isActiveAndEnabled)
				{
					float num3 = Vector2.Distance(centerPoint, forgeHammerController.sprite.WorldCenter);
					if (num3 <= CurrentRadius)
					{
						AffectForgeHammer(forgeHammerController);
					}
					processedHammers.Add(forgeHammerController);
				}
			}
			List<BaseShopController> allShops = StaticReferenceManager.AllShops;
			for (int l = 0; l < allShops.Count; l++)
			{
				BaseShopController baseShopController = allShops[l];
				if (processedShops.Contains(baseShopController))
				{
					continue;
				}
				float num4 = Vector2.Distance(centerPoint, baseShopController.CenterPosition);
				if (num4 <= CurrentRadius)
				{
					AffectShop(baseShopController);
					if (OnTargetEffectVFX != null)
					{
						SpawnManager.SpawnVFX(OnTargetEffectVFX, baseShopController.CenterPosition, Quaternion.identity, false);
					}
					processedShops.Add(baseShopController);
				}
			}
			HandleAmbientVFXSpawn(centerPoint, CurrentRadius);
			yield return null;
		}
		if (AmbientVFXTime > EffectTime)
		{
			StartCoroutine(HandleAmbientSpawnTime(centerPoint, AmbientVFXTime - EffectTime));
		}
	}

	protected abstract void AffectEnemy(AIActor target);

	protected virtual void AffectProjectileTrap(ProjectileTrapController target)
	{
	}

	protected virtual void AffectShop(BaseShopController target)
	{
	}

	protected virtual void AffectForgeHammer(ForgeHammerController target)
	{
	}

	protected virtual void AffectMajorBreakable(MajorBreakable majorBreakable)
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
