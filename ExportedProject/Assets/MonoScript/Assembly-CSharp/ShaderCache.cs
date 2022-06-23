using System.Collections.Generic;
using UnityEngine;

public static class ShaderCache
{
	private static Dictionary<string, Shader> m_shaderCache = new Dictionary<string, Shader>();

	public static Shader Acquire(string shaderName)
	{
		if (m_shaderCache.ContainsKey(shaderName) && !m_shaderCache[shaderName])
		{
			m_shaderCache.Remove(shaderName);
		}
		if (!m_shaderCache.ContainsKey(shaderName))
		{
			m_shaderCache.Add(shaderName, Shader.Find(shaderName));
		}
		return m_shaderCache[shaderName];
	}

	public static void ClearCache()
	{
		m_shaderCache.Clear();
	}
}
