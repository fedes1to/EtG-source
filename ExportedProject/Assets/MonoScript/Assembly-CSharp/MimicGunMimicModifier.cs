using UnityEngine;

public class MimicGunMimicModifier : MonoBehaviour
{
	private Gun m_gun;

	private bool m_initialized;

	private void Start()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if (!m_initialized && m_gun.CurrentOwner != null)
		{
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			if (playerController.IsGunLocked)
			{
				Object.Destroy(this);
				return;
			}
			Gun gun = playerController.inventory.AddGunToInventory(PickupObjectDatabase.GetById(GlobalItemIds.GunMimicID) as Gun, true);
			gun.GetComponent<MimicGunController>().Initialize(playerController, m_gun);
			m_initialized = true;
			Object.Destroy(this);
		}
	}
}
