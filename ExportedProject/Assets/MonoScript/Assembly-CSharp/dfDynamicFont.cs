using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Dynamic Font")]
[ExecuteInEditMode]
public class dfDynamicFont : dfFontBase
{
	protected class FontCharacterRequest : IPoolable
	{
		private static dfList<FontCharacterRequest> pool = new dfList<FontCharacterRequest>();

		public string Characters;

		public int FontSize;

		public FontStyle FontStyle;

		public static FontCharacterRequest Obtain()
		{
			return (pool.Count <= 0) ? new FontCharacterRequest() : pool.Pop();
		}

		public void Release()
		{
			Characters = null;
			FontSize = 0;
			FontStyle = FontStyle.Normal;
			pool.Add(this);
		}
	}

	public class DynamicFontRenderer : dfFontRendererBase, IPoolable
	{
		private static Queue<DynamicFontRenderer> objectPool = new Queue<DynamicFontRenderer>();

		private static Vector2[] OUTLINE_OFFSETS = new Vector2[4]
		{
			new Vector2(-1f, -1f),
			new Vector2(-1f, 1f),
			new Vector2(1f, -1f),
			new Vector2(1f, 1f)
		};

		private static int[] TRIANGLE_INDICES = new int[6] { 0, 1, 3, 3, 1, 2 };

		private static Stack<Color32> textColors = new Stack<Color32>();

		private dfList<LineRenderInfo> lines;

		private dfList<dfMarkupToken> tokens;

		private bool inUse;

		public int LineCount
		{
			get
			{
				return lines.Count;
			}
		}

		public dfAtlas SpriteAtlas { get; set; }

		public dfRenderData SpriteBuffer { get; set; }

		internal DynamicFontRenderer()
		{
		}

		public static dfFontRendererBase Obtain(dfDynamicFont font)
		{
			DynamicFontRenderer dynamicFontRenderer = ((objectPool.Count <= 0) ? new DynamicFontRenderer() : objectPool.Dequeue());
			dynamicFontRenderer.Reset();
			dynamicFontRenderer.Font = font;
			dynamicFontRenderer.inUse = true;
			return dynamicFontRenderer;
		}

		public override void Release()
		{
			if (inUse)
			{
				inUse = false;
				Reset();
				if (tokens != null)
				{
					tokens.Release();
					tokens = null;
				}
				if (lines != null)
				{
					lines.ReleaseItems();
					lines.Release();
					lines = null;
				}
				base.BottomColor = null;
				objectPool.Enqueue(this);
			}
		}

		public override float[] GetCharacterWidths(string text)
		{
			float totalWidth = 0f;
			return GetCharacterWidths(text, 0, text.Length - 1, out totalWidth);
		}

		public float[] GetCharacterWidths(string text, int startIndex, int endIndex, out float totalWidth)
		{
			totalWidth = 0f;
			dfDynamicFont dfDynamicFont2 = (dfDynamicFont)base.Font;
			int size = Mathf.CeilToInt((float)dfDynamicFont2.FontSize * base.TextScale);
			float[] array = new float[text.Length];
			float num = 0f;
			float num2 = 0f;
			dfDynamicFont2.RequestCharacters(text, size, FontStyle.Normal);
			CharacterInfo info = default(CharacterInfo);
			int num3 = startIndex;
			while (num3 <= endIndex)
			{
				if (dfDynamicFont2.BaseFont.GetCharacterInfo(text[num3], out info, size, FontStyle.Normal))
				{
					num2 = ((text[num3] == '\t') ? (num2 + (float)base.TabSize) : ((text[num3] != ' ') ? (num2 + (float)info.maxX) : (num2 + (float)info.advance)));
					array[num3] = (num2 - num) * base.PixelRatio;
				}
				num3++;
				num = num2;
			}
			return array;
		}

		public override Vector2 MeasureString(string text)
		{
			dfDynamicFont dfDynamicFont2 = (dfDynamicFont)base.Font;
			int size = Mathf.CeilToInt((float)dfDynamicFont2.FontSize * base.TextScale);
			dfDynamicFont2.RequestCharacters(text, size, FontStyle.Normal);
			tokenize(text);
			dfList<LineRenderInfo> dfList2 = calculateLinebreaks();
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < dfList2.Count; i++)
			{
				num = Mathf.Max(dfList2[i].lineWidth, num);
				num2 += dfList2[i].lineHeight;
			}
			tokens.Release();
			tokens = null;
			return new Vector2(num, num2);
		}

