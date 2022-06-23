using UnityEngine;

[dfMarkupTagInfo("img")]
public class dfMarkupTagImg : dfMarkupTag
{
	public dfMarkupTagImg()
		: base("img")
	{
		IsClosedTag = true;
	}

	public dfMarkupTagImg(dfMarkupTag original)
		: base(original)
	{
		IsClosedTag = true;
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		if (base.Owner == null)
		{
			Debug.LogError("Tag has no parent: " + this);
			return;
		}
		style = applyStyleAttributes(style);
		dfMarkupAttribute dfMarkupAttribute2 = findAttribute("src");
		if (dfMarkupAttribute2 == null)
		{
			return;
		}
		string value = dfMarkupAttribute2.Value;
		dfMarkupBox dfMarkupBox2 = createImageBox(base.Owner.Atlas, value, style);
		if (dfMarkupBox2 != null)
		{
			Vector2 size = Vector2.zero;
			dfMarkupAttribute dfMarkupAttribute3 = findAttribute("height");
			if (dfMarkupAttribute3 != null)
			{
				size.y = dfMarkupStyle.ParseSize(dfMarkupAttribute3.Value, (int)dfMarkupBox2.Size.y);
			}
			dfMarkupAttribute dfMarkupAttribute4 = findAttribute("width");
			if (dfMarkupAttribute4 != null)
			{
				size.x = dfMarkupStyle.ParseSize(dfMarkupAttribute4.Value, (int)dfMarkupBox2.Size.x);
			}
			if (size.sqrMagnitude <= float.Epsilon)
			{
				size = dfMarkupBox2.Size;
			}
			else if (size.x <= float.Epsilon)
			{
				size.x = size.y * (dfMarkupBox2.Size.x / dfMarkupBox2.Size.y);
			}
			else if (size.y <= float.Epsilon)
			{
				size.y = size.x * (dfMarkupBox2.Size.y / dfMarkupBox2.Size.x);
			}
			dfMarkupBox2.Size = size;
			dfMarkupBox2.Baseline = (int)size.y;
			container.AddChild(dfMarkupBox2);
		}
	}

	private dfMarkupStyle applyStyleAttributes(dfMarkupStyle style)
	{
		dfMarkupAttribute dfMarkupAttribute2 = findAttribute("valign");
		if (dfMarkupAttribute2 != null)
		{
			style.VerticalAlign = dfMarkupStyle.ParseVerticalAlignment(dfMarkupAttribute2.Value);
		}
		dfMarkupAttribute dfMarkupAttribute3 = findAttribute("color");
		if (dfMarkupAttribute3 != null)
		{
			Color color = dfMarkupStyle.ParseColor(dfMarkupAttribute3.Value, base.Owner.Color);
			color.a = style.Opacity;
			style.Color = color;
		}
		return style;
	}

	private dfMarkupBox createImageBox(dfAtlas atlas, string source, dfMarkupStyle style)
	{
		if (source.ToLowerInvariant().StartsWith("http://"))
		{
			return null;
		}
		if (atlas != null && atlas[source] != null)
		{
			dfMarkupBoxSprite dfMarkupBoxSprite2 = new dfMarkupBoxSprite(this, dfMarkupDisplayType.inline, style);
			dfMarkupBoxSprite2.LoadImage(atlas, source);
			return dfMarkupBoxSprite2;
		}
		Texture texture = dfMarkupImageCache.Load(source);
		if (texture != null)
		{
			dfMarkupBoxTexture dfMarkupBoxTexture2 = new dfMarkupBoxTexture(this, dfMarkupDisplayType.inline, style);
			dfMarkupBoxTexture2.LoadTexture(texture);
			return dfMarkupBoxTexture2;
		}
		return null;
	}
}
