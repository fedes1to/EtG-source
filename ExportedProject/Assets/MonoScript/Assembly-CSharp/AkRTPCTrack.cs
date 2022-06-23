using System.Collections.Generic;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.32f, 0.13f, 0.13f)]
[TrackClipType(typeof(AkRTPCPlayable))]
[TrackBindingType(typeof(GameObject))]
public class AkRTPCTrack : TrackAsset
{
	public RTPC Parameter;

	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		ScriptPlayable<AkRTPCPlayableBehaviour> scriptPlayable = ScriptPlayable<AkRTPCPlayableBehaviour>.Create(graph, inputCount);
		setPlayableProperties();
		return scriptPlayable;
	}

	public void setPlayableProperties()
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			AkRTPCPlayable akRTPCPlayable = (AkRTPCPlayable)item.asset;
			akRTPCPlayable.Parameter = Parameter;
			akRTPCPlayable.OwningClip = item;
		}
	}

	public void OnValidate()
	{
		IEnumerable<TimelineClip> enumerable = GetClips();
		foreach (TimelineClip item in enumerable)
		{
			AkRTPCPlayable akRTPCPlayable = (AkRTPCPlayable)item.asset;
			akRTPCPlayable.Parameter = Parameter;
		}
	}
}
