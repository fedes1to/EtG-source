using UnityEngine;

namespace com.subjectnerd
{
	public class GLDraw
	{
		protected static bool clippingEnabled;

		protected static Rect clippingBounds;

		public static Material lineMaterial;

		protected static bool clip_test(float p, float q, ref float u1, ref float u2)
		{
			bool result = true;
			if ((double)p < 0.0)
			{
				float num = q / p;
				if (num > u2)
				{
					result = false;
				}
				else if (num > u1)
				{
					u1 = num;
				}
			}
			else if ((double)p > 0.0)
			{
				float num = q / p;
				if (num < u1)
				{
					result = false;
				}
				else if (num < u2)
				{
					u2 = num;
				}
			}
			else if ((double)q < 0.0)
			{
				result = false;
			}
			return result;
		}

		protected static bool segment_rect_intersection(Rect bounds, ref Vector2 p1, ref Vector2 p2)
		{
			float u = 0f;
			float u2 = 1f;
			float num = p2.x - p1.x;
			if (clip_test(0f - num, p1.x - bounds.xMin, ref u, ref u2) && clip_test(num, bounds.xMax - p1.x, ref u, ref u2))
			{
				float num2 = p2.y - p1.y;
				if (clip_test(0f - num2, p1.y - bounds.yMin, ref u, ref u2) && clip_test(num2, bounds.yMax - p1.y, ref u, ref u2))
				{
					if ((double)u2 < 1.0)
					{
						p2.x = p1.x + u2 * num;
						p2.y = p1.y + u2 * num2;
					}
					if ((double)u > 0.0)
					{
						p1.x += u * num;
						p1.y += u * num2;
					}
					return true;
				}
			}
			return false;
		}

		public static void BeginGroup(Rect position)
		{
			clippingEnabled = true;
			clippingBounds = new Rect(0f, 0f, position.width, position.height);
			GUI.BeginGroup(position);
		}

		public static void EndGroup()
		{
			GUI.EndGroup();
			clippingBounds = new Rect(0f, 0f, Screen.width, Screen.height);
			clippingEnabled = false;
		}

		public static void CreateMaterial()
		{
			if (lineMaterial == null)
			{
				lineMaterial = new Material(ShaderCache.Acquire("Brave/DebugLines"));
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
				lineMaterial.shader.hideFlags = HideFlags.None;
			}
		}

		public static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
		{
			if (Event.current != null && Event.current.type == EventType.Repaint && (!clippingEnabled || segment_rect_intersection(clippingBounds, ref start, ref end)))
			{
				CreateMaterial();
				lineMaterial.SetPass(0);
				if (width == 1f)
				{
					GL.Begin(1);
					GL.Color(color);
					Vector3 v = new Vector3(start.x, start.y, 0f);
					Vector3 v2 = new Vector3(end.x, end.y, 0f);
					GL.Vertex(v);
					GL.Vertex(v2);
				}
				else
				{
					GL.Begin(7);
					GL.Color(color);
					Vector3 v = new Vector3(end.y, start.x, 0f);
					Vector3 v2 = new Vector3(start.y, end.x, 0f);
					Vector3 vector = (v - v2).normalized * width;
					Vector3 vector2 = new Vector3(start.x, start.y, 0f);
					Vector3 vector3 = new Vector3(end.x, end.y, 0f);
					GL.Vertex(vector2 - vector);
					GL.Vertex(vector2 + vector);
					GL.Vertex(vector3 + vector);
					GL.Vertex(vector3 - vector);
				}
				GL.End();
			}
		}

		public static void DrawBox(Rect box, Color color, float width)
		{
			Vector2 vector = new Vector2(box.xMin, box.yMin);
			Vector2 vector2 = new Vector2(box.xMax, box.yMin);
			Vector2 vector3 = new Vector2(box.xMax, box.yMax);
			Vector2 vector4 = new Vector2(box.xMin, box.yMax);
			DrawLine(vector, vector2, color, width);
			DrawLine(vector2, vector3, color, width);
			DrawLine(vector3, vector4, color, width);
			DrawLine(vector4, vector, color, width);
		}

		public static void DrawBox(Vector2 topLeftCorner, Vector2 bottomRightCorner, Color color, float width)
		{
			Rect box = new Rect(topLeftCorner.x, topLeftCorner.y, bottomRightCorner.x - topLeftCorner.x, bottomRightCorner.y - topLeftCorner.y);
			DrawBox(box, color, width);
		}

		public static void DrawRoundedBox(Rect box, float radius, Color color, float width)
		{
			Vector2 vector = new Vector2(box.xMin + radius, box.yMin);
			Vector2 vector2 = new Vector2(box.xMax - radius, box.yMin);
			Vector2 vector3 = new Vector2(box.xMax, box.yMin + radius);
			Vector2 vector4 = new Vector2(box.xMax, box.yMax - radius);
			Vector2 vector5 = new Vector2(box.xMax - radius, box.yMax);
			Vector2 vector6 = new Vector2(box.xMin + radius, box.yMax);
			Vector2 vector7 = new Vector2(box.xMin, box.yMax - radius);
			Vector2 vector8 = new Vector2(box.xMin, box.yMin + radius);
			DrawLine(vector, vector2, color, width);
			DrawLine(vector3, vector4, color, width);
			DrawLine(vector5, vector6, color, width);
			DrawLine(vector7, vector8, color, width);
			float num = radius / 2f;
			DrawBezier(startTangent: new Vector2(vector8.x, vector8.y + num), endTangent: new Vector2(vector.x - num, vector.y), start: vector8, end: vector, color: color, width: width);
			DrawBezier(startTangent: new Vector2(vector2.x + num, vector2.y), endTangent: new Vector2(vector3.x, vector3.y - num), start: vector2, end: vector3, color: color, width: width);
			DrawBezier(startTangent: new Vector2(vector4.x, vector4.y + num), endTangent: new Vector2(vector5.x + num, vector5.y), start: vector4, end: vector5, color: color, width: width);
			DrawBezier(startTangent: new Vector2(vector6.x - num, vector6.y), endTangent: new Vector2(vector7.x, vector7.y + num), start: vector6, end: vector7, color: color, width: width);
		}

		public static void DrawConnectingCurve(Vector2 start, Vector2 end, Color color, float width)
		{
			Vector2 vector = start - end;
			Vector2 startTangent = start;
			startTangent.x -= (vector / 2f).x;
			Vector2 endTangent = end;
			endTangent.x += (vector / 2f).x;
			int segments = Mathf.FloorToInt(vector.magnitude / 20f * 3f);
			DrawBezier(start, startTangent, end, endTangent, color, width, segments);
		}

		public static void DrawBezier(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width)
		{
			int segments = Mathf.FloorToInt((start - end).magnitude / 20f) * 3;
			DrawBezier(start, startTangent, end, endTangent, color, width, segments);
		}

		public static void DrawBezier(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, int segments)
		{
			Vector2 start2 = CubeBezier(start, startTangent, end, endTangent, 0f);
			for (int i = 1; i <= segments; i++)
			{
				Vector2 vector = CubeBezier(start, startTangent, end, endTangent, (float)i / (float)segments);
				DrawLine(start2, vector, color, width);
				start2 = vector;
			}
		}

		private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
		{
			float num = 1f - t;
			float num2 = num * t;
			return num * num * num * s + 3f * num * num2 * st + 3f * num2 * t * et + t * t * t * e;
		}
	}
}
