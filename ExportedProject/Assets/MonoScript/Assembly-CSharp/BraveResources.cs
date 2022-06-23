using System;
using UnityEngine;

public class BraveResources
{
	private static AssetBundle m_assetBundle;

	public static UnityEngine.Object Load(string path, string extension = ".prefab")
	{
		if (m_assetBundle == null)
		{
			EnsureLoaded();
		}
		return m_assetBundle.LoadAsset<UnityEngine.Object>("assets/ResourcesBundle/" + path + extension);
	}

	public static UnityEngine.Object Load(string path, Type type, string extension = ".prefab")
	{
		if (m_assetBundle == null)
		{
			EnsureLoaded();
		}
		return m_assetBundle.LoadAsset("assets/ResourcesBundle/" + path + extension, type);
	}

	public static T Load<T>(string path, string extension = ".prefab") where T : UnityEngine.Object
	{
		if (m_assetBundle == null)
		{
			EnsureLoaded();
		}
		return m_assetBundle.LoadAsset<T>("assets/ResourcesBundle/" + path + extension);
	}

	public static void EnsureLoaded()
	{
		if (m_assetBundle == null)
		{
			m_assetBundle = ResourceManager.LoadAssetBundle("brave_resources_001");
		}
	}
}
