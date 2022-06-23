using UnityEngine;

public class PrefabDatabase : ScriptableObject
{
	public GameObject SuperReaper;

	public GameObject ResourcefulRatThief;

	private static PrefabDatabase m_instance;

	private static AssetBundle m_assetBundle;

	public static PrefabDatabase Instance
	{
		get
		{
			if (m_instance == null)
			{
				DebugTime.RecordStartTime();
				m_instance = AssetBundle.LoadAsset<PrefabDatabase>("PrefabDatabase");
				DebugTime.Log("Loading PrefabDatabase from AssetBundle");
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
				m_assetBundle = ResourceManager.LoadAssetBundle("shared_base_001");
			}
			return m_assetBundle;
		}
	}
}
