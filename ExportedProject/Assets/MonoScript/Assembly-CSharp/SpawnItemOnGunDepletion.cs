using Dungeonator;
using UnityEngine;

public class SpawnItemOnGunDepletion : MonoBehaviour
{
	public bool IsSynergyContingent;

	public CustomSynergyType SynergyToCheck;

	public bool UsesSpecificItem;

	[PickupIdentifier]
	public int SpecificItemId;

	protected Gun m_gun;

	private void Start()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if (!base.enabled || !m_gun || m_gun.ammo > 0 || !(m_gun.CurrentOwner is PlayerController))
		{
			return;
		}
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if (IsSynergyContingent && !playerController.HasActiveBonusSynergy(SynergyToCheck))
		{
			return;
		}
		if (UsesSpecificItem)
		{
			LootEngine.TryGivePrefabToPlayer(PickupObjectDatabase.GetById(SpecificItemId).gameObject, playerController);
		}
		else if ((bool)playerController && playerController.CurrentRoom != null)
		{
			IntVector2 bestRewardLocation = playerController.CurrentRoom.GetBestRewardLocation(IntVector2.One * 3, RoomHandler.RewardLocationStyle.PlayerCenter);
			Chest chest = GameManager.Instance.RewardManager.SpawnTotallyRandomChest(bestRewardLocation);
			if ((bool)chest)
			{
				chest.IsLocked = false;
			}
		}
		playerController.inventory.RemoveGunFromInventory(m_gun);
		Object.Destroy(m_gun.gameObject);
	}
}
