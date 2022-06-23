public class WwiseEventTracker
{
	public float currentDuration = -1f;

	public float currentDurationProportion = 1f;

	public bool eventIsPlaying;

	public bool fadeoutTriggered;

	public uint playingID;

	public float previousEventStartTime;

	public void CallbackHandler(object in_cookie, AkCallbackType in_type, object in_info)
	{
		switch (in_type)
		{
		case AkCallbackType.AK_EndOfEvent:
			eventIsPlaying = false;
			fadeoutTriggered = false;
			break;
		case AkCallbackType.AK_Duration:
		{
			float fEstimatedDuration = ((AkDurationCallbackInfo)in_info).fEstimatedDuration;
			currentDuration = fEstimatedDuration * currentDurationProportion / 1000f;
			break;
		}
		}
	}
}
