using System.Collections.Generic;
using UnityEngine;

public static class tk2dTextGeomGen
{
	public class GeomData
	{
		internal tk2dTextMeshData textMeshData;

		internal tk2dFontData fontInst;

		internal string formattedText = string.Empty;
	}

	private static GeomData tmpData = new GeomData();

	private static readonly Color32[] channelSelectColors = new Color32[4]
	{
		new Color32(0, 0, byte.MaxValue, 0),
		new Color(0f, 255f, 0f, 0f),
		new Color(255f, 0f, 0f, 0f),
		new Color(0f, 0f, 0f, 255f)
	};

	private static Color32 meshTopColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private static Color32 meshBottomColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	private static float meshGradientTexU = 0f;

	private static int curGradientCount = 1;

	private static Color32 errorColor = new Color32(byte.MaxValue, 0, byte.MaxValue, byte.MaxValue);

	public static List<Vector3> inlineSpriteOffsetsForLastString;

	public static GeomData Data(tk2dTextMeshData textMeshData, tk2dFontData fontData, string formattedText)
	{
		tmpData.textMeshData = textMeshData;
		tmpData.fontInst = fontData;
		tmpData.formattedText = formattedText;
		return tmpData;
	}

	public static Vector2 GetMeshDimensionsForString(string str, GeomData geomData)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		float b = 0f;
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		int num3 = 0;
		for (int i = 0; i < str.Length && num3 < textMeshData.maxChars; i++)
		{
			if (flag)
			{
				flag = false;
				continue;
			}
			int num4 = str[i];
			if (num4 == 10)
			{
				b = Mathf.Max(num, b);
				num = 0f;
				num2 -= (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
				continue;
			}
			if (textMeshData.inlineStyling && num4 == 94 && i + 1 < str.Length)
			{
				if (str[i + 1] != '^')
				{
					int num5 = 0;
					switch (str[i + 1])
					{
					case 'c':
						num5 = 5;
						break;
					case 'C':
						num5 = 9;
						break;
					case 'g':
						num5 = 9;
						break;
					case 'G':
						num5 = 17;
						break;
					}
					i += num5;
					continue;
				}
				flag = true;
			}
			bool flag2 = num4 == 94;
			tk2dFontChar tk2dFontChar2;
			if (fontInst.useDictionary)
			{
				if (!fontInst.charDict.ContainsKey(num4))
				{
					num4 = 0;
				}
				tk2dFontChar2 = fontInst.charDict[num4];
			}
			else
			{
				if (num4 >= fontInst.chars.Length)
				{
					num4 = 0;
				}
				tk2dFontChar2 = fontInst.chars[num4];
			}
			if (flag2)
			{
				num4 = 94;
			}
			num += (tk2dFontChar2.advance + textMeshData.spacing) * textMeshData.scale.x;
			if (textMeshData.kerning && i < str.Length - 1)
			{
				tk2dFontKerning[] kerning = fontInst.kerning;
				foreach (tk2dFontKerning tk2dFontKerning2 in kerning)
				{
					if (tk2dFontKerning2.c0 == str[i] && tk2dFontKerning2.c1 == str[i + 1])
					{
						num += tk2dFontKerning2.amount * textMeshData.scale.x;
						break;
					}
				}
			}
			num3++;
		}
		b = Mathf.Max(num, b);
		num2 -= (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
		return new Vector2(b, num2);
	}

	public static float GetYAnchorForHeight(float textHeight, GeomData geomData)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		int num = (int)textMeshData.anchor / 3;
		float num2 = (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
		switch (num)
		{
		case 0:
			return 0f - num2;
		case 1:
		{
			float num3 = (0f - textHeight) / 2f - num2;
			if (fontInst.version >= 2)
			{
				float num4 = fontInst.texelSize.y * textMeshData.scale.y;
				return Mathf.Floor(num3 / num4) * num4;
			}
			return num3;
		}
		case 2:
			return 0f - textHeight - num2;
		default:
			return 0f - num2;
		}
	}

	public static float GetXAnchorForWidth(float lineWidth, GeomData geomData)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		switch ((int)textMeshData.anchor % 3)
		{
		case 0:
			return 0f;
		case 1:
		{
			float num = (0f - lineWidth) / 2f;
			if (fontInst.version >= 2)
			{
				float num2 = fontInst.texelSize.x * textMeshData.scale.x;
				return Mathf.Floor(num / num2) * num2;
			}
			return num;
		}
		case 2:
			return 0f - lineWidth;
		default:
			return 0f;
		}
	}

