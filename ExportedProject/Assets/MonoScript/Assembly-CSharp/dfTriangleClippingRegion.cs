using System;
using System.Collections.Generic;
using UnityEngine;

internal class dfTriangleClippingRegion : IDisposable
{
	private static Queue<dfTriangleClippingRegion> pool = new Queue<dfTriangleClippingRegion>();

	private static dfList<Plane> intersectedPlanes = new dfList<Plane>(32);

	private dfList<Plane> planes;

	private dfTriangleClippingRegion()
	{
		planes = new dfList<Plane>();
	}

	public static dfTriangleClippingRegion Obtain()
	{
		return (pool.Count <= 0) ? new dfTriangleClippingRegion() : pool.Dequeue();
	}

	public static dfTriangleClippingRegion Obtain(dfTriangleClippingRegion parent, dfControl control)
	{
		dfTriangleClippingRegion dfTriangleClippingRegion2 = ((pool.Count <= 0) ? new dfTriangleClippingRegion() : pool.Dequeue());
		dfTriangleClippingRegion2.planes.AddRange(control.GetClippingPlanes());
		if (parent != null)
		{
			dfTriangleClippingRegion2.planes.AddRange(parent.planes);
		}
		return dfTriangleClippingRegion2;
	}

	public void Release()
	{
		planes.Clear();
		if (!pool.Contains(this))
		{
			pool.Enqueue(this);
		}
	}

	public bool PerformClipping(dfRenderData dest, ref Bounds bounds, uint checksum, dfRenderData controlData)
	{
		if (planes == null || planes.Count == 0)
		{
			dest.Merge(controlData);
			return true;
		}
		if (controlData.Checksum == checksum)
		{
			if (controlData.Intersection == dfIntersectionType.Inside)
			{
				dest.Merge(controlData);
				return true;
			}
			if (controlData.Intersection == dfIntersectionType.None)
			{
				return false;
			}
		}
		bool result = false;
		dfIntersectionType type;
		dfList<Plane> dfList2 = TestIntersection(bounds, out type);
		switch (type)
		{
		case dfIntersectionType.Inside:
			dest.Merge(controlData);
			result = true;
			break;
		case dfIntersectionType.Intersecting:
			clipToPlanes(dfList2, controlData, dest, checksum);
			result = true;
			break;
		}
		controlData.Checksum = checksum;
		controlData.Intersection = type;
		return result;
	}

	public dfList<Plane> TestIntersection(Bounds bounds, out dfIntersectionType type)
	{
		if (planes == null || planes.Count == 0)
		{
			type = dfIntersectionType.Inside;
			return null;
		}
		intersectedPlanes.Clear();
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		bool flag = false;
		int count = planes.Count;
		Plane[] items = planes.Items;
		for (int i = 0; i < count; i++)
		{
			Plane item = items[i];
			Vector3 normal = item.normal;
			float distance = item.distance;
			float num = extents.x * Mathf.Abs(normal.x) + extents.y * Mathf.Abs(normal.y) + extents.z * Mathf.Abs(normal.z);
			float num2 = Vector3.Dot(normal, center) + distance;
			if (Mathf.Abs(num2) <= num)
			{
				flag = true;
				intersectedPlanes.Add(item);
			}
			else if (num2 < 0f - num)
			{
				type = dfIntersectionType.None;
				return null;
			}
		}
		if (flag)
		{
			type = dfIntersectionType.Intersecting;
			return intersectedPlanes;
		}
		type = dfIntersectionType.Inside;
		return null;
	}

	public void clipToPlanes(dfList<Plane> planes, dfRenderData data, dfRenderData dest, uint controlChecksum)
	{
		if (data != null && data.Vertices.Count != 0)
		{
			if (planes == null || planes.Count == 0)
			{
				dest.Merge(data);
			}
			else
			{
				dfClippingUtil.Clip(planes, data, dest);
			}
		}
	}

	public void Dispose()
	{
		Release();
	}
}
