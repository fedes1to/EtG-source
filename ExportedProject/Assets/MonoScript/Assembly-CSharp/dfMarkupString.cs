using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public class dfMarkupString : dfMarkupElement
{
	private static StringBuilder buffer = new StringBuilder();

	private static Regex whitespacePattern = new Regex("\\s+");

	private static Queue<dfMarkupString> objectPool = new Queue<dfMarkupString>();

	private bool isWhitespace;

	public string Text { get; private set; }

	public bool IsWhitespace
	{
		get
		{
			return isWhitespace;
		}
	}

	public dfMarkupString(string text)
	{
		Text = processWhitespace(dfMarkupEntity.Replace(text));
		isWhitespace = whitespacePattern.IsMatch(Text);
	}

	public override string ToString()
	{
		return Text;
	}

	internal dfMarkupElement SplitWords()
	{
		dfMarkupTagSpan dfMarkupTagSpan2 = dfMarkupTagSpan.Obtain();
		int i = 0;
		int num = 0;
		int length = Text.Length;
		while (i < length)
		{
			for (; i < length && !char.IsWhiteSpace(Text[i]); i++)
			{
			}
			if (i > num)
			{
				dfMarkupTagSpan2.AddChildNode(Obtain(Text.Substring(num, i - num)));
				num = i;
			}
			for (; i < length && Text[i] != '\n' && char.IsWhiteSpace(Text[i]); i++)
			{
			}
			if (i > num)
			{
				dfMarkupTagSpan2.AddChildNode(Obtain(Text.Substring(num, i - num)));
				num = i;
			}
			if (i < length && Text[i] == '\n')
			{
				dfMarkupTagSpan2.AddChildNode(Obtain("\n"));
				num = ++i;
			}
		}
		return dfMarkupTagSpan2;
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		if (!(style.Font == null))
		{
			string text = ((!style.PreserveWhitespace && isWhitespace) ? " " : Text);
			dfMarkupBoxText dfMarkupBoxText2 = dfMarkupBoxText.Obtain(this, dfMarkupDisplayType.inline, style);
			dfMarkupBoxText2.SetText(text);
			container.AddChild(dfMarkupBoxText2);
		}
	}

	internal static dfMarkupString Obtain(string text)
	{
		if (objectPool.Count > 0)
		{
			dfMarkupString dfMarkupString2 = objectPool.Dequeue();
			dfMarkupString2.Text = dfMarkupEntity.Replace(text);
			dfMarkupString2.isWhitespace = whitespacePattern.IsMatch(dfMarkupString2.Text);
			return dfMarkupString2;
		}
		return new dfMarkupString(text);
	}

	internal override void Release()
	{
		base.Release();
		objectPool.Enqueue(this);
	}

	private string processWhitespace(string text)
	{
		buffer.Length = 0;
		buffer.Append(text);
		buffer.Replace("\r\n", "\n");
		buffer.Replace("\r", "\n");
		buffer.Replace("\t", "    ");
		return buffer.ToString();
	}
}
