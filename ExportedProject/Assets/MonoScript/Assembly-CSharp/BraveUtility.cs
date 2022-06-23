using System;
using System.Collections.Generic;
using UnityEngine;

public static class BraveUtility
{
	public enum LogVerbosity
	{
		NONE,
		IMPORTANT,
		CHATTY,
		VERBOSE
	}

	private const float c_screenWidthTiles = 30f;

	private const float c_screenHeightTiles = 16.875f;

	public static LogVerbosity verbosity = LogVerbosity.IMPORTANT;

	public static bool isLoadingLevel
	{
		get
		{
			return Application.isLoadingLevel;
		}
	}

	public static void DrawDebugSquare(Vector2 min, Color color)
	{
		DrawDebugSquare(min.x, min.x + 1f, min.y, min.y + 1f, color);
	}

	public static void DrawDebugSquare(Vector2 min, Vector2 max, Color color)
	{
		DrawDebugSquare(min.x, max.x, min.y, max.y, color);
	}

	public static void DrawDebugSquare(float minX, float maxX, float minY, float maxY, Color color)
	{
		Debug.DrawLine(new Vector3(minX, minY, 0f), new Vector3(maxX, minY, 0f), color);
		Debug.DrawLine(new Vector3(minX, maxY, 0f), new Vector3(maxX, maxY, 0f), color);
		Debug.DrawLine(new Vector3(minX, minY, 0f), new Vector3(minX, maxY, 0f), color);
		Debug.DrawLine(new Vector3(maxX, minY, 0f), new Vector3(maxX, maxY, 0f), color);
	}

	public static void DrawDebugSquare(Vector2 min, Color color, float duration)
	{
		DrawDebugSquare(min.x, min.x + 1f, min.y, min.y + 1f, color, duration);
	}

	public static void DrawDebugSquare(Vector2 min, Vector2 max, Color color, float duration)
	{
		DrawDebugSquare(min.x, max.x, min.y, max.y, color, duration);
	}

	public static void DrawDebugSquare(float minX, float maxX, float minY, float maxY, Color color, float duration)
	{
		Debug.DrawLine(new Vector3(minX, minY, 0f), new Vector3(maxX, minY, 0f), color, duration);
		Debug.DrawLine(new Vector3(minX, maxY, 0f), new Vector3(maxX, maxY, 0f), color, duration);
		Debug.DrawLine(new Vector3(minX, minY, 0f), new Vector3(minX, maxY, 0f), color, duration);
		Debug.DrawLine(new Vector3(maxX, minY, 0f), new Vector3(maxX, maxY, 0f), color, duration);
	}

