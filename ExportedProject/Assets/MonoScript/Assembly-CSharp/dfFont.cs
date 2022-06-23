using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Font Definition")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_font.html")]
public class dfFont : dfFontBase
{
	private class GlyphKerningList
	{
		private Dictionary<int, int> list = new Dictionary<int, int>();

		public void Add(GlyphKerning kerning)
		{
			list[kerning.second] = kerning.amount;
		}

		public int GetKerning(int firstCharacter, int secondCharacter)
		{
			int value = 0;
			list.TryGetValue(secondCharacter, out value);
			return value;
		}
	}

	[Serializable]
	public class GlyphKerning : IComparable<GlyphKerning>
	{
		public int first;

		public int second;

		public int amount;

		public int CompareTo(GlyphKerning other)
		{
			if (first == other.first)
			{
				return second.CompareTo(other.second);
			}
			return first.CompareTo(other.first);
		}
	}

	[Serializable]
	public class GlyphDefinition : IComparable<GlyphDefinition>
	{
		[SerializeField]
		public int id;

		[SerializeField]
		public int x;

		[SerializeField]
		public int y;

		[SerializeField]
		public int width;

		[SerializeField]
		public int height;

		[SerializeField]
		public int xoffset;

		[SerializeField]
		public int yoffset;

		[SerializeField]
		public int xadvance;

		[SerializeField]
		public bool rotated;

		public int CompareTo(GlyphDefinition other)
		{
			return id.CompareTo(other.id);
		}
	}

	public class BitmappedFontRenderer : dfFontRendererBase, IPoolable
	{
		private static Queue<BitmappedFontRenderer> objectPool = new Queue<BitmappedFontRenderer>();

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

		public int LineCount
		{
			get
			{
				return lines.Count;
			}
		}

		internal BitmappedFontRenderer()
		{
		}

		public static dfFontRendererBase Obtain(dfFont font)
		{
			BitmappedFontRenderer bitmappedFontRenderer = ((objectPool.Count <= 0) ? new BitmappedFontRenderer() : objectPool.Dequeue());
			bitmappedFontRenderer.Reset();
			bitmappedFontRenderer.Font = font;
			return bitmappedFontRenderer;
		}

		public override void Release()
		{
			Reset();
			if (tokens != null)
			{
				tokens.ReleaseItems();
				tokens.Release();
			}
			tokens = null;
			if (lines != null)
			{
				lines.Release();
				lines = null;
			}
			LineRenderInfo.ResetPool();
			base.BottomColor = null;
			objectPool.Enqueue(this);
		}

		public override float[] GetCharacterWidths(string text)
		{
			float totalWidth = 0f;
			return GetCharacterWidths(text, 0, text.Length - 1, out totalWidth);
		}

		public float[] GetCharacterWidths(string text, int startIndex, int endIndex, out float totalWidth)
		{
			totalWidth = 0f;
			dfFont dfFont2 = (dfFont)base.Font;
			float[] array = new float[text.Length];
			float num = base.TextScale * base.PixelRatio;
			float num2 = (float)base.CharacterSpacing * num;
			for (int i = startIndex; i <= endIndex; i++)
			{
				GlyphDefinition glyph = dfFont2.GetGlyph(text[i]);
				if (glyph != null)
				{
					if (i > 0)
					{
						array[i - 1] += num2;
						totalWidth += num2;
					}
					totalWidth += (array[i] = (float)glyph.xadvance * num);
				}
			}
			return array;
		}

		public override Vector2 MeasureString(string text)
		{
			tokenize(text);
			dfList<LineRenderInfo> dfList2 = calculateLinebreaks();
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < dfList2.Count; i++)
			{
				num = Mathf.Max((int)dfList2[i].lineWidth, num);
				num2 += (int)dfList2[i].lineHeight;
			}
			return new Vector2(num, num2) * base.TextScale;
		}

