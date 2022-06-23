using UnityEngine;

public class SpawnItemOnRoomClearItem : PassiveItem
{
	public float chanceToSpawn = 0.05f;

	[PickupIdentifier]
	public int spawnItemId = -1;

	public bool requirePlayerDamaged;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			player.OnRoomClearEvent += RoomCleared;
			base.Pickup(player);
		}
	}

	private void RoomCleared(PlayerController obj)
	{
		float value = Random.value;
		if ((!requirePlayerDamaged || !(obj.healthHaver.GetCurrentHealthPercentage() >= 1f)) && !obj.CurrentRoom.PlayerHasTakenDamageInThisRoom)
		{
			if ((bool)base.Owner && base.Owner.HasActiveBonusSynergy(CustomSynergyType.THE_COIN_KING) && itemName == "Crown of the Coin King")
			{
				chanceToSpawn *= 2f;
			}
			if (value < chanceToSpawn)
			{
				PickupObject byId = PickupObjectDatabase.GetById(spawnItemId);
				LootEngine.SpawnItem(byId.gameObject, obj.specRigidbody.UnitCenter, Vector2.up, 1f, false, true);
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.OnRoomClearEvent -= RoomCleared;
		debrisObject.GetComponent<SpawnItemOnRoomClearItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
