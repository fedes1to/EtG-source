using UnityEngine;

public static class dfRectExtensions
{
	public static RectOffset ConstrainPadding(this RectOffset borders)
	{
		if (borders == null)
		{
			return new RectOffset();
		}
		borders.left = Mathf.Max(0, borders.left);
		borders.right = Mathf.Max(0, borders.right);
		borders.top = Mathf.Max(0, borders.top);
		borders.bottom = Mathf.Max(0, borders.bottom);
		return borders;
	}

	public static bool IsEmpty(this Rect rect)
	{
		return rect.xMin == rect.xMax || rect.yMin == rect.yMax;
	}

	public static Rect Intersection(this Rect a, Rect b)
	{
		if (!a.Intersects(b))
		{
			return default(Rect);
		}
		float xmin = Mathf.Max(a.xMin, b.xMin);
		float xmax = Mathf.Min(a.xMax, b.xMax);
		float ymin = Mathf.Max(a.yMin, b.yMin);
		float ymax = Mathf.Min(a.yMax, b.yMax);
		return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
	}

	public static Rect Union(this Rect a, Rect b)
	{
		float xmin = Mathf.Min(a.xMin, b.xMin);
		float xmax = Mathf.Max(a.xMax, b.xMax);
		float ymin = Mathf.Min(a.yMin, b.yMin);
		float ymax = Mathf.Max(a.yMax, b.yMax);
		return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
	}

	public static bool Contains(this Rect rect, Rect other)
	{
		bool flag = rect.x <= other.x;
		bool flag2 = rect.x + rect.width >= other.x + other.width;
		bool flag3 = rect.yMin <= other.yMin;
		bool flag4 = rect.y + rect.height >= other.y + other.height;
		return flag && flag2 && flag3 && flag4;
	}

	public static bool Intersects(this Rect rect, Rect other)
	{
		bool flag = rect.xMax < other.xMin || rect.yMax < other.yMin || rect.xMin > other.xMax || rect.yMin > other.yMax;
		return !flag;
	}

	public static Rect RoundToInt(this Rect rect)
	{
		return new Rect(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height));
	}

	public static string Debug(this Rect rect)
	{
		return string.Format("[{0},{1},{2},{3}]", rect.xMin, rect.yMin, rect.xMax, rect.yMax);
	}
}
