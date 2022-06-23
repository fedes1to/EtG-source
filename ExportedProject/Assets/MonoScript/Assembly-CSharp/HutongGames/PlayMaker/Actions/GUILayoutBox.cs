using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[Tooltip("GUILayout Box.")]
	[ActionCategory(ActionCategory.GUILayout)]
	public class GUILayoutBox : GUILayoutAction
	{
		[Tooltip("Image to display in the Box.")]
		public FsmTexture image;

		[Tooltip("Text to display in the Box.")]
		public FsmString text;

		[Tooltip("Optional Tooltip string.")]
		public FsmString tooltip;

		[Tooltip("Optional GUIStyle in the active GUISkin.")]
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
			if (string.IsNullOrEmpty(style.Value))
			{
				GUILayout.Box(new GUIContent(text.Value, image.Value, tooltip.Value), base.LayoutOptions);
			}
			else
			{
				GUILayout.Box(new GUIContent(text.Value, image.Value, tooltip.Value), style.Value, base.LayoutOptions);
			}
		}
	}
}
