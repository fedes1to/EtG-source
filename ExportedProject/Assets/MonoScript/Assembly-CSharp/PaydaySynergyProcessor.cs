using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PaydaySynergyProcessor : MonoBehaviour
{
	[PickupIdentifier]
	public int ItemID01;

	[PickupIdentifier]
	public int ItemID02;

	[PickupIdentifier]
	public int ItemID03;

	private PlayerController m_player;

	public IEnumerator Start()
	{
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if ((bool)base.transform.parent && (bool)base.transform.parent.GetComponent<PlayerController>())
		{
			Initialize(base.transform.parent.GetComponent<PlayerController>());
		}
	}

	private List<IPaydayItem> GetExtantPaydayItems()
	{
		List<IPaydayItem> list = new List<IPaydayItem>();
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if (!playerController || playerController.IsGhost)
			{
				continue;
			}
			for (int j = 0; j < playerController.activeItems.Count; j++)
			{
				if (playerController.activeItems[j] is IPaydayItem)
				{
					list.Add(playerController.activeItems[j] as IPaydayItem);
				}
			}
			for (int k = 0; k < playerController.passiveItems.Count; k++)
			{
				if (playerController.passiveItems[k] is IPaydayItem)
				{
					list.Add(playerController.passiveItems[k] as IPaydayItem);
				}
			}
		}
		return list;
	}

	public void Initialize(PlayerController ownerPlayer)
	{
		if (ownerPlayer == null)
		{
			return;
		}
		m_player = ownerPlayer;
		CompanionSynergyProcessor[] components = GetComponents<CompanionSynergyProcessor>();
		List<string> list = new List<string>();
		for (int i = 0; i < components.Length; i++)
		{
			list.Add(components[i].CompanionGuid);
			components[i].ManuallyAssignedPlayer = m_player;
		}
		List<IPaydayItem> extantPaydayItems = GetExtantPaydayItems();
		bool flag = false;
		IPaydayItem paydayItem = null;
		for (int j = 0; j < extantPaydayItems.Count; j++)
		{
			if (extantPaydayItems[j].HasCachedData())
			{
				flag = true;
				paydayItem = extantPaydayItems[j];
				break;
			}
		}
		if (flag)
		{
			list.Clear();
			list.Add(paydayItem.GetID(0));
			list.Add(paydayItem.GetID(1));
			list.Add(paydayItem.GetID(2));
			for (int k = 0; k < components.Length; k++)
			{
				components[k].CompanionGuid = list[k];
			}
			return;
		}
		list = list.Shuffle();
		for (int l = 0; l < components.Length; l++)
		{
			components[l].CompanionGuid = list[l];
		}
		for (int m = 0; m < components.Length; m++)
		{
			for (int n = 0; n < extantPaydayItems.Count; n++)
			{
				extantPaydayItems[n].StoreData(components[0].CompanionGuid, components[1].CompanionGuid, components[2].CompanionGuid);
			}
		}
	}

	public void Update()
	{
		int num = 0;
		bool flag = false;
		if (!m_player)
		{
			Initialize(base.transform.parent.GetComponent<PlayerController>());
		}
		for (int i = 0; i < m_player.passiveItems.Count; i++)
		{
			if (m_player.passiveItems[i] is BankMaskItem)
			{
				flag = true;
			}
			if (m_player.passiveItems[i].PickupObjectId == ItemID01)
			{
				num++;
			}
			if (m_player.passiveItems[i].PickupObjectId == ItemID02)
			{
				num++;
			}
			if (m_player.passiveItems[i].PickupObjectId == ItemID03)
			{
				num++;
			}
		}
		for (int j = 0; j < m_player.activeItems.Count; j++)
		{
			if (m_player.activeItems[j].PickupObjectId == ItemID01)
			{
				num++;
			}
			if (m_player.activeItems[j].PickupObjectId == ItemID02)
			{
				num++;
			}
			if (m_player.activeItems[j].PickupObjectId == ItemID03)
			{
				num++;
			}
		}
		if (!flag)
		{
			num = 0;
		}
		m_player.CustomEventSynergies.Remove(CustomSynergyType.PAYDAY_ONEITEM);
		m_player.CustomEventSynergies.Remove(CustomSynergyType.PAYDAY_TWOITEM);
		m_player.CustomEventSynergies.Remove(CustomSynergyType.PAYDAY_THREEITEM);
		if (num > 0)
		{
			switch (num)
			{
			case 1:
				m_player.CustomEventSynergies.Add(CustomSynergyType.PAYDAY_ONEITEM);
				break;
			case 2:
				m_player.CustomEventSynergies.Add(CustomSynergyType.PAYDAY_ONEITEM);
				m_player.CustomEventSynergies.Add(CustomSynergyType.PAYDAY_TWOITEM);
				break;
			default:
				m_player.CustomEventSynergies.Add(CustomSynergyType.PAYDAY_ONEITEM);
				m_player.CustomEventSynergies.Add(CustomSynergyType.PAYDAY_TWOITEM);
				m_player.CustomEventSynergies.Add(CustomSynergyType.PAYDAY_THREEITEM);
				break;
			}
		}
	}
}
