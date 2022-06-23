using System;

public class AudioAnimatorListener : BraveBehaviour
{
	public ActorAudioEvent[] animationAudioEvents;

	private void Start()
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		if (base.spriteAnimator.CurrentClip != null)
		{
			HandleAnimationEvent(base.spriteAnimator, base.spriteAnimator.CurrentClip, 0);
		}
	}

	protected void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		for (int i = 0; i < animationAudioEvents.Length; i++)
		{
			if (animationAudioEvents[i].eventTag == frame.eventInfo)
			{
				AkSoundEngine.PostEvent(animationAudioEvents[i].eventName, base.gameObject);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
