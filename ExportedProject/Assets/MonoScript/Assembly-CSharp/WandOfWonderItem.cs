using System.Collections.Generic;
using System.Linq;
using Dungeonator;
using UnityEngine;

public class WandOfWonderItem : PlayerItem
{
	public float ItemChance = 0.25f;

	public float GunChance = 0.25f;

	public float EnemyChance = 0.5f;

	public float VanishChance = 0.25f;

	public GenericLootTable ItemTable;

	public GenericLootTable GunTable;

	public DungeonPlaceable EnemyPlaceable;

	public bool AffectsAllEnemiesInRoom;

	public int MaxItemsPerRoom = 1;

	public int MaxGunsPerRoom = 1;

	public GameObject OnEffectVFX;

	private AIActor GetTargetEnemy(PlayerController user)
	{
		if (user.CurrentRoom == null)
		{
			return null;
		}
		List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null || activeEnemies.Count <= 0)
		{
			return null;
		}
		List<AIActor> list = activeEnemies.Where((AIActor x) => !x.healthHaver.IsBoss).ToList();
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	public override bool CanBeUsed(PlayerController user)
	{
		if (GetTargetEnemy(user) == null)
		{
			return false;
		}
		return true;
	}

	protected void ProcessSingleTarget(PlayerController user, AIActor randomEnemy, ref int spawnedItems, ref int spawnedGuns)
	{
		float num = ItemChance;
		float num2 = GunChance;
		if (spawnedItems == MaxItemsPerRoom)
		{
			num = 0f;
		}
		if (spawnedGuns == MaxGunsPerRoom)
		{
			num2 = 0f;
		}
		float num3 = num + num2 + EnemyChance + VanishChance;
		float num4 = Random.value * num3;
		Vector2 centerPosition = randomEnemy.CenterPosition;
		randomEnemy.EraseFromExistence();
		if (OnEffectVFX != null)
		{
			SpawnManager.SpawnVFX(OnEffectVFX, centerPosition, Quaternion.identity);
		}
		if (num4 < num)
		{
			GameObject item = ItemTable.SelectByWeight();
			LootEngine.SpawnItem(item, centerPosition, Vector2.up, 1f);
			spawnedItems++;
		}
		else if (num4 < num + num2)
		{
			GameObject item2 = GunTable.SelectByWeight();
			LootEngine.SpawnItem(item2, centerPosition, Vector2.up, 1f);
			spawnedGuns++;
		}
		else if (num4 < num + num2 + EnemyChance)
		{
			List<EnemyDatabaseEntry> list = EnemyDatabase.Instance.Entries.Where((EnemyDatabaseEntry x) => x != null && x.isNormalEnemy && !x.isInBossTab).ToList();
			EnemyDatabaseEntry enemyDatabaseEntry = list[Random.Range(0, list.Count)];
			AIActor.Spawn(enemyDatabaseEntry.GetPrefab<AIActor>(), centerPosition.ToIntVector2(VectorConversions.Floor), user.CurrentRoom, true);
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		int spawnedItems = 0;
		int spawnedGuns = 0;
		if (AffectsAllEnemiesInRoom)
		{
			List<AIActor> activeEnemies = user.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies == null || activeEnemies.Count <= 0)
			{
				return;
			}
			List<AIActor> list = activeEnemies.Where((AIActor x) => !x.healthHaver.IsBoss).ToList();
			if (list != null && list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ProcessSingleTarget(user, list[i], ref spawnedItems, ref spawnedGuns);
				}
			}
		}
		else
		{
			AIActor targetEnemy = GetTargetEnemy(user);
			if (!(targetEnemy == null))
			{
				ProcessSingleTarget(user, targetEnemy, ref spawnedItems, ref spawnedGuns);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
