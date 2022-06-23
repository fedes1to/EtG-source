using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class Trigger : BaseType
	{
		public void Post(GameObject gameObject)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.PostTrigger(GetID(), gameObject);
				Verify(result);
			}
		}
	}
}
