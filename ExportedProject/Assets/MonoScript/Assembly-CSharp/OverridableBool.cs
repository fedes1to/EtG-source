using System.Collections.Generic;
using UnityEngine;

public class OverridableBool
{
	private class OverrideData
	{
		public string key;

		public float? duration;
	}

	public bool BaseValue;

	private List<OverrideData> m_overrides = new List<OverrideData>();

	public bool Value
	{
		get
		{
			return (m_overrides.Count <= 0) ? BaseValue : (!BaseValue);
		}
	}

	public OverridableBool(bool defaultValue)
	{
		BaseValue = defaultValue;
	}

	public void Debug()
	{
		for (int i = 0; i < m_overrides.Count; i++)
		{
			float? duration = m_overrides[i].duration;
			string text = (duration.HasValue ? m_overrides[i].duration.Value.ToString() : "null");
			UnityEngine.Debug.LogWarningFormat("override set: {0} (duration: {1})", m_overrides[i], text);
		}
	}

	public bool HasOverride(string key)
	{
		for (int i = 0; i < m_overrides.Count; i++)
		{
			if (m_overrides[i].key == key)
			{
				return true;
			}
		}
		return false;
	}

	public void AddOverride(string key, float? duration = null)
	{
		for (int i = 0; i < m_overrides.Count; i++)
		{
			if (!(m_overrides[i].key == key))
			{
				continue;
			}
			if (duration.HasValue)
			{
				float? duration2 = m_overrides[i].duration;
				if (duration2.HasValue)
				{
					m_overrides[i].duration = Mathf.Max(m_overrides[i].duration.Value, duration.Value);
					return;
				}
			}
			m_overrides[i].duration = null;
			return;
		}
		m_overrides.Add(new OverrideData
		{
			key = key,
			duration = duration
		});
	}

	public void RemoveOverride(string key)
	{
		for (int i = 0; i < m_overrides.Count; i++)
		{
			if (m_overrides[i].key == key)
			{
				m_overrides.RemoveAt(i);
				break;
			}
		}
	}

	public void SetOverride(string key, bool value, float? duration = null)
	{
		if (value != BaseValue)
		{
			AddOverride(key, duration);
			return;
		}
		if (duration.HasValue)
		{
			UnityEngine.Debug.LogWarningFormat("Trying to disable an override with a duration! {0} {1} {2}", key, value, duration.Value);
		}
		RemoveOverride(key);
	}

	public void ClearOverrides()
	{
		m_overrides.Clear();
	}

	public bool UpdateTimers(float deltaTime)
	{
		bool result = false;
		for (int num = m_overrides.Count - 1; num >= 0; num--)
		{
			float? duration = m_overrides[num].duration;
			if (duration.HasValue)
			{
				m_overrides[num].duration -= deltaTime;
				float? duration2 = m_overrides[num].duration;
				if (duration2.HasValue && duration2.GetValueOrDefault() <= 0f)
				{
					m_overrides.RemoveAt(num);
					result = true;
				}
			}
		}
		return result;
	}
}
