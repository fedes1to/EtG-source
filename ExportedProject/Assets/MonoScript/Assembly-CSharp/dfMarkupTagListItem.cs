using UnityEngine;

[dfMarkupTagInfo("li")]
public class dfMarkupTagListItem : dfMarkupTag
{
	public dfMarkupTagListItem()
		: base("li")
	{
	}

	public dfMarkupTagListItem(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		if (base.ChildNodes.Count == 0)
		{
			return;
		}
		float x = container.Size.x;
		dfMarkupBox dfMarkupBox2 = new dfMarkupBox(this, dfMarkupDisplayType.listItem, style);
		dfMarkupBox2.Margins.top = 10;
		container.AddChild(dfMarkupBox2);
		dfMarkupTagList dfMarkupTagList2 = base.Parent as dfMarkupTagList;
		if (dfMarkupTagList2 == null)
		{
			base._PerformLayoutImpl(container, style);
			return;
		}
		style.VerticalAlign = dfMarkupVerticalAlign.Baseline;
		string text = "•";
		if (dfMarkupTagList2.TagName == "ol")
		{
			text = container.Children.Count + ".";
		}
		dfMarkupStyle style2 = style;
		style2.VerticalAlign = dfMarkupVerticalAlign.Baseline;
		style2.Align = dfMarkupTextAlign.Right;
		dfMarkupBoxText dfMarkupBoxText2 = dfMarkupBoxText.Obtain(this, dfMarkupDisplayType.inlineBlock, style2);
		dfMarkupBoxText2.SetText(text);
		dfMarkupBoxText2.Width = dfMarkupTagList2.BulletWidth;
		dfMarkupBoxText2.Margins.left = style.FontSize * 2;
		dfMarkupBox2.AddChild(dfMarkupBoxText2);
		dfMarkupBox dfMarkupBox3 = new dfMarkupBox(this, dfMarkupDisplayType.inlineBlock, style);
		int fontSize = style.FontSize;
		float x2 = x - dfMarkupBoxText2.Size.x - (float)dfMarkupBoxText2.Margins.left - (float)fontSize;
		dfMarkupBox3.Size = new Vector2(x2, fontSize);
		dfMarkupBox3.Margins.left = (int)((float)style.FontSize * 0.5f);
		dfMarkupBox2.AddChild(dfMarkupBox3);
		for (int i = 0; i < base.ChildNodes.Count; i++)
		{
			base.ChildNodes[i].PerformLayout(dfMarkupBox3, style);
		}
		dfMarkupBox3.FitToContents();
		if (dfMarkupBox3.Parent != null)
		{
			dfMarkupBox3.Parent.FitToContents();
		}
		dfMarkupBox2.FitToContents();
	}
}
