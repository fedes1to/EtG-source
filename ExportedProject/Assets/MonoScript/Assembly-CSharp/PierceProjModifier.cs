using UnityEngine;

public class PierceProjModifier : MonoBehaviour
{
	public enum BeastModeStatus
	{
		NOT_BEAST_MODE,
		BEAST_MODE_LEVEL_ONE
	}

	public int penetration = 1;

	public bool penetratesBreakables;

	public bool preventPenetrationOfActors;

	public BeastModeStatus BeastModeLevel;

	public bool UsesMaxBossImpacts;

	public int MaxBossImpacts = -1;

	private int m_bossImpacts;

	public bool HandleBossImpact()
	{
		if (UsesMaxBossImpacts)
		{
			m_bossImpacts++;
			if (m_bossImpacts >= MaxBossImpacts)
			{
				return true;
			}
		}
		return false;
	}
}
