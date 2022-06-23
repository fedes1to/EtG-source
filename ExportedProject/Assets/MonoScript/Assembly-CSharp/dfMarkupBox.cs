using System;
using System.Collections.Generic;
using UnityEngine;

public class dfMarkupBox
{
	public Vector2 Position = Vector2.zero;

	public Vector2 Size = Vector2.zero;

	public dfMarkupDisplayType Display;

	public dfMarkupBorders Margins = new dfMarkupBorders(0, 0, 0, 0);

	public dfMarkupBorders Padding = new dfMarkupBorders(0, 0, 0, 0);

	public dfMarkupStyle Style;

	public bool IsNewline;

	public int Baseline;

	private List<dfMarkupBox> children = new List<dfMarkupBox>();

	private dfMarkupBox currentLine;

	private int currentLinePos;

	public dfMarkupBox Parent { get; protected set; }

	public dfMarkupElement Element { get; protected set; }

	public List<dfMarkupBox> Children
	{
		get
		{
			return children;
		}
	}

	public int Width
	{
		get
		{
			return (int)Size.x;
		}
		set
		{
			Size = new Vector2(value, Size.y);
		}
	}

	public int Height
	{
		get
		{
			return (int)Size.y;
		}
		set
		{
			Size = new Vector2(Size.x, value);
		}
	}

	private dfMarkupBox()
	{
		throw new NotImplementedException();
	}

	public dfMarkupBox(dfMarkupElement element, dfMarkupDisplayType display, dfMarkupStyle style)
	{
		Element = element;
		Display = display;
		Style = style;
		Baseline = style.FontSize;
	}

	internal dfMarkupBox HitTest(Vector2 point)
	{
		Vector2 offset = GetOffset();
		Vector2 vector = offset + Size;
		if (point.x < offset.x || point.x > vector.x || point.y < offset.y || point.y > vector.y)
		{
			return null;
		}
		for (int i = 0; i < children.Count; i++)
		{
			dfMarkupBox dfMarkupBox2 = children[i].HitTest(point);
			if (dfMarkupBox2 != null)
			{
				return dfMarkupBox2;
			}
		}
		return this;
	}

	internal dfRenderData Render()
	{
		try
		{
			endCurrentLine();
			return OnRebuildRenderData();
		}
		finally
		{
		}
	}

	public virtual Vector2 GetOffset()
	{
		Vector2 zero = Vector2.zero;
		for (dfMarkupBox dfMarkupBox2 = this; dfMarkupBox2 != null; dfMarkupBox2 = dfMarkupBox2.Parent)
		{
			zero += dfMarkupBox2.Position;
		}
		return zero;
	}

	internal void AddLineBreak()
	{
		if (currentLine != null)
		{
			currentLine.IsNewline = true;
		}
		int verticalPosition = getVerticalPosition(0);
		endCurrentLine();
		dfMarkupBox containingBlock = GetContainingBlock();
		currentLine = new dfMarkupBox(Element, dfMarkupDisplayType.block, Style)
		{
			Size = new Vector2(containingBlock.Size.x, Style.FontSize),
			Position = new Vector2(0f, verticalPosition),
			Parent = this
		};
		children.Add(currentLine);
	}

	public virtual void AddChild(dfMarkupBox box)
	{
		dfMarkupDisplayType display = box.Display;
		if (display == dfMarkupDisplayType.block || display == dfMarkupDisplayType.table || display == dfMarkupDisplayType.listItem || display == dfMarkupDisplayType.tableRow)
		{
			addBlock(box);
		}
		else
		{
			addInline(box);
		}
	}

	public virtual void Release()
	{
		for (int i = 0; i < children.Count; i++)
		{
			children[i].Release();
		}
		children.Clear();
		Element = null;
		Parent = null;
		Margins = default(dfMarkupBorders);
	}

	protected virtual dfRenderData OnRebuildRenderData()
	{
		return null;
	}