		public override void Render(string text, dfRenderData destination)
		{
			textColors.Clear();
			textColors.Push(Color.white);
			dfDynamicFont dfDynamicFont2 = (dfDynamicFont)base.Font;
			int size = Mathf.CeilToInt((float)dfDynamicFont2.FontSize * base.TextScale);
			dfDynamicFont2.RequestCharacters(text, size, FontStyle.Normal);
			tokenize(text);
			dfList<LineRenderInfo> dfList2 = calculateLinebreaks();
			destination.EnsureCapacity(getAnticipatedVertCount(tokens));
			int num = 0;
			int num2 = 0;
			Vector3 position = (base.VectorOffset / base.PixelRatio).CeilToInt();
			for (int i = 0; i < dfList2.Count; i++)
			{
				LineRenderInfo lineRenderInfo = dfList2[i];
				int count = destination.Vertices.Count;
				int startIndex = ((SpriteBuffer != null) ? SpriteBuffer.Vertices.Count : 0);
				renderLine(dfList2[i], textColors, position, destination);
				position.y -= lineRenderInfo.lineHeight;
				num = Mathf.Max((int)lineRenderInfo.lineWidth, num);
				num2 += Mathf.CeilToInt(lineRenderInfo.lineHeight);
				if (lineRenderInfo.lineWidth > base.MaxSize.x)
				{
					clipRight(destination, count);
					clipRight(SpriteBuffer, startIndex);
				}
				clipBottom(destination, count);
				clipBottom(SpriteBuffer, startIndex);
			}
			base.RenderedSize = new Vector2(Mathf.Min(base.MaxSize.x, num), Mathf.Min(base.MaxSize.y, num2)) * base.TextScale;
			tokens.Release();
			tokens = null;
		}

		private int getAnticipatedVertCount(dfList<dfMarkupToken> tokens)
		{
			int num = 4 + (base.Shadow ? 4 : 0) + (base.Outline ? 4 : 0);
			int num2 = 0;
			for (int i = 0; i < tokens.Count; i++)
			{
				dfMarkupToken dfMarkupToken2 = tokens[i];
				if (dfMarkupToken2.TokenType == dfMarkupTokenType.Text)
				{
					num2 += num * dfMarkupToken2.Length;
				}
				else if (dfMarkupToken2.TokenType == dfMarkupTokenType.StartTag)
				{
					num2 += 4;
				}
			}
			return num2;
		}

		private void renderLine(LineRenderInfo line, Stack<Color32> colors, Vector3 position, dfRenderData destination)
		{
			position.x += calculateLineAlignment(line);
			for (int i = line.startOffset; i <= line.endOffset; i++)
			{
				dfMarkupToken dfMarkupToken2 = tokens[i];
				switch (dfMarkupToken2.TokenType)
				{
				case dfMarkupTokenType.Text:
					renderText(dfMarkupToken2, colors.Peek(), position, destination);
					break;
				case dfMarkupTokenType.StartTag:
					if (dfMarkupToken2.Matches("sprite") && SpriteAtlas != null && SpriteBuffer != null)
					{
						renderSprite(dfMarkupToken2, colors.Peek(), position, SpriteBuffer);
					}
					else if (dfMarkupToken2.Matches("color"))
					{
						colors.Push(parseColor(dfMarkupToken2));
					}
					break;
				case dfMarkupTokenType.EndTag:
					if (dfMarkupToken2.Matches("color") && colors.Count > 1)
					{
						colors.Pop();
					}
					break;
				}
				position.x += dfMarkupToken2.Width;
				if (dfMarkupToken2.Width > 0)
				{
					position += base.PerCharacterAccumulatedOffset;
				}
			}
		}

