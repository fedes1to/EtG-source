using System.Collections.Generic;

[dfMarkupTagInfo("span")]
public class dfMarkupTagSpan : dfMarkupTag
{
	private static Queue<dfMarkupTagSpan> objectPool = new Queue<dfMarkupTagSpan>();

	public dfMarkupTagSpan()
		: base("span")
	{
	}

	public dfMarkupTagSpan(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		style = applyTextStyleAttributes(style);
		dfMarkupBox dfMarkupBox2 = container;
		dfMarkupAttribute dfMarkupAttribute2 = findAttribute("margin");
		if (dfMarkupAttribute2 != null)
		{
			dfMarkupBox2 = new dfMarkupBox(this, dfMarkupDisplayType.inlineBlock, style);
			dfMarkupBox2.Margins = dfMarkupBorders.Parse(dfMarkupAttribute2.Value);
			dfMarkupBox2.Margins.top = 0;
			dfMarkupBox2.Margins.bottom = 0;
			container.AddChild(dfMarkupBox2);
		}
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
						dfMarkupBox2.AddLineBreak();
					}
					continue;
				}
			}
			dfMarkupElement2.PerformLayout(dfMarkupBox2, style);
		}
	}

	internal static dfMarkupTagSpan Obtain()
	{
		if (objectPool.Count > 0)
		{
			return objectPool.Dequeue();
		}
		return new dfMarkupTagSpan();
	}

	internal override void Release()
	{
		base.Release();
		objectPool.Enqueue(this);
	}
}
