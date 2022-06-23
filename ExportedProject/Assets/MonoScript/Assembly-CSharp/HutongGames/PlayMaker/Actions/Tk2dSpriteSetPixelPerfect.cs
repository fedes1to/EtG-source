using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("2D Toolkit/Sprite")]
	[Tooltip("Set the pixel perfect flag of a sprite. \nNOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite)")]
	public class Tk2dSpriteSetPixelPerfect : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dBaseSprite))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite).")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.FsmBool)]
		[Tooltip("Does the sprite needs to be kept pixelPerfect? This is only necessary when using a perspective camera.")]
		public FsmBool pixelPerfect;

		[Tooltip("Repeat every frame.")]
		[ActionSection("")]
		public bool everyframe;

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
			pixelPerfect = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			_getSprite();
			DoSetSpritePixelPerfect();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoSetSpritePixelPerfect();
		}

		private void DoSetSpritePixelPerfect()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dBaseSprite component");
			}
			else if (pixelPerfect.Value)
			{
				_sprite.MakePixelPerfect();
			}
		}
	}
}
