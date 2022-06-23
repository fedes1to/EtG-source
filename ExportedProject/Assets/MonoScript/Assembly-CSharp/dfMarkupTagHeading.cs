using UnityEngine;

[dfMarkupTagInfo("h3")]
[dfMarkupTagInfo("h2")]
[dfMarkupTagInfo("h1")]
[dfMarkupTagInfo("h6")]
[dfMarkupTagInfo("h5")]
[dfMarkupTagInfo("h4")]
public class dfMarkupTagHeading : dfMarkupTag
{
	public dfMarkupTagHeading()
		: base("h1")
	{
	}

	public dfMarkupTagHeading(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		dfMarkupBorders margins = default(dfMarkupBorders);
		dfMarkupStyle style2 = applyDefaultStyles(style, ref margins);
		style2 = applyTextStyleAttributes(style2);
		dfMarkupAttribute dfMarkupAttribute2 = findAttribute("margin");
		if (dfMarkupAttribute2 != null)
		{
			margins = dfMarkupBorders.Parse(dfMarkupAttribute2.Value);
		}
		dfMarkupBox dfMarkupBox2 = new dfMarkupBox(this, dfMarkupDisplayType.block, style2);
		dfMarkupBox2.Margins = margins;
		container.AddChild(dfMarkupBox2);
		base._PerformLayoutImpl(dfMarkupBox2, style2);
		dfMarkupBox2.FitToContents();
	}

	private dfMarkupStyle applyDefaultStyles(dfMarkupStyle style, ref dfMarkupBorders margins)
	{
		float num = 1f;
		float num2 = 1f;
		switch (base.TagName)
		{
		case "h1":
			num2 = 2f;
			num = 0.65f;
			break;
		case "h2":
			num2 = 1.5f;
			num = 0.75f;
			break;
		case "h3":
			num2 = 1.35f;
			num = 0.85f;
			break;
		case "h4":
			num2 = 1.15f;
			num = 0f;
			break;
		case "h5":
			num2 = 0.85f;
			num = 1.5f;
			break;
		case "h6":
			num2 = 0.75f;
			num = 1.75f;
			break;
		}
		style.FontSize = (int)((float)style.FontSize * num2);
		style.FontStyle = FontStyle.Bold;
		style.Align = dfMarkupTextAlign.Left;
		num *= (float)style.FontSize;
		margins.top = (margins.bottom = (int)num);
		return style;
	}
}
