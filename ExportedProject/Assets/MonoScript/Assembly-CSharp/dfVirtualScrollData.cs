using System.Collections.Generic;
using UnityEngine;

public class dfVirtualScrollData<T>
{
	public IList<T> BackingList;

	public List<IDFVirtualScrollingTile> Tiles = new List<IDFVirtualScrollingTile>();

	public RectOffset ItemPadding;

	public Vector2 LastScrollPosition = Vector2.zero;

	public int MaxExtraOffscreenTiles = 10;

	public IDFVirtualScrollingTile DummyTop;

	public IDFVirtualScrollingTile DummyBottom;

	public bool IsPaging;

	public bool IsInitialized;

	public void GetNewLimits(bool isVerticalFlow, bool getMaxes, out int index, out float newY)
	{
		IDFVirtualScrollingTile iDFVirtualScrollingTile = Tiles[0];
		index = iDFVirtualScrollingTile.VirtualScrollItemIndex;
		newY = ((!isVerticalFlow) ? iDFVirtualScrollingTile.GetDfPanel().RelativePosition.x : iDFVirtualScrollingTile.GetDfPanel().RelativePosition.y);
		foreach (IDFVirtualScrollingTile tile in Tiles)
		{
			dfPanel dfPanel2 = tile.GetDfPanel();
			float num = ((!isVerticalFlow) ? dfPanel2.RelativePosition.x : dfPanel2.RelativePosition.y);
			if (getMaxes)
			{
				if (num > newY)
				{
					newY = num;
				}
				if (tile.VirtualScrollItemIndex > index)
				{
					index = tile.VirtualScrollItemIndex;
				}
			}
			else
			{
				if (num < newY)
				{
					newY = num;
				}
				if (tile.VirtualScrollItemIndex < index)
				{
					index = tile.VirtualScrollItemIndex;
				}
			}
		}
		if (getMaxes)
		{
			index++;
		}
		else
		{
			index--;
		}
	}
}
