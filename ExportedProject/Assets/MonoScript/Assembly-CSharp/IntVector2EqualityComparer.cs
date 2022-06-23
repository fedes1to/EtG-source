using System.Collections.Generic;

public class IntVector2EqualityComparer : IEqualityComparer<IntVector2>
{
	public bool Equals(IntVector2 a, IntVector2 b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public int GetHashCode(IntVector2 obj)
	{
		int num = 17;
		num = num * 23 + obj.x.GetHashCode();
		return num * 23 + obj.y.GetHashCode();
	}
}
