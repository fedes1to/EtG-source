using Dungeonator;
using UnityEngine;

public class DungeonDatabase
{
	public static Dungeon GetOrLoadByName(string name)
	{
		AssetBundle assetBundle = ResourceManager.LoadAssetBundle("dungeons/" + name.ToLower());
		DebugTime.RecordStartTime();
		Dungeon component = assetBundle.LoadAsset<GameObject>(name).GetComponent<Dungeon>();
		DebugTime.Log("AssetBundle.LoadAsset<Dungeon>({0})", name);
		return component;
	}
}
