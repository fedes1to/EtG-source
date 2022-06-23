using System;
using UnityEngine;

[Serializable]
public class BagelCollider
{
	public int width;

	public int height;

	[SerializeField]
	private int minX;

	[SerializeField]
	private int minY;

	[SerializeField]
	private int actualWidth;

	[SerializeField]
	private int actualHeight;

	[SerializeField]
	private bool[] bagelCollider;

	public bool this[int x, int y]
	{
		get
		{
			if (actualWidth == 0 && bagelCollider != null && bagelCollider.Length > 0)
			{
				actualWidth = width;
				actualHeight = height;
			}
			if (x < minX || x >= minX + actualWidth || y < minY || y >= minY + actualHeight)
			{
				return false;
			}
			return bagelCollider[(y - minY) * actualWidth + (x - minX)];
		}
	}

	public BagelCollider(BagelCollider source)
	{
		width = source.width;
		height = source.height;
		minX = source.minX;
		minY = source.minY;
		actualWidth = source.actualWidth;
		actualHeight = source.actualHeight;
		bagelCollider = new bool[source.bagelCollider.Length];
		for (int i = 0; i < source.bagelCollider.Length; i++)
		{
			bagelCollider[i] = source.bagelCollider[i];
		}
	}

	public BagelCollider(int width, int height)
	{
		this.width = width;
		this.height = height;
		actualWidth = 0;
		actualHeight = 0;
		bagelCollider = new bool[0];
	}
}
