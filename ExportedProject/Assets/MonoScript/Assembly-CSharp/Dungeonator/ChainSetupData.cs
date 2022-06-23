using System;
using UnityEngine;

namespace Dungeonator
{
	[Serializable]
	public class ChainSetupData
	{
		public enum ExitPreferenceMetric
		{
			RANDOM,
			HORIZONTAL,
			VERTICAL,
			FARTHEST,
			NEAREST
		}

		[SerializeField]
		public DungeonChain chain;

		[SerializeField]
		public int minSubchainsToBuild;

		[SerializeField]
		public int maxSubchainsToBuild = 3;

		[SerializeField]
		public ChainSetupData[] childChains;

		[SerializeField]
		public ExitPreferenceMetric exitMetric;
	}
}
