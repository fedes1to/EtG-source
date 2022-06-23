using System;
using UnityEngine;

public class EnemyDatabase : AssetBundleDatabase<AIActor, EnemyDatabaseEntry>
{
	private static EnemyDatabase m_instance;

	private static AssetBundle m_assetBundle;

	public static EnemyDatabase Instance
	{
		get
		{
			if (m_instance == null)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				int frameCount = Time.frameCount;
				m_instance = AssetBundle.LoadAsset<EnemyDatabase>("EnemyDatabase");
				DebugTime.Log(realtimeSinceStartup, frameCount, "Loading EnemyDatabase from AssetBundle");
			}
			return m_instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance != null;
		}
	}

	public static AssetBundle AssetBundle
	{
		get
		{
			if (m_assetBundle == null)
			{
				m_assetBundle = ResourceManager.LoadAssetBundle("enemies_base_001");
			}
			return m_assetBundle;
		}
	}

	public override void DropReferences()
	{
		base.DropReferences();
	}

	public AIActor InternalGetByName(string name)
	{
		int i = 0;
		for (int count = Entries.Count; i < count; i++)
		{
			EnemyDatabaseEntry enemyDatabaseEntry = Entries[i];
			if (enemyDatabaseEntry != null && enemyDatabaseEntry.name.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return enemyDatabaseEntry.GetPrefab<AIActor>();
			}
		}
		return null;
	}

	public AIActor InternalGetByGuid(string guid)
	{
		int i = 0;
		for (int count = Entries.Count; i < count; i++)
		{
			EnemyDatabaseEntry enemyDatabaseEntry = Entries[i];
			if (enemyDatabaseEntry != null && enemyDatabaseEntry.myGuid == guid)
			{
				return enemyDatabaseEntry.GetPrefab<AIActor>();
			}
		}
		return null;
	}

	public static void Unload()
	{
		m_instance = null;
	}

	public static AIActor GetOrLoadByName(string name)
	{
		return Instance.InternalGetByName(name);
	}

	public static AIActor GetOrLoadByGuid(string guid)
	{
		return Instance.InternalGetByGuid(guid);
	}

	public static EnemyDatabaseEntry GetEntry(string guid)
	{
		return Instance.InternalGetDataByGuid(guid);
	}
}
