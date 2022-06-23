using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class Event : BaseType
	{
		private void VerifyPlayingID(uint playingId)
		{
		}

		public uint Post(GameObject gameObject)
		{
			if (!IsValid())
			{
				return 0u;
			}
			uint num = AkSoundEngine.PostEvent(GetID(), gameObject);
			VerifyPlayingID(num);
			return num;
		}

		public uint Post(GameObject gameObject, CallbackFlags flags, AkCallbackManager.EventCallback callback, object cookie = null)
		{
			if (!IsValid())
			{
				return 0u;
			}
			uint num = AkSoundEngine.PostEvent(GetID(), gameObject, flags.value, callback, cookie);
			VerifyPlayingID(num);
			return num;
		}

		public uint Post(GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback, object cookie = null)
		{
			if (!IsValid())
			{
				return 0u;
			}
			uint num = AkSoundEngine.PostEvent(GetID(), gameObject, flags, callback, cookie);
			VerifyPlayingID(num);
			return num;
		}

		public void Stop(GameObject gameObject, int transitionDuration = 0, AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear)
		{
			ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Stop, transitionDuration, curveInterpolation);
		}

		public void ExecuteAction(GameObject gameObject, AkActionOnEventType actionOnEventType, int transitionDuration, AkCurveInterpolation curveInterpolation)
		{
			if (IsValid())
			{
				AKRESULT result = AkSoundEngine.ExecuteActionOnEvent(GetID(), actionOnEventType, gameObject, transitionDuration, curveInterpolation);
				Verify(result);
			}
		}

		public void PostMIDI(GameObject gameObject, AkMIDIPostArray array)
		{
			if (IsValid())
			{
				array.PostOnEvent(GetID(), gameObject);
			}
		}

		public void PostMIDI(GameObject gameObject, AkMIDIPostArray array, int count)
		{
			if (IsValid())
			{
				array.PostOnEvent(GetID(), gameObject, count);
			}
		}

		public void StopMIDI(GameObject gameObject)
		{
			if (IsValid())
			{
				AkSoundEngine.StopMIDIOnEvent(GetID(), gameObject);
			}
		}

		public void StopMIDI()
		{
			if (IsValid())
			{
				AkSoundEngine.StopMIDIOnEvent(GetID());
			}
		}
	}
}
