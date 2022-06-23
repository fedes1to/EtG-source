using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("2D Toolkit/Sprite")]
	[Tooltip("Set the sprite id of a sprite. Can use id or name. \nNOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite)")]
	public class Tk2dSpriteSetId : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dBaseSprite))]
		[RequiredField]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dAnimatedSprite).")]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.FsmInt)]
		[Tooltip("The sprite Id")]
		public FsmInt spriteID;

		[Tooltip("OR The sprite name ")]
		[UIHint(UIHint.FsmString)]
		public FsmString ORSpriteName;

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
			ORSpriteName = null;
		}

		public override void OnEnter()
		{
			_getSprite();
			DoSetSpriteID();
			Finish();
		}

		private void DoSetSpriteID()
		{
			if (_sprite == null)
			{
				LogWarning("Missing tk2dBaseSprite component: " + _sprite.gameObject.name);
				return;
			}
			int num = spriteID.Value;
			if (ORSpriteName.Value != string.Empty)
			{
				num = _sprite.GetSpriteIdByName(ORSpriteName.Value);
			}
			if (num != _sprite.spriteId)
			{
				_sprite.spriteId = num;
			}
		}
	}
}
