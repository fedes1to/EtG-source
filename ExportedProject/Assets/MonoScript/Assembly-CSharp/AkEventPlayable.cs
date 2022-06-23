using System;
using AK.Wwise;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class AkEventPlayable : PlayableAsset, ITimelineClipAsset
{
	private readonly WwiseEventTracker eventTracker = new WwiseEventTracker();

	public AK.Wwise.Event akEvent;

	private float blendInDuration;

	private float blendOutDuration;

	private float easeInDuration;

	private float easeOutDuration;

	public ExposedReference<GameObject> emitterObjectRef;

	[SerializeField]
	private float eventDurationMax = -1f;

	[SerializeField]
	private float eventDurationMin = -1f;

	public bool overrideTrackEmitterObject;

	private TimelineClip owningClip;

	public bool retriggerEvent;

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

	public override double duration
	{
		get
		{
			if (akEvent == null)
			{
				return base.duration;
			}
			return eventDurationMax;
		}
	}

	public ClipCaps clipCaps
	{
		get
		{
			if (!retriggerEvent)
			{
				return ClipCaps.All;
			}
			return ClipCaps.None;
		}
	}

	public void setEaseInDuration(float d)
	{
		easeInDuration = d;
	}

	public void setEaseOutDuration(float d)
	{
		easeOutDuration = d;
	}

	public void setBlendInDuration(float d)
	{
		blendInDuration = d;
	}

	public void setBlendOutDuration(float d)
	{
		blendOutDuration = d;
	}

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<AkEventPlayableBehavior> scriptPlayable = ScriptPlayable<AkEventPlayableBehavior>.Create(graph);
		AkEventPlayableBehavior behaviour = scriptPlayable.GetBehaviour();
		initializeBehaviour(graph, behaviour, owner);
		behaviour.akEventMinDuration = eventDurationMin;
		behaviour.akEventMaxDuration = eventDurationMax;
		return scriptPlayable;
	}

	public void initializeBehaviour(PlayableGraph graph, AkEventPlayableBehavior b, GameObject owner)
	{
		b.akEvent = akEvent;
		b.eventTracker = eventTracker;
		b.easeInDuration = easeInDuration;
		b.easeOutDuration = easeOutDuration;
		b.blendInDuration = blendInDuration;
		b.blendOutDuration = blendOutDuration;
		b.eventShouldRetrigger = retriggerEvent;
		b.overrideTrackEmittorObject = overrideTrackEmitterObject;
		if (overrideTrackEmitterObject)
		{
			b.eventObject = emitterObjectRef.Resolve(graph.GetResolver());
		}
		else
		{
			b.eventObject = owner;
		}
	}
}
