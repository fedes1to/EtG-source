using System;
using System.Collections.Generic;
using UnityEngine;

public class dfRenderData : IDisposable
{
	private static Queue<dfRenderData> pool = new Queue<dfRenderData>();

	public Material Material;

	public Shader Shader;

	public Matrix4x4 Transform;

	public dfList<Vector3> Vertices;

	public dfList<Vector2> UV;

	public dfList<Vector3> Normals;

	public dfList<Vector4> Tangents;

	public dfList<int> Triangles;

	public dfList<Color32> Colors;

	public uint Checksum;

	public dfIntersectionType Intersection;

	public bool Glitchy;

	internal dfRenderData()
		: this(32)
	{
	}

	internal dfRenderData(int capacity)
	{
		Vertices = new dfList<Vector3>(capacity);
		Triangles = new dfList<int>(capacity);
		Normals = new dfList<Vector3>(capacity);
		Tangents = new dfList<Vector4>(capacity);
		UV = new dfList<Vector2>(capacity);
		Colors = new dfList<Color32>(capacity);
		Transform = Matrix4x4.identity;
	}

	public static dfRenderData Obtain()
	{
		lock (pool)
		{
			return (pool.Count <= 0) ? new dfRenderData() : pool.Dequeue();
		}
	}

	public static void FlushObjectPool()
	{
		lock (pool)
		{
			while (pool.Count > 0)
			{
				dfRenderData dfRenderData2 = pool.Dequeue();
				dfRenderData2.Vertices.TrimExcess();
				dfRenderData2.Triangles.TrimExcess();
				dfRenderData2.UV.TrimExcess();
				dfRenderData2.Colors.TrimExcess();
			}
		}
	}

	public void Release()
	{
		lock (pool)
		{
			Clear();
			pool.Enqueue(this);
		}
	}

	public void Clear()
	{
		Material = null;
		Shader = null;
		Transform = Matrix4x4.identity;
		Checksum = 0u;
		Intersection = dfIntersectionType.None;
		Vertices.Clear();
		UV.Clear();
		Triangles.Clear();
		Colors.Clear();
		Normals.Clear();
		Tangents.Clear();
	}

	public bool IsValid()
	{
		int count = Vertices.Count;
		return count > 0 && count <= 65000 && UV.Count == count && Colors.Count == count;
	}

	public void EnsureCapacity(int capacity)
	{
		Vertices.EnsureCapacity(capacity);
		Triangles.EnsureCapacity(Mathf.RoundToInt((float)capacity * 1.5f));
		UV.EnsureCapacity(capacity);
		Colors.EnsureCapacity(capacity);
		if (Normals != null)
		{
			Normals.EnsureCapacity(capacity);
		}
		if (Tangents != null)
		{
			Tangents.EnsureCapacity(capacity);
		}
	}

	public void Merge(dfRenderData buffer)
	{
		Merge(buffer, true);
	}

	public void Merge(dfRenderData buffer, bool transformVertices)
	{
		int count = Vertices.Count;
		Vertices.AddRange(buffer.Vertices);
		if (transformVertices)
		{
			Vector3[] items = Vertices.Items;
			int count2 = buffer.Vertices.Count;
			Matrix4x4 transform = buffer.Transform;
			for (int i = count; i < count + count2; i++)
			{
				items[i] = transform.MultiplyPoint(items[i]);
			}
		}
		int count3 = Triangles.Count;
		Triangles.AddRange(buffer.Triangles);
		int count4 = buffer.Triangles.Count;
		int[] items2 = Triangles.Items;
		for (int j = count3; j < count3 + count4; j++)
		{
			items2[j] += count;
		}
		UV.AddRange(buffer.UV);
		Colors.AddRange(buffer.Colors);
		Normals.AddRange(buffer.Normals);
		Tangents.AddRange(buffer.Tangents);
	}

	internal void ApplyTransform(Matrix4x4 transform)
	{
		int count = Vertices.Count;
		Vector3[] items = Vertices.Items;
		for (int i = 0; i < count; i++)
		{
			items[i] = transform.MultiplyPoint(items[i]);
		}
		if (Normals.Count > 0)
		{
			Vector3[] items2 = Normals.Items;
			for (int j = 0; j < count; j++)
			{
				items2[j] = transform.MultiplyVector(items2[j]);
			}
		}
	}

	public override string ToString()
	{
		return string.Format("V:{0} T:{1} U:{2} C:{3}", Vertices.Count, Triangles.Count, UV.Count, Colors.Count);
	}

	public void Dispose()
	{
		Release();
	}
}
