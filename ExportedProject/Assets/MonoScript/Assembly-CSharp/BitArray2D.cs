using System;

public class BitArray2D
{
	private int m_width;

	private int m_height;

	private bool[] m_bits;

	private float[] m_floats;

	private float c_sizeScalar = 2f;

	public int Width
	{
		get
		{
			return m_width;
		}
	}

	public int Height
	{
		get
		{
			return m_height;
		}
	}

	public bool IsEmpty
	{
		get
		{
			return m_width == 0 && m_height == 0;
		}
	}

	public bool IsValid { get; set; }

	public bool IsAabb { get; set; }

	public bool UsesBackingFloats { get; set; }

	public bool ReadOnly { get; set; }

	public bool this[int x, int y]
	{
		get
		{
			return IsAabb || m_bits[x + y * m_width];
		}
		set
		{
			m_bits[x + y * m_width] = value;
		}
	}

	public BitArray2D(bool useBackingFloats = false)
	{
		UsesBackingFloats = useBackingFloats;
	}

	public void Reinitialize(int width, int height, bool fixedSize = false)
	{
		m_width = width;
		m_height = height;
		int num = m_width * m_height;
		if (m_bits == null || num > m_bits.Length)
		{
			if (!fixedSize)
			{
				num = (int)((float)num * c_sizeScalar);
			}
			m_bits = new bool[num];
		}
		if (UsesBackingFloats && (m_floats == null || num > m_floats.Length))
		{
			m_floats = new float[num];
		}
		IsValid = true;
	}

	public void ReinitializeWithDefault(int width, int height, bool defaultValue, float defaultFloatValue = 0f, bool fixedSize = false)
	{
		m_width = width;
		m_height = height;
		int num = m_width * m_height;
		if (!defaultValue && (m_bits == null || num > m_bits.Length))
		{
			if (!fixedSize)
			{
				num = (int)((float)num * c_sizeScalar);
			}
			m_bits = new bool[num];
		}
		if (UsesBackingFloats && (m_floats == null || num > m_floats.Length))
		{
			m_floats = new float[num];
		}
		int num2 = m_width * m_height;
		if (!defaultValue)
		{
			Array.Clear(m_bits, 0, num2);
		}
		else
		{
			IsAabb = true;
		}
		if (UsesBackingFloats)
		{
			for (int i = 0; i < num2; i++)
			{
				m_floats[i] = defaultFloatValue;
			}
		}
		IsValid = true;
	}

	public float GetFloat(int x, int y)
	{
		return m_floats[x + y * m_width];
	}

	public void SetFloat(int x, int y, float value)
	{
		m_floats[x + y * m_width] = value;
	}

	public void SetCircle(int x0, int y0, int radius, bool value, SetBackingFloatFunc floatFunc = null)
	{
		int num = radius;
		int num2 = 0;
		int num3 = 1 - num;
		while (num2 <= num)
		{
			SetColumn(num + x0, num2 + y0, -num2 + y0, value, floatFunc);
			SetColumn(num2 + x0, num + y0, -num + y0, value, floatFunc);
			SetColumn(-num2 + x0, num + y0, -num + y0, value, floatFunc);
			SetColumn(-num + x0, num2 + y0, -num2 + y0, value, floatFunc);
			num2++;
			if (num3 <= 0)
			{
				num3 += 2 * num2 + 1;
				continue;
			}
			num--;
			num3 += 2 * (num2 - num) + 1;
		}
	}

	public void SetColumn(int x, int y0, int y1, bool value, SetBackingFloatFunc floatFunc = null)
	{
		if (y0 > y1)
		{
			BraveUtility.Swap(ref y0, ref y1);
		}
		for (int i = y0; i <= y1; i++)
		{
			SetSafe(x, i, value, floatFunc);
		}
	}

	public void SetSafe(int x, int y, bool value, SetBackingFloatFunc floatFunc = null)
	{
		if (x >= 0 && x < m_width && y >= 0 && y < m_height)
		{
			int num = x + y * m_width;
			m_bits[num] = value;
			if (floatFunc != null)
			{
				m_floats[num] = floatFunc(x, y, value, m_floats[num]);
			}
		}
	}
}
