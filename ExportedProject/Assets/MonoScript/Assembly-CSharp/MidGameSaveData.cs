using System.Collections.Generic;
using FullSerializer;
using InControl;
using UnityEngine;

public class MidGameSaveData
{
	[fsProperty]
	public GlobalDungeonData.ValidTilesets levelSaved = GlobalDungeonData.ValidTilesets.CASTLEGEON;

	[fsProperty]
	public GameManager.GameType savedGameType;

	[fsProperty]
	public GameManager.GameMode savedGameMode;

	[fsProperty]
	public int LastShortcutFloorLoaded;

	[fsProperty]
	public MidGamePlayerData playerOneData;

	[fsProperty]
	public MidGamePlayerData playerTwoData;

	[fsProperty]
	public GameStats PriorSessionStats;

	[fsProperty]
	public MidGameStaticShopData StaticShopData;

	[fsProperty]
	public RunData RunData;

	[fsProperty]
	public string midGameSaveGuid;

	[fsProperty]
	public bool invalidated;

	public static InputDevice ContinuePressedDevice;

	public static bool IsInitializingPlayerData;

	public MidGameSaveData()
	{
	}

	public MidGameSaveData(PlayerController p1, PlayerController p2, GlobalDungeonData.ValidTilesets targetLevel, string midGameSaveGuid)
	{
		this.midGameSaveGuid = midGameSaveGuid;
		levelSaved = targetLevel;
		savedGameMode = GameManager.Instance.CurrentGameMode;
		if (savedGameMode == GameManager.GameMode.SHORTCUT)
		{
			LastShortcutFloorLoaded = GameManager.Instance.LastShortcutFloorLoaded;
		}
		if (p2 != null)
		{
			savedGameType = GameManager.GameType.COOP_2_PLAYER;
		}
		else
		{
			savedGameType = GameManager.GameType.SINGLE_PLAYER;
		}
		playerOneData = new MidGamePlayerData(p1);
		if (savedGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			playerTwoData = new MidGamePlayerData(p2);
		}
		PriorSessionStats = GameStatsManager.Instance.MoveSessionStatsToSavedSessionStats();
		StaticShopData = BaseShopController.GetStaticShopDataForMidGameSave();
		RunData = GameManager.Instance.RunData;
	}

	public bool IsValid()
	{
		return !invalidated;
	}

	public void Invalidate()
	{
		invalidated = true;
	}

	public void Revalidate()
	{
		invalidated = false;
	}

	public GameObject GetPlayerOnePrefab()
	{
		string path = CharacterSelectController.GetCharacterPathFromIdentity(playerOneData.CharacterIdentity);
		if (levelSaved == GlobalDungeonData.ValidTilesets.FINALGEON && playerOneData.CharacterIdentity == PlayableCharacters.Pilot)
		{
			path = "PlayerRogueShip";
		}
		return (GameObject)BraveResources.Load(path);
	}

	public void LoadPreGenDataFromMidGameSave()
	{
		GameManager.Instance.RunData = RunData;
	}

	public void LoadDataFromMidGameSave(PlayerController p1, PlayerController p2)
	{
		if (StaticShopData != null)
		{
			BaseShopController.LoadFromMidGameSave(StaticShopData);
		}
		GameManager.Instance.CurrentGameMode = savedGameMode;
		GameManager.Instance.LastShortcutFloorLoaded = LastShortcutFloorLoaded;
		GameStatsManager.Instance.AssignMidGameSavedSessionStats(PriorSessionStats);
		if ((bool)p1)
		{
			PassiveItem.DecrementFlag(p1, typeof(SevenLeafCloverItem));
		}
		if ((bool)p2)
		{
			PassiveItem.DecrementFlag(p2, typeof(SevenLeafCloverItem));
		}
		InitializePlayerData(p1, playerOneData, true);
		if (savedGameType == GameManager.GameType.COOP_2_PLAYER && (bool)p2)
		{
			InitializePlayerData(p2, playerTwoData, false);
			BraveInput.ReassignAllControllers(ContinuePressedDevice);
		}
		ContinuePressedDevice = null;
	}

