using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShrineBenefit
{
	public enum BenefitType
	{
		MONEY,
		HEALTH,
		ARMOR,
		BLANK,
		KEY,
		AMMO_PERCENTAGE,
		STATS,
		CLEANSE_CURSE,
		SPAWN_CHEST,
		SPECIFIC_ITEM,
		COMPANION,
		BLOODTHIRST
	}

	public BenefitType benefitType;

	public float amount;

	[ShowInInspectorIf("benefitType", 5, false)]
	public bool appliesToAllGuns;

	public StatModifier[] statMods;

	public string rngString;

	public float rngWeight = 1f;

	[PickupIdentifier]
	public int targetItemID;

	[PickupIdentifier]
	public int TurkeyCompanionForCompanionShrine;

	[NonSerialized]
	public bool IsRNGChest;

	public void ApplyBenefit(PlayerController interactor)
	{
		int num = Mathf.RoundToInt(amount);
		switch (benefitType)
		{
		case BenefitType.MONEY:
			interactor.carriedConsumables.Currency += num;
			break;
		case BenefitType.HEALTH:
			if (interactor.healthHaver.GetCurrentHealthPercentage() >= 1f)
			{
				interactor.Blanks++;
			}
			else
			{
				interactor.healthHaver.ApplyHealing(amount);
			}
			break;
		case BenefitType.ARMOR:
			interactor.healthHaver.Armor += amount;
			break;
		case BenefitType.BLANK:
			interactor.Blanks += num;
			break;
		case BenefitType.KEY:
			interactor.carriedConsumables.KeyBullets += num;
			break;
		case BenefitType.AMMO_PERCENTAGE:
			interactor.ResetTarnisherClipCapacity();
			if (appliesToAllGuns)
			{
				for (int j = 0; j < interactor.inventory.AllGuns.Count; j++)
				{
					if (interactor.inventory.AllGuns[j].CanGainAmmo)
					{
						int num2 = Mathf.FloorToInt((float)interactor.inventory.AllGuns[j].AdjustedMaxAmmo * amount);
						if (num2 <= 0)
						{
							AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", interactor.gameObject);
							num2 = Mathf.FloorToInt((float)interactor.inventory.AllGuns[j].ammo * amount);
						}
						if (num2 <= 0)
						{
							Debug.LogError("Shrine is attempting to give negative ammo!");
							num2 = 1;
						}
						interactor.inventory.AllGuns[j].GainAmmo(num2);
					}
				}
			}
			else if (interactor.inventory.CurrentGun != null && interactor.inventory.CurrentGun.CanGainAmmo)
			{
				int num3 = Mathf.FloorToInt((float)interactor.inventory.CurrentGun.AdjustedMaxAmmo * amount);
				if (num3 <= 0)
				{
					AkSoundEngine.PostEvent("Play_OBJ_ammo_pickup_01", interactor.gameObject);
					num3 = Mathf.FloorToInt((float)interactor.inventory.CurrentGun.ammo * amount);
				}
				if (num3 <= 0)
				{
					Debug.LogError("Shrine is attempting to give negative ammo!");
					num3 = 1;
				}
				interactor.inventory.CurrentGun.GainAmmo(num3);
			}
			break;
		case BenefitType.STATS:
		{
			for (int i = 0; i < statMods.Length; i++)
			{
				if (interactor.ownerlessStatModifiers == null)
				{
					interactor.ownerlessStatModifiers = new List<StatModifier>();
				}
				interactor.ownerlessStatModifiers.Add(statMods[i]);
			}
			interactor.stats.RecalculateStats(interactor);
			break;
		}
		case BenefitType.CLEANSE_CURSE:
		{
			StatModifier statModifier = new StatModifier();
			statModifier.amount = Mathf.Min(amount, PlayerStats.GetTotalCurse() * -1);
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			statModifier.statToBoost = PlayerStats.StatType.Curse;
			interactor.ownerlessStatModifiers.Add(statModifier);
			interactor.stats.RecalculateStats(interactor);
			break;
		}
		case BenefitType.SPAWN_CHEST:
		{
			IntVector2 position = interactor.CurrentRoom.GetBestRewardLocation(new IntVector2(2, 3)) + new IntVector2(0, 2);
			if (IsRNGChest)
			{
				Chest chest = GameManager.Instance.RewardManager.SpawnTotallyRandomChest(position);
				if (chest != null)
				{
					chest.RegisterChestOnMinimap(interactor.CurrentRoom);
				}
			}
			else
			{
				Chest chest2 = GameManager.Instance.RewardManager.SpawnRewardChestAt(position);
				if (chest2 != null)
				{
					chest2.RegisterChestOnMinimap(interactor.CurrentRoom);
				}
			}
			break;
		}
		case BenefitType.SPECIFIC_ITEM:
			LootEngine.GivePrefabToPlayer(PickupObjectDatabase.GetById(targetItemID).gameObject, interactor);
			break;
		case BenefitType.COMPANION:
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.TIMES_COMPANION_SHRINED, 1f);
			CompanionItem companionItem = LootEngine.GetItemOfTypeAndQuality<CompanionItem>(PickupObject.ItemQuality.A, GameManager.Instance.RewardManager.ItemsLootTable, true);
			if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_COMPANION_SHRINED) >= 2f)
			{
				GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_TURKEY, true);
				if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_COMPANION_SHRINED) == 2f)
				{
					companionItem = PickupObjectDatabase.GetById(TurkeyCompanionForCompanionShrine) as CompanionItem;
				}
			}
			if (GameStatsManager.Instance.IsRainbowRun)
			{
				LootEngine.SpawnBowlerNote(GameManager.Instance.RewardManager.BowlerNoteOtherSource, interactor.transform.position + new Vector3(0f, -0.5f, 0f), interactor.CurrentRoom, true);
			}
			else if ((bool)companionItem)
			{
				LootEngine.GivePrefabToPlayer(companionItem.gameObject, interactor);
			}
			break;
		}
		case BenefitType.BLOODTHIRST:
			interactor.gameObject.GetOrAddComponent<Bloodthirst>();
			break;
		}
	}
}
