using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	public class SharedDungeonSettings : MonoBehaviour
	{
		[Header("Currency")]
		public GenericCurrencyDropSettings currencyDropSettings;

		[Header("Boss Chests")]
		public WeightedGameObjectCollection ChestsForBosses;

		[Header("Mimics")]
		public List<DungeonPrerequisite> MimicPrerequisites = new List<DungeonPrerequisite>();

		public float MimicChance = 0.05f;

		public float MimicChancePerCurseLevel = 0.01f;

		[Header("Pedestal Mimics")]
		public List<DungeonPrerequisite> PedestalMimicPrerequisites = new List<DungeonPrerequisite>();

		public float PedestalMimicChance = 0.05f;

		public float PedestalMimicChancePerCurseLevel = 0.01f;

		[Header("Pot Fairies")]
		public List<DungeonPrerequisite> PotFairyPrerequisites = new List<DungeonPrerequisite>();

		[EnemyIdentifier]
		public string PotFairyGuid;

		public float PotFairyChance = 0.005f;

		[Header("Cross-Dungeon Settings")]
		public RobotDaveIdea DefaultProceduralIdea;

		public ExplosionData DefaultExplosionData;

		public ExplosionData DefaultSmallExplosionData;

		public GameActorFreezeEffect DefaultFreezeExplosionEffect;

		public GoopDefinition DefaultFreezeGoop;

		public GoopDefinition DefaultFireGoop;

		public GoopDefinition DefaultPoisonGoop;

		public GameObject additionalCheeseDustup;

		public GameObject additionalWebDustup;

		public GameObject additionalTableDustup;

		public GameActorCharmEffect DefaultPermanentCharmEffect;

		public List<GameObject> GetCurrencyToDrop(int amountToDrop, bool isMetaCurrency = false, bool randomAmounts = false)
		{
			List<GameObject> list = new List<GameObject>();
			int currencyValue = currencyDropSettings.goldCoinPrefab.GetComponent<CurrencyPickup>().currencyValue;
			int currencyValue2 = currencyDropSettings.silverCoinPrefab.GetComponent<CurrencyPickup>().currencyValue;
			int currencyValue3 = currencyDropSettings.bronzeCoinPrefab.GetComponent<CurrencyPickup>().currencyValue;
			int num = 1;
			while (amountToDrop > 0)
			{
				GameObject gameObject = null;
				if (isMetaCurrency)
				{
					amountToDrop -= num;
					gameObject = currencyDropSettings.metaCoinPrefab;
				}
				else if (randomAmounts)
				{
					if (amountToDrop >= currencyValue)
					{
						float value = Random.value;
						if (value < 0.05f)
						{
							amountToDrop -= currencyValue;
							gameObject = currencyDropSettings.goldCoinPrefab;
						}
						else if (value < 0.25f)
						{
							amountToDrop -= currencyValue2;
							gameObject = currencyDropSettings.silverCoinPrefab;
						}
						else
						{
							amountToDrop -= currencyValue3;
							gameObject = currencyDropSettings.bronzeCoinPrefab;
						}
					}
					else if (amountToDrop >= currencyValue2)
					{
						if (Random.value < 0.25f)
						{
							amountToDrop -= currencyValue2;
							gameObject = currencyDropSettings.silverCoinPrefab;
						}
						else
						{
							amountToDrop -= currencyValue3;
							gameObject = currencyDropSettings.bronzeCoinPrefab;
						}
					}
					else
					{
						if (amountToDrop < currencyValue3)
						{
							amountToDrop = 0;
							break;
						}
						amountToDrop -= currencyValue3;
						gameObject = currencyDropSettings.bronzeCoinPrefab;
					}
				}
				else if (amountToDrop >= currencyValue)
				{
					amountToDrop -= currencyValue;
					gameObject = currencyDropSettings.goldCoinPrefab;
				}
				else if (amountToDrop >= currencyValue2)
				{
					amountToDrop -= currencyValue2;
					gameObject = currencyDropSettings.silverCoinPrefab;
				}
				else
				{
					if (amountToDrop < currencyValue3)
					{
						amountToDrop = 0;
						break;
					}
					amountToDrop -= currencyValue3;
					gameObject = currencyDropSettings.bronzeCoinPrefab;
				}
				if (gameObject != null)
				{
					list.Add(gameObject);
				}
			}
			return list;
		}

		public bool RandomShouldBecomeMimic(float overrideChance = -1f)
		{
			for (int i = 0; i < MimicPrerequisites.Count; i++)
			{
				if (!MimicPrerequisites[i].CheckConditionsFulfilled())
				{
					return false;
				}
			}
			float num;
			if (overrideChance >= 0f)
			{
				num = overrideChance;
			}
			else
			{
				float num2 = PlayerStats.GetTotalCurse();
				num = MimicChance + MimicChancePerCurseLevel * num2;
				if (PassiveItem.IsFlagSetAtAll(typeof(MimicToothNecklaceItem)))
				{
					num += 10f;
				}
			}
			float value = Random.value;
			Debug.Log("mimic " + value + "|" + num);
			return value <= num;
		}

		public bool RandomShouldBecomePedestalMimic(float overrideChance = -1f)
		{
			for (int i = 0; i < PedestalMimicPrerequisites.Count; i++)
			{
				if (!PedestalMimicPrerequisites[i].CheckConditionsFulfilled())
				{
					return false;
				}
			}
			float num;
			if (overrideChance >= 0f)
			{
				num = overrideChance;
			}
			else
			{
				float num2 = PlayerStats.GetTotalCurse();
				num = PedestalMimicChance + PedestalMimicChancePerCurseLevel * num2;
				if (PassiveItem.IsFlagSetAtAll(typeof(MimicToothNecklaceItem)))
				{
					num += 10f;
				}
			}
			float value = Random.value;
			Debug.Log("pedestal mimic " + value + "|" + num);
			return value <= num;
		}

		public bool RandomShouldSpawnPotFairy()
		{
			for (int i = 0; i < PotFairyPrerequisites.Count; i++)
			{
				if (!PotFairyPrerequisites[i].CheckConditionsFulfilled())
				{
					return false;
				}
			}
			return Random.value <= PotFairyChance;
		}
	}
}
