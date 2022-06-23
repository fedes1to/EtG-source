using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[HelpUrl("https://hutonggames.fogbugz.com/default.asp?W723")]
	[Tooltip("Commit a TextMesh. This is so you can change multiple parameters without reconstructing the mesh repeatedly, simply use that after you have set all the different properties. \nNOTE: The Game Object must have a tk2dTextMesh attached.")]
	[ActionCategory("2D Toolkit/TextMesh")]
	public class Tk2dTextMeshCommit : FsmStateAction
	{
		[CheckForComponent(typeof(tk2dTextMesh))]
		[Tooltip("The Game Object to work with. NOTE: The Game Object must have a tk2dTextMesh component attached.")]
		[RequiredField]
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
			DoCommit();
			Finish();
		}

		private void DoCommit()
		{
			if (_textMesh == null)
			{
				LogWarning("Missing tk2dTextMesh component: " + _textMesh.gameObject.name);
			}
			else
			{
				_textMesh.Commit();
			}
		}
	}
}
