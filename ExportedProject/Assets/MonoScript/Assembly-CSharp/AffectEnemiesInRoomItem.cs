using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public abstract class AffectEnemiesInRoomItem : PlayerItem
{
	public float EffectTime;

	public GameObject OnUserEffectVFX;

	public GameObject OnTargetEffectVFX;

	public float AmbientVFXTime;

	public GameObject AmbientVFX;

	public float minTimeBetweenAmbientVFX = 0.1f;

	public bool FlashScreen;

	public bool AffectsBosses;

	private float m_ambientTimer;

	protected override void DoEffect(PlayerController user)
	{
		List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (OnUserEffectVFX != null)
		{
			SpawnManager.SpawnVFX(OnUserEffectVFX, user.CenterPosition, Quaternion.identity, false);
		}
		if (FlashScreen)
		{
			Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.1f);
		}
		if (activeEnemies == null)
		{
			return;
		}
		if (EffectTime <= 0f)
		{
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				AIActor aIActor = activeEnemies[i];
				if (AffectsBosses || !aIActor.healthHaver.IsBoss)
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
				user.StartCoroutine(HandleAmbientSpawnTime(user.CenterPosition, AmbientVFXTime, 10f));
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

	protected IEnumerator HandleAmbientSpawnTime(Vector2 centerPoint, float remainingTime, float maxEffectRadius)
	{
		float elapsed = 0f;
		while (elapsed < remainingTime)
		{
			elapsed += BraveTime.DeltaTime;
			HandleAmbientVFXSpawn(centerPoint, maxEffectRadius);
			yield return null;
		}
	}

	protected IEnumerator ProcessEffectOverTime(Vector2 centerPoint, List<AIActor> enemiesInRoom)
	{
		float elapsed = 0f;
		List<AIActor> processedEnemies = new List<AIActor>();
		float maxEffectRadius = 10f;
		for (int i = 0; i < enemiesInRoom.Count; i++)
		{
			maxEffectRadius = Mathf.Max(maxEffectRadius, Vector2.Distance(enemiesInRoom[i].CenterPosition, centerPoint));
		}
		while (elapsed < EffectTime)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / EffectTime;
			float CurrentRadius = Mathf.Lerp(0f, maxEffectRadius, t);
			for (int j = 0; j < enemiesInRoom.Count; j++)
			{
				AIActor aIActor = enemiesInRoom[j];
				if (processedEnemies.Contains(aIActor))
				{
					continue;
				}
				if (!AffectsBosses && aIActor.healthHaver.IsBoss)
				{
					processedEnemies.Add(aIActor);
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
			HandleAmbientVFXSpawn(centerPoint, CurrentRadius);
			yield return null;
		}
		if (AmbientVFXTime > EffectTime)
		{
			StartCoroutine(HandleAmbientSpawnTime(centerPoint, AmbientVFXTime - EffectTime, maxEffectRadius));
		}
	}

	protected abstract void AffectEnemy(AIActor target);

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
