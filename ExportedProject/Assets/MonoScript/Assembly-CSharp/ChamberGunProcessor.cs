using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class ChamberGunProcessor : MonoBehaviour, ILevelLoadedListener
{
	[PickupIdentifier]
	public int CastleGunID;

	[PickupIdentifier]
	public int GungeonGunID;

	[PickupIdentifier]
	public int MinesGunID;

	[PickupIdentifier]
	public int HollowGunID;

	[PickupIdentifier]
	public int ForgeGunID;

	[PickupIdentifier]
	public int HellGunID;

	[PickupIdentifier]
	public int OublietteGunID;

	[PickupIdentifier]
	public int AbbeyGunID;

	[PickupIdentifier]
	public int RatgeonGunID;

	[PickupIdentifier]
	public int OfficeGunID;

	public bool RefillsOnFloorChange = true;

	private GlobalDungeonData.ValidTilesets m_currentTileset;

	private Gun m_gun;

	[NonSerialized]
	public bool JustActiveReloaded;

	private void Awake()
	{
		m_currentTileset = GlobalDungeonData.ValidTilesets.CASTLEGEON;
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
	}

	private GlobalDungeonData.ValidTilesets GetFloorTileset()
	{
		if (GameManager.Instance.IsLoadingLevel || !GameManager.Instance.Dungeon)
		{
			return GlobalDungeonData.ValidTilesets.CASTLEGEON;
		}
		return GameManager.Instance.Dungeon.tileIndices.tilesetId;
	}

	private bool IsValidTileset(GlobalDungeonData.ValidTilesets t)
	{
		if (t == GetFloorTileset())
		{
			return true;
		}
		PlayerController playerController = m_gun.CurrentOwner as PlayerController;
		if ((bool)playerController)
		{
			if (t == GlobalDungeonData.ValidTilesets.CASTLEGEON && playerController.HasPassiveItem(GlobalItemIds.MasteryToken_Castle))
			{
				return true;
			}
			if (t == GlobalDungeonData.ValidTilesets.GUNGEON && playerController.HasPassiveItem(GlobalItemIds.MasteryToken_Gungeon))
			{
				return true;
			}
			if (t == GlobalDungeonData.ValidTilesets.MINEGEON && playerController.HasPassiveItem(GlobalItemIds.MasteryToken_Mines))
			{
				return true;
			}
			if (t == GlobalDungeonData.ValidTilesets.CATACOMBGEON && playerController.HasPassiveItem(GlobalItemIds.MasteryToken_Catacombs))
			{
				return true;
			}
			if (t == GlobalDungeonData.ValidTilesets.FORGEGEON && playerController.HasPassiveItem(GlobalItemIds.MasteryToken_Forge))
			{
				return true;
			}
		}
		return false;
	}

	private void ChangeToTileset(GlobalDungeonData.ValidTilesets t)
	{
		switch (t)
		{
		case GlobalDungeonData.ValidTilesets.CASTLEGEON:
			ChangeForme(CastleGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.CASTLEGEON;
			break;
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			ChangeForme(GungeonGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.GUNGEON;
			break;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			ChangeForme(MinesGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.MINEGEON;
			break;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			ChangeForme(HollowGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.CATACOMBGEON;
			break;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			ChangeForme(ForgeGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.FORGEGEON;
			break;
		case GlobalDungeonData.ValidTilesets.HELLGEON:
			ChangeForme(HellGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.HELLGEON;
			break;
		case GlobalDungeonData.ValidTilesets.SEWERGEON:
			ChangeForme(OublietteGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.SEWERGEON;
			break;
		case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
			ChangeForme(AbbeyGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.CATHEDRALGEON;
			break;
		case GlobalDungeonData.ValidTilesets.RATGEON:
			ChangeForme(RatgeonGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.RATGEON;
			break;
		case GlobalDungeonData.ValidTilesets.OFFICEGEON:
			ChangeForme(OfficeGunID);
			m_currentTileset = GlobalDungeonData.ValidTilesets.OFFICEGEON;
			break;
		default:
			ChangeForme(CastleGunID);
			m_currentTileset = GetFloorTileset();
			break;
		}
	}

	private void ChangeForme(int targetID)
	{
		Gun targetGun = PickupObjectDatabase.GetById(targetID) as Gun;
		m_gun.TransformToTargetGun(targetGun);
	}

	private void Update()
	{
		if (Dungeon.IsGenerating || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		if ((bool)m_gun && (!m_gun.CurrentOwner || !IsValidTileset(m_currentTileset)))
		{
			GlobalDungeonData.ValidTilesets validTilesets = GetFloorTileset();
			if (!m_gun.CurrentOwner)
			{
				validTilesets = GlobalDungeonData.ValidTilesets.CASTLEGEON;
			}
			if (m_currentTileset != validTilesets)
			{
				ChangeToTileset(validTilesets);
			}
		}
		JustActiveReloaded = false;
	}

	private GlobalDungeonData.ValidTilesets GetNextTileset(GlobalDungeonData.ValidTilesets inTileset)
	{
		switch (inTileset)
		{
		case GlobalDungeonData.ValidTilesets.CASTLEGEON:
			return GlobalDungeonData.ValidTilesets.SEWERGEON;
		case GlobalDungeonData.ValidTilesets.SEWERGEON:
			return GlobalDungeonData.ValidTilesets.GUNGEON;
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			return GlobalDungeonData.ValidTilesets.CATHEDRALGEON;
		case GlobalDungeonData.ValidTilesets.CATHEDRALGEON:
			return GlobalDungeonData.ValidTilesets.MINEGEON;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			return GlobalDungeonData.ValidTilesets.RATGEON;
		case GlobalDungeonData.ValidTilesets.RATGEON:
			return GlobalDungeonData.ValidTilesets.CATACOMBGEON;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			return GlobalDungeonData.ValidTilesets.OFFICEGEON;
		case GlobalDungeonData.ValidTilesets.OFFICEGEON:
			return GlobalDungeonData.ValidTilesets.FORGEGEON;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			return GlobalDungeonData.ValidTilesets.HELLGEON;
		case GlobalDungeonData.ValidTilesets.HELLGEON:
			return GlobalDungeonData.ValidTilesets.CASTLEGEON;
		default:
			return GlobalDungeonData.ValidTilesets.CASTLEGEON;
		}
	}

	private GlobalDungeonData.ValidTilesets GetNextValidTileset()
	{
		GlobalDungeonData.ValidTilesets nextTileset = GetNextTileset(m_currentTileset);
		while (!IsValidTileset(nextTileset))
		{
			nextTileset = GetNextTileset(nextTileset);
		}
		return nextTileset;
	}

	private void HandleReloadPressed(PlayerController ownerPlayer, Gun sourceGun, bool manual)
	{
		if (!JustActiveReloaded && manual && !sourceGun.IsReloading)
		{
			GlobalDungeonData.ValidTilesets nextValidTileset = GetNextValidTileset();
			if (m_currentTileset != nextValidTileset)
			{
				ChangeToTileset(nextValidTileset);
			}
		}
	}

	public void BraveOnLevelWasLoaded()
	{
		if (RefillsOnFloorChange && (bool)m_gun && (bool)m_gun.CurrentOwner)
		{
			m_gun.StartCoroutine(DelayedRegainAmmo());
		}
	}

	private IEnumerator DelayedRegainAmmo()
	{
		yield return null;
		while (Dungeon.IsGenerating)
		{
			yield return null;
		}
		if (RefillsOnFloorChange && (bool)m_gun && (bool)m_gun.CurrentOwner)
		{
			m_gun.GainAmmo(m_gun.AdjustedMaxAmmo);
		}
	}
}
