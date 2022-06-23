using System;
using System.Collections.Generic;

[Serializable]
public class TalkModule
{
	public string moduleID;

	public string[] stringKeys;

	public bool sequentialStrings;

	[NonSerialized]
	public int sequentialStringLastIndex = -1;

	public bool usesAnimation;

	[ShowInInspectorIf("usesAnimation", false)]
	public string animationName = string.Empty;

	[ShowInInspectorIf("usesAnimation", false)]
	public float animationDuration = -1f;

	public string additionalAnimationName = string.Empty;

	public List<TalkResponse> responses;

	public string noResponseFollowupModule = string.Empty;

	public List<TalkResult> moduleResultActions;

	public void CopyFrom(TalkModule source)
	{
		moduleID = source.moduleID + " copy";
		stringKeys = new List<string>(source.stringKeys).ToArray();
		sequentialStrings = source.sequentialStrings;
		usesAnimation = source.usesAnimation;
		animationName = source.animationName;
		animationDuration = source.animationDuration;
		additionalAnimationName = source.additionalAnimationName;
		responses = new List<TalkResponse>(source.responses);
		moduleResultActions = new List<TalkResult>(source.moduleResultActions);
	}
}
