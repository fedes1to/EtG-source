using System.Text;
using UnityEngine;

namespace Brave
{
	public static class PlayerPrefs
	{
		public static void DeleteAll()
		{
			UnityEngine.PlayerPrefs.DeleteAll();
		}

		public static void DeleteKey(string key)
		{
			UnityEngine.PlayerPrefs.DeleteKey(key);
		}

		public static float GetFloat(string key, float defaultValue)
		{
			return UnityEngine.PlayerPrefs.GetFloat(key, defaultValue);
		}

		public static float GetFloat(string key)
		{
			return UnityEngine.PlayerPrefs.GetFloat(key);
		}

		public static int GetInt(string key)
		{
			return UnityEngine.PlayerPrefs.GetInt(key);
		}

		public static int GetInt(string key, int defaultValue)
		{
			return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
		}

		public static string GetString(string key)
		{
			return UnityEngine.PlayerPrefs.GetString(key);
		}

		public static string GetString(string key, string defaultValue)
		{
			return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
		}

		public static bool HasKey(string key)
		{
			return UnityEngine.PlayerPrefs.HasKey(key);
		}

		public static void Save()
		{
			UnityEngine.PlayerPrefs.Save();
		}

		public static void SetFloat(string key, float value)
		{
			UnityEngine.PlayerPrefs.SetFloat(key, value);
		}

		public static void SetInt(string key, int value)
		{
			UnityEngine.PlayerPrefs.SetInt(key, value);
		}

		public static void SetString(string key, string value)
		{
			UnityEngine.PlayerPrefs.SetString(key, value);
		}

		public static string[] GetStringArray(string key, char delineator = ';')
		{
			string @string = GetString(key);
			if (string.IsNullOrEmpty(@string))
			{
				return new string[0];
			}
			return @string.Split(delineator);
		}

		public static void SetStringArray(string key, string[] value, char delineator = ';')
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				stringBuilder.Append(value[i]);
				if (i < value.Length - 1)
				{
					stringBuilder.Append(delineator);
				}
			}
			SetString(key, stringBuilder.ToString());
		}
	}
}
