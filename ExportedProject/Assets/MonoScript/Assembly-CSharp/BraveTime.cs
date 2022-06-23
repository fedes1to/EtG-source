using System.Collections.Generic;
using UnityEngine;

public static class BraveTime
{
	private static List<GameObject> m_sources = new List<GameObject>();

	private static List<float> m_multipliers = new List<float>();

	private static int s_lastScaledTimeFrameUpdate = -1;

	private static float s_scaledTimeSinceStartup = 0f;

	private static float m_cachedDeltaTime;

	public static float DeltaTime
	{
		get
		{
			return m_cachedDeltaTime;
		}
	}

	public static float ScaledTimeSinceStartup
	{
		get
		{
			UpdateScaledTimeSinceStartup();
			return s_scaledTimeSinceStartup;
		}
	}

	public static void CacheDeltaTimeForFrame()
	{
		m_cachedDeltaTime = Mathf.Min(0.1f, Time.deltaTime);
	}

	public static void UpdateScaledTimeSinceStartup()
	{
		if (s_lastScaledTimeFrameUpdate != Time.frameCount)
		{
			s_scaledTimeSinceStartup += DeltaTime;
			s_lastScaledTimeFrameUpdate = Time.frameCount;
		}
	}

	public static void RegisterTimeScaleMultiplier(float multiplier, GameObject source)
	{
		if (!m_sources.Contains(source))
		{
			m_sources.Add(source);
			m_multipliers.Add(1f);
		}
		int index = m_sources.IndexOf(source);
		m_multipliers[index] *= multiplier;
		UpdateTimeScale();
	}

	public static void SetTimeScaleMultiplier(float multiplier, GameObject source)
	{
		if (!m_sources.Contains(source))
		{
			m_sources.Add(source);
			m_multipliers.Add(1f);
		}
		int index = m_sources.IndexOf(source);
		m_multipliers[index] = multiplier;
		UpdateTimeScale();
	}

	public static void ClearMultiplier(GameObject source)
	{
		int num = m_sources.IndexOf(source);
		if (num >= 0)
		{
			m_sources.RemoveAt(num);
			m_multipliers.RemoveAt(num);
		}
		UpdateTimeScale();
	}

	public static void ClearAllMultipliers()
	{
		m_sources.Clear();
		m_multipliers.Clear();
		UpdateTimeScale();
	}

	private static void UpdateTimeScale()
	{
		float num = 1f;
		for (int i = 0; i < m_multipliers.Count; i++)
		{
			num = m_multipliers[i] * num;
		}
		if (float.IsNaN(num))
		{
			Debug.LogError("TIMESCALE WAS MY NAN ALL ALONG");
			num = 1f;
		}
		num = (Time.timeScale = Mathf.Clamp(num, 0f, (!ChallengeManager.CHALLENGE_MODE_ACTIVE) ? 1f : 1.5f));
	}
}
