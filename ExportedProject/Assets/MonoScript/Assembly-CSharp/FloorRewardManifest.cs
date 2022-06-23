using System.Collections.Generic;
using UnityEngine;

public class FloorRewardManifest
{
	public Dictionary<Chest, List<PickupObject>> PregeneratedChestContents = new Dictionary<Chest, List<PickupObject>>();

	public List<PickupObject> PregeneratedBossRewards = new List<PickupObject>();

	private int m_bossIndex;

	public List<PickupObject> PregeneratedBossRewardsGunsOnly = new List<PickupObject>();

	private int m_bossGunIndex;

	public List<PickupObject> OtherRegisteredRewards = new List<PickupObject>();

	public void Initialize(RewardManager manager)
	{
		for (int i = 0; i < 5; i++)
		{
			GameObject rewardObjectForBossSeeded = manager.GetRewardObjectForBossSeeded(null, false);
			if ((bool)rewardObjectForBossSeeded)
			{
				PickupObject component = rewardObjectForBossSeeded.GetComponent<PickupObject>();
				PregeneratedBossRewards.Add(component);
			}
			rewardObjectForBossSeeded = manager.GetRewardObjectForBossSeeded(null, true);
			if ((bool)rewardObjectForBossSeeded)
			{
				PickupObject component2 = rewardObjectForBossSeeded.GetComponent<PickupObject>();
				PregeneratedBossRewardsGunsOnly.Add(component2);
			}
		}
	}

	public void Reinitialize(RewardManager manager)
	{
		PregeneratedChestContents.Clear();
		OtherRegisteredRewards.Clear();
	}

	public bool CheckManifestDifferentiator(PickupObject testItem)
	{
		if (PregeneratedBossRewards.Count > 0 && testItem.PickupObjectId == PregeneratedBossRewards[0].PickupObjectId)
		{
			return true;
		}
		if (PregeneratedBossRewardsGunsOnly.Count > 0 && testItem.PickupObjectId == PregeneratedBossRewardsGunsOnly[0].PickupObjectId)
		{
			return true;
		}
		foreach (KeyValuePair<Chest, List<PickupObject>> pregeneratedChestContent in PregeneratedChestContents)
		{
			for (int i = 0; i < pregeneratedChestContent.Value.Count; i++)
			{
				if (pregeneratedChestContent.Value[i].PickupObjectId == testItem.PickupObjectId)
				{
					return true;
				}
			}
		}
		for (int j = 0; j < OtherRegisteredRewards.Count; j++)
		{
			if (OtherRegisteredRewards[j].PickupObjectId == testItem.PickupObjectId)
			{
				return true;
			}
		}
		return false;
	}

	public PickupObject GetNextBossReward(bool forceGun)
	{
		if (forceGun)
		{
			m_bossGunIndex++;
			return PregeneratedBossRewardsGunsOnly[m_bossGunIndex - 1];
		}
		m_bossIndex++;
		return PregeneratedBossRewards[m_bossIndex - 1];
	}

	public void RegisterContents(Chest source, List<PickupObject> contents)
	{
		PregeneratedChestContents.Add(source, contents);
	}
}
