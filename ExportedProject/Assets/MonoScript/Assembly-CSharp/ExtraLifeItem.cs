using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class ExtraLifeItem : PassiveItem
{
	public enum ExtraLifeMode
	{
		ESCAPE_ROPE,
		DARK_SOULS,
		CLONE
	}

	public static GameObject LastActivatedBonfire;

	private static List<RoomHandler> s_bonfiredRooms = new List<RoomHandler>();

	public ExtraLifeMode extraLifeMode;

	public bool consumedOnUse = true;

	[ShowInInspectorIf("extraLifeMode", 1, false)]
	public bool DropDarkSoulsItems;

	[ShowInInspectorIf("extraLifeMode", 1, false)]
	public int DarkSoulsCursedHealthMax = -1;

	[PickupIdentifier]
	public int[] ExcludedPickupIDs;

	public bool DoesBonfireSynergy;

	public GameObject BonfireSynergyBonfire;

	public static void ClearPerLevelData()
	{
		s_bonfiredRooms.Clear();
		LastActivatedBonfire = null;
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			player.healthHaver.OnPreDeath += HandlePreDeath;
			base.Pickup(player);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!DoesBonfireSynergy || !m_owner || !m_owner.HasActiveBonusSynergy(CustomSynergyType.THE_REAL_DARK_SOULS) || m_owner.CurrentRoom == null || s_bonfiredRooms.Contains(m_owner.CurrentRoom) || GameManager.Instance.CurrentLevelOverrideState != 0)
		{
			return;
		}
		RoomHandler currentRoom = m_owner.CurrentRoom;
		bool flag = currentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.SPECIAL && currentRoom.area.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.STANDARD_SHOP;
		if (flag | (currentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.EXIT))
		{
			bool success = false;
			IntVector2 centeredVisibleClearSpot = currentRoom.GetCenteredVisibleClearSpot(4, 4, out success);
			if (success)
			{
				LastActivatedBonfire = Object.Instantiate(BonfireSynergyBonfire, (centeredVisibleClearSpot + new IntVector2(1, 1)).ToVector2().ToVector3ZisY(), Quaternion.identity);
				LootEngine.DoDefaultSynergyPoof(centeredVisibleClearSpot.ToVector2() + new Vector2(2f, 2f));
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(LastActivatedBonfire.GetComponent<SpeculativeRigidbody>());
			}
			s_bonfiredRooms.Add(currentRoom);
		}
	}

	private void HandlePreDeath(Vector2 damageDirection)
	{
		if ((bool)m_owner)
		{
			if (m_owner.IsInMinecart)
			{
				m_owner.currentMineCart.EvacuateSpecificPlayer(m_owner, true);
			}
			for (int i = 0; i < m_owner.passiveItems.Count; i++)
			{
				if (m_owner.passiveItems[i] is CompanionItem && m_owner.passiveItems[i].DisplayName == "Pig")
				{
					return;
				}
				if (m_owner.passiveItems[i] is ExtraLifeItem && extraLifeMode != ExtraLifeMode.DARK_SOULS)
				{
					ExtraLifeItem extraLifeItem = m_owner.passiveItems[i] as ExtraLifeItem;
					if (extraLifeItem.extraLifeMode == ExtraLifeMode.DARK_SOULS)
					{
						return;
					}
				}
			}
		}
		if (m_owner.IsInMinecart)
		{
			m_owner.currentMineCart.EvacuateSpecificPlayer(m_owner, true);
		}
		switch (extraLifeMode)
		{
		case ExtraLifeMode.ESCAPE_ROPE:
			HandleEscapeRopeStyle();
			break;
		case ExtraLifeMode.DARK_SOULS:
			HandleDarkSoulsStyle();
			break;
		case ExtraLifeMode.CLONE:
			HandleCloneStyle();
			return;
		}
		if (consumedOnUse)
		{
			m_owner.RemovePassiveItem(PickupObjectId);
		}
	}

	private void HandleEscapeRopeStyle()
	{
		m_owner.healthHaver.FullHeal();
		m_owner.EscapeRoom(PlayerController.EscapeSealedRoomStyle.NONE, true);
	}

	private void HandleCloneStyle()
	{
		m_owner.HandleCloneItem(this);
	}

	private void HandleDarkSoulsStyle()
	{
		m_owner.TriggerDarkSoulsReset(DropDarkSoulsItems, DarkSoulsCursedHealthMax);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.healthHaver.OnPreDeath -= HandlePreDeath;
		debrisObject.GetComponent<ExtraLifeItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_owner != null)
		{
			m_owner.healthHaver.OnPreDeath -= HandlePreDeath;
		}
		base.OnDestroy();
	}
}
