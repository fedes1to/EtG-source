using System;
using FullInspector;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EnemyDatabaseEntry : AssetBundleDatabaseEntry
{
	[InspectorDisabled]
	public DungeonPlaceableBehaviour.PlaceableDifficulty difficulty;

	[InspectorDisabled]
	public int placeableWidth;

	[InspectorDisabled]
	public int placeableHeight;

	[InspectorDisabled]
	public bool isNormalEnemy;

	[FormerlySerializedAs("isBoss")]
	[InspectorDisabled]
	public bool isInBossTab;

	[InspectorDisabled]
	public string encounterGuid;

	[InspectorDisabled]
	public int ForcedPositionInAmmonomicon = -1;

	public override AssetBundle assetBundle
	{
		get
		{
			return EnemyDatabase.AssetBundle;
		}
	}

	public EnemyDatabaseEntry()
	{
	}

	public EnemyDatabaseEntry(AIActor enemy)
	{
		myGuid = enemy.EnemyGuid;
		SetAll(enemy);
	}

	public override void DropReference()
	{
		base.DropReference();
	}

	public T GetPrefab<T>() where T : UnityEngine.Object
	{
		if (!loadedPrefab)
		{
			loadedPrefab = assetBundle.LoadAsset<GameObject>(base.name + ".prefab").GetComponent<T>();
		}
		return loadedPrefab as T;
	}

	public void SetAll(AIActor enemy)
	{
		difficulty = enemy.difficulty;
		placeableWidth = enemy.placeableWidth;
		placeableHeight = enemy.placeableHeight;
		isNormalEnemy = enemy.IsNormalEnemy;
		isInBossTab = enemy.InBossAmmonomiconTab;
		encounterGuid = ((!enemy.encounterTrackable) ? string.Empty : enemy.encounterTrackable.TrueEncounterGuid);
		ForcedPositionInAmmonomicon = enemy.ForcedPositionInAmmonomicon;
	}

	public bool Equals(AIActor other)
	{
		if (other == null)
		{
			return false;
		}
		return difficulty == other.difficulty && placeableWidth == other.placeableWidth && placeableHeight == other.placeableHeight && isNormalEnemy == other.IsNormalEnemy && isInBossTab == other.InBossAmmonomiconTab && encounterGuid == ((!other.encounterTrackable) ? string.Empty : other.encounterTrackable.TrueEncounterGuid) && ForcedPositionInAmmonomicon == other.ForcedPositionInAmmonomicon;
	}
}
