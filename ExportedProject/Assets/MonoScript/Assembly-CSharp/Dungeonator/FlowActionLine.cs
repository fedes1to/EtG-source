using UnityEngine;

namespace Dungeonator
{
	public class FlowActionLine
	{
		public Vector2 point1;

		public Vector2 point2;

		public FlowActionLine(Vector2 p1, Vector2 p2)
		{
			point1 = p1;
			point2 = p2;
		}

		protected bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
		{
			if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) && q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
			{
				return true;
			}
			return false;
		}

		protected int GetOrientation(Vector2 p, Vector2 q, Vector2 r)
		{
			float num = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
			if (num == 0f)
			{
				return 0;
			}
			return (num > 0f) ? 1 : 2;
		}

		public bool Crosses(FlowActionLine other)
		{
			int orientation = GetOrientation(point1, point2, other.point1);
			int orientation2 = GetOrientation(point1, point2, other.point2);
			int orientation3 = GetOrientation(other.point1, other.point2, point1);
			int orientation4 = GetOrientation(other.point1, other.point2, point2);
			if (orientation != orientation2 && orientation3 != orientation4)
			{
				return true;
			}
			if (orientation == 0 && OnSegment(point1, other.point1, point2))
			{
				return true;
			}
			if (orientation2 == 0 && OnSegment(point1, other.point2, point2))
			{
				return true;
			}
			if (orientation3 == 0 && OnSegment(other.point1, point1, other.point2))
			{
				return true;
			}
			if (orientation4 == 0 && OnSegment(other.point1, point2, other.point2))
			{
				return true;
			}
			return false;
		}
	}
}