		private void renderText(dfMarkupToken token, Color32 color, Vector3 position, dfRenderData renderData)
		{
			try
			{
				dfDynamicFont dfDynamicFont2 = (dfDynamicFont)base.Font;
				int num = Mathf.CeilToInt((float)dfDynamicFont2.FontSize * base.TextScale);
				FontStyle style = FontStyle.Normal;
				CharacterInfo info = default(CharacterInfo);
				int descent = dfDynamicFont2.Descent;
				dfList<Vector3> vertices = renderData.Vertices;
				dfList<int> triangles = renderData.Triangles;
				dfList<Vector2> uV = renderData.UV;
				dfList<Color32> colors = renderData.Colors;
				float num2 = position.x;
				float y = position.y;
				renderData.Material = dfDynamicFont2.Material;
				Color32 color2 = applyOpacity(multiplyColors(color, base.DefaultColor));
				Color32 item = color2;
				if (base.BottomColor.HasValue)
				{
					item = applyOpacity(multiplyColors(color, base.BottomColor.Value));
				}
				for (int i = 0; i < token.Length; i++)
				{
					if (i > 0)
					{
						num2 += (float)base.CharacterSpacing * base.TextScale;
					}
					if (!dfDynamicFont2.baseFont.GetCharacterInfo(token[i], out info, num, style))
					{
						continue;
					}
					int num3 = dfDynamicFont2.FontSize + info.maxY - num + descent;
					float num4 = num2 + (float)info.minX;
					float num5 = y + (float)num3;
					float x = num4 + (float)info.glyphWidth;
					float y2 = num5 - (float)info.glyphHeight;
					Vector3 vector = new Vector3(num4, num5) * base.PixelRatio;
					Vector3 vector2 = new Vector3(x, num5) * base.PixelRatio;
					Vector3 vector3 = new Vector3(x, y2) * base.PixelRatio;
					Vector3 vector4 = new Vector3(num4, y2) * base.PixelRatio;
					if (base.Shadow)
					{
						addTriangleIndices(vertices, triangles);
						Vector3 vector5 = (Vector3)base.ShadowOffset * base.PixelRatio;
						vertices.Add(vector + vector5);
						vertices.Add(vector2 + vector5);
						vertices.Add(vector3 + vector5);
						vertices.Add(vector4 + vector5);
						Color32 item2 = applyOpacity(base.ShadowColor);
						colors.Add(item2);
						colors.Add(item2);
						colors.Add(item2);
						colors.Add(item2);
						addUVCoords(uV, info);
					}
					if (base.Outline)
					{
						for (int j = 0; j < OUTLINE_OFFSETS.Length; j++)
						{
							addTriangleIndices(vertices, triangles);
							Vector3 vector6 = (Vector3)OUTLINE_OFFSETS[j] * (float)base.OutlineSize * base.PixelRatio;
							vertices.Add(vector + vector6);
							vertices.Add(vector2 + vector6);
							vertices.Add(vector3 + vector6);
							vertices.Add(vector4 + vector6);
							Color32 item3 = applyOpacity(base.OutlineColor);
							colors.Add(item3);
							colors.Add(item3);
							colors.Add(item3);
							colors.Add(item3);
							addUVCoords(uV, info);
						}
					}
					addTriangleIndices(vertices, triangles);
					vertices.Add(vector);
					vertices.Add(vector2);
					vertices.Add(vector3);
					vertices.Add(vector4);
					colors.Add(color2);
					colors.Add(color2);
					colors.Add(item);
					colors.Add(item);
					addUVCoords(uV, info);
					num2 += (float)Mathf.CeilToInt(info.maxX);
				}
			}
			finally
			{
			}
		}

		private static void addUVCoords(dfList<Vector2> uvs, CharacterInfo glyph)
		{
			uvs.Add(glyph.uvTopLeft);
			uvs.Add(glyph.uvTopRight);
			uvs.Add(glyph.uvBottomRight);
			uvs.Add(glyph.uvBottomLeft);
		}

