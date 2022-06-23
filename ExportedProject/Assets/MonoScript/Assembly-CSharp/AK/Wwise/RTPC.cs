using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class RTPC : BaseType
	{
		public void SetValue(GameObject gameObject, float value)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetRTPCValue(GetID(), value, gameObject);
				Verify(result);
			}
		}

		public void SetGlobalValue(float value)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.SetRTPCValue(GetID(), value);
				Verify(result);
			}
		}
	}
}
