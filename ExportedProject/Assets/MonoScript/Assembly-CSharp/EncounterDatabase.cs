using UnityEngine;

public class EncounterDatabase : AssetBundleDatabase<EncounterTrackable, EncounterDatabaseEntry>
{
	public static EncounterDatabase m_instance;

	private static AssetBundle m_assetBundle;

	public static EncounterDatabase Instance
	{
		get
		{
			if (m_instance == null)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				int frameCount = Time.frameCount;
				m_instance = AssetBundle.LoadAsset<EncounterDatabase>("EncounterDatabase");
				DebugTime.Log(realtimeSinceStartup, frameCount, "Loading EncounterDatabase from AssetBundle");
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
				m_assetBundle = ResourceManager.LoadAssetBundle("encounters_base_001");
			}
			return m_assetBundle;
		}
	}

	public static void Unload()
	{
		m_instance = null;
	}

	public static EncounterDatabaseEntry GetEntry(string guid)
	{
		EncounterDatabaseEntry encounterDatabaseEntry = Instance.InternalGetDataByGuid(guid);
		if (encounterDatabaseEntry != null && string.IsNullOrEmpty(encounterDatabaseEntry.ProxyEncounterGuid))
		{
			Instance.InternalGetDataByGuid(encounterDatabaseEntry.ProxyEncounterGuid);
		}
		return encounterDatabaseEntry;
	}

	public static bool IsProxy(string guid)
	{
		EncounterDatabaseEntry entry = GetEntry(guid);
		return entry != null && !string.IsNullOrEmpty(entry.ProxyEncounterGuid);
	}
}
