using System.Collections.Generic;
using UnityEngine;

public class BasicStatPickup : PassiveItem
{
	[BetterList]
	public List<StatModifier> modifiers;

	public int ArmorToGive;

	public bool ModifiesDodgeRoll;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public float DodgeRollTimeMultiplier = 0.9f;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public float DodgeRollDistanceMultiplier = 1.25f;

	[ShowInInspectorIf("ModifiesDodgeRoll", false)]
	public int AdditionalInvulnerabilityFrames;

	public bool IsJunk;

	public bool GivesCurrency;

	public int CurrencyToGive;

	public bool IsMasteryToken;

	public override void Pickup(PlayerController player)
	{
		if (m_pickedUp)
		{
			return;
		}
		if (ArmorToGive > 0 && !m_pickedUpThisRun)
		{
			player.healthHaver.Armor += ArmorToGive;
		}
		else if (!m_pickedUpThisRun && IsMasteryToken && player.characterIdentity == PlayableCharacters.Robot)
		{
			player.healthHaver.Armor += 1f;
		}
		if (ModifiesDodgeRoll)
		{
			player.rollStats.rollDistanceMultiplier *= DodgeRollDistanceMultiplier;
			player.rollStats.rollTimeMultiplier *= DodgeRollTimeMultiplier;
			player.rollStats.additionalInvulnerabilityFrames += AdditionalInvulnerabilityFrames;
		}
		if (!m_pickedUpThisRun && IsJunk && player.characterIdentity == PlayableCharacters.Robot)
		{
			StatModifier statModifier = new StatModifier();
			statModifier.statToBoost = PlayerStats.StatType.Damage;
			statModifier.amount = 0.05f;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			player.ownerlessStatModifiers.Add(statModifier);
			player.stats.RecalculateStats(player);
		}
		if (!m_pickedUpThisRun && GivesCurrency)
		{
			player.carriedConsumables.Currency += CurrencyToGive;
		}
		if (!m_pickedUpThisRun && player.characterIdentity == PlayableCharacters.Robot)
		{
			for (int i = 0; i < modifiers.Count; i++)
			{
				if (modifiers[i].statToBoost == PlayerStats.StatType.Health && modifiers[i].amount > 0f)
				{
					int amountToDrop = Mathf.FloorToInt(modifiers[i].amount * (float)Random.Range(GameManager.Instance.RewardManager.RobotMinCurrencyPerHealthItem, GameManager.Instance.RewardManager.RobotMaxCurrencyPerHealthItem + 1));
					LootEngine.SpawnCurrency(player.CenterPosition, amountToDrop);
				}
			}
		}
		base.Pickup(player);
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		if (ModifiesDodgeRoll)
		{
			player.rollStats.rollDistanceMultiplier /= DodgeRollDistanceMultiplier;
			player.rollStats.rollTimeMultiplier /= DodgeRollTimeMultiplier;
			player.rollStats.additionalInvulnerabilityFrames -= AdditionalInvulnerabilityFrames;
			player.rollStats.additionalInvulnerabilityFrames = Mathf.Max(player.rollStats.additionalInvulnerabilityFrames, 0);
		}
		debrisObject.GetComponent<BasicStatPickup>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
