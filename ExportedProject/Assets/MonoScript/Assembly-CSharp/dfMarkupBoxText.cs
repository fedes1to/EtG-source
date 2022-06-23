using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class dfMarkupBoxText : dfMarkupBox
{
	private static int[] TRIANGLE_INDICES = new int[6] { 0, 1, 2, 0, 2, 3 };

	private static Queue<dfMarkupBoxText> objectPool = new Queue<dfMarkupBoxText>();

	private static Regex whitespacePattern = new Regex("\\s+");

	private dfRenderData renderData = new dfRenderData();

	private bool isWhitespace;

	public string Text { get; private set; }

	public bool IsWhitespace
	{
		get
		{
			return isWhitespace;
		}
	}

	public dfMarkupBoxText(dfMarkupElement element, dfMarkupDisplayType display, dfMarkupStyle style)
		: base(element, display, style)
	{
	}

	public static dfMarkupBoxText Obtain(dfMarkupElement element, dfMarkupDisplayType display, dfMarkupStyle style)
	{
		if (objectPool.Count > 0)
		{
			dfMarkupBoxText dfMarkupBoxText2 = objectPool.Dequeue();
			dfMarkupBoxText2.Element = element;
			dfMarkupBoxText2.Display = display;
			dfMarkupBoxText2.Style = style;
			dfMarkupBoxText2.Position = Vector2.zero;
			dfMarkupBoxText2.Size = Vector2.zero;
			dfMarkupBoxText2.Baseline = (int)((float)style.FontSize * 1.1f);
			dfMarkupBoxText2.Margins = default(dfMarkupBorders);
			dfMarkupBoxText2.Padding = default(dfMarkupBorders);
			return dfMarkupBoxText2;
		}
		return new dfMarkupBoxText(element, display, style);
	}

	public override void Release()
	{
		base.Release();
		Text = string.Empty;
		renderData.Clear();
		objectPool.Enqueue(this);
	}

	internal void SetText(string text)
	{
		Text = text;
		if (Style.Font == null)
		{
			return;
		}
		isWhitespace = whitespacePattern.IsMatch(Text);
		string text2 = ((!Style.PreserveWhitespace && isWhitespace) ? " " : Text);
		int fontSize = Style.FontSize;
		Vector2 size = new Vector2(0f, Style.LineHeight);
		Style.Font.RequestCharacters(text2, Style.FontSize, Style.FontStyle);
		CharacterInfo info = default(CharacterInfo);
		for (int i = 0; i < text2.Length; i++)
		{
			if (Style.Font.BaseFont.GetCharacterInfo(text2[i], out info, fontSize, Style.FontStyle))
			{
				float num = info.maxX;
				if (text2[i] == ' ')
				{
					num = Mathf.Max(num, (float)fontSize * 0.33f);
				}
				else if (text2[i] == '\t')
				{
					num += (float)(fontSize * 3);
				}
				size.x += num;
			}
		}
		Size = size;
		dfDynamicFont font = Style.Font;
		float num2 = (float)fontSize / (float)font.FontSize;
		Baseline = Mathf.CeilToInt((float)font.Baseline * num2);
	}

	protected override dfRenderData OnRebuildRenderData()
	{
		renderData.Clear();
		if (Style.Font == null)
		{
			return null;
		}
		if (Style.TextDecoration == dfMarkupTextDecoration.Underline)
		{
			renderUnderline();
		}
		renderText(Text);
		return renderData;
	}

	private void renderUnderline()
	{
	}

	private void renderText(string text)
	{
		dfDynamicFont font = Style.Font;
		int fontSize = Style.FontSize;
		FontStyle fontStyle = Style.FontStyle;
		CharacterInfo info = default(CharacterInfo);
		dfList<Vector3> vertices = renderData.Vertices;
		dfList<int> triangles = renderData.Triangles;
		dfList<Vector2> uV = renderData.UV;
		dfList<Color32> colors = renderData.Colors;
		float num = (float)fontSize / (float)font.FontSize;
		float num2 = (float)font.Descent * num;
		float num3 = 0f;
		font.RequestCharacters(text, fontSize, fontStyle);
		renderData.Material = font.Material;
		for (int i = 0; i < text.Length; i++)
		{
			if (font.BaseFont.GetCharacterInfo(text[i], out info, fontSize, fontStyle))
			{
				addTriangleIndices(vertices, triangles);
				float num4 = (float)(font.FontSize + info.maxY - fontSize) + num2;
				float num5 = num3 + (float)info.minX;
				float num6 = num4;
				float x = num5 + (float)info.glyphWidth;
				float y = num6 - (float)info.glyphHeight;
				Vector3 item = new Vector3(num5, num6);
				Vector3 item2 = new Vector3(x, num6);
				Vector3 item3 = new Vector3(x, y);
				Vector3 item4 = new Vector3(num5, y);
				vertices.Add(item);
				vertices.Add(item2);
				vertices.Add(item3);
				vertices.Add(item4);
				Color color = Style.Color;
				colors.Add(color);
				colors.Add(color);
				colors.Add(color);
				colors.Add(color);
				uV.Add(info.uvTopLeft);
				uV.Add(info.uvTopRight);
				uV.Add(info.uvBottomRight);
				uV.Add(info.uvBottomLeft);
				num3 += (float)Mathf.CeilToInt(info.maxX);
			}
		}
	}

	private static void addTriangleIndices(dfList<Vector3> verts, dfList<int> triangles)
	{
		int count = verts.Count;
		int[] tRIANGLE_INDICES = TRIANGLE_INDICES;
		for (int i = 0; i < tRIANGLE_INDICES.Length; i++)
		{
			triangles.Add(count + tRIANGLE_INDICES[i]);
		}
	}
}
