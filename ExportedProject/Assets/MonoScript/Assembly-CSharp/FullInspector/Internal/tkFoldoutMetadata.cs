using System;

namespace FullInspector.Internal
{
	[Serializable]
	public class tkFoldoutMetadata : IGraphMetadataItemPersistent
	{
		public bool IsExpanded;

		bool IGraphMetadataItemPersistent.ShouldSerialize()
		{
			return !IsExpanded;
		}
	}
}
