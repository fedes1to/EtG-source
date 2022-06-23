using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class dfMarkupTag : dfMarkupElement
{
	private static int ELEMENTID;

	public List<dfMarkupAttribute> Attributes;

	private dfRichTextLabel owner;

	private string id;

	public string TagName { get; set; }

	public string ID
	{
		get
		{
			return id;
		}
	}

	public virtual bool IsEndTag { get; set; }

	public virtual bool IsClosedTag { get; set; }

	public virtual bool IsInline { get; set; }

	public dfRichTextLabel Owner
	{
		get
		{
			return owner;
		}
		set
		{
			owner = value;
			for (int i = 0; i < base.ChildNodes.Count; i++)
			{
				dfMarkupTag dfMarkupTag2 = base.ChildNodes[i] as dfMarkupTag;
				if (dfMarkupTag2 != null)
				{
					dfMarkupTag2.Owner = value;
				}
			}
		}
	}

	public dfMarkupTag(string tagName)
	{
		Attributes = new List<dfMarkupAttribute>();
		TagName = tagName;
		id = tagName + ELEMENTID++.ToString("X");
	}

	public dfMarkupTag(dfMarkupTag original)
	{
		TagName = original.TagName;
		Attributes = original.Attributes;
		IsEndTag = original.IsEndTag;
		IsClosedTag = original.IsClosedTag;
		IsInline = original.IsInline;
		id = original.id;
		List<dfMarkupElement> childNodes = original.ChildNodes;
		for (int i = 0; i < childNodes.Count; i++)
		{
			dfMarkupElement node = childNodes[i];
			AddChildNode(node);
		}
	}

	internal override void Release()
	{
		base.Release();
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		if (!IsEndTag)
		{
			dfMarkupAttribute dfMarkupAttribute2 = findAttribute("margin");
			if (dfMarkupAttribute2 != null)
			{
			}
			for (int i = 0; i < base.ChildNodes.Count; i++)
			{
				base.ChildNodes[i].PerformLayout(container, style);
			}
		}
	}

	protected dfMarkupStyle applyTextStyleAttributes(dfMarkupStyle style)
	{
		dfMarkupAttribute dfMarkupAttribute2 = findAttribute("font", "font-family");
		if (dfMarkupAttribute2 != null)
		{
			style.Font = dfDynamicFont.FindByName(dfMarkupAttribute2.Value) ?? owner.Font;
		}
		dfMarkupAttribute dfMarkupAttribute3 = findAttribute("style", "font-style");
		if (dfMarkupAttribute3 != null)
		{
			style.FontStyle = dfMarkupStyle.ParseFontStyle(dfMarkupAttribute3.Value, style.FontStyle);
		}
		dfMarkupAttribute dfMarkupAttribute4 = findAttribute("size", "font-size");
		if (dfMarkupAttribute4 != null)
		{
			style.FontSize = dfMarkupStyle.ParseSize(dfMarkupAttribute4.Value, style.FontSize);
		}
		dfMarkupAttribute dfMarkupAttribute5 = findAttribute("color");
		if (dfMarkupAttribute5 != null)
		{
			Color color = dfMarkupStyle.ParseColor(dfMarkupAttribute5.Value, style.Color);
			color.a = style.Opacity;
			style.Color = color;
		}
		dfMarkupAttribute dfMarkupAttribute6 = findAttribute("align", "text-align");
		if (dfMarkupAttribute6 != null)
		{
			style.Align = dfMarkupStyle.ParseTextAlignment(dfMarkupAttribute6.Value);
		}
		dfMarkupAttribute dfMarkupAttribute7 = findAttribute("valign", "vertical-align");
		if (dfMarkupAttribute7 != null)
		{
			style.VerticalAlign = dfMarkupStyle.ParseVerticalAlignment(dfMarkupAttribute7.Value);
		}
		dfMarkupAttribute dfMarkupAttribute8 = findAttribute("line-height");
		if (dfMarkupAttribute8 != null)
		{
			style.LineHeight = dfMarkupStyle.ParseSize(dfMarkupAttribute8.Value, style.LineHeight);
		}
		dfMarkupAttribute dfMarkupAttribute9 = findAttribute("text-decoration");
		if (dfMarkupAttribute9 != null)
		{
			style.TextDecoration = dfMarkupStyle.ParseTextDecoration(dfMarkupAttribute9.Value);
		}
		dfMarkupAttribute dfMarkupAttribute10 = findAttribute("background", "background-color");
		if (dfMarkupAttribute10 != null)
		{
			style.BackgroundColor = dfMarkupStyle.ParseColor(dfMarkupAttribute10.Value, Color.clear);
			style.BackgroundColor.a = style.Opacity;
		}
		return style;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		if (IsEndTag)
		{
			stringBuilder.Append("/");
		}
		stringBuilder.Append(TagName);
		for (int i = 0; i < Attributes.Count; i++)
		{
			stringBuilder.Append(" ");
			stringBuilder.Append(Attributes[i].ToString());
		}
		if (IsClosedTag)
		{
			stringBuilder.Append("/");
		}
		stringBuilder.Append("]");
		if (!IsClosedTag)
		{
			for (int j = 0; j < base.ChildNodes.Count; j++)
			{
				stringBuilder.Append(base.ChildNodes[j].ToString());
			}
			stringBuilder.Append("[/");
			stringBuilder.Append(TagName);
			stringBuilder.Append("]");
		}
		return stringBuilder.ToString();
	}

	protected dfMarkupAttribute findAttribute(params string[] names)
	{
		for (int i = 0; i < Attributes.Count; i++)
		{
			for (int j = 0; j < names.Length; j++)
			{
				if (Attributes[i].Name == names[j])
				{
					return Attributes[i];
				}
			}
		}
		return null;
	}
}
