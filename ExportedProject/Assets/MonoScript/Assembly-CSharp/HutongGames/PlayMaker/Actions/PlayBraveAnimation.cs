using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".Brave")]
	[Tooltip("Plays an animation on the specified object.")]
	public class PlayBraveAnimation : FsmStateAction
	{
		public enum PlayMode
		{
			UntilCancelled,
			Duration,
			UntilFinished
		}

		public enum NextMode
		{
			ReturnToPrevious,
			NewAnimation
		}

		public FsmOwnerDefault GameObject;

		[Tooltip("Name of the animation to play.")]
		public FsmString animName;

		[Tooltip("How to play the animation.")]
		public PlayMode mode;

		[Tooltip("How long to play the animation for.")]
		public FsmFloat duration;

		[Tooltip("If the animation is already playing, don't trigger it again.")]
		public FsmBool dontPlayIfPlaying;

		[Tooltip("What animation to play next.")]
		public NextMode next;

		[Tooltip("The next animation to play (used only for UntilFinishedThenNext).")]
		public FsmString nextAnimName;

		[Tooltip("Time to wait after the animation before continuing to the next action; 0 continues immediately.")]
		public FsmFloat waitTime;

		public FsmBool playOnOtherTalkDoerInRoom;

		private float m_timer;

		private bool UsesNextAnim
		{
			get
			{
				return next == NextMode.NewAnimation;
			}
		}

		public override void Reset()
		{
			GameObject = null;
			animName = string.Empty;
			mode = PlayMode.UntilCancelled;
			nextAnimName = string.Empty;
			waitTime = 0f;
		}

		public override string ErrorCheck()
		{
			string text = string.Empty;
			GameObject gameObject = ((GameObject.OwnerOption != 0) ? GameObject.GameObject.Value : base.Owner);
			if ((bool)gameObject)
			{
				tk2dSpriteAnimator component = gameObject.GetComponent<tk2dSpriteAnimator>();
				AIAnimator component2 = gameObject.GetComponent<AIAnimator>();
				if (!component && !component2)
				{
					return "Requires a 2D Toolkit animator or an AI Animator.\n";
				}
				if ((bool)component2)
				{
					if (!component2.HasDirectionalAnimation(animName.Value))
					{
						text = text + "Unknown animation " + animName.Value + ".\n";
					}
					if (UsesNextAnim && !component2.HasDirectionalAnimation(nextAnimName.Value))
					{
						text = text + "Unknown animation " + nextAnimName.Value + ".\n";
					}
				}
				else if ((bool)component)
				{
					if (component.GetClipByName(animName.Value) == null)
					{
						text = text + "Unknown animation " + animName.Value + ".\n";
					}
					if (UsesNextAnim && component.GetClipByName(nextAnimName.Value) == null)
					{
						text = text + "Unknown animation " + nextAnimName.Value + ".\n";
					}
				}
			}
			else if (!GameObject.GameObject.UseVariable)
			{
				return "No object specified";
			}
			return text;
		}

		public override void OnEnter()
		{
			GameObject gameObject = base.Fsm.GetOwnerDefaultTarget(GameObject);
			if (playOnOtherTalkDoerInRoom.Value)
			{
				TalkDoerLite component = base.Owner.GetComponent<TalkDoerLite>();
				for (int i = 0; i < StaticReferenceManager.AllNpcs.Count; i++)
				{
					if (StaticReferenceManager.AllNpcs[i].ParentRoom == component.ParentRoom && StaticReferenceManager.AllNpcs[i] != component)
					{
						gameObject = StaticReferenceManager.AllNpcs[i].gameObject;
						break;
					}
				}
			}
			tk2dSpriteAnimator component2 = gameObject.GetComponent<tk2dSpriteAnimator>();
			AIAnimator component3 = gameObject.GetComponent<AIAnimator>();
			string text = ((!(component2 != null) || component2.CurrentClip == null) ? string.Empty : component2.CurrentClip.name);
			if (!dontPlayIfPlaying.Value || !(text == animName.Value))
			{
				if (mode == PlayMode.UntilCancelled)
				{
					if ((bool)component3)
					{
						bool flag = true;
						if ((bool)component3.talkDoer && animName.Value == "idle" && component3.talkDoer.IsPlayingZombieAnimation)
						{
							flag = false;
						}
						if (flag)
						{
							component3.PlayUntilCancelled(animName.Value);
						}
					}
					else
					{
						component2.Play(animName.Value);
					}
				}
				else if (mode == PlayMode.Duration)
				{
					if ((bool)component3)
					{
						component3.PlayForDuration(animName.Value, duration.Value);
					}
					else if (next == NextMode.ReturnToPrevious)
					{
						component2.PlayForDuration(animName.Value, duration.Value);
					}
					else if (next == NextMode.NewAnimation)
					{
						component2.PlayForDuration(animName.Value, duration.Value, nextAnimName.Value);
					}
				}
				else if (mode == PlayMode.UntilFinished)
				{
					if ((bool)component3)
					{
						component3.PlayUntilFinished(animName.Value);
					}
					else if (next == NextMode.ReturnToPrevious)
					{
						component2.PlayForDuration(animName.Value, -1f);
					}
					else if (next == NextMode.NewAnimation)
					{
						component2.PlayForDuration(animName.Value, -1f, nextAnimName.Value);
					}
				}
			}
			if (waitTime.Value > 0f)
			{
				m_timer = waitTime.Value;
			}
			else
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (m_timer > 0f)
			{
				m_timer -= BraveTime.DeltaTime;
				if (m_timer <= 0f)
				{
					Finish();
				}
			}
		}
	}
}
