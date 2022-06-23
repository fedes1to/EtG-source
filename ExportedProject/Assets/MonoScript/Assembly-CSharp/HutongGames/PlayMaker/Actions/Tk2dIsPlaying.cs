using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Check if a sprite animation is playing. \nNOTE: The Game Object must have a tk2dSpriteAnimator attached.")]
	[ActionCategory("2D Toolkit/SpriteAnimator")]
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W720")]
	public class Tk2dIsPlaying : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dSpriteAnimator))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dSpriteAnimator component attached.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("The clip name to play")]
		[RequiredField]
		public FsmString clipName;

		[UIHint(UIHint.Variable)]
		[Tooltip("is the clip playing?")]
		public FsmBool isPlaying;

		[Tooltip("EVvnt sent if clip is playing")]
		public FsmEvent isPlayingEvent;

		[Tooltip("Event sent if clip is not playing")]
		public FsmEvent isNotPlayingEvent;

		[Tooltip("Repeat every frame.")]
		public bool everyframe;

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
			clipName = null;
			everyframe = false;
			isPlayingEvent = null;
			isNotPlayingEvent = null;
		}

		public override void OnEnter()
		{
			_getSprite();
			DoIsPlaying();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoIsPlaying();
		}

		private void DoIsPlaying()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dSpriteAnimator component: " + _sprite.gameObject.name);
				return;
			}
			bool flag = _sprite.IsPlaying(clipName.Value);
			isPlaying.Value = flag;
			if (flag)
			{
				base.Fsm.Event(isPlayingEvent);
			}
			else
			{
				base.Fsm.Event(isNotPlayingEvent);
			}
		}
	}
}
