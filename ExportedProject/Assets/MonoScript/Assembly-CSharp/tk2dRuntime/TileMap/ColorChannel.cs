using System;
using UnityEngine;

namespace tk2dRuntime.TileMap
{
	[Serializable]
	public class ColorChannel
	{
		public Color clearColor = Color.white;

		public ColorChunk[] chunks;

		public int numColumns;

		public int numRows;

		public int divX;

		public int divY;

		public bool IsEmpty
		{
			get
			{
				return chunks.Length == 0;
			}
		}

		public int NumActiveChunks
		{
			get
			{
				int num = 0;
				ColorChunk[] array = chunks;
				foreach (ColorChunk colorChunk in array)
				{
					if (colorChunk != null && colorChunk.colors != null && colorChunk.colors.Length > 0)
					{
						num++;
					}
				}
				return num;
			}
		}

		public ColorChannel(int width, int height, int divX, int divY)
		{
			Init(width, height, divX, divY);
		}

		public ColorChannel()
		{
			chunks = new ColorChunk[0];
		}

		public void Init(int width, int height, int divX, int divY)
		{
			numColumns = (width + divX - 1) / divX;
			numRows = (height + divY - 1) / divY;
			chunks = new ColorChunk[0];
			this.divX = divX;
			this.divY = divY;
		}

		public ColorChunk FindChunkAndCoordinate(int x, int y, out int offset)
		{
			int value = x / divX;
			int value2 = y / divY;
			value = Mathf.Clamp(value, 0, numColumns - 1);
			value2 = Mathf.Clamp(value2, 0, numRows - 1);
			int num = value2 * numColumns + value;
			ColorChunk result = chunks[num];
			int num2 = x - value * divX;
			int num3 = y - value2 * divY;
			offset = num3 * (divX + 1) + num2;
			return result;
		}

		public Color GetColor(int x, int y)
		{
			if (IsEmpty)
			{
				return clearColor;
			}
			int offset;
			ColorChunk colorChunk = FindChunkAndCoordinate(x, y, out offset);
			if (colorChunk.colors.Length == 0)
			{
				return clearColor;
			}
			return colorChunk.colors[offset];
		}

		private void InitChunk(ColorChunk chunk)
		{
			if (chunk.colors.Length == 0)
			{
				chunk.colors = new Color32[(divX + 1) * (divY + 1)];
				for (int i = 0; i < chunk.colors.Length; i++)
				{
					chunk.colors[i] = clearColor;
				}
				chunk.colorOverrides = new Color32[(divX + 1) * (divY + 1), 4];
				for (int j = 0; j < chunk.colorOverrides.GetLength(0); j++)
				{
					chunk.colorOverrides[j, 0] = clearColor;
					chunk.colorOverrides[j, 1] = clearColor;
					chunk.colorOverrides[j, 2] = clearColor;
					chunk.colorOverrides[j, 3] = clearColor;
				}
			}
		}

		public void SetTileColorOverride(int x, int y, Color32 color)
		{
			if (IsEmpty)
			{
				Create();
			}
			int num = x / divX;
			int num2 = y / divY;
			ColorChunk chunk = GetChunk(num, num2, true);
			int num3 = x - num * divX;
			int num4 = y - num2 * divY;
			int num5 = divX + 1;
			chunk.colorOverrides[num4 * num5 + num3, 0] = color;
			chunk.colorOverrides[num4 * num5 + num3, 1] = color;
			chunk.colorOverrides[num4 * num5 + num3, 2] = color;
			chunk.colorOverrides[num4 * num5 + num3, 3] = color;
			chunk.Dirty = true;
		}

		public void SetTileColorGradient(int x, int y, Color32 bottomLeft, Color32 bottomRight, Color32 topLeft, Color32 topRight)
		{
			if (IsEmpty)
			{
				Create();
			}
			int num = divX + 1;
			int num2 = x / divX;
			int num3 = y / divY;
			ColorChunk chunk = GetChunk(num2, num3, true);
			int num4 = x - num2 * divX;
			int num5 = y - num3 * divY;
			chunk.colorOverrides[num5 * num + num4, 0] = bottomLeft;
			chunk.colorOverrides[num5 * num + num4, 1] = bottomRight;
			chunk.colorOverrides[num5 * num + num4, 2] = topLeft;
			chunk.colorOverrides[num5 * num + num4, 3] = topRight;
			chunk.Dirty = true;
		}

