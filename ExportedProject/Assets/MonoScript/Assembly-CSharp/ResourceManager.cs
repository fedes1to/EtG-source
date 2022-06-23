using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResourceManager
{
	private static Dictionary<string, AssetBundle> LoadedBundles;

	private static string[] BundlePrereqs = new string[6] { "shared_base_001", "shared_auto_001", "shared_auto_002", "brave_resources_001", "enemies_base_001", "dungeons/base_foyer" };

	public static void Init()
	{
		if (LoadedBundles == null)
		{
			LoadedBundles = new Dictionary<string, AssetBundle>();
			for (int i = 0; i < BundlePrereqs.Length; i++)
			{
				LoadAssetBundle(BundlePrereqs[i]);
			}
		}
	}

	public static IEnumerator InitAsync()
	{
		if (LoadedBundles == null)
		{
			LoadedBundles = new Dictionary<string, AssetBundle>();
			for (int i = 0; i < BundlePrereqs.Length; i++)
			{
				LoadAssetBundle(BundlePrereqs[i]);
				yield return null;
			}
		}
	}

	public static AssetBundle LoadAssetBundle(string path)
	{
		if (LoadedBundles == null)
		{
			Init();
		}
		AssetBundle value;
		if (LoadedBundles.TryGetValue(path, out value))
		{
			return value;
		}
		string path2 = Path.Combine(Application.streamingAssetsPath, Path.Combine("Assets/", path));
		DebugTime.RecordStartTime();
		value = AssetBundle.LoadFromFile(path2);
		DebugTime.Log("AssetBundle.LoadFromFile({0})", path);
		LoadedBundles.Add(path, value);
		return value;
	}

	public static void LoadSceneFromBundle(AssetBundle assetBundle, LoadSceneMode mode)
	{
		SceneManager.LoadScene(GetSceneName(assetBundle), mode);
	}

	public static AsyncOperation LoadSceneAsyncFromBundle(AssetBundle assetBundle, LoadSceneMode mode)
	{
		return SceneManager.LoadSceneAsync(GetSceneName(assetBundle), mode);
	}

	public static void LoadLevelFromBundle(AssetBundle assetBundle)
	{
		Application.LoadLevel(GetSceneName(assetBundle));
	}

	private static string GetSceneName(AssetBundle assetBundle)
	{
		return assetBundle.GetAllScenePaths()[0];
	}
}
