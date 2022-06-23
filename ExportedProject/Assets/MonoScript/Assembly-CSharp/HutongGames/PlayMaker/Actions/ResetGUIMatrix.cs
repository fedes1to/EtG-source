using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("Resets the GUI matrix. Useful if you've rotated or scaled the GUI and now want to reset it.")]
	[ActionCategory(ActionCategory.GUI)]
	public class ResetGUIMatrix : FsmStateAction
	{
		public override void OnGUI()
		{
			Matrix4x4 matrix4x2 = (PlayMakerGUI.GUIMatrix = (GUI.matrix = Matrix4x4.identity));
		}
	}
}