		private void renderSprite(dfMarkupToken token, Color32 color, Vector3 position, dfRenderData destination)
		{
			try
			{
				string value = token.GetAttribute(0).Value.Value;
				dfAtlas.ItemInfo itemInfo = SpriteAtlas[value];
				if (!(itemInfo == null))
				{
					dfSprite.RenderOptions renderOptions = default(dfSprite.RenderOptions);
					renderOptions.atlas = SpriteAtlas;
					renderOptions.color = color;
					renderOptions.fillAmount = 1f;
					renderOptions.flip = dfSpriteFlip.None;
					renderOptions.offset = position;
					renderOptions.pixelsToUnits = base.PixelRatio;
					renderOptions.size = new Vector2(token.Width, token.Height);
					renderOptions.spriteInfo = itemInfo;
					dfSprite.RenderOptions options = renderOptions;
					dfSprite.renderSprite(SpriteBuffer, options);
				}
			}
			finally
			{
			}
		}

		private Color32 parseColor(dfMarkupToken token)
		{
			Color color = Color.white;
			if (token.AttributeCount == 1)
			{
				string value = token.GetAttribute(0).Value.Value;
				if (value.Length == 7 && value[0] == '#')
				{
					uint result = 0u;
					uint.TryParse(value.Substring(1), NumberStyles.HexNumber, null, out result);
					color = UIntToColor(result | 0xFF000000u);
				}
				else
				{
					color = dfMarkupStyle.ParseColor(value, base.DefaultColor);
				}
			}
			return applyOpacity(color);
		}

		private Color32 UIntToColor(uint color)
		{
			byte a = (byte)(color >> 24);
			byte r = (byte)(color >> 16);
			byte g = (byte)(color >> 8);
			byte b = (byte)(color >> 0);
			return new Color32(r, g, b, a);
		}

		private dfList<LineRenderInfo> calculateLinebreaks()
		{
			try
			{
				if (lines != null)
				{
					return lines;
				}
				lines = dfList<LineRenderInfo>.Obtain();
				dfDynamicFont dfDynamicFont2 = (dfDynamicFont)base.Font;
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				float num5 = (float)dfDynamicFont2.Baseline * base.TextScale;
				while (num3 < tokens.Count && (float)lines.Count * num5 <= base.MaxSize.y + num5)
				{
					dfMarkupToken dfMarkupToken2 = tokens[num3];
					dfMarkupTokenType tokenType = dfMarkupToken2.TokenType;
					if (tokenType == dfMarkupTokenType.Newline)
					{
						lines.Add(LineRenderInfo.Obtain(num2, num3));
						num2 = (num = ++num3);
						num4 = 0;
						continue;
					}
					int num6 = Mathf.CeilToInt(dfMarkupToken2.Width);
					if (base.WordWrap && num > num2 && (tokenType == dfMarkupTokenType.Text || (tokenType == dfMarkupTokenType.StartTag && dfMarkupToken2.Matches("sprite"))) && (float)(num4 + num6) >= base.MaxSize.x)
					{
						if (num > num2)
						{
							lines.Add(LineRenderInfo.Obtain(num2, num - 1));
							num2 = (num3 = ++num);
							num4 = 0;
						}
						else
						{
							lines.Add(LineRenderInfo.Obtain(num2, num - 1));
							num2 = (num = ++num3);
							num4 = 0;
						}
					}
					else
					{
						if (tokenType == dfMarkupTokenType.Whitespace)
						{
							num = num3;
						}
						num4 += num6;
						num3++;
					}
				}
				if (num2 < tokens.Count)
				{
					lines.Add(LineRenderInfo.Obtain(num2, tokens.Count - 1));
				}
				for (int i = 0; i < lines.Count; i++)
				{
					calculateLineSize(lines[i]);
				}
				return lines;
			}
			finally
			{
			}
		}

		private int calculateLineAlignment(LineRenderInfo line)
		{
			float lineWidth = line.lineWidth;
			if (base.TextAlign == TextAlignment.Left || lineWidth < 1f)
			{
				return 0;
			}
			float num = 0f;
			num = ((base.TextAlign != TextAlignment.Right) ? ((base.MaxSize.x - lineWidth) * 0.5f) : (base.MaxSize.x - lineWidth));
			return Mathf.CeilToInt(Mathf.Max(0f, num));
		}

		private void calculateLineSize(LineRenderInfo line)
		{
			dfDynamicFont dfDynamicFont2 = (dfDynamicFont)base.Font;
			line.lineHeight = (float)dfDynamicFont2.Baseline * base.TextScale;
			int num = 0;
			for (int i = line.startOffset; i <= line.endOffset; i++)
			{
				num += tokens[i].Width;
			}
			line.lineWidth = num;
		}

