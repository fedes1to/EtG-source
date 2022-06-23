using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public struct dfMarkupStyle
{
	private static Dictionary<string, Color> namedColors = new Dictionary<string, Color>
	{
		{
			"aqua",
			UIntToColor(65535u)
		},
		{
			"black",
			Color.black
		},
		{
			"blue",
			Color.blue
		},
		{
			"cyan",
			Color.cyan
		},
		{
			"fuchsia",
			UIntToColor(16711935u)
		},
		{
			"gray",
			Color.gray
		},
		{
			"green",
			Color.green
		},
		{
			"lime",
			UIntToColor(65280u)
		},
		{
			"magenta",
			Color.magenta
		},
		{
			"maroon",
			UIntToColor(8388608u)
		},
		{
			"navy",
			UIntToColor(128u)
		},
		{
			"olive",
			UIntToColor(8421376u)
		},
		{
			"orange",
			UIntToColor(16753920u)
		},
		{
			"purple",
			UIntToColor(8388736u)
		},
		{
			"red",
			Color.red
		},
		{
			"silver",
			UIntToColor(12632256u)
		},
		{
			"teal",
			UIntToColor(32896u)
		},
		{
			"white",
			Color.white
		},
		{
			"yellow",
			Color.yellow
		}
	};

	internal int Version;

	public dfRichTextLabel Host;

	public dfAtlas Atlas;

	public dfDynamicFont Font;

	public int FontSize;

	public FontStyle FontStyle;

	public dfMarkupTextDecoration TextDecoration;

	public dfMarkupTextAlign Align;

	public dfMarkupVerticalAlign VerticalAlign;

	public Color Color;

	public Color BackgroundColor;

	public float Opacity;

	public bool PreserveWhitespace;

	public bool Preformatted;

	public int WordSpacing;

	public int CharacterSpacing;

	private int lineHeight;

	public int LineHeight
	{
		get
		{
			if (lineHeight == 0)
			{
				return Mathf.CeilToInt(FontSize);
			}
			return Mathf.Max(FontSize, lineHeight);
		}
		set
		{
			lineHeight = value;
		}
	}

	public dfMarkupStyle(dfDynamicFont Font, int FontSize, FontStyle FontStyle)
	{
		Host = null;
		Atlas = null;
		Version = 0;
		this.Font = Font;
		this.FontSize = FontSize;
		this.FontStyle = FontStyle;
		Align = dfMarkupTextAlign.Left;
		VerticalAlign = dfMarkupVerticalAlign.Baseline;
		Color = Color.white;
		BackgroundColor = Color.clear;
		TextDecoration = dfMarkupTextDecoration.None;
		PreserveWhitespace = false;
		Preformatted = false;
		WordSpacing = 0;
		CharacterSpacing = 0;
		lineHeight = 0;
		Opacity = 1f;
	}

	public static dfMarkupTextDecoration ParseTextDecoration(string value)
	{
		switch (value)
		{
		case "underline":
			return dfMarkupTextDecoration.Underline;
		case "overline":
			return dfMarkupTextDecoration.Overline;
		case "line-through":
			return dfMarkupTextDecoration.LineThrough;
		default:
			return dfMarkupTextDecoration.None;
		}
	}

	public static dfMarkupVerticalAlign ParseVerticalAlignment(string value)
	{
		switch (value)
		{
		case "top":
			return dfMarkupVerticalAlign.Top;
		case "center":
		case "middle":
			return dfMarkupVerticalAlign.Middle;
		case "bottom":
			return dfMarkupVerticalAlign.Bottom;
		default:
			return dfMarkupVerticalAlign.Baseline;
		}
	}

	public static dfMarkupTextAlign ParseTextAlignment(string value)
	{
		switch (value)
		{
		case "right":
			return dfMarkupTextAlign.Right;
		case "center":
			return dfMarkupTextAlign.Center;
		case "justify":
			return dfMarkupTextAlign.Justify;
		default:
			return dfMarkupTextAlign.Left;
		}
	}

	public static FontStyle ParseFontStyle(string value, FontStyle baseStyle)
	{
		switch (value)
		{
		case "normal":
			return FontStyle.Normal;
		case "bold":
			switch (baseStyle)
			{
			case FontStyle.Normal:
				return FontStyle.Bold;
			case FontStyle.Italic:
				return FontStyle.BoldAndItalic;
			}
			break;
		case "italic":
			switch (baseStyle)
			{
			case FontStyle.Normal:
				return FontStyle.Italic;
			case FontStyle.Bold:
				return FontStyle.BoldAndItalic;
			}
			break;
		}
		return baseStyle;
	}

	public static int ParseSize(string value, int baseValue)
	{
		int result;
		if (value.Length > 1 && value.EndsWith("%") && int.TryParse(value.TrimEnd('%'), out result))
		{
			return (int)((float)baseValue * ((float)result / 100f));
		}
		if (value.EndsWith("px"))
		{
			value = value.Substring(0, value.Length - 2);
		}
		int result2;
		if (int.TryParse(value, out result2))
		{
			return result2;
		}
		return baseValue;
	}

	public static Color ParseColor(string color, Color defaultColor)
	{
		Color result = defaultColor;
		Color value;
		if (color.StartsWith("#"))
		{
			uint result2 = 0u;
			result = ((!uint.TryParse(color.Substring(1), NumberStyles.HexNumber, null, out result2)) ? Color.red : ((Color)UIntToColor(result2)));
		}
		else if (namedColors.TryGetValue(color.ToLowerInvariant(), out value))
		{
			result = value;
		}
		return result;
	}

	private static Color32 UIntToColor(uint color)
	{
		byte r = (byte)(color >> 16);
		byte g = (byte)(color >> 8);
		byte b = (byte)(color >> 0);
		return new Color32(r, g, b, byte.MaxValue);
	}
}
