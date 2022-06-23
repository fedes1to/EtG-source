using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace FullInspector.Internal
{
	public static class fiUtility
	{
		private static bool? _cachedIsEditor;

		private static bool? _isUnity4;

		public static bool IsEditor
		{
			get
			{
				if (!_cachedIsEditor.HasValue)
				{
					_cachedIsEditor = Type.GetType("UnityEditor.Editor, UnityEditor", false) != null;
				}
				return _cachedIsEditor.Value;
			}
		}

		public static bool IsMainThread
		{
			get
			{
				if (!IsEditor)
				{
					throw new InvalidOperationException("Only available in the editor");
				}
				return Thread.CurrentThread.ManagedThreadId == 1;
			}
		}

		public static bool IsUnity4
		{
			get
			{
				if (!_isUnity4.HasValue)
				{
					_isUnity4 = Type.GetType("UnityEngine.RuntimeInitializeOnLoadMethodAttribute, UnityEngine", false) == null;
				}
				return _isUnity4.Value;
			}
		}

		public static string CombinePaths(string a, string b)
		{
			return Path.Combine(a, b).Replace('\\', '/');
		}

		public static string CombinePaths(string a, string b, string c)
		{
			return Path.Combine(Path.Combine(a, b), c).Replace('\\', '/');
		}

		public static string CombinePaths(string a, string b, string c, string d)
		{
			return Path.Combine(Path.Combine(Path.Combine(a, b), c), d).Replace('\\', '/');
		}

		public static bool NearlyEqual(float a, float b)
		{
			return NearlyEqual(a, b, float.Epsilon);
		}

		public static bool NearlyEqual(float a, float b, float epsilon)
		{
			float num = Math.Abs(a);
			float num2 = Math.Abs(b);
			float num3 = Math.Abs(a - b);
			if (a == b)
			{
				return true;
			}
			if (a == 0f || b == 0f || num3 < float.MinValue)
			{
				return (double)num3 < (double)epsilon * double.MinValue;
			}
			return num3 / (num + num2) < epsilon;
		}

		public static void DestroyObject(UnityEngine.Object obj)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(obj);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(obj, true);
			}
		}

		public static void DestroyObject<T>(ref T obj) where T : UnityEngine.Object
		{
			DestroyObject(obj);
			obj = (T)null;
		}

		public static string StripLeadingWhitespace(this string s)
		{
			Regex regex = new Regex("^\\s+", RegexOptions.Multiline);
			return regex.Replace(s, string.Empty);
		}

		public static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(IList<TKey> keys, IList<TValue> values)
		{
			Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
			if (keys != null && values != null)
			{
				for (int i = 0; i < Mathf.Min(keys.Count, values.Count); i++)
				{
					if (!object.ReferenceEquals(keys[i], null))
					{
						dictionary[keys[i]] = values[i];
					}
				}
			}
			return dictionary;
		}

		public static void Swap<T>(ref T a, ref T b)
		{
			T val = a;
			a = b;
			b = val;
		}
	}
}
