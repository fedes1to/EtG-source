using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Make a sprite pixelPerfect. \nNOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite)")]
	[ActionCategory("2D Toolkit/Sprite")]
	public class Tk2dSpriteMakePixelPerfect : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(tk2dBaseSprite))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite)")]
		public FsmOwnerDefault gameObject;

		private tk2dBaseSprite _sprite;

		private void _getSprite()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (!(ownerDefaultTarget == null))
			{
				_sprite = ownerDefaultTarget.GetComponent<tk2dBaseSprite>();
			}
		}

		public override void Reset()
		{
			gameObject = null;
		}

		public override void OnEnter()
		{
			_getSprite();
			MakePixelPerfect();
			Finish();
		}

		private void MakePixelPerfect()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dBaseSprite component: ");
			}
			else
			{
				_sprite.MakePixelPerfect();
			}
		}
	}
}
