using System;

namespace Dungeonator
{
	[Serializable]
	public class StampIndexVariant
	{
		public int baseIndex;

		public int tileHeight = 1;

		public int stampSheetWidth = 8;

		public float likelihood = 0.1f;
	}
}
