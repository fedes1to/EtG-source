using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class RewardManager : ScriptableObject
{
	public enum RewardSource
	{
		UNSPECIFIED,
		BOSS_PEDESTAL
	}

	[NonSerialized]
	public static float AdditionalHeartTierMagnificence;

	[SerializeField]
	public List<FloorRewardData> FloorRewardData;

	[Header("Chest Definitions")]
	public Chest D_Chest;

	public Chest C_Chest;

	public Chest B_Chest;

	public Chest A_Chest;

	public Chest S_Chest;

	public Chest Rainbow_Chest;

	public Chest Synergy_Chest;

	[Header("Loot Table Definitions")]
	public GenericLootTable GunsLootTable;

	public GenericLootTable ItemsLootTable;

	[Header("Global Currency Settings")]
	public float BossGoldCoinChance = 0.0003f;

	public float PowerfulGoldCoinChance = 0.000125f;

	public float NormalGoldCoinChance = 5E-05f;

	[Space(5f)]
	public int RobotMinCurrencyPerHealthItem = 5;

	public int RobotMaxCurrencyPerHealthItem = 10;

	[Header("Synergy Settings")]
	public float GlobalSynerchestChance = 0.02f;

	public float SynergyCompletionMultiplier = 1f;

	public bool SynergyCompletionIgnoresQualities;

	[Header("Additional Settings")]
	public float EarlyChestChanceIfNotChump = 0.2f;

	public float RoomClearRainbowChance = 0.0001f;

	[PickupIdentifier]
	public int FullHeartIdPrefab = -1;

	[PickupIdentifier]
	public int HalfHeartIdPrefab = -1;

	public float SinglePlayerPickupIncrementModifier = 1.25f;

	public float CoopPickupIncrementModifier = 1.5f;

	public float CoopAmmoChanceModifier = 1.5f;

	public float GunMimicMimicGunChance = 0.001f;

	[Header("Bonus Enemy Spawn Settings")]
	public BonusEnemySpawns KeybulletsChances;

	public BonusEnemySpawns ChanceBulletChances;

	public BonusEnemySpawns WallMimicChances;

	[Header("Heart Magnificence Settings")]
	public float OneOrTwoHeartMagMultiplier = 0.333f;

	public float ThreeOrMoreHeartMagMultiplier = 0.1f;

	[Header("Chest Destruction Settings")]
	public float ChestDowngradeChance = 0.25f;

	public float ChestHalfHeartChance = 0.2f;

	public float ChestJunkChance = 0.45f;

	public float ChestExplosionChance = 0.1f;

	public float ChestJunkanUnlockedChance = 0.05f;

	public float HasKeyJunkMultiplier = 3f;

	public float HasJunkanJunkMultiplier = 1.5f;

	[Header("Data References (for Brents)")]
	[EnemyIdentifier]
	public string FacelessCultistGuid;

	public float FacelessChancePerFloor = 0.15f;

	[Header("Bowler Notes")]
	public GameObject BowlerNotePostRainbow;

	public GameObject BowlerNoteChest;

	public GameObject BowlerNoteOtherSource;

	public GameObject BowlerNoteMimic;

	public GameObject BowlerNoteShop;

	public GameObject BowlerNoteBoss;

	[Header("Demo Mode Stuff For Pax EAST 2018")]
	[EnemyIdentifier]
	public string PhaseSpiderGUID;

	[EnemyIdentifier]
	public string ChancebulonGUID;

	[EnemyIdentifier]
	public string DisplacerBeastGUID;

	[EnemyIdentifier]
	public string GripmasterGUID;

	public List<EnemyReplacementTier> ReplacementTiers;

	[NonSerialized]
	public Dictionary<GlobalDungeonData.ValidTilesets, FloorRewardManifest> SeededRunManifests = new Dictionary<GlobalDungeonData.ValidTilesets, FloorRewardManifest>();

	public PickupObject FullHeartPrefab
	{
		get
		{
			return PickupObjectDatabase.GetById(FullHeartIdPrefab);
		}
	}

	public PickupObject HalfHeartPrefab
	{
		get
		{
			return PickupObjectDatabase.GetById(HalfHeartIdPrefab);
		}
	}

	public FloorRewardData CurrentRewardData
	{
		get
		{
			return GetRewardDataForFloor(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId);
		}
	}

	private float ItemVsGunChanceBossReward
	{
		get
		{
			return 0.625f;
		}
	}

	private FloorRewardData GetRewardDataForFloor(GlobalDungeonData.ValidTilesets targetTileset)
	{
		FloorRewardData floorRewardData = null;
		for (int i = 0; i < FloorRewardData.Count; i++)
		{
			if ((FloorRewardData[i].AssociatedTilesets | targetTileset) == FloorRewardData[i].AssociatedTilesets)
			{
				floorRewardData = FloorRewardData[i];
			}
		}
		if (floorRewardData == null)
		{
			floorRewardData = FloorRewardData[0];
		}
		return floorRewardData;
	}

	public GameObject GetShopItemResourcefulRatStyle(List<GameObject> excludedObjects = null, System.Random safeRandom = null)
	{
		PickupObject.ItemQuality targetQuality = PickupObject.ItemQuality.D;
		switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
		{
		case GlobalDungeonData.ValidTilesets.GUNGEON:
			targetQuality = PickupObject.ItemQuality.D;
			break;
		case GlobalDungeonData.ValidTilesets.MINEGEON:
			targetQuality = PickupObject.ItemQuality.C;
			break;
		case GlobalDungeonData.ValidTilesets.CATACOMBGEON:
			targetQuality = PickupObject.ItemQuality.B;
			break;
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			targetQuality = PickupObject.ItemQuality.B;
			break;
		}
		return GetRawItem(GunsLootTable, targetQuality, excludedObjects, true, safeRandom);
	}

	public GameObject GetRewardObjectShopStyle(PlayerController player, bool forceGun = false, bool forceItem = false, List<GameObject> excludedObjects = null)
	{
		FloorRewardData currentRewardData = CurrentRewardData;
		bool flag = ((!GameManager.Instance.IsSeeded) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) > 0.5f;
		if (forceGun)
		{
			flag = true;
		}
		if (forceItem)
		{
			flag = false;
		}
		PickupObject.ItemQuality shopTargetQuality = currentRewardData.GetShopTargetQuality(GameManager.Instance.IsSeeded);
		System.Random random = null;
		if (GameManager.Instance.IsSeeded)
		{
			random = BraveRandom.GeneratorRandom;
		}
		PlayerController player2;
		GenericLootTable gunsLootTable;
		PickupObject.ItemQuality targetQuality;
		List<GameObject> excludedObjects2;
		System.Random safeRandom;
		if (flag)
		{
			List<GameObject> list = new List<GameObject>();
			ExcludeUnfinishedGunIfNecessary(list);
			player2 = player;
			gunsLootTable = GunsLootTable;
			targetQuality = shopTargetQuality;
			excludedObjects2 = excludedObjects;
			safeRandom = random;
			return GetItemForPlayer(player2, gunsLootTable, targetQuality, excludedObjects2, false, safeRandom, false, list);
		}
		List<GameObject> list2 = new List<GameObject>();
		BuildExcludedShopList(list2);
		player2 = player;
		gunsLootTable = ItemsLootTable;
		targetQuality = shopTargetQuality;
		excludedObjects2 = excludedObjects;
		safeRandom = random;
		return GetItemForPlayer(player2, gunsLootTable, targetQuality, excludedObjects2, false, safeRandom, false, list2);
	}

	private void ExcludeUnfinishedGunIfNecessary(List<GameObject> excluded)
	{
		for (int i = 0; i < GunsLootTable.defaultItemDrops.elements.Count; i++)
		{
			WeightedGameObject weightedGameObject = GunsLootTable.defaultItemDrops.elements[i];
			if ((bool)weightedGameObject.gameObject)
			{
				PickupObject component = weightedGameObject.gameObject.GetComponent<PickupObject>();
				if ((bool)component && component.PickupObjectId == GlobalItemIds.UnfinishedGun && GameStatsManager.HasInstance && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE))
				{
					excluded.Add(weightedGameObject.gameObject);
				}
			}
		}
	}

	private void BuildExcludedShopList(List<GameObject> excluded)
	{
		for (int i = 0; i < ItemsLootTable.defaultItemDrops.elements.Count; i++)
		{
			WeightedGameObject weightedGameObject = ItemsLootTable.defaultItemDrops.elements[i];
			if ((bool)weightedGameObject.gameObject)
			{
				PickupObject component = weightedGameObject.gameObject.GetComponent<PickupObject>();
				if ((bool)component && component.ShouldBeExcludedFromShops)
				{
					excluded.Add(weightedGameObject.gameObject);
				}
				else if ((bool)component && component.PickupObjectId == GlobalItemIds.UnfinishedGun && GameStatsManager.HasInstance && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_AMMONOMICON_COMPLETE))
				{
					excluded.Add(weightedGameObject.gameObject);
				}
			}
		}
	}

	public bool IsBossRewardForcedGun()
	{
		if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.BOSSRUSH)
		{
			bool flag = true;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if ((bool)GameManager.Instance.AllPlayers[i] && (GameManager.Instance.AllPlayers[i].HasReceivedNewGunThisFloor || GameManager.Instance.AllPlayers[i].CharacterUsesRandomGuns))
				{
					flag = false;
				}
			}
			if (flag)
			{
				Debug.LogWarning("Forcing boss drop GUN!");
				return true;
			}
		}
		return false;
	}

	public GameObject GetRewardObjectForBossSeeded(List<PickupObject> AlreadyGenerated, bool forceGun)
	{
		FloorRewardData currentRewardData = CurrentRewardData;
		bool flag = forceGun || BraveRandom.GenerationRandomValue() > ItemVsGunChanceBossReward;
		if (flag)
		{
			PickupObject.ItemQuality randomBossTargetQuality = currentRewardData.GetRandomBossTargetQuality(BraveRandom.GeneratorRandom);
			return GetItemForSeededRun(GunsLootTable, randomBossTargetQuality, AlreadyGenerated, BraveRandom.GeneratorRandom, true);
		}
		return GetItemForSeededRun((!flag) ? ItemsLootTable : GunsLootTable, GetDaveStyleItemQuality(), AlreadyGenerated, BraveRandom.GeneratorRandom, true);
	}

	public GameObject GetRewardObjectBossStyle(PlayerController player)
	{
		FloorRewardData currentRewardData = CurrentRewardData;
		bool flag = false;
		flag = ((GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON && GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER && (bool)player && player.inventory != null && player.inventory.GunCountModified <= 3) ? (UnityEngine.Random.value > 0.2f) : ((GameManager.Instance.CurrentGameType != 0 || !player || player.inventory == null || player.inventory.GunCountModified > 2) ? (UnityEngine.Random.value > ItemVsGunChanceBossReward) : (UnityEngine.Random.value > 0.3f)));
		if (IsBossRewardForcedGun())
		{
			flag = true;
		}
		if ((GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH) && !GameManager.Instance.Dungeon.HasGivenBossrushGun)
		{
			GameManager.Instance.Dungeon.HasGivenBossrushGun = true;
			flag = true;
		}
		if (flag)
		{
			PickupObject.ItemQuality randomBossTargetQuality = currentRewardData.GetRandomBossTargetQuality();
			return GetItemForPlayer(player, GunsLootTable, randomBossTargetQuality, null, false, null, false, null, false, RewardSource.BOSS_PEDESTAL);
		}
		return GetRewardItemDaveStyle(player, true);
	}

	private PickupObject.ItemQuality GetDaveStyleItemQuality()
	{
		float num = 0.1f;
		float num2 = 0.4f;
		float num3 = 0.7f;
		float num4 = 0.95f;
		float value = UnityEngine.Random.value;
		PickupObject.ItemQuality result = PickupObject.ItemQuality.D;
		if (value > num && value <= num2)
		{
			result = PickupObject.ItemQuality.C;
		}
		else if (value > num2 && value <= num3)
		{
			result = PickupObject.ItemQuality.B;
		}
		else if (value > num3 && value <= num4)
		{
			result = PickupObject.ItemQuality.A;
		}
		else if (value > num4)
		{
			result = PickupObject.ItemQuality.S;
		}
		return result;
	}

	private GameObject GetRewardItemDaveStyle(PlayerController player, bool bossStyle = false)
	{
		PickupObject.ItemQuality daveStyleItemQuality = GetDaveStyleItemQuality();
		Debug.Log("Get Reward Item Dave Style: " + daveStyleItemQuality);
		RewardSource rewardSource = (bossStyle ? RewardSource.BOSS_PEDESTAL : RewardSource.UNSPECIFIED);
		GenericLootTable itemsLootTable = ItemsLootTable;
		PickupObject.ItemQuality targetQuality = daveStyleItemQuality;
		List<GameObject> excludedObjects = null;
		bool bossStyle2 = bossStyle;
		RewardSource rewardSource2 = rewardSource;
		return GetItemForPlayer(player, itemsLootTable, targetQuality, excludedObjects, false, null, bossStyle2, null, false, rewardSource2);
	}

	public GameObject GetRewardObjectDaveStyle(PlayerController player)
	{
		FloorRewardData currentRewardData = CurrentRewardData;
		if (UnityEngine.Random.value > 0.5f)
		{
			PickupObject.ItemQuality randomTargetQuality = currentRewardData.GetRandomTargetQuality();
			return GetItemForPlayer(player, GunsLootTable, randomTargetQuality, null);
		}
		return GetRewardItemDaveStyle(player);
	}

	public static bool PlayerHasItemInSynergyContainingOtherItem(PlayerController player, PickupObject prefab)
	{
		bool usesStartingItem = false;
		return PlayerHasItemInSynergyContainingOtherItem(player, prefab, ref usesStartingItem);
	}

	public static bool TestItemWouldCompleteSpecificSynergy(AdvancedSynergyEntry entry, PickupObject newPickup)
	{
		if (entry.ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE)
		{
			return false;
		}
		if (!entry.SynergyIsAvailable(GameManager.Instance.PrimaryPlayer, GameManager.Instance.SecondaryPlayer))
		{
			return entry.SynergyIsAvailable(GameManager.Instance.PrimaryPlayer, GameManager.Instance.SecondaryPlayer, newPickup.PickupObjectId);
		}
		return false;
	}

	public static bool AnyPlayerHasItemInSynergyContainingOtherItem(PickupObject prefab, ref bool usesStartingItem)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && PlayerHasItemInSynergyContainingOtherItem(playerController, prefab, ref usesStartingItem))
			{
				return true;
			}
		}
		return false;
	}

	public static bool PlayerHasItemInSynergyContainingOtherItem(PlayerController player, PickupObject prefab, ref bool usesStartingItem)
	{
		int pickupObjectId = prefab.PickupObjectId;
		AdvancedSynergyEntry[] synergies = GameManager.Instance.SynergyManager.synergies;
		foreach (AdvancedSynergyEntry advancedSynergyEntry in synergies)
		{
			if (advancedSynergyEntry.ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE || advancedSynergyEntry.ActivationStatus == SynergyEntry.SynergyActivation.DEMO || advancedSynergyEntry.ActivationStatus == SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED || !advancedSynergyEntry.ContainsPickup(pickupObjectId))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < player.inventory.AllGuns.Count; j++)
			{
				bool flag2 = advancedSynergyEntry.ContainsPickup(player.inventory.AllGuns[j].PickupObjectId);
				if (flag2)
				{
					flag2 = TestItemWouldCompleteSpecificSynergy(advancedSynergyEntry, prefab);
				}
				flag = flag || flag2;
				if (flag2)
				{
					usesStartingItem |= player.startingGunIds.Contains(player.inventory.AllGuns[j].PickupObjectId);
				}
				if (flag2)
				{
					usesStartingItem |= player.startingAlternateGunIds.Contains(player.inventory.AllGuns[j].PickupObjectId);
				}
			}
			if (!flag)
			{
				for (int k = 0; k < player.activeItems.Count; k++)
				{
					bool flag3 = advancedSynergyEntry.ContainsPickup(player.activeItems[k].PickupObjectId);
					if (flag3)
					{
						flag3 = TestItemWouldCompleteSpecificSynergy(advancedSynergyEntry, prefab);
					}
					flag = flag || flag3;
					if (flag3)
					{
						usesStartingItem |= player.startingActiveItemIds.Contains(player.activeItems[k].PickupObjectId);
					}
				}
			}
			if (!flag)
			{
				for (int l = 0; l < player.passiveItems.Count; l++)
				{
					bool flag4 = advancedSynergyEntry.ContainsPickup(player.passiveItems[l].PickupObjectId);
					if (flag4)
					{
						flag4 = TestItemWouldCompleteSpecificSynergy(advancedSynergyEntry, prefab);
					}
					flag = flag || flag4;
					if (flag4)
					{
						usesStartingItem |= player.startingPassiveItemIds.Contains(player.passiveItems[l].PickupObjectId);
					}
				}
			}
			if (!flag && SynercacheManager.UseCachedSynergyIDs)
			{
				for (int m = 0; m < SynercacheManager.LastCachedSynergyIDs.Count; m++)
				{
					flag |= advancedSynergyEntry.ContainsPickup(SynercacheManager.LastCachedSynergyIDs[m]);
					flag |= advancedSynergyEntry.ContainsPickup(SynercacheManager.LastCachedSynergyIDs[m]);
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckQualityForItem(PickupObject prefab, PlayerController player, PickupObject.ItemQuality targetQuality, bool completesSynergy, RewardSource source)
	{
		bool flag = prefab.quality == targetQuality;
		if (!player)
		{
			return flag;
		}
		bool flag2 = completesSynergy || GameManager.Instance.RewardManager.SynergyCompletionIgnoresQualities;
		if (GameStatsManager.Instance.GetNumberOfSynergiesEncounteredThisRun() == 0 && source == RewardSource.BOSS_PEDESTAL)
		{
			flag2 = true;
		}
		if (!flag && flag2 && PlayerHasItemInSynergyContainingOtherItem(player, prefab))
		{
			flag = true;
		}
		return flag;
	}

	public static bool AnyPlayerHasItem(int id)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && (playerController.HasPassiveItem(id) || playerController.HasActiveItem(id) || playerController.HasGun(id)))
			{
				return true;
			}
		}
		return false;
	}

	public static float GetMultiplierForItem(PickupObject prefab, PlayerController player, bool completesSynergy)
	{
		if (!prefab)
		{
			return 1f;
		}
		float num = 1f;
		int pickupObjectId = prefab.PickupObjectId;
		if (player == null)
		{
			return num;
		}
		bool flag = false;
		float num2 = SynergyFactorConstants.GetSynergyFactor();
		if (completesSynergy)
		{
			if (AnyPlayerHasItem(prefab.PickupObjectId))
			{
				return 0f;
			}
			if (prefab is BasicStatPickup && (prefab as BasicStatPickup).IsMasteryToken)
			{
				return 0f;
			}
			num2 = 1E+08f;
		}
		if (num2 > 1f || flag)
		{
			bool usesStartingItem = false;
			if (AnyPlayerHasItemInSynergyContainingOtherItem(prefab, ref usesStartingItem))
			{
				if (completesSynergy && usesStartingItem)
				{
					num2 = 10000f;
				}
				else if (usesStartingItem)
				{
					num2 = 1f;
				}
				num *= num2;
			}
		}
		for (int i = 0; i < player.lootModData.Count; i++)
		{
			if (player.lootModData[i].AssociatedPickupId == pickupObjectId)
			{
				num *= player.lootModData[i].DropRateMultiplier;
			}
		}
		return num;
	}

	public GameObject GetRawItem(GenericLootTable tableToUse, PickupObject.ItemQuality targetQuality, List<GameObject> excludedObjects, bool ignorePlayerTraits = false, System.Random safeRandom = null)
	{
		bool flag = false;
		while (targetQuality >= PickupObject.ItemQuality.COMMON)
		{
			if (targetQuality > PickupObject.ItemQuality.COMMON)
			{
				flag = true;
			}
			List<WeightedGameObject> compiledRawItems = tableToUse.GetCompiledRawItems();
			List<KeyValuePair<WeightedGameObject, float>> list = new List<KeyValuePair<WeightedGameObject, float>>();
			float num = 0f;
			for (int i = 0; i < compiledRawItems.Count; i++)
			{
				if (!(compiledRawItems[i].gameObject != null))
				{
					continue;
				}
				PickupObject component = compiledRawItems[i].gameObject.GetComponent<PickupObject>();
				if (component == null)
				{
					continue;
				}
				bool flag2 = component.quality == targetQuality;
				if (!(component != null) || !flag2)
				{
					continue;
				}
				bool flag3 = true;
				float weight = compiledRawItems[i].weight;
				if (excludedObjects != null && excludedObjects.Contains(component.gameObject))
				{
					flag3 = false;
					continue;
				}
				if (!component.PrerequisitesMet())
				{
					flag3 = false;
				}
				if (flag3)
				{
					num += weight;
					KeyValuePair<WeightedGameObject, float> item = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], weight);
					list.Add(item);
				}
			}
			if (num > 0f && list.Count > 0)
			{
				float num2 = 0f;
				if (ignorePlayerTraits)
				{
					float num3 = (float)safeRandom.NextDouble();
					num2 = num * num3;
				}
				else
				{
					num2 = num * UnityEngine.Random.value;
				}
				for (int j = 0; j < list.Count; j++)
				{
					num2 -= list[j].Value;
					if (num2 <= 0f)
					{
						return list[j].Key.gameObject;
					}
				}
				return list[list.Count - 1].Key.gameObject;
			}
			targetQuality--;
			if (targetQuality < PickupObject.ItemQuality.COMMON && !flag)
			{
				targetQuality = PickupObject.ItemQuality.D;
			}
		}
		return null;
	}

	public GameObject GetItemForPlayer(PlayerController player, GenericLootTable tableToUse, PickupObject.ItemQuality targetQuality, List<GameObject> excludedObjects, bool ignorePlayerTraits = false, System.Random safeRandom = null, bool bossStyle = false, List<GameObject> additionalExcludedObjects = null, bool forceSynergyCompletion = false, RewardSource rewardSource = RewardSource.UNSPECIFIED)
	{
		bool flag = false;
		while (targetQuality >= PickupObject.ItemQuality.COMMON)
		{
			if (targetQuality > PickupObject.ItemQuality.COMMON)
			{
				flag = true;
			}
			List<WeightedGameObject> compiledRawItems = tableToUse.GetCompiledRawItems();
			List<KeyValuePair<WeightedGameObject, float>> list = new List<KeyValuePair<WeightedGameObject, float>>();
			float num = 0f;
			List<KeyValuePair<WeightedGameObject, float>> list2 = new List<KeyValuePair<WeightedGameObject, float>>();
			float num2 = 0f;
			for (int i = 0; i < compiledRawItems.Count; i++)
			{
				if (!(compiledRawItems[i].gameObject != null))
				{
					continue;
				}
				PickupObject component = compiledRawItems[i].gameObject.GetComponent<PickupObject>();
				if (component == null || (bossStyle && component is GungeonMapItem))
				{
					continue;
				}
				bool flag2 = CheckQualityForItem(component, player, targetQuality, forceSynergyCompletion, rewardSource);
				if ((component.ItemSpansBaseQualityTiers || component.ItemRespectsHeartMagnificence) && targetQuality != PickupObject.ItemQuality.D && targetQuality != 0 && targetQuality != PickupObject.ItemQuality.S)
				{
					flag2 = true;
				}
				if (!ignorePlayerTraits && component is SpiceItem && (bool)player && player.spiceCount > 0)
				{
					Debug.Log("BAM spicing it up");
					flag2 = true;
				}
				if (!(component != null) || !flag2)
				{
					continue;
				}
				bool flag3 = true;
				float num3 = compiledRawItems[i].weight;
				if (excludedObjects != null && excludedObjects.Contains(component.gameObject))
				{
					flag3 = false;
					continue;
				}
				if (additionalExcludedObjects != null && additionalExcludedObjects.Contains(component.gameObject))
				{
					flag3 = false;
					continue;
				}
				if (!component.PrerequisitesMet())
				{
					flag3 = false;
				}
				if (component is Gun)
				{
					Gun gun = component as Gun;
					if (gun.InfiniteAmmo && !gun.CanBeDropped && gun.quality == PickupObject.ItemQuality.SPECIAL)
					{
						flag3 = false;
						continue;
					}
					GunClass gunClass = gun.gunClass;
					if (!ignorePlayerTraits && gunClass != 0)
					{
						int num4 = ((!(player == null) && player.inventory != null) ? player.inventory.ContainsGunOfClass(gunClass, true) : 0);
						float modifierForClass = LootDataGlobalSettings.Instance.GetModifierForClass(gunClass);
						num3 *= Mathf.Pow(modifierForClass, num4);
					}
				}
				if (!ignorePlayerTraits)
				{
					float multiplierForItem = GetMultiplierForItem(component, player, forceSynergyCompletion);
					num3 *= multiplierForItem;
				}
				bool flag4 = !GameManager.Instance.IsSeeded;
				EncounterTrackable component2 = component.GetComponent<EncounterTrackable>();
				if (component2 != null && flag4)
				{
					int num5 = 0;
					if (Application.isPlaying)
					{
						num5 = GameStatsManager.Instance.QueryEncounterableDifferentiator(component2);
					}
					if (num5 > 0 || (Application.isPlaying && GameManager.Instance.ExtantShopTrackableGuids.Contains(component2.EncounterGuid)))
					{
						flag3 = false;
						num2 += num3;
						KeyValuePair<WeightedGameObject, float> item = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], num3);
						list2.Add(item);
					}
					else if (Application.isPlaying && GameStatsManager.Instance.QueryEncounterable(component2) == 0 && GameStatsManager.Instance.QueryEncounterableAnnouncement(component2.EncounterGuid))
					{
						num3 *= 10f;
					}
				}
				if (component.ItemSpansBaseQualityTiers || component.ItemRespectsHeartMagnificence)
				{
					if (AdditionalHeartTierMagnificence >= 3f)
					{
						num3 *= ThreeOrMoreHeartMagMultiplier;
					}
					else if (AdditionalHeartTierMagnificence >= 1f)
					{
						num3 *= OneOrTwoHeartMagMultiplier;
					}
				}
				if (flag3)
				{
					num += num3;
					KeyValuePair<WeightedGameObject, float> item2 = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], num3);
					list.Add(item2);
				}
			}
			if (list.Count == 0 && list2.Count > 0)
			{
				list = list2;
				num = num2;
			}
			if (num > 0f && list.Count > 0)
			{
				float num6 = 0f;
				if (ignorePlayerTraits)
				{
					float num7 = (float)safeRandom.NextDouble();
					Debug.LogError("safe random: " + num7);
					num6 = num * num7;
				}
				else
				{
					num6 = num * UnityEngine.Random.value;
				}
				for (int j = 0; j < list.Count; j++)
				{
					num6 -= list[j].Value;
					if (num6 <= 0f)
					{
						return list[j].Key.gameObject;
					}
				}
				return list[list.Count - 1].Key.gameObject;
			}
			targetQuality--;
			if (targetQuality < PickupObject.ItemQuality.COMMON && !flag)
			{
				targetQuality = PickupObject.ItemQuality.D;
			}
		}
		return null;
	}

	public GameObject GetItemForSeededRun(GenericLootTable tableToUse, PickupObject.ItemQuality targetQuality, List<PickupObject> AlreadyGeneratedItems, System.Random safeRandom, bool bossStyle = false)
	{
		bool flag = false;
		while (targetQuality >= PickupObject.ItemQuality.COMMON)
		{
			if (targetQuality > PickupObject.ItemQuality.COMMON)
			{
				flag = true;
			}
			List<WeightedGameObject> compiledRawItems = tableToUse.GetCompiledRawItems();
			List<KeyValuePair<WeightedGameObject, float>> list = new List<KeyValuePair<WeightedGameObject, float>>();
			float num = 0f;
			for (int i = 0; i < compiledRawItems.Count; i++)
			{
				if (!(compiledRawItems[i].gameObject != null))
				{
					continue;
				}
				PickupObject component = compiledRawItems[i].gameObject.GetComponent<PickupObject>();
				if (component == null || (bossStyle && component is GungeonMapItem))
				{
					continue;
				}
				bool flag2 = component.quality == targetQuality;
				if ((component.ItemSpansBaseQualityTiers || component.ItemRespectsHeartMagnificence) && targetQuality != PickupObject.ItemQuality.D && targetQuality != 0 && targetQuality != PickupObject.ItemQuality.S)
				{
					flag2 = true;
				}
				if (!(component != null) || !flag2)
				{
					continue;
				}
				bool flag3 = true;
				float weight = compiledRawItems[i].weight;
				if (AlreadyGeneratedItems != null && AlreadyGeneratedItems.Contains(component))
				{
					flag3 = false;
					continue;
				}
				if (!component.PrerequisitesMet())
				{
					flag3 = false;
				}
				if (component is Gun)
				{
					Gun gun = component as Gun;
					if (gun.InfiniteAmmo && !gun.CanBeDropped && gun.quality == PickupObject.ItemQuality.SPECIAL)
					{
						flag3 = false;
						continue;
					}
				}
				if (GameManager.Instance.RewardManager.IsItemInSeededManifests(component))
				{
					continue;
				}
				float num2 = 1f;
				if (AlreadyGeneratedItems != null)
				{
					for (int j = 0; j < AlreadyGeneratedItems.Count; j++)
					{
						for (int k = 0; k < AlreadyGeneratedItems[j].associatedItemChanceMods.Length; k++)
						{
							if (AlreadyGeneratedItems[j].associatedItemChanceMods[k].AssociatedPickupId == component.PickupObjectId)
							{
								num2 *= AlreadyGeneratedItems[j].associatedItemChanceMods[k].DropRateMultiplier;
							}
						}
					}
				}
				weight *= num2;
				if (component.ItemSpansBaseQualityTiers || component.ItemRespectsHeartMagnificence)
				{
					if (AdditionalHeartTierMagnificence >= 3f)
					{
						weight *= ThreeOrMoreHeartMagMultiplier;
					}
					else if (AdditionalHeartTierMagnificence >= 1f)
					{
						weight *= OneOrTwoHeartMagMultiplier;
					}
				}
				if (flag3)
				{
					num += weight;
					KeyValuePair<WeightedGameObject, float> item = new KeyValuePair<WeightedGameObject, float>(compiledRawItems[i], weight);
					list.Add(item);
				}
			}
			if (num > 0f && list.Count > 0)
			{
				float num3 = (float)safeRandom.NextDouble();
				float num4 = num * num3;
				for (int l = 0; l < list.Count; l++)
				{
					num4 -= list[l].Value;
					if (num4 <= 0f)
					{
						return list[l].Key.gameObject;
					}
				}
				return list[list.Count - 1].Key.gameObject;
			}
			targetQuality--;
			if (targetQuality < PickupObject.ItemQuality.COMMON && !flag)
			{
				targetQuality = PickupObject.ItemQuality.D;
			}
		}
		return null;
	}

	private Chest GetTargetChestPrefab(PickupObject.ItemQuality targetQuality)
	{
		Chest result = null;
		switch (targetQuality)
		{
		case PickupObject.ItemQuality.D:
			result = D_Chest;
			break;
		case PickupObject.ItemQuality.C:
			result = C_Chest;
			break;
		case PickupObject.ItemQuality.B:
			result = B_Chest;
			break;
		case PickupObject.ItemQuality.A:
			result = A_Chest;
			break;
		case PickupObject.ItemQuality.S:
			result = S_Chest;
			break;
		}
		return result;
	}

	private Chest SpawnInternal(IntVector2 position, float gunVersusItemPercentChance, PickupObject.ItemQuality targetQuality, Chest overrideChestPrefab = null)
	{
		Chest chestPrefab = overrideChestPrefab ?? GetTargetChestPrefab(targetQuality);
		GenericLootTable genericLootTable = ((!(UnityEngine.Random.value < gunVersusItemPercentChance)) ? ItemsLootTable : GunsLootTable);
		Chest chest = Chest.Spawn(chestPrefab, position);
		chest.lootTable.lootTable = genericLootTable;
		if (chest.lootTable.canDropMultipleItems && chest.lootTable.overrideItemLootTables != null && chest.lootTable.overrideItemLootTables.Count > 0)
		{
			chest.lootTable.overrideItemLootTables[0] = genericLootTable;
		}
		return chest;
	}

	public Chest SpawnRoomClearChestAt(IntVector2 position)
	{
		FloorRewardData rewardDataForFloor = GetRewardDataForFloor(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId);
		PickupObject.ItemQuality randomRoomTargetQuality = rewardDataForFloor.GetRandomRoomTargetQuality();
		int count = -1;
		if ((randomRoomTargetQuality == PickupObject.ItemQuality.D || randomRoomTargetQuality == PickupObject.ItemQuality.C) && PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.DOUBLE_CHEST_FRIENDS, out count))
		{
			randomRoomTargetQuality = rewardDataForFloor.GetRandomRoomTargetQuality();
		}
		Chest overrideChestPrefab = null;
		if (UnityEngine.Random.value < RoomClearRainbowChance)
		{
			overrideChestPrefab = Rainbow_Chest;
		}
		return SpawnInternal(position, rewardDataForFloor.GunVersusItemPercentChance, randomRoomTargetQuality, overrideChestPrefab);
	}

	public DebrisObject SpawnTotallyRandomItem(Vector2 position, PickupObject.ItemQuality startQuality = PickupObject.ItemQuality.D, PickupObject.ItemQuality endQuality = PickupObject.ItemQuality.S)
	{
		PickupObject.ItemQuality targetQuality = (PickupObject.ItemQuality)UnityEngine.Random.Range((int)startQuality, (int)(endQuality + 1));
		return LootEngine.SpawnItem(GetItemForPlayer(GameManager.Instance.PrimaryPlayer, (!(UnityEngine.Random.value < 0.5f)) ? ItemsLootTable : GunsLootTable, targetQuality, null).gameObject, position, Vector2.zero, 0f);
	}

	public Chest SpawnTotallyRandomChest(IntVector2 position)
	{
		PickupObject.ItemQuality targetQuality = (PickupObject.ItemQuality)UnityEngine.Random.Range(1, 6);
		if (PassiveItem.IsFlagSetAtAll(typeof(SevenLeafCloverItem)))
		{
			targetQuality = ((!(UnityEngine.Random.value < 0.5f)) ? PickupObject.ItemQuality.S : PickupObject.ItemQuality.A);
		}
		return SpawnInternal(position, 0.5f, targetQuality);
	}

	public PickupObject.ItemQuality GetQualityFromChest(Chest c)
	{
		if (CompareChest(c, D_Chest))
		{
			return PickupObject.ItemQuality.D;
		}
		if (CompareChest(c, C_Chest))
		{
			return PickupObject.ItemQuality.C;
		}
		if (CompareChest(c, B_Chest))
		{
			return PickupObject.ItemQuality.B;
		}
		if (CompareChest(c, A_Chest))
		{
			return PickupObject.ItemQuality.A;
		}
		if (CompareChest(c, S_Chest))
		{
			return PickupObject.ItemQuality.S;
		}
		return PickupObject.ItemQuality.EXCLUDED;
	}

	private bool CompareChest(Chest c1, Chest c2)
	{
		return c1.lootTable.D_Chance == c2.lootTable.D_Chance && c1.lootTable.C_Chance == c2.lootTable.C_Chance && c1.lootTable.B_Chance == c2.lootTable.B_Chance && c1.lootTable.A_Chance == c2.lootTable.A_Chance && c1.lootTable.S_Chance == c2.lootTable.S_Chance;
	}

	public Chest SpawnRewardChestAt(IntVector2 position, float overrideGunVsItemChance = -1f, PickupObject.ItemQuality excludedQuality = PickupObject.ItemQuality.EXCLUDED)
	{
		FloorRewardData rewardDataForFloor = GetRewardDataForFloor(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId);
		PickupObject.ItemQuality targetQuality = rewardDataForFloor.GetRandomTargetQuality();
		if (PassiveItem.IsFlagSetAtAll(typeof(SevenLeafCloverItem)))
		{
			targetQuality = ((!(UnityEngine.Random.value < 0.5f)) ? PickupObject.ItemQuality.S : PickupObject.ItemQuality.A);
		}
		return SpawnInternal(position, (!(overrideGunVsItemChance >= 0f)) ? rewardDataForFloor.GunVersusItemPercentChance : overrideGunVsItemChance, targetQuality);
	}

	public Chest GenerationSpawnRewardChestAt(IntVector2 positionInRoom, RoomHandler targetRoom, PickupObject.ItemQuality? targetQuality = null, float overrideMimicChance = -1f)
	{
		System.Random random = ((!GameManager.Instance.IsSeeded) ? null : BraveRandom.GeneratorRandom);
		FloorRewardData rewardDataForFloor = GetRewardDataForFloor(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId);
		bool forceDChanceZero = StaticReferenceManager.DChestsSpawnedInTotal >= 2;
		if (!targetQuality.HasValue)
		{
			targetQuality = rewardDataForFloor.GetRandomTargetQuality(true, forceDChanceZero);
			if (PassiveItem.IsFlagSetAtAll(typeof(SevenLeafCloverItem)))
			{
				targetQuality = ((!(((random == null) ? UnityEngine.Random.value : ((float)random.NextDouble())) < 0.5f)) ? PickupObject.ItemQuality.S : PickupObject.ItemQuality.A);
			}
		}
		if (targetQuality.GetValueOrDefault() == PickupObject.ItemQuality.D && targetQuality.HasValue && StaticReferenceManager.DChestsSpawnedOnFloor >= 1 && GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON)
		{
			targetQuality = PickupObject.ItemQuality.C;
		}
		Vector2 vector = Vector2.zero;
		if (targetQuality == PickupObject.ItemQuality.A || targetQuality == PickupObject.ItemQuality.S)
		{
			vector = new Vector2(-0.5f, 0f);
		}
		Chest chest = GetTargetChestPrefab(targetQuality.Value);
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.SYNERGRACE_UNLOCKED) && GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.CASTLEGEON)
		{
			float num = ((random == null) ? UnityEngine.Random.value : ((float)random.NextDouble()));
			if (num < GlobalSynerchestChance)
			{
				chest = Synergy_Chest;
				vector = new Vector2(-0.1875f, 0f);
			}
		}
		Chest.GeneralChestType generalChestType = ((BraveRandom.GenerationRandomValue() < rewardDataForFloor.GunVersusItemPercentChance) ? Chest.GeneralChestType.WEAPON : Chest.GeneralChestType.ITEM);
		if (StaticReferenceManager.ItemChestsSpawnedOnFloor > 0 && StaticReferenceManager.WeaponChestsSpawnedOnFloor == 0)
		{
			generalChestType = Chest.GeneralChestType.WEAPON;
		}
		else if (StaticReferenceManager.WeaponChestsSpawnedOnFloor > 0 && StaticReferenceManager.ItemChestsSpawnedOnFloor == 0)
		{
			generalChestType = Chest.GeneralChestType.ITEM;
		}
		GenericLootTable genericLootTable = ((generalChestType != Chest.GeneralChestType.WEAPON) ? ItemsLootTable : GunsLootTable);
		GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(chest.gameObject, targetRoom, positionInRoom, true);
		gameObject.transform.position = gameObject.transform.position + vector.ToVector3ZUp();
		Chest component = gameObject.GetComponent<Chest>();
		if (overrideMimicChance >= 0f)
		{
			component.overrideMimicChance = overrideMimicChance;
		}
		Component[] componentsInChildren = gameObject.GetComponentsInChildren(typeof(IPlaceConfigurable));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			IPlaceConfigurable placeConfigurable = componentsInChildren[i] as IPlaceConfigurable;
			if (placeConfigurable != null)
			{
				placeConfigurable.ConfigureOnPlacement(targetRoom);
			}
		}
		if (targetQuality == PickupObject.ItemQuality.A)
		{
			GameManager.Instance.Dungeon.GeneratedMagnificence += 1f;
			component.GeneratedMagnificence += 1f;
		}
		else if (targetQuality == PickupObject.ItemQuality.S)
		{
			GameManager.Instance.Dungeon.GeneratedMagnificence += 1f;
			component.GeneratedMagnificence += 1f;
		}
		if ((bool)component.specRigidbody)
		{
			component.specRigidbody.Reinitialize();
		}
		component.ChestType = generalChestType;
		component.lootTable.lootTable = genericLootTable;
		if (component.lootTable.canDropMultipleItems && component.lootTable.overrideItemLootTables != null && component.lootTable.overrideItemLootTables.Count > 0)
		{
			component.lootTable.overrideItemLootTables[0] = genericLootTable;
		}
		if (targetQuality.GetValueOrDefault() == PickupObject.ItemQuality.D && targetQuality.HasValue && !component.IsMimic)
		{
			StaticReferenceManager.DChestsSpawnedOnFloor++;
			StaticReferenceManager.DChestsSpawnedInTotal++;
			component.IsLocked = true;
			if ((bool)component.LockAnimator)
			{
				component.LockAnimator.renderer.enabled = true;
			}
		}
		targetRoom.RegisterInteractable(component);
		if (SeededRunManifests.ContainsKey(GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId))
		{
			component.GenerationDetermineContents(SeededRunManifests[GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId], random);
		}
		return component;
	}

	public bool IsItemInSeededManifests(PickupObject testItem)
	{
		foreach (KeyValuePair<GlobalDungeonData.ValidTilesets, FloorRewardManifest> seededRunManifest in SeededRunManifests)
		{
			FloorRewardManifest value = seededRunManifest.Value;
			if (value.CheckManifestDifferentiator(testItem))
			{
				return true;
			}
		}
		return false;
	}

	public FloorRewardManifest GetSeededManifestForCurrentFloor()
	{
		GlobalDungeonData.ValidTilesets tilesetId = GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId;
		if (SeededRunManifests != null && SeededRunManifests.ContainsKey(tilesetId))
		{
			return SeededRunManifests[tilesetId];
		}
		return null;
	}

	public void CopyBossGunChancesFromTierZero(int targetTier)
	{
		FloorRewardData[targetTier].D_BossGun_Chance = FloorRewardData[0].D_BossGun_Chance;
		FloorRewardData[targetTier].C_BossGun_Chance = FloorRewardData[0].C_BossGun_Chance;
		FloorRewardData[targetTier].B_BossGun_Chance = FloorRewardData[0].B_BossGun_Chance;
		FloorRewardData[targetTier].A_BossGun_Chance = FloorRewardData[0].A_BossGun_Chance;
		FloorRewardData[targetTier].S_BossGun_Chance = FloorRewardData[0].S_BossGun_Chance;
	}

	public void CopyShopChancesFromTierZero(int targetTier)
	{
		FloorRewardData[targetTier].D_Shop_Chance = FloorRewardData[0].D_Shop_Chance;
		FloorRewardData[targetTier].C_Shop_Chance = FloorRewardData[0].C_Shop_Chance;
		FloorRewardData[targetTier].B_Shop_Chance = FloorRewardData[0].B_Shop_Chance;
		FloorRewardData[targetTier].A_Shop_Chance = FloorRewardData[0].A_Shop_Chance;
		FloorRewardData[targetTier].S_Shop_Chance = FloorRewardData[0].S_Shop_Chance;
	}

	public void CopyChestChancesFromTierZero(int targetTier)
	{
		FloorRewardData[targetTier].D_Chest_Chance = FloorRewardData[0].D_Chest_Chance;
		FloorRewardData[targetTier].C_Chest_Chance = FloorRewardData[0].C_Chest_Chance;
		FloorRewardData[targetTier].B_Chest_Chance = FloorRewardData[0].B_Chest_Chance;
		FloorRewardData[targetTier].A_Chest_Chance = FloorRewardData[0].A_Chest_Chance;
		FloorRewardData[targetTier].S_Chest_Chance = FloorRewardData[0].S_Chest_Chance;
	}

	public void CopyDropChestChancesFromTierZero(int targetTier)
	{
		FloorRewardData[targetTier].D_RoomChest_Chance = FloorRewardData[0].D_RoomChest_Chance;
		FloorRewardData[targetTier].C_RoomChest_Chance = FloorRewardData[0].C_RoomChest_Chance;
		FloorRewardData[targetTier].B_RoomChest_Chance = FloorRewardData[0].B_RoomChest_Chance;
		FloorRewardData[targetTier].A_RoomChest_Chance = FloorRewardData[0].A_RoomChest_Chance;
		FloorRewardData[targetTier].S_RoomChest_Chance = FloorRewardData[0].S_RoomChest_Chance;
	}

	public void CopyTertiaryBossSpawnsFromTierZero(int targetTier)
	{
		FloorRewardData[targetTier].TertiaryBossRewardSets = new List<TertiaryBossRewardSet>(FloorRewardData[0].TertiaryBossRewardSets);
	}
}
