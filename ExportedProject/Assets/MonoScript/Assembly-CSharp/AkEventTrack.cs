using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackBindingType(typeof(GameObject))]
[TrackClipType(typeof(AkEventPlayable))]
public class AkEventTrack : TrackAsset
{
	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		ScriptPlayable<AkEventPlayableBehavior> scriptPlayable = ScriptPlayable<AkEventPlayableBehavior>.Create(graph);
		scriptPlayable.SetInputCount(inputCount);
		setFadeTimes();
		setOwnerClips();
		return scriptPlayable;
	}

	public void setFadeTimes()
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			AkEventPlayable akEventPlayable = (AkEventPlayable)item.asset;
			akEventPlayable.setBlendInDuration((float)getBlendInTime(akEventPlayable));
			akEventPlayable.setBlendOutDuration((float)getBlendOutTime(akEventPlayable));
			akEventPlayable.setEaseInDuration((float)getEaseInTime(akEventPlayable));
			akEventPlayable.setEaseOutDuration((float)getEaseOutTime(akEventPlayable));
		}
	}

	public void setOwnerClips()
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			AkEventPlayable akEventPlayable = (AkEventPlayable)item.asset;
			akEventPlayable.OwningClip = item;
		}
	}

	public double getBlendInTime(AkEventPlayable playableClip)
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			if (playableClip == (AkEventPlayable)item.asset)
			{
				return item.blendInDuration;
			}
		}
		return 0.0;
	}

	public double getBlendOutTime(AkEventPlayable playableClip)
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			if (playableClip == (AkEventPlayable)item.asset)
			{
				return item.blendOutDuration;
			}
		}
		return 0.0;
	}

	public double getEaseInTime(AkEventPlayable playableClip)
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			if (playableClip == (AkEventPlayable)item.asset)
			{
				return item.easeInDuration;
			}
		}
		return 0.0;
	}

	public double getEaseOutTime(AkEventPlayable playableClip)
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			if (playableClip == (AkEventPlayable)item.asset)
			{
				return item.easeOutDuration;
			}
		}
		return 0.0;
	}
}
