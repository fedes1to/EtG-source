using System.Collections.Generic;
using FullSerializer;
using UnityEngine;

[fsObject]
public class GameStats
{
	[fsProperty]
	private Dictionary<TrackedStats, float> stats;

	[fsProperty]
	private Dictionary<TrackedMaximums, float> maxima;

	[fsProperty]
	public HashSet<CharacterSpecificGungeonFlags> m_flags = new HashSet<CharacterSpecificGungeonFlags>();

	public GameStats()
	{
		stats = new Dictionary<TrackedStats, float>(new TrackedStatsComparer());
		maxima = new Dictionary<TrackedMaximums, float>(new TrackedMaximumsComparer());
	}

	public float GetStatValue(TrackedStats statToCheck)
	{
		if (!stats.ContainsKey(statToCheck))
		{
			return 0f;
		}
		return stats[statToCheck];
	}

	public float GetMaximumValue(TrackedMaximums maxToCheck)
	{
		if (!maxima.ContainsKey(maxToCheck))
		{
			return 0f;
		}
		return maxima[maxToCheck];
	}

	public bool GetFlag(CharacterSpecificGungeonFlags flag)
	{
		if (flag == CharacterSpecificGungeonFlags.NONE)
		{
			Debug.LogError("Something is attempting to get a NONE character-specific save flag!");
			return false;
		}
		return m_flags.Contains(flag);
	}

	public void SetStat(TrackedStats stat, float val)
	{
		if (stats.ContainsKey(stat))
		{
			stats[stat] = val;
		}
		else
		{
			stats.Add(stat, val);
		}
	}

	public void SetMax(TrackedMaximums max, float val)
	{
		if (maxima.ContainsKey(max))
		{
			maxima[max] = Mathf.Max(maxima[max], val);
		}
		else
		{
			maxima.Add(max, val);
		}
	}

	public void SetFlag(CharacterSpecificGungeonFlags flag, bool value)
	{
		if (flag == CharacterSpecificGungeonFlags.NONE)
		{
			Debug.LogError("Something is attempting to set a NONE character-specific save flag!");
		}
		else if (value)
		{
			m_flags.Add(flag);
		}
		else
		{
			m_flags.Remove(flag);
		}
	}

	public void IncrementStat(TrackedStats stat, float val)
	{
		if (stats.ContainsKey(stat))
		{
			stats[stat] += val;
		}
		else
		{
			stats.Add(stat, val);
		}
	}

	public void AddStats(GameStats otherStats)
	{
		foreach (KeyValuePair<TrackedStats, float> stat in otherStats.stats)
		{
			IncrementStat(stat.Key, stat.Value);
		}
		foreach (KeyValuePair<TrackedMaximums, float> item in otherStats.maxima)
		{
			SetMax(item.Key, item.Value);
		}
		foreach (CharacterSpecificGungeonFlags flag in otherStats.m_flags)
		{
			m_flags.Add(flag);
		}
	}

	public void ClearAllState()
	{
		List<TrackedStats> list = new List<TrackedStats>();
		foreach (KeyValuePair<TrackedStats, float> stat in stats)
		{
			list.Add(stat.Key);
		}
		foreach (TrackedStats item in list)
		{
			stats[item] = 0f;
		}
		List<TrackedMaximums> list2 = new List<TrackedMaximums>();
		foreach (KeyValuePair<TrackedMaximums, float> item2 in maxima)
		{
			list2.Add(item2.Key);
		}
		foreach (TrackedMaximums item3 in list2)
		{
			maxima[item3] = 0f;
		}
	}
}
