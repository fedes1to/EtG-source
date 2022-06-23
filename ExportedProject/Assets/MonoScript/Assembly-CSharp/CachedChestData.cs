using System;

[Serializable]
public class CachedChestData
{
	private bool m_glitch;

	private bool m_rainbow;

	private bool m_synergy;

	private PickupObject.ItemQuality chestQuality = PickupObject.ItemQuality.D;

	private bool m_isLocked;

	public CachedChestData(string data)
	{
		Deserialize(data);
	}

	public CachedChestData(Chest c)
	{
		m_isLocked = c.IsLocked;
		m_glitch = c.IsGlitched;
		m_rainbow = c.IsRainbowChest;
		m_synergy = c.lootTable != null && c.lootTable.CompletesSynergy;
		RewardManager rewardManager = GameManager.Instance.RewardManager;
		chestQuality = PickupObject.ItemQuality.D;
		if (c.name.Contains(rewardManager.S_Chest.name))
		{
			chestQuality = PickupObject.ItemQuality.S;
		}
		else if (c.name.Contains(rewardManager.A_Chest.name))
		{
			chestQuality = PickupObject.ItemQuality.A;
		}
		else if (c.name.Contains(rewardManager.B_Chest.name))
		{
			chestQuality = PickupObject.ItemQuality.B;
		}
		else if (c.name.Contains(rewardManager.C_Chest.name))
		{
			chestQuality = PickupObject.ItemQuality.C;
		}
	}

	public void Upgrade()
	{
		switch (chestQuality)
		{
		case PickupObject.ItemQuality.COMMON:
			chestQuality = PickupObject.ItemQuality.D;
			break;
		case PickupObject.ItemQuality.D:
			chestQuality = PickupObject.ItemQuality.C;
			break;
		case PickupObject.ItemQuality.C:
			chestQuality = PickupObject.ItemQuality.B;
			break;
		case PickupObject.ItemQuality.B:
			chestQuality = PickupObject.ItemQuality.A;
			break;
		case PickupObject.ItemQuality.A:
			chestQuality = PickupObject.ItemQuality.S;
			break;
		}
	}

	public string Serialize()
	{
		return chestQuality.ToString() + "|" + m_glitch + "|" + m_rainbow + "|" + m_isLocked + "|" + m_synergy;
	}

	public void Deserialize(string data)
	{
		string[] array = data.Split('|');
		chestQuality = (PickupObject.ItemQuality)Enum.Parse(typeof(PickupObject.ItemQuality), array[0]);
		m_glitch = bool.Parse(array[1]);
		m_rainbow = bool.Parse(array[2]);
		m_isLocked = bool.Parse(array[3]);
		if (array.Length > 4)
		{
			m_synergy = bool.Parse(array[4]);
		}
	}

	public void SpawnChest(IntVector2 position)
	{
		Chest chest = null;
		if (m_synergy)
		{
			chest = GameManager.Instance.RewardManager.Synergy_Chest;
		}
		else
		{
			switch (chestQuality)
			{
			case PickupObject.ItemQuality.S:
				chest = GameManager.Instance.RewardManager.S_Chest;
				break;
			case PickupObject.ItemQuality.A:
				chest = GameManager.Instance.RewardManager.A_Chest;
				break;
			case PickupObject.ItemQuality.B:
				chest = GameManager.Instance.RewardManager.B_Chest;
				break;
			case PickupObject.ItemQuality.C:
				chest = GameManager.Instance.RewardManager.C_Chest;
				break;
			case PickupObject.ItemQuality.D:
				chest = GameManager.Instance.RewardManager.D_Chest;
				break;
			default:
				chest = GameManager.Instance.RewardManager.D_Chest;
				break;
			}
		}
		if ((bool)chest)
		{
			Chest chest2 = Chest.Spawn(chest, position);
			chest2.RegisterChestOnMinimap(position.ToVector2().GetAbsoluteRoom());
			chest2.IsLocked = m_isLocked;
			if (m_glitch)
			{
				chest2.BecomeGlitchChest();
			}
			if (m_rainbow)
			{
				chest2.BecomeRainbowChest();
			}
		}
	}
}
