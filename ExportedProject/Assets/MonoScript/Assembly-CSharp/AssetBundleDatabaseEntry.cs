using System;
using UnityEngine;

[Serializable]
public abstract class AssetBundleDatabaseEntry
{
	public string myGuid;

	public string unityGuid;

	public string path;

	[NonSerialized]
	protected UnityEngine.Object loadedPrefab;

	public abstract AssetBundle assetBundle { get; }

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

	public virtual void DropReference()
	{
		loadedPrefab = null;
	}
}
