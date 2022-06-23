using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("2D Toolkit/SpriteAnimator")]
	[Tooltip("Plays a sprite animation. \nNOTE: The Game Object must have a tk2dSpriteAnimator attached.")]
	public class Tk2dPlayAnimation : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dSpriteAnimator))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dSpriteAnimator component attached.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[Tooltip("The anim Lib name. Leave empty to use the one current selected")]
		public FsmString animLibName;

		[Tooltip("The clip name to play")]
		[RequiredField]
		public FsmString clipName;

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
			animLibName = null;
			clipName = null;
		}

		public override void OnEnter()
		{
			_getSprite();
			DoPlayAnimation();
		}

		private void DoPlayAnimation()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dSpriteAnimator component");
				return;
			}
			if (!animLibName.Value.Equals(string.Empty))
			{
			}
			_sprite.Play(clipName.Value);
		}
	}
}
