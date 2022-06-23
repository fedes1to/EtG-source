using UnityEngine;

public class FlowDatabase
{
	private static AssetBundle m_assetBundle;

	public static DungeonFlow GetOrLoadByName(string name)
	{
		if (!m_assetBundle)
		{
			m_assetBundle = ResourceManager.LoadAssetBundle("flows_base_001");
		}
		string text = name;
		if (text.Contains("/"))
		{
			text = name.Substring(name.LastIndexOf("/") + 1);
		}
		DebugTime.RecordStartTime();
		DungeonFlow result = m_assetBundle.LoadAsset<DungeonFlow>(text);
		DebugTime.Log("AssetBundle.LoadAsset<DungeonFlow>({0})", text);
		return result;
	}
}
