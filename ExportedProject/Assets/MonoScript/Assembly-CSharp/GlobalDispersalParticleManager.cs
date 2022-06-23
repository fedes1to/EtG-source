using System.Collections.Generic;
using UnityEngine;

public static class GlobalDispersalParticleManager
{
	public static Dictionary<GameObject, ParticleSystem> PrefabToSystemMap;

	public static ParticleSystem GetSystemForPrefab(GameObject prefab)
	{
		if (PrefabToSystemMap == null)
		{
			PrefabToSystemMap = new Dictionary<GameObject, ParticleSystem>();
		}
		if (PrefabToSystemMap.ContainsKey(prefab))
		{
			return PrefabToSystemMap[prefab];
		}
		ParticleSystem component = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
		PrefabToSystemMap.Add(prefab, component);
		return component;
	}

	public static void Clear()
	{
		if (PrefabToSystemMap != null)
		{
			PrefabToSystemMap.Clear();
		}
	}
}