	private static void PostAlignTextData(Vector3[] pos, int offset, int targetStart, int targetEnd, float offsetX, List<int> inlineSpritePositions = null)
	{
		for (int i = targetStart * 4; i < targetEnd * 4; i++)
		{
			Vector3 vector = pos[offset + i];
			vector.x += offsetX;
			pos[offset + i] = vector;
		}
		if (inlineSpritePositions != null)
		{
			for (int j = 0; j < inlineSpritePositions.Count; j++)
			{
				inlineSpriteOffsetsForLastString[inlineSpritePositions[j]] += new Vector3(offsetX, 0f, 0f);
			}
		}
	}

	private static int GetFullHexColorComponent(int c1, int c2)
	{
		int num = 0;
		if (c1 >= 48 && c1 <= 57)
		{
			num += (c1 - 48) * 16;
		}
		else if (c1 >= 97 && c1 <= 102)
		{
			num += (10 + c1 - 97) * 16;
		}
		else
		{
			if (c1 < 65 || c1 > 70)
			{
				return -1;
			}
			num += (10 + c1 - 65) * 16;
		}
		if (c2 >= 48 && c2 <= 57)
		{
			return num + (c2 - 48);
		}
		if (c2 >= 97 && c2 <= 102)
		{
			return num + (10 + c2 - 97);
		}
		if (c2 >= 65 && c2 <= 70)
		{
			return num + (10 + c2 - 65);
		}
		return -1;
	}

	private static int GetCompactHexColorComponent(int c)
	{
		if (c >= 48 && c <= 57)
		{
			return (c - 48) * 17;
		}
		if (c >= 97 && c <= 102)
		{
			return (10 + c - 97) * 17;
		}
		if (c >= 65 && c <= 70)
		{
			return (10 + c - 65) * 17;
		}
		return -1;
	}

	private static int GetStyleHexColor(string str, bool fullHex, ref Color32 color)
	{
		int num;
		int num2;
		int num3;
		int num4;
		if (fullHex)
		{
			if (str.Length < 8)
			{
				return 1;
			}
			num = GetFullHexColorComponent(str[0], str[1]);
			num2 = GetFullHexColorComponent(str[2], str[3]);
			num3 = GetFullHexColorComponent(str[4], str[5]);
			num4 = GetFullHexColorComponent(str[6], str[7]);
		}
		else
		{
			if (str.Length < 4)
			{
				return 1;
			}
			num = GetCompactHexColorComponent(str[0]);
			num2 = GetCompactHexColorComponent(str[1]);
			num3 = GetCompactHexColorComponent(str[2]);
			num4 = GetCompactHexColorComponent(str[3]);
		}
		if (num == -1 || num2 == -1 || num3 == -1 || num4 == -1)
		{
			return 1;
		}
		color = new Color32((byte)num, (byte)num2, (byte)num3, (byte)num4);
		return 0;
	}

