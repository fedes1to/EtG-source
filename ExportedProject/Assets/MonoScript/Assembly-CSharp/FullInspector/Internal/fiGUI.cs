using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Internal
{
	public static class fiGUI
	{
		private static readonly List<float> s_regionWidths = new List<float>();

		private static readonly Stack<float> s_savedLabelWidths = new Stack<float>();

		public static float PushLabelWidth(GUIContent controlLabel, float controlWidth)
		{
			s_regionWidths.Add(controlWidth);
			s_savedLabelWidths.Push(controlWidth);
			return ComputeActualLabelWidth(s_regionWidths[0], controlLabel, controlWidth);
		}

		public static float PopLabelWidth()
		{
			s_regionWidths.RemoveAt(s_regionWidths.Count - 1);
			return s_savedLabelWidths.Pop();
		}

		public static float ComputeActualLabelWidth(float inspectorWidth, GUIContent controlLabel, float controlWidth)
		{
			float num = inspectorWidth - controlWidth;
			float num2 = Mathf.Max(inspectorWidth * fiSettings.LabelWidthPercentage - fiSettings.LabelWidthOffset, 120f);
			float value = num2 - num;
			float min = Mathf.Max(fiLateBindings.EditorStyles.label.CalcSize(controlLabel).x, fiSettings.LabelWidthMin);
			return Mathf.Clamp(value, min, fiSettings.LabelWidthMax);
		}
	}
}