	protected void renderDebugBox(dfRenderData renderData)
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = zero + Vector3.right * Size.x;
		Vector3 item = vector + Vector3.down * Size.y;
		Vector3 item2 = zero + Vector3.down * Size.y;
		renderData.Vertices.Add(zero);
		renderData.Vertices.Add(vector);
		renderData.Vertices.Add(item);
		renderData.Vertices.Add(item2);
		renderData.Triangles.AddRange(new int[6] { 0, 1, 3, 3, 1, 2 });
		renderData.UV.Add(Vector2.zero);
		renderData.UV.Add(Vector2.zero);
		renderData.UV.Add(Vector2.zero);
		renderData.UV.Add(Vector2.zero);
		Color backgroundColor = Style.BackgroundColor;
		renderData.Colors.Add(backgroundColor);
		renderData.Colors.Add(backgroundColor);
		renderData.Colors.Add(backgroundColor);
		renderData.Colors.Add(backgroundColor);
	}

	public void FitToContents()
	{
		FitToContents(false);
	}

	public void FitToContents(bool recursive)
	{
		if (children.Count == 0)
		{
			Size = new Vector2(Size.x, 0f);
			return;
		}
		endCurrentLine();
		Vector2 vector = Vector2.zero;
		for (int i = 0; i < children.Count; i++)
		{
			dfMarkupBox dfMarkupBox2 = children[i];
			vector = Vector2.Max(vector, dfMarkupBox2.Position + dfMarkupBox2.Size);
		}
		Size = vector;
	}

	private dfMarkupBox GetContainingBlock()
	{
		for (dfMarkupBox dfMarkupBox2 = this; dfMarkupBox2 != null; dfMarkupBox2 = dfMarkupBox2.Parent)
		{
			dfMarkupDisplayType display = dfMarkupBox2.Display;
			if (display == dfMarkupDisplayType.block || display == dfMarkupDisplayType.inlineBlock || display == dfMarkupDisplayType.listItem || display == dfMarkupDisplayType.table || display == dfMarkupDisplayType.tableRow || display == dfMarkupDisplayType.tableCell)
			{
				return dfMarkupBox2;
			}
		}
		return null;
	}

	private void addInline(dfMarkupBox box)
	{
		dfMarkupBorders margins = box.Margins;
		bool flag = !Style.Preformatted && currentLine != null && (float)currentLinePos + box.Size.x > currentLine.Size.x;
		if (currentLine == null || flag)
		{
			endCurrentLine();
			int verticalPosition = getVerticalPosition(margins.top);
			dfMarkupBox containingBlock = GetContainingBlock();
			if (containingBlock == null)
			{
				Debug.LogError("Containing block not found");
				return;
			}
			dfDynamicFont dfDynamicFont2 = Style.Font ?? Style.Host.Font;
			float num = (float)dfDynamicFont2.FontSize / (float)dfDynamicFont2.FontSize;
			float num2 = (float)dfDynamicFont2.Baseline * num;
			currentLine = new dfMarkupBox(Element, dfMarkupDisplayType.block, Style)
			{
				Size = new Vector2(containingBlock.Size.x, Style.LineHeight),
				Position = new Vector2(0f, verticalPosition),
				Parent = this,
				Baseline = (int)num2
			};
			children.Add(currentLine);
		}
		if (currentLinePos == 0 && !box.Style.PreserveWhitespace && box is dfMarkupBoxText)
		{
			dfMarkupBoxText dfMarkupBoxText2 = box as dfMarkupBoxText;
			if (dfMarkupBoxText2.IsWhitespace)
			{
				return;
			}
		}
		Vector2 vector = (box.Position = new Vector2(currentLinePos + margins.left, margins.top));
		box.Parent = currentLine;
		currentLine.children.Add(box);
		currentLinePos = (int)(vector.x + box.Size.x + (float)box.Margins.right);
		float x = Mathf.Max(currentLine.Size.x, vector.x + box.Size.x);
		float y = Mathf.Max(currentLine.Size.y, vector.y + box.Size.y);
		currentLine.Size = new Vector2(x, y);
	}

	private int getVerticalPosition(int topMargin)
	{
		if (children.Count == 0)
		{
			return topMargin;
		}
		int num = 0;
		int index = 0;
		for (int i = 0; i < children.Count; i++)
		{
			dfMarkupBox dfMarkupBox2 = children[i];
			float num2 = dfMarkupBox2.Position.y + dfMarkupBox2.Size.y + (float)dfMarkupBox2.Margins.bottom;
			if (num2 > (float)num)
			{
				num = (int)num2;
				index = i;
			}
		}
		dfMarkupBox dfMarkupBox3 = children[index];
		int num3 = Mathf.Max(dfMarkupBox3.Margins.bottom, topMargin);
		return (int)(dfMarkupBox3.Position.y + dfMarkupBox3.Size.y + (float)num3);
	}

	private void addBlock(dfMarkupBox box)
	{
		if (currentLine != null)
		{
			currentLine.IsNewline = true;
			endCurrentLine(true);
		}
		dfMarkupBox containingBlock = GetContainingBlock();
		if (box.Size.sqrMagnitude <= float.Epsilon)
		{
			box.Size = new Vector2(containingBlock.Size.x - (float)box.Margins.horizontal, Style.FontSize);
		}
		int verticalPosition = getVerticalPosition(box.Margins.top);
		box.Position = new Vector2(box.Margins.left, verticalPosition);
		Size = new Vector2(Size.x, Mathf.Max(Size.y, box.Position.y + box.Size.y));
		box.Parent = this;
		children.Add(box);
	}

	private void endCurrentLine()
	{
		endCurrentLine(false);
	}

	private void endCurrentLine(bool removeEmpty)
	{
		if (currentLine == null)
		{
			return;
		}
		if (currentLinePos == 0)
		{
			if (removeEmpty)
			{
				children.Remove(currentLine);
			}
		}
		else
		{
			currentLine.doHorizontalAlignment();
			currentLine.doVerticalAlignment();
		}
		currentLine = null;
		currentLinePos = 0;
	}

	private void doVerticalAlignment()
	{
		if (children.Count == 0)
		{
			return;
		}
		float num = float.MinValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		Baseline = (int)(Size.y * 0.95f);
		for (int i = 0; i < children.Count; i++)
		{
			dfMarkupBox dfMarkupBox2 = children[i];
			num3 = Mathf.Max(num3, dfMarkupBox2.Position.y + (float)dfMarkupBox2.Baseline);
		}
		for (int j = 0; j < children.Count; j++)
		{
			dfMarkupBox dfMarkupBox3 = children[j];
			dfMarkupVerticalAlign verticalAlign = dfMarkupBox3.Style.VerticalAlign;
			Vector2 position = dfMarkupBox3.Position;
			if (verticalAlign == dfMarkupVerticalAlign.Baseline)
			{
				position.y = num3 - (float)dfMarkupBox3.Baseline;
			}
			dfMarkupBox3.Position = position;
		}
		for (int k = 0; k < children.Count; k++)
		{
			dfMarkupBox dfMarkupBox4 = children[k];
			Vector2 position2 = dfMarkupBox4.Position;
			Vector2 size = dfMarkupBox4.Size;
			num2 = Mathf.Min(num2, position2.y);
			num = Mathf.Max(num, position2.y + size.y);
		}
		for (int l = 0; l < children.Count; l++)
		{
			dfMarkupBox dfMarkupBox5 = children[l];
			dfMarkupVerticalAlign verticalAlign2 = dfMarkupBox5.Style.VerticalAlign;
			Vector2 position3 = dfMarkupBox5.Position;
			Vector2 size2 = dfMarkupBox5.Size;
			switch (verticalAlign2)
			{
			case dfMarkupVerticalAlign.Top:
				position3.y = num2;
				break;
			case dfMarkupVerticalAlign.Bottom:
				position3.y = num - size2.y;
				break;
			case dfMarkupVerticalAlign.Middle:
				position3.y = (Size.y - size2.y) * 0.5f;
				break;
			}
			dfMarkupBox5.Position = position3;
		}
		int num4 = int.MaxValue;
		for (int m = 0; m < children.Count; m++)
		{
			num4 = Mathf.Min(num4, (int)children[m].Position.y);
		}
		for (int n = 0; n < children.Count; n++)
		{
			Vector2 position4 = children[n].Position;
			position4.y -= num4;
			children[n].Position = position4;
		}
	}

	private void doHorizontalAlignment()
	{
		if (Style.Align == dfMarkupTextAlign.Left || children.Count == 0)
		{
			return;
		}
		int num;
		for (num = children.Count - 1; num > 0; num--)
		{
			dfMarkupBoxText dfMarkupBoxText2 = children[num] as dfMarkupBoxText;
			if (dfMarkupBoxText2 == null || !dfMarkupBoxText2.IsWhitespace)
			{
				break;
			}
		}
		if (Style.Align == dfMarkupTextAlign.Center)
		{
			float num2 = 0f;
			for (int i = 0; i <= num; i++)
			{
				num2 += children[i].Size.x;
			}
			float num3 = (Size.x - (float)Padding.horizontal - num2) * 0.5f;
			for (int j = 0; j <= num; j++)
			{
				Vector2 position = children[j].Position;
				position.x += num3;
				children[j].Position = position;
			}
			return;
		}
		if (Style.Align == dfMarkupTextAlign.Right)
		{
			float num4 = Size.x - (float)Padding.horizontal;
			for (int num5 = num; num5 >= 0; num5--)
			{
				Vector2 position2 = children[num5].Position;
				position2.x = num4 - children[num5].Size.x;
				children[num5].Position = position2;
				num4 -= children[num5].Size.x;
			}
			return;
		}
		if (Style.Align == dfMarkupTextAlign.Justify)
		{
			if (children.Count > 1 && !IsNewline && !children[children.Count - 1].IsNewline)
			{
				float num6 = 0f;
				for (int k = 0; k <= num; k++)
				{
					dfMarkupBox dfMarkupBox2 = children[k];
					num6 = Mathf.Max(num6, dfMarkupBox2.Position.x + dfMarkupBox2.Size.x);
				}
				float num7 = (Size.x - (float)Padding.horizontal - num6) / (float)children.Count;
				for (int l = 1; l <= num; l++)
				{
					children[l].Position += new Vector2((float)l * num7, 0f);
				}
				dfMarkupBox dfMarkupBox3 = children[num];
				Vector2 position3 = dfMarkupBox3.Position;
				position3.x = Size.x - (float)Padding.horizontal - dfMarkupBox3.Size.x;
				dfMarkupBox3.Position = position3;
			}
			return;
		}
		throw new NotImplementedException(string.Concat("text-align: ", Style.Align, " is not implemented"));
	}
}
