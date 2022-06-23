using System.Collections.Generic;
using UnityEngine;

public static class IntToStringSansGarbage
{
	private static Dictionary<int, string> m_map = new Dictionary<int, string>();

	public static string GetStringForInt(int input)
	{
		if (m_map.ContainsKey(input))
		{
			return m_map[input];
		}
		string text = input.ToString();
		m_map.Add(input, text);
		if (m_map.Count > 25000)
		{
			Debug.LogError("Int To String (sans Garbage) map count greater than 25000!");
			m_map.Clear();
		}
		return text;
	}
}
