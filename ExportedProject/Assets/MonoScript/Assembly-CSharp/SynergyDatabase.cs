using System.Collections.Generic;
using UnityEngine;

public class SynergyDatabase : ScriptableObject
{
	public static Color SynergyBlue = new Color(0.596078455f, 50f / 51f, 1f);

	[SerializeField]
	public SynergyEntry[] synergies;

	public void RebuildSynergies(PlayerController p, List<int> previouslyActiveSynergies)
	{
		if (!p)
		{
			return;
		}
		if (p.ActiveExtraSynergies == null)
		{
			p.ActiveExtraSynergies = new List<int>();
		}
		p.ActiveExtraSynergies.Clear();
		if (p.inventory == null)
		{
			return;
		}
		for (int i = 0; i < synergies.Length; i++)
		{
			if (synergies[i].SynergyIsAvailable(GameManager.Instance.PrimaryPlayer, GameManager.Instance.SecondaryPlayer))
			{
				p.ActiveExtraSynergies.Add(i);
			}
		}
	}
}
