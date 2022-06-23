using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShrineCost
{
	public enum CostType
	{
		MONEY,
		HEALTH,
		ARMOR,
		BLANK,
		KEY,
		CURRENT_GUN,
		BEATEN_GAME,
		STATS,
		MONEY_PER_CURSE,
		SPECIFIC_ITEM
	}

	public CostType costType;

	public int cost;

	public bool AllowsArmorConversionForRobot;

	public StatModifier[] statMods;

	public string rngString;

	public float rngWeight = 1f;

	[PickupIdentifier]
	public int targetItemID;

	public bool CheckCost(PlayerController interactor)
	{
		switch (costType)
		{
		case CostType.MONEY:
			return interactor.carriedConsumables.Currency >= cost;
		case CostType.HEALTH:
			if (AllowsArmorConversionForRobot && interactor.characterIdentity == PlayableCharacters.Robot)
			{
				return interactor.healthHaver.Armor > (float)(cost * 2);
			}
			return interactor.healthHaver.GetCurrentHealth() > (float)cost;
		case CostType.ARMOR:
			return interactor.healthHaver.Armor >= (float)cost;
		case CostType.BLANK:
			return interactor.Blanks >= cost;
		case CostType.KEY:
			if (interactor.carriedConsumables.InfiniteKeys)
			{
				return true;
			}
			return interactor.carriedConsumables.KeyBullets >= cost;
		case CostType.CURRENT_GUN:
			return interactor.CurrentGun != null && interactor.CurrentGun.CanActuallyBeDropped(interactor) && !interactor.CurrentGun.InfiniteAmmo;
		case CostType.BEATEN_GAME:
			if (!GameStatsManager.Instance.HasPast(GameManager.Instance.PrimaryPlayer.characterIdentity))
			{
				return true;
			}
			return GameStatsManager.Instance.GetCharacterSpecificFlag(GameManager.Instance.PrimaryPlayer.characterIdentity, CharacterSpecificGungeonFlags.KILLED_PAST);
		case CostType.STATS:
			if (interactor.characterIdentity == PlayableCharacters.Robot && AllowsArmorConversionForRobot && statMods[0].statToBoost == PlayerStats.StatType.Health && statMods[0].amount * -2f < interactor.healthHaver.Armor)
			{
				return true;
			}
			if (statMods[0].statToBoost == PlayerStats.StatType.Health && statMods[0].amount * -1f >= interactor.healthHaver.GetMaxHealth())
			{
				return false;
			}
			return true;
		case CostType.MONEY_PER_CURSE:
			return interactor.carriedConsumables.Currency >= cost * PlayerStats.GetTotalCurse();
		case CostType.SPECIFIC_ITEM:
		{
			bool result = false;
			for (int i = 0; i < interactor.passiveItems.Count; i++)
			{
				if (interactor.passiveItems[i].PickupObjectId == targetItemID)
				{
					result = true;
				}
			}
			return result;
		}
		default:
			return false;
		}
	}

	public void ApplyCost(PlayerController interactor)
	{
		switch (costType)
		{
		case CostType.MONEY:
			interactor.carriedConsumables.Currency -= cost;
			break;
		case CostType.HEALTH:
			if (AllowsArmorConversionForRobot && interactor.characterIdentity == PlayableCharacters.Robot)
			{
				interactor.healthHaver.Armor = interactor.healthHaver.Armor - (float)(cost * 2);
				break;
			}
			interactor.healthHaver.NextDamageIgnoresArmor = true;
			interactor.healthHaver.ApplyDamage(cost, Vector2.zero, StringTableManager.GetEnemiesString("#SHRINE"), CoreDamageTypes.None, DamageCategory.Environment, true);
			break;
		case CostType.ARMOR:
			interactor.healthHaver.Armor -= cost;
			break;
		case CostType.BLANK:
			interactor.Blanks -= cost;
			break;
		case CostType.KEY:
			if (!interactor.carriedConsumables.InfiniteKeys)
			{
				interactor.carriedConsumables.KeyBullets -= cost;
			}
			break;
		case CostType.CURRENT_GUN:
			interactor.inventory.DestroyCurrentGun();
			break;
		case CostType.STATS:
		{
			for (int i = 0; i < statMods.Length; i++)
			{
				if (interactor.ownerlessStatModifiers == null)
				{
					interactor.ownerlessStatModifiers = new List<StatModifier>();
				}
				interactor.ownerlessStatModifiers.Add(statMods[i]);
			}
			if (interactor.characterIdentity == PlayableCharacters.Robot && AllowsArmorConversionForRobot && statMods[0].statToBoost == PlayerStats.StatType.Health && statMods[0].amount * -2f < interactor.healthHaver.Armor)
			{
				interactor.healthHaver.Armor = interactor.healthHaver.Armor - statMods[0].amount * -2f;
			}
			interactor.stats.RecalculateStats(interactor);
			break;
		}
		case CostType.MONEY_PER_CURSE:
			interactor.carriedConsumables.Currency -= Mathf.FloorToInt(cost * PlayerStats.GetTotalCurse());
			break;
		case CostType.SPECIFIC_ITEM:
			interactor.RemovePassiveItem(targetItemID);
			break;
		case CostType.BEATEN_GAME:
			break;
		}
	}
}
