using System;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class GenericCurrencyDropSettings
	{
		[PickupIdentifier]
		public int bronzeCoinId = -1;

		[PickupIdentifier]
		public int silverCoinId = -1;

		[PickupIdentifier]
		public int goldCoinId = -1;

		[PickupIdentifier]
		public int metaCoinId = -1;

		public WeightedIntCollection blackPhantomCoinDropChances;

		public GameObject bronzeCoinPrefab
		{
			get
			{
				return PickupObjectDatabase.GetById(bronzeCoinId).gameObject;
			}
		}

		public GameObject silverCoinPrefab
		{
			get
			{
				return PickupObjectDatabase.GetById(silverCoinId).gameObject;
			}
		}

		public GameObject goldCoinPrefab
		{
			get
			{
				return PickupObjectDatabase.GetById(goldCoinId).gameObject;
			}
		}

		public GameObject metaCoinPrefab
		{
			get
			{
				return PickupObjectDatabase.GetById(metaCoinId).gameObject;
			}
		}
	}
}
