using System;
using UnityEngine;

public class IntRect
{
	private int x;

	private int y;

	private int dimX;

	private int dimY;

	public IntVector2 Dimensions
	{
		get
		{
			return new IntVector2(Width, Height);
		}
	}

	public float Aspect
	{
		get
		{
			return (float)Width / (float)Height;
		}
	}

	public int Area
	{
		get
		{
			return Width * Height;
		}
	}

	public IntVector2[] FourPoints
	{
		get
		{
			return new IntVector2[4]
			{
				new IntVector2(x, y),
				new IntVector2(x, y + dimY),
				new IntVector2(x + dimX, y + dimY),
				new IntVector2(x + dimX, y)
			};
		}
	}

	public IntVector2 Min
	{
		get
		{
			return new IntVector2(x, y);
		}
	}

	public IntVector2 Max
	{
		get
		{
			return new IntVector2(x + dimX, y + dimY);
		}
	}

	public int Left
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public int Top
	{
		get
		{
			return y + dimY;
		}
	}

	public int Right
	{
		get
		{
			return x + dimX;
		}
	}

	public int Bottom
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public int Width
	{
		get
		{
			return dimX;
		}
	}

	public int Height
	{
		get
		{
			return dimY;
		}
	}

	public int Metric
	{
		get
		{
			return Math.Max(Width, Height);
		}
	}

	public Vector2 Center
	{
		get
		{
			return new Vector2((float)x + (float)dimX / 2f, (float)y + (float)dimY / 2f);
		}
	}

	public IntRect(int left, int bottom, int width, int height)
	{
		x = left;
		y = bottom;
		dimX = width;
		dimY = height;
	}

	public static IntRect FromTwoPoints(IntVector2 p1, IntVector2 p2)
	{
		IntVector2 intVector = IntVector2.Min(p1, p2);
		IntVector2 intVector2 = IntVector2.Max(p1, p2);
		IntVector2 intVector3 = intVector2 - intVector;
		return new IntRect(intVector.x, intVector.y, intVector3.x, intVector3.y);
	}

	public static IntRect Intersection(IntRect a, IntRect b, int expand = 0)
	{
		int num = Math.Max(a.x - expand, b.x - expand);
		int num2 = Math.Max(a.y - expand, b.y - expand);
		int num3 = Math.Min(a.Right + expand, b.Right + expand) - num;
		int num4 = Math.Min(a.Top + expand, b.Top + expand) - num2;
		if (num4 <= 0 || num3 <= 0)
		{
			return null;
		}
		return new IntRect(num, num2, num3, num4);
	}

	public bool Contains(Vector2 vec)
	{
		return vec.x >= (float)x && vec.x <= (float)(x + dimX) && vec.y >= (float)y && vec.y <= (float)(y + dimY);
	}
}
