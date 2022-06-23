namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the pixel perfect flag of a sprite. \nNOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dClippedSprite)")]
	[ActionCategory("2D Toolkit/Sprite")]
	public class Tk2dSpriteGetPixelPerfect : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dBaseSprite))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dBaseSprite or derived component attached ( tk2dSprite, tk2dClippedSprite).")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[Tooltip("(Deprecated in 2D Toolkit 2.0) Is the sprite pixelPerfect")]
		public FsmBool pixelPerfect;

		public override void OnEnter()
		{
			Finish();
		}

		public override void Reset()
		{
			gameObject = null;
			pixelPerfect = null;
		}
	}
}
