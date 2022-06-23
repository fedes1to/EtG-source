using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the maximum characters number of a TextMesh. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	[ActionCategory("2D Toolkit/TextMesh")]
	public class Tk2dTextMeshGetMaxChars : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dTextMesh))]
		[RequiredField]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[Tooltip("The max number of characters")]
		public FsmInt maxChars;

		[Tooltip("Repeat every frame.")]
		[ActionSection("")]
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
			maxChars = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			_getTextMesh();
			DoGetMaxChars();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetMaxChars();
		}

		private void DoGetMaxChars()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component: ");
			}
			else
			{
				maxChars.Value = _textMesh.maxChars;
			}
		}
	}
}
