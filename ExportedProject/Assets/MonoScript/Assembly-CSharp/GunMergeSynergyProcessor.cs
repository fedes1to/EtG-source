using UnityEngine;

public class GunMergeSynergyProcessor : MonoBehaviour
{
	[PickupIdentifier]
	public int OtherGunID;

	[PickupIdentifier]
	public int MergeGunID;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	public void Update()
	{
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (!playerController)
		{
			return;
		}
		for (int i = 0; i < playerController.inventory.AllGuns.Count; i++)
		{
			if (playerController.inventory.AllGuns[i].PickupObjectId == OtherGunID)
			{
				playerController.inventory.RemoveGunFromInventory(playerController.inventory.AllGuns[i]);
				playerController.inventory.RemoveGunFromInventory(m_gun);
				LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(MergeGunID).gameObject, playerController, true);
			}
		}
	}
}