	private static int SetColorsFromStyleCommand(string args, bool twoColors, bool fullHex)
	{
		int num = ((!twoColors) ? 1 : 2) * ((!fullHex) ? 4 : 8);
		bool flag = false;
		if (args.Length >= num)
		{
			if (GetStyleHexColor(args, fullHex, ref meshTopColor) != 0)
			{
				flag = true;
			}
			if (twoColors)
			{
				string str = args.Substring((!fullHex) ? 4 : 8);
				if (GetStyleHexColor(str, fullHex, ref meshBottomColor) != 0)
				{
					flag = true;
				}
			}
			else
			{
				meshBottomColor = meshTopColor;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			meshTopColor = (meshBottomColor = errorColor);
		}
		return num;
	}

	private static void SetGradientTexUFromStyleCommand(int arg)
	{
		meshGradientTexU = (float)(arg - 48) / (float)((curGradientCount <= 0) ? 1 : curGradientCount);
	}

	private static int HandleStyleCommand(string cmd)
	{
		if (cmd.Length == 0)
		{
			return 0;
		}
		int num = cmd[0];
		string args = cmd.Substring(1);
		int result = 0;
		switch (num)
		{
		case 99:
			result = 1 + SetColorsFromStyleCommand(args, false, false);
			break;
		case 67:
			result = 1 + SetColorsFromStyleCommand(args, false, true);
			break;
		case 103:
			result = 1 + SetColorsFromStyleCommand(args, true, false);
			break;
		case 71:
			result = 1 + SetColorsFromStyleCommand(args, true, true);
			break;
		}
		if (num >= 48 && num <= 57)
		{
			SetGradientTexUFromStyleCommand(num);
			result = 1;
		}
		return result;
	}

	public static void GetTextMeshGeomDesc(out int numVertices, out int numIndices, GeomData geomData)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		numVertices = textMeshData.maxChars * 4;
		numIndices = textMeshData.maxChars * 6;
	}

