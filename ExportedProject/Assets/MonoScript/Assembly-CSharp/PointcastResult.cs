using System;

public class PointcastResult : IComparable<PointcastResult>
{
	public RaycastResult hitResult;

	public int pointIndex;

	public int boneIndex;

	public HitDirection hitDirection;

	public static ObjectPool<PointcastResult> Pool = new ObjectPool<PointcastResult>(() => new PointcastResult(), 10, Cleanup);

	private PointcastResult()
	{
	}

	public void SetAll(HitDirection hitDirection, int pointIndex, int boneIndex, RaycastResult hitResult)
	{
		this.hitDirection = hitDirection;
		this.pointIndex = pointIndex;
		this.boneIndex = boneIndex;
		this.hitResult = hitResult;
	}

	public static void Cleanup(PointcastResult pointcastResult)
	{
		pointcastResult.hitDirection = HitDirection.Unknown;
		pointcastResult.pointIndex = 0;
		pointcastResult.boneIndex = 0;
		RaycastResult.Pool.Free(ref pointcastResult.hitResult);
	}

	public int CompareTo(PointcastResult other)
	{
		int num = boneIndex - other.boneIndex;
		if (num != 0)
		{
			return num;
		}
		return pointIndex - other.pointIndex;
	}
}
