using UnityEngine;

public static class TransformExtensions
{
	public static Vector2 PositionVector2(this Transform t)
	{
		return new Vector2(t.position.x, t.position.y);
	}

	public static void MovePixelsWorld(this Transform t, IntVector2 offset)
	{
		t.MovePixelsWorld(offset.x, offset.y);
	}

	public static void MovePixelsWorld(this Transform t, int x, int y)
	{
		t.position += new Vector3((float)x * 0.0625f, (float)y * 0.0625f, 0f);
	}

	public static void MovePixelsWorld(this Transform t, int x, int y, int z)
	{
		t.position += new Vector3((float)x * 0.0625f, (float)y * 0.0625f, (float)z * 0.0625f);
	}

	public static void MovePixelsLocal(this Transform t, IntVector2 offset)
	{
		t.MovePixelsLocal(offset.x, offset.y);
	}

	public static void MovePixelsLocal(this Transform t, int x, int y)
	{
		t.localPosition += new Vector3((float)x * 0.0625f, (float)y * 0.0625f, 0f);
	}

	public static void MovePixelsLocal(this Transform t, int x, int y, int z)
	{
		t.localPosition += new Vector3((float)x * 0.0625f, (float)y * 0.0625f, (float)z * 0.0625f);
	}

	public static Transform GetFirstLeafChild(this Transform t)
	{
		Transform transform = t;
		while (transform.childCount > 0)
		{
			transform = transform.GetChild(0);
		}
		return transform;
	}
}
