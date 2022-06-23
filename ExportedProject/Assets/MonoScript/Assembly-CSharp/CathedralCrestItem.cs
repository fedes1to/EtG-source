public class CathedralCrestItem : PassiveItem
{
	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.healthHaver.HasCrest = true;
			player.OnReceivedDamage += PlayerDamaged;
			player.healthHaver.Armor += 1f;
		}
	}

	private void PlayerDamaged(PlayerController obj)
	{
		obj.healthHaver.HasCrest = false;
		obj.RemovePassiveItem(PickupObjectId);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.healthHaver.HasCrest = false;
		if ((bool)debrisObject)
		{
			CathedralCrestItem component = debrisObject.GetComponent<CathedralCrestItem>();
			if ((bool)component)
			{
				component.m_pickedUpThisRun = true;
			}
		}
		player.OnReceivedDamage -= PlayerDamaged;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_pickedUp && GameManager.HasInstance && (bool)base.Owner)
		{
			base.Owner.healthHaver.HasCrest = false;
			base.Owner.OnReceivedDamage -= PlayerDamaged;
		}
		base.OnDestroy();
	}
}