	public void InitializePlayerData(PlayerController p1, MidGamePlayerData playerData, bool isPlayerOne)
	{
		IsInitializingPlayerData = true;
		p1.MasteryTokensCollectedThisRun = playerData.MasteryTokensCollected;
		p1.CharacterUsesRandomGuns = playerData.CharacterUsesRandomGuns;
		p1.HasTakenDamageThisRun = playerData.HasTakenDamageThisRun;
		p1.HasFiredNonStartingGun = playerData.HasFiredNonStartingGun;
		GameObject gameObject = (GameObject)ResourceCache.Acquire("Global Prefabs/VFX_ParadoxPortal");
		ParadoxPortalController component = gameObject.GetComponent<ParadoxPortalController>();
		p1.portalEeveeTex = component.CosmicTex;
		p1.IsTemporaryEeveeForUnlock = playerData.IsTemporaryEeveeForUnlock;
		ChallengeManager.ChallengeModeType = playerData.ChallengeMode;
		if (levelSaved == GlobalDungeonData.ValidTilesets.FINALGEON)
		{
			p1.CharacterUsesRandomGuns = false;
		}
		if (levelSaved != GlobalDungeonData.ValidTilesets.FINALGEON || !(p1 is PlayerSpaceshipController))
		{
			p1.inventory.DestroyAllGuns();
			p1.RemoveAllPassiveItems();
			p1.RemoveAllActiveItems();
			if (playerData.passiveItems != null)
			{
				for (int i = 0; i < playerData.passiveItems.Count; i++)
				{
					EncounterTrackable.SuppressNextNotification = true;
					LootEngine.GivePrefabToPlayer(PickupObjectDatabase.GetById(playerData.passiveItems[i].PickupID).gameObject, p1);
				}
			}
			if (playerData.activeItems != null)
			{
				for (int j = 0; j < playerData.activeItems.Count; j++)
				{
					EncounterTrackable.SuppressNextNotification = true;
					LootEngine.GivePrefabToPlayer(PickupObjectDatabase.GetById(playerData.activeItems[j].PickupID).gameObject, p1);
				}
			}
			if (playerData.guns != null)
			{
				for (int k = 0; k < playerData.guns.Count; k++)
				{
					EncounterTrackable.SuppressNextNotification = true;
					LootEngine.GivePrefabToPlayer(PickupObjectDatabase.GetById(playerData.guns[k].PickupID).gameObject, p1);
				}
				for (int l = 0; l < playerData.guns.Count; l++)
				{
					for (int m = 0; m < p1.inventory.AllGuns.Count; m++)
					{
						if (p1.inventory.AllGuns[m].PickupObjectId != playerData.guns[l].PickupID)
						{
							continue;
						}
						p1.inventory.AllGuns[m].MidGameDeserialize(playerData.guns[l].SerializedData);
						for (int n = 0; n < playerData.guns[l].DuctTapedGunIDs.Count; n++)
						{
							Gun gun = PickupObjectDatabase.GetById(playerData.guns[l].DuctTapedGunIDs[n]) as Gun;
							if ((bool)gun)
							{
								DuctTapeItem.DuctTapeGuns(gun, p1.inventory.AllGuns[m]);
							}
						}
						p1.inventory.AllGuns[m].ammo = playerData.guns[l].CurrentAmmo;
						TransformGunSynergyProcessor[] componentsInChildren = p1.inventory.AllGuns[m].GetComponentsInChildren<TransformGunSynergyProcessor>();
						for (int num = 0; num < componentsInChildren.Length; num++)
						{
							componentsInChildren[num].ShouldResetAmmoAfterTransformation = true;
							componentsInChildren[num].ResetAmmoCount = playerData.guns[l].CurrentAmmo;
						}
					}
				}
			}
			if (playerData.CurrentHealth <= 0f && playerData.CurrentArmor <= 0f)
			{
				p1.healthHaver.Armor = 0f;
				p1.DieOnMidgameLoad();
			}
			else
			{
				p1.healthHaver.ForceSetCurrentHealth(playerData.CurrentHealth);
				p1.healthHaver.Armor = playerData.CurrentArmor;
			}
			if (isPlayerOne)
			{
				p1.carriedConsumables.KeyBullets = playerData.CurrentKeys;
				p1.carriedConsumables.Currency = playerData.CurrentCurrency;
			}
			p1.Blanks = Mathf.Max(p1.Blanks, playerData.CurrentBlanks);
			if (playerData.activeItems != null)
			{
				for (int num2 = 0; num2 < playerData.activeItems.Count; num2++)
				{
					for (int num3 = 0; num3 < p1.activeItems.Count; num3++)
					{
						if (playerData.activeItems[num2].PickupID == p1.activeItems[num3].PickupObjectId)
						{
							p1.activeItems[num3].MidGameDeserialize(playerData.activeItems[num2].SerializedData);
							p1.activeItems[num3].CurrentDamageCooldown = playerData.activeItems[num2].DamageCooldown;
							p1.activeItems[num3].CurrentRoomCooldown = playerData.activeItems[num2].RoomCooldown;
							p1.activeItems[num3].CurrentTimeCooldown = playerData.activeItems[num2].TimeCooldown;
							if (p1.activeItems[num3].consumable && playerData.activeItems[num2].NumberOfUses > 0)
							{
								p1.activeItems[num3].numberOfUses = playerData.activeItems[num2].NumberOfUses;
							}
						}
					}
				}
			}
			if (playerData.passiveItems != null)
			{
				for (int num4 = 0; num4 < playerData.passiveItems.Count; num4++)
				{
					for (int num5 = 0; num5 < p1.passiveItems.Count; num5++)
					{
						if (playerData.passiveItems[num4].PickupID == p1.passiveItems[num5].PickupObjectId)
						{
							p1.passiveItems[num5].MidGameDeserialize(playerData.passiveItems[num4].SerializedData);
						}
					}
				}
			}
			if (playerData.ownerlessStatModifiers != null)
			{
				if (p1.ownerlessStatModifiers == null)
				{
					p1.ownerlessStatModifiers = new List<StatModifier>();
				}
				for (int num6 = 0; num6 < playerData.ownerlessStatModifiers.Count; num6++)
				{
					p1.ownerlessStatModifiers.Add(playerData.ownerlessStatModifiers[num6]);
				}
			}
			if (levelSaved == GlobalDungeonData.ValidTilesets.FINALGEON && p1.characterIdentity != PlayableCharacters.Gunslinger)
			{
				p1.ResetToFactorySettings(true, true);
			}
			if ((bool)p1 && p1.stats != null)
			{
				p1.stats.RecalculateStats(p1);
			}
			if (playerData.HasBloodthirst)
			{
				p1.gameObject.GetOrAddComponent<Bloodthirst>();
			}
		}
		IsInitializingPlayerData = false;
		EncounterTrackable.SuppressNextNotification = false;
	}
}
