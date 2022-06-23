using System.Text.RegularExpressions;

public struct dfMarkupBorders
{
	public int left;

	public int top;

	public int right;

	public int bottom;

	public int horizontal
	{
		get
		{
			return left + right;
		}
	}

	public int vertical
	{
		get
		{
			return top + bottom;
		}
	}

	public dfMarkupBorders(int left, int right, int top, int bottom)
	{
		this.left = left;
		this.top = top;
		this.right = right;
		this.bottom = bottom;
	}

	public static dfMarkupBorders Parse(string value)
	{
		dfMarkupBorders result = default(dfMarkupBorders);
		value = Regex.Replace(value, "\\s+", " ");
		string[] array = value.Split(' ');
		if (array.Length == 1)
		{
			int num = dfMarkupStyle.ParseSize(value, 0);
			result.left = (result.right = num);
			result.top = (result.bottom = num);
		}
		else if (array.Length == 2)
		{
			int num2 = dfMarkupStyle.ParseSize(array[0], 0);
			result.top = (result.bottom = num2);
			int num3 = dfMarkupStyle.ParseSize(array[1], 0);
			result.left = (result.right = num3);
		}
		else if (array.Length == 3)
		{
			int num4 = (result.top = dfMarkupStyle.ParseSize(array[0], 0));
			int num5 = dfMarkupStyle.ParseSize(array[1], 0);
			result.left = (result.right = num5);
			int num6 = (result.bottom = dfMarkupStyle.ParseSize(array[2], 0));
		}
		else if (array.Length == 4)
		{
			int num7 = (result.top = dfMarkupStyle.ParseSize(array[0], 0));
			int num8 = (result.right = dfMarkupStyle.ParseSize(array[1], 0));
			int num9 = (result.bottom = dfMarkupStyle.ParseSize(array[2], 0));
			int num10 = (result.left = dfMarkupStyle.ParseSize(array[3], 0));
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("[T:{0},R:{1},L:{2},B:{3}]", top, right, left, bottom);
	}
}
