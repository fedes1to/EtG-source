using System;

[Serializable]
public class tk2dSpriteAnimationFrame
{
	public enum OutlineModifier
	{
		Unspecified = 0,
		TurnOn = 10,
		TurnOff = 20
	}

	public tk2dSpriteCollectionData spriteCollection;

	public int spriteId;

	public bool invulnerableFrame;

	public bool groundedFrame = true;

	public bool requiresOffscreenUpdate;

	public string eventAudio = string.Empty;

	public string eventVfx = string.Empty;

	public string eventStopVfx = string.Empty;

	public bool eventLerpEmissive;

	public float eventLerpEmissiveTime = 0.5f;

	public float eventLerpEmissivePower = 30f;

	public bool forceMaterialUpdate;

	public bool finishedSpawning;

	public bool triggerEvent;

	public string eventInfo = string.Empty;

	public int eventInt;

	public float eventFloat;

	public OutlineModifier eventOutline;

	public void CopyFrom(tk2dSpriteAnimationFrame source)
	{
		CopyFrom(source, true);
	}

	public void CopyTriggerFrom(tk2dSpriteAnimationFrame source)
	{
		triggerEvent = source.triggerEvent;
		eventInfo = source.eventInfo;
		eventInt = source.eventInt;
		eventFloat = source.eventFloat;
		eventAudio = source.eventAudio;
		eventVfx = source.eventVfx;
		eventStopVfx = source.eventStopVfx;
		eventOutline = source.eventOutline;
		forceMaterialUpdate = source.forceMaterialUpdate;
		finishedSpawning = source.finishedSpawning;
		eventLerpEmissive = source.eventLerpEmissive;
		eventLerpEmissivePower = source.eventLerpEmissivePower;
		eventLerpEmissiveTime = source.eventLerpEmissiveTime;
	}

	public void ClearTrigger()
	{
		triggerEvent = false;
		eventInt = 0;
		eventFloat = 0f;
		eventInfo = string.Empty;
		eventAudio = string.Empty;
		eventVfx = string.Empty;
		eventStopVfx = string.Empty;
		eventOutline = OutlineModifier.Unspecified;
		forceMaterialUpdate = false;
		finishedSpawning = false;
		eventLerpEmissive = false;
		eventLerpEmissivePower = 30f;
		eventLerpEmissiveTime = 0.5f;
	}

	public void CopyFrom(tk2dSpriteAnimationFrame source, bool full)
	{
		spriteCollection = source.spriteCollection;
		spriteId = source.spriteId;
		invulnerableFrame = source.invulnerableFrame;
		groundedFrame = source.groundedFrame;
		requiresOffscreenUpdate = source.requiresOffscreenUpdate;
		if (full)
		{
			CopyTriggerFrom(source);
		}
	}
}
