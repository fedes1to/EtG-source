using System;
using UnityEngine;

[Serializable]
public class PlayerConsumables
{
	[NonSerialized]
	private bool m_infiniteKeys;

	[SerializeField]
	private int StartCurrency;

	[SerializeField]
	private int StartKeyBullets = 1;

	private int m_currency;

	private int m_keyBullets;

	private int m_ratKeys;

	public int Currency
	{
		get
		{
			return m_currency;
		}
		set
		{
			int num = Mathf.Max(0, value);
			if (num > m_currency && GameStatsManager.HasInstance)
			{
				GameStatsManager.Instance.RegisterStatChange(TrackedStats.TOTAL_MONEY_COLLECTED, num - m_currency);
			}
			if (num >= 300 && GameManager.HasInstance && GameManager.Instance.platformInterface != null)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				if (realtimeSinceStartup > PlatformInterface.LastManyCoinsUnlockTime + 5f || realtimeSinceStartup < PlatformInterface.LastManyCoinsUnlockTime)
				{
					GameManager.Instance.platformInterface.AchievementUnlock(Achievement.HAVE_MANY_COINS);
					PlatformInterface.LastManyCoinsUnlockTime = realtimeSinceStartup;
				}
			}
			m_currency = num;
			if (GameUIRoot.HasInstance)
			{
				GameUIRoot.Instance.UpdatePlayerConsumables(this);
			}
		}
	}

	public int KeyBullets
	{
		get
		{
			return m_keyBullets;
		}
		set
		{
			m_keyBullets = value;
			GameStatsManager.Instance.UpdateMaximum(TrackedMaximums.MOST_KEYS_HELD, m_keyBullets);
			GameUIRoot.Instance.UpdatePlayerConsumables(this);
		}
	}

	public int ResourcefulRatKeys
	{
		get
		{
			return m_ratKeys;
		}
		set
		{
			m_ratKeys = value;
			GameUIRoot.Instance.UpdatePlayerConsumables(this);
		}
	}

	public bool InfiniteKeys
	{
		get
		{
			if (GameManager.Instance.AllPlayers != null)
			{
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					PlayerController playerController = GameManager.Instance.AllPlayers[i];
					if ((bool)playerController && (bool)playerController.CurrentGun && playerController.CurrentGun.gunName == "AKey-47")
					{
						return true;
					}
				}
			}
			return m_infiniteKeys;
		}
		set
		{
			m_infiniteKeys = value;
		}
	}

	public void Initialize()
	{
		Currency = StartCurrency;
		KeyBullets = StartKeyBullets;
	}

	public void ForceUpdateUI()
	{
		if (GameUIRoot.Instance != null)
		{
			GameUIRoot.Instance.UpdatePlayerConsumables(this);
		}
	}
}
