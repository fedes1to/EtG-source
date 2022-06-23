using System;

namespace Dungeonator
{
	[Serializable]
	public class TileIndexVariant
	{
		public int index;

		public float likelihood = 0.1f;

		public int overrideLayerIndex = -1;

		public int overrideIndex = -1;
	}
}
