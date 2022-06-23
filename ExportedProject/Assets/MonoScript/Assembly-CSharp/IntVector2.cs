using System;
using UnityEngine;

[Serializable]
public struct IntVector2
{
	public static IntVector2 Zero = new IntVector2(0, 0);

	public static IntVector2 One = new IntVector2(1, 1);

	public static IntVector2 NegOne = new IntVector2(-1, -1);

	public static IntVector2 Up = new IntVector2(0, 1);

	public static IntVector2 Right = new IntVector2(1, 0);

	public static IntVector2 Down = new IntVector2(0, -1);

	public static IntVector2 Left = new IntVector2(-1, 0);

	public static IntVector2 UpRight = new IntVector2(1, 1);

	public static IntVector2 DownRight = new IntVector2(1, -1);

	public static IntVector2 DownLeft = new IntVector2(-1, -1);

	public static IntVector2 UpLeft = new IntVector2(-1, 1);

	public static IntVector2 North = new IntVector2(0, 1);

	public static IntVector2 East = new IntVector2(1, 0);

	public static IntVector2 South = new IntVector2(0, -1);

	public static IntVector2 West = new IntVector2(-1, 0);

	public static IntVector2 NorthEast = new IntVector2(1, 1);

	public static IntVector2 SouthEast = new IntVector2(1, -1);

	public static IntVector2 SouthWest = new IntVector2(-1, -1);

	public static IntVector2 NorthWest = new IntVector2(-1, 1);

	public static IntVector2 MinValue = new IntVector2(int.MinValue, int.MinValue);

	public static IntVector2 MaxValue = new IntVector2(int.MaxValue, int.MaxValue);

	public static IntVector2[] m_cachedCardinals;

	public static IntVector2[] m_cachedOrdinals;

	public static IntVector2[] m_cachedCardinalsAndOrdinals;

	public int x;

	public int y;

	public static IntVector2[] Cardinals
	{
		get
		{
			if (m_cachedCardinals == null)
			{
				m_cachedCardinals = new IntVector2[4] { North, East, South, West };
			}
			return m_cachedCardinals;
		}
	}

	public static IntVector2[] Ordinals
	{
		get
		{
			if (m_cachedOrdinals == null)
			{
				m_cachedOrdinals = new IntVector2[4] { NorthEast, SouthEast, SouthWest, NorthWest };
			}
			return m_cachedOrdinals;
		}
	}

	public static IntVector2[] CardinalsAndOrdinals
	{
		get
		{
			if (m_cachedCardinalsAndOrdinals == null)
			{
				m_cachedCardinalsAndOrdinals = new IntVector2[8] { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
			}
			return m_cachedCardinalsAndOrdinals;
		}
	}

	public int X
	{
		get
		{
			return x;
		}
	}

	public int Y
	{
		get
		{
			return y;
		}
	}

	public IntVector2 MajorAxis
	{
		get
		{
			if (x == 0 && y == 0)
			{
				return Zero;
			}
			return (Mathf.Abs(x) <= Mathf.Abs(y)) ? new IntVector2(0, Math.Sign(y)) : new IntVector2(Math.Sign(x), 0);
		}
	}

	public int sqrMagnitude
	{
		get
		{
			return x * x + y * y;
		}
	}

	public int ComponentSum
	{
		get
		{
			return Math.Abs(x) + Math.Abs(y);
		}
	}

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public Vector2 ToVector2(float xOffset, float yOffset)
	{
		return new Vector2((float)x + xOffset, (float)y + yOffset);
	}

	public Vector2 ToCenterVector2()
	{
		return new Vector3((float)x + 0.5f, (float)y + 0.5f);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, 0f);
	}

	public Vector3 ToVector3(float height)
	{
		return new Vector3(x, y, height);
	}

	public Vector3 ToCenterVector3(float height)
	{
		return new Vector3((float)x + 0.5f, (float)y + 0.5f, height);
	}

	public bool IsWithin(IntVector2 min, IntVector2 max)
	{
		return x >= min.x && x <= max.x && y >= min.y && y <= max.y;
	}

	public override string ToString()
	{
		return string.Format("{0},{1}", x, y);
	}

	public bool Equals(IntVector2 other)
	{
		return x == other.x && y == other.y;
	}

	public override bool Equals(object obj)
	{
		if (obj is IntVector2)
		{
			return this == (IntVector2)obj;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return 100267 * x + 200233 * y;
	}

	public float GetHashedRandomValue()
	{
		int num = 0;
		num += x;
		num += num << 10;
		num ^= num >> 6;
		num += y;
		num += num << 10;
		num ^= num >> 6;
		num += num << 3;
		num ^= num >> 11;
		num += num << 15;
		uint num2 = (uint)num;
		return (float)num2 * 1f / 4.2949673E+09f;
	}

	public IntVector2 WithX(int newX)
	{
		return new IntVector2(newX, y);
	}

	public IntVector2 WithY(int newY)
	{
		return new IntVector2(x, newY);
	}

	public static IntVector2 operator +(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x + b.x, a.y + b.y);
	}

