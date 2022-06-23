using System;
using System.Collections.Generic;

public class dfMarkupTokenizer : IDisposable, IPoolable
{
	private static dfList<dfMarkupTokenizer> pool = new dfList<dfMarkupTokenizer>();

	private static List<string> validTags = new List<string> { "color", "sprite" };

	private string source;

	private int index;

	public static dfList<dfMarkupToken> Tokenize(string source)
	{
		using (dfMarkupTokenizer dfMarkupTokenizer2 = ((pool.Count <= 0) ? new dfMarkupTokenizer() : pool.Pop()))
		{
			return dfMarkupTokenizer2.tokenize(source);
		}
	}

	public void Release()
	{
		source = null;
		index = 0;
		if (!pool.Contains(this))
		{
			pool.Add(this);
		}
	}

	private dfList<dfMarkupToken> tokenize(string source)
	{
		dfList<dfMarkupToken> dfList2 = dfList<dfMarkupToken>.Obtain();
		dfList2.EnsureCapacity(estimateTokenCount(source));
		dfList2.AutoReleaseItems = true;
		this.source = source;
		index = 0;
		while (index < source.Length)
		{
			char c = Peek();
			if (AtTagPosition())
			{
				dfMarkupToken dfMarkupToken2 = parseTag();
				if (dfMarkupToken2 != null)
				{
					dfList2.Add(dfMarkupToken2);
				}
				continue;
			}
			dfMarkupToken dfMarkupToken3 = null;
			if (char.IsWhiteSpace(c))
			{
				if (c != '\r')
				{
					dfMarkupToken3 = parseWhitespace();
				}
			}
			else
			{
				dfMarkupToken3 = parseNonWhitespace();
			}
			if (dfMarkupToken3 == null)
			{
				Advance();
			}
			else
			{
				dfList2.Add(dfMarkupToken3);
			}
		}
		return dfList2;
	}

	private int estimateTokenCount(string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			return 0;
		}
		int num = 1;
		bool flag = char.IsWhiteSpace(source[0]);
		for (int i = 1; i < source.Length; i++)
		{
			char c = source[i];
			if (char.IsControl(c) || c == '<')
			{
				num++;
				continue;
			}
			bool flag2 = char.IsWhiteSpace(c);
			if (flag2 != flag)
			{
				num++;
				flag = flag2;
			}
		}
		return num;
	}

	private bool AtTagPosition()
	{
		if (Peek() != '[')
		{
			return false;
		}
		char c = Peek(1);
		if (c == '/')
		{
			if (char.IsLetter(Peek(2)))
			{
				return isValidTag(index + 2, true);
			}
			return false;
		}
		if (char.IsLetter(c))
		{
			return isValidTag(index + 1, false);
		}
		return false;
	}

	private bool isValidTag(int index, bool endTag)
	{
		for (int i = 0; i < validTags.Count; i++)
		{
			string text = validTags[i];
			bool flag = true;
			for (int j = 0; j < text.Length - 1 && j + index < source.Length - 1 && (endTag || source[j + index] != ' ') && source[j + index] != ']'; j++)
			{
				if (char.ToLowerInvariant(text[j]) != char.ToLowerInvariant(source[j + index]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private dfMarkupToken parseQuotedString()
	{
		char c = Peek();
		if (c != '"' && c != '\'')
		{
			return null;
		}
		Advance();
		int startIndex = index;
		int num = index;
		while (index < source.Length && Advance() != c)
		{
			num++;
		}
		if (Peek() == c)
		{
			Advance();
		}
		return dfMarkupToken.Obtain(source, dfMarkupTokenType.Text, startIndex, num);
	}

	private dfMarkupToken parseNonWhitespace()
	{
		int startIndex = index;
		int num = index;
		while (index < source.Length)
		{
			char c = Advance();
			if (char.IsWhiteSpace(c) || AtTagPosition())
			{
				break;
			}
			num++;
		}
		return dfMarkupToken.Obtain(source, dfMarkupTokenType.Text, startIndex, num);
	}

	private dfMarkupToken parseWhitespace()
	{
		int num = index;
		int num2 = index;
		if (Peek() == '\n')
		{
			Advance();
			return dfMarkupToken.Obtain(source, dfMarkupTokenType.Newline, num, num);
		}
		while (index < source.Length)
		{
			char c = Advance();
			if (c == '\n' || c == '\r' || !char.IsWhiteSpace(c))
			{
				break;
			}
			num2++;
		}
		return dfMarkupToken.Obtain(source, dfMarkupTokenType.Whitespace, num, num2);
	}

	private dfMarkupToken parseWord()
	{
		if (!char.IsLetter(Peek()))
		{
			return null;
		}
		int startIndex = index;
		int num = index;
		while (index < source.Length && char.IsLetter(Advance()))
		{
			num++;
		}
		return dfMarkupToken.Obtain(source, dfMarkupTokenType.Text, startIndex, num);
	}

	private dfMarkupToken parseTag()
	{
		if (Peek() != '[')
		{
			return null;
		}
		char c = Peek(1);
		if (c == '/')
		{
			return parseEndTag();
		}
		Advance();
		c = Peek();
		if (!char.IsLetterOrDigit(c))
		{
			return null;
		}
		int startIndex = index;
		int num = index;
		while (index < source.Length && char.IsLetterOrDigit(Advance()))
		{
			num++;
		}
		dfMarkupToken dfMarkupToken2 = dfMarkupToken.Obtain(source, dfMarkupTokenType.StartTag, startIndex, num);
		if (index < source.Length && Peek() != ']')
		{
			c = Peek();
			if (char.IsWhiteSpace(c))
			{
				parseWhitespace();
			}
			int startIndex2 = index;
			int num2 = index;
			if (Peek() == '"')
			{
				dfMarkupToken dfMarkupToken3 = parseQuotedString();
				dfMarkupToken2.AddAttribute(dfMarkupToken3, dfMarkupToken3);
			}
			else
			{
				while (index < source.Length && Advance() != ']')
				{
					num2++;
				}
				dfMarkupToken dfMarkupToken4 = dfMarkupToken.Obtain(source, dfMarkupTokenType.Text, startIndex2, num2);
				dfMarkupToken2.AddAttribute(dfMarkupToken4, dfMarkupToken4);
			}
		}
		if (Peek() == ']')
		{
			Advance();
		}
		return dfMarkupToken2;
	}

	private dfMarkupToken parseAttributeValue()
	{
		int startIndex = index;
		int num = index;
		while (index < source.Length)
		{
			char c = Advance();
			if (c == ']' || char.IsWhiteSpace(c))
			{
				break;
			}
			num++;
		}
		return dfMarkupToken.Obtain(source, dfMarkupTokenType.Text, startIndex, num);
	}

	private dfMarkupToken parseEndTag()
	{
		Advance(2);
		int startIndex = index;
		int num = index;
		while (index < source.Length && char.IsLetterOrDigit(Advance()))
		{
			num++;
		}
		if (Peek() == ']')
		{
			Advance();
		}
		return dfMarkupToken.Obtain(source, dfMarkupTokenType.EndTag, startIndex, num);
	}

	private char Peek()
	{
		return Peek(0);
	}

	private char Peek(int offset)
	{
		if (index + offset > source.Length - 1)
		{
			return '\0';
		}
		return source[index + offset];
	}

	private char Advance()
	{
		return Advance(1);
	}

	private char Advance(int amount)
	{
		index += amount;
		return Peek();
	}

	public void Dispose()
	{
		Release();
	}
}
