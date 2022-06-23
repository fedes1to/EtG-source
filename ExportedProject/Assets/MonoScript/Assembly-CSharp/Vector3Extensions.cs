using Dungeonator;
using UnityEngine;

public static class Vector3Extensions
{
	public static Vector3 WithX(this Vector3 vector, float x)
	{
		return new Vector3(x, vector.y, vector.z);
	}

	public static Vector3 WithY(this Vector3 vector, float y)
	{
		return new Vector3(vector.x, y, vector.z);
	}

	public static Vector3 WithZ(this Vector3 vector, float z)
	{
		return new Vector3(vector.x, vector.y, z);
	}

	public static Color WithAlpha(this Color color, float alpha)
	{
		return new Color(color.r, color.g, color.b, alpha);
	}

	public static Vector3 RotateBy(this Vector3 vector, Quaternion rotation)
	{
		return rotation * vector;
	}

	public static Vector4 ToVector4(this Vector3 vector)
	{
		return new Vector4(vector.x, vector.y, vector.z, 0f);
	}

	public static Vector2 XY(this Vector3 vector)
	{
		return new Vector2(vector.x, vector.y);
	}

	public static Vector2 YZ(this Vector3 vector)
	{
		return new Vector2(vector.y, vector.z);
	}

	public static Vector2 XZ(this Vector3 vector)
	{
		return new Vector2(vector.x, vector.z);
	}

	public static IntVector2 IntXY(this Vector3 vector, VectorConversions convert = VectorConversions.Round)
	{
		switch (convert)
		{
		case VectorConversions.Floor:
			return new IntVector2(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y));
		case VectorConversions.Ceil:
			return new IntVector2(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y));
		case VectorConversions.Round:
			return new IntVector2(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
		default:
			BraveUtility.Log(string.Format("Called IntXY() with an unknown conversion type ({0})", convert), Color.red, BraveUtility.LogVerbosity.IMPORTANT);
			return IntVector2.Zero;
		}
	}

	public static bool IsWithin(this Vector3 vector, Vector3 min, Vector3 max)
	{
		return vector.x >= min.x && vector.x <= max.x && vector.y >= min.y && vector.y <= max.y;
	}

	public static CellData GetCell(this Vector2 vector)
	{
		return GameManager.Instance.Dungeon.data[vector.ToIntVector2(VectorConversions.Floor)];
	}

	public static CellData GetCell(this Vector3 vector)
	{
		if (!GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(vector.IntXY(VectorConversions.Floor)))
		{
			return null;
		}
		return GameManager.Instance.Dungeon.data[vector.IntXY(VectorConversions.Floor)];
	}

	public static RoomHandler GetAbsoluteRoom(this Vector2 vector)
	{
		return GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(vector.ToIntVector2(VectorConversions.Floor));
	}

	public static RoomHandler GetAbsoluteRoom(this Vector3 vector)
	{
		return GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(vector.IntXY(VectorConversions.Floor));
	}
}