		public void SetColor(int x, int y, Color color)
		{
			if (IsEmpty)
			{
				Create();
			}
			int num = divX + 1;
			int num2 = Mathf.Max(x - 1, 0) / divX;
			int num3 = Mathf.Max(y - 1, 0) / divY;
			ColorChunk chunk = GetChunk(num2, num3, true);
			int num4 = x - num2 * divX;
			int num5 = y - num3 * divY;
			chunk.colors[num5 * num + num4] = color;
			chunk.Dirty = true;
			bool flag = false;
			bool flag2 = false;
			if (x != 0 && x % divX == 0 && num2 + 1 < numColumns)
			{
				flag = true;
			}
			if (y != 0 && y % divY == 0 && num3 + 1 < numRows)
			{
				flag2 = true;
			}
			if (flag)
			{
				int num6 = num2 + 1;
				chunk = GetChunk(num6, num3, true);
				num4 = x - num6 * divX;
				num5 = y - num3 * divY;
				chunk.colors[num5 * num + num4] = color;
				chunk.Dirty = true;
			}
			if (flag2)
			{
				int num7 = num3 + 1;
				chunk = GetChunk(num2, num7, true);
				num4 = x - num2 * divX;
				num5 = y - num7 * divY;
				chunk.colors[num5 * num + num4] = color;
				chunk.Dirty = true;
			}
			if (flag && flag2)
			{
				int num8 = num2 + 1;
				int num9 = num3 + 1;
				chunk = GetChunk(num8, num9, true);
				num4 = x - num8 * divX;
				num5 = y - num9 * divY;
				chunk.colors[num5 * num + num4] = color;
				chunk.Dirty = true;
			}
		}

		public ColorChunk GetChunk(int x, int y)
		{
			if (chunks == null || chunks.Length == 0)
			{
				return null;
			}
			return chunks[y * numColumns + x];
		}

		public ColorChunk GetChunk(int x, int y, bool init)
		{
			if (chunks == null || chunks.Length == 0)
			{
				return null;
			}
			ColorChunk colorChunk = chunks[y * numColumns + x];
			InitChunk(colorChunk);
			return colorChunk;
		}

		public void ClearChunk(ColorChunk chunk)
		{
			for (int i = 0; i < chunk.colors.Length; i++)
			{
				chunk.colors[i] = clearColor;
			}
			for (int j = 0; j < chunk.colorOverrides.GetLength(0); j++)
			{
				chunk.colorOverrides[j, 0] = clearColor;
				chunk.colorOverrides[j, 1] = clearColor;
				chunk.colorOverrides[j, 2] = clearColor;
				chunk.colorOverrides[j, 3] = clearColor;
			}
		}

		public void ClearDirtyFlag()
		{
			ColorChunk[] array = chunks;
			foreach (ColorChunk colorChunk in array)
			{
				colorChunk.Dirty = false;
			}
		}

		public void Clear(Color color)
		{
			clearColor = color;
			ColorChunk[] array = chunks;
			foreach (ColorChunk chunk in array)
			{
				ClearChunk(chunk);
			}
			Optimize();
		}

		public void Delete()
		{
			chunks = new ColorChunk[0];
		}

		public void Create()
		{
			chunks = new ColorChunk[numColumns * numRows];
			for (int i = 0; i < chunks.Length; i++)
			{
				chunks[i] = new ColorChunk();
			}
		}

		private void Optimize(ColorChunk chunk)
		{
			bool flag = true;
			Color32 color = clearColor;
			Color32[] colors = chunk.colors;
			for (int i = 0; i < colors.Length; i++)
			{
				Color32 color2 = colors[i];
				if (color2.r != color.r || color2.g != color.g || color2.b != color.b || color2.a != color.a)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				Color32[,] colorOverrides = chunk.colorOverrides;
				int length = colorOverrides.GetLength(0);
				int length2 = colorOverrides.GetLength(1);
				for (int j = 0; j < length; j++)
				{
					int num = 0;
					while (num < length2)
					{
						Color32 color3 = colorOverrides[j, num];
						if (color3.r == color.r && color3.g == color.g && color3.b == color.b && color3.a == color.a)
						{
							num++;
							continue;
						}
						goto IL_0119;
					}
					continue;
					IL_0119:
					flag = false;
					break;
				}
			}
			if (flag)
			{
				chunk.colors = new Color32[0];
				chunk.colorOverrides = new Color32[0, 0];
			}
		}

		public void Optimize()
		{
			ColorChunk[] array = chunks;
			foreach (ColorChunk chunk in array)
			{
				Optimize(chunk);
			}
		}
	}
}
