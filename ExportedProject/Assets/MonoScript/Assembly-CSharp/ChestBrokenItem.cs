using System;
using UnityEngine;

public class ChestBrokenItem : PassiveItem
{
	public float ActivationChance = 1f;

	public float HealAmount = 0.5f;

	public GameObject HealVFX;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnChestBroken = (Action<PlayerController, Chest>)Delegate.Combine(player.OnChestBroken, new Action<PlayerController, Chest>(HandleChestBroken));
		}
	}

	private void HandleChestBroken(PlayerController arg1, Chest arg2)
	{
		if (UnityEngine.Random.value < ActivationChance)
		{
			arg1.healthHaver.ApplyHealing(HealAmount);
			arg1.PlayEffectOnActor(HealVFX, Vector3.zero);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		ChestBrokenItem component = debrisObject.GetComponent<ChestBrokenItem>();
		if ((bool)player)
		{
			player.OnChestBroken = (Action<PlayerController, Chest>)Delegate.Remove(player.OnChestBroken, new Action<PlayerController, Chest>(HandleChestBroken));
		}
		if ((bool)component)
		{
			component.m_pickedUpThisRun = true;
		}
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			PlayerController owner = m_owner;
			owner.OnChestBroken = (Action<PlayerController, Chest>)Delegate.Remove(owner.OnChestBroken, new Action<PlayerController, Chest>(HandleChestBroken));
		}
		base.OnDestroy();
	}
}
