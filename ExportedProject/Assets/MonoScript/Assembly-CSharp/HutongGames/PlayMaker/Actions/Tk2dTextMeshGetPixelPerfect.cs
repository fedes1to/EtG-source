namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the pixelPerfect flag of a TextMesh. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	[ActionCategory("2D Toolkit/TextMesh")]
	public class Tk2dTextMeshGetPixelPerfect : FsmStateAction
	{
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		[RequiredField]
		[CheckForComponent(typeof(tk2dTextMesh))]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[Tooltip("(Deprecated in 2D Toolkit 2.0) Is the text pixelPerfect")]
		[RequiredField]
		public FsmBool pixelPerfect;

		public override void Reset()
		{
			gameObject = null;
			pixelPerfect = null;
		}

		public override void OnEnter()
		{
			Finish();
		}
	}
}
