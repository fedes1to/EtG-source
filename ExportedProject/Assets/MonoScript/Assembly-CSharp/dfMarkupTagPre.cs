[dfMarkupTagInfo("pre")]
public class dfMarkupTagPre : dfMarkupTag
{
	public dfMarkupTagPre()
		: base("pre")
	{
	}

	public dfMarkupTagPre(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		style = applyTextStyleAttributes(style);
		style.PreserveWhitespace = true;
		style.Preformatted = true;
		if (style.Align == dfMarkupTextAlign.Justify)
		{
			style.Align = dfMarkupTextAlign.Left;
		}
		dfMarkupBox dfMarkupBox2 = null;
		if (style.BackgroundColor.a > 0.1f)
		{
			dfMarkupBoxSprite dfMarkupBoxSprite2 = new dfMarkupBoxSprite(this, dfMarkupDisplayType.block, style);
			dfMarkupBoxSprite2.LoadImage(base.Owner.Atlas, base.Owner.BlankTextureSprite);
			dfMarkupBoxSprite2.Style.Color = style.BackgroundColor;
			dfMarkupBox2 = dfMarkupBoxSprite2;
		}
		else
		{
			dfMarkupBox2 = new dfMarkupBox(this, dfMarkupDisplayType.block, style);
		}
		dfMarkupAttribute dfMarkupAttribute2 = findAttribute("margin");
		if (dfMarkupAttribute2 != null)
		{
			dfMarkupBox2.Margins = dfMarkupBorders.Parse(dfMarkupAttribute2.Value);
		}
		dfMarkupAttribute dfMarkupAttribute3 = findAttribute("padding");
		if (dfMarkupAttribute3 != null)
		{
			dfMarkupBox2.Padding = dfMarkupBorders.Parse(dfMarkupAttribute3.Value);
		}
		container.AddChild(dfMarkupBox2);
		base._PerformLayoutImpl(dfMarkupBox2, style);
		dfMarkupBox2.FitToContents();
	}
}