	public static IntVector2 operator -(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x - b.x, a.y - b.y);
	}

	public static IntVector2 operator *(IntVector2 a, int b)
	{
		return new IntVector2(a.x * b, a.y * b);
	}

	public static IntVector2 operator *(int a, IntVector2 b)
	{
		return new IntVector2(a * b.x, a * b.y);
	}

	public static IntVector2 operator /(IntVector2 a, int b)
	{
		return new IntVector2(a.x / b, a.y / b);
	}

	public static IntVector2 operator -(IntVector2 a)
	{
		return new IntVector2(-a.x, -a.y);
	}

	public static bool operator ==(IntVector2 a, IntVector2 b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator !=(IntVector2 a, IntVector2 b)
	{
		return a.x != b.x || a.y != b.y;
	}

	public static int ManhattanDistance(IntVector2 a, IntVector2 b)
	{
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}

	public static int ManhattanDistance(IntVector2 a, int bx, int by)
	{
		return Math.Abs(a.x - bx) + Math.Abs(a.y - by);
	}

	public static int ManhattanDistance(int ax, int ay, int bx, int by)
	{
		return Math.Abs(ax - bx) + Math.Abs(ay - by);
	}

	public static float Distance(IntVector2 a, IntVector2 b)
	{
		return Mathf.Sqrt((b.y - a.y) * (b.y - a.y) + (b.x - a.x) * (b.x - a.x));
	}

	public static float Distance(IntVector2 a, int bx, int by)
	{
		return Mathf.Sqrt((by - a.y) * (by - a.y) + (bx - a.x) * (bx - a.x));
	}

	public static float Distance(int ax, int ay, int bx, int by)
	{
		return Mathf.Sqrt((by - ay) * (by - ay) + (bx - ax) * (bx - ax));
	}

	public static float DistanceSquared(IntVector2 a, IntVector2 b)
	{
		return (b.y - a.y) * (b.y - a.y) + (b.x - a.x) * (b.x - a.x);
	}

	public static float DistanceSquared(IntVector2 a, int bx, int by)
	{
		return (by - a.y) * (by - a.y) + (bx - a.x) * (bx - a.x);
	}

	public static float DistanceSquared(int ax, int ay, int bx, int by)
	{
		return (by - ay) * (by - ay) + (bx - ax) * (bx - ax);
	}

	public void Clamp(IntVector2 min, IntVector2 max)
	{
		x = Math.Max(x, min.x);
		y = Math.Max(y, min.y);
		x = Math.Min(x, max.x);
		y = Math.Min(y, max.y);
	}

	public static IntVector2 Scale(IntVector2 lhs, IntVector2 rhs)
	{
		return new IntVector2(lhs.x * rhs.x, lhs.y * rhs.y);
	}

	public static IntVector2 Min(IntVector2 lhs, IntVector2 rhs)
	{
		return new IntVector2(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
	}

	public static IntVector2 Max(IntVector2 lhs, IntVector2 rhs)
	{
		return new IntVector2(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
	}

	public static bool AABBOverlap(IntVector2 posA, IntVector2 dimensionsA, IntVector2 posB, IntVector2 dimensionsB)
	{
		if (posA.x + dimensionsA.x - 1 < posB.x || posA.x > posB.x + dimensionsB.x - 1 || posA.y + dimensionsA.y - 1 < posB.y || posA.y > posB.y + dimensionsB.y - 1)
		{
			return false;
		}
		return true;
	}

	public static bool AABBOverlapWithArea(IntVector2 posA, IntVector2 dimensionsA, IntVector2 posB, IntVector2 dimensionsB, out int cellsOverlapping)
	{
		if (posA.x + dimensionsA.x - 1 < posB.x || posA.x > posB.x + dimensionsB.x - 1 || posA.y + dimensionsA.y - 1 < posB.y || posA.y > posB.y + dimensionsB.y - 1)
		{
			cellsOverlapping = 0;
			return false;
		}
		int num = Mathf.Max(0, Mathf.Min(posA.x + dimensionsA.x, posB.x + dimensionsB.x) - Mathf.Max(posA.x, posB.x));
		int num2 = Mathf.Max(0, Mathf.Min(posA.y + dimensionsA.y, posB.y + dimensionsB.y) - Mathf.Max(posA.y, posB.y));
		cellsOverlapping = num * num2;
		return true;
	}

	public static explicit operator Vector2(IntVector2 v)
	{
		return new Vector2(v.x, v.y);
	}
}
