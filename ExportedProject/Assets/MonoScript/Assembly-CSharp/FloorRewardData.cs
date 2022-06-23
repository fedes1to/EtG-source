using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FloorRewardData
{
	public string Annotation;

	[EnumFlags]
	public GlobalDungeonData.ValidTilesets AssociatedTilesets;

	[Header("Currency Drops")]
	public float AverageCurrencyDropsThisFloor = 60f;

	public float CurrencyDropsStandardDeviation = 15f;

	public float MinimumCurrencyDropsThisFloor = 40f;

	[RewardManagerReset("Chest Type Chances", "Copy From Tier 0", "CopyChestChancesFromTierZero", 0)]
	public float D_Chest_Chance = 0.2f;

	public float C_Chest_Chance = 0.2f;

	public float B_Chest_Chance = 0.2f;

	public float A_Chest_Chance = 0.2f;

	public float S_Chest_Chance = 0.2f;

	[Header("Global Drops")]
	public float ChestSystem_ChestChanceLowerBound = 0.01f;

	public float ChestSystem_ChestChanceUpperBound = 0.2f;

	public float ChestSystem_Increment = 0.03f;

	[Space(3f)]
	public float GunVersusItemPercentChance = 0.5f;

	[Space(3f)]
	public float PercentOfRoomClearRewardsThatAreChests = 0.2f;

	public GenericLootTable SingleItemRewardTable;

	[Space(3f)]
	public float FloorChanceToDropAmmo = 0.0625f;

	public float FloorChanceForSpreadAmmo = 0.5f;

	[RewardManagerReset("Global Drop Type Chances", "Copy From Tier 0", "CopyDropChestChancesFromTierZero", 2)]
	public float D_RoomChest_Chance = 0.2f;

	public float C_RoomChest_Chance = 0.2f;

	public float B_RoomChest_Chance = 0.2f;

	public float A_RoomChest_Chance = 0.2f;

	public float S_RoomChest_Chance = 0.2f;

	[RewardManagerReset("Boss Gun Qualities", "Copy From Tier 0", "CopyBossGunChancesFromTierZero", 0)]
	public float D_BossGun_Chance = 0.1f;

	public float C_BossGun_Chance = 0.3f;

	public float B_BossGun_Chance = 0.3f;

	public float A_BossGun_Chance = 0.2f;

	public float S_BossGun_Chance = 0.1f;

	[RewardManagerReset("Shop Gun/Item Qualities", "Copy From Tier 0", "CopyShopChancesFromTierZero", 0)]
	public float D_Shop_Chance = 0.1f;

	public float C_Shop_Chance = 0.3f;

	public float B_Shop_Chance = 0.3f;

	public float A_Shop_Chance = 0.2f;

	public float S_Shop_Chance = 0.1f;

	public float ReplaceFirstRewardWithPickup = 0.2f;

	[Header("Meta Currency")]
	public int MinMetaCurrencyFromBoss;

	public int MaxMetaCurrencyFromBoss;

	public bool AlternateItemChestChances;

	[ShowInInspectorIf("AlternateItemChestChances", false)]
	public float D_Item_Chest_Chance = 0.2f;

	[ShowInInspectorIf("AlternateItemChestChances", false)]
	public float C_Item_Chest_Chance = 0.2f;

	[ShowInInspectorIf("AlternateItemChestChances", false)]
	public float B_Item_Chest_Chance = 0.2f;

	[ShowInInspectorIf("AlternateItemChestChances", false)]
	public float A_Item_Chest_Chance = 0.2f;

	[ShowInInspectorIf("AlternateItemChestChances", false)]
	public float S_Item_Chest_Chance = 0.2f;

	[RewardManagerReset("For Bosses", "Copy From Tier 0", "CopyTertiaryBossSpawnsFromTierZero", 1)]
	public GenericLootTable FallbackBossLootTable;

	public List<TertiaryBossRewardSet> TertiaryBossRewardSets;

	public float SumChances()
	{
		return D_Chest_Chance + C_Chest_Chance + B_Chest_Chance + A_Chest_Chance + S_Chest_Chance;
	}

	public float SumRoomChances()
	{
		return D_RoomChest_Chance + C_RoomChest_Chance + B_RoomChest_Chance + A_RoomChest_Chance + S_RoomChest_Chance;
	}

	public float SumBossGunChances()
	{
		return D_BossGun_Chance + C_BossGun_Chance + B_BossGun_Chance + A_BossGun_Chance + S_BossGun_Chance;
	}

	public float SumShopChances()
	{
		return D_Shop_Chance + C_Shop_Chance + B_Shop_Chance + A_Shop_Chance + S_Shop_Chance;
	}

	public float DetermineCurrentMagnificence(bool isGenerationForMagnificence = false)
	{
		float num = 0f;
		if (GameManager.Instance.PrimaryPlayer != null)
		{
			num += GameManager.Instance.PrimaryPlayer.stats.Magnificence;
		}
		if (GameManager.Instance.Dungeon != null)
		{
			num = ((!isGenerationForMagnificence) ? (num + GameManager.Instance.Dungeon.GeneratedMagnificence) : (num + GameManager.Instance.Dungeon.GeneratedMagnificence * 2f));
		}
		return num;
	}

	public PickupObject.ItemQuality GetTargetQualityFromChances(float fran, float dChance, float cChance, float bChance, float aChance, float sChance, bool isGenerationForMagnificence = false)
	{
		float currentMagnificence = DetermineCurrentMagnificence(isGenerationForMagnificence);
		if (fran < dChance)
		{
			return MagnificenceConstants.ModifyQualityByMagnificence(PickupObject.ItemQuality.D, currentMagnificence, dChance, cChance, bChance);
		}
		if (fran < dChance + cChance)
		{
			return MagnificenceConstants.ModifyQualityByMagnificence(PickupObject.ItemQuality.C, currentMagnificence, dChance, cChance, bChance);
		}
		if (fran < dChance + cChance + bChance)
		{
			return MagnificenceConstants.ModifyQualityByMagnificence(PickupObject.ItemQuality.B, currentMagnificence, dChance, cChance, bChance);
		}
		if (fran < dChance + cChance + bChance + aChance)
		{
			return MagnificenceConstants.ModifyQualityByMagnificence(PickupObject.ItemQuality.A, currentMagnificence, dChance, cChance, bChance);
		}
		return MagnificenceConstants.ModifyQualityByMagnificence(PickupObject.ItemQuality.S, currentMagnificence, dChance, cChance, bChance);
	}

	public PickupObject.ItemQuality GetShopTargetQuality(bool useSeedRandom = false)
	{
		float num = SumShopChances();
		float fran = ((!useSeedRandom) ? UnityEngine.Random.value : BraveRandom.GenerationRandomValue()) * num;
		return GetTargetQualityFromChances(fran, D_Shop_Chance, C_Shop_Chance, B_Shop_Chance, A_Shop_Chance, S_Shop_Chance);
	}

	public PickupObject.ItemQuality GetRandomBossTargetQuality(System.Random safeRandom = null)
	{
		float num = SumBossGunChances();
		float fran = ((safeRandom == null) ? UnityEngine.Random.value : ((float)safeRandom.NextDouble())) * num;
		PickupObject.ItemQuality targetQualityFromChances = GetTargetQualityFromChances(fran, D_BossGun_Chance, C_BossGun_Chance, B_BossGun_Chance, A_BossGun_Chance, S_BossGun_Chance);
		Debug.Log(string.Concat(targetQualityFromChances, " <= boss quality"));
		return targetQualityFromChances;
	}

	public PickupObject.ItemQuality GetRandomTargetQuality(bool isGenerationForMagnificence = false, bool forceDChanceZero = false)
	{
		float num = ((!forceDChanceZero) ? SumChances() : (C_Chest_Chance + B_Chest_Chance + A_Chest_Chance + S_Chest_Chance));
		float num2 = 0f;
		num2 = ((!isGenerationForMagnificence) ? (UnityEngine.Random.value * num) : (BraveRandom.GenerationRandomValue() * num));
		return GetTargetQualityFromChances(num2, (!forceDChanceZero) ? D_Chest_Chance : 0f, C_Chest_Chance, B_Chest_Chance, A_Chest_Chance, S_Chest_Chance, isGenerationForMagnificence);
	}

	public PickupObject.ItemQuality GetRandomRoomTargetQuality()
	{
		float num = SumRoomChances();
		float fran = UnityEngine.Random.value * num;
		float num2 = D_RoomChest_Chance;
		float num3 = C_RoomChest_Chance;
		float num4 = B_RoomChest_Chance;
		if (PassiveItem.IsFlagSetAtAll(typeof(AmazingChestAheadItem)))
		{
			float num5 = num2 / 2f;
			num2 -= num5;
			num3 += num5 / 2f;
			num4 += num5 / 2f;
		}
		return GetTargetQualityFromChances(fran, num2, num3, num4, A_RoomChest_Chance, S_RoomChest_Chance);
	}
}
