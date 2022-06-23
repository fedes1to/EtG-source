using System.Collections.Generic;
using FullSerializer;

public class MidGamePlayerData
{
	[fsProperty]
	public PlayableCharacters CharacterIdentity;

	[fsProperty]
	public float CurrentHealth = 1f;

	[fsProperty]
	public float CurrentArmor;

	[fsProperty]
	public int CurrentKeys;

	[fsProperty]
	public int CurrentCurrency;

	[fsProperty]
	public int CurrentBlanks;

	[fsProperty]
	public List<MidGameGunData> guns;

	[fsProperty]
	public List<MidGameActiveItemData> activeItems;

	[fsProperty]
	public List<MidGamePassiveItemData> passiveItems;

	[fsProperty]
	public List<StatModifier> ownerlessStatModifiers;

	[fsProperty]
	public int CostumeID;

	[fsProperty]
	public int MasteryTokensCollected;

	[fsProperty]
	public bool CharacterUsesRandomGuns;

	[fsProperty]
	public ChallengeModeType ChallengeMode;

	[fsProperty]
	public bool HasTakenDamageThisRun;

	[fsProperty]
	public bool HasFiredNonStartingGun;

	[fsProperty]
	public bool HasBloodthirst;

	[fsProperty]
	public bool IsTemporaryEeveeForUnlock;

	public MidGamePlayerData(PlayerController p)
	{
		CharacterIdentity = p.characterIdentity;
		CostumeID = (p.IsUsingAlternateCostume ? 1 : 0);
		MasteryTokensCollected = p.MasteryTokensCollectedThisRun;
		CharacterUsesRandomGuns = p.CharacterUsesRandomGuns;
		ChallengeMode = ChallengeManager.ChallengeModeType;
		HasTakenDamageThisRun = p.HasTakenDamageThisRun;
		HasFiredNonStartingGun = p.HasFiredNonStartingGun;
		HasBloodthirst = p.GetComponent<Bloodthirst>();
		IsTemporaryEeveeForUnlock = p.IsTemporaryEeveeForUnlock;
		CurrentHealth = p.healthHaver.GetCurrentHealth();
		CurrentArmor = p.healthHaver.Armor;
		CurrentKeys = p.carriedConsumables.KeyBullets;
		CurrentCurrency = p.carriedConsumables.Currency;
		CurrentBlanks = p.Blanks;
		guns = new List<MidGameGunData>();
		if (p.inventory != null && p.inventory.AllGuns != null)
		{
			for (int i = 0; i < p.inventory.AllGuns.Count; i++)
			{
				if (!p.inventory.AllGuns[i].PreventSaveSerialization)
				{
					guns.Add(new MidGameGunData(p.inventory.AllGuns[i]));
				}
			}
		}
		activeItems = new List<MidGameActiveItemData>();
		if (p.activeItems != null)
		{
			for (int j = 0; j < p.activeItems.Count; j++)
			{
				if (!p.activeItems[j].PreventSaveSerialization)
				{
					activeItems.Add(new MidGameActiveItemData(p.activeItems[j]));
				}
			}
		}
		passiveItems = new List<MidGamePassiveItemData>();
		if (p.passiveItems != null)
		{
			for (int k = 0; k < p.passiveItems.Count; k++)
			{
				if (!p.passiveItems[k].PreventSaveSerialization)
				{
					passiveItems.Add(new MidGamePassiveItemData(p.passiveItems[k]));
				}
			}
		}
		ownerlessStatModifiers = new List<StatModifier>();
		if (p.ownerlessStatModifiers == null)
		{
			return;
		}
		for (int l = 0; l < p.ownerlessStatModifiers.Count; l++)
		{
			if (!p.ownerlessStatModifiers[l].ignoredForSaveData)
			{
				ownerlessStatModifiers.Add(p.ownerlessStatModifiers[l]);
			}
		}
	}
}