		private void tokenize(string text)
		{
			try
			{
				if (base.ProcessMarkup)
				{
					tokens = dfMarkupTokenizer.Tokenize(text);
				}
				else
				{
					tokens = dfPlainTextTokenizer.Tokenize(text);
				}
				for (int i = 0; i < tokens.Count; i++)
				{
					calculateTokenRenderSize(tokens[i]);
				}
			}
			finally
			{
			}
		}

		private void calculateTokenRenderSize(dfMarkupToken token)
		{
			try
			{
				float num = 0f;
				char c = '\0';
				dfDynamicFont dfDynamicFont2 = (dfDynamicFont)base.Font;
				CharacterInfo info = default(CharacterInfo);
				if (token.TokenType == dfMarkupTokenType.Text)
				{
					int size = Mathf.CeilToInt((float)dfDynamicFont2.FontSize * base.TextScale);
					for (int i = 0; i < token.Length; i++)
					{
						c = token[i];
						dfDynamicFont2.baseFont.GetCharacterInfo(c, out info, size, FontStyle.Normal);
						num = ((c != '\t') ? (num + ((c == ' ') ? ((float)info.advance + (float)base.CharacterSpacing * base.TextScale) : ((float)info.maxX))) : (num + (float)base.TabSize));
					}
					if (token.Length > 2)
					{
						num += (float)((token.Length - 2) * base.CharacterSpacing) * base.TextScale;
					}
					token.Height = base.Font.LineHeight;
					token.Width = Mathf.CeilToInt(num);
				}
				else if (token.TokenType == dfMarkupTokenType.Whitespace)
				{
					int size2 = Mathf.CeilToInt((float)dfDynamicFont2.FontSize * base.TextScale);
					float num2 = (float)base.CharacterSpacing * base.TextScale;
					for (int j = 0; j < token.Length; j++)
					{
						c = token[j];
						switch (c)
						{
						case '\t':
							num += (float)base.TabSize;
							break;
						case ' ':
							dfDynamicFont2.baseFont.GetCharacterInfo(c, out info, size2, FontStyle.Normal);
							num += (float)info.advance + num2;
							break;
						}
					}
					token.Height = base.Font.LineHeight;
					token.Width = Mathf.CeilToInt(num);
				}
				else if (token.TokenType == dfMarkupTokenType.StartTag && token.Matches("sprite") && SpriteAtlas != null && token.AttributeCount == 1)
				{
					Texture2D texture = SpriteAtlas.Texture;
					float num3 = (float)dfDynamicFont2.Baseline * base.TextScale;
					string value = token.GetAttribute(0).Value.Value;
					dfAtlas.ItemInfo itemInfo = SpriteAtlas[value];
					if (itemInfo != null)
					{
						float num4 = itemInfo.region.width * (float)texture.width / (itemInfo.region.height * (float)texture.height);
						num = Mathf.CeilToInt(num3 * num4);
					}
					token.Height = Mathf.CeilToInt(num3);
					token.Width = Mathf.CeilToInt(num);
				}
			}
			finally
			{
			}
		}

		private float getTabStop(float position)
		{
			float num = base.PixelRatio * base.TextScale;
			if (base.TabStops != null && base.TabStops.Count > 0)
			{
				for (int i = 0; i < base.TabStops.Count; i++)
				{
					if ((float)base.TabStops[i] * num > position)
					{
						return (float)base.TabStops[i] * num;
					}
				}
			}
			if (base.TabSize > 0)
			{
				return position + (float)base.TabSize * num;
			}
			return position + (float)(base.Font.FontSize * 4) * num;
		}

