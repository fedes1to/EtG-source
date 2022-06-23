using System;

namespace Dungeonator
{
	[Serializable]
	public class RuntimeInjectionFlags
	{
		public bool ShopAnnexed;

		public bool CastleFireplace;

		public void Clear()
		{
			ShopAnnexed = false;
			CastleFireplace = false;
		}

		public bool Merge(RuntimeInjectionFlags flags)
		{
			bool result = false;
			if (!CastleFireplace && flags.CastleFireplace)
			{
				result = true;
			}
			ShopAnnexed |= flags.ShopAnnexed;
			CastleFireplace |= flags.CastleFireplace;
			return result;
		}

		public bool IsValid(RuntimeInjectionFlags other)
		{
			bool result = true;
			if (ShopAnnexed && other.ShopAnnexed)
			{
				result = false;
			}
			if (CastleFireplace && other.CastleFireplace)
			{
				result = false;
			}
			return result;
		}
	}
}
