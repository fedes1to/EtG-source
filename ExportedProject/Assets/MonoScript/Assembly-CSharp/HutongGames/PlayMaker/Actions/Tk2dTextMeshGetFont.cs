using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Get the font of a TextMesh. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	[ActionCategory("2D Toolkit/TextMesh")]
	public class Tk2dTextMeshGetFont : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dTextMesh))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.FsmGameObject)]
		[Tooltip("The font gameObject")]
		[RequiredField]
		public FsmGameObject font;

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
			font = null;
		}

		public override void OnEnter()
		{
			_getTextMesh();
			DoGetFont();
			Finish();
		}

		private void DoGetFont()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component: " + _textMesh.gameObject.name);
				return;
			}
			GameObject value = font.Value;
			if (!(value == null))
			{
				tk2dFont component = value.GetComponent<tk2dFont>();
				if (!(component == null))
				{
				}
			}
		}
	}
}
