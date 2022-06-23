using UnityEngine;

internal class dfRenderBatch
{
	public Material Material;

	private dfList<dfRenderData> buffers = new dfList<dfRenderData>();

	public void Add(dfRenderData buffer)
	{
		if (Material == null && buffer.Material != null)
		{
			Material = buffer.Material;
		}
		buffers.Add(buffer);
	}

	public dfRenderData Combine()
	{
		dfRenderData dfRenderData2 = dfRenderData.Obtain();
		int count = buffers.Count;
		dfRenderData[] items = buffers.Items;
		if (count == 0)
		{
			return dfRenderData2;
		}
		dfRenderData2.Material = buffers[0].Material;
		int capacity = 0;
		for (int i = 0; i < count; i++)
		{
			capacity = items[i].Vertices.Count;
		}
		dfRenderData2.EnsureCapacity(capacity);
		int[] items2 = dfRenderData2.Triangles.Items;
		for (int j = 0; j < count; j++)
		{
			dfRenderData dfRenderData3 = items[j];
			int count2 = dfRenderData2.Vertices.Count;
			int count3 = dfRenderData2.Triangles.Count;
			int count4 = dfRenderData3.Triangles.Count;
			dfRenderData2.Vertices.AddRange(dfRenderData3.Vertices);
			dfRenderData2.Triangles.AddRange(dfRenderData3.Triangles);
			dfRenderData2.Colors.AddRange(dfRenderData3.Colors);
			dfRenderData2.UV.AddRange(dfRenderData3.UV);
			for (int k = count3; k < count3 + count4; k++)
			{
				items2[k] += count2;
			}
		}
		return dfRenderData2;
	}
}
