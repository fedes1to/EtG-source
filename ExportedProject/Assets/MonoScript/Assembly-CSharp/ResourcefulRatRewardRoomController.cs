using System.Collections.Generic;
using System.Linq;
using Dungeonator;
using UnityEngine;

public class ResourcefulRatRewardRoomController : DungeonPlaceableBehaviour
{
	[PickupIdentifier]
	public int[] PedestalA1Items;

	[PickupIdentifier]
	public int[] PedestalA2Items;

	[PickupIdentifier]
	public int[] PedestalA3Items;

	[PickupIdentifier]
	public int[] PedestalA4Items;

	[PickupIdentifier]
	public int[] PedestalB1Items;

	[PickupIdentifier]
	public int[] PedestalB2Items;

	[PickupIdentifier]
	public int[] PedestalB3Items;

	[PickupIdentifier]
	public int[] PedestalB4Items;

	[PickupIdentifier]
	public int[] RatChestItems;

	private RewardPedestal[] m_pedestals;

	private Chest[] m_ratChests;

	public void Start()
	{
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		List<RewardPedestal> componentsInRoom = absoluteRoom.GetComponentsInRoom<RewardPedestal>();
		componentsInRoom = componentsInRoom.ttOrderByDescending((RewardPedestal a) => a.transform.position.x * 10000f + a.transform.position.y);
		m_pedestals = componentsInRoom.ToArray();
		m_pedestals[0].SpecificItemId = PedestalA1Items[Random.Range(0, PedestalA1Items.Length)];
		m_pedestals[1].SpecificItemId = PedestalA2Items[Random.Range(0, PedestalA2Items.Length)];
		m_pedestals[2].SpecificItemId = PedestalA3Items[Random.Range(0, PedestalA3Items.Length)];
		m_pedestals[3].SpecificItemId = PedestalA4Items[Random.Range(0, PedestalA4Items.Length)];
		m_pedestals[4].SpecificItemId = PedestalB1Items[Random.Range(0, PedestalB1Items.Length)];
		m_pedestals[5].SpecificItemId = PedestalB2Items[Random.Range(0, PedestalB2Items.Length)];
		m_pedestals[6].SpecificItemId = PedestalB3Items[Random.Range(0, PedestalB3Items.Length)];
		m_pedestals[7].SpecificItemId = PedestalB4Items[Random.Range(0, PedestalB4Items.Length)];
		for (int i = 0; i < m_pedestals.Length; i++)
		{
			m_pedestals[i].ForceConfiguration();
		}
		List<Chest> componentsInRoom2 = absoluteRoom.GetComponentsInRoom<Chest>();
		m_ratChests = new Chest[4];
		int num = 0;
		for (int j = 0; j < componentsInRoom2.Count; j++)
		{
			if (componentsInRoom2[j].ChestIdentifier == Chest.SpecialChestIdentifier.RAT)
			{
				m_ratChests[num] = componentsInRoom2[j];
				num++;
				if (num >= m_ratChests.Length)
				{
					break;
				}
			}
		}
		List<int> list = Enumerable.Range(0, RatChestItems.Length).ToList().Shuffle();
		for (int k = 0; k < m_ratChests.Length; k++)
		{
			m_ratChests[k].forceContentIds = new List<int>();
			m_ratChests[k].forceContentIds.Add(RatChestItems[list[k]]);
		}
	}
}
