using System;
using UnityEngine;

[Serializable]
public struct HSBColor
{
	public float h;

	public float s;

	public float b;

	public float a;

	public HSBColor(float h, float s, float b, float a)
	{
		this.h = h;
		this.s = s;
		this.b = b;
		this.a = a;
	}

	public HSBColor(float h, float s, float b)
	{
		this.h = h;
		this.s = s;
		this.b = b;
		a = 1f;
	}

	public HSBColor(Color col)
	{
		HSBColor hSBColor = FromColor(col);
		h = hSBColor.h;
		s = hSBColor.s;
		b = hSBColor.b;
		a = hSBColor.a;
	}

	public static Color GetHue(Color color)
	{
		HSBColor hSBColor = FromColor(color);
		hSBColor.s = (hSBColor.b = 1f);
		return hSBColor;
	}

	public static HSBColor FromColor(Color color)
	{
		HSBColor result = new HSBColor(0f, 0f, 0f, color.a);
		RGBToHSV(color, out result.h, out result.s, out result.b);
		return result;
	}

	public static Color ToColor(HSBColor hsbColor)
	{
		float value = hsbColor.b;
		float value2 = hsbColor.b;
		float value3 = hsbColor.b;
		if (hsbColor.s != 0f)
		{
			float num = hsbColor.b;
			float num2 = hsbColor.b * hsbColor.s;
			float num3 = hsbColor.b - num2;
			float num4 = hsbColor.h * 360f;
			if (num4 < 60f)
			{
				value = num;
				value2 = num4 * num2 / 60f + num3;
				value3 = num3;
			}
			else if (num4 < 120f)
			{
				value = (0f - (num4 - 120f)) * num2 / 60f + num3;
				value2 = num;
				value3 = num3;
			}
			else if (num4 < 180f)
			{
				value = num3;
				value2 = num;
				value3 = (num4 - 120f) * num2 / 60f + num3;
			}
			else if (num4 < 240f)
			{
				value = num3;
				value2 = (0f - (num4 - 240f)) * num2 / 60f + num3;
				value3 = num;
			}
			else if (num4 < 300f)
			{
				value = (num4 - 240f) * num2 / 60f + num3;
				value2 = num3;
				value3 = num;
			}
			else if (num4 <= 360f)
			{
				value = num;
				value2 = num3;
				value3 = (0f - (num4 - 360f)) * num2 / 60f + num3;
			}
			else
			{
				value = 0f;
				value2 = 0f;
				value3 = 0f;
			}
		}
		return new Color(Mathf.Clamp01(value), Mathf.Clamp01(value2), Mathf.Clamp01(value3), hsbColor.a);
	}

	public Color ToColor()
	{
		return ToColor(this);
	}

	public static HSBColor Lerp(HSBColor a, HSBColor b, float t)
	{
		float num;
		float num2;
		if (a.b == 0f)
		{
			num = b.h;
			num2 = b.s;
		}
		else if (b.b == 0f)
		{
			num = a.h;
			num2 = a.s;
		}
		else
		{
			if (a.s == 0f)
			{
				num = b.h;
			}
			else if (b.s == 0f)
			{
				num = a.h;
			}
			else
			{
				float num3;
				for (num3 = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t); num3 < 0f; num3 += 360f)
				{
				}
				while (num3 > 360f)
				{
					num3 -= 360f;
				}
				num = num3 / 360f;
			}
			num2 = Mathf.Lerp(a.s, b.s, t);
		}
		return new HSBColor(num, num2, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
	}

	private static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
	{
		if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
		{
			RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
		}
		else if (rgbColor.g > rgbColor.r)
		{
			RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
		}
		else
		{
			RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
		}
	}

	private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
	{
		V = dominantcolor;
		if (V != 0f)
		{
			float num = ((!(colorone > colortwo)) ? colorone : colortwo);
			float num2 = V - num;
			if (num2 != 0f)
			{
				S = num2 / V;
				H = offset + (colorone - colortwo) / num2;
			}
			else
			{
				S = 0f;
				H = offset + (colorone - colortwo);
			}
			H /= 6f;
			if (H < 0f)
			{
				H += 1f;
			}
		}
		else
		{
			S = 0f;
			H = 0f;
		}
	}

	public static implicit operator HSBColor(Color color)
	{
		return FromColor(color);
	}

	public static implicit operator HSBColor(Color32 color)
	{
		return FromColor(color);
	}

	public static implicit operator Color(HSBColor hsb)
	{
		return hsb.ToColor();
	}

	public static implicit operator Color32(HSBColor hsb)
	{
		return hsb.ToColor();
	}

	public override string ToString()
	{
		return string.Format("H:{0}, S:{1}, B:{2}", h, s, b);
	}
}
