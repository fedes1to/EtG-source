using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class Switch : BaseGroupType
	{
		public void SetValue(GameObject gameObject)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetSwitch(GetGroupID(), GetID(), gameObject);
				Verify(result);
			}
		}
	}
}
