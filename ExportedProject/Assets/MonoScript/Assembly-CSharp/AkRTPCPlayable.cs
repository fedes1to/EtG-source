using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class AkRTPCPlayable : PlayableAsset, ITimelineClipAsset
{
	public bool overrideTrackObject;

	private TimelineClip owningClip;

	private RTPC RTPC;

	public ExposedReference<GameObject> RTPCObject;

	public bool setRTPCGlobally;

	public AkRTPCPlayableBehaviour template = new AkRTPCPlayableBehaviour();

	public RTPC Parameter
	{
		get
		{
			return RTPC;
		}
		set
		{
			RTPC = value;
		}
	}

	public TimelineClip OwningClip
	{
		get
		{
			return owningClip;
		}
		set
		{
			owningClip = value;
		}
	}

	public ClipCaps clipCaps
	{
		get
		{
			return ClipCaps.None;
		}
	}

	public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
	{
		ScriptPlayable<AkRTPCPlayableBehaviour> scriptPlayable = ScriptPlayable<AkRTPCPlayableBehaviour>.Create(graph, template);
		AkRTPCPlayableBehaviour b = scriptPlayable.GetBehaviour();
		InitializeBehavior(graph, ref b, go);
		return scriptPlayable;
	}

	public void InitializeBehavior(PlayableGraph graph, ref AkRTPCPlayableBehaviour b, GameObject owner)
	{
		b.overrideTrackObject = overrideTrackObject;
		b.setRTPCGlobally = setRTPCGlobally;
		if (overrideTrackObject)
		{
			b.rtpcObject = RTPCObject.Resolve(graph.GetResolver());
		}
		else
		{
			b.rtpcObject = owner;
		}
		b.parameter = RTPC;
	}
}
