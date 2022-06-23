using System.Collections.Generic;

public class GungeonFlagsComparer : IEqualityComparer<GungeonFlags>
{
	public bool Equals(GungeonFlags x, GungeonFlags y)
	{
		return x == y;
	}

	public int GetHashCode(GungeonFlags obj)
	{
		return (int)obj;
	}
}
