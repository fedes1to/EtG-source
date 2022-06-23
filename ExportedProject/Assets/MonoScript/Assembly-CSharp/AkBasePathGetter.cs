using System.IO;
using UnityEngine;

public class AkBasePathGetter
{
	public static string GetPlatformName()
	{
		string empty = string.Empty;
		if (!string.IsNullOrEmpty(empty))
		{
			return empty;
		}
		return "Windows";
	}

	public static string GetPlatformBasePath()
	{
		string platformName = GetPlatformName();
		string path = Path.Combine(GetFullSoundBankPath(), platformName);
		FixSlashes(ref path);
		return path;
	}

	public static string GetFullSoundBankPath()
	{
		string path = Path.Combine(Application.streamingAssetsPath, AkInitializer.GetBasePath());
		FixSlashes(ref path);
		return path;
	}

	public static void FixSlashes(ref string path, char separatorChar, char badChar, bool addTrailingSlash)
	{
		if (!string.IsNullOrEmpty(path))
		{
			path = path.Trim().Replace(badChar, separatorChar).TrimStart('\\');
			if (addTrailingSlash && !path.EndsWith(separatorChar.ToString()))
			{
				path += separatorChar;
			}
		}
	}

	public static void FixSlashes(ref string path)
	{
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		char badChar = ((directorySeparatorChar != '\\') ? '\\' : '/');
		FixSlashes(ref path, directorySeparatorChar, badChar, true);
	}

	public static string GetSoundbankBasePath()
	{
		string platformBasePath = GetPlatformBasePath();
		bool flag = true;
		string path = Path.Combine(platformBasePath, "Init.bnk");
		if (!File.Exists(path))
		{
			flag = false;
		}
		if (platformBasePath == string.Empty || !flag)
		{
			Debug.Log("WwiseUnity: Looking for SoundBanks in " + platformBasePath);
			Debug.LogError("WwiseUnity: Could not locate the SoundBanks. Did you make sure to copy them to the StreamingAssets folder?");
		}
		return platformBasePath;
	}
}
