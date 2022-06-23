using UnityEngine;

[dfMarkupTagInfo("b")]
[dfMarkupTagInfo("strong")]
public class dfMarkupTagBold : dfMarkupTag
{
	public dfMarkupTagBold()
		: base("b")
	{
	}

	public dfMarkupTagBold(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		style = applyTextStyleAttributes(style);
		if (style.FontStyle == FontStyle.Normal)
		{
			style.FontStyle = FontStyle.Bold;
		}
		else if (style.FontStyle == FontStyle.Italic)
		{
			style.FontStyle = FontStyle.BoldAndItalic;
		}
		base._PerformLayoutImpl(container, style);
	}
}
