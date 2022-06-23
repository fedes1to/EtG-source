using System;
using UnityEngine;

namespace tk2dRuntime.TileMap
{
	[Serializable]
	public class ColorChunk
	{
		public Color32[] colors;

		public Color32[,] colorOverrides;

		public bool Dirty { get; set; }

		public bool Empty
		{
			get
			{
				return colors.Length == 0;
			}
		}

		public ColorChunk()
		{
			colors = new Color32[0];
			colorOverrides = new Color32[0, 0];
		}
	}
}
