public class dfPlainTextTokenizer
{
	private static dfPlainTextTokenizer singleton;

	public static dfList<dfMarkupToken> Tokenize(string source)
	{
		if (singleton == null)
		{
			singleton = new dfPlainTextTokenizer();
		}
		return singleton.tokenize(source);
	}

	private dfList<dfMarkupToken> tokenize(string source)
	{
		dfList<dfMarkupToken> dfList2 = dfList<dfMarkupToken>.Obtain();
		dfList2.EnsureCapacity(estimateTokenCount(source));
		dfList2.AutoReleaseItems = true;
		int i = 0;
		int num = 0;
		int length = source.Length;
		while (i < length)
		{
			if (source[i] == '\r')
			{
				i++;
				num = i;
				continue;
			}
			for (; i < length && !char.IsWhiteSpace(source[i]); i++)
			{
			}
			if (i > num)
			{
				dfList2.Add(dfMarkupToken.Obtain(source, dfMarkupTokenType.Text, num, i - 1));
				num = i;
			}
			if (i < length && source[i] == '\n')
			{
				dfList2.Add(dfMarkupToken.Obtain(source, dfMarkupTokenType.Newline, i, i));
				i++;
				num = i;
			}
			for (; i < length && source[i] != '\n' && source[i] != '\r' && char.IsWhiteSpace(source[i]); i++)
			{
			}
			if (i > num)
			{
				dfList2.Add(dfMarkupToken.Obtain(source, dfMarkupTokenType.Whitespace, num, i - 1));
				num = i;
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
			if (char.IsControl(c))
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
}