	public static Vector3 GetMousePosition()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter;
		new Plane(Vector3.back, Vector3.zero).Raycast(ray, out enter);
		return ray.GetPoint(enter);
	}

	public static Vector3 ViewportToWorldpoint(Vector2 viewportPos, ViewportType viewportType)
	{
		switch (viewportType)
		{
		case ViewportType.Camera:
		{
			Ray ray = Camera.main.ViewportPointToRay(viewportPos);
			float enter;
			new Plane(Vector3.back, Vector3.zero).Raycast(ray, out enter);
			return ray.GetPoint(enter);
		}
		case ViewportType.Gameplay:
		{
			Vector2 vector = ScreenCenterWorldPoint();
			Vector2 vector2 = new Vector2(30f * (viewportPos.x - 0.5f), 16.875f * (viewportPos.y - 0.5f));
			float overrideZoomScale = GameManager.Instance.MainCameraController.OverrideZoomScale;
			if (overrideZoomScale != 1f && overrideZoomScale != 0f)
			{
				vector2 /= overrideZoomScale;
			}
			return vector + vector2;
		}
		default:
			throw new ArgumentException("Unknown viewport type: " + viewportType);
		}
	}

	public static Vector2 WorldPointToViewport(Vector3 worldPoint, ViewportType viewportType)
	{
		switch (viewportType)
		{
		case ViewportType.Camera:
			return Camera.main.WorldToViewportPoint(worldPoint);
		case ViewportType.Gameplay:
		{
			Vector2 vector = ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay);
			Vector2 vector2 = ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay);
			Vector2 vector3 = vector2 - vector;
			return new Vector2((worldPoint.x - vector.x) / vector3.x, (worldPoint.y - vector.y) / vector3.y);
		}
		default:
			throw new ArgumentException("Unknown viewport type: " + viewportType);
		}
	}

	public static Vector3 ScreenCenterWorldPoint()
	{
		return ViewportToWorldpoint(new Vector2(0.5f, 0.5f), ViewportType.Camera);
	}

	public static bool PointIsVisible(Vector2 flatPoint, float percentBuffer, ViewportType viewportType)
	{
		Vector2 vector = ViewportToWorldpoint(new Vector2(0f, 0f), viewportType);
		Vector2 vector2 = ViewportToWorldpoint(new Vector2(1f, 1f), viewportType);
		Vector2 vector3 = (vector2 - vector) * percentBuffer;
		return flatPoint.x > vector.x - vector3.x && flatPoint.x < vector2.x + vector3.x && flatPoint.y > vector.y - vector3.y && flatPoint.y < vector2.y + vector3.y;
	}

	public static Vector3 GetMinimapViewportPosition(Vector2 pos)
	{
		float num = pos.x / (float)Screen.width;
		float num2 = pos.y / (float)Screen.height;
		num = (num - 0.5f) / BraveCameraUtility.GetRect().width + 0.5f;
		num2 = (num2 - 0.5f) / BraveCameraUtility.GetRect().height + 0.5f;
		return new Vector2(num, num2);
	}

	public static Vector2[] ResizeArray(Vector2[] a, int size)
	{
		Vector2[] array = new Vector2[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = a[i];
		}
		return array;
	}

	public static Vector2 GetClosestPoint(Vector2 a, Vector2 b, Vector2 p)
	{
		Vector2 lhs = p - a;
		Vector2 vector = b - a;
		float num = Vector2.Dot(lhs, vector) / vector.sqrMagnitude;
		return a + vector * num;
	}

	public static bool LineIntersectsLine(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
	{
		intersection = Vector2.zero;
		Vector2 vector = a2 - a1;
		Vector2 vector2 = b2 - b1;
		float num = vector.x * vector2.y - vector.y * vector2.x;
		if (num == 0f)
		{
			return false;
		}
		Vector2 vector3 = b1 - a1;
		float num2 = (vector3.x * vector2.y - vector3.y * vector2.x) / num;
		if (num2 < 0f || num2 > 1f)
		{
			return false;
		}
		float num3 = (vector3.x * vector.y - vector3.y * vector.x) / num;
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		intersection = a1 + num2 * vector;
		return true;
	}

	public static bool LineIntersectsAABB(Vector2 l1, Vector2 l2, Vector2 bOrigin, Vector2 bSize, out Vector2 intersection)
	{
		intersection = default(Vector2);
		float num = float.MaxValue;
		Vector2 intersection2;
		if (LineIntersectsLine(l1, l2, bOrigin, bOrigin + new Vector2(0f, bSize.y), out intersection2))
		{
			float sqrMagnitude = (l1 - intersection2).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				intersection = intersection2;
				num = sqrMagnitude;
			}
		}
		if (LineIntersectsLine(l1, l2, bOrigin + new Vector2(0f, bSize.y), bOrigin + bSize, out intersection2))
		{
			float sqrMagnitude2 = (l1 - intersection2).sqrMagnitude;
			if (sqrMagnitude2 < num)
			{
				intersection = intersection2;
				num = sqrMagnitude2;
			}
		}
		if (LineIntersectsLine(l1, l2, bOrigin + bSize, bOrigin + new Vector2(bSize.x, 0f), out intersection2))
		{
			float sqrMagnitude3 = (l1 - intersection2).sqrMagnitude;
			if (sqrMagnitude3 < num)
			{
				intersection = intersection2;
				num = sqrMagnitude3;
			}
		}
		if (LineIntersectsLine(l1, l2, bOrigin + new Vector2(bSize.x, 0f), bOrigin, out intersection2))
		{
			float sqrMagnitude4 = (l1 - intersection2).sqrMagnitude;
			if (sqrMagnitude4 < num)
			{
				intersection = intersection2;
				num = sqrMagnitude4;
			}
		}
		return num != float.MaxValue;
	}

	public static bool GreaterThanAlongMajorAxis(Vector2 lhs, Vector2 rhs, Vector2 axis)
	{
		Vector2 majorAxis = GetMajorAxis(axis);
		Vector2 vector = Vector2.Scale(lhs, majorAxis);
		Vector2 vector2 = Vector2.Scale(rhs, majorAxis);
		float num = lhs.x + lhs.y;
		float num2 = rhs.x + rhs.y;
		return num > num2;
	}

	public static Vector2 GetMajorAxis(Vector2 vector)
	{
		if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
		{
			return new Vector2(Mathf.Sign(vector.x), 0f);
		}
		return new Vector2(0f, Mathf.Sign(vector.y));
	}

	public static IntVector2 GetMajorAxis(IntVector2 vector)
	{
		if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
		{
			return new IntVector2(Math.Sign(vector.x), 0);
		}
		return new IntVector2(0, Math.Sign(vector.y));
	}

	public static IntVector2 GetIntMajorAxis(IntVector2 vector)
	{
		return GetIntMajorAxis(vector.ToVector2());
	}

	public static IntVector2 GetIntMajorAxis(Vector2 vector)
	{
		if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
		{
			return new IntVector2(Math.Sign(vector.x), 0);
		}
		return new IntVector2(0, Math.Sign(vector.y));
	}

	public static Vector2 GetMinorAxis(Vector2 vector)
	{
		if (Mathf.Abs(vector.x) <= Mathf.Abs(vector.y))
		{
			return new Vector2(Mathf.Sign(vector.x), 0f);
		}
		return new Vector2(0f, Mathf.Sign(vector.y));
	}

	public static IntVector2 GetMinorAxis(IntVector2 vector)
	{
		if (Mathf.Abs(vector.x) <= Mathf.Abs(vector.y))
		{
			return new IntVector2(Math.Sign(vector.x), 0);
		}
		return new IntVector2(0, Math.Sign(vector.y));
	}

	public static IntVector2 GetIntMinorAxis(Vector2 vector)
	{
		if (Mathf.Abs(vector.x) <= Mathf.Abs(vector.y))
		{
			return new IntVector2(Math.Sign(vector.x), 0);
		}
		return new IntVector2(0, Math.Sign(vector.y));
	}

	public static Vector2 GetPerp(Vector2 v)
	{
		return new Vector2(0f - v.y, v.x);
	}

	public static Vector2 QuantizeVector(Vector2 vec)
	{
		int num = ((!(PhysicsEngine.Instance == null)) ? PhysicsEngine.Instance.PixelsPerUnit : 16);
		return QuantizeVector(vec, num);
	}

	public static Vector2 QuantizeVector(Vector2 vec, float unitsPerUnit)
	{
		return new Vector2(Mathf.Round(vec.x * unitsPerUnit), Mathf.Round(vec.y * unitsPerUnit)) / unitsPerUnit;
	}

	public static Vector3 QuantizeVector(Vector3 vec)
	{
		return QuantizeVector(vec, PhysicsEngine.Instance.PixelsPerUnit);
	}

	public static Vector3 QuantizeVector(Vector3 vec, float unitsPerUnit)
	{
		return new Vector3(Mathf.Round(vec.x * unitsPerUnit), Mathf.Round(vec.y * unitsPerUnit), Mathf.Round(vec.z * unitsPerUnit)) / unitsPerUnit;
	}

	public static int GCD(int a, int b)
	{
		while (b != 0)
		{
			int num = a % b;
			a = b;
			b = num;
		}
		return a;
	}

	public static int GetTileMapLayerByName(string name, tk2dTileMap tileMap)
	{
		for (int i = 0; i < tileMap.data.tileMapLayers.Count; i++)
		{
			if (tileMap.data.tileMapLayers[i].name == name)
			{
				return i;
			}
		}
		return -1;
	}

	public static T GetClosestToPosition<T>(List<T> sources, Vector2 pos, params T[] excluded) where T : BraveBehaviour
	{
		return GetClosestToPosition(sources, pos, null, excluded);
	}

	public static T GetClosestToPosition<T>(List<T> sources, Vector2 pos, Func<T, bool> isValid, params T[] excluded) where T : BraveBehaviour
	{
		return GetClosestToPosition(sources, pos, isValid, -1f, excluded);
	}

	public static T GetClosestToPosition<T>(List<T> sources, Vector2 pos, Func<T, bool> isValid, float maxDistance, params T[] excluded) where T : BraveBehaviour
	{
		T result = (T)null;
		float num = float.MaxValue;
		if (sources == null)
		{
			return result;
		}
		for (int i = 0; i < sources.Count; i++)
		{
			if ((bool)(UnityEngine.Object)sources[i] && (excluded == null || excluded.Length >= sources.Count || Array.IndexOf(excluded, sources[i]) == -1) && (isValid == null || isValid(sources[i])))
			{
				float num2 = float.MaxValue;
				T val = sources[i];
				if (val.sprite != null)
				{
					T val2 = sources[i];
					num2 = Vector2.SqrMagnitude(val2.sprite.WorldCenter - pos);
				}
				else
				{
					T val3 = sources[i];
					num2 = Vector2.SqrMagnitude(val3.transform.position.XY() - pos);
				}
				if ((!(maxDistance > 0f) || !(num2 > maxDistance)) && num2 < num)
				{
					result = sources[i];
					num = num2;
				}
			}
		}
		return result;
	}

	public static T[][] MultidimensionalArrayResize<T>(T[][] original, int oldWidth, int oldHeight, int newWidth, int newHeight)
	{
		T[][] array = new T[newWidth][];
		for (int i = 0; i < newWidth; i++)
		{
			array[i] = new T[newHeight];
		}
		int num = Mathf.Min(oldWidth, newWidth);
		int num2 = Mathf.Min(oldHeight, newHeight);
		for (int j = 0; j < num; j++)
		{
			for (int k = 0; k < num2; k++)
			{
				array[j][k] = original[j][k];
			}
		}
		return array;
	}

	public static T[,] MultidimensionalArrayResize<T>(T[,] original, int rows, int cols)
	{
		T[,] array = new T[rows, cols];
		int num = Mathf.Min(rows, original.GetLength(0));
		int num2 = Mathf.Min(cols, original.GetLength(1));
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				array[i, j] = original[i, j];
			}
		}
		return array;
	}

	public static int[] ParsePageNums(string str)
	{
		string[] array = str.Split(',');
		List<int> list = new List<int>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			int result;
			if (int.TryParse(text, out result))
			{
				list.Add(result);
				continue;
			}
			string[] array2 = text.Split('-');
			int result2;
			int result3;
			if (array2.Length > 1 && int.TryParse(array2[0], out result2) && int.TryParse(array2[1], out result3) && result3 >= result2)
			{
				for (int j = result2; j <= result3; j++)
				{
					list.Add(j);
				}
			}
		}
		return list.ToArray();
	}

	public static int EnumFlagsContains(uint data, uint valToFind)
	{
		if ((data & valToFind) == valToFind)
		{
			return 1;
		}
		return 0;
	}

	public static void Assert(bool assert, string s, bool pauseEditor = false)
	{
		if (assert)
		{
			Log(s, Color.red, LogVerbosity.IMPORTANT);
		}
	}

	public static void Log(string s, Color c, LogVerbosity v = LogVerbosity.VERBOSE)
	{
	}

	public static List<T> GenerationShuffle<T>(this List<T> input)
	{
		for (int num = input.Count - 1; num > 1; num--)
		{
			int index = BraveRandom.GenerationRandomRange(0, num);
			T value = input[num];
			input[num] = input[index];
			input[index] = value;
		}
		return input;
	}

	public static List<T> Shuffle<T>(this List<T> input)
	{
		for (int num = input.Count - 1; num > 1; num--)
		{
			int index = UnityEngine.Random.Range(0, num);
			T value = input[num];
			input[num] = input[index];
			input[index] = value;
		}
		return input;
	}

	public static List<T> SafeShuffle<T>(this List<T> input)
	{
		System.Random random = new System.Random();
		for (int num = input.Count - 1; num > 1; num--)
		{
			int index = random.Next(num);
			T value = input[num];
			input[num] = input[index];
			input[index] = value;
		}
		return input;
	}

	public static T RandomElement<T>(List<T> list)
	{
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static T RandomElement<T>(T[] array)
	{
		return array[UnityEngine.Random.Range(0, array.Length)];
	}

	public static bool RandomBool()
	{
		return UnityEngine.Random.value >= 0.5f;
	}

	public static float RandomSign()
	{
		return (UnityEngine.Random.value > 0.5f) ? 1 : (-1);
	}

	public static float RandomAngle()
	{
		return UnityEngine.Random.Range(0f, 360f);
	}

	public static Vector2 RandomVector2(Vector2 min, Vector2 max)
	{
		return new Vector2(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y));
	}

	public static Vector2 RandomVector2(Vector2 min, Vector2 max, Vector2 padding)
	{
		if (padding.x < 0f && padding.y < 0f)
		{
			if (RandomBool())
			{
				padding.x *= -1f;
			}
			else
			{
				padding.y *= -1f;
			}
		}
		float x = ((!(padding.x >= 0f)) ? ((!RandomBool()) ? UnityEngine.Random.Range(max.x + padding.x, max.x) : UnityEngine.Random.Range(min.x, min.x - padding.x)) : UnityEngine.Random.Range(min.x + padding.x, max.x - padding.x));
		float y = ((!(padding.y >= 0f)) ? ((!RandomBool()) ? UnityEngine.Random.Range(max.y + padding.y, max.y) : UnityEngine.Random.Range(min.y, min.y - padding.y)) : UnityEngine.Random.Range(min.y + padding.y, max.y - padding.y));
		return new Vector2(x, y);
	}

	public static void RandomizeList<T>(List<T> list, int startIndex = 0, int length = -1)
	{
		int num = ((length >= 0) ? (startIndex + length) : list.Count);
		for (int i = startIndex; i < num - 1; i++)
		{
			int index = UnityEngine.Random.Range(i + 1, num);
			T value = list[i];
			list[i] = list[index];
			list[index] = value;
		}
	}

	public static void RandomizeArray<T>(T[] array, int startIndex = 0, int length = -1)
	{
		int num = ((length >= 0) ? (startIndex + length) : array.Length);
		for (int i = startIndex; i < num - 1; i++)
		{
			int num2 = UnityEngine.Random.Range(i + 1, num);
			T val = array[i];
			array[i] = array[num2];
			array[num2] = val;
		}
	}

	public static void DrawDebugSquare(IntVector2 pos, Color col)
	{
		Debug.DrawLine(pos.ToVector2(), pos.ToVector2() + Vector2.up, col, 1000f);
		Debug.DrawLine(pos.ToVector2(), pos.ToVector2() + Vector2.right, col, 1000f);
		Debug.DrawLine(pos.ToVector2() + Vector2.up, pos.ToVector2() + Vector2.right + Vector2.up, col, 1000f);
		Debug.DrawLine(pos.ToVector2() + Vector2.right, pos.ToVector2() + Vector2.right + Vector2.up, col, 1000f);
	}

	private static string ColorToHex(Color col)
	{
		float num = col.r * 255f;
		float num2 = col.g * 255f;
		float num3 = col.b * 255f;
		string hex = GetHex(Mathf.FloorToInt(num / 16f));
		string hex2 = GetHex(Mathf.RoundToInt(num % 16f));
		string hex3 = GetHex(Mathf.FloorToInt(num2 / 16f));
		string hex4 = GetHex(Mathf.RoundToInt(num2 % 16f));
		string hex5 = GetHex(Mathf.FloorToInt(num3 / 16f));
		string hex6 = GetHex(Mathf.RoundToInt(num3 % 16f));
		return hex + hex2 + hex3 + hex4 + hex5 + hex6;
	}

	public static string ColorToHexWithAlpha(Color col)
	{
		float num = col.r * 255f;
		float num2 = col.g * 255f;
		float num3 = col.b * 255f;
		float num4 = col.a * 255f;
		string hex = GetHex(Mathf.FloorToInt(num / 16f));
		string hex2 = GetHex(Mathf.RoundToInt(num % 16f));
		string hex3 = GetHex(Mathf.FloorToInt(num2 / 16f));
		string hex4 = GetHex(Mathf.RoundToInt(num2 % 16f));
		string hex5 = GetHex(Mathf.FloorToInt(num3 / 16f));
		string hex6 = GetHex(Mathf.RoundToInt(num3 % 16f));
		string hex7 = GetHex(Mathf.FloorToInt(num4 / 16f));
		string hex8 = GetHex(Mathf.RoundToInt(num4 % 16f));
		return hex + hex2 + hex3 + hex4 + hex5 + hex6 + hex7 + hex8;
	}

	public static void AssignPositionalSoundTracking(GameObject obj)
	{
		if (!(obj.GetComponent<AkGameObj>() == null))
		{
		}
	}

	public static bool DX11Supported()
	{
		return SystemInfo.graphicsShaderLevel >= 50;
	}

	private static string GetHex(int d)
	{
		d = Mathf.Min(15, Mathf.Max(0, d));
		string text = "0123456789ABCDEF";
		return string.Empty + text[d];
	}

	public static string DecrementString(string str)
	{
		string baseStr;
		string suffixStr;
		SplitNumericSuffix(str, out baseStr, out suffixStr);
		if (suffixStr.Length == 0)
		{
			return str;
		}
		int num = int.Parse(suffixStr);
		return string.Concat(str1: Mathf.Max(0, num - 1).ToString("X" + suffixStr.Length), str0: baseStr);
	}

	public static string IncrementString(string str)
	{
		string baseStr;
		string suffixStr;
		SplitNumericSuffix(str, out baseStr, out suffixStr);
		if (suffixStr.Length == 0)
		{
			return str;
		}
		int num = int.Parse(suffixStr);
		return string.Concat(str1: Mathf.Max(0, num + 1).ToString("X" + suffixStr.Length), str0: baseStr);
	}

	public static void SplitNumericSuffix(string str, out string baseStr, out string suffixStr)
	{
		int num = 0;
		for (int num2 = str.Length - 1; num2 >= 0; num2--)
		{
			if (!char.IsDigit(str[num2]))
			{
				num = num2 + 1;
				break;
			}
		}
		if (num >= str.Length)
		{
			baseStr = str;
			suffixStr = string.Empty;
		}
		else
		{
			baseStr = str.Substring(0, num);
			suffixStr = str.Substring(num);
		}
	}

	public static List<int> GetPathCorners(List<IntVector2> path)
	{
		List<int> list = new List<int>();
		for (int i = 1; i < path.Count - 1; i++)
		{
			IntVector2 intVector = path[i - 1];
			IntVector2 intVector2 = path[i];
			IntVector2 intVector3 = path[i + 1];
			IntVector2 intVector4 = intVector2 - intVector;
			IntVector2 intVector5 = intVector3 - intVector2;
			if (intVector4 != intVector5)
			{
				list.Add(i);
			}
		}
		return list;
	}

	public static IEnumerable<T> Zip<A, B, T>(this IEnumerable<A> seqA, IEnumerable<B> seqB, Func<A, B, T> func)
	{
		if (seqA == null)
		{
			throw new ArgumentNullException("seqA");
		}
		if (seqB == null)
		{
			throw new ArgumentNullException("seqB");
		}
		using (IEnumerator<A> iteratorA = seqA.GetEnumerator())
		{
			using (IEnumerator<B> iteratorB = seqB.GetEnumerator())
			{
				while (iteratorA.MoveNext() && iteratorB.MoveNext())
				{
					yield return func(iteratorA.Current, iteratorB.Current);
				}
			}
		}
	}

	public static int GetNthIndexOf(string s, char t, int n)
	{
		int num = 0;
		for (int i = 0; i < s.Length; i++)
		{
			if (s[i] == t)
			{
				num++;
				if (num == n)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static void Swap<T>(ref T v1, ref T v2)
	{
		T val = v1;
		v1 = v2;
		v2 = val;
	}

	public static Color GetRainbowLerp(float t)
	{
		t %= 1f;
		t *= 6f;
		if (t < 1f)
		{
			return Color.Lerp(Color.red, new Color(1f, 0.5f, 0f), t % 1f);
		}
		if (t < 2f)
		{
			return Color.Lerp(new Color(1f, 0.5f, 0f), Color.yellow, t % 1f);
		}
		if (t < 3f)
		{
			return Color.Lerp(Color.yellow, Color.green, t % 1f);
		}
		if (t < 4f)
		{
			return Color.Lerp(Color.green, Color.blue, t % 1f);
		}
		if (t < 5f)
		{
			return Color.Lerp(Color.blue, new Color(0.5f, 0f, 1f), t % 1f);
		}
		if (t < 6f)
		{
			return Color.Lerp(new Color(0.5f, 0f, 1f), Color.red, t % 1f);
		}
		return Color.red;
	}

	public static Color GetRainbowColor(int index)
	{
		switch (index)
		{
		case 0:
			return Color.red;
		case 1:
			return new Color(1f, 0.5f, 0f, 1f);
		case 2:
			return Color.yellow;
		case 3:
			return Color.green;
		case 4:
			return Color.blue;
		case 5:
			return Color.magenta;
		case 6:
			return new Color(0.5f, 0f, 1f);
		case 7:
			return Color.grey;
		case 8:
			return Color.white;
		default:
			return Color.white;
		}
	}

	public static T[] AppendArray<T>(T[] oldArray, T newElement)
	{
		T[] array = new T[oldArray.Length + 1];
		Array.Copy(oldArray, array, oldArray.Length);
		array[array.Length - 1] = newElement;
		return array;
	}

	public static int SequentialRandomRange(int min, int max, int lastValue, int? maxDistFromLast = null, bool excludeLastValue = false)
	{
		if (maxDistFromLast.HasValue)
		{
			min = Mathf.Max(min, lastValue - maxDistFromLast.Value);
			max = Mathf.Min(max, lastValue + maxDistFromLast.Value + 1);
		}
		if (excludeLastValue)
		{
			max--;
		}
		int num = UnityEngine.Random.Range(min, max);
		if (excludeLastValue && num >= lastValue)
		{
			num++;
		}
		return num;
	}

	public static int SmartListResizer(int currentSize, int desiredSize, int minGrowingSize = 100, int forceMultipleOf = 0)
	{
		int num = ((currentSize == 0) ? desiredSize : ((currentSize < minGrowingSize && desiredSize < minGrowingSize) ? minGrowingSize : ((desiredSize >= currentSize * 2) ? (desiredSize + currentSize) : (currentSize * 2))));
		if (forceMultipleOf > 0 && num % forceMultipleOf > 0)
		{
			num += forceMultipleOf - num % forceMultipleOf;
		}
		return num;
	}

	public static void EnableEmission(ParticleSystem ps, bool enabled)
	{
		ParticleSystem.EmissionModule emission = ps.emission;
		emission.enabled = enabled;
	}

	public static float GetEmissionRate(ParticleSystem ps)
	{
		return ps.emission.rate.constant;
	}

	public static void SetEmissionRate(ParticleSystem ps, float emissionRate)
	{
		ParticleSystem.EmissionModule emission = ps.emission;
		emission.rate = emissionRate;
	}
}
