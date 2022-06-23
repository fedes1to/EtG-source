using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("2D Toolkit/TextMesh")]
	[Tooltip("Get the number of drawn characters of a TextMesh. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	public class Tk2dTextMeshGetNumDrawnCharacters : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dTextMesh))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		[RequiredField]
		public FsmOwnerDefault gameObject;

		[UIHint(UIHint.Variable)]
		[RequiredField]
		[Tooltip("The number of drawn characters")]
		public FsmInt numDrawnCharacters;

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
			numDrawnCharacters = null;
			everyframe = false;
		}

		public override void OnEnter()
		{
			_getTextMesh();
			DoGetNumDrawnCharacters();
			if (!everyframe)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoGetNumDrawnCharacters();
		}

		private void DoGetNumDrawnCharacters()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component");
			}
			else
			{
				numDrawnCharacters.Value = _textMesh.NumDrawnCharacters();
			}
		}
	}
}
