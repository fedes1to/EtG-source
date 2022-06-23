using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("2D Toolkit/SpriteAnimator")]
	[Tooltip("Stops a sprite animation. \nNOTE: The Game Object must have a tk2dSpriteAnimator attached.")]
	public class Tk2dStopAnimation : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dSpriteAnimator))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dSpriteAnimator component attached.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

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
		}

		public override void OnEnter()
		{
			_getSprite();
			DoStopAnimation();
			Finish();
		}

		private void DoStopAnimation()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dSpriteAnimator component: " + _sprite.gameObject.name);
			}
			else
			{
				_sprite.Stop();
			}
		}
	}
}
