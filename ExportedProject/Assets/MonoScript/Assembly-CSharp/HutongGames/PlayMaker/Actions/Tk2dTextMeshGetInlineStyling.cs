using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("2D Toolkit/TextMesh")]
	[Tooltip("Get the inline styling flag of a TextMesh. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	public class Tk2dTextMeshGetInlineStyling : FsmStateAction
	{
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		[RequiredField]
		[CheckForComponent(typeof(tk2dTextMesh))]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("The max number of characters")]
		[UIHint(UIHint.Variable)]
		public FsmBool inlineStyling;

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
			inlineStyling = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			_getTextMesh();
			DoGetInlineStyling();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetInlineStyling();
		}

		private void DoGetInlineStyling()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component: ");
			}
			else
			{
				inlineStyling.Value = _textMesh.inlineStyling;
			}
		}
	}
}
