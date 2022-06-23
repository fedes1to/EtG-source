using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("GUILayout BeginHorizontal.")]
	[ActionCategory(ActionCategory.GUILayout)]
	public class GUILayoutBeginHorizontal : GUILayoutAction
	{
		public FsmTexture image;

		public FsmString text;

		public FsmString tooltip;

		public FsmString style;

		public override void Reset()
		{
			base.Reset();
			text = string.Empty;
			image = null;
			tooltip = string.Empty;
			style = string.Empty;
		}

		public override void OnGUI()
		{
			GUILayout.BeginHorizontal(new GUIContent(text.Value, image.Value, tooltip.Value), style.Value, base.LayoutOptions);
		}
	}
}
