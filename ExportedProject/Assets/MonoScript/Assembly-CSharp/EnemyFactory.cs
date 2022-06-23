using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class EnemyFactory : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	[BetterList]
	public List<EnemyFactoryWaveDefinition> waves;

	public float delayBetweenWaves = 1f;

	public GameObject rewardChestPrefab;

	protected int m_currentWave;

	protected RoomHandler m_room;

	protected int m_spawnPointIterator;

	protected bool m_finished;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		room.OnEnemiesCleared = (Action)Delegate.Combine(room.OnEnemiesCleared, new Action(OnWaveCleared));
		m_room = room;
	}

	private void Start()
	{
		SpawnWave();
	}

	protected List<EnemyFactorySpawnPoint> AcquireSpawnPoints()
	{
		return m_room.GetComponentsInRoom<EnemyFactorySpawnPoint>();
	}

	private IEnumerator SpawnWaveCR()
	{
		yield return new WaitForSeconds(delayBetweenWaves);
		EnemyFactoryWaveDefinition waveToSpawn = waves[m_currentWave];
		List<EnemyFactorySpawnPoint> spawnPoints = AcquireSpawnPoints();
		if (waveToSpawn.exactDefinition)
		{
			for (int i = 0; i < waveToSpawn.enemyList.Count; i++)
			{
				IntVector2 spawnPosition = spawnPoints[m_spawnPointIterator].transform.position.IntXY(VectorConversions.Floor);
				spawnPoints[m_spawnPointIterator].OnSpawn(waveToSpawn.enemyList[i], spawnPosition, m_room);
				m_spawnPointIterator = (m_spawnPointIterator + 1) % spawnPoints.Count;
			}
			yield break;
		}
		int num = UnityEngine.Random.Range(waveToSpawn.inexactMinCount, waveToSpawn.inexactMaxCount + 1);
		for (int j = 0; j < num; j++)
		{
			IntVector2 spawnPosition2 = spawnPoints[m_spawnPointIterator].transform.position.IntXY(VectorConversions.Floor);
			spawnPoints[m_spawnPointIterator].OnSpawn(waveToSpawn.enemyList[UnityEngine.Random.Range(0, waveToSpawn.enemyList.Count)], spawnPosition2, m_room);
			m_spawnPointIterator = (m_spawnPointIterator + 1) % spawnPoints.Count;
		}
	}

	public void SpawnWave()
	{
		StartCoroutine(SpawnWaveCR());
	}

	protected void ProvideReward()
	{
		if (rewardChestPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(rewardChestPrefab, base.transform.position, Quaternion.identity);
			Chest component = gameObject.GetComponent<Chest>();
			component.ConfigureOnPlacement(m_room);
			m_room.RegisterInteractable(component);
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(component.specRigidbody);
		}
	}

	public void OnWaveCleared()
	{
		if (m_currentWave < waves.Count - 1)
		{
			m_currentWave++;
			SpawnWave();
		}
		else if (!m_finished)
		{
			m_finished = true;
			m_room.HandleRoomAction(RoomEventTriggerAction.UNSEAL_ROOM);
			ProvideReward();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
