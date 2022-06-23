using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;

public class AkEventPlayableBehavior : PlayableBehaviour
{
	public enum AkPlayableAction
	{
		None = 0,
		Playback = 1,
		Retrigger = 2,
		Stop = 4,
		DelayedStop = 8,
		Seek = 0x10,
		FadeIn = 0x20,
		FadeOut = 0x40
	}

	public static int scrubPlaybackLengthMs = 100;

	public AK.Wwise.Event akEvent;

	public float akEventMaxDuration = -1f;

	public float akEventMinDuration = -1f;

	public float blendInDuration;

	public float blendOutDuration;

	public float easeInDuration;

	public float easeOutDuration;

	public GameObject eventObject;

	public bool eventShouldRetrigger;

	public WwiseEventTracker eventTracker;

	public float lastEffectiveWeight = 1f;

	public bool overrideTrackEmittorObject;

	public uint requiredActions;

	public override void PrepareFrame(Playable playable, FrameData info)
	{
		if (eventTracker == null)
		{
			return;
		}
		if (info.evaluationType == FrameData.EvaluationType.Evaluate && Application.isPlaying && ShouldPlay(playable))
		{
			if (!eventTracker.eventIsPlaying)
			{
				requiredActions |= 1u;
				requiredActions |= 8u;
				checkForFadeIn((float)playable.GetTime());
				checkForFadeOut(playable);
			}
			requiredActions |= 16u;
		}
		else
		{
			if (!eventTracker.eventIsPlaying && (requiredActions & 1) == 0)
			{
				requiredActions |= 2u;
				checkForFadeIn((float)playable.GetTime());
			}
			checkForFadeOut(playable);
		}
	}

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		if (akEvent == null || !ShouldPlay(playable))
		{
			return;
		}
		requiredActions |= 1u;
		if (info.evaluationType == FrameData.EvaluationType.Evaluate && Application.isPlaying)
		{
			requiredActions |= 8u;
			checkForFadeIn((float)playable.GetTime());
			checkForFadeOut(playable);
			return;
		}
		float proportionalTime = getProportionalTime(playable);
		float num = 0.05f;
		if (proportionalTime > num)
		{
			requiredActions |= 16u;
		}
		checkForFadeIn((float)playable.GetTime());
		checkForFadeOut(playable);
	}

	public override void OnBehaviourPause(Playable playable, FrameData info)
	{
		if (eventObject != null)
		{
			stopEvent();
		}
	}

	public override void ProcessFrame(Playable playable, FrameData info, object playerData)
	{
		if (!overrideTrackEmittorObject)
		{
			GameObject gameObject = playerData as GameObject;
			if (gameObject != null)
			{
				eventObject = gameObject;
			}
		}
		if (eventObject != null)
		{
			float currentClipTime = (float)playable.GetTime();
			if (actionIsRequired(AkPlayableAction.Playback))
			{
				playEvent();
			}
			if (eventShouldRetrigger && actionIsRequired(AkPlayableAction.Retrigger))
			{
				retriggerEvent(playable);
			}
			if (actionIsRequired(AkPlayableAction.Stop))
			{
				akEvent.Stop(eventObject);
			}
			if (actionIsRequired(AkPlayableAction.DelayedStop))
			{
				stopEvent(scrubPlaybackLengthMs);
			}
			if (actionIsRequired(AkPlayableAction.Seek))
			{
				seekToTime(playable);
			}
			if (actionIsRequired(AkPlayableAction.FadeIn))
			{
				triggerFadeIn(currentClipTime);
			}
			if (actionIsRequired(AkPlayableAction.FadeOut))
			{
				float fadeDuration = (float)(playable.GetDuration() - playable.GetTime());
				triggerFadeOut(fadeDuration);
			}
		}
		requiredActions = 0u;
	}

	private bool actionIsRequired(AkPlayableAction actionType)
	{
		return (requiredActions & (uint)actionType) != 0;
	}

	private bool ShouldPlay(Playable playable)
	{
		if (eventTracker != null)
		{
			if (akEventMaxDuration == akEventMinDuration && akEventMinDuration != -1f)
			{
				return (float)playable.GetTime() < akEventMaxDuration || eventShouldRetrigger;
			}
			float num = (float)playable.GetTime() - eventTracker.previousEventStartTime;
			float currentDuration = eventTracker.currentDuration;
			float num2 = ((currentDuration != -1f) ? currentDuration : ((float)playable.GetDuration()));
			return num < num2 || eventShouldRetrigger;
		}
		return false;
	}

	private bool fadeInRequired(float currentClipTime)
	{
		float num = blendInDuration - currentClipTime;
		float num2 = easeInDuration - currentClipTime;
		return num > 0f || num2 > 0f;
	}

	private void checkForFadeIn(float currentClipTime)
	{
		if (fadeInRequired(currentClipTime))
		{
			requiredActions |= 32u;
		}
	}

	private void checkForFadeInImmediate(float currentClipTime)
	{
		if (fadeInRequired(currentClipTime))
		{
			triggerFadeIn(currentClipTime);
		}
	}

	private bool fadeOutRequired(Playable playable)
	{
		float num = (float)(playable.GetDuration() - playable.GetTime());
		float num2 = blendOutDuration - num;
		float num3 = easeOutDuration - num;
		return num2 >= 0f || num3 >= 0f;
	}

	private void checkForFadeOutImmediate(Playable playable)
	{
		if (eventTracker != null && !eventTracker.fadeoutTriggered && fadeOutRequired(playable))
		{
			float fadeDuration = (float)(playable.GetDuration() - playable.GetTime());
			triggerFadeOut(fadeDuration);
		}
	}

	private void checkForFadeOut(Playable playable)
	{
		if (eventTracker != null && !eventTracker.fadeoutTriggered && fadeOutRequired(playable))
		{
			requiredActions |= 64u;
		}
	}

	protected void triggerFadeIn(float currentClipTime)
	{
		if (eventObject != null && akEvent != null)
		{
			float num = Mathf.Max(easeInDuration - currentClipTime, blendInDuration - currentClipTime);
			if (num > 0f)
			{
				akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Pause, 0, AkCurveInterpolation.AkCurveInterpolation_Linear);
				akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Resume, (int)(num * 1000f), AkCurveInterpolation.AkCurveInterpolation_Linear);
			}
		}
	}

	protected void triggerFadeOut(float fadeDuration)
	{
		if (eventObject != null && akEvent != null)
		{
			if (eventTracker != null)
			{
				eventTracker.fadeoutTriggered = true;
			}
			akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Stop, (int)(fadeDuration * 1000f), AkCurveInterpolation.AkCurveInterpolation_Linear);
		}
	}

	protected void stopEvent(int transition = 0)
	{
		if (eventObject != null && akEvent != null && eventTracker.eventIsPlaying)
		{
			akEvent.Stop(eventObject, transition);
			if (eventTracker != null)
			{
				eventTracker.eventIsPlaying = false;
			}
		}
	}

	protected void playEvent()
	{
		if (eventObject != null && akEvent != null && eventTracker != null)
		{
			eventTracker.playingID = akEvent.Post(eventObject, 9u, eventTracker.CallbackHandler);
			if (eventTracker.playingID != 0)
			{
				eventTracker.eventIsPlaying = true;
				eventTracker.currentDurationProportion = 1f;
				eventTracker.previousEventStartTime = 0f;
			}
		}
	}

	protected void retriggerEvent(Playable playable)
	{
		if (eventObject != null && akEvent != null && eventTracker != null)
		{
			eventTracker.playingID = akEvent.Post(eventObject, 9u, eventTracker.CallbackHandler);
			if (eventTracker.playingID != 0)
			{
				eventTracker.eventIsPlaying = true;
				float currentDurationProportion = seekToTime(playable);
				eventTracker.currentDurationProportion = currentDurationProportion;
				eventTracker.previousEventStartTime = (float)playable.GetTime();
			}
		}
	}

	protected float getProportionalTime(Playable playable)
	{
		if (eventTracker != null)
		{
			if (akEventMaxDuration == akEventMinDuration && akEventMinDuration != -1f)
			{
				return (float)playable.GetTime() % akEventMaxDuration / akEventMaxDuration;
			}
			float num = (float)playable.GetTime() - eventTracker.previousEventStartTime;
			float currentDuration = eventTracker.currentDuration;
			float num2 = ((currentDuration != -1f) ? currentDuration : ((float)playable.GetDuration()));
			return num % num2 / num2;
		}
		return 0f;
	}

	protected float seekToTime(Playable playable)
	{
		if (eventObject != null && akEvent != null)
		{
			float proportionalTime = getProportionalTime(playable);
			if (proportionalTime < 1f)
			{
				AkSoundEngine.SeekOnEvent((uint)akEvent.ID, eventObject, proportionalTime);
				return 1f - proportionalTime;
			}
		}
		return 1f;
	}
}