		private void clipRight(dfRenderData destination, int startIndex)
		{
			if (destination == null)
			{
				return;
			}
			float num = base.VectorOffset.x + base.MaxSize.x * base.PixelRatio;
			dfList<Vector3> vertices = destination.Vertices;
			dfList<Vector2> uV = destination.UV;
			for (int i = startIndex; i < vertices.Count; i += 4)
			{
				Vector3 vector = vertices[i];
				Vector3 vector2 = vertices[i + 1];
				Vector3 vector3 = vertices[i + 2];
				Vector3 vector4 = vertices[i + 3];
				float num2 = vector2.x - vector.x;
				if (vector2.x > num)
				{
					float t = 1f - (num - vector2.x + num2) / num2;
					vector = (vertices[i] = new Vector3(Mathf.Min(vector.x, num), vector.y, vector.z));
					vector2 = (vertices[i + 1] = new Vector3(Mathf.Min(vector2.x, num), vector2.y, vector2.z));
					vector3 = (vertices[i + 2] = new Vector3(Mathf.Min(vector3.x, num), vector3.y, vector3.z));
					vector4 = (vertices[i + 3] = new Vector3(Mathf.Min(vector4.x, num), vector4.y, vector4.z));
					float x = Mathf.Lerp(uV[i + 1].x, uV[i].x, t);
					uV[i + 1] = new Vector2(x, uV[i + 1].y);
					uV[i + 2] = new Vector2(x, uV[i + 2].y);
					num2 = vector2.x - vector.x;
				}
			}
		}

		private void clipBottom(dfRenderData destination, int startIndex)
		{
			if (destination == null)
			{
				return;
			}
			float num = base.VectorOffset.y - base.MaxSize.y * base.PixelRatio;
			dfList<Vector3> vertices = destination.Vertices;
			dfList<Vector2> uV = destination.UV;
			dfList<Color32> colors = destination.Colors;
			for (int i = startIndex; i < vertices.Count; i += 4)
			{
				Vector3 vector = vertices[i];
				Vector3 vector2 = vertices[i + 1];
				Vector3 vector3 = vertices[i + 2];
				Vector3 vector4 = vertices[i + 3];
				float num2 = vector.y - vector4.y;
				if (vector4.y <= num)
				{
					float t = 1f - Mathf.Abs(0f - num + vector.y) / num2;
					vector = (vertices[i] = new Vector3(vector.x, Mathf.Max(vector.y, num), vector2.z));
					vector2 = (vertices[i + 1] = new Vector3(vector2.x, Mathf.Max(vector2.y, num), vector2.z));
					vector3 = (vertices[i + 2] = new Vector3(vector3.x, Mathf.Max(vector3.y, num), vector3.z));
					vector4 = (vertices[i + 3] = new Vector3(vector4.x, Mathf.Max(vector4.y, num), vector4.z));
					uV[i + 3] = Vector2.Lerp(uV[i + 3], uV[i], t);
					uV[i + 2] = Vector2.Lerp(uV[i + 2], uV[i + 1], t);
					Color color = Color.Lerp(colors[i + 3], colors[i], t);
					colors[i + 3] = color;
					colors[i + 2] = color;
				}
			}
		}

		private Color32 applyOpacity(Color32 color)
		{
			color.a = (byte)(base.Opacity * 255f);
			return color;
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

		private Color multiplyColors(Color lhs, Color rhs)
		{
			return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
		}
	}

	private class LineRenderInfo : IPoolable
	{
		public int startOffset;

		public int endOffset;

		public float lineWidth;

		public float lineHeight;

		private static dfList<LineRenderInfo> pool = new dfList<LineRenderInfo>();

		public int length
		{
			get
			{
				return endOffset - startOffset + 1;
			}
		}

		private LineRenderInfo()
		{
		}

		public static LineRenderInfo Obtain(int start, int end)
		{
			LineRenderInfo lineRenderInfo = ((pool.Count <= 0) ? new LineRenderInfo() : pool.Pop());
			lineRenderInfo.startOffset = start;
			lineRenderInfo.endOffset = end;
			lineRenderInfo.lineHeight = 0f;
			return lineRenderInfo;
		}

		public void Release()
		{
			startOffset = (endOffset = 0);
			lineWidth = (lineHeight = 0f);
			pool.Add(this);
		}
	}

	private static List<dfDynamicFont> loadedFonts = new List<dfDynamicFont>();

	[SerializeField]
	private Font baseFont;

	[SerializeField]
	private Material material;

	[SerializeField]
	private Shader shader;

	[SerializeField]
	private int baseFontSize = -1;

	[SerializeField]
	private int baseline = -1;

	[SerializeField]
	private int lineHeight;

