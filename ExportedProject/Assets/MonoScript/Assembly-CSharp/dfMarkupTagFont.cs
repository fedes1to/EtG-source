using UnityEngine;

[dfMarkupTagInfo("font")]
public class dfMarkupTagFont : dfMarkupTag
{
	public dfMarkupTagFont()
		: base("font")
	{
	}

	public dfMarkupTagFont(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		dfMarkupAttribute dfMarkupAttribute2 = findAttribute("name", "face");
		if (dfMarkupAttribute2 != null)
		{
			style.Font = dfDynamicFont.FindByName(dfMarkupAttribute2.Value) ?? style.Font;
		}
		dfMarkupAttribute dfMarkupAttribute3 = findAttribute("size", "font-size");
		if (dfMarkupAttribute3 != null)
		{
			style.FontSize = dfMarkupStyle.ParseSize(dfMarkupAttribute3.Value, style.FontSize);
		}
		dfMarkupAttribute dfMarkupAttribute4 = findAttribute("color");
		if (dfMarkupAttribute4 != null)
		{
			style.Color = dfMarkupStyle.ParseColor(dfMarkupAttribute4.Value, Color.red);
			style.Color.a = style.Opacity;
		}
		dfMarkupAttribute dfMarkupAttribute5 = findAttribute("style");
		if (dfMarkupAttribute5 != null)
		{
			style.FontStyle = dfMarkupStyle.ParseFontStyle(dfMarkupAttribute5.Value, style.FontStyle);
		}
		base._PerformLayoutImpl(container, style);
	}
}
