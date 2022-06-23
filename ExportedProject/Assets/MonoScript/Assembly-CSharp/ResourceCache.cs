using System.Collections.Generic;
using UnityEngine;

public static class ResourceCache
{
	private static Dictionary<string, Object> m_resourceCache = new Dictionary<string, Object>();

	public static Object Acquire(string resourceName)
	{
		if (!m_resourceCache.ContainsKey(resourceName))
		{
			m_resourceCache.Add(resourceName, BraveResources.Load(resourceName));
		}
		return m_resourceCache[resourceName];
	}

	public static void ClearCache()
	{
		m_resourceCache.Clear();
	}
}
