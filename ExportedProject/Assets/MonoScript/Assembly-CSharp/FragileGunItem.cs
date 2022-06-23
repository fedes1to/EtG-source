using System.Collections.Generic;
using UnityEngine;

public class FragileGunItem : PassiveItem
{
	public GameObject GunPiecePrefab;

	private PlayerController m_player;

	private Dictionary<int, int> m_workingDictionary = new Dictionary<int, int>();

	private Dictionary<int, int> m_gunToAmmoDictionary = new Dictionary<int, int>();

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.OnReceivedDamage += HandleTookDamage;
		}
	}

	private void HandleTookDamage(PlayerController obj)
	{
		if ((bool)obj && (bool)obj.CurrentGun && !obj.CurrentGun.InfiniteAmmo)
		{
			BreakGun(obj, obj.CurrentGun);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<FragileGunItem>().m_pickedUpThisRun = true;
		player.OnReceivedDamage -= HandleTookDamage;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.OnReceivedDamage -= HandleTookDamage;
		}
	}

	private void BreakGun(PlayerController sourcePlayer, Gun sourceGun)
	{
		int num = 5;
		for (int i = 0; i < num; i++)
		{
			DebrisObject debrisObject = LootEngine.SpawnItem(GunPiecePrefab, sourcePlayer.CenterPosition, Random.insideUnitCircle.normalized, 10f);
			FragileGunItemPiece componentInChildren = debrisObject.GetComponentInChildren<FragileGunItemPiece>();
			componentInChildren.AssignGun(sourceGun);
		}
		m_workingDictionary.Add(sourceGun.PickupObjectId, num);
		m_gunToAmmoDictionary.Add(sourceGun.PickupObjectId, sourceGun.ammo);
		sourcePlayer.inventory.RemoveGunFromInventory(sourceGun);
	}

	public void AcquirePiece(FragileGunItemPiece piece)
	{
		if (piece.AssignedGunId == -1 || !m_workingDictionary.ContainsKey(piece.AssignedGunId))
		{
			return;
		}
		m_workingDictionary[piece.AssignedGunId] = m_workingDictionary[piece.AssignedGunId] - 1;
		if (m_workingDictionary[piece.AssignedGunId] > 0)
		{
			return;
		}
		m_workingDictionary.Remove(piece.AssignedGunId);
		PickupObject byId = PickupObjectDatabase.GetById(piece.AssignedGunId);
		if ((bool)byId)
		{
			Gun gun = LootEngine.TryGiveGunToPlayer(byId.gameObject, m_owner);
			if (m_gunToAmmoDictionary.ContainsKey(piece.AssignedGunId) && (bool)gun)
			{
				gun.ammo = m_gunToAmmoDictionary[piece.AssignedGunId];
				m_gunToAmmoDictionary.Remove(piece.AssignedGunId);
			}
		}
	}
}
