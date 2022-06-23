using System;

namespace AK.Wwise
{
	[Serializable]
	public class BaseGroupType : BaseType
	{
		public int groupID;

		protected uint GetGroupID()
		{
			return (uint)groupID;
		}

		public override bool IsValid()
		{
			return base.IsValid() && groupID != 0;
		}
	}
}