		public override void Render(string text, dfRenderData destination)
		{
			textColors.Clear();
			textColors.Push(Color.white);
			tokenize(text);
			dfList<LineRenderInfo> dfList2 = calculateLinebreaks();
			destination.EnsureCapacity(getAnticipatedVertCount(tokens));
			int num = 0;
			int num2 = 0;
			Vector3 vectorOffset = base.VectorOffset;
			float num3 = base.TextScale * base.PixelRatio;
			for (int i = 0; i < dfList2.Count; i++)
			{
				LineRenderInfo lineRenderInfo = dfList2[i];
				int count = destination.Vertices.Count;
				renderLine(dfList2[i], textColors, vectorOffset, destination);
				vectorOffset.y -= (float)base.Font.LineHeight * num3;
				num = Mathf.Max((int)lineRenderInfo.lineWidth, num);
				num2 += (int)lineRenderInfo.lineHeight;
				if (lineRenderInfo.lineWidth * base.TextScale > base.MaxSize.x)
				{
					clipRight(destination, count);
				}
				if ((float)num2 * base.TextScale > base.MaxSize.y)
				{
					clipBottom(destination, count);
				}
			}
			base.RenderedSize = new Vector2(Mathf.Min(base.MaxSize.x, num), Mathf.Min(base.MaxSize.y, num2)) * base.TextScale;
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
			float num = base.TextScale * base.PixelRatio;
			position.x += (float)calculateLineAlignment(line) * num;
			for (int i = line.startOffset; i <= line.endOffset; i++)
			{
				dfMarkupToken dfMarkupToken2 = tokens[i];
				switch (dfMarkupToken2.TokenType)
				{
				case dfMarkupTokenType.Text:
					renderText(dfMarkupToken2, colors.Peek(), position, destination);
					position += base.PerCharacterAccumulatedOffset * dfMarkupToken2.Length;
					break;
				case dfMarkupTokenType.StartTag:
					if (dfMarkupToken2.Matches("sprite"))
					{
						renderSprite(dfMarkupToken2, colors.Peek(), position, destination);
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
				position.x += (float)dfMarkupToken2.Width * num;
			}
		}

		private void renderText(dfMarkupToken token, Color32 color, Vector3 position, dfRenderData destination)
		{
			try
			{
				dfList<Vector3> vertices = destination.Vertices;
				dfList<int> triangles = destination.Triangles;
				dfList<Color32> colors = destination.Colors;
				dfList<Vector2> uV = destination.UV;
				dfFont dfFont2 = (dfFont)base.Font;
				dfAtlas.ItemInfo itemInfo = dfFont2.Atlas[dfFont2.sprite];
				Texture texture = dfFont2.Texture;
				float num = 1f / (float)texture.width;
				float num2 = 1f / (float)texture.height;
				float num3 = base.TextScale * base.PixelRatio;
				char previousChar = '\0';
				char c = '\0';
				Color32 color2 = applyOpacity(multiplyColors(color, base.DefaultColor));
				Color32 item = color2;
				if (base.BottomColor.HasValue)
				{
					item = applyOpacity(multiplyColors(color, base.BottomColor.Value));
				}
				int num4 = 0;
				while (num4 < token.Length)
				{
					c = token[num4];
					if (c != 0)
					{
						GlyphDefinition glyph = dfFont2.GetGlyph(c);
						if (glyph != null)
						{
							int kerning = dfFont2.GetKerning(previousChar, c);
							float num5 = position.x + (float)(glyph.xoffset + kerning) * num3;
							float num6 = position.y - (float)glyph.yoffset * num3;
							float num7 = (float)glyph.width * num3;
							float num8 = (float)glyph.height * num3;
							float x = num5 + num7;
							float y = num6 - num8;
							Vector3 vector = new Vector3(num5, num6);
							Vector3 vector2 = new Vector3(x, num6);
							Vector3 vector3 = new Vector3(x, y);
							Vector3 vector4 = new Vector3(num5, y);
							float num9 = itemInfo.region.x + (float)glyph.x * num;
							float num10 = itemInfo.region.yMax - (float)glyph.y * num2;
							float num11 = num9 + (float)glyph.width * num;
							float num12 = num10 - (float)glyph.height * num2;
							if (base.Shadow)
							{
								addTriangleIndices(vertices, triangles);
								Vector3 vector5 = (Vector3)base.ShadowOffset * num3;
								vertices.Add(vector + vector5);
								vertices.Add(vector2 + vector5);
								vertices.Add(vector3 + vector5);
								vertices.Add(vector4 + vector5);
								Color32 item2 = applyOpacity(base.ShadowColor);
								colors.Add(item2);
								colors.Add(item2);
								colors.Add(item2);
								colors.Add(item2);
								uV.Add(new Vector2(num9, num10));
								uV.Add(new Vector2(num11, num10));
								uV.Add(new Vector2(num11, num12));
								uV.Add(new Vector2(num9, num12));
							}
							if (base.Outline)
							{
								for (int i = 0; i < OUTLINE_OFFSETS.Length; i++)
								{
									addTriangleIndices(vertices, triangles);
									Vector3 vector6 = (Vector3)OUTLINE_OFFSETS[i] * (float)base.OutlineSize * num3;
									vertices.Add(vector + vector6);
									vertices.Add(vector2 + vector6);
									vertices.Add(vector3 + vector6);
									vertices.Add(vector4 + vector6);
									Color32 item3 = applyOpacity(base.OutlineColor);
									colors.Add(item3);
									colors.Add(item3);
									colors.Add(item3);
									colors.Add(item3);
									uV.Add(new Vector2(num9, num10));
									uV.Add(new Vector2(num11, num10));
									uV.Add(new Vector2(num11, num12));
									uV.Add(new Vector2(num9, num12));
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
							if (destination.Glitchy)
							{
								float num13 = num11 - num9;
								float num14 = num12 - num10;
								float num15 = UnityEngine.Random.value * num13 * UnityEngine.Random.Range(2f, 3f);
								float num16 = UnityEngine.Random.value * num14 * UnityEngine.Random.Range(2f, 3f);
								num13 *= UnityEngine.Random.Range(2f, 5f);
								num14 *= UnityEngine.Random.Range(2f, 5f);
								Vector2 vector7 = new Vector2(num9 - num15, num10 - num16);
								uV.Add(vector7);
								uV.Add(vector7 + new Vector2(num13, 0f));
								uV.Add(vector7 + new Vector2(num13, num14));
								uV.Add(vector7 + new Vector2(0f, num14));
							}
							else
							{
								uV.Add(new Vector2(num9, num10));
								uV.Add(new Vector2(num11, num10));
								uV.Add(new Vector2(num11, num12));
								uV.Add(new Vector2(num9, num12));
							}
							position.x += (float)(glyph.xadvance + kerning + base.CharacterSpacing) * num3;
							position += base.PerCharacterAccumulatedOffset;
						}
					}
					num4++;
					previousChar = c;
				}
			}
			finally
			{
			}
		}

		private void renderSprite(dfMarkupToken token, Color32 color, Vector3 position, dfRenderData destination)
		{
			try
			{
				dfList<Vector3> vertices = destination.Vertices;
				dfList<int> triangles = destination.Triangles;
				dfList<Color32> colors = destination.Colors;
				dfList<Vector2> uV = destination.UV;
				dfFont dfFont2 = (dfFont)base.Font;
				string value = token.GetAttribute(0).Value.Value;
				dfAtlas.ItemInfo itemInfo = dfFont2.Atlas[value];
				if (!(itemInfo == null))
				{
					float num = (float)token.Height * base.TextScale * base.PixelRatio;
					float num2 = (float)token.Width * base.TextScale * base.PixelRatio;
					float x = position.x;
					float num3 = position.y;
					if (base.Font.IsSpriteScaledUIFont())
					{
						num3 = position.y + (float)token.Height * base.TextScale * base.PixelRatio * 0.2f;
					}
					int count = vertices.Count;
					vertices.Add(new Vector3(x, num3));
					vertices.Add(new Vector3(x + num2, num3));
					vertices.Add(new Vector3(x + num2, num3 - num));
					vertices.Add(new Vector3(x, num3 - num));
					triangles.Add(count);
					triangles.Add(count + 1);
					triangles.Add(count + 3);
					triangles.Add(count + 3);
					triangles.Add(count + 1);
					triangles.Add(count + 2);
					Color32 item = ((!base.ColorizeSymbols) ? applyOpacity(base.DefaultColor) : applyOpacity(color));
					colors.Add(item);
					colors.Add(item);
					colors.Add(item);
					colors.Add(item);
					Rect region = itemInfo.region;
					uV.Add(new Vector2(region.x, region.yMax));
					uV.Add(new Vector2(region.xMax, region.yMax));
					uV.Add(new Vector2(region.xMax, region.y));
					uV.Add(new Vector2(region.x, region.y));
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
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				float num5 = (float)base.Font.LineHeight * base.TextScale;
				while (num3 < tokens.Count && (float)lines.Count * num5 < base.MaxSize.y)
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
					int num6 = Mathf.CeilToInt((float)dfMarkupToken2.Width * base.TextScale);
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
						if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
						{
							num2--;
						}
						continue;
					}
					switch (tokenType)
					{
					case dfMarkupTokenType.Whitespace:
						num = num3;
						break;
					case dfMarkupTokenType.Text:
						if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
						{
							num = num3;
						}
						break;
					}
					num4 += num6;
					num3++;
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
			if (base.TextAlign == TextAlignment.Left || lineWidth == 0f)
			{
				return 0;
			}
			int num = 0;
			num = ((base.TextAlign != TextAlignment.Right) ? Mathf.FloorToInt((base.MaxSize.x / base.TextScale - lineWidth) * 0.5f) : Mathf.FloorToInt(base.MaxSize.x / base.TextScale - lineWidth));
			return Mathf.Max(0, num);
		}

		private void calculateLineSize(LineRenderInfo line)
		{
			line.lineHeight = base.Font.LineHeight;
			int num = 0;
			for (int i = line.startOffset; i <= line.endOffset; i++)
			{
				num += tokens[i].Width;
			}
			line.lineWidth = num;
		}

		private dfList<dfMarkupToken> tokenize(string text)
		{
			try
			{
				if (tokens != null)
				{
					if (object.ReferenceEquals(tokens[0].Source, text))
					{
						return tokens;
					}
					tokens.ReleaseItems();
					tokens.Release();
				}
				if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE)
				{
					tokens = dfJapaneseMarkupTokenizer.Tokenize(text);
				}
				else if (base.ProcessMarkup)
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
				return tokens;
			}
			finally
			{
			}
		}

		private void calculateTokenRenderSize(dfMarkupToken token)
		{
			try
			{
				dfFont dfFont2 = (dfFont)base.Font;
				int num = 0;
				char previousChar = '\0';
				char c = '\0';
				bool flag = token.TokenType == dfMarkupTokenType.Whitespace || token.TokenType == dfMarkupTokenType.Text;
				bool flag2 = false;
				if (flag)
				{
					int num2 = 0;
					while (num2 < token.Length)
					{
						c = token[num2];
						if (c == '\t')
						{
							num += base.TabSize;
						}
						else
						{
							GlyphDefinition glyph = dfFont2.GetGlyph(c);
							if (glyph != null)
							{
								if (num2 > 0)
								{
									num += dfFont2.GetKerning(previousChar, c);
									num += base.CharacterSpacing;
								}
								num += glyph.xadvance;
							}
						}
						num2++;
						previousChar = c;
					}
				}
				else if (token.TokenType == dfMarkupTokenType.StartTag && token.Matches("sprite"))
				{
					if (token.AttributeCount < 1)
					{
						throw new Exception("Missing sprite name in markup");
					}
					Texture texture = dfFont2.Texture;
					int lineHeight = dfFont2.LineHeight;
					string value = token.GetAttribute(0).Value.Value;
					dfAtlas.ItemInfo itemInfo = dfFont2.atlas[value];
					if (itemInfo != null)
					{
						float num3 = itemInfo.region.width * (float)texture.width / (itemInfo.region.height * (float)texture.height);
						num = Mathf.CeilToInt((float)lineHeight * num3);
						float num4 = 1f;
						if (base.Font.IsSpriteScaledUIFont())
						{
							num4 = 5f;
						}
						flag2 = true;
						token.Height = Mathf.CeilToInt(itemInfo.region.height * (float)texture.height * num4);
						token.Width = Mathf.CeilToInt(itemInfo.region.width * (float)texture.width * num4);
					}
				}
				if (!flag2)
				{
					token.Height = base.Font.LineHeight;
					token.Width = num;
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
					float y = Mathf.Lerp(uV[i + 3].y, uV[i].y, t);
					uV[i + 3] = new Vector2(uV[i + 3].x, y);
					uV[i + 2] = new Vector2(uV[i + 2].x, y);
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
			for (int i = 0; i < TRIANGLE_INDICES.Length; i++)
			{
				triangles.Add(count + TRIANGLE_INDICES[i]);
			}
		}

		private Color multiplyColors(Color lhs, Color rhs)
		{
			return new Color(lhs.r * rhs.r, lhs.g * rhs.g, lhs.b * rhs.b, lhs.a * rhs.a);
		}
	}

	private class LineRenderInfo
	{
		public int startOffset;

		public int endOffset;

		public float lineWidth;

		public float lineHeight;

		private static dfList<LineRenderInfo> pool = new dfList<LineRenderInfo>();

		private static int poolIndex = 0;

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

		public static void ResetPool()
		{
			poolIndex = 0;
		}

		public static LineRenderInfo Obtain(int start, int end)
		{
			if (poolIndex >= pool.Count - 1)
			{
				pool.Add(new LineRenderInfo());
			}
			LineRenderInfo lineRenderInfo = pool[poolIndex++];
			lineRenderInfo.startOffset = start;
			lineRenderInfo.endOffset = end;
			lineRenderInfo.lineHeight = 0f;
			return lineRenderInfo;
		}
	}

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string sprite;

	[SerializeField]
	protected string face = string.Empty;

	[SerializeField]
	protected int size;

	[SerializeField]
	protected bool bold;

	[SerializeField]
	protected bool italic;

	[SerializeField]
	protected string charset;

	[SerializeField]
	protected int stretchH;

	[SerializeField]
	protected bool smooth;

	[SerializeField]
	protected int aa;

	[SerializeField]
	protected int[] padding;

	[SerializeField]
	protected int[] spacing;

	[SerializeField]
	protected int outline;

	[SerializeField]
	protected int lineHeight;

	[SerializeField]
	private List<GlyphDefinition> glyphs = new List<GlyphDefinition>();

	[SerializeField]
	protected List<GlyphKerning> kerning = new List<GlyphKerning>();

	private Dictionary<int, GlyphDefinition> glyphMap;

	private Dictionary<int, GlyphKerningList> kerningMap;

	public List<GlyphDefinition> Glyphs
	{
		get
		{
			return glyphs;
		}
	}

	public List<GlyphKerning> KerningInfo
	{
		get
		{
			return kerning;
		}
	}

	public dfAtlas Atlas
	{
		get
		{
			return atlas;
		}
		set
		{
			if (value != atlas)
			{
				atlas = value;
				glyphMap = null;
			}
		}
	}

	public override Material Material
	{
		get
		{
			return Atlas.Material;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public override Texture Texture
	{
		get
		{
			return Atlas.Texture;
		}
	}

	public string Sprite
	{
		get
		{
			return sprite;
		}
		set
		{
			if (value != sprite)
			{
				sprite = value;
				glyphMap = null;
			}
		}
	}

	public override bool IsValid
	{
		get
		{
			if (Atlas == null || Atlas[Sprite] == null)
			{
				return false;
			}
			return true;
		}
	}

	public string FontFace
	{
		get
		{
			return face;
		}
	}

	public override int FontSize
	{
		get
		{
			return size;
		}
		set
		{
			throw new InvalidOperationException();
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
			lineHeight = value;
		}
	}

	public bool Bold
	{
		get
		{
			return bold;
		}
	}

	public bool Italic
	{
		get
		{
			return italic;
		}
	}

	public int[] Padding
	{
		get
		{
			return padding;
		}
	}

	public int[] Spacing
	{
		get
		{
			return spacing;
		}
	}

	public int Outline
	{
		get
		{
			return outline;
		}
	}

	public int Count
	{
		get
		{
			return glyphs.Count;
		}
	}

	public void OnEnable()
	{
		glyphMap = null;
	}

	public override dfFontRendererBase ObtainRenderer()
	{
		return BitmappedFontRenderer.Obtain(this);
	}

	public void AddKerning(int first, int second, int amount)
	{
		kerning.Add(new GlyphKerning
		{
			first = first,
			second = second,
			amount = amount
		});
	}

	public int GetKerning(char previousChar, char currentChar)
	{
		try
		{
			if (kerningMap == null)
			{
				buildKerningMap();
			}
			GlyphKerningList value = null;
			if (!kerningMap.TryGetValue(previousChar, out value))
			{
				return 0;
			}
			return value.GetKerning(previousChar, currentChar);
		}
		finally
		{
		}
	}

	private void buildKerningMap()
	{
		Dictionary<int, GlyphKerningList> dictionary = (kerningMap = new Dictionary<int, GlyphKerningList>());
		for (int i = 0; i < kerning.Count; i++)
		{
			GlyphKerning glyphKerning = kerning[i];
			if (!dictionary.ContainsKey(glyphKerning.first))
			{
				dictionary[glyphKerning.first] = new GlyphKerningList();
			}
			GlyphKerningList glyphKerningList = dictionary[glyphKerning.first];
			glyphKerningList.Add(glyphKerning);
		}
	}

	public GlyphDefinition GetGlyph(char id)
	{
		if (glyphMap == null)
		{
			glyphMap = new Dictionary<int, GlyphDefinition>();
			for (int i = 0; i < glyphs.Count; i++)
			{
				GlyphDefinition glyphDefinition = glyphs[i];
				glyphMap[glyphDefinition.id] = glyphDefinition;
			}
		}
		GlyphDefinition value = null;
		glyphMap.TryGetValue(id, out value);
		return value;
	}
}
