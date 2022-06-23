using System;
using UnityEngine;

[Serializable]
public class StatModifier
{
	public enum ModifyMethod
	{
		ADDITIVE,
		MULTIPLICATIVE
	}

	public PlayerStats.StatType statToBoost;

	public ModifyMethod modifyType;

	public float amount;

	[NonSerialized]
	public bool hasBeenOwnerlessProcessed;

	[NonSerialized]
	public bool ignoredForSaveData;

	[HideInInspector]
	public bool isMeatBunBuff;

	public bool PersistsOnCoopDeath
	{
		get
		{
			return statToBoost == PlayerStats.StatType.Curse && amount > 0f;
		}
	}

	public static StatModifier Create(PlayerStats.StatType targetStat, ModifyMethod method, float amt)
	{
		StatModifier statModifier = new StatModifier();
		statModifier.statToBoost = targetStat;
		statModifier.amount = amt;
		statModifier.modifyType = method;
		return statModifier;
	}
}
