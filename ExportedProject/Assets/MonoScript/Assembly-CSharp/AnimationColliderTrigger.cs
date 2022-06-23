using System;

public class AnimationColliderTrigger : BraveBehaviour
{
	private void Awake()
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	protected override void OnDestroy()
	{
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
		base.OnDestroy();
	}

	private void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (clip.GetFrame(frame).eventInfo == "collider_on")
		{
			if ((bool)base.aiActor)
			{
				base.aiActor.IsGone = false;
			}
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.CollideWithOthers = true;
			}
		}
		else if (clip.GetFrame(frame).eventInfo == "collider_off")
		{
			if ((bool)base.aiActor)
			{
				base.aiActor.IsGone = true;
			}
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.CollideWithOthers = false;
			}
		}
	}
}
