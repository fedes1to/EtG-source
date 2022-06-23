using System.IO;
using UnityEngine;

public class VersionManager
{
	private static bool s_initialized;

	private static string s_displayVersionNumber = string.Empty;

	private static string s_realVersionNumber;

	public static string DisplayVersionNumber
	{
		get
		{
			if (!s_initialized)
			{
				Initialize();
			}
			return s_displayVersionNumber;
		}
	}

	public static string UniqueVersionNumber
	{
		get
		{
			if (!s_initialized)
			{
				Initialize();
			}
			return s_realVersionNumber ?? s_displayVersionNumber;
		}
	}

	private static void Initialize()
	{
		try
		{
			string path = Path.Combine(Application.streamingAssetsPath, "version.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllLines(path);
				if (array.Length > 0)
				{
					s_initialized = true;
					s_displayVersionNumber = array[0];
					s_realVersionNumber = ((array.Length <= 1) ? null : array[1]);
					return;
				}
			}
		}
		catch
		{
		}
		s_initialized = true;
		s_displayVersionNumber = string.Empty;
		s_realVersionNumber = null;
	}
}
