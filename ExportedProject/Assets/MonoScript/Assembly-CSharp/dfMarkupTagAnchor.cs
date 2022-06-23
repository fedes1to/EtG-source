[dfMarkupTagInfo("a")]
public class dfMarkupTagAnchor : dfMarkupTag
{
	public string HRef
	{
		get
		{
			dfMarkupAttribute dfMarkupAttribute2 = findAttribute("href");
			return (dfMarkupAttribute2 == null) ? string.Empty : dfMarkupAttribute2.Value;
		}
	}

	public dfMarkupTagAnchor()
		: base("a")
	{
	}

	public dfMarkupTagAnchor(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		style.TextDecoration = dfMarkupTextDecoration.Underline;
		style = applyTextStyleAttributes(style);
		for (int i = 0; i < base.ChildNodes.Count; i++)
		{
			dfMarkupElement dfMarkupElement2 = base.ChildNodes[i];
			if (dfMarkupElement2 is dfMarkupString)
			{
				dfMarkupString dfMarkupString2 = dfMarkupElement2 as dfMarkupString;
				if (dfMarkupString2.Text == "\n")
				{
					if (style.PreserveWhitespace)
					{
						container.AddLineBreak();
					}
					continue;
				}
			}
			dfMarkupElement2.PerformLayout(container, style);
		}
	}
}
