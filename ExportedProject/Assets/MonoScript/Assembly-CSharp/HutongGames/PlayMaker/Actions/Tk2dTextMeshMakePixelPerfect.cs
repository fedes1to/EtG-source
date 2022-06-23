using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Make a TextMesh pixelPerfect. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	[ActionCategory("2D Toolkit/TextMesh")]
	public class Tk2dTextMeshMakePixelPerfect : FsmStateAction
	{
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		[RequiredField]
		[CheckForComponent(typeof(tk2dTextMesh))]
		public FsmOwnerDefault gameObject;

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
		}

		public override void OnEnter()
		{
			_getTextMesh();
			MakePixelPerfect();
			Finish();
		}

		private void MakePixelPerfect()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component ");
			}
			else
			{
				_textMesh.MakePixelPerfect();
			}
		}
	}
}
