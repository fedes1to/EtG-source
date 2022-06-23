using System.Collections.Generic;
using UnityEngine;

namespace Dungeonator
{
	public class RoomHandlerBoundingPolygon
	{
		private List<Vector2> m_points;

		public RoomHandlerBoundingPolygon(List<Vector2> points)
		{
			m_points = points;
		}

		public RoomHandlerBoundingPolygon(List<Vector2> points, float inset)
		{
			m_points = new List<Vector2>();
			for (int i = 0; i < points.Count; i++)
			{
				Vector2 vector = points[(i + points.Count - 1) % points.Count];
				Vector2 vector2 = points[i];
				Vector2 vector3 = points[(i + 1) % points.Count];
				Vector2 normalized = (vector2 - vector).normalized;
				normalized = new Vector2(normalized.y, 0f - normalized.x);
				Vector2 normalized2 = (vector3 - vector2).normalized;
				normalized2 = new Vector2(normalized2.y, 0f - normalized2.x);
				Vector2 normalized3 = ((normalized + normalized2) / 2f).normalized;
				m_points.Add(vector2 + normalized3 * inset);
			}
		}

		public bool ContainsPoint(Vector2 point)
		{
			int num = m_points.Count - 1;
			int index = m_points.Count - 1;
			bool flag = false;
			for (num = 0; num < m_points.Count; num++)
			{
				if (((m_points[num].y < point.y && m_points[index].y >= point.y) || (m_points[index].y < point.y && m_points[num].y >= point.y)) && (m_points[num].x <= point.x || m_points[index].x <= point.x))
				{
					flag ^= m_points[num].x + (point.y - m_points[num].y) / (m_points[index].y - m_points[num].y) * (m_points[index].x - m_points[num].x) < point.x;
				}
				index = num;
			}
			return flag;
		}
	}
}