	protected dfList<FontCharacterRequest> requests = new dfList<FontCharacterRequest>();

	public override Material Material
	{
		get
		{
			if (baseFont != null && material != null)
			{
				material.mainTexture = baseFont.material.mainTexture;
				material.shader = Shader;
			}
			return material;
		}
		set
		{
			if (value != material)
			{
				material = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public Shader Shader
	{
		get
		{
			if (shader == null)
			{
				shader = Shader.Find("Daikon Forge/Dynamic Font Shader");
			}
			return shader;
		}
		set
		{
			shader = value;
			dfGUIManager.RefreshAll();
		}
	}

	public override Texture Texture
	{
		get
		{
			return baseFont.material.mainTexture;
		}
	}

	public override bool IsValid
	{
		get
		{
			return baseFont != null && Material != null && Texture != null;
		}
	}

	public override int FontSize
	{
		get
		{
			return baseFontSize;
		}
		set
		{
			if (value != baseFontSize)
			{
				baseFontSize = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public override int LineHeight
	{
		get
		{
			return lineHeight;
		}
		set
		{
			if (value != lineHeight)
			{
				lineHeight = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public Font BaseFont
	{
		get
		{
			return baseFont;
		}
		set
		{
			if (value != baseFont)
			{
				baseFont = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public int Baseline
	{
		get
		{
			return baseline;
		}
		set
		{
			if (value != baseline)
			{
				baseline = value;
				dfGUIManager.RefreshAll();
			}
		}
	}

	public int Descent
	{
		get
		{
			return LineHeight - baseline;
		}
	}

	public override dfFontRendererBase ObtainRenderer()
	{
		return DynamicFontRenderer.Obtain(this);
	}

	public static dfDynamicFont FindByName(string name)
	{
		for (int i = 0; i < loadedFonts.Count; i++)
		{
			if (string.Equals(loadedFonts[i].name, name, StringComparison.OrdinalIgnoreCase))
			{
				return loadedFonts[i];
			}
		}
		GameObject gameObject = BraveResources.Load(name) as GameObject;
		if (gameObject == null)
		{
			return null;
		}
		dfDynamicFont component = gameObject.GetComponent<dfDynamicFont>();
		if (component == null)
		{
			return null;
		}
		loadedFonts.Add(component);
		return component;
	}

	public Vector2 MeasureText(string text, int size, FontStyle style)
	{
		RequestCharacters(text, size, style);
		float num = (float)size / (float)FontSize;
		int num2 = Mathf.CeilToInt((float)Baseline * num);
		Vector2 result = new Vector2(0f, num2);
		CharacterInfo info = default(CharacterInfo);
		for (int i = 0; i < text.Length; i++)
		{
			BaseFont.GetCharacterInfo(text[i], out info, size, style);
			float num3 = Mathf.Ceil(info.maxX);
			if (text[i] == ' ')
			{
				num3 = Mathf.Ceil(info.advance);
			}
			else if (text[i] == '\t')
			{
				num3 += (float)(size * 4);
			}
			result.x += num3;
		}
		return result;
	}

	public void RequestCharacters(string text, int size, FontStyle style)
	{
		if (baseFont == null)
		{
			throw new NullReferenceException("Base Font not assigned: " + base.name);
		}
		dfFontManager.Invalidate(this);
		baseFont.RequestCharactersInTexture(text, size, style);
	}

	public virtual void AddCharacterRequest(string characters, int fontSize, FontStyle style)
	{
		dfFontManager.FlagPendingRequests(this);
		FontCharacterRequest fontCharacterRequest = FontCharacterRequest.Obtain();
		fontCharacterRequest.Characters = characters;
		fontCharacterRequest.FontSize = fontSize;
		fontCharacterRequest.FontStyle = style;
		requests.Add(fontCharacterRequest);
	}

	public virtual void FlushCharacterRequests()
	{
		for (int i = 0; i < requests.Count; i++)
		{
			FontCharacterRequest fontCharacterRequest = requests[i];
			baseFont.RequestCharactersInTexture(fontCharacterRequest.Characters, fontCharacterRequest.FontSize, fontCharacterRequest.FontStyle);
		}
		requests.ReleaseItems();
	}
}
