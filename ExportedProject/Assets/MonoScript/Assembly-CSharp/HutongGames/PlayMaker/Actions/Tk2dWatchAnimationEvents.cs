using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Receive animation events and animation complete event of the current animation playing. \nNOTE: The Game Object must have a tk2dSpriteAnimator attached.")]
	[ActionCategory("2D Toolkit/SpriteAnimator")]
	public class Tk2dWatchAnimationEvents : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dSpriteAnimator))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dSpriteAnimator component attached.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("Trigger event defined in the clip. The event holds the following triggers infos: the eventInt, eventInfo and eventFloat properties")]
		public FsmEvent animationTriggerEvent;

		[Tooltip("Animation complete event. The event holds the clipId reference")]
		public FsmEvent animationCompleteEvent;

		private tk2dSpriteAnimator _sprite;

		private void _getSprite()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (!(ownerDefaultTarget == null))
			{
				_sprite = ownerDefaultTarget.GetComponent<tk2dSpriteAnimator>();
			}
		}

		public override void Reset()
		{
			gameObject = null;
			animationTriggerEvent = null;
			animationCompleteEvent = null;
		}

		public override void OnEnter()
		{
			_getSprite();
			DoWatchAnimationWithEvents();
		}

		private void DoWatchAnimationWithEvents()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dSpriteAnimator component");
				return;
			}
			if (animationTriggerEvent != null)
			{
				_sprite.AnimationEventTriggered = AnimationEventDelegate;
			}
			if (animationCompleteEvent != null)
			{
				_sprite.AnimationCompleted = AnimationCompleteDelegate;
			}
		}

		private void AnimationEventDelegate(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
		{
			tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
			Fsm.EventData.IntData = frame.eventInt;
			Fsm.EventData.StringData = frame.eventInfo;
			Fsm.EventData.FloatData = frame.eventFloat;
			base.Fsm.Event(animationTriggerEvent);
		}

		private void AnimationCompleteDelegate(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip)
		{
			int intData = -1;
			tk2dSpriteAnimationClip[] array = ((!(sprite.Library != null)) ? null : sprite.Library.clips);
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] == clip)
					{
						intData = i;
						break;
					}
				}
			}
			Fsm.EventData.IntData = intData;
			base.Fsm.Event(animationCompleteEvent);
		}
	}
}
