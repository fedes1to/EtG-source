using System.Collections;
using Dungeonator;

public class EveryEnemySpawner : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public string[] ignoreList;

	public bool reinforce;

	private RoomHandler m_room;

	private AIActor m_blobulinPrefab;

	public void Start()
	{
		m_room.Entered += PlayerEntered;
		m_blobulinPrefab = EnemyDatabase.Instance.Entries.Find((EnemyDatabaseEntry e) => e.path.Contains("/Blobulin.prefab")).GetPrefab<AIActor>();
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		m_room = room;
	}

	public void PlayerEntered(PlayerController playerController)
	{
		StartCoroutine(SpawnAllEnemies());
	}

	private IEnumerator SpawnAllEnemies()
	{
		foreach (EnemyDatabaseEntry entry in EnemyDatabase.Instance.Entries)
		{
			if (!entry.isNormalEnemy || entry.isInBossTab)
			{
				continue;
			}
			bool shouldBreak = false;
			for (int i = 0; i < ignoreList.Length; i++)
			{
				if (entry.path.Contains("/" + ignoreList[i] + ".prefab"))
				{
					shouldBreak = true;
					break;
				}
			}
			if (shouldBreak)
			{
				continue;
			}
			IntVector2 pos = base.transform.position.XY().ToIntVector2(VectorConversions.Floor);
			for (int j = -5; j <= 5; j++)
			{
				for (int k = -5; k <= 5; k++)
				{
					DeadlyDeadlyGoopManager.ForceClearGoopsInCell(new IntVector2(pos.x + j, pos.y + k));
				}
			}
			AIActor prefab = entry.GetPrefab<AIActor>();
			IntVector2 position = base.transform.position.XY().ToIntVector2(VectorConversions.Floor);
			RoomHandler room = m_room;
			bool autoEngage = !reinforce;
			AIActor enemy = AIActor.Spawn(prefab, position, room, false, AIActor.AwakenAnimationType.Default, autoEngage);
			if (reinforce)
			{
				enemy.HandleReinforcementFallIntoRoom();
			}
			if (enemy.name.Contains("MetalCubeGuy"))
			{
				AIActor.Spawn(m_blobulinPrefab, base.transform.position.XY().ToIntVector2(VectorConversions.Floor) + new IntVector2(-2, 0), m_room);
			}
			m_room.SealRoom();
			float unsealedTime = 0f;
			float requiredUnsealedTime = ((!enemy.GetComponent<SpawnEnemyOnDeath>()) ? 0.5f : 1.5f);
			while (((bool)enemy && enemy.healthHaver.IsAlive) || unsealedTime < requiredUnsealedTime)
			{
				unsealedTime = ((!m_room.IsSealed) ? (unsealedTime + BraveTime.DeltaTime) : 0f);
				yield return null;
			}
		}
	}
}