	public static int SetTextMeshGeom(Vector3[] pos, Vector2[] uv, Vector2[] uv2, Color32[] color, int offset, GeomData geomData, int visibleCharacters, Vector2[] characterOffsetVectors, bool[] rainbowValues)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		string formattedText = geomData.formattedText;
		inlineSpriteOffsetsForLastString = new List<Vector3>();
		meshTopColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		meshBottomColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		meshGradientTexU = (float)textMeshData.textureGradient / (float)((fontInst.gradientCount <= 0) ? 1 : fontInst.gradientCount);
		curGradientCount = fontInst.gradientCount;
		float yAnchorForHeight = GetYAnchorForHeight(GetMeshDimensionsForString(geomData.formattedText, geomData).y, geomData);
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		List<int> list = new List<int>();
		bool flag = false;
		for (int i = 0; i < rainbowValues.Length; i++)
		{
			flag = flag || rainbowValues[i];
		}
		for (int j = 0; j < formattedText.Length && num3 < textMeshData.maxChars; j++)
		{
			int num6 = formattedText[j];
			tk2dFontChar tk2dFontChar2 = null;
			bool flag2 = false;
			if (num6 == 91 && j < formattedText.Length - 1 && formattedText[j + 1] != ']')
			{
				for (int k = j; k < formattedText.Length; k++)
				{
					char c = formattedText[k];
					if (c == ']')
					{
						flag2 = true;
						int num7 = k - j;
						string substringInsideBrackets = formattedText.Substring(j + 9, num7 - 10);
						tk2dFontChar2 = GetSpecificSpriteCharDef(substringInsideBrackets);
						j += num7;
						num5 += num7;
						break;
					}
				}
			}
			bool flag3 = num6 == 94;
			if (!flag2)
			{
				if (fontInst.useDictionary)
				{
					if (!fontInst.charDict.ContainsKey(num6))
					{
						num6 = 0;
					}
					tk2dFontChar2 = fontInst.charDict[num6];
				}
				else
				{
					if (num6 >= fontInst.chars.Length)
					{
						num6 = 0;
					}
					tk2dFontChar2 = fontInst.chars[num6];
				}
			}
			if (flag3)
			{
				num6 = 94;
			}
			if (num6 == 10)
			{
				float lineWidth = num;
				int targetEnd = num3;
				if (num4 != num3)
				{
					float xAnchorForWidth = GetXAnchorForWidth(lineWidth, geomData);
					PostAlignTextData(pos, offset, num4, targetEnd, xAnchorForWidth, list);
				}
				num4 = num3;
				num = 0f;
				num2 -= (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
				list.Clear();
				continue;
			}
			if (textMeshData.inlineStyling && num6 == 94)
			{
				if (j + 1 >= formattedText.Length || formattedText[j + 1] != '^')
				{
					j += HandleStyleCommand(formattedText.Substring(j + 1));
					continue;
				}
				j++;
			}
			Vector2 vector = characterOffsetVectors[j];
			vector = BraveUtility.QuantizeVector(vector.ToVector3ZUp(), 32f).XY();
			pos[offset + num3 * 4] = new Vector3(num + tk2dFontChar2.p0.x * textMeshData.scale.x + vector.x, yAnchorForHeight + num2 + tk2dFontChar2.p0.y * textMeshData.scale.y + vector.y, 0f);
			pos[offset + num3 * 4 + 1] = new Vector3(num + tk2dFontChar2.p1.x * textMeshData.scale.x + vector.x, yAnchorForHeight + num2 + tk2dFontChar2.p0.y * textMeshData.scale.y + vector.y, 0f);
			pos[offset + num3 * 4 + 2] = new Vector3(num + tk2dFontChar2.p0.x * textMeshData.scale.x + vector.x, yAnchorForHeight + num2 + tk2dFontChar2.p1.y * textMeshData.scale.y + vector.y, 0f);
			pos[offset + num3 * 4 + 3] = new Vector3(num + tk2dFontChar2.p1.x * textMeshData.scale.x + vector.x, yAnchorForHeight + num2 + tk2dFontChar2.p1.y * textMeshData.scale.y + vector.y, 0f);
			if (flag2)
			{
				inlineSpriteOffsetsForLastString.Add(pos[offset + num3 * 4 + 2]);
				list.Add(inlineSpriteOffsetsForLastString.Count - 1);
			}
			if (tk2dFontChar2.flipped)
			{
				uv[offset + num3 * 4] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv1.y);
				uv[offset + num3 * 4 + 1] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv0.y);
				uv[offset + num3 * 4 + 2] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv1.y);
				uv[offset + num3 * 4 + 3] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv0.y);
			}
			else
			{
				uv[offset + num3 * 4] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv0.y);
				uv[offset + num3 * 4 + 1] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv0.y);
				uv[offset + num3 * 4 + 2] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv1.y);
				uv[offset + num3 * 4 + 3] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv1.y);
			}
			if (fontInst.textureGradients)
			{
				uv2[offset + num3 * 4] = tk2dFontChar2.gradientUv[0] + new Vector2(meshGradientTexU, 0f);
				uv2[offset + num3 * 4 + 1] = tk2dFontChar2.gradientUv[1] + new Vector2(meshGradientTexU, 0f);
				uv2[offset + num3 * 4 + 2] = tk2dFontChar2.gradientUv[2] + new Vector2(meshGradientTexU, 0f);
				uv2[offset + num3 * 4 + 3] = tk2dFontChar2.gradientUv[3] + new Vector2(meshGradientTexU, 0f);
			}
			if (j - num5 > visibleCharacters)
			{
				Color32 color2 = Color.clear;
				color[offset + num3 * 4] = color2;
				color[offset + num3 * 4 + 1] = color2;
				color[offset + num3 * 4 + 2] = color2;
				color[offset + num3 * 4 + 3] = color2;
			}
			else if (fontInst.isPacked)
			{
				Color32 color3 = channelSelectColors[tk2dFontChar2.channel];
				color[offset + num3 * 4] = color3;
				color[offset + num3 * 4 + 1] = color3;
				color[offset + num3 * 4 + 2] = color3;
				color[offset + num3 * 4 + 3] = color3;
			}
			else if (rainbowValues[j])
			{
				color[offset + num3 * 4] = BraveUtility.GetRainbowLerp(Time.time * 3f);
				color[offset + num3 * 4 + 1] = BraveUtility.GetRainbowLerp(Time.time * 3f);
				color[offset + num3 * 4 + 2] = BraveUtility.GetRainbowLerp(Time.time * 3f);
				color[offset + num3 * 4 + 3] = BraveUtility.GetRainbowLerp(Time.time * 3f);
			}
			else if (flag)
			{
				color[offset + num3 * 4] = Color.black;
				color[offset + num3 * 4 + 1] = Color.black;
				color[offset + num3 * 4 + 2] = Color.black;
				color[offset + num3 * 4 + 3] = Color.black;
			}
			else
			{
				color[offset + num3 * 4] = meshTopColor;
				color[offset + num3 * 4 + 1] = meshTopColor;
				color[offset + num3 * 4 + 2] = meshBottomColor;
				color[offset + num3 * 4 + 3] = meshBottomColor;
			}
			num += (tk2dFontChar2.advance + textMeshData.spacing) * textMeshData.scale.x;
			if (textMeshData.kerning && j < formattedText.Length - 1)
			{
				tk2dFontKerning[] kerning = fontInst.kerning;
				foreach (tk2dFontKerning tk2dFontKerning2 in kerning)
				{
					if (tk2dFontKerning2.c0 == formattedText[j] && tk2dFontKerning2.c1 == formattedText[j + 1])
					{
						num += tk2dFontKerning2.amount * textMeshData.scale.x;
						break;
					}
				}
			}
			num3++;
		}
		if (num4 != num3)
		{
			float lineWidth2 = num;
			int targetEnd2 = num3;
			float xAnchorForWidth2 = GetXAnchorForWidth(lineWidth2, geomData);
			PostAlignTextData(pos, offset, num4, targetEnd2, xAnchorForWidth2, list);
		}
		for (int m = num3; m < textMeshData.maxChars; m++)
		{
			pos[offset + m * 4] = (pos[offset + m * 4 + 1] = (pos[offset + m * 4 + 2] = (pos[offset + m * 4 + 3] = Vector3.zero)));
			uv[offset + m * 4] = (uv[offset + m * 4 + 1] = (uv[offset + m * 4 + 2] = (uv[offset + m * 4 + 3] = Vector2.zero)));
			if (fontInst.textureGradients)
			{
				uv2[offset + m * 4] = (uv2[offset + m * 4 + 1] = (uv2[offset + m * 4 + 2] = (uv2[offset + m * 4 + 3] = Vector2.zero)));
			}
			if (!fontInst.isPacked)
			{
				color[offset + m * 4] = (color[offset + m * 4 + 1] = meshTopColor);
				color[offset + m * 4 + 2] = (color[offset + m * 4 + 3] = meshBottomColor);
			}
			else
			{
				color[offset + m * 4] = (color[offset + m * 4 + 1] = (color[offset + m * 4 + 2] = (color[offset + m * 4 + 3] = Color.clear)));
			}
		}
		return num3;
	}

	public static tk2dFontChar GetSpecificSpriteCharDef(string substringInsideBrackets)
	{
		tk2dBaseSprite component = ((GameObject)ResourceCache.Acquire("ControllerButtonSprite")).GetComponent<tk2dBaseSprite>();
		tk2dSpriteDefinition spriteDefinition = component.Collection.GetSpriteDefinition(substringInsideBrackets);
		if (spriteDefinition != null)
		{
			tk2dFontChar tk2dFontChar2 = new tk2dFontChar();
			float num = (tk2dFontChar2.advance = Mathf.Abs(spriteDefinition.position1.x - spriteDefinition.position0.x));
			tk2dFontChar2.channel = 0;
			tk2dFontChar2.p0 = new Vector3(0f, 0.6875f, 0f);
			tk2dFontChar2.p1 = new Vector3(0.6875f, 0f, 0f);
			tk2dFontChar2.uv0 = Vector3.zero;
			tk2dFontChar2.uv1 = Vector3.zero;
			tk2dFontChar2.flipped = false;
			return tk2dFontChar2;
		}
		return GetGenericSpriteCharDef();
	}

	public static tk2dFontChar GetGenericSpriteCharDef()
	{
		tk2dFontChar tk2dFontChar2 = new tk2dFontChar();
		tk2dFontChar2.advance = 0.8125f;
		tk2dFontChar2.channel = 0;
		tk2dFontChar2.p0 = new Vector3(0f, 0.6875f, 0f);
		tk2dFontChar2.p1 = new Vector3(0.6875f, 0f, 0f);
		tk2dFontChar2.uv0 = Vector3.zero;
		tk2dFontChar2.uv1 = Vector3.zero;
		tk2dFontChar2.flipped = false;
		return tk2dFontChar2;
	}

	public static int SetTextMeshGeom(Vector3[] pos, Vector2[] uv, Vector2[] uv2, Color32[] color, int offset, GeomData geomData, int visibleCharacters)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		string formattedText = geomData.formattedText;
		inlineSpriteOffsetsForLastString = new List<Vector3>();
		meshTopColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		meshBottomColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		meshGradientTexU = (float)textMeshData.textureGradient / (float)((fontInst.gradientCount <= 0) ? 1 : fontInst.gradientCount);
		curGradientCount = fontInst.gradientCount;
		float yAnchorForHeight = GetYAnchorForHeight(GetMeshDimensionsForString(geomData.formattedText, geomData).y, geomData);
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < formattedText.Length && num3 < textMeshData.maxChars; i++)
		{
			int num6 = formattedText[i];
			tk2dFontChar tk2dFontChar2 = null;
			bool flag = false;
			if (num6 == 91 && i < formattedText.Length - 1 && formattedText[i + 1] != ']')
			{
				for (int j = i; j < formattedText.Length; j++)
				{
					char c = formattedText[j];
					if (c == ']')
					{
						flag = true;
						int num7 = j - i;
						string substringInsideBrackets = formattedText.Substring(i + 9, num7 - 10);
						tk2dFontChar2 = GetSpecificSpriteCharDef(substringInsideBrackets);
						i += num7;
						num5 += num7;
						break;
					}
				}
			}
			bool flag2 = num6 == 94;
			if (!flag)
			{
				if (fontInst.useDictionary)
				{
					if (!fontInst.charDict.ContainsKey(num6))
					{
						num6 = 0;
					}
					tk2dFontChar2 = fontInst.charDict[num6];
				}
				else
				{
					if (num6 >= fontInst.chars.Length)
					{
						num6 = 0;
					}
					tk2dFontChar2 = fontInst.chars[num6];
				}
			}
			if (flag2)
			{
				num6 = 94;
			}
			if (num6 == 10)
			{
				float lineWidth = num;
				int targetEnd = num3;
				if (num4 != num3)
				{
					float xAnchorForWidth = GetXAnchorForWidth(lineWidth, geomData);
					PostAlignTextData(pos, offset, num4, targetEnd, xAnchorForWidth, list);
				}
				num4 = num3;
				num = 0f;
				num2 -= (fontInst.lineHeight + textMeshData.lineSpacing) * textMeshData.scale.y;
				list.Clear();
				continue;
			}
			if (textMeshData.inlineStyling && num6 == 94)
			{
				if (i + 1 >= formattedText.Length || formattedText[i + 1] != '^')
				{
					i += HandleStyleCommand(formattedText.Substring(i + 1));
					continue;
				}
				i++;
			}
			pos[offset + num3 * 4] = new Vector3(num + tk2dFontChar2.p0.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p0.y * textMeshData.scale.y, 0f);
			pos[offset + num3 * 4 + 1] = new Vector3(num + tk2dFontChar2.p1.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p0.y * textMeshData.scale.y, 0f);
			pos[offset + num3 * 4 + 2] = new Vector3(num + tk2dFontChar2.p0.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p1.y * textMeshData.scale.y, 0f);
			pos[offset + num3 * 4 + 3] = new Vector3(num + tk2dFontChar2.p1.x * textMeshData.scale.x, yAnchorForHeight + num2 + tk2dFontChar2.p1.y * textMeshData.scale.y, 0f);
			if (flag)
			{
				inlineSpriteOffsetsForLastString.Add(pos[offset + num3 * 4 + 2]);
				list.Add(inlineSpriteOffsetsForLastString.Count - 1);
			}
			if (tk2dFontChar2.flipped)
			{
				uv[offset + num3 * 4] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv1.y);
				uv[offset + num3 * 4 + 1] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv0.y);
				uv[offset + num3 * 4 + 2] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv1.y);
				uv[offset + num3 * 4 + 3] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv0.y);
			}
			else
			{
				uv[offset + num3 * 4] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv0.y);
				uv[offset + num3 * 4 + 1] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv0.y);
				uv[offset + num3 * 4 + 2] = new Vector2(tk2dFontChar2.uv0.x, tk2dFontChar2.uv1.y);
				uv[offset + num3 * 4 + 3] = new Vector2(tk2dFontChar2.uv1.x, tk2dFontChar2.uv1.y);
			}
			if (fontInst.textureGradients)
			{
				uv2[offset + num3 * 4] = tk2dFontChar2.gradientUv[0] + new Vector2(meshGradientTexU, 0f);
				uv2[offset + num3 * 4 + 1] = tk2dFontChar2.gradientUv[1] + new Vector2(meshGradientTexU, 0f);
				uv2[offset + num3 * 4 + 2] = tk2dFontChar2.gradientUv[2] + new Vector2(meshGradientTexU, 0f);
				uv2[offset + num3 * 4 + 3] = tk2dFontChar2.gradientUv[3] + new Vector2(meshGradientTexU, 0f);
			}
			if (i - num5 > visibleCharacters)
			{
				Color32 color2 = Color.clear;
				color[offset + num3 * 4] = color2;
				color[offset + num3 * 4 + 1] = color2;
				color[offset + num3 * 4 + 2] = color2;
				color[offset + num3 * 4 + 3] = color2;
			}
			else if (fontInst.isPacked)
			{
				Color32 color3 = channelSelectColors[tk2dFontChar2.channel];
				color[offset + num3 * 4] = color3;
				color[offset + num3 * 4 + 1] = color3;
				color[offset + num3 * 4 + 2] = color3;
				color[offset + num3 * 4 + 3] = color3;
			}
			else
			{
				color[offset + num3 * 4] = meshTopColor;
				color[offset + num3 * 4 + 1] = meshTopColor;
				color[offset + num3 * 4 + 2] = meshBottomColor;
				color[offset + num3 * 4 + 3] = meshBottomColor;
			}
			num += (tk2dFontChar2.advance + textMeshData.spacing) * textMeshData.scale.x;
			if (textMeshData.kerning && i < formattedText.Length - 1)
			{
				tk2dFontKerning[] kerning = fontInst.kerning;
				foreach (tk2dFontKerning tk2dFontKerning2 in kerning)
				{
					if (tk2dFontKerning2.c0 == formattedText[i] && tk2dFontKerning2.c1 == formattedText[i + 1])
					{
						num += tk2dFontKerning2.amount * textMeshData.scale.x;
						break;
					}
				}
			}
			num3++;
		}
		if (num4 != num3)
		{
			float lineWidth2 = num;
			int targetEnd2 = num3;
			float xAnchorForWidth2 = GetXAnchorForWidth(lineWidth2, geomData);
			PostAlignTextData(pos, offset, num4, targetEnd2, xAnchorForWidth2, list);
		}
		for (int l = num3; l < textMeshData.maxChars; l++)
		{
			pos[offset + l * 4] = (pos[offset + l * 4 + 1] = (pos[offset + l * 4 + 2] = (pos[offset + l * 4 + 3] = Vector3.zero)));
			uv[offset + l * 4] = (uv[offset + l * 4 + 1] = (uv[offset + l * 4 + 2] = (uv[offset + l * 4 + 3] = Vector2.zero)));
			if (fontInst.textureGradients)
			{
				uv2[offset + l * 4] = (uv2[offset + l * 4 + 1] = (uv2[offset + l * 4 + 2] = (uv2[offset + l * 4 + 3] = Vector2.zero)));
			}
			if (!fontInst.isPacked)
			{
				color[offset + l * 4] = (color[offset + l * 4 + 1] = meshTopColor);
				color[offset + l * 4 + 2] = (color[offset + l * 4 + 3] = meshBottomColor);
			}
			else
			{
				color[offset + l * 4] = (color[offset + l * 4 + 1] = (color[offset + l * 4 + 2] = (color[offset + l * 4 + 3] = Color.clear)));
			}
		}
		return num3;
	}

	public static void SetTextMeshIndices(int[] indices, int offset, int vStart, GeomData geomData, int target)
	{
		tk2dTextMeshData textMeshData = geomData.textMeshData;
		for (int i = 0; i < textMeshData.maxChars; i++)
		{
			indices[offset + i * 6] = vStart + i * 4;
			indices[offset + i * 6 + 1] = vStart + i * 4 + 1;
			indices[offset + i * 6 + 2] = vStart + i * 4 + 3;
			indices[offset + i * 6 + 3] = vStart + i * 4 + 2;
			indices[offset + i * 6 + 4] = vStart + i * 4;
			indices[offset + i * 6 + 5] = vStart + i * 4 + 3;
		}
	}
}
