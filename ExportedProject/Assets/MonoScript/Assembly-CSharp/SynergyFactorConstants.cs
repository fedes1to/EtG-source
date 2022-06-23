using UnityEngine;

public static class SynergyFactorConstants
{
	public static float GetSynergyFactor()
	{
		int numberOfSynergiesEncounteredThisRun = GameStatsManager.Instance.GetNumberOfSynergiesEncounteredThisRun();
		float num = 0.6f;
		float num2 = 0.006260342f + 0.9935921f * Mathf.Exp(-1.626339f * (float)numberOfSynergiesEncounteredThisRun);
		if (numberOfSynergiesEncounteredThisRun == 0)
		{
			num = ((GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_FORGE) >= 3f) ? 0.8f : ((GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_CATACOMBS) >= 3f) ? 1f : ((GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_MINES) >= 3f) ? 1.5f : ((!(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_REACHED_GUNGEON) >= 3f)) ? 5f : 3f))));
		}
		return 1f + num * num2;
	}
}
