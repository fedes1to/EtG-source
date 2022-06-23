using System.Collections.Generic;

namespace Dungeonator
{
	public class SpiralPointLayoutHandler
	{
		public static Queue<IntVector2> spiralOffsets;

		public static int nextElementIndex;

		public static IntVector2 resultOffset;

		public static int currentResultElementIndex = -1;

		private SemioticLayoutManager canvas;

		private SemioticLayoutManager otherCanvas;

		private IntVector2 otherCanvasOffset;

		private int currentElementIndex = -1;

		public SpiralPointLayoutHandler(SemioticLayoutManager c1, SemioticLayoutManager c2, int id)
		{
			canvas = c1;
			otherCanvas = c2;
			currentElementIndex = -1;
		}

		public void ThreadRun()
		{
			while (currentResultElementIndex == -1)
			{
				lock (spiralOffsets)
				{
					if (spiralOffsets.Count > 0)
					{
						otherCanvasOffset = spiralOffsets.Dequeue();
						currentElementIndex = nextElementIndex;
						nextElementIndex++;
					}
					else
					{
						currentElementIndex = -1;
					}
				}
				if (currentElementIndex >= 0)
				{
					CheckRectangleDecompositionCollisions();
					continue;
				}
				break;
			}
		}

		public void CheckRectangleDecompositionCollisions()
		{
			bool flag = true;
			for (int i = 0; i < otherCanvas.RectangleDecomposition.Count; i++)
			{
				Tuple<IntVector2, IntVector2> tuple = otherCanvas.RectangleDecomposition[i];
				for (int j = 0; j < canvas.RectangleDecomposition.Count; j++)
				{
					Tuple<IntVector2, IntVector2> tuple2 = canvas.RectangleDecomposition[j];
					if (IntVector2.AABBOverlap(tuple.First + otherCanvasOffset, tuple.Second, tuple2.First, tuple2.Second))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			lock (spiralOffsets)
			{
				if (currentResultElementIndex == -1 || currentElementIndex < currentResultElementIndex)
				{
					spiralOffsets.Clear();
					currentResultElementIndex = currentElementIndex;
					resultOffset = otherCanvasOffset;
				}
			}
		}
	}
}
