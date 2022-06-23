using System;
using System.Collections.Generic;
using Dungeonator;

public class SynercacheManager : BraveBehaviour
{
	public static bool UseCachedSynergyIDs = false;

	public static List<int> LastCachedSynergyIDs = new List<int>();

	public bool TriggersOnMinimapVisibility;

	private bool m_synercached;

	private RoomHandler m_room;

	public static void ClearPerLevelData()
	{
		UseCachedSynergyIDs = false;
		LastCachedSynergyIDs.Clear();
	}

	private void Start()
	{
		m_room = base.transform.position.GetAbsoluteRoom();
		if (TriggersOnMinimapVisibility)
		{
			RoomHandler room = m_room;
			room.OnRevealedOnMap = (Action)Delegate.Combine(room.OnRevealedOnMap, new Action(Cache));
		}
		m_room.BecameVisible += HandleBecameVisible;
	}

	private void HandleBecameVisible(float delay)
	{
		Cache();
	}

	private void Cache()
	{
		if (m_synercached)
		{
			return;
		}
		m_synercached = true;
		LastCachedSynergyIDs.Clear();
		m_room.BecameVisible -= HandleBecameVisible;
		RoomHandler room = m_room;
		room.OnRevealedOnMap = (Action)Delegate.Remove(room.OnRevealedOnMap, new Action(Cache));
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			for (int j = 0; j < playerController.passiveItems.Count; j++)
			{
				PickupObject pickupObject = playerController.passiveItems[j];
				if ((bool)pickupObject)
				{
					LastCachedSynergyIDs.Add(pickupObject.PickupObjectId);
				}
			}
			for (int k = 0; k < playerController.activeItems.Count; k++)
			{
				PickupObject pickupObject2 = playerController.activeItems[k];
				if ((bool)pickupObject2)
				{
					LastCachedSynergyIDs.Add(pickupObject2.PickupObjectId);
				}
			}
			for (int l = 0; l < playerController.inventory.AllGuns.Count; l++)
			{
				PickupObject pickupObject3 = playerController.inventory.AllGuns[l];
				if ((bool)pickupObject3)
				{
					LastCachedSynergyIDs.Add(pickupObject3.PickupObjectId);
				}
			}
		}
	}
}
