using System;
using UnityEngine;

public class ReloadSwitchSynergyProcessor : MonoBehaviour
{
	[PickupIdentifier]
	public int PartnerGunID;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool ReloadsTargetGun = true;

	private Gun m_gun;

	public void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnPostFired = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnPostFired, new Action<PlayerController, Gun>(HandlePostFired));
	}

	private void HandlePostFired(PlayerController sourcePlayer, Gun sourceGun)
	{
		if (!sourcePlayer.HasActiveBonusSynergy(RequiredSynergy) || m_gun.ClipShotsRemaining != 0)
		{
			return;
		}
		for (int i = 0; i < sourcePlayer.inventory.AllGuns.Count; i++)
		{
			if (sourcePlayer.inventory.AllGuns[i].PickupObjectId == PartnerGunID && sourcePlayer.inventory.AllGuns[i].ammo > 0)
			{
				sourcePlayer.inventory.GunChangeForgiveness = true;
				sourcePlayer.ChangeToGunSlot(i);
				sourcePlayer.inventory.AllGuns[i].ForceImmediateReload(true);
				sourcePlayer.inventory.GunChangeForgiveness = false;
				break;
			}
		}
	}
}
