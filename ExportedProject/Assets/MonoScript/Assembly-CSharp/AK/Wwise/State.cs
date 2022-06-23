using System;

namespace AK.Wwise
{
	[Serializable]
	public class State : BaseGroupType
	{
		public void SetValue()
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetState(GetGroupID(), GetID());
				Verify(result);
			}
		}
	}
}
