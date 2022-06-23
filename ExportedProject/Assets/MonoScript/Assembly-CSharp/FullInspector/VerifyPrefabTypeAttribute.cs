using System;

namespace FullInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class VerifyPrefabTypeAttribute : Attribute
	{
		public VerifyPrefabTypeFlags PrefabType;

		public VerifyPrefabTypeAttribute(VerifyPrefabTypeFlags prefabType)
		{
			PrefabType = prefabType;
		}
	}
}
