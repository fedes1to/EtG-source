using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the sprite id of a sprite. \nNOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite).")]
	[ActionCategory("2D Toolkit/Sprite")]
	public class Tk2dSpriteGetId : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dBaseSprite))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite)")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.FsmInt)]
		[Tooltip("The sprite Id")]
		public FsmInt spriteID;

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
			spriteID = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			_getSprite();
			DoGetSpriteID();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetSpriteID();
		}

		private void DoGetSpriteID()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dBaseSprite component");
			}
			else if (spriteID.Value != _sprite.spriteId)
			{
				spriteID.Value = _sprite.spriteId;
			}
		}
	}
}
