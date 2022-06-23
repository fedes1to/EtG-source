using UnityEngine;

public class MiserlyProtectionItem : BasicStatPickup
{
	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnItemPurchased += OnItemPurchased;
		}
	}

	public void Break()
	{
		m_pickedUp = true;
		Object.Destroy(base.gameObject, 1f);
	}

	private void OnItemPurchased(PlayerController player, ShopItemController obj)
	{
		if ((!(obj != null) || !(obj.item is MiserlyProtectionItem)) && !player.HasActiveBonusSynergy(CustomSynergyType.MISERLY_PIGTECTION))
		{
			player.DropPassiveItem(this);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		MiserlyProtectionItem component = debrisObject.GetComponent<MiserlyProtectionItem>();
		player.OnItemPurchased -= OnItemPurchased;
		component.m_pickedUpThisRun = true;
		if (!player.HasActiveBonusSynergy(CustomSynergyType.MISERLY_PIGTECTION))
		{
			component.Break();
		}
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
