[dfMarkupTagInfo("p")]
public class dfMarkupTagParagraph : dfMarkupTag
{
	public dfMarkupTagParagraph()
		: base("p")
	{
	}

	public dfMarkupTagParagraph(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		if (base.ChildNodes.Count != 0)
		{
			style = applyTextStyleAttributes(style);
			int top = ((container.Children.Count != 0) ? style.LineHeight : 0);
			dfMarkupBox dfMarkupBox2 = null;
			if (style.BackgroundColor.a > 0.005f)
			{
				dfMarkupBoxSprite dfMarkupBoxSprite2 = new dfMarkupBoxSprite(this, dfMarkupDisplayType.block, style);
				dfMarkupBoxSprite2.Atlas = base.Owner.Atlas;
				dfMarkupBoxSprite2.Source = base.Owner.BlankTextureSprite;
				dfMarkupBoxSprite2.Style.Color = style.BackgroundColor;
				dfMarkupBox2 = dfMarkupBoxSprite2;
			}
			else
			{
				dfMarkupBox2 = new dfMarkupBox(this, dfMarkupDisplayType.block, style);
			}
			dfMarkupBox2.Margins = new dfMarkupBorders(0, 0, top, style.LineHeight);
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
			if (dfMarkupBox2.Children.Count > 0)
			{
				dfMarkupBox2.Children[dfMarkupBox2.Children.Count - 1].IsNewline = true;
			}
			dfMarkupBox2.FitToContents(true);
		}
	}
}
