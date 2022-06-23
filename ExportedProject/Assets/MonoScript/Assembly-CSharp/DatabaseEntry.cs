using System;
using UnityEngine;

[Serializable]
public class DatabaseEntry
{
	public string myGuid;

	public string unityGuid;

	public string path;

	[NonSerialized]
	private UnityEngine.Object loadedPrefab;

	public string name
	{
		get
		{
			return path.Substring(path.LastIndexOf('/') + 1).Replace(".prefab", string.Empty);
		}
	}

	public bool HasLoadedPrefab
	{
		get
		{
			return loadedPrefab;
		}
	}

	public T GetPrefab<T>() where T : UnityEngine.Object
	{
		if (!loadedPrefab)
		{
			if (!path.StartsWith("Assets/Resources/"))
			{
				Debug.LogErrorFormat("Trying to instantate an object that doesn't live in Resources! {0} {1} {2}", myGuid, unityGuid, path);
				return (T)null;
			}
			loadedPrefab = BraveResources.Load<T>(path.Replace("Assets/Resources/", string.Empty).Replace(".prefab", string.Empty));
		}
		return loadedPrefab as T;
	}

	public void DropReference()
	{
		loadedPrefab = null;
	}
}
