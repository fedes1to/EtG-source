using System;

public static class dfStringExtensions
{
	public static string MakeRelativePath(this string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return string.Empty;
		}
		return path.Substring(path.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase));
	}

	public static bool Contains(this string value, string pattern, bool caseInsensitive)
	{
		if (caseInsensitive)
		{
			return value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) != -1;
		}
		return value.IndexOf(pattern) != -1;
	}
}
