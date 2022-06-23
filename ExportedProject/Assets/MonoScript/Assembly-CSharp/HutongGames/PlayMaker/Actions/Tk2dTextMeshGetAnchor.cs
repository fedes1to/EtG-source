using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the anchor of a TextMesh. \nThe anchor is stored as a string. tk2dTextMeshSetAnchor can work with this string. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	[ActionCategory("2D Toolkit/TextMesh")]
	public class Tk2dTextMeshGetAnchor : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		[CheckForComponent(typeof(tk2dTextMesh))]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The anchor as a string. \npossible values: LowerLeft,LowerCenter,LowerRight,MiddleLeft,MiddleCenter,MiddleRight,UpperLeft,UpperCenter or UpperRight ")]
		public FsmString textAnchorAsString;

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
			textAnchorAsString = string.Empty;
			everyframe = false;
		}

		public override void OnEnter()
		{
			_getTextMesh();
			DoGetAnchor();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetAnchor();
		}

		private void DoGetAnchor()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component");
			}
			else
			{
				textAnchorAsString.Value = _textMesh.anchor.ToString();
			}
		}
	}
}
