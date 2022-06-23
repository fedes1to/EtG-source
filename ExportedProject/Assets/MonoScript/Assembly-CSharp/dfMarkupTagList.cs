using UnityEngine;

[dfMarkupTagInfo("ul")]
[dfMarkupTagInfo("ol")]
public class dfMarkupTagList : dfMarkupTag
{
	internal int BulletWidth { get; private set; }

	public dfMarkupTagList()
		: base("ul")
	{
	}

	public dfMarkupTagList(dfMarkupTag original)
		: base(original)
	{
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		if (base.ChildNodes.Count == 0)
		{
			return;
		}
		style = applyTextStyleAttributes(style);
		style.Align = dfMarkupTextAlign.Left;
		dfMarkupBox dfMarkupBox2 = new dfMarkupBox(this, dfMarkupDisplayType.block, style);
		container.AddChild(dfMarkupBox2);
		calculateBulletWidth(style);
		for (int i = 0; i < base.ChildNodes.Count; i++)
		{
			dfMarkupTag dfMarkupTag2 = base.ChildNodes[i] as dfMarkupTag;
			if (dfMarkupTag2 != null && !(dfMarkupTag2.TagName != "li"))
			{
				dfMarkupTag2.PerformLayout(dfMarkupBox2, style);
			}
		}
		dfMarkupBox2.FitToContents();
	}

	private void calculateBulletWidth(dfMarkupStyle style)
	{
		if (base.TagName == "ul")
		{
			BulletWidth = Mathf.CeilToInt(style.Font.MeasureText("•", style.FontSize, style.FontStyle).x);
			return;
		}
		int num = 0;
		for (int i = 0; i < base.ChildNodes.Count; i++)
		{
			dfMarkupTag dfMarkupTag2 = base.ChildNodes[i] as dfMarkupTag;
			if (dfMarkupTag2 != null && dfMarkupTag2.TagName == "li")
			{
				num++;
			}
		}
		string text = new string('X', num.ToString().Length) + ".";
		BulletWidth = Mathf.CeilToInt(style.Font.MeasureText(text, style.FontSize, style.FontStyle).x);
	}
}
