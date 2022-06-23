using System.Collections.Generic;
using System.Text;

namespace FullInspector.Internal
{
	public static class fiDisplayNameMapper
	{
		private static readonly Dictionary<string, string> _mappedNames = new Dictionary<string, string>();

		public static string Map(string propertyName)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				return string.Empty;
			}
			string value;
			if (!_mappedNames.TryGetValue(propertyName, out value))
			{
				value = MapInternal(propertyName);
				_mappedNames[propertyName] = value;
			}
			return value;
		}

		private static string MapInternal(string propertyName)
		{
			if (propertyName.StartsWith("m_") && propertyName != "m_")
			{
				propertyName = propertyName.Substring(2);
			}
			int i;
			for (i = 0; i < propertyName.Length && propertyName[i] == '_'; i++)
			{
			}
			if (i >= propertyName.Length)
			{
				return propertyName;
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			for (int j = i; j < propertyName.Length; j++)
			{
				char c = propertyName[j];
				if (c == '_')
				{
					flag = true;
					continue;
				}
				if (flag)
				{
					flag = false;
					c = char.ToUpper(c);
				}
				if (j != i && ShouldInsertSpace(j, propertyName))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}

		private static bool ShouldInsertSpace(int currentIndex, string str)
		{
			if (char.IsUpper(str[currentIndex]))
			{
				if (currentIndex + 1 >= str.Length || char.IsUpper(str[currentIndex + 1]))
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}
}
