using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Check that inline styling can indeed be used ( the font needs to have texture gradients for inline styling to work). \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	[ActionCategory("2D Toolkit/TextMesh")]
	public class Tk2dTextMeshGetInlineStylingIsAvailable : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(tk2dTextMesh))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("Is inline styling available? true if inlineStyling is true AND the font texturGradients is true")]
		public FsmBool InlineStylingAvailable;

		[ActionSection("")]
		[Tooltip("Repeat every frame.")]
		public bool everyframe;

		private tk2dTextMesh _textMesh;

		private void _getTextMesh()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (!(ownerDefaultTarget == null))
			{
				_textMesh = ownerDefaultTarget.GetComponent<tk2dTextMesh>();
			}
		}

		public override void Reset()
		{
			gameObject = null;
			InlineStylingAvailable = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			_getTextMesh();
			DoGetInlineStylingAvailable();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetInlineStylingAvailable();
		}

		private void DoGetInlineStylingAvailable()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component: ");
			}
			else
			{
				InlineStylingAvailable.Value = _textMesh.inlineStyling && _textMesh.font.textureGradients;
			}
		}
	}
}
