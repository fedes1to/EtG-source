using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FullSerializer
{
	public class fsJsonParser
	{
		private int _start;

		private string _input;

		private readonly StringBuilder _cachedStringBuilder = new StringBuilder(256);

		private fsJsonParser(string input)
		{
			_input = input;
			_start = 0;
		}

		private fsResult MakeFailure(string message)
		{
			int num = Math.Max(0, _start - 20);
			int length = Math.Min(50, _input.Length - num);
			string warning = "Error while parsing: " + message + "; context = <" + _input.Substring(num, length) + ">";
			return fsResult.Fail(warning);
		}

		private bool TryMoveNext()
		{
			if (_start < _input.Length)
			{
				_start++;
				return true;
			}
			return false;
		}

		private bool HasValue()
		{
			return HasValue(0);
		}

		private bool HasValue(int offset)
		{
			return _start + offset >= 0 && _start + offset < _input.Length;
		}

		private char Character()
		{
			return Character(0);
		}

		private char Character(int offset)
		{
			return _input[_start + offset];
		}

		private void SkipSpace()
		{
			while (HasValue())
			{
				char c = Character();
				if (char.IsWhiteSpace(c))
				{
					TryMoveNext();
					continue;
				}
				if (!HasValue(1) || Character(0) != '/')
				{
					break;
				}
				if (Character(1) == '/')
				{
					while (HasValue() && !Environment.NewLine.Contains(string.Empty + Character()))
					{
						TryMoveNext();
					}
				}
				else
				{
					if (Character(1) != '*')
					{
						continue;
					}
					TryMoveNext();
					TryMoveNext();
					while (HasValue(1))
					{
						if (Character(0) == '*' && Character(1) == '/')
						{
							TryMoveNext();
							TryMoveNext();
							TryMoveNext();
							break;
						}
						TryMoveNext();
					}
				}
			}
		}

		private bool IsHex(char c)
		{
			return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
		}

		private uint ParseSingleChar(char c1, uint multipliyer)
		{
			uint result = 0u;
			if (c1 >= '0' && c1 <= '9')
			{
				result = (uint)(c1 - 48) * multipliyer;
			}
			else if (c1 >= 'A' && c1 <= 'F')
			{
				result = (uint)(c1 - 65 + 10) * multipliyer;
			}
			else if (c1 >= 'a' && c1 <= 'f')
			{
				result = (uint)(c1 - 97 + 10) * multipliyer;
			}
			return result;
		}

		private uint ParseUnicode(char c1, char c2, char c3, char c4)
		{
			uint num = ParseSingleChar(c1, 4096u);
			uint num2 = ParseSingleChar(c2, 256u);
			uint num3 = ParseSingleChar(c3, 16u);
			uint num4 = ParseSingleChar(c4, 1u);
			return num + num2 + num3 + num4;
		}

		private fsResult TryUnescapeChar(out char escaped)
		{
			TryMoveNext();
			if (!HasValue())
			{
				escaped = ' ';
				return MakeFailure("Unexpected end of input after \\");
			}
			switch (Character())
			{
			case '\\':
				TryMoveNext();
				escaped = '\\';
				return fsResult.Success;
			case '/':
				TryMoveNext();
				escaped = '/';
				return fsResult.Success;
			case '"':
				TryMoveNext();
				escaped = '"';
				return fsResult.Success;
			case 'a':
				TryMoveNext();
				escaped = '\a';
				return fsResult.Success;
			case 'b':
				TryMoveNext();
				escaped = '\b';
				return fsResult.Success;
			case 'f':
				TryMoveNext();
				escaped = '\f';
				return fsResult.Success;
			case 'n':
				TryMoveNext();
				escaped = '\n';
				return fsResult.Success;
			case 'r':
				TryMoveNext();
				escaped = '\r';
				return fsResult.Success;
			case 't':
				TryMoveNext();
				escaped = '\t';
				return fsResult.Success;
			case '0':
				TryMoveNext();
				escaped = '\0';
				return fsResult.Success;
			case 'u':
				TryMoveNext();
				if (IsHex(Character(0)) && IsHex(Character(1)) && IsHex(Character(2)) && IsHex(Character(3)))
				{
					uint num = ParseUnicode(Character(0), Character(1), Character(2), Character(3));
					TryMoveNext();
					TryMoveNext();
					TryMoveNext();
					TryMoveNext();
					escaped = (char)num;
					return fsResult.Success;
				}
				escaped = '\0';
				return MakeFailure(string.Format("invalid escape sequence '\\u{0}{1}{2}{3}'\n", Character(0), Character(1), Character(2), Character(3)));
			default:
				escaped = '\0';
				return MakeFailure(string.Format("Invalid escape sequence \\{0}", Character()));
			}
		}

		private fsResult TryParseExact(string content)
		{
			for (int i = 0; i < content.Length; i++)
			{
				if (Character() != content[i])
				{
					return MakeFailure("Expected " + content[i]);
				}
				if (!TryMoveNext())
				{
					return MakeFailure("Unexpected end of content when parsing " + content);
				}
			}
			return fsResult.Success;
		}

		private fsResult TryParseTrue(out fsData data)
		{
			fsResult result = TryParseExact("true");
			if (result.Succeeded)
			{
				data = new fsData(true);
				return fsResult.Success;
			}
			data = null;
			return result;
		}

		private fsResult TryParseFalse(out fsData data)
		{
			fsResult result = TryParseExact("false");
			if (result.Succeeded)
			{
				data = new fsData(false);
				return fsResult.Success;
			}
			data = null;
			return result;
		}

		private fsResult TryParseNull(out fsData data)
		{
			fsResult result = TryParseExact("null");
			if (result.Succeeded)
			{
				data = new fsData();
				return fsResult.Success;
			}
			data = null;
			return result;
		}

		private bool IsSeparator(char c)
		{
			return char.IsWhiteSpace(c) || c == ',' || c == '}' || c == ']';
		}

		private fsResult TryParseNumber(out fsData data)
		{
			int start = _start;
			while (TryMoveNext() && HasValue() && !IsSeparator(Character()))
			{
			}
			string text = _input.Substring(start, _start - start);
			if (!text.Contains("."))
			{
				switch (text)
				{
				case "Infinity":
				case "-Infinity":
				case "NaN":
					break;
				default:
				{
					long result;
					if (!long.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						data = null;
						return MakeFailure("Bad Int64 format with " + text);
					}
					data = new fsData(result);
					return fsResult.Success;
				}
				}
			}
			double result2;
			if (!double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
			{
				data = null;
				return MakeFailure("Bad double format with " + text);
			}
			data = new fsData(result2);
			return fsResult.Success;
		}

		private fsResult TryParseString(out string str)
		{
			_cachedStringBuilder.Length = 0;
			if (Character() != '"' || !TryMoveNext())
			{
				str = string.Empty;
				return MakeFailure("Expected initial \" when parsing a string");
			}
			while (HasValue() && Character() != '"')
			{
				char c = Character();
				if (c == '\\')
				{
					char escaped;
					fsResult result = TryUnescapeChar(out escaped);
					if (result.Failed)
					{
						str = string.Empty;
						return result;
					}
					_cachedStringBuilder.Append(escaped);
				}
				else
				{
					_cachedStringBuilder.Append(c);
					if (!TryMoveNext())
					{
						str = string.Empty;
						return MakeFailure("Unexpected end of input when reading a string");
					}
				}
			}
			if (!HasValue() || Character() != '"' || !TryMoveNext())
			{
				str = string.Empty;
				return MakeFailure("No closing \" when parsing a string");
			}
			str = _cachedStringBuilder.ToString();
			return fsResult.Success;
		}

		private fsResult TryParseArray(out fsData arr)
		{
			if (Character() != '[')
			{
				arr = null;
				return MakeFailure("Expected initial [ when parsing an array");
			}
			if (!TryMoveNext())
			{
				arr = null;
				return MakeFailure("Unexpected end of input when parsing an array");
			}
			SkipSpace();
			List<fsData> list = new List<fsData>();
			while (HasValue() && Character() != ']')
			{
				fsData data;
				fsResult result = RunParse(out data);
				if (result.Failed)
				{
					arr = null;
					return result;
				}
				list.Add(data);
				SkipSpace();
				if (HasValue() && Character() == ',')
				{
					if (!TryMoveNext())
					{
						break;
					}
					SkipSpace();
				}
			}
			if (!HasValue() || Character() != ']' || !TryMoveNext())
			{
				arr = null;
				return MakeFailure("No closing ] for array");
			}
			arr = new fsData(list);
			return fsResult.Success;
		}

		private fsResult TryParseObject(out fsData obj)
		{
			if (Character() != '{')
			{
				obj = null;
				return MakeFailure("Expected initial { when parsing an object");
			}
			if (!TryMoveNext())
			{
				obj = null;
				return MakeFailure("Unexpected end of input when parsing an object");
			}
			SkipSpace();
			Dictionary<string, fsData> dictionary = new Dictionary<string, fsData>((!fsConfig.IsCaseSensitive) ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture);
			while (HasValue() && Character() != '}')
			{
				SkipSpace();
				string str;
				fsResult result = TryParseString(out str);
				if (result.Failed)
				{
					obj = null;
					return result;
				}
				SkipSpace();
				if (!HasValue() || Character() != ':' || !TryMoveNext())
				{
					obj = null;
					return MakeFailure("Expected : after key \"" + str + "\"");
				}
				SkipSpace();
				fsData data;
				result = RunParse(out data);
				if (result.Failed)
				{
					obj = null;
					return result;
				}
				dictionary.Add(str, data);
				SkipSpace();
				if (HasValue() && Character() == ',')
				{
					if (!TryMoveNext())
					{
						break;
					}
					SkipSpace();
				}
			}
			if (!HasValue() || Character() != '}' || !TryMoveNext())
			{
				obj = null;
				return MakeFailure("No closing } for object");
			}
			obj = new fsData(dictionary);
			return fsResult.Success;
		}

		private fsResult RunParse(out fsData data)
		{
			SkipSpace();
			if (!HasValue())
			{
				data = null;
				return MakeFailure("Unexpected end of input");
			}
			switch (Character())
			{
			case '+':
			case '-':
			case '.':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
			case 'I':
			case 'N':
				return TryParseNumber(out data);
			case '"':
			{
				string str;
				fsResult result = TryParseString(out str);
				if (result.Failed)
				{
					data = null;
					return result;
				}
				data = new fsData(str);
				return fsResult.Success;
			}
			case '[':
				return TryParseArray(out data);
			case '{':
				return TryParseObject(out data);
			case 't':
				return TryParseTrue(out data);
			case 'f':
				return TryParseFalse(out data);
			case 'n':
				return TryParseNull(out data);
			default:
				data = null;
				return MakeFailure("unable to parse; invalid token \"" + Character() + "\"");
			}
		}

		public static fsResult Parse(string input, out fsData data)
		{
			if (string.IsNullOrEmpty(input))
			{
				data = null;
				return fsResult.Fail("No input");
			}
			fsJsonParser fsJsonParser2 = new fsJsonParser(input);
			return fsJsonParser2.RunParse(out data);
		}

		public static fsData Parse(string input)
		{
			fsData data;
			Parse(input, out data).AssertSuccess();
			return data;
		}
	}
}
