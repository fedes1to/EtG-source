using UnityEngine;

public class OnPurchasePassiveItem : PassiveItem
{
	public float ActivationChance = 0.5f;

	public bool DoesHeal = true;

	public float HealingAmount = 0.5f;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnItemPurchased += OnItemPurchased;
		}
	}

	private void OnItemPurchased(PlayerController player, ShopItemController obj)
	{
		if (Random.value < ActivationChance && DoesHeal)
		{
			player.healthHaver.ApplyHealing(HealingAmount);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		OnPurchasePassiveItem component = debrisObject.GetComponent<OnPurchasePassiveItem>();
		player.OnItemPurchased -= OnItemPurchased;
		component.m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
