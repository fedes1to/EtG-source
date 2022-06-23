using System;
using UnityEngine;

namespace FullInspector.Internal
{
	public static class fiCommentUtility
	{
		public static int GetCommentHeight(string comment, CommentType commentType)
		{
			int val = 38;
			if (commentType == CommentType.None)
			{
				val = 17;
			}
			GUIStyle gUIStyle = "HelpBox";
			return Math.Max((int)gUIStyle.CalcHeight(new GUIContent(comment), Screen.width), val);
		}
	}
}
